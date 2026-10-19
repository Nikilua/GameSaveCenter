using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class BackupLineageDisplayBehaviorTests
{
    [Fact]
    public void DisplaysForksSameContentAndDeviceNamesAsChronologicalText()
    {
        var start = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var root = Backup("root", string.Empty, start, "desktop", "root-fingerprint");
        var firstBranch = Backup("branch-a", "root", start.AddHours(1), "desktop", "shared-fingerprint");
        var secondBranch = Backup("branch-b", "root", start.AddHours(2), "laptop", "other-fingerprint");
        var equivalent = Backup("copy-a", "branch-a", start.AddHours(3), "handheld", "shared-fingerprint");
        var loaded = new[] { equivalent, secondBranch, firstBranch, root };

        var fork = BackupLineageDisplayBuilder.Build(root, loaded, loaded.Length);
        Assert.Contains("2 个直接后续版本", fork.Summary, StringComparison.Ordinal);
        Assert.True(HasText(fork.Items, "分叉后续版本", "desktop"));
        Assert.True(HasText(fork.Items, "分叉后续版本", "laptop"));
        Assert.True(IndexOf(fork.Items, "branch-a") < IndexOf(fork.Items, "branch-b"));

        var equalContent = BackupLineageDisplayBuilder.Build(firstBranch, loaded, loaded.Length);
        Assert.True(HasText(equalContent.Items, "内容相同", "copy-a"));
        Assert.True(HasText(equalContent.Items, "来源父版本", "root"));
    }

    [Fact]
    public void MissingParentIsMarkedUnloadedUntilTheFullHistoryHasBeenRead()
    {
        var version = Backup("child", "older-parent", DateTime.UtcNow, "desktop", "fingerprint");
        var partial = BackupLineageDisplayBuilder.Build(version, new[] { version }, totalVersionCount: 2);
        Assert.True(HasText(partial.Items, "不能据此判断版本缺失"));

        var complete = BackupLineageDisplayBuilder.Build(version, new[] { version }, totalVersionCount: 1);
        Assert.True(HasText(complete.Items, "未出现在当前完整索引"));
    }

    [Fact]
    public void UnknownParentAndMissingFingerprintAreDescribedWithoutColorCues()
    {
        var version = Backup("legacy", string.Empty, DateTime.UtcNow, "", string.Empty);
        var display = BackupLineageDisplayBuilder.Build(version, new[] { version }, totalVersionCount: 1);

        Assert.True(HasText(display.Items, "来源未知"));
        Assert.True(HasText(display.Items, "内容关系未知"));
    }

    [Fact]
    public void ClockSkewAndOldReadinessRemainEvidenceLedWithoutWritingArchiveData()
    {
        var fixtureDirectory = Path.Combine(Path.GetTempPath(), "GameSaveCenter-S03-08-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtureDirectory);
        var archiveFixture = Path.Combine(fixtureDirectory, "synthetic-save.bin");
        var originalArchiveBytes = new byte[] { 0x47, 0x53, 0x43, 0x08, 0x00, 0xFF };
        File.WriteAllBytes(archiveFixture, originalArchiveBytes);

        try
        {
            var now = new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
            var baseTime = now.AddHours(-2);
            var childTime = now.AddHours(-4);
            var equivalentTime = now.AddHours(-3);
            var source = Backup("desktop-base", string.Empty, baseTime, "Desktop", "base-fingerprint");
            var child = Backup("laptop-branch", source.BackupId, childTime, "Laptop", "shared-fingerprint");
            var equivalent = Backup("handheld-copy", source.BackupId, equivalentTime, "Handheld", "SHARED-FINGERPRINT");
            var loaded = new[] { equivalent, source, child };

            var lineage = BackupLineageDisplayBuilder.Build(child, loaded, totalVersionCount: loaded.Length);
            var equivalentPosition = IndexOf(lineage.Items, "handheld-copy");
            var parentPosition = IndexOf(lineage.Items, "desktop-base");

            Assert.True(HasText(lineage.Items, "来源父版本", "desktop-base"));
            Assert.True(HasText(lineage.Items, "内容相同", "handheld-copy"));
            Assert.True(equivalentPosition >= 0 && parentPosition >= 0 && equivalentPosition < parentPosition);

            var conflictText = string.Join("\n", DeviceConflictComparisonDisplayBuilder.Build(new DeviceConflictStatusDto
            {
                HasConflict = true,
                Reason = "DivergedFromCommonBase",
                LocalDevice = "Laptop",
                RemoteDevice = "Desktop",
                LocalBackupId = "laptop-branch",
                RemoteBackupId = "desktop-branch",
                LocalParentBackupId = "desktop-base",
                RemoteParentBackupId = "desktop-base",
                LocalCreatedUtc = childTime,
                RemoteCreatedUtc = equivalentTime,
                LocalContentFingerprintAvailable = true,
                RemoteContentFingerprintAvailable = true,
                ContentFingerprintsMatch = false,
                LocalReadinessStatus = "Ready",
                LocalReadinessCheckedUtc = now.AddDays(-41)
            }, now));

            Assert.Contains("两个版本从同一父版本分叉", conflictText);
            Assert.Contains("41 天前", conflictText);
            Assert.Contains("结果仅反映该次检查", conflictText);
            Assert.DoesNotContain("已过期", conflictText);
            Assert.Contains("系统不会因为创建时间较新就推荐覆盖另一侧", conflictText);
            Assert.Equal(new[] { archiveFixture }, Directory.GetFiles(fixtureDirectory));
            Assert.Equal(originalArchiveBytes, File.ReadAllBytes(archiveFixture));
        }
        finally
        {
            Directory.Delete(fixtureDirectory, recursive: true);
        }
    }

    [Fact]
    public void SaveHistoryShowsRelationTextAndIncrementalLoadAction()
    {
        var root = TestRepositoryContext.Root;
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var history = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.BackupHistory.cs"));

        Assert.Contains("SelectedBackupLineageItems", save);
        Assert.Contains("备份来源关系文字列表", save);
        Assert.Contains("LoadMoreBackupHistoryCommand", save);
        Assert.Contains("BeforeCreatedUtcKey", history);
        Assert.Contains("BeforeBackupId", history);
    }

    private static int IndexOf(System.Collections.Generic.IReadOnlyList<string> items, string backupId)
        => items.ToList().FindIndex(item => item.IndexOf(backupId, StringComparison.Ordinal) >= 0);

    private static bool HasText(System.Collections.Generic.IEnumerable<string> items, string first, string? second = null)
        => items.Any(item => item.IndexOf(first, StringComparison.Ordinal) >= 0
            && (second == null || item.IndexOf(second, StringComparison.Ordinal) >= 0));

    private static BackupVersionDto Backup(string id, string parentId, DateTime createdUtc, string device, string fingerprint)
        => new BackupVersionDto
        {
            BackupId = id,
            ParentBackupId = parentId,
            CreatedUtc = createdUtc,
            SourceDevice = device,
            ContentFingerprint = fingerprint
        };
}
