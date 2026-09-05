using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;

namespace GameSaveCenter.Worker.Services;

/// <summary>Implements an explicit, auditable restore state machine with a mandatory PreRestore backup.</summary>
public sealed class RestoreOrchestrator
{
    private readonly IRestoreCatalog _catalog;
    private readonly SqliteStateStore _store;
    private readonly IRestoreClient _ludusavi;
    private readonly TaskCoordinator _tasks;
    private readonly IRestoreSessionState _sessions;
    private readonly CloudTransferCoordinator _cloudTransfers;
    private readonly IRemoteBackupStageProvider _remoteBackups;
    private readonly GameOperationLock _gameLock;

    public RestoreOrchestrator(IRestoreCatalog catalog,SqliteStateStore store,IRestoreClient ludusavi,TaskCoordinator tasks,
        IRestoreSessionState sessions,CloudTransferCoordinator cloudTransfers,IRemoteBackupStageProvider remoteBackups,GameOperationLock gameLock)
    { _catalog=catalog;_store=store;_ludusavi=ludusavi;_tasks=tasks;_sessions=sessions;_cloudTransfers=cloudTransfers;_remoteBackups=remoteBackups;_gameLock=gameLock; }

    public async Task<LudusaviCommandResult> PreviewAsync(RestoreRequestDto request,CancellationToken token)
    {
        var match=await ResolveAsync(request.PlayniteId,token).ConfigureAwait(false);
        using var lease = await _gameLock.AcquireAsync(request.PlayniteId, GameOperationKind.Restore, TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
        if (lease == null)
            throw new WorkerOperationException("GAME_OPERATION_BUSY","该游戏已有备份、恢复或媒体操作正在执行，已跳过恢复预览。",request.PlayniteId);
        return await _ludusavi.RestoreAsync(match,request.BackupId,true,token).ConfigureAwait(false);
    }

    public async Task<TaskStatusDto> ExecuteAsync(RestoreRequestDto request,CancellationToken token)
        =>await ExecuteCoreAsync(request,null,token).ConfigureAwait(false);

    public async Task<TaskStatusDto> ExecuteRemoteAsync(RemoteRestoreRequestDto request,CancellationToken token)
    {
        var stage=await _remoteBackups.RevalidateAsync(request.StagingId,token).ConfigureAwait(false);
        return await ExecuteCoreAsync(new RestoreRequestDto
        {
            PlayniteId=stage.Manifest.PlayniteId,BackupId=stage.Manifest.BackupId,
            RequestId=request.RequestId,
            ConfirmedCurrentSnapshot=request.ConfirmedCurrentSnapshot,ConfirmedGameClosed=request.ConfirmedGameClosed,
            UserComment=$"Remote device {stage.Manifest.RemoteDevice}: {request.UserComment}".Trim()
        },stage.VaultPath,token).ConfigureAwait(false);
    }

    private async Task<TaskStatusDto> ExecuteCoreAsync(RestoreRequestDto request,string? targetBackupPath,CancellationToken token)
    {
        if(!request.ConfirmedCurrentSnapshot||!request.ConfirmedGameClosed)
            throw new InvalidOperationException("Restore requires explicit confirmation that the game is closed and the current state may be snapshotted.");
        var game=await _catalog.GetGameAsync(request.PlayniteId,token).ConfigureAwait(false)??throw new InvalidOperationException("Game not found.");
        var match=await ResolveAsync(request.PlayniteId,token).ConfigureAwait(false);
        using var lease = await _gameLock.AcquireAsync(request.PlayniteId, GameOperationKind.Restore, TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
        if (lease == null)
            throw new WorkerOperationException("GAME_OPERATION_BUSY","该游戏已有备份、恢复或媒体操作正在执行，已阻止恢复。",request.PlayniteId);
        return await _tasks.RunAsync("Restore",game.PlayniteId,game.Name,async(progress,ct)=>
        {
            var state=RestoreState.Requested;
            await AuditAsync(game.PlayniteId,state,request,ct).ConfigureAwait(false);
            await progress.ReportAsync(5,"正在确认游戏已关闭").ConfigureAwait(false);
            await EnsureGameClosedAsync(game.PlayniteId,ct).ConfigureAwait(false);
            state=RestoreState.GameClosedVerified;await AuditAsync(game.PlayniteId,state,request,ct).ConfigureAwait(false);

            var before=await _ludusavi.ListBackupsAsync(new[]{match},ct).ConfigureAwait(false);
            var beforeIds=before.Success&&before.Json.HasValue?LudusaviResultParser.ParseBackupList(before.Json.Value,game.PlayniteId,match).Select(x=>x.BackupId).ToHashSet(StringComparer.OrdinalIgnoreCase):new HashSet<string>();
            await progress.ReportAsync(15,"正在创建 PreRestore 安全快照").ConfigureAwait(false);
            var pre=await _ludusavi.BackupAsync(new[]{match},true,false,ct).ConfigureAwait(false);
            if(!pre.Success||!pre.Json.HasValue||LudusaviResultParser.SomeGamesFailed(pre.Json.Value)) throw new InvalidOperationException("PreRestore backup failed; restore was not started.");
            var after=await _ludusavi.ListBackupsAsync(new[]{match},ct).ConfigureAwait(false);
            var versions=after.Success&&after.Json.HasValue?LudusaviResultParser.ParseBackupList(after.Json.Value,game.PlayniteId,match):new List<BackupVersionDto>();
            var preVersion=versions.FirstOrDefault(x=>!beforeIds.Contains(x.BackupId))??versions.OrderByDescending(x=>x.CreatedUtc).FirstOrDefault();
            if(preVersion==null) throw new InvalidOperationException("Could not identify the PreRestore backup version.");
            preVersion.IsPreRestore=true;preVersion.IsLocked=true;preVersion.Comment=$"PreRestore {DateTime.Now:yyyy-MM-dd HH:mm:ss} {request.UserComment}".Trim();
            var edit=await _ludusavi.EditBackupAsync(match,preVersion.BackupId,preVersion.Comment,true,ct).ConfigureAwait(false);
            if(!edit.Success) throw new InvalidOperationException("PreRestore was created, but it could not be locked in Ludusavi: "+edit.ErrorMessage);
            await _store.AddBackupVersionAsync(preVersion,"{}",ct).ConfigureAwait(false);
            state=RestoreState.PreRestoreBackupCreated;await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId},ct).ConfigureAwait(false);

            await progress.ReportAsync(40,"正在预览目标版本").ConfigureAwait(false);
            var preview=await RestoreTargetAsync(targetBackupPath,match,request.BackupId,true,ct).ConfigureAwait(false);
            if(!preview.Success||!preview.Json.HasValue||LudusaviResultParser.SomeGamesFailed(preview.Json.Value)) throw new InvalidOperationException("Restore preview failed; live files were not changed.");

            await progress.ReportAsync(55,"正在等待现有云端传输安全结束").ConfigureAwait(false);
            using var cloudPause=await _cloudTransfers.PauseForRestoreAsync(ct).ConfigureAwait(false);
            state=RestoreState.CloudJobsPaused;await AuditAsync(game.PlayniteId,state,request,ct).ConfigureAwait(false);
            await progress.ReportAsync(60,"正在恢复指定版本").ConfigureAwait(false);
            try
            {
                // From this point onward the restore client may have changed a subset of live
                // files even when it throws or reports failure. Every failure path therefore
                // goes through the same mandatory PreRestore rollback below.
                var restored=await RestoreTargetAsync(targetBackupPath,match,request.BackupId,false,ct).ConfigureAwait(false);
                if(!restored.Success||!restored.Json.HasValue||LudusaviResultParser.SomeGamesFailed(restored.Json.Value))
                    throw new WorkerOperationException("RESTORE_WRITE_FAILED","恢复未完整写入目标存档。",restored.ErrorMessage);
                state=RestoreState.RestoreExecuted;await AuditAsync(game.PlayniteId,state,request,ct).ConfigureAwait(false);
                await progress.ReportAsync(88,"正在执行恢复后校验").ConfigureAwait(false);
                var post=await RestoreTargetAsync(targetBackupPath,match,request.BackupId,true,ct).ConfigureAwait(false);
                if(!post.Success||!post.Json.HasValue||LudusaviResultParser.SomeGamesFailed(post.Json.Value))
                    throw new WorkerOperationException("RESTORE_POST_VALIDATION_FAILED","恢复后校验未通过。",post.ErrorMessage);
                state=RestoreState.PostRestoreValidated;await AuditAsync(game.PlayniteId,state,request,ct).ConfigureAwait(false);
            }
            catch(Exception restoreError)
            {
                // Safety rollback must not inherit a user cancellation token: once live files
                // may have changed, restoring the locked snapshot is mandatory best effort.
                state=RestoreState.RollbackAttempted;
                await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId,error=restoreError.Message},CancellationToken.None).ConfigureAwait(false);
                try
                {
                    var rollback=await _ludusavi.RestoreAsync(match,preVersion.BackupId,false,CancellationToken.None).ConfigureAwait(false);
                    if(!rollback.Success||!rollback.Json.HasValue||LudusaviResultParser.SomeGamesFailed(rollback.Json.Value))
                    {
                        state=RestoreState.ManualInterventionRequired;
                        await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId,error=restoreError.Message,rollback=rollback.ErrorMessage},CancellationToken.None).ConfigureAwait(false);
                        throw new WorkerOperationException("RESTORE_ROLLBACK_FAILED","恢复失败，自动回滚也失败，需要人工检查存档目录。",rollback.ErrorMessage,restoreError);
                    }
                    state=RestoreState.RolledBack;
                    await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId,error=restoreError.Message},CancellationToken.None).ConfigureAwait(false);
                    throw new WorkerOperationException("RESTORE_FAILED_ROLLED_BACK","恢复未完成，已自动恢复到操作前的 PreRestore 快照。",restoreError.Message,restoreError);
                }
                catch(WorkerOperationException ex) when(ex.Code is "RESTORE_ROLLBACK_FAILED" or "RESTORE_FAILED_ROLLED_BACK")
                {
                    throw;
                }
                catch(Exception rollbackError)
                {
                    state=RestoreState.ManualInterventionRequired;
                    await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId,error=restoreError.Message,rollback=rollbackError.Message},CancellationToken.None).ConfigureAwait(false);
                    throw new WorkerOperationException("RESTORE_ROLLBACK_FAILED","恢复失败，自动回滚也发生异常，需要人工检查存档目录。",rollbackError.Message,restoreError);
                }
            }
            state=RestoreState.Completed;await AuditAsync(game.PlayniteId,state,new{request,preVersion.BackupId},ct).ConfigureAwait(false);
            await progress.ReportAsync(100,"安全恢复完成").ConfigureAwait(false);
        },token, requestId: request.RequestId).ConfigureAwait(false);
    }

    private Task<LudusaviCommandResult> RestoreTargetAsync(string? backupPath,string game,string backupId,bool preview,CancellationToken token)
        =>string.IsNullOrWhiteSpace(backupPath)
            ?_ludusavi.RestoreAsync(game,backupId,preview,token)
            :_ludusavi.RestoreFromPathAsync(backupPath,game,backupId,preview,token);

    public async Task<TaskStatusDto> UndoAsync(string playniteId,CancellationToken token,string requestId = "")
    {
        var version=(await _store.GetBackupVersionsAsync(playniteId,token).ConfigureAwait(false)).FirstOrDefault(x=>x.IsPreRestore)
            ??throw new InvalidOperationException("No PreRestore snapshot is indexed for this game.");
        return await ExecuteAsync(new RestoreRequestDto{RequestId=requestId,PlayniteId=playniteId,BackupId=version.BackupId,ConfirmedCurrentSnapshot=true,ConfirmedGameClosed=true,UserComment="Undo previous restore"},token).ConfigureAwait(false);
    }

    private async Task<string> ResolveAsync(string playniteId,CancellationToken token)
    {
        var matches=await _catalog.GetMatchesAsync(token).ConfigureAwait(false);
        return matches.TryGetValue(playniteId,out var match)&&!string.IsNullOrWhiteSpace(match.Name)?match.Name:throw new InvalidOperationException("Game is not matched to Ludusavi.");
    }

    private async Task EnsureGameClosedAsync(string playniteId,CancellationToken token)
    {
        if(_sessions.ActiveSessions.Any(x=>string.Equals(x.PlayniteId,playniteId,StringComparison.OrdinalIgnoreCase)))
            throw new WorkerOperationException("RESTORE_GAME_RUNNING","Worker 仍检测到该游戏会话正在运行，已阻止恢复。请退出游戏并稍后重试。",playniteId);

        var persisted=await _store.GetOpenSessionsAsync(token).ConfigureAwait(false);
        foreach(var session in persisted.Where(x=>string.Equals(x.PlayniteId,playniteId,StringComparison.OrdinalIgnoreCase)))
        {
            if(session.ProcessId is not int processId)continue;
            try
            {
                using var process=System.Diagnostics.Process.GetProcessById(processId);
                if(!process.HasExited)
                    throw new WorkerOperationException("RESTORE_GAME_PROCESS_RUNNING","Worker 仍检测到该游戏的已记录进程，已阻止恢复。请退出游戏、启动器及相关 MOD 工具后重试。",$"PID={processId}; Name={process.ProcessName}");
            }
            catch(ArgumentException)
            {
                // A stale persisted session cannot prove that a game is still running. The explicit
                // confirmation remains the safe fallback if no live process is known to the Worker.
            }
        }
    }

    private Task AuditAsync(string gameId,RestoreState state,object detail,CancellationToken token)=>_store.AppendAuditAsync("Restore",state.ToString(),JsonSerializer.Serialize(new{gameId,state,detail}),token);
}
