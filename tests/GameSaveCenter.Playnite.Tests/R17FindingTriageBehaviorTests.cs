using System;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17FindingTriageBehaviorTests
{
    [Fact]
    public void SameProblemFromTwoSourcesCountsOnceAndKeepsHighestImpactEvidence()
    {
        var olderWarning = new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "BACKUP_ARCHIVE_MISSING",
            Title = "备份归档文件缺失",
            Detail = "完整性检查发现",
            Severity = FindingSeverity.Warning,
            CreatedUtc = new DateTime(2026, 9, 19, 10, 0, 0, DateTimeKind.Utc)
        };
        var newerError = new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = " backup_archive_missing ",
            Title = " 备份归档文件缺失 ",
            Detail = "健康巡检也发现",
            Severity = FindingSeverity.Error,
            CreatedUtc = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc)
        };

        var result = FindingTriageResolver.Resolve(new[] { olderWarning, newerError });

        var finding = Assert.Single(result.Items);
        Assert.Equal(FindingSeverity.Error, finding.Severity);
        Assert.Equal(newerError.CreatedUtc, finding.CreatedUtc);
        Assert.Equal(1, Assert.Single(result.Groups, group => group.Group == FindingImpactGroup.Immediate).Count);
        Assert.Equal(0, Assert.Single(result.Groups, group => group.Group == FindingImpactGroup.Recommended).Count);
    }

    [Fact]
    public void DifferentHealthInspectionBackupsRemainSeparateAndAreOrderedByImpact()
    {
        var findings = new[]
        {
            new ValidationFindingDto { PlayniteId = "game-1", Code = "HEALTH_INSPECTION_FAILED", Title = "备份恢复校验需关注：backup-warning.zip", Severity = FindingSeverity.Warning, CreatedUtc = DateTime.UtcNow.AddMinutes(-2) },
            new ValidationFindingDto { PlayniteId = "game-1", Code = "HEALTH_INSPECTION_FAILED", Title = "备份恢复校验需关注：backup-error.zip", Severity = FindingSeverity.Error, CreatedUtc = DateTime.UtcNow.AddMinutes(-1) },
            new ValidationFindingDto { PlayniteId = "game-1", Code = "MEDIA_INFO", Title = "媒体来源已记录", Severity = FindingSeverity.Info, CreatedUtc = DateTime.UtcNow }
        };

        var result = FindingTriageResolver.Resolve(findings);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(FindingSeverity.Error, result.Items[0].Severity);
        Assert.Equal(FindingSeverity.Warning, result.Items[1].Severity);
        Assert.Equal(FindingSeverity.Info, result.Items[2].Severity);
        Assert.Equal(1, Assert.Single(result.Groups, group => group.Group == FindingImpactGroup.Immediate).Count);
        Assert.Equal(1, Assert.Single(result.Groups, group => group.Group == FindingImpactGroup.Recommended).Count);
        Assert.Equal(1, Assert.Single(result.Groups, group => group.Group == FindingImpactGroup.Information).Count);
    }

    [Fact]
    public void EvidenceDisplayRetainsUtcRecordedMomentForDetails()
    {
        var finding = new ValidationFindingDto
        {
            CreatedUtc = new DateTime(2026, 9, 20, 3, 4, 5, DateTimeKind.Utc)
        };

        Assert.Contains("证据时间：", finding.EvidenceTimeDisplay);
        Assert.Contains("2026-09-20", finding.EvidenceTimeDisplay);
        Assert.DoesNotContain("未知", finding.EvidenceTimeDisplay);
    }
}
