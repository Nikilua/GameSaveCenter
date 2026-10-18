using System;
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
