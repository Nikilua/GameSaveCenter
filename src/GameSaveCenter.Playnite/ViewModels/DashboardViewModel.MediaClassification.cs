using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

public sealed class MediaClassificationHistoryStateOption
{
    public MediaClassificationHistoryStateOption(string code, string display)
    {
        Code = code;
        Display = display;
    }

    public string Code { get; }
    public string Display { get; }
}

public sealed partial class DashboardViewModel
{
    public IReadOnlyList<MediaClassificationHistoryStateOption> MediaClassificationHistoryStateOptions { get; } = new[]
    {
        new MediaClassificationHistoryStateOption(string.Empty, "全部批次"),
        new MediaClassificationHistoryStateOption("Preview", "待确认"),
        new MediaClassificationHistoryStateOption("Applied", "已应用"),
        new MediaClassificationHistoryStateOption("AppliedWithConflicts", "已应用 · 有冲突"),
        new MediaClassificationHistoryStateOption("Undone", "已撤销"),
        new MediaClassificationHistoryStateOption("UndoneWithConflicts", "已撤销 · 有冲突"),
        new MediaClassificationHistoryStateOption("Conflict", "应用冲突"),
        new MediaClassificationHistoryStateOption("Expired", "已过期")
    };

    private async Task LoadMediaClassificationHistoryAsync(bool reset, bool allowConsistencyRetry = true)
    {
        if (CurrentWorkspace != WorkspaceKind.Media)
            return;

        var generation = Interlocked.Increment(ref mediaClassificationHistoryLoadGeneration);
        var page = reset ? 0 : mediaClassificationHistoryPage + 1;
        var requestCancellation = BeginMediaClassificationHistoryRequest();
        try
        {
            var response = await plugin.RequestAsync<MediaClassificationHistoryDto>(
                MessageTypes.ListMediaClassificationHistory,
                new MediaClassificationHistoryRequestDto
                {
                    Page = page,
                    PageSize = 25,
                    State = MediaClassificationHistoryStateFilter,
                    ConsistencyToken = reset ? string.Empty : mediaClassificationHistoryConsistencyToken
                },
                cancellationToken: requestCancellation.Token).ConfigureAwait(false);

            var retryFromFirstPage = false;
            var pageResetReceived = false;
            var loadPendingSelectionPage = false;
            ApplyOnUi(() =>
            {
                if (generation != Interlocked.Read(ref mediaClassificationHistoryLoadGeneration)
                    || CurrentWorkspace != WorkspaceKind.Media)
                    return;

                var selectedBatchId = !string.IsNullOrWhiteSpace(pendingMediaClassificationBatchId)
                    ? pendingMediaClassificationBatchId
                    : SelectedMediaClassificationBatch?.BatchId;
                if (response?.PageResetRequired == true)
                {
                    pageResetReceived = true;
                    if (!string.IsNullOrWhiteSpace(selectedBatchId))
                        pendingMediaClassificationBatchId = selectedBatchId;
                    MediaClassificationHistoryItems.ReplaceAll(
                        Array.Empty<MediaClassificationBatchSummaryDto>(), AreSameMediaClassificationBatch);
                    mediaClassificationHistoryPage = 0;
                    mediaClassificationHistoryHasMore = false;
                    mediaClassificationHistoryConsistencyToken = response.ConsistencyToken ?? string.Empty;
                    SelectedMediaClassificationBatch = null;
                    mediaClassificationHistoryNeedsManualRefresh = !allowConsistencyRetry;
                    StatusMessage = !allowConsistencyRetry
                        ? "媒体归类历史仍在持续变化，自动重试已停止，请点击“刷新”后继续。"
                        : string.IsNullOrWhiteSpace(response.PageResetReason)
                            ? "媒体归类历史已更新，正在从第一页刷新。"
                            : response.PageResetReason;
                    OnPropertyChanged(nameof(MediaClassificationHistoryHasMore));
                    OnPropertyChanged(nameof(MediaClassificationHistoryNeedsManualRefresh));
                    OnPropertyChanged(nameof(MediaClassificationHistoryLoadedSummary));
                    RaiseCommandStates();
                    retryFromFirstPage = allowConsistencyRetry;
                    return;
                }

                var incoming = response?.Items ?? new List<MediaClassificationBatchSummaryDto>();
                if (reset)
                {
                    MediaClassificationHistoryItems.ReplaceAll(incoming, AreSameMediaClassificationBatch);
                }
                else if (incoming.Count > 0)
                {
                    MediaClassificationHistoryItems.ApplyBatch(() =>
                    {
                        var existing = new HashSet<string>(
                            MediaClassificationHistoryItems.Select(x => x.BatchId), StringComparer.OrdinalIgnoreCase);
                        foreach (var item in incoming)
                            if (existing.Add(item.BatchId)) MediaClassificationHistoryItems.Add(item);
                    });
                }

                mediaClassificationHistoryPage = response?.Page ?? page;
                mediaClassificationHistoryHasMore = response?.HasMore == true;
                mediaClassificationHistoryConsistencyToken = response?.ConsistencyToken ?? string.Empty;
                mediaClassificationHistoryNeedsManualRefresh = false;
                var restored = !string.IsNullOrWhiteSpace(selectedBatchId)
                    ? MediaClassificationHistoryItems.FirstOrDefault(x => string.Equals(x.BatchId, selectedBatchId, StringComparison.OrdinalIgnoreCase))
                    : null;
                SelectedMediaClassificationBatch = restored;
                var shouldLoadPending = restored == null
                    && !string.IsNullOrWhiteSpace(selectedBatchId)
                    && response?.HasMore == true;
                if (shouldLoadPending)
                    pendingMediaClassificationBatchId = selectedBatchId;
                if (restored != null && string.Equals(pendingMediaClassificationBatchId, restored.BatchId, StringComparison.OrdinalIgnoreCase))
                    pendingMediaClassificationBatchId = null;
                else if (!shouldLoadPending)
                    pendingMediaClassificationBatchId = null;
                if (string.IsNullOrWhiteSpace(LastMediaClassificationBatchId))
                {
                    var latestUndoable = MediaClassificationHistoryItems.FirstOrDefault(x => x.IsUndoable);
                    if (latestUndoable != null)
                        LastMediaClassificationBatchId = latestUndoable.BatchId;
                }
                OnPropertyChanged(nameof(MediaClassificationHistoryHasMore));
                OnPropertyChanged(nameof(MediaClassificationHistoryNeedsManualRefresh));
                OnPropertyChanged(nameof(MediaClassificationHistoryLoadedSummary));
                RaiseCommandStates();
                if (shouldLoadPending)
                    loadPendingSelectionPage = true;
            });
            if (retryFromFirstPage)
                await LoadMediaClassificationHistoryAsync(true, false).ConfigureAwait(false);
            else if (!pageResetReceived && loadPendingSelectionPage && !string.IsNullOrWhiteSpace(pendingMediaClassificationBatchId))
                await LoadMediaClassificationHistoryAsync(false, allowConsistencyRetry).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (requestCancellation.IsCancellationRequested)
        {
            // A newer history request or workspace switch superseded this page.
        }
        finally
        {
            EndMediaClassificationHistoryRequest(requestCancellation);
        }
    }

    private bool CanUndoMediaClassification()
    {
        if (SelectedMediaClassificationBatch != null)
            return SelectedMediaClassificationBatch.IsUndoable;
        return !string.IsNullOrWhiteSpace(LastMediaClassificationBatchId);
    }

    private string GetMediaClassificationUndoBatchId()
    {
        if (SelectedMediaClassificationBatch?.IsUndoable == true)
            return SelectedMediaClassificationBatch.BatchId;
        return LastMediaClassificationBatchId;
    }

    private static bool AreSameMediaClassificationBatch(
        MediaClassificationBatchSummaryDto left, MediaClassificationBatchSummaryDto right)
        => string.Equals(left.BatchId, right.BatchId, StringComparison.OrdinalIgnoreCase)
           && left.State == right.State
           && left.UpdatedUtc == right.UpdatedUtc
           && left.ItemCount == right.ItemCount
           && left.AppliedCount == right.AppliedCount
           && left.UndoneCount == right.UndoneCount
           && left.ConflictCount == right.ConflictCount
           && left.LastError == right.LastError;
}
