using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistDefinitionTests
{
    [Fact]
    public void ItemNormalizationDeduplicatesStableReferencesWithinKind()
    {
        var listId = Guid.NewGuid();
        var list = new WorklistDefinition
        {
            Id = listId,
            Name = "  恢复后续  ",
            Items = new List<WorklistItemRecord>
            {
                new()
                {
                    Kind = WorklistObjectKind.BackupVersion,
                    StableObjectId = " Game-1:Backup-5 ",
                    Note = "  检查档案  ",
                    IsManuallyComplete = true
                },
                new()
                {
                    Kind = WorklistObjectKind.BackupVersion,
                    StableObjectId = "game-1:backup-5",
                    Note = "重复引用",
                    IsManuallyComplete = false
                },
                new()
                {
                    Kind = WorklistObjectKind.MediaItem,
                    StableObjectId = "game-1:backup-5"
                }
            }
        };

        var normalized = WorklistDefinition.Normalize(list)!;

        Assert.Equal(listId, normalized.Id);
        Assert.Equal("恢复后续", normalized.Name);
        Assert.Equal(2, normalized.Items.Count);
        Assert.Equal("Game-1:Backup-5", normalized.Items[0].StableObjectId);
        Assert.Equal("检查档案", normalized.Items[0].Note);
        Assert.True(normalized.Items[0].IsManuallyComplete);
        Assert.Equal(WorklistObjectKind.MediaItem, normalized.Items[1].Kind);
    }

    [Fact]
    public void ListItemAndTextSizesAreBounded()
    {
        var source = Enumerable.Range(0, WorklistDefinition.MaximumWorklists + 5)
            .Select(listIndex => new WorklistDefinition
            {
                Id = Guid.NewGuid(),
                Name = new string('n', WorklistDefinition.MaximumNameLength + 20),
                Items = Enumerable.Range(0, WorklistDefinition.MaximumItemsPerWorklist + 50)
                    .Select(itemIndex => new WorklistItemRecord
                    {
                        Kind = WorklistObjectKind.BackgroundTask,
                        StableObjectId = $"task-{listIndex}-{itemIndex:D3}",
                        Note = new string('x', WorklistDefinition.MaximumNoteLength + 2)
                    })
                    .ToList()
            })
            .ToArray();

        var normalized = WorklistDefinition.NormalizeMany(source);

        Assert.Equal(WorklistDefinition.MaximumWorklists, normalized.Count);
        Assert.All(normalized, worklist =>
        {
            Assert.Equal(WorklistDefinition.MaximumNameLength, worklist.Name.Length);
            Assert.Equal(WorklistDefinition.MaximumItemsPerWorklist, worklist.Items.Count);
            Assert.All(worklist.Items, item => Assert.Equal(WorklistDefinition.MaximumNoteLength, item.Note.Length));
        });
    }

    [Fact]
    public void InvalidKindsAndUnstableIdentifiersAreDroppedWithoutTruncatingIdentity()
    {
        var worklist = new WorklistDefinition
        {
            Name = " ",
            Items = new List<WorklistItemRecord>
            {
                new() { Kind = (WorklistObjectKind)999, StableObjectId = "invalid-kind" },
                new() { Kind = WorklistObjectKind.MediaItem, StableObjectId = " " },
                new() { Kind = WorklistObjectKind.MediaItem, StableObjectId = new string('i', WorklistDefinition.MaximumStableObjectIdLength + 1) },
                new()
                {
                    Kind = WorklistObjectKind.ValidationFinding,
                    StableObjectId = "finding:stable-7",
                    Note = "  跟进  ",
                    IsManuallyComplete = true
                }
            }
        };

        var normalized = WorklistDefinition.Normalize(worklist)!;

        Assert.NotEqual(Guid.Empty, normalized.Id);
        Assert.Equal("未命名清单", normalized.Name);
        var item = Assert.Single(normalized.Items);
        Assert.Equal(WorklistObjectKind.ValidationFinding, item.Kind);
        Assert.Equal("finding:stable-7", item.StableObjectId);
        Assert.Equal("跟进", item.Note);
        Assert.True(item.IsManuallyComplete);
    }

    [Fact]
    public void NormalizeManyKeepsListIdentityAndProducesIndependentClones()
    {
        var id = Guid.NewGuid();
        var original = new WorklistDefinition
        {
            Id = id,
            Name = "旧名称",
            Items = new List<WorklistItemRecord>
            {
                new() { Kind = WorklistObjectKind.BackupVersion, StableObjectId = "game:version" }
            }
        };
        var replacement = new WorklistDefinition
        {
            Id = id,
            Name = "新名称",
            Items = new List<WorklistItemRecord>
            {
                new() { Kind = WorklistObjectKind.BackupVersion, StableObjectId = "game:version", Note = "新备注" }
            }
        };

        var normalized = Assert.Single(WorklistDefinition.NormalizeMany(new[] { original, replacement }));
        var clone = normalized.Clone();
        clone.Items[0].Note = "只改副本";

        Assert.Equal(id, normalized.Id);
        Assert.Equal("新名称", normalized.Name);
        Assert.NotSame(normalized.Items[0], clone.Items[0]);
        Assert.Equal("新备注", normalized.Items[0].Note);
    }
}
