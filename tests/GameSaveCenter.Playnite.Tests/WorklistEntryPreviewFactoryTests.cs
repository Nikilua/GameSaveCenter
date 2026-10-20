using System;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistEntryPreviewFactoryTests
{
    [Fact]
    public void BackupAndMediaPreviewsUseStableIdsWithoutPersistingPaths()
    {
        const string backupPath = @"C:\private\saves\archive.zip";
        const string mediaPath = @"C:\private\screenshots\capture.png";
        var backup = new BackupVersionDto
        {
            PlayniteId = "game-1",
            BackupId = "backup-2",
            ArchivePath = backupPath,
            FileCount = 3,
            TotalBytes = 1024
        };
        var media = new MediaItemDto
        {
            MediaId = "media-7",
            PlayniteId = "game-1",
            OriginalPath = mediaPath,
            ArchivePath = mediaPath
        };

        var backupPreview = WorklistEntryPreviewFactory.FromBackupVersion(backup, "测试游戏")!;
        var mediaPreview = WorklistEntryPreviewFactory.FromMediaItem(media, "测试游戏")!;

        Assert.Equal(WorklistObjectKind.BackupVersion, backupPreview.Kind);
        Assert.Contains("6:game-1", backupPreview.StableObjectId, StringComparison.Ordinal);
        Assert.Contains("backup-2", backupPreview.StableObjectId, StringComparison.Ordinal);
        Assert.DoesNotContain(backupPath, backupPreview.StableObjectId, StringComparison.Ordinal);
        Assert.Equal("media-7", mediaPreview.StableObjectId);
        Assert.DoesNotContain(mediaPath, mediaPreview.StableObjectId, StringComparison.Ordinal);
        Assert.Equal("media-7", media.MediaId);
        Assert.DoesNotContain(mediaPath, mediaPreview.ToRecord().StableObjectId, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskAndFindingPreviewsRequireTheirPersistentPrimaryKeys()
    {
        var task = new TaskStatusDto
        {
            TaskId = "task-9",
            TaskType = "Backup",
            GameName = "测试游戏",
            State = TaskState.Failed
        };
        var findingWithoutId = new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "CHECK_FAILED",
            Title = "校验失败"
        };
        var finding = new ValidationFindingDto
        {
            FindingId = "finding-11",
            PlayniteId = "game-1",
            Code = "CHECK_FAILED",
            Title = "校验失败"
        };

        var taskPreview = WorklistEntryPreviewFactory.FromBackgroundTask(task)!;
        var findingPreview = WorklistEntryPreviewFactory.FromValidationFinding(finding)!;

        Assert.Equal("task-9", taskPreview.StableObjectId);
        Assert.Equal(WorklistObjectKind.ValidationFinding, findingPreview.Kind);
        Assert.Equal("finding-11", findingPreview.StableObjectId);
        Assert.Null(WorklistEntryPreviewFactory.FromValidationFinding(findingWithoutId));
        Assert.Equal("CHECK_FAILED", finding.Code);
    }

    [Fact]
    public void WorklistsAndActiveSelectionSurviveSettingsSerialization()
    {
        var id = Guid.NewGuid();
        var settings = new GameSaveCenterSettings
        {
            Worklists = new System.Collections.Generic.List<WorklistDefinition>
            {
                new()
                {
                    Id = id,
                    Name = "后续检查",
                    Items = new System.Collections.Generic.List<WorklistItemRecord>
                    {
                        new() { Kind = WorklistObjectKind.ValidationFinding, StableObjectId = "finding-1" }
                    }
                }
            },
            ActiveWorklistId = id.ToString("D")
        };

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(settings))!;

        var list = Assert.Single(restored.Worklists);
        Assert.Equal(id, list.Id);
        Assert.Equal(id.ToString("D"), restored.ActiveWorklistId);
        Assert.Equal("finding-1", Assert.Single(list.Items).StableObjectId);
    }
}
