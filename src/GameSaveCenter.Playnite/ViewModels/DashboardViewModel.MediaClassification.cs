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

    private async Task LoadMediaClassificationHistoryAsync(bool reset)
    {
        if (CurrentWorkspace != WorkspaceKind.Media)
            return;

        var generation = Interlocked.Increment(ref mediaClassificationHistoryLoadGeneration);
        var page = reset ? 0 : mediaClassificationHistoryPage + 1;
        var selectedBatchId = SelectedMediaClassificationBatch?.BatchId;
        var requestCancellation = BeginMediaClassificationHistoryRequest();
        try
        {
            var response = await plugin.RequestAsync<MediaClassificationHistoryDto>(
                MessageTypes.ListMediaClassificationHistory,
                new MediaClassificationHistoryRequestDto
                {
                    Page = page,
                    PageSize = 25,
                    State = MediaClassificationHistoryStateFilter
                },
                cancellationToken: requestCancellation.Token).ConfigureAwait(false);

            ApplyOnUi(() =>
            {
                if (generation != Interlocked.Read(ref mediaClassificationHistoryLoadGeneration)
                    || CurrentWorkspace != WorkspaceKind.Media)
                    return;

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
                var restored = !string.IsNullOrWhiteSpace(selectedBatchId)
                    ? MediaClassificationHistoryItems.FirstOrDefault(x => string.Equals(x.BatchId, selectedBatchId, StringComparison.OrdinalIgnoreCase))
                    : null;
                if (restored != null || reset)
                    SelectedMediaClassificationBatch = restored;
                if (string.IsNullOrWhiteSpace(LastMediaClassificationBatchId))
                {
                    var latestUndoable = MediaClassificationHistoryItems.FirstOrDefault(x => x.IsUndoable);
                    if (latestUndoable != null)
                        LastMediaClassificationBatchId = latestUndoable.BatchId;
                }
                OnPropertyChanged(nameof(MediaClassificationHistoryHasMore));
                OnPropertyChanged(nameof(MediaClassificationHistoryLoadedSummary));
                RaiseCommandStates();
            });
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
