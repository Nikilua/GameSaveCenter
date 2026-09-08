using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels;

public enum MaintenanceActionKind
{
    HealthInspection,
    CloudTransfer,
    RetentionQuarantine
}

public enum MaintenanceActionGroup
{
    NeedsManualHandling,
    WaitingForRetry,
    Routine
}

internal sealed class MaintenanceCloudTransferMergeResult
{
    public MaintenanceCloudTransferMergeResult(
        IReadOnlyList<CloudTransferStatusDto> items,
        int supersededSnapshotAttentionCount,
        int addedAttentionCount)
    {
        Items = items;
        SupersededSnapshotAttentionCount = supersededSnapshotAttentionCount;
        AddedAttentionCount = addedAttentionCount;
    }

    public IReadOnlyList<CloudTransferStatusDto> Items { get; }
    public int SupersededSnapshotAttentionCount { get; }
    public int AddedAttentionCount { get; }

    public int GetEffectiveAttentionCount(int snapshotAttentionCount)
        => Math.Max(0, snapshotAttentionCount - SupersededSnapshotAttentionCount + AddedAttentionCount);
}

/// <summary>
/// Merges the dashboard snapshot and independently paged cloud rows before deciding
/// whether a transfer still needs attention. The detail page wins ties; otherwise the
/// durable UpdatedUtc is the source of truth for the latest state.
/// </summary>
internal static class MaintenanceCloudTransferResolver
{
    public static MaintenanceCloudTransferMergeResult Resolve(
        IEnumerable<CloudTransferStatusDto>? snapshotItems,
        IEnumerable<CloudTransferStatusDto>? loadedItems)
    {
        var candidates = (snapshotItems ?? Enumerable.Empty<CloudTransferStatusDto>())
            .Select(item => new Candidate(item, 0))
            .Concat((loadedItems ?? Enumerable.Empty<CloudTransferStatusDto>())
                .Select(item => new Candidate(item, 1)))
            .ToList();

        var merged = new List<CloudTransferStatusDto>();
        var supersededSnapshotAttention = 0;
        var addedAttention = 0;
        foreach (var group in candidates.GroupBy(candidate => candidate.Item.TransferKey, StringComparer.OrdinalIgnoreCase))
        {
            var latest = group
                .OrderBy(candidate => candidate.Item.UpdatedUtc)
                .ThenBy(candidate => candidate.SourcePriority)
                .Last();
            merged.Add(latest.Item);

            var snapshot = group.Where(candidate => candidate.SourcePriority == 0).ToList();
            var snapshotHadAttention = snapshot.Any(candidate => IsAttention(candidate.Item));
            var latestNeedsAttention = IsAttention(latest.Item);
            if (snapshotHadAttention && !latestNeedsAttention && latest.SourcePriority > 0)
                supersededSnapshotAttention++;
            else if (latestNeedsAttention && !snapshotHadAttention)
                addedAttention++;
        }

        return new MaintenanceCloudTransferMergeResult(merged, supersededSnapshotAttention, addedAttention);
    }

    public static bool IsAttention(CloudTransferStatusDto item)
        => item.State is "RetryScheduled" or "AuthenticationRequired" or "CheckFailed" or "Failed";

    private sealed class Candidate
    {
        public Candidate(CloudTransferStatusDto item, int sourcePriority)
        {
            Item = item;
            SourcePriority = sourcePriority;
        }

        public CloudTransferStatusDto Item { get; }
        public int SourcePriority { get; }
    }
}

