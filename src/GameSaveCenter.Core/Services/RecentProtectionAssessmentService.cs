using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>Classifies recently played games that do not currently have a clear protection path.</summary>
/// <remarks>
/// This service consumes the dashboard snapshot only. It performs no matching, backup, restore,
/// filesystem, database, cloud or network work, so filtering the overview cannot change backup state.
/// </remarks>
public sealed class RecentProtectionAssessmentService
{
    private static readonly int[] SupportedWindows = { 7, 30, 90 };

    public RecentProtectionSummary Assess(IEnumerable<GameStatusDto> games, int windowDays, DateTime nowUtc)
    {
        if (games == null) throw new ArgumentNullException(nameof(games));

        var normalizedWindow = NormalizeWindowDays(windowDays);
        var currentUtc = nowUtc.ToUniversalTime();
        var recentGames = games
            .Where(game => IsRecentlyPlayed(game, currentUtc, normalizedWindow))
            .ToList();
        var attentionItems = new List<RecentProtectionItem>();

        foreach (var game in recentGames)
        {
            var issue = Classify(game);
            if (issue != null) attentionItems.Add(issue);
        }

        var orderedAttentionItems = attentionItems
            .OrderBy(item => item.Priority)
            .ThenByDescending(item => item.LastPlayedUtc)
            .ThenBy(item => item.GameName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var orderedItems = recentGames
            .Select(game => Classify(game) ?? CreateProtected(game))
            .OrderBy(item => item.IsProtected ? 1000 : item.Priority)
            .ThenByDescending(item => item.LastPlayedUtc)
            .ThenBy(item => item.GameName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RecentProtectionSummary(
            normalizedWindow,
            recentGames.Count,
            recentGames.Count - orderedAttentionItems.Count,
            orderedAttentionItems.Count,
            orderedAttentionItems.Count(item => item.IssueKind == RecentProtectionIssueKind.UnrecognizedSave),
            orderedItems,
            orderedAttentionItems);
    }

    public static bool IsSupportedWindowDays(int value)
        => SupportedWindows.Contains(value);

    public static int NormalizeWindowDays(int value)
        => IsSupportedWindowDays(value) ? value : 30;

    private static bool IsRecentlyPlayed(GameStatusDto game, DateTime nowUtc, int windowDays)
    {
        if (game == null || !game.LastPlayedUtc.HasValue) return false;
        var lastPlayedUtc = game.LastPlayedUtc.Value.ToUniversalTime();
        return lastPlayedUtc <= nowUtc && nowUtc - lastPlayedUtc <= TimeSpan.FromDays(windowDays);
    }

    private static RecentProtectionItem? Classify(GameStatusDto game)
    {
        if (!game.LudusaviMatched)
        {
            return Create(game, RecentProtectionIssueKind.UnrecognizedSave, 10,
                "未识别存档", "最近游玩过，但尚未识别到可用的存档规则。保护动作不会因查看此项而自动执行。");
        }

        var hasBackup = game.BackupVersionCount > 0 || game.LastBackupUtc.HasValue;
        if (!hasBackup)
        {
            return Create(game, RecentProtectionIssueKind.NeverBackedUp, 20,
                "有存档但从未备份", "已识别到存档规则，但目前没有本地备份版本。请在存档中心确认后手动备份。");
        }

        if (game.LatestRestoreReadinessStatus.HasValue
            && (game.LatestRestoreReadinessStatus.Value == RestoreReadinessStatus.Corrupted
                || game.LatestRestoreReadinessStatus.Value == RestoreReadinessStatus.Failed
                || game.LatestRestoreReadinessStatus.Value == RestoreReadinessStatus.Unsupported))
        {
            return Create(game, RecentProtectionIssueKind.RestorePointUnavailable, 30,
                "最新版本不可恢复", BuildReadinessDetail(game.LatestRestoreReadinessStatus.Value));
        }

        var policy = game.Policy ?? new BackupPolicyDto();
        if (!policy.Enabled || (!policy.BackupOnGameStop && !policy.BackupDuringPlay))
        {
            return Create(game, RecentProtectionIssueKind.AutomaticProtectionDisabled, 40,
                "自动保护未开启", "已有备份，但当前游戏策略未启用游戏结束或游玩中自动保护。");
        }

        if (policy.UploadAfterBackup && IsCloudFailure(game.CloudState))
        {
            return Create(game, RecentProtectionIssueKind.CloudFailure, 50,
                "云同步异常", "本地备份不受影响，但最近云端复制失败或正在重试，请在任务中心查看详情。");
        }

        if (!game.LastBackupUtc.HasValue || game.LastBackupUtc.Value.ToUniversalTime() < game.LastPlayedUtc!.Value.ToUniversalTime())
        {
            return Create(game, RecentProtectionIssueKind.BackupOutdated, 60,
                "最近备份过旧", "最近一次游玩发生在最新备份之后，当前游玩内容可能尚未得到保护。");
        }

        if (!game.LatestRestoreReadinessStatus.HasValue
            || game.LatestRestoreReadinessStatus.Value == RestoreReadinessStatus.Warning
            || game.LatestRestoreReadinessStatus.Value == RestoreReadinessStatus.Checking)
        {
            var detail = game.LatestRestoreReadinessStatus == RestoreReadinessStatus.Warning
                ? "最新备份检查存在警告，请在存档中心查看具体原因。"
                : game.LatestRestoreReadinessStatus == RestoreReadinessStatus.Checking
                    ? "最新备份仍在检查中，完成前不会计入已保护。"
                    : "最新备份还没有完成恢复可用性验证，请在存档中心检查该版本。";
            return Create(game, RecentProtectionIssueKind.RestorePointUnavailable, 65,
                "最新版本尚未确认可恢复", detail);
        }

        if (string.Equals(game.HealthState, GameHealthState.Risk.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(game.HealthState, GameHealthState.Attention.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var detail = string.IsNullOrWhiteSpace(game.HealthReasonDisplay)
                ? "健康检查发现需要处理的备份问题。"
                : game.HealthReasonDisplay;
            return Create(game, RecentProtectionIssueKind.BackupHealthRisk, 70,
                "备份状态异常", detail);
        }

        return null;
    }

    private static RecentProtectionItem Create(GameStatusDto game, RecentProtectionIssueKind kind, int priority, string title, string detail)
        => new RecentProtectionItem
        {
            PlayniteId = game.PlayniteId ?? string.Empty,
            GameName = string.IsNullOrWhiteSpace(game.Name) ? "未命名游戏" : game.Name,
            LastPlayedUtc = game.LastPlayedUtc!.Value.ToUniversalTime(),
            IssueKind = kind,
            Priority = priority,
            Title = title,
            Detail = detail
        };

    private static RecentProtectionItem CreateProtected(GameStatusDto game)
        => Create(game, RecentProtectionIssueKind.Protected, 1000,
            "已保护", "最近备份已完成恢复可用性检查，且自动保护策略已开启。");

    private static bool IsCloudFailure(string? state)
        => string.Equals(state, "Failed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(state, "RetryScheduled", StringComparison.OrdinalIgnoreCase);

    private static string BuildReadinessDetail(RestoreReadinessStatus status)
        => status switch
        {
            RestoreReadinessStatus.Corrupted => "最新备份归档疑似损坏，当前不能把它视为可靠恢复点。",
            RestoreReadinessStatus.Failed => "最新备份的恢复可用性检查失败，请在存档中心重新检查。",
            RestoreReadinessStatus.Unsupported => "当前格式不受恢复检查器支持，暂不能确认该版本可恢复。",
            RestoreReadinessStatus.Warning => "最新备份检查存在警告，请在存档中心查看具体原因。",
            RestoreReadinessStatus.Checking => "最新备份仍在检查中，完成前不会计入已保护。",
            _ => "最新备份尚未确认可恢复。"
        };
}

public enum RecentProtectionIssueKind
{
    Protected,
    UnrecognizedSave,
    NeverBackedUp,
    RestorePointUnavailable,
    AutomaticProtectionDisabled,
    CloudFailure,
    BackupOutdated,
    BackupHealthRisk
}

public sealed class RecentProtectionItem
{
    public string PlayniteId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public DateTime LastPlayedUtc { get; set; }
    public RecentProtectionIssueKind IssueKind { get; set; }
    public int Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public bool IsProtected => IssueKind == RecentProtectionIssueKind.Protected;
    public bool IsSelectable => !IsProtected;
    public string StatusDisplay => IssueKind switch
    {
        RecentProtectionIssueKind.Protected => "已保护",
        RecentProtectionIssueKind.UnrecognizedSave => "未匹配",
        RecentProtectionIssueKind.NeverBackedUp => "存档未保护",
        _ => "风险"
    };

    public string IssueKindDisplay => IssueKind switch
    {
        RecentProtectionIssueKind.Protected => "已保护",
        RecentProtectionIssueKind.UnrecognizedSave => "未识别存档",
        RecentProtectionIssueKind.NeverBackedUp => "从未备份",
        RecentProtectionIssueKind.RestorePointUnavailable => "恢复点不可用",
        RecentProtectionIssueKind.AutomaticProtectionDisabled => "自动保护关闭",
        RecentProtectionIssueKind.CloudFailure => "云同步异常",
        RecentProtectionIssueKind.BackupOutdated => "备份过旧",
        RecentProtectionIssueKind.BackupHealthRisk => "备份异常",
        _ => "需要处理"
    };
}

public sealed class RecentProtectionSummary
{
    public RecentProtectionSummary(
        int windowDays,
        int recentlyPlayedGames,
        int protectedGames,
        int attentionGames,
        int unrecognizedSaveGames,
        IReadOnlyList<RecentProtectionItem> items)
        : this(windowDays, recentlyPlayedGames, protectedGames, attentionGames, unrecognizedSaveGames, items, items)
    {
    }

    public RecentProtectionSummary(
        int windowDays,
        int recentlyPlayedGames,
        int protectedGames,
        int attentionGames,
        int unrecognizedSaveGames,
        IReadOnlyList<RecentProtectionItem> items,
        IReadOnlyList<RecentProtectionItem> attentionItems)
    {
        WindowDays = windowDays;
        RecentlyPlayedGames = recentlyPlayedGames;
        ProtectedGames = protectedGames;
        AttentionGames = attentionGames;
        UnrecognizedSaveGames = unrecognizedSaveGames;
        var ordered = items ?? Array.Empty<RecentProtectionItem>();
        Items = ordered.Take(MaxVisibleItems).ToList();
        AttentionItems = (attentionItems ?? Array.Empty<RecentProtectionItem>()).Take(MaxVisibleItems).ToList();
        HiddenAttentionGames = Math.Max(0, attentionGames - AttentionItems.Count);
    }

    public int WindowDays { get; }
    public int RecentlyPlayedGames { get; }
    public int ProtectedGames { get; }
    public int AttentionGames { get; }
    public int UnrecognizedSaveGames { get; }
    public IReadOnlyList<RecentProtectionItem> Items { get; }
    public IReadOnlyList<RecentProtectionItem> AttentionItems { get; }
    public int HiddenAttentionGames { get; }
    public bool HasMoreItems => HiddenAttentionGames > 0;
    public string WindowLabel => $"最近 {WindowDays} 天";
    public bool HasAttention => AttentionGames > 0;

    private const int MaxVisibleItems = 6;
}
