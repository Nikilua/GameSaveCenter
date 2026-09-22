using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22TimeDisplayBehaviorTests
{
    [Fact]
    public void RelativeDisplayHasStableJustNowAndMinuteBoundaries()
    {
        var now = new DateTime(2026, 9, 21, 4, 0, 0, DateTimeKind.Utc);

        Assert.Equal("刚刚", TimeDisplayFormatter.Relative(now.AddSeconds(-59), now));
        Assert.Equal("1 分钟前", TimeDisplayFormatter.Relative(now.AddMinutes(-1), now));
        Assert.Equal("即将", TimeDisplayFormatter.Relative(now.AddSeconds(30), now));
    }

    [Fact]
    public void RelativeDisplayUsesLocalYesterdayBoundary()
    {
        var nowUtc = new DateTime(2026, 9, 21, 4, 0, 0, DateTimeKind.Utc);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZoneInfo.Local);
        var yesterdayLocal = DateTime.SpecifyKind(
            localNow.Date.AddDays(-1).AddHours(12),
            DateTimeKind.Unspecified);
        var yesterdayUtc = TimeZoneInfo.ConvertTimeToUtc(yesterdayLocal, TimeZoneInfo.Local);

        Assert.Equal($"昨天 {yesterdayLocal:HH:mm}", TimeDisplayFormatter.Relative(yesterdayUtc, nowUtc));
    }

    [Fact]
    public void FullDisplayKeepsTimezoneHintAndCopyableUtcValue()
    {
        var timestamp = new DateTime(2026, 9, 21, 4, 5, 6, 123, DateTimeKind.Utc).AddTicks(4567);

        var raw = TimeDisplayFormatter.RawUtc(timestamp);
        var full = TimeDisplayFormatter.Full(timestamp);

        Assert.Equal(timestamp.ToString("O"), raw);
        Assert.Contains("UTC", full, StringComparison.Ordinal);
        Assert.Contains(raw, full, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskAndActivityModelsExposeTheSameFullAndRawTimeContract()
    {
        var timestamp = new DateTime(2026, 9, 20, 12, 34, 56, DateTimeKind.Utc);
        var task = new TaskStatusDto
        {
            TaskId = "time-contract-task",
            TaskType = "Backup",
            State = TaskState.Succeeded,
            CreatedUtc = timestamp,
            StartedUtc = timestamp.AddSeconds(2)
        };
        var activity = new ActivityEntryDto { CreatedUtc = timestamp };

        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), task.CreatedRawUtcDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), task.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), activity.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), activity.CreatedRawUtcDisplay);
        Assert.NotEqual("时间未知", task.CreatedRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(task.StartedUtc.Value), task.StartedFullDisplay);

        task.StartedUtc = null;
        Assert.Equal("未开始", task.StartedRelativeDisplay);
        Assert.Equal("未开始", task.StartedFullDisplay);
    }

    [Fact]
    public void SaveAndMaintenanceTimeEntrypointsUseSharedDisplayBindings()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var saveDto = new BackupVersionDto { CreatedUtc = new DateTime(2026, 9, 20, 12, 34, 56, DateTimeKind.Utc) };
        var audit = new AuditLogEntryDto { CreatedUtc = saveDto.CreatedUtc };

        Assert.Equal(TimeDisplayFormatter.Full(saveDto.CreatedUtc), saveDto.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(saveDto.CreatedUtc), saveDto.CreatedRawUtcDisplay);
        Assert.Equal(saveDto.CreatedFullDisplay, audit.CreatedFullDisplay);
        Assert.Equal(saveDto.CreatedRawUtcDisplay, audit.CreatedRawUtcDisplay);
        Assert.Contains("Binding=\"{Binding CreatedRelativeDisplay, Mode=OneWay}\"", save, StringComparison.Ordinal);
        Assert.Contains("SelectedBackup.CreatedFullDisplay", save, StringComparison.Ordinal);
        Assert.Contains("Binding=\"{Binding CreatedRelativeDisplay, Mode=OneWay}\"", maintenance, StringComparison.Ordinal);
        Assert.Contains("CreatedFullDisplay, Mode=OneWay", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void RetentionPreviewKeepsLegacyTextAndExposesSharedTimeContract()
    {
        var timestamp = new DateTime(2026, 9, 19, 8, 7, 6, DateTimeKind.Utc);
        var item = new RetentionSimulationItemDto { CreatedUtc = timestamp };

        Assert.Equal(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), item.CreatedDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), item.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), item.CreatedRawUtcDisplay);
        Assert.NotEqual("时间未知", item.CreatedRelativeDisplay);
    }

    [Fact]
    public void MediaEntrypointsUseSharedCaptureTimeBindings()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var item = new MediaItemDto { CapturedUtc = timestamp };

        Assert.Equal(TimeDisplayFormatter.Full(timestamp), item.CapturedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), item.CapturedRawUtcDisplay);
        Assert.NotEqual("时间未知", item.CapturedRelativeDisplay);
        Assert.Contains("Binding=\"{Binding CapturedRelativeDisplay, Mode=OneWay}\"", media, StringComparison.Ordinal);
        Assert.Contains("SelectedMedia.CapturedFullDisplay", media, StringComparison.Ordinal);
        Assert.Contains("CapturedFullDisplay, Mode=OneWay", media, StringComparison.Ordinal);
    }

    [Fact]
    public void MediaClassificationTimeEntrypointsUseSharedContractAndKeepUnknownNegative()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var suggestion = new MediaClassificationSuggestionDto { CapturedUtc = timestamp };
        var preview = new MediaClassificationPreviewDto
        {
            CreatedUtc = timestamp,
            ExpiresUtc = timestamp.AddMinutes(30)
        };
        var batch = new MediaClassificationBatchSummaryDto
        {
            State = "Preview",
            UpdatedUtc = timestamp,
            ExpiresUtc = timestamp.AddMinutes(30)
        };

        Assert.Equal(TimeDisplayFormatter.Full(timestamp), suggestion.CapturedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), suggestion.CapturedRawUtcDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(preview.ExpiresUtc), preview.ExpiresFullDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(batch.UpdatedUtc), batch.UpdatedFullDisplay);
        Assert.Contains("有效至", batch.ExpiryRelativeDisplay, StringComparison.Ordinal);
        Assert.Equal("时间未知", new MediaClassificationSuggestionDto().CapturedRelativeDisplay);
        Assert.Contains("CapturedRelativeDisplay, Mode=OneWay", media, StringComparison.Ordinal);
        Assert.Contains("UpdatedRelativeDisplay, Mode=OneWay", media, StringComparison.Ordinal);
        Assert.Contains("ExpiryRelativeDisplay, Mode=OneWay", media, StringComparison.Ordinal);
        Assert.Contains("ExpiresFullDisplay", media, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidationFindingTimeEntrypointsKeepLegacyUnknownAndExposeFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var finding = new ValidationFindingDto { CreatedUtc = timestamp };
        var unknown = new ValidationFindingDto();

        Assert.Equal($"证据时间：{timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss}", finding.EvidenceTimeDisplay);
        Assert.Equal($"证据时间：{TimeDisplayFormatter.Full(timestamp)}", finding.EvidenceTimeFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), finding.EvidenceTimeRawUtcDisplay);
        Assert.NotEqual("证据时间未知", finding.EvidenceTimeRelativeDisplay);
        Assert.Equal("证据时间未知", unknown.EvidenceTimeRelativeDisplay);
        Assert.Equal(2, maintenance.Split(new[] { "SelectedFinding.EvidenceTimeRelativeDisplay" }, StringSplitOptions.None).Length - 1);
        Assert.Contains("SelectedFinding.EvidenceTimeFullDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedFinding.EvidenceTimeFullDisplay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void RestoreReadinessTimeEntrypointKeepsLegacyStaleAndUnknownSemantics()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var backup = new BackupVersionDto
        {
            CreatedUtc = timestamp,
            RestoreReadiness = new RestoreReadinessDto { CheckedUtc = timestamp }
        };
        var unknown = new BackupVersionDto();

        Assert.Equal($"检查于 {timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss}（结果较旧，建议重新验证）", backup.RestoreReadinessCheckedDisplay);
        Assert.Contains("结果较旧", backup.RestoreReadinessCheckedRelativeDisplay, StringComparison.Ordinal);
        Assert.Contains(TimeDisplayFormatter.Full(timestamp), backup.RestoreReadinessCheckedFullDisplay, StringComparison.Ordinal);
        Assert.Equal("尚未检查", unknown.RestoreReadinessCheckedRelativeDisplay);
        Assert.Equal("尚未检查", unknown.RestoreReadinessCheckedFullDisplay);
        Assert.Contains("SelectedBackup.RestoreReadinessCheckedRelativeDisplay", save, StringComparison.Ordinal);
        Assert.Contains("SelectedBackup.RestoreReadinessCheckedFullDisplay", save, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedBackup.RestoreReadinessCheckedFullDisplay, Mode=OneWay}\"", save, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalMirrorTimeEntrypointKeepsLegacyUnknownAndExposesFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var mirror = new LocalMirrorStatusDto { LastSyncUtc = timestamp };
        var unknown = new LocalMirrorStatusDto();

        Assert.Equal(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), mirror.LastSyncDisplay);
        Assert.NotEqual("尚未同步", mirror.LastSyncRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), mirror.LastSyncFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), mirror.LastSyncRawUtcDisplay);
        Assert.Equal("尚未同步", unknown.LastSyncRelativeDisplay);
        Assert.Equal("尚未同步", unknown.LastSyncFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.LastSyncRawUtcDisplay);
        Assert.Contains("LocalMirrorStatus.LastSyncRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("LocalMirrorStatus.LastSyncFullDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding LocalMirrorStatus.LastSyncFullDisplay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void RecentAccessTimeEntrypointKeepsLegacyTextAndUsesFullTooltip()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var item = new RecentAccessItem(
            new RecentAccessRecord
            {
                PlayniteId = "recent-time-game",
                Workspace = RecentAccessRecord.SavesWorkspace,
                LastAccessUtc = timestamp
            },
            "时间测试游戏");

        Assert.Equal(timestamp.ToLocalTime().ToString("MM-dd HH:mm"), item.LastAccessDisplay);
        Assert.NotEqual("时间未知", item.LastAccessRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), item.LastAccessFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), item.LastAccessRawUtcDisplay);
        Assert.Contains(item.LastAccessRelativeDisplay, item.SummaryDisplay, StringComparison.Ordinal);
        Assert.Contains(item.LastAccessFullDisplay, item.SummaryFullDisplay, StringComparison.Ordinal);
        var unknown = new RecentAccessItem(
            new RecentAccessRecord
            {
                PlayniteId = "recent-unknown-game",
                Workspace = RecentAccessRecord.SavesWorkspace,
                LastAccessUtc = DateTime.MinValue
            },
            "未知时间游戏");

        Assert.Equal("时间未知", unknown.LastAccessRelativeDisplay);
        Assert.Equal("时间未知", unknown.LastAccessFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.LastAccessRawUtcDisplay);
        Assert.Contains("Text=\"{Binding SummaryDisplay, Mode=OneWay}\"", overview, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"{Binding SummaryFullDisplay}\"", overview, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SummaryFullDisplay}\"", overview, StringComparison.Ordinal);
    }

    [Fact]
    public void HealthInspectionTimeEntrypointsKeepUnknownSemanticsAndExposeFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var timestamp = new DateTime(2026, 9, 18, 9, 8, 7, DateTimeKind.Utc);
        var state = new HealthInspectionStateDto
        {
            LastSuccessfulUtc = timestamp,
            LastCompletedUtc = timestamp.AddMinutes(2),
            NextDueUtc = timestamp.AddHours(1),
            IntervalMinutes = 30,
            MaxDurationSeconds = 120
        };
        var unknown = new HealthInspectionStateDto();

        Assert.Equal(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), state.LastSuccessfulLocalDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), state.LastSuccessfulFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), state.LastSuccessfulRawUtcDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(state.LastCompletedUtc!.Value), state.LastCompletedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(state.NextDueUtc!.Value), state.NextDueFullDisplay);
        Assert.Contains(state.NextDueRelativeDisplay, state.NextPlanRelativeDisplay, StringComparison.Ordinal);
        Assert.Contains(state.NextDueFullDisplay, state.NextPlanFullDisplay, StringComparison.Ordinal);
        Assert.Equal("尚未成功验证", unknown.LastSuccessfulRelativeDisplay);
        Assert.Equal("尚未结束一轮巡检", unknown.LastCompletedRelativeDisplay);
        Assert.Equal("待安排", unknown.NextDueRelativeDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.NextDueRawUtcDisplay);
        Assert.Contains("LastSuccessfulRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("LastCompletedRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("NextPlanRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding Snapshot.HealthInspection.NextPlanFullDisplay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void EnvironmentCheckTimeEntrypointKeepsUnknownSemanticsAndExposesFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var timestamp = new DateTime(2026, 9, 19, 11, 12, 13, DateTimeKind.Utc);
        var report = new EnvironmentCheckReportDto { CheckedUtc = timestamp };
        var unknown = new EnvironmentCheckReportDto();

        Assert.Equal(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), report.CheckedLocalDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), report.CheckedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), report.CheckedRawUtcDisplay);
        Assert.Equal("尚未检查", unknown.CheckedRelativeDisplay);
        Assert.Equal("尚未检查", unknown.CheckedFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.CheckedRawUtcDisplay);
        Assert.Contains("EnvironmentCheck.CheckedRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("EnvironmentCheck.CheckedFullDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding EnvironmentCheck.CheckedFullDisplay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void CloudTransferEvidenceEntrypointsKeepUnknownSemanticsAndExposeFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var attempted = new DateTime(2026, 9, 20, 7, 8, 9, DateTimeKind.Utc);
        var verified = attempted.AddMinutes(3);
        var transfer = new CloudTransferStatusDto
        {
            LastAttemptUtc = attempted,
            LastSuccessfulVerificationUtc = verified
        };
        var unknown = new CloudTransferStatusDto();

        Assert.Equal(attempted.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), transfer.LastAttemptDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(attempted), transfer.LastAttemptFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(attempted), transfer.LastAttemptRawUtcDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(verified), transfer.LastSuccessfulVerificationFullDisplay);
        Assert.Equal("未知", unknown.LastAttemptRelativeDisplay);
        Assert.Equal("未知", unknown.LastSuccessfulVerificationFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.LastSuccessfulVerificationRawUtcDisplay);
        Assert.Contains("SelectedCloudTransfer.LastAttemptRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.LastSuccessfulVerificationRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedCloudTransfer.LastSuccessfulVerificationFullDisplay, TargetNullValue=未知}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void CloudTransferRetryTimingKeepsImmediateAndUnknownStatesWithFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var scheduled = new CloudTransferStatusDto { NextAttemptUtc = DateTime.UtcNow.AddHours(1) };
        var immediate = new CloudTransferStatusDto { NextAttemptUtc = DateTime.UtcNow.AddMinutes(-1) };
        var unknown = new CloudTransferStatusDto();

        Assert.Contains("后", scheduled.RetryTimingRelativeDisplay, StringComparison.Ordinal);
        Assert.Equal("可立即重试", immediate.RetryTimingRelativeDisplay);
        Assert.Equal("无自动重试", unknown.RetryTimingRelativeDisplay);
        Assert.Contains(TimeDisplayFormatter.Full(scheduled.NextAttemptUtc!.Value), scheduled.RetryTimingFullDisplay, StringComparison.Ordinal);
        Assert.Equal("未记录 UTC 时间", unknown.RetryTimingRawUtcDisplay);
        Assert.Contains("SelectedCloudTransfer.RetryTimingRelativeDisplay", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedCloudTransfer.RetryTimingFullDisplay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void StorageRankKeepsLegacyDateAndExposesRelativeFullAndRawEvidence()
    {
        var timestamp = DateTime.UtcNow.AddMinutes(-2);
        var rank = new StorageGameRankDto { LatestBackupUtc = timestamp };
        var unknown = new StorageGameRankDto();

        Assert.Equal(timestamp.ToLocalTime().ToString("MM-dd HH:mm"), rank.LatestBackupDisplay);
        Assert.Contains("前", rank.LatestBackupRelativeDisplay, StringComparison.Ordinal);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), rank.LatestBackupFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), rank.LatestBackupRawUtcDisplay);
        Assert.Equal("时间未知", unknown.LatestBackupRelativeDisplay);
        Assert.Equal("时间未知", unknown.LatestBackupFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.LatestBackupRawUtcDisplay);
    }

    [Fact]
    public void OverviewSelectedGameBackupKeepsLegacyProjectionAndExposesFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var timestamp = DateTime.UtcNow.AddHours(-3);
        var game = new GameStatusDto { LastBackupUtc = timestamp };
        var unknown = new GameStatusDto();

        Assert.Equal(timestamp.ToLocalTime().ToString("MM-dd HH:mm"), game.LastBackupLocal?.ToString("MM-dd HH:mm"));
        Assert.NotEqual("时间未知", game.LastBackupRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), game.LastBackupFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), game.LastBackupRawUtcDisplay);
        Assert.Equal("时间未知", unknown.LastBackupRelativeDisplay);
        Assert.Equal("时间未知", unknown.LastBackupFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.LastBackupRawUtcDisplay);
        Assert.Contains("SelectedGameLastBackupRelativeDisplay, Mode=OneWay", overview, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"{Binding SelectedGameLastBackupFullDisplay, Mode=OneWay}\"", overview, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedGameLastBackupFullDisplay, Mode=OneWay}\"", overview, StringComparison.Ordinal);
    }

    [Fact]
    public void RestoreWorkflowSelectionUsesRelativeTimeAndFullEvidence()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var timestamp = DateTime.UtcNow.AddDays(-2);
        var backup = new BackupVersionDto
        {
            BackupId = "restore-time-contract",
            CreatedUtc = timestamp
        };

        var selection = RestoreWorkflowProgress.Build(backup, false, false, false, null, string.Empty, string.Empty)
            .Single(step => step.Key == "selection");

        Assert.Contains(backup.CreatedRelativeDisplay, selection.Detail, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedFullDisplay, selection.DetailFullDisplay, StringComparison.Ordinal);
        Assert.Contains(TimeDisplayFormatter.RawUtc(timestamp), selection.DetailFullDisplay, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding Detail}\"", save, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"{Binding DetailFullDisplay, Mode=OneWay}\"", save, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding DetailFullDisplay, Mode=OneWay}\"", save, StringComparison.Ordinal);

        var pending = RestoreWorkflowProgress.Build(null, false, false, false, null, string.Empty, string.Empty)
            .Single(step => step.Key == "selection");
        Assert.Equal("请先从历史列表选择一个稳定 ID 的版本。", pending.Detail);
        Assert.Equal(pending.Detail, pending.DetailFullDisplay);
    }

    [Fact]
    public void ComparisonEntryKeepsLegacyLabelAndExposesRelativeFullAndRawEvidence()
    {
        var timestamp = DateTime.UtcNow.AddDays(-2);
        var backup = new BackupVersionDto
        {
            BackupId = "comparison-time-contract",
            CreatedUtc = timestamp
        };
        var unknown = new BackupVersionDto { BackupId = "comparison-unknown" };

        Assert.Equal($"{timestamp.ToLocalTime():yyyy-MM-dd HH:mm} · 普通备份 · comparison-time-contract", backup.ComparisonDisplay);
        Assert.Contains("前", backup.ComparisonRelativeDisplay, StringComparison.Ordinal);
        Assert.Contains(TimeDisplayFormatter.Full(timestamp), backup.ComparisonFullDisplay, StringComparison.Ordinal);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), backup.ComparisonRawUtcDisplay);
        Assert.Equal("时间未知 · 普通备份 · comparison-unknown", unknown.ComparisonRelativeDisplay);
        Assert.Equal("时间未知 · 普通备份 · comparison-unknown", unknown.ComparisonFullDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.ComparisonRawUtcDisplay);
    }

    [Fact]
    public void ComparisonSummariesUseRelativeTextAndExposeFullEvidence()
    {
        var left = new BackupVersionDto
        {
            BackupId = "summary-left",
            CreatedUtc = DateTime.UtcNow.AddDays(-3)
        };
        var right = new BackupVersionDto
        {
            BackupId = "summary-right",
            CreatedUtc = DateTime.UtcNow.AddHours(-2)
        };

        var selection = DashboardViewModel.BuildComparisonSelectionSummary(left, right);
        var selectionFull = DashboardViewModel.BuildComparisonSelectionSummaryFull(left, right);
        var result = DashboardViewModel.BuildComparisonSummary(left, right);
        var resultFull = DashboardViewModel.BuildComparisonSummaryFull(left, right);

        Assert.Contains(left.ComparisonRelativeDisplay, selection, StringComparison.Ordinal);
        Assert.Contains(right.ComparisonRelativeDisplay, selection, StringComparison.Ordinal);
        Assert.Contains(left.ComparisonFullDisplay, selectionFull, StringComparison.Ordinal);
        Assert.Contains(right.ComparisonFullDisplay, selectionFull, StringComparison.Ordinal);
        Assert.Contains(left.CreatedRelativeDisplay, result, StringComparison.Ordinal);
        Assert.Contains(right.CreatedRelativeDisplay, result, StringComparison.Ordinal);
        Assert.Contains(left.CreatedFullDisplay, resultFull, StringComparison.Ordinal);
        Assert.Contains(right.CreatedFullDisplay, resultFull, StringComparison.Ordinal);
        Assert.Contains("新增属于 B", selection, StringComparison.Ordinal);
        Assert.Contains("删除属于 A", selectionFull, StringComparison.Ordinal);
        Assert.Equal("A、B 当前是同一版本；请选择不同版本，不会发起比较或恢复。", DashboardViewModel.BuildComparisonSelectionSummary(left, left));
        Assert.Equal("尚未选择可比较的版本。", DashboardViewModel.BuildComparisonSummaryFull(null, right));
    }

    [Fact]
    public void TaskPageUpdatedTimeUsesSharedRelativeFullAndRawContract()
    {
        var timestamp = new DateTime(2026, 9, 18, 12, 34, 56, DateTimeKind.Utc);

        Assert.NotEqual("未知", DashboardViewModel.FormatTaskPageLastUpdatedRelative(timestamp));
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), DashboardViewModel.FormatTaskPageLastUpdatedFull(timestamp));
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), DashboardViewModel.FormatTaskPageLastUpdatedRawUtc(timestamp));
        Assert.Equal("未知", DashboardViewModel.FormatTaskPageLastUpdatedRelative(null));
        Assert.Equal("未知", DashboardViewModel.FormatTaskPageLastUpdatedFull(null));
        Assert.Equal("未记录 UTC 时间", DashboardViewModel.FormatTaskPageLastUpdatedRawUtc(null));
    }

    [Fact]
    public void GameDiscoveryDiagnosticUsesRelativeSummaryAndFullEvidence()
    {
        var now = new DateTime(2026, 9, 22, 8, 0, 0, DateTimeKind.Utc);
        var timestamp = now.AddMinutes(-3);
        var diagnostic = new GameDiscoveryDiagnosticDto
        {
            Name = "合成测试游戏",
            PlayniteExists = true,
            WorkerRecordExists = true,
            DescriptorSyncedUtc = timestamp,
            LastMatchAttemptUtc = timestamp.AddMinutes(1),
            LastBackupUtc = timestamp.AddMinutes(2),
            MatchState = "Matched",
            LudusaviName = "合成测试游戏",
            BackupVersionCount = 2
        };

        var summary = DashboardViewModel.FormatGameDiscoveryDiagnostic(diagnostic, now);
        var full = DashboardViewModel.FormatGameDiscoveryDiagnostic(diagnostic, now, useFullTime: true);

        Assert.Contains("Worker 描述同步：3 分钟前", summary, StringComparison.Ordinal);
        Assert.Contains("最后尝试：2 分钟前", summary, StringComparison.Ordinal);
        Assert.Contains("最近备份：1 分钟前", summary, StringComparison.Ordinal);
        Assert.Contains(TimeDisplayFormatter.Full(timestamp), full, StringComparison.Ordinal);
        Assert.Contains(TimeDisplayFormatter.RawUtc(timestamp), full, StringComparison.Ordinal);
    }

    [Fact]
    public void GameDiscoveryDiagnosticUnknownTimesRemainUnknownAndExposeFullBinding()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var diagnostic = new GameDiscoveryDiagnosticDto { Name = "未知时间游戏" };

        var summary = DashboardViewModel.FormatGameDiscoveryDiagnostic(
            diagnostic,
            new DateTime(2026, 9, 22, 8, 0, 0, DateTimeKind.Utc));
        var full = DashboardViewModel.FormatGameDiscoveryDiagnostic(
            diagnostic,
            new DateTime(2026, 9, 22, 8, 0, 0, DateTimeKind.Utc),
            useFullTime: true);

        Assert.Contains("Worker 描述同步：未知", summary, StringComparison.Ordinal);
        Assert.Contains("最后尝试：未知", summary, StringComparison.Ordinal);
        Assert.Contains("最近备份：未知", summary, StringComparison.Ordinal);
        Assert.Contains("Worker 描述同步：未知", full, StringComparison.Ordinal);
        Assert.Contains("最后尝试：未知", full, StringComparison.Ordinal);
        Assert.Contains("最近备份：未知", full, StringComparison.Ordinal);
        Assert.DoesNotContain("1970", full, StringComparison.Ordinal);
        Assert.Contains("GameDiscoveryDiagnosticFullSummary", maintenance, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"{Binding GameDiscoveryDiagnosticFullSummary, Mode=OneWay}\"", maintenance, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding GameDiscoveryDiagnosticFullSummary, Mode=OneWay}\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void TimelineOrderingRemainsUtcBasedWhileRelativeTextIsComputedSeparately()
    {
        var created = new DateTime(2026, 9, 20, 1, 0, 0, DateTimeKind.Utc);
        var started = created.AddMinutes(1);
        var task = new TaskStatusDto
        {
            TaskId = "relative-order",
            TaskType = "Backup",
            State = TaskState.Running,
            CreatedUtc = created,
            StartedUtc = started
        };

        var timeline = TaskTimelineBuilder.Build(task, new[]
        {
            new TaskChangeEventDto
            {
                Sequence = 2,
                OccurredUtc = started,
                Task = new TaskStatusDto
                {
                    TaskId = task.TaskId,
                    State = TaskState.Running,
                    StartedUtc = started,
                    CreatedUtc = created
                }
            },
            new TaskChangeEventDto
            {
                Sequence = 1,
                OccurredUtc = created.AddSeconds(10),
                Task = new TaskStatusDto
                {
                    TaskId = task.TaskId,
                    State = TaskState.Queued,
                    CreatedUtc = created
                }
            }
        });

        Assert.Equal(new[] { "Created", "Started" }, timeline.Select(entry => entry.Kind).ToArray());
        Assert.All(timeline.Where(entry => entry.HasKnownTime), entry =>
        {
            Assert.NotEqual("时间未知", entry.RelativeTimeDisplay);
            Assert.Contains(entry.RawUtcTimeDisplay, entry.FullTimeDisplay, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void UnknownTimelineTimeRemainsExplicit()
    {
        var task = new TaskStatusDto
        {
            TaskId = "unknown-relative",
            TaskType = "Restore",
            State = TaskState.Running,
            CreatedUtc = new DateTime(2026, 9, 20, 2, 0, 0, DateTimeKind.Utc)
        };

        var unknown = TaskTimelineBuilder.Build(task, new[]
        {
            new TaskChangeEventDto
            {
                Sequence = 1,
                OccurredUtc = DateTime.MinValue,
                Task = task
            }
        }).Single(entry => entry.Kind == "Started");

        Assert.False(unknown.HasKnownTime);
        Assert.Equal("时间未知", unknown.RelativeTimeDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.RawUtcTimeDisplay);
    }
}