/// <summary>
/// One concrete next step in the maintenance overview. The payload identifies the
/// existing record that the action operates on; the view never invents a bulk repair.
/// </summary>
public sealed class MaintenanceActionItem
{
    public string ItemId { get; set; } = string.Empty;
    public string CategoryDisplay { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string LastVerifiedDisplay { get; set; } = "尚未验证";
    public string LastAttemptDisplay { get; set; } = "尚未尝试";
    public string LedgerUpdatedDisplay { get; set; } = "尚未更新";
    public string NextAttemptDisplay { get; set; } = "待安排";
    public string TimingDisplay => ActionKind switch
    {
        MaintenanceActionKind.CloudTransfer => $"上次尝试：{LastAttemptDisplay} · 下次尝试：{NextAttemptDisplay}",
        MaintenanceActionKind.RetentionQuarantine => $"账本更新：{LedgerUpdatedDisplay} · 下次尝试：{NextAttemptDisplay}",
        _ => $"上次验证：{LastVerifiedDisplay} · 下次尝试：{NextAttemptDisplay}"
    };
    public string ActionText { get; set; } = string.Empty;
    public string ActionToolTip { get; set; } = string.Empty;
    public MaintenanceActionKind ActionKind { get; set; }
    public string TransferKey { get; set; } = string.Empty;
    public CloudTransferKind TransferKind { get; set; }
    public string TransferState { get; set; } = string.Empty;
    public string EntryId { get; set; } = string.Empty;

    public MaintenanceActionGroup Group => ActionKind switch
    {
        MaintenanceActionKind.RetentionQuarantine => MaintenanceActionGroup.NeedsManualHandling,
        MaintenanceActionKind.CloudTransfer when string.Equals(TransferState, "RetryScheduled", StringComparison.OrdinalIgnoreCase)
            => MaintenanceActionGroup.WaitingForRetry,
        MaintenanceActionKind.CloudTransfer => MaintenanceActionGroup.NeedsManualHandling,
        _ => MaintenanceActionGroup.Routine
    };
}

/// <summary>
/// One bounded section in the maintenance overview. The complete records remain available
/// through the explicit overflow expander; the overview itself does not grow once a group
/// contains more than its first few actionable records.
/// </summary>
public sealed class MaintenanceActionSection
{
    public MaintenanceActionSection(
        MaintenanceActionGroup group,
        string title,
        string description,
        IEnumerable<MaintenanceActionItem> items)
    {
        Group = group;
        Title = title;
        Description = description;
        Items = items.ToArray();
    }

    public MaintenanceActionGroup Group { get; }
    public string Title { get; }
    public string Description { get; }
    public IReadOnlyList<MaintenanceActionItem> Items { get; }
    public IReadOnlyList<MaintenanceActionItem> PreviewItems => Items.Take(3).ToArray();
    public IReadOnlyList<MaintenanceActionItem> OverflowItems => Items.Skip(3).ToArray();
    public int OverflowCount => Math.Max(0, Items.Count - 3);
    public bool HasOverflow => OverflowCount > 0;
    public string OverflowHeader => $"显示其余 {OverflowCount} 项";
}

public sealed partial class DashboardViewModel
{
    private string? pendingCloudTransferKey;
    private IReadOnlyList<MaintenanceActionSection> maintenanceActionSections = Array.Empty<MaintenanceActionSection>();

    public BatchObservableCollection<RetentionQuarantineEntryDto> PendingQuarantineEntries { get; } = new();
    public BatchObservableCollection<MaintenanceActionItem> MaintenanceActionItems { get; } = new();
    public IReadOnlyList<MaintenanceActionSection> MaintenanceActionSections
    {
        get => maintenanceActionSections;
        private set => SetValue(ref maintenanceActionSections, value);
    }

    public string MaintenanceActionSummary
    {
        get
        {
            var cloudMerge = MaintenanceCloudTransferResolver.Resolve(
                Snapshot.CloudTransfers?.Items,
                CloudTransferItems);
            var cloudCount = cloudMerge.GetEffectiveAttentionCount(Snapshot.CloudTransfers?.AttentionCount ?? 0);
            var quarantineCount = PendingQuarantineEntries.Count;
            return $"恢复巡检：{Snapshot.HealthInspection?.LastStatusDisplay ?? "尚未运行"} · 云端待处理：{cloudCount} · 隔离账本：{quarantineCount} 项";
        }
    }

    private void UpdatePendingQuarantineEntries(IEnumerable<RetentionQuarantineEntryDto>? entries)
    {
        var active = (entries ?? Enumerable.Empty<RetentionQuarantineEntryDto>())
            .Where(entry => entry.State != RetentionQuarantineState.Deleted)
            .OrderByDescending(entry => entry.UpdatedUtc)
            .ToList();
        Replace(PendingQuarantineEntries, active, AreSameQuarantineEntry);
        RebuildMaintenanceActionItems();
    }

