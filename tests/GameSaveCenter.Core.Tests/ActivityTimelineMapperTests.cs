using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class ActivityTimelineMapperTests
{
    [Fact]
    public void BackupAuditMapsToSucceededBackupActivityWithGameName()
    {
        var entry = new AuditLogEntryDto
        {
            Category = "Backup",
            Message = "Ludusavi 备份结果：Cyberpunk 2077 / 新增 1 个文件",
            DetailJson = "{\"playniteId\":\"g1\",\"change\":\"Added\"}",
            CreatedUtc = DateTime.UtcNow
        };
        var games = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["g1"] = "Cyberpunk 2077" };

        var activity = ActivityTimelineMapper.Map(entry, games);

        Assert.Equal("Backup", activity.Kind);
        Assert.Equal("Succeeded", activity.Result);
        Assert.Equal("Cyberpunk 2077", activity.GameName);
        Assert.Contains("备份结果", activity.Summary);
    }

    [Fact]
    public void CloudFailureMapsToFailedCloudActivity()
    {
        var entry = new AuditLogEntryDto
        {
            Category = "CloudRetry",
            Message = "云端复制失败（NETWORK），未安排自动重试",
            DetailJson = "{}",
            CreatedUtc = DateTime.UtcNow
        };

        var activity = ActivityTimelineMapper.Map(entry, null);

        Assert.Equal("Cloud", activity.Kind);
        Assert.Equal("Failed", activity.Result);
        Assert.Equal("全局", activity.GameName);
    }

    [Fact]
    public void ConflictAndRepositoryRepairMapToCuratedKinds()
    {
        var conflict = ActivityTimelineMapper.Map(new AuditLogEntryDto
        {
            Category = "DeviceConflict",
            Message = "已记录人工冲突决策",
            CreatedUtc = DateTime.UtcNow
        }, null);
        var repair = ActivityTimelineMapper.Map(new AuditLogEntryDto
        {
            Category = "RepositoryRebuild",
            Message = "备份索引重建完成",
            CreatedUtc = DateTime.UtcNow
        }, null);

        Assert.Equal("Conflict", conflict.Kind);
        Assert.Equal("Info", conflict.Result);
        Assert.Equal("RepositoryRepair", repair.Kind);
        Assert.Equal("Succeeded", repair.Result);
    }

    [Fact]
    public void LongMessagesAreTrimmedForTimeline()
    {
        var entry = new AuditLogEntryDto
        {
            Category = "Media",
            Message = new string('x', 300),
            CreatedUtc = DateTime.UtcNow
        };

        var activity = ActivityTimelineMapper.Map(entry, null);

        Assert.True(activity.Summary.Length <= 181);
        Assert.EndsWith("…", activity.Summary);
    }
}
