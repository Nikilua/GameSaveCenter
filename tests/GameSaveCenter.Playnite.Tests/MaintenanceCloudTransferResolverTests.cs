using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MaintenanceCloudTransferResolverTests
    {
        [Fact]
        public void NewerLoadedSuccessReplacesSnapshotFailureBeforeAttentionFiltering()
        {
            var snapshot = Transfer("cloud:a", "Failed", 10);
            var loaded = Transfer("cloud:a", "Uploaded", 20);

            var result = MaintenanceCloudTransferResolver.Resolve(new[] { snapshot }, new[] { loaded });

            Assert.Single(result.Items);
            Assert.Equal("Uploaded", result.Items[0].State);
            Assert.Equal(1, result.SupersededSnapshotAttentionCount);
            Assert.Equal(0, result.GetEffectiveAttentionCount(1));
        }

        [Fact]
        public void OlderLoadedFailureDoesNotReplaceNewerSnapshotSuccess()
        {
            var snapshot = Transfer("cloud:a", "RemoteVerified", 20);
            var loaded = Transfer("cloud:a", "Failed", 10);

            var result = MaintenanceCloudTransferResolver.Resolve(new[] { snapshot }, new[] { loaded });

            Assert.Single(result.Items);
            Assert.Equal("RemoteVerified", result.Items[0].State);
            Assert.Equal(0, result.GetEffectiveAttentionCount(0));
        }

        [Fact]
        public void LoadedDetailWinsWhenSourcesHaveTheSameTimestamp()
        {
            var snapshot = Transfer("cloud:a", "Failed", 20);
            var loaded = Transfer("cloud:a", "Uploaded", 20);

            var result = MaintenanceCloudTransferResolver.Resolve(new[] { snapshot }, new[] { loaded });

            Assert.Equal("Uploaded", Assert.Single(result.Items).State);
        }

        [Fact]
        public void EffectiveAttentionCountAccountsForResolvedAndNewKeys()
        {
            var snapshot = Transfer("cloud:a", "Failed", 10);
            var loaded = new[]
            {
                Transfer("cloud:a", "Uploaded", 20),
                Transfer("cloud:b", "Failed", 20)
            };

            var result = MaintenanceCloudTransferResolver.Resolve(new[] { snapshot }, loaded);

            Assert.Equal(1, result.GetEffectiveAttentionCount(1));
            Assert.Equal(1, result.AddedAttentionCount);
        }

        [Fact]
        public void TimingLabelsDoNotCallUploadAttemptsVerification()
        {
            var cloud = new MaintenanceActionItem
            {
                ActionKind = MaintenanceActionKind.CloudTransfer,
                LastVerifiedDisplay = "不应显示",
                LastAttemptDisplay = "2026-09-08 12:00",
                NextAttemptDisplay = "稍后"
            };
            var quarantine = new MaintenanceActionItem
            {
                ActionKind = MaintenanceActionKind.RetentionQuarantine,
                LastVerifiedDisplay = "不应显示",
                LedgerUpdatedDisplay = "2026-09-08 12:01",
                NextAttemptDisplay = "人工确认"
            };

            Assert.Equal("上次尝试：2026-09-08 12:00 · 下次尝试：稍后", cloud.TimingDisplay);
            Assert.Equal("账本更新：2026-09-08 12:01 · 下次尝试：人工确认", quarantine.TimingDisplay);
        }

        [Fact]
        public void CloudTransferActionUsesRelativeTimingWithFullEvidenceAndKeepsUnknownStates()
        {
            var cloud = new MaintenanceActionItem
            {
                ActionKind = MaintenanceActionKind.CloudTransfer,
                LastAttemptDisplay = "3 分钟前",
                LastAttemptFullDisplay = "2026-09-08 20:00:00 (UTC+08:00) · 2026-09-08T12:00:00.0000000Z",
                NextAttemptDisplay = "约 12 分钟后",
                NextAttemptFullDisplay = "2026-09-08 20:15:00 (UTC+08:00) · 2026-09-08T12:15:00.0000000Z · 约 12 分钟后"
            };
            var unknownSummary = new CloudTransferSummaryDto();
            var expiredSummary = new CloudTransferSummaryDto
            {
                NextAttemptUtc = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            Assert.Equal("上次尝试：3 分钟前 · 下次尝试：约 12 分钟后", cloud.TimingDisplay);
            Assert.Contains("2026-09-08T12:00:00.0000000Z", cloud.TimingFullDisplay);
            Assert.Contains("2026-09-08T12:15:00.0000000Z", cloud.TimingFullDisplay);
            Assert.Equal("按队列状态", unknownSummary.NextAttemptRelativeDisplay);
            Assert.Equal("可立即重试", expiredSummary.NextAttemptRelativeDisplay);
            Assert.Contains("2020-01-01T00:00:00.0000000Z", expiredSummary.NextAttemptFullDisplay);
        }

        [Fact]
        public void MaintenanceOverviewGroupsActionsAndBoundsTheDefaultPreview()
        {
            var items = new List<MaintenanceActionItem>
            {
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.HealthInspection, Title = "巡检" },
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.CloudTransfer, TransferState = "RetryScheduled", Title = "等待重试" },
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.RetentionQuarantine, Title = "隔离 1" },
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.RetentionQuarantine, Title = "隔离 2" },
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.RetentionQuarantine, Title = "隔离 3" },
                new MaintenanceActionItem { ActionKind = MaintenanceActionKind.RetentionQuarantine, Title = "隔离 4" }
            };
            for (var index = 5; index <= 20; index++)
            {
                items.Add(new MaintenanceActionItem
                {
                    ActionKind = MaintenanceActionKind.RetentionQuarantine,
                    Title = index == 20 ? "隔离 " + new string('x', 180) : $"隔离 {index}"
                });
            }

            var manual = new MaintenanceActionSection(
                MaintenanceActionGroup.NeedsManualHandling,
                "需要人工处理",
                "说明",
                items.Where(item => item.Group == MaintenanceActionGroup.NeedsManualHandling));
            var waiting = new MaintenanceActionSection(
                MaintenanceActionGroup.WaitingForRetry,
                "等待自动重试",
                "说明",
                items.Where(item => item.Group == MaintenanceActionGroup.WaitingForRetry));

            Assert.Equal(3, manual.PreviewItems.Count);
            Assert.Equal(20, manual.Items.Count);
            Assert.Equal(17, manual.OverflowCount);
            Assert.True(manual.HasOverflow);
            Assert.Single(waiting.PreviewItems);
            Assert.False(waiting.HasOverflow);

            var empty = new MaintenanceActionSection(
                MaintenanceActionGroup.Routine,
                "例行巡检",
                "说明",
                Enumerable.Empty<MaintenanceActionItem>());
            Assert.Empty(empty.PreviewItems);
            Assert.False(empty.HasOverflow);
        }

        private static CloudTransferStatusDto Transfer(string key, string state, int minute)
            => new CloudTransferStatusDto
            {
                TransferKey = key,
                State = state,
                UpdatedUtc = new DateTime(2026, 9, 8, 0, minute, 0, DateTimeKind.Utc)
            };
    }
}
