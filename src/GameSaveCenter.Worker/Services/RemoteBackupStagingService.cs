using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Downloads a remote Ludusavi vault to an isolated directory and verifies both the
/// transfer and requested backup before it can be used by the restore orchestrator.
/// </summary>
public sealed class RemoteBackupStagingService : IRemoteBackupStageProvider
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly WorkerOptions options;
    private readonly RcloneClient rclone;
    private readonly LudusaviClient ludusavi;
    private readonly GameCatalogService catalog;
    private readonly SqliteStateStore store;
    private readonly CloudTransferCoordinator cloudTransfers;
    private readonly TaskCoordinator tasks;

    public RemoteBackupStagingService(WorkerOptions options,RcloneClient rclone,LudusaviClient ludusavi,
        GameCatalogService catalog,SqliteStateStore store,CloudTransferCoordinator cloudTransfers,TaskCoordinator tasks)
    {
        this.options=options;this.rclone=rclone;this.ludusavi=ludusavi;this.catalog=catalog;this.store=store;this.cloudTransfers=cloudTransfers;this.tasks=tasks;
    }

    public async Task<RemoteBackupStageResultDto> StageAsync(RemoteBackupStageRequestDto request,CancellationToken token,string requestId = "")
    {
        ValidateRequest(request);
        if(!rclone.IsConfigured)throw new WorkerOperationException("RCLONE_NOT_CONFIGURED","Rclone 尚未配置，无法下载远端备份。",options.RcloneDestination);
        if(!ludusavi.IsAvailable)throw new WorkerOperationException("LUDUSAVI_NOT_CONFIGURED","Ludusavi 尚未配置，无法校验远端备份。",options.LudusaviExecutable);
        var game=await catalog.GetGameAsync(request.PlayniteId,token).ConfigureAwait(false)
                 ??throw new WorkerOperationException("REMOTE_GAME_NOT_FOUND","找不到远端备份对应的游戏。",request.PlayniteId);
        var matches=await catalog.GetMatchesAsync(token).ConfigureAwait(false);
        if(!matches.TryGetValue(request.PlayniteId,out var match)||string.IsNullOrWhiteSpace(match.Name))
            throw new WorkerOperationException("REMOTE_GAME_UNMATCHED","该游戏尚未匹配到 Ludusavi，无法校验远端版本。",request.PlayniteId);

        RemoteBackupStageResultDto? stagedResult=null;
        var task=await tasks.RunAsync("RemoteStage",request.PlayniteId,game.Name,
            async (progress,stageToken)=>stagedResult=await StageCoreAsync(request,game.Name,match.Name,progress,stageToken).ConfigureAwait(false),
            token,requestId:requestId,sourceReferences: new[]
            {
                new TaskSourceReferenceDto
                {
                    Kind = TaskSourceReferenceKind.BackupVersion,
                    StableId = request.BackupId,
                    PlayniteId = game.PlayniteId,
                    DisplayName = request.BackupId,
                    Detail = "远端暂存目标版本；版本消失时保留任务诊断"
                }
            }).ConfigureAwait(false);
        if(task.State==TaskState.Succeeded&&stagedResult!=null)return stagedResult;
        if(task.State==TaskState.Succeeded)
            throw new InvalidOperationException("远端下载任务已成功，但没有返回隔离结果。");
        if(task.State==TaskState.Cancelled)
            throw new WorkerOperationException("REMOTE_STAGE_CANCELLED","远端备份下载已取消；隔离区清理结果请查看任务状态。",task.Message);
        if(task.State==TaskState.Failed)
            throw new WorkerOperationException(
                string.IsNullOrWhiteSpace(task.ErrorCode)?"REMOTE_STAGE_FAILED":task.ErrorCode,
                string.IsNullOrWhiteSpace(task.ErrorMessage)?"远端备份下载或校验失败。":task.ErrorMessage);
        throw new InvalidOperationException("远端下载任务返回了未知状态。");
    }

    private async Task<RemoteBackupStageResultDto> StageCoreAsync(RemoteBackupStageRequestDto request,string gameName,string matchName,
        TaskProgress progress,CancellationToken token)
    {
        await progress.ReportAsync(5,"正在准备远端备份下载").ConfigureAwait(false);
        Directory.CreateDirectory(options.RemoteBackupStagingDirectory);
        CleanupExpired();
        var stagingId=Guid.NewGuid().ToString("N");
        var stagingRoot=ResolveStagingRoot(stagingId);
        var vaultPath=Path.Combine(stagingRoot,"Vault");
        Directory.CreateDirectory(vaultPath);
        var remoteStorageKey=ResolveRemoteStorageKey(request.RemoteDeviceId,request.RemoteDevice);
        var remoteSubPath=Path.Combine(remoteStorageKey,"Saves");
        try
        {
            var transfer=await cloudTransfers.RunUploadAsync("remote backup download",async ct=>
            {
                var copy=await rclone.DownloadAsync(remoteSubPath,vaultPath,ct).ConfigureAwait(false);
                if(!copy.Success)return (Copy:copy,Check:(ProcessResult?)null);
                await progress.ReportAsync(50,"已下载到隔离区，正在校验完整性").ConfigureAwait(false);
                var check=await rclone.ChecksumCheckAsync(vaultPath,remoteSubPath,ct).ConfigureAwait(false);
                return (Copy:copy,Check:(ProcessResult?)check);
            },token).ConfigureAwait(false);
            if(!transfer.Copy.Success)throw new WorkerOperationException("RCLONE_DOWNLOAD_FAILED","远端备份下载失败；本机存档未发生变化。",transfer.Copy.StandardError);
            if(transfer.Check==null||!transfer.Check.Success)
                throw new WorkerOperationException("RCLONE_DOWNLOAD_CHECK_FAILED","远端备份哈希一致性校验失败；暂存内容不会用于恢复。",transfer.Check?.StandardError??string.Empty);
            await progress.ReportAsync(70,"一致性校验通过，正在确认目标 Ludusavi 版本").ConfigureAwait(false);
            var listed=await ludusavi.ListBackupsFromPathAsync(vaultPath,new[]{matchName},token).ConfigureAwait(false);
            var versions=listed.Success&&listed.Json.HasValue
                ?LudusaviResultParser.ParseBackupList(listed.Json.Value,request.PlayniteId,matchName)
                :new List<BackupVersionDto>();
            if(!versions.Any(x=>string.Equals(x.BackupId,request.BackupId,StringComparison.OrdinalIgnoreCase)))
                throw new WorkerOperationException("REMOTE_BACKUP_NOT_FOUND","下载完成，但隔离库中找不到所选备份版本。",request.BackupId);
            await progress.ReportAsync(90,"已确认目标版本，正在写入隔离清单").ConfigureAwait(false);
            var now=DateTime.UtcNow;
            var result=new RemoteBackupStageResultDto
            {
                StagingId=stagingId,PlayniteId=request.PlayniteId,GameName=gameName,RemoteDevice=request.RemoteDevice,RemoteDeviceId=remoteStorageKey,
                BackupId=request.BackupId,StagedUtc=now,ExpiresUtc=now+Lifetime,Verified=true,
                StatusMessage="远端备份已下载到隔离区并通过一致性与 Ludusavi 版本校验；当前存档尚未恢复。"
            };
            WriteManifest(stagingRoot,result);
            await store.AppendAuditAsync("RemoteBackup","远端备份已隔离暂存",JsonSerializer.Serialize(result),token).ConfigureAwait(false);
            await progress.ReportAsync(100,"下载完成并已校验，等待恢复确认").ConfigureAwait(false);
            return result;
        }
        catch(OperationCanceledException)
        {
            var cleaned=TryDeleteStaging(stagingRoot);
            progress.SetCancellationMessage(cleaned
                ?"远端备份下载已取消；隔离区已清理。"
                :"远端备份下载已取消；隔离区清理失败，仍有残留内容需要维护中心复核。");
            throw;
        }
        catch
        {
            var cleaned=TryDeleteStaging(stagingRoot);
            if(!cleaned)
                throw new WorkerOperationException("REMOTE_STAGE_CLEANUP_FAILED","远端备份下载未完成，且隔离区清理失败；请在维护中心重新检查残留状态。",stagingRoot);
            throw;
        }
    }

    public RemoteBackupStage OpenVerified(string stagingId)
    {
        if(!IsOpaqueId(stagingId))throw new WorkerOperationException("REMOTE_STAGE_ID_INVALID","远端暂存标识无效。",stagingId);
        var root=ResolveStagingRoot(stagingId);
        var manifestPath=Path.Combine(root,"manifest.json");
        if(!File.Exists(manifestPath))throw new WorkerOperationException("REMOTE_STAGE_NOT_FOUND","远端暂存已不存在，请重新下载到隔离区并校验。",stagingId);
        RemoteBackupStageResultDto? manifest;
        try { manifest=JsonSerializer.Deserialize<RemoteBackupStageResultDto>(File.ReadAllText(manifestPath),JsonOptions); }
        catch(JsonException ex){throw new WorkerOperationException("REMOTE_STAGE_MANIFEST_INVALID","远端暂存清单损坏，请重新下载。",ex.Message);}
        if(manifest==null||!manifest.Verified||!string.Equals(manifest.StagingId,stagingId,StringComparison.Ordinal)||
           manifest.ExpiresUtc<=DateTime.UtcNow||!IsSafeDeviceName(manifest.RemoteDevice))
            throw new WorkerOperationException("REMOTE_STAGE_EXPIRED","远端暂存无效或已过期，请重新下载到隔离区并校验。",stagingId);
        var vault=Path.Combine(root,"Vault");
        if(!Directory.Exists(vault))throw new WorkerOperationException("REMOTE_STAGE_VAULT_MISSING","远端暂存目录不完整，请重新下载。",stagingId);
        return new RemoteBackupStage(manifest,vault);
    }

    public async Task<RemoteBackupStage> RevalidateAsync(string stagingId,CancellationToken token)
    {
        var stage=OpenVerified(stagingId);
        var remoteSubPath=Path.Combine(ResolveRemoteStorageKey(stage.Manifest.RemoteDeviceId,stage.Manifest.RemoteDevice),"Saves");
        var check=await cloudTransfers.RunUploadAsync("remote backup restore revalidation",
            ct=>rclone.ChecksumCheckAsync(stage.VaultPath,remoteSubPath,ct),token).ConfigureAwait(false);
        if(!check.Success)
            throw new WorkerOperationException("REMOTE_STAGE_CHANGED","远端或本机隔离备份在暂存后发生变化，已阻止恢复；请重新下载到隔离区并校验。",check.StandardError);
        var matches=await catalog.GetMatchesAsync(token).ConfigureAwait(false);
        if(!matches.TryGetValue(stage.Manifest.PlayniteId,out var match)||string.IsNullOrWhiteSpace(match.Name))
            throw new WorkerOperationException("REMOTE_GAME_UNMATCHED","该游戏当前未匹配到 Ludusavi，已阻止远端恢复。",stage.Manifest.PlayniteId);
        var listed=await ludusavi.ListBackupsFromPathAsync(stage.VaultPath,new[]{match.Name},token).ConfigureAwait(false);
        var versions=listed.Success&&listed.Json.HasValue
            ?LudusaviResultParser.ParseBackupList(listed.Json.Value,stage.Manifest.PlayniteId,match.Name)
            :new List<BackupVersionDto>();
        if(!versions.Any(x=>string.Equals(x.BackupId,stage.Manifest.BackupId,StringComparison.OrdinalIgnoreCase)))
            throw new WorkerOperationException("REMOTE_BACKUP_CHANGED","隔离库不再包含所选备份版本，已阻止恢复；请重新下载到隔离区并校验。",stage.Manifest.BackupId);
        return stage;
    }

    internal static bool IsSafeDeviceName(string? value)
        => !string.IsNullOrWhiteSpace(value)&&value.Length<=128&&value!="."&&value!=".."&&
           value.IndexOfAny(Path.GetInvalidFileNameChars())<0&&!value.Contains('/')&&!value.Contains('\\');

    internal static bool IsOpaqueId(string? value)
        => value?.Length==32&&value.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static void ValidateRequest(RemoteBackupStageRequestDto request)
    {
        if(string.IsNullOrWhiteSpace(request.PlayniteId)||string.IsNullOrWhiteSpace(request.BackupId))
            throw new ArgumentException("必须选择包含远端备份版本的设备记录。");
        if(!IsSafeDeviceName(request.RemoteDevice))throw new ArgumentException("远端设备名称包含不安全字符。");
        if(!string.IsNullOrWhiteSpace(request.RemoteDeviceId)&&!WorkerOptions.IsValidDeviceId(request.RemoteDeviceId))
            throw new ArgumentException("远端设备标识无效。");
    }

    private static string ResolveRemoteStorageKey(string? deviceId,string deviceName)
        => WorkerOptions.IsValidDeviceId(deviceId) ? deviceId!.ToLowerInvariant() : deviceName;

    private string ResolveStagingRoot(string stagingId)
    {
        var root=Path.GetFullPath(options.RemoteBackupStagingDirectory);
        var candidate=Path.GetFullPath(Path.Combine(root,stagingId));
        if(!candidate.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
            throw new WorkerOperationException("REMOTE_STAGE_PATH_INVALID","远端暂存路径越界。",candidate);
        return candidate;
    }

    private static void WriteManifest(string root,RemoteBackupStageResultDto result)
    {
        var path=Path.Combine(root,"manifest.json");
        var temporary=path+".tmp";
        File.WriteAllText(temporary,JsonSerializer.Serialize(result,JsonOptions));
        File.Move(temporary,path,true);
    }

    private void CleanupExpired()
    {
        foreach(var directory in Directory.EnumerateDirectories(options.RemoteBackupStagingDirectory))
        {
            try
            {
                var manifest=JsonSerializer.Deserialize<RemoteBackupStageResultDto>(
                    File.ReadAllText(Path.Combine(directory,"manifest.json")),JsonOptions);
                if(manifest==null||manifest.ExpiresUtc<=DateTime.UtcNow)TryDeleteStaging(directory);
            }
            catch { if(Directory.GetCreationTimeUtc(directory)<=DateTime.UtcNow-Lifetime)TryDeleteStaging(directory); }
        }
    }

    internal static bool TryDeleteStaging(string? directory)
    {
        try { if(!string.IsNullOrWhiteSpace(directory)&&Directory.Exists(directory))Directory.Delete(directory,true); return true; } catch { return false; }
    }
}

public sealed record RemoteBackupStage(RemoteBackupStageResultDto Manifest,string VaultPath);
