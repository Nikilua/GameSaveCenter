using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Publishes and compares small, content-free device backup summaries. It never restores or overwrites a save.</summary>
public sealed class DeviceStateService
{
    private const int MaxComparisonsPerDevice = 500;
    private const int MaxSidecarLength = 1024 * 1024;
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly RcloneClient _rclone;
    private readonly CloudTransferCoordinator _cloudTransfers;
    private readonly ILogger<DeviceStateService> _logger;
    private readonly DeviceConflictDetector _detector=new();
    private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web){WriteIndented=true};

    public DeviceStateService(WorkerOptions options,SqliteStateStore store,RcloneClient rclone,CloudTransferCoordinator cloudTransfers,ILogger<DeviceStateService> logger)
    { _options=options;_store=store;_rclone=rclone;_cloudTransfers=cloudTransfers;_logger=logger; }

    public async Task<DeviceStateSyncResultDto> SyncAsync(CancellationToken token)
    {
        var device=Environment.MachineName;
        var deviceId=_options.DeviceId;
        var sidecar=new DeviceStateSidecarDto{SchemaVersion=4,DeviceId=deviceId,DeviceName=device,GeneratedUtc=DateTime.UtcNow,
            Backups=await _store.GetLatestBackupSummariesAsync(token).ConfigureAwait(false)};
        var directory=Path.Combine(_options.DataDirectory,"DeviceState");Directory.CreateDirectory(directory);
        var localPath=Path.Combine(directory,deviceId+".json");
        var temporary=localPath+".tmp";
        await File.WriteAllTextAsync(temporary,JsonSerializer.Serialize(sidecar,JsonOptions),token).ConfigureAwait(false);
        File.Move(temporary,localPath,true);

        var result=new DeviceStateSyncResultDto{LocalDevice=device,GeneratedUtc=sidecar.GeneratedUtc,
            StatusMessage="本地设备摘要已更新；尚未配置云端，未读取其他设备。"};
        if(!_options.EnableCloudUpload||!_rclone.IsConfigured)return result;

        var upload=await _cloudTransfers.RunUploadAsync("device state",ct=>_rclone.CopyAsync(directory,Path.Combine(deviceId,"DeviceState"),ct),token).ConfigureAwait(false);
        if(!upload.Success)
        {
            result.StatusMessage="本地设备摘要已更新，但上传失败："+upload.StandardError;
            return result;
        }
        result.Uploaded=true;
        var (listing, remotePaths, hasMoreFiles) = await _rclone.ListDeviceStateFilesAsync(token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (!listing.Success)
        {
            _logger.LogWarning("Device-state listing failed with exit code {ExitCode} and error code {ErrorCode}.",
                listing.ExitCode, listing.ErrorCode);
            result.StatusMessage = "已上传本机摘要。读取云端设备摘要列表失败，请检查云端连接后重试。";
            return result;
        }

        var localByGame=sidecar.Backups.ToDictionary(x=>x.PlayniteId,StringComparer.OrdinalIgnoreCase);
        var unreadableFiles = 0;
        foreach(var remotePath in remotePaths)
        {
            token.ThrowIfCancellationRequested();
            var read = await _rclone.ReadDeviceStateFileAsync(remotePath, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (!read.Success || read.StandardOutput.Length > MaxSidecarLength || string.IsNullOrWhiteSpace(read.StandardOutput))
            {
                unreadableFiles++;
                _logger.LogWarning("Could not read a device-state sidecar. Exit code {ExitCode}, error code {ErrorCode}, output length {Length}.",
                    read.ExitCode, read.ErrorCode, read.StandardOutput.Length);
                continue;
            }
            try
            {
                var remote=JsonSerializer.Deserialize<DeviceStateSidecarDto>(read.StandardOutput,JsonOptions);
                if (remote == null || remote.SchemaVersion is < 1 or > 4 || string.IsNullOrWhiteSpace(remote.DeviceName) || remote.Backups == null)
                {
                    unreadableFiles++;
                    continue;
                }
                var remoteDeviceId=WorkerOptions.IsValidDeviceId(remote.DeviceId)
                    ? remote.DeviceId.ToLowerInvariant()
                    : remote.DeviceName;
                if(string.Equals(remoteDeviceId,deviceId,StringComparison.OrdinalIgnoreCase))continue;
                result.RemoteSidecarsRead++;
                var coverage=new DeviceComparisonCoverageDto
                {
                    DeviceName=remote.DeviceName,
                    DeviceId=WorkerOptions.IsValidDeviceId(remote.DeviceId) ? remoteDeviceId : string.Empty,
                    Total=remote.Backups.Count
                };
                result.Coverage.Add(coverage);
                var compared = new List<DeviceConflictStatusDto>();
                foreach(var item in remote.Backups.Take(MaxComparisonsPerDevice))
                {
                    token.ThrowIfCancellationRequested();
                    if(item==null||string.IsNullOrWhiteSpace(item.PlayniteId))continue;
                    localByGame.TryGetValue(item.PlayniteId,out var local);
                    var conflict=_detector.Detect(ToSnapshot(local,deviceId),ToSnapshot(item,remoteDeviceId));
                    compared.Add(new DeviceConflictStatusDto
                    {
                        PlayniteId=item.PlayniteId,GameName=local?.GameName??item.GameName,RemoteDevice=remote.DeviceName,RemoteDeviceId=remoteDeviceId,
                        LocalDevice=device,LocalBackupId=local?.BackupId??string.Empty,RemoteBackupId=item.BackupId,
                        LocalParentBackupId=local?.ParentBackupId??string.Empty,RemoteParentBackupId=item.ParentBackupId,
                        LocalTotalBytes=local?.TotalBytes??0,RemoteTotalBytes=item.TotalBytes,
                        LocalFileCount=local?.FileCount??0,RemoteFileCount=item.FileCount,
                        LocalContentFingerprintAvailable=!string.IsNullOrWhiteSpace(local?.ContentFingerprint),
                        RemoteContentFingerprintAvailable=!string.IsNullOrWhiteSpace(item.ContentFingerprint),
                        ContentFingerprintsMatch=HasComparableFingerprints(local,item)
                            ? string.Equals(local!.ContentFingerprint,item.ContentFingerprint,StringComparison.OrdinalIgnoreCase)
                            : null,
                        LocalReadinessStatus=local?.RestoreReadinessStatus??string.Empty,RemoteReadinessStatus=item.RestoreReadinessStatus,
                        LocalReadinessCheckedUtc=local?.RestoreReadinessCheckedUtc,RemoteReadinessCheckedUtc=item.RestoreReadinessCheckedUtc,
                        LocalSummaryGeneratedUtc=sidecar.GeneratedUtc,RemoteSummaryGeneratedUtc=remote.GeneratedUtc==default?null:remote.GeneratedUtc,
                        HasConflict=conflict.HasConflict,
                        Reason=conflict.Reason,SuggestedBackupId=conflict.PreferredBackupId,Confidence=conflict.Confidence,
                        LocalCreatedUtc=local?.CreatedUtc??default,RemoteCreatedUtc=item.CreatedUtc
                    });
                }
                coverage.Compared = compared.Count;
                result.Comparisons.AddRange(compared);
            }
            catch (JsonException)
            {
                unreadableFiles++;
                _logger.LogWarning("A device-state sidecar contains invalid JSON.");
            }
        }
        token.ThrowIfCancellationRequested();
        var fileWarning = unreadableFiles > 0 ? $"有 {unreadableFiles} 份云端设备摘要读取失败或格式无效。" : string.Empty;
        if (hasMoreFiles)
            fileWarning += "云端摘要文件超过本次读取上限，其余文件尚未比较。";
        var partial = fileWarning.Length > 0 || result.Coverage.Any(x => x.Compared < x.Total);
        var uniqueNames = result.Coverage.GroupBy(x => x.DeviceName, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() == 1).Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var group in result.Comparisons.GroupBy(x => x.RemoteDeviceId, StringComparer.OrdinalIgnoreCase))
        {
            var name = group.First().RemoteDevice;
            var legacyName = !partial && uniqueNames.Contains(name) ? name : string.Empty;
            if (!WorkerOptions.IsValidDeviceId(group.Key) && legacyName.Length == 0) continue;
            var decisions = await _store.GetDeviceConflictDecisionsAsync(group.Key, legacyName,
                group.Select(x => x.PlayniteId).Distinct(StringComparer.Ordinal).ToArray(), token).ConfigureAwait(false);
            foreach (var comparison in group)
                if (decisions.TryGetValue(comparison.PlayniteId, out var decision))
                    comparison.ApplyRecordedDecision(decision);
        }
        await _store.ObserveDeviceComparisonAsync(result, localByGame, !partial, token).ConfigureAwait(false);
        result.Comparisons=result.Comparisons.OrderByDescending(x=>x.HasConflict).ThenBy(x=>x.GameName,StringComparer.OrdinalIgnoreCase).ToList();
        result.StatusMessage = result.RemoteSidecarsRead == 0
            ? (partial ? $"已上传本机摘要。{fileWarning}未能完成其他设备的比较。" : "已上传本机摘要；云端暂未发现其他设备摘要。")
            : (partial ? "仅完成部分比较。其余条目仍待比较。" : "设备摘要已比较。")
              + fileWarning
              + (result.Comparisons.Any(x => x.HasConflict)
                  ? "在已比较条目中发现需要人工决定的设备分叉。"
                  : "在已比较条目中未发现需要人工决定的分叉。");
        return result;
    }

    private static BackupSnapshot? ToSnapshot(DeviceBackupSummaryDto? value,string device)=>value==null?null:new BackupSnapshot
    { BackupId=value.BackupId,ParentBackupId=value.ParentBackupId,SourceDevice=device,CreatedUtc=value.CreatedUtc,TotalBytes=value.TotalBytes,FileCount=value.FileCount,ContentFingerprint=value.ContentFingerprint };

    private static bool HasComparableFingerprints(DeviceBackupSummaryDto? local,DeviceBackupSummaryDto remote)
        => !string.IsNullOrWhiteSpace(local?.ContentFingerprint)&&!string.IsNullOrWhiteSpace(remote.ContentFingerprint);
}
