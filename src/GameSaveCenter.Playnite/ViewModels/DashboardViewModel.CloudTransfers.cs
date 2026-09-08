using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

public sealed class CloudTransferFilterOption
{
    public CloudTransferFilterOption(string code, string display)
    {
        Code = code;
        Display = display;
    }

    public string Code { get; }
    public string Display { get; }
}

public sealed partial class DashboardViewModel
{
    private async Task LoadCloudTransferPageAsync(bool reset, bool allowConsistencyRetry = true)
    {
        if (CurrentWorkspace != WorkspaceKind.Maintenance)
            return;

        var generation = Interlocked.Increment(ref cloudTransferLoadGeneration);
        var page = reset ? 0 : cloudTransferPage + 1;
        var requestCancellation = BeginCloudTransferRequest();
        try
        {
            var request = new CloudTransferStatusRequestDto
            {
                Page = page,
                PageSize = 100,
                State = CloudTransferStateFilter,
                Kind = ParseCloudTransferKind(CloudTransferKindFilter),
                ConsistencyToken = reset ? string.Empty : cloudTransferConsistencyToken
            };
            var response = await plugin.RequestAsync<CloudTransferSummaryDto>(
                MessageTypes.GetCloudTransferStatus,
                request,
                cancellationToken: requestCancellation.Token).ConfigureAwait(false);

            var retryFromFirstPage = false;
            var pageResetReceived = false;
            var loadPendingSelectionPage = false;
            ApplyOnUi(() =>
            {
                if (generation != Interlocked.Read(ref cloudTransferLoadGeneration))
                    return;

                if (CurrentWorkspace != WorkspaceKind.Maintenance)
                    return;

                var selectedKey = !string.IsNullOrWhiteSpace(pendingCloudTransferKey)
                    ? pendingCloudTransferKey
                    : SelectedCloudTransfer?.TransferKey;
                if (response?.PageResetRequired == true)
                {
                    pageResetReceived = true;
                    if (!string.IsNullOrWhiteSpace(selectedKey))
                        pendingCloudTransferKey = selectedKey;
                    CloudTransferItems.ReplaceAll(Array.Empty<CloudTransferStatusDto>(), AreSameCloudTransfer);
                    CloudTransferViewSummary = response;
                    cloudTransferPage = 0;
                    cloudTransferHasMore = false;
                    cloudTransferConsistencyToken = response.ConsistencyToken ?? string.Empty;
                    SelectedCloudTransfer = null!;
                    cloudTransferNeedsManualRefresh = !allowConsistencyRetry;
                    StatusMessage = !allowConsistencyRetry
                        ? "云端队列仍在持续变化，自动重试已停止，请点击“刷新队列”后继续。"
                        : string.IsNullOrWhiteSpace(response.PageResetReason)
                            ? "云端队列已更新，正在从第一页刷新。"
                            : response.PageResetReason;
                    OnPropertyChanged(nameof(CloudTransferHasMore));
                    OnPropertyChanged(nameof(CloudTransferNeedsManualRefresh));
                    OnPropertyChanged(nameof(CloudTransferLoadedSummary));
                    RaiseCommandStates();
                    retryFromFirstPage = allowConsistencyRetry;
                    return;
                }

                if (reset)
                {
                    Replace(CloudTransferItems, response?.Items ?? Enumerable.Empty<CloudTransferStatusDto>(), AreSameCloudTransfer);
                }
                else if (response?.Items != null && response.Items.Count > 0)
                {
                    var incoming = response.Items;
                    CloudTransferItems.ApplyBatch(() =>
                    {
                        var existingKeys = new HashSet<string>(CloudTransferItems.Select(x => x.TransferKey), StringComparer.OrdinalIgnoreCase);
                        foreach (var item in incoming)
                        {
                            if (existingKeys.Add(item.TransferKey))
                                CloudTransferItems.Add(item);
                        }
                    });
                }

                CloudTransferViewSummary = response ?? new CloudTransferSummaryDto();
                cloudTransferPage = response?.Page ?? page;
                cloudTransferHasMore = response?.HasMore == true;
                cloudTransferConsistencyToken = response?.ConsistencyToken ?? string.Empty;
                cloudTransferNeedsManualRefresh = false;
                var restored = !string.IsNullOrWhiteSpace(selectedKey)
                    ? CloudTransferItems.FirstOrDefault(x => string.Equals(x.TransferKey, selectedKey, StringComparison.OrdinalIgnoreCase))
                    : null;
                SelectedCloudTransfer = restored!;
                var shouldLoadPending = restored == null
                    && !string.IsNullOrWhiteSpace(selectedKey)
                    && response?.HasMore == true;
                if (shouldLoadPending)
                    pendingCloudTransferKey = selectedKey;
                if (restored != null && string.Equals(pendingCloudTransferKey, restored.TransferKey, StringComparison.OrdinalIgnoreCase))
                    pendingCloudTransferKey = null;
                else if (!shouldLoadPending)
                    pendingCloudTransferKey = null;
                OnPropertyChanged(nameof(CloudTransferHasMore));
                OnPropertyChanged(nameof(CloudTransferNeedsManualRefresh));
                OnPropertyChanged(nameof(CloudTransferLoadedSummary));
                RebuildMaintenanceActionItems();
                if (shouldLoadPending)
                    loadPendingSelectionPage = true;
            });
            if (retryFromFirstPage)
                await LoadCloudTransferPageAsync(true, false).ConfigureAwait(false);
            else if (!pageResetReceived && loadPendingSelectionPage && !string.IsNullOrWhiteSpace(pendingCloudTransferKey))
                await LoadCloudTransferPageAsync(false, allowConsistencyRetry).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (requestCancellation.IsCancellationRequested)
        {
            // A filter refresh or workspace switch superseded this page request.
        }
        finally
        {
            EndCloudTransferRequest(requestCancellation);
        }
    }

    private async Task VerifySelectedCloudTransferAsync()
    {
        var selected = SelectedCloudTransfer ?? throw new InvalidOperationException("请先选择一条云端传输记录。");
        var result = await plugin.RequestAsync<CloudTransferStatusDto>(
            MessageTypes.VerifyCloudTransfer,
            new CloudTransferVerifyRequestDto { PlayniteId = selected.PlayniteId, Kind = selected.Kind },
            TimeSpan.FromHours(2));
        StatusMessage = result?.State == "RemoteVerified"
            ? "远端 check 已成功；本地副本未被修改。"
            : "远端 check 已完成，但未提升为远端已校验。";
        await LoadCloudTransferPageAsync(true);
    }

    private async Task RetrySelectedCloudUploadAsync()
    {
        var selected = SelectedCloudTransfer ?? throw new InvalidOperationException("请先选择一条云端传输记录。");
        if (selected.Kind == CloudTransferKind.Media)
        {
            var result = await plugin.RequestAsync<MediaCloudRetryResultDto>(
                MessageTypes.RetryMediaCloudUpload,
                new MediaCloudRetryRequestDto { PlayniteId = selected.PlayniteId },
                TimeSpan.FromHours(2));
            StatusMessage = result?.Outcome switch
            {
                MediaCloudRetryOutcome.Submitted when result.Task?.State == TaskState.Succeeded
                    => result.Message,
                MediaCloudRetryOutcome.Submitted
                    => "媒体云端上传重试已提交；请在队列状态变为已上传或远端已校验后再确认结果。",
                MediaCloudRetryOutcome.PausedByPolicy
                    => result.Message,
                _
                    => $"媒体云端上传重试未提交：{result?.Message ?? "Worker 未返回结果。"}"
            };
        }
        else
        {
            await plugin.RequestAsync<TaskStatusDto>(
                MessageTypes.RetryCloudUpload,
                new GameQueryDto { PlayniteId = selected.PlayniteId },
                TimeSpan.FromHours(2));
            StatusMessage = "云端上传重试已提交；请在队列状态变为已上传或远端已校验后再确认结果。";
        }
        await LoadCloudTransferPageAsync(true);
        await RefreshDashboardAsync(false, false);
    }

    private bool CanVerifySelectedCloudTransfer()
    {
        var state = SelectedCloudTransfer?.State;
        return state != null
            && !string.Equals(state, "Pending", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(state, "Transferring", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(state, "Verifying", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(state, "Paused", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(state, "AuthenticationRequired", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(state, "RetryScheduled", StringComparison.OrdinalIgnoreCase);
    }

    private bool CanRetrySelectedCloudUpload()
    {
        var state = SelectedCloudTransfer?.State;
        return string.Equals(state, "Failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(state, "RetryScheduled", StringComparison.OrdinalIgnoreCase);
    }

    private static CloudTransferKind? ParseCloudTransferKind(string value)
        => value switch
        {
            "Backup" => CloudTransferKind.Backup,
            "Media" => CloudTransferKind.Media,
            _ => null
        };

    private static bool AreSameCloudTransfer(CloudTransferStatusDto left, CloudTransferStatusDto right)
        => string.Equals(left.TransferKey, right.TransferKey, StringComparison.OrdinalIgnoreCase)
           && left.State == right.State
           && left.OperationKind == right.OperationKind
           && left.AttemptCount == right.AttemptCount
           && left.NextAttemptUtc == right.NextAttemptUtc
           && left.LastAttemptUtc == right.LastAttemptUtc
           && left.LastErrorCode == right.LastErrorCode
           && left.LastError == right.LastError
           && left.UpdatedUtc == right.UpdatedUtc
           && left.GameName == right.GameName;
}