    private void RebuildMaintenanceActionItems()
    {
        var items = new List<MaintenanceActionItem>();
        var inspection = Snapshot.HealthInspection ?? new HealthInspectionStateDto();
        items.Add(new MaintenanceActionItem
        {
            ItemId = "health-inspection",
            CategoryDisplay = "恢复巡检",
            Title = "恢复可用性巡检",
            StatusDisplay = inspection.LastStatusDisplay,
            Detail = string.IsNullOrWhiteSpace(inspection.LastSummary) ? "尚未记录巡检摘要。" : inspection.LastSummary,
            LastVerifiedDisplay = inspection.LastSuccessfulLocalDisplay,
            NextAttemptDisplay = inspection.NextDueLocalDisplay,
            ActionText = inspection.IsRunning ? "查看巡检状态" : "立即巡检",
            ActionToolTip = "运行现有的非破坏性恢复可用性巡检，不覆盖真实存档。",
            ActionKind = MaintenanceActionKind.HealthInspection
        });

        var cloudMerge = MaintenanceCloudTransferResolver.Resolve(
            Snapshot.CloudTransfers?.Items,
            CloudTransferItems);
        var knownCloudKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cloudItems = cloudMerge.Items
            .Where(MaintenanceCloudTransferResolver.IsAttention)
            .OrderBy(item => item.NextAttemptUtc ?? DateTime.MaxValue)
            .ThenBy(item => item.GameName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var transfer in cloudItems)
        {
            if (!knownCloudKeys.Add(transfer.TransferKey))
                continue;

            var next = transfer.NextAttemptLocal?.ToString("yyyy-MM-dd HH:mm")
                ?? (transfer.State is "AuthenticationRequired" or "Failed" or "CheckFailed" ? "需处理后再试" : "手动确认");
            items.Add(new MaintenanceActionItem
            {
                ItemId = "cloud:" + transfer.TransferKey,
                CategoryDisplay = "云端队列",
                Title = $"{transfer.KindDisplay} · {DisplayGameName(transfer.PlayniteId, transfer.GameName)}",
                StatusDisplay = transfer.StateDisplay,
                Detail = string.IsNullOrWhiteSpace(transfer.LastError)
                    ? transfer.GuaranteeLevelDisplay
                    : $"{transfer.GuaranteeLevelDisplay} · {transfer.LastError}",
                LastAttemptDisplay = transfer.LastAttemptUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "尚未尝试",
                NextAttemptDisplay = next,
                ActionText = "打开云队列",
                ActionToolTip = "打开这条真实云端传输记录；重试或远端 check 仍沿用原有状态与确认边界。",
                ActionKind = MaintenanceActionKind.CloudTransfer,
                TransferKey = transfer.TransferKey,
                TransferKind = transfer.Kind,
                TransferState = transfer.State
            });
        }

        var totalCloudAttention = cloudMerge.GetEffectiveAttentionCount(Snapshot.CloudTransfers?.AttentionCount ?? 0);
        if (totalCloudAttention > knownCloudKeys.Count)
        {
            items.Add(new MaintenanceActionItem
            {
                ItemId = "cloud:remaining",
                CategoryDisplay = "云端队列",
                Title = $"还有 {totalCloudAttention - knownCloudKeys.Count} 项云端记录待处理",
                StatusDisplay = "未全部加载",
                Detail = "维护页只列当前已加载的真实记录；打开云队列可继续分页查看，不会把加载窗口当成全集。",
                LastAttemptDisplay = "摘要未列出明细",
                NextAttemptDisplay = Snapshot.CloudTransfers?.NextAttemptLocal?.ToString("yyyy-MM-dd HH:mm") ?? "按队列状态",
                ActionText = "查看全部队列",
                ActionToolTip = "打开云端队列并保留服务端分页、筛选和一致性校验。",
                ActionKind = MaintenanceActionKind.CloudTransfer
            });
        }

        foreach (var entry in PendingQuarantineEntries)
        {
            var fileName = System.IO.Path.GetFileName(entry.OriginalPath);
            var location = string.IsNullOrWhiteSpace(fileName) ? entry.BackupId : fileName;
            var detail = string.IsNullOrWhiteSpace(entry.LastError)
                ? "隔离账本仍未完成协调，文件不会被猜测、覆盖或自动替换。"
                : entry.LastError;
            items.Add(new MaintenanceActionItem
            {
                ItemId = "quarantine:" + entry.EntryId,
                CategoryDisplay = "清理隔离账本",
                Title = $"待恢复文件 · {location}",
                StatusDisplay = GetQuarantineStateDisplay(entry.State),
                Detail = $"游戏 {DisplayGameName(entry.PlayniteId, entry.PlayniteId)} · {detail}",
                LedgerUpdatedDisplay = entry.UpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                NextAttemptDisplay = entry.State == RetentionQuarantineState.RecoveryRequired ? "需人工确认" : "Worker 下次启动时协调",
                ActionText = "再次协调",
                ActionToolTip = "只针对这条已持久化账本执行安全检查；遇到路径或文件身份冲突会保留并标记人工处理。",
                ActionKind = MaintenanceActionKind.RetentionQuarantine,
                EntryId = entry.EntryId
            });
        }

        var ordered = items
            .OrderBy(item => item.Group)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
        Replace(MaintenanceActionItems, ordered, AreSameMaintenanceAction);
        MaintenanceActionSections = new[]
        {
            new MaintenanceActionSection(
                MaintenanceActionGroup.NeedsManualHandling,
                "需要人工处理",
                "冲突、失败或隔离账本不会被自动覆盖；逐条确认后再继续。",
                ordered.Where(item => item.Group == MaintenanceActionGroup.NeedsManualHandling)),
            new MaintenanceActionSection(
                MaintenanceActionGroup.WaitingForRetry,
                "等待自动重试",
                "这些记录已有下一次尝试时间，不需要重复点击上传。",
                ordered.Where(item => item.Group == MaintenanceActionGroup.WaitingForRetry)),
            new MaintenanceActionSection(
                MaintenanceActionGroup.Routine,
                "例行巡检",
                "按需运行非破坏性检查，结果会回到同一维护上下文。",
                ordered.Where(item => item.Group == MaintenanceActionGroup.Routine))
        }.Where(section => section.Items.Count > 0).ToArray();
        OnPropertyChanged(nameof(MaintenanceActionSummary));
    }

