using System;
using System.Linq;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class BackupLineageBuilderTests
{
    [Fact]
    public void UsesExplicitParentIdsAndContentFingerprintsAcrossDevices()
    {
        var snapshots = new[]
        {
            new BackupSnapshot
            {
                BackupId = "base",
                SourceDevice = "Desktop",
                ContentFingerprint = "base-content",
                Files = { new FileManifestEntry { RelativePath = "save.dat", Sha256 = "hash" } }
            },
            new BackupSnapshot
            {
                BackupId = "handheld-child",
                ParentBackupId = "BASE",
                SourceDevice = "Handheld",
                ContentFingerprint = "same-final-content",
                TotalBytes = 42,
                FileCount = 1
            },
            new BackupSnapshot
            {
                BackupId = "desktop-copy",
                SourceDevice = "Desktop",
                ContentFingerprint = "SAME-FINAL-CONTENT"
            }
        };

        var model = new BackupLineageBuilder().Build(snapshots);

        Assert.Equal(3, model.Nodes.Count);
        Assert.Equal("Handheld", model.Nodes[1].SourceDevice);
        Assert.Equal(42, model.Nodes[1].TotalBytes);
        Assert.Contains(model.Relations, relation =>
            relation.Kind == BackupLineageRelationKind.Parent &&
            relation.FromNodeIndex == 0 && relation.ToNodeIndex == 1);
        Assert.Contains(model.Relations, relation =>
            relation.Kind == BackupLineageRelationKind.EquivalentContent &&
            relation.FromNodeIndex == 1 && relation.ToNodeIndex == 2);
        Assert.DoesNotContain(model.Issues, issue => issue.Kind == BackupLineageIssueKind.ParentNotFound);
    }

    [Fact]
    public void ReportsMissingParentsAndUnspecifiedRoots()
    {
        var model = new BackupLineageBuilder().Build(new[]
        {
            new BackupSnapshot { BackupId = "child", ParentBackupId = "not-loaded" },
            new BackupSnapshot { BackupId = "root-without-parent" }
        });

        var missing = Assert.Single(model.Issues, issue => issue.Kind == BackupLineageIssueKind.ParentNotFound);
        Assert.Equal("child", missing.BackupId);
        Assert.Equal("not-loaded", missing.RelatedBackupId);
        var unknown = Assert.Single(model.Issues, issue => issue.Kind == BackupLineageIssueKind.UnknownRelationship);
        Assert.Equal("root-without-parent", unknown.BackupId);
    }

    [Fact]
    public void ReportsParentCycles()
    {
        var model = new BackupLineageBuilder().Build(new[]
        {
            new BackupSnapshot { BackupId = "A", ParentBackupId = "B" },
            new BackupSnapshot { BackupId = "B", ParentBackupId = "A" }
        });

        var cycle = Assert.Single(model.Issues, issue => issue.Kind == BackupLineageIssueKind.ParentCycle);
        Assert.Equal(2, cycle.NodeIndices.Count);
        Assert.Equal(new[] { 0, 1 }, cycle.NodeIndices.OrderBy(index => index));
    }

    [Fact]
    public void ReportsVersionsWithoutAnIdentity()
    {
        var model = new BackupLineageBuilder().Build(new[] { new BackupSnapshot() });

        var issue = Assert.Single(model.Issues, value => value.Kind == BackupLineageIssueKind.MissingBackupId);
        Assert.Equal(new[] { 0 }, issue.NodeIndices);
    }

    [Fact]
    public void DuplicateIdsAreReportedAndDoNotResolveAnAmbiguousParent()
    {
        var model = new BackupLineageBuilder().Build(new[]
        {
            new BackupSnapshot { BackupId = "Version" },
            new BackupSnapshot { BackupId = "version" },
            new BackupSnapshot { BackupId = "child", ParentBackupId = "VERSION" }
        });

        var duplicate = Assert.Single(model.Issues, issue => issue.Kind == BackupLineageIssueKind.DuplicateBackupId);
        Assert.Equal(new[] { 0, 1 }, duplicate.NodeIndices);
        var ambiguous = Assert.Single(model.Issues, issue => issue.Kind == BackupLineageIssueKind.AmbiguousParentReference);
        Assert.Equal("child", ambiguous.BackupId);
        Assert.Equal("VERSION", ambiguous.RelatedBackupId);
        Assert.DoesNotContain(model.Relations, relation => relation.Kind == BackupLineageRelationKind.Parent);
    }

    [Fact]
    public void TimestampsDoNotInventLinearRelationships()
    {
        var model = new BackupLineageBuilder().Build(new[]
        {
            new BackupSnapshot { BackupId = "older", CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new BackupSnapshot { BackupId = "newer", CreatedUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
        });

        Assert.Empty(model.Relations);
        Assert.Equal(2, model.Issues.Count(issue => issue.Kind == BackupLineageIssueKind.UnknownRelationship));
    }

    [Fact]
    public void StopsAfterTheLimitAndOneLookaheadItem()
    {
        var yielded = 0;
        var model = new BackupLineageBuilder().Build(CreateSnapshots(), inputItemLimit: 3);

        Assert.Equal(3, model.Nodes.Count);
        Assert.Equal(3, model.InputItemLimit);
        Assert.True(model.InputTruncated);
        Assert.Equal(4, yielded);

        System.Collections.Generic.IEnumerable<BackupSnapshot> CreateSnapshots()
        {
            for (var index = 0; index < 10; index++)
            {
                yielded++;
                yield return new BackupSnapshot { BackupId = $"backup-{index}" };
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(BackupLineageBuilder.MaximumNodeCount + 1)]
    public void RejectsUnboundedOrEmptyLimits(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BackupLineageBuilder().Build(Array.Empty<BackupSnapshot>(), limit));
    }
}