    private async Task RunMaintenanceActionAsync(object? value)
    {
        if (value is not MaintenanceActionItem item)
            return;

        switch (item.ActionKind)
        {
            case MaintenanceActionKind.HealthInspection:
                if (Snapshot.HealthInspection?.IsRunning == true)
                {
                    await RefreshDashboardAsync(false, false);
                    return;
                }
                await RunHealthInspectionAsync();
                return;
            case MaintenanceActionKind.CloudTransfer:
                OpenCloudQueue(item.TransferKey, item.TransferState, item.TransferKind);
                return;
            case MaintenanceActionKind.RetentionQuarantine:
                var confirmed = await plugin.ConfirmAsync(
                    "再次协调隔离账本",
                    $"将只检查并协调“{item.Title}”对应的已持久化账本。遇到路径不安全、原路径冲突或文件身份不一致时会停止并保留文件。是否继续？",
                    "继续协调",
                    "取消",
                    isDangerous: true);
                if (!confirmed)
                    return;

                var result = await plugin.RequestAsync<RetentionQuarantineRecoverySummaryDto>(
                    MessageTypes.RecoverRetentionQuarantine,
                    new RetentionQuarantineRecoveryRequestDto { EntryId = item.EntryId, Confirmed = true },
                    TimeSpan.FromMinutes(3));
                StatusMessage = $"隔离账本协调完成：恢复 {result.RestoredCount} 项，清理 {result.DeletedCount} 项，仍需人工确认 {result.RecoveryRequiredCount} 项。";
                await LoadDiagnosticsAsync();
                return;
        }
    }

    private string DisplayGameName(string playniteId, string fallback)
        => Games.FirstOrDefault(game => string.Equals(game.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase))?.Name
            ?? (string.IsNullOrWhiteSpace(fallback) ? "全局" : fallback);

    private static string GetQuarantineStateDisplay(RetentionQuarantineState state)
        => state switch
        {
            RetentionQuarantineState.Planned => "已记账，待移动确认",
            RetentionQuarantineState.Moved => "已移入隔离区，待恢复或继续处理",
            RetentionQuarantineState.IndexRemoved => "索引已移除，待确认隔离文件",
            RetentionQuarantineState.RecoveryRequired => "需要人工确认",
            _ => state.ToString()
        };

    private static bool AreSameQuarantineEntry(RetentionQuarantineEntryDto left, RetentionQuarantineEntryDto right)
        => string.Equals(left.EntryId, right.EntryId, StringComparison.OrdinalIgnoreCase)
           && left.State == right.State
           && left.UpdatedUtc == right.UpdatedUtc
           && left.LastError == right.LastError;

    private static bool AreSameMaintenanceAction(MaintenanceActionItem left, MaintenanceActionItem right)
        => left.ItemId == right.ItemId
           && left.StatusDisplay == right.StatusDisplay
           && left.Detail == right.Detail
           && left.TimingDisplay == right.TimingDisplay;
}
