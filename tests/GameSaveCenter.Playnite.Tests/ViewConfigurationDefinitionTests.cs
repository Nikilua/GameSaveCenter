using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class ViewConfigurationDefinitionTests
{
    [Fact]
    public void NormalizeKeepsStableScalarStateAndDropsInvalidValues()
    {
        var id = Guid.NewGuid().ToString("N");
        var presetId = Guid.NewGuid().ToString("N");
        var normalized = ViewConfigurationDefinition.Normalize(new ViewConfigurationDefinition
        {
            Id = id,
            Name = "  任务视图  ",
            WorkspaceKey = " Tasks ",
            FilterPresetId = presetId,
            SortColumnKey = " state ",
            SortDirection = "descending",
            VisibleColumnKeys = new List<string> { " state ", "task", "state", " " },
            ColumnWidths = new Dictionary<string, double>
            {
                ["state"] = 180,
                ["task"] = double.NaN,
                ["detail"] = 5000
            },
            DensityMode = "compact"
        });

        Assert.NotNull(normalized);
        Assert.Equal(1, normalized!.SchemaVersion);
        Assert.Equal(id, normalized.Id);
        Assert.Equal("任务视图", normalized.Name);
        Assert.Equal("Tasks", normalized.WorkspaceKey);
        Assert.Equal(presetId, normalized.FilterPresetId);
        Assert.Equal("state", normalized.SortColumnKey);
        Assert.Equal("Descending", normalized.SortDirection);
        Assert.Equal(new[] { "state", "task" }, normalized.VisibleColumnKeys);
        Assert.Equal(new[] { "state" }, normalized.ColumnWidths.Keys);
        Assert.Equal(180, normalized.ColumnWidths["state"]);
        Assert.Equal("Compact", normalized.DensityMode);
    }

    [Fact]
    public void ConfigurationContractContainsOnlyStableKeysAndScalarValues()
    {
        var allowedTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(string),
            typeof(List<string>),
            typeof(Dictionary<string, double>)
        };

        Assert.All(typeof(ViewConfigurationDefinition).GetProperties(BindingFlags.Public | BindingFlags.Instance), property =>
            Assert.Contains(property.PropertyType, allowedTypes));

        var json = JsonConvert.SerializeObject(new ViewConfigurationDefinition
        {
            Name = "任务视图",
            WorkspaceKey = "Tasks",
            VisibleColumnKeys = new List<string> { "state" }
        });
        Assert.DoesNotContain("Dto", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FileContent", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeManyRejectsInvalidAndUnsupportedEntriesAndCapsCollection()
    {
        var entries = new List<ViewConfigurationDefinition>
        {
            new() { Name = "", WorkspaceKey = "Tasks" },
            new() { SchemaVersion = 2, Name = "Future", WorkspaceKey = "Tasks" }
        };
        entries.AddRange(Enumerable.Range(0, 40).Select(index => new ViewConfigurationDefinition
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = $"View {index}",
            WorkspaceKey = "Tasks"
        }));

        var normalized = ViewConfigurationDefinition.NormalizeMany(entries);

        Assert.Equal(32, normalized.Count);
        Assert.Equal(32, normalized.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(normalized, item => Assert.Equal(ViewConfigurationDefinition.CurrentSchemaVersion, item.SchemaVersion));
    }

    [Fact]
    public void UnknownJsonMembersAndMissingViewCollectionRemainCompatible()
    {
        const string legacyJson = "{\"DataGridColumnWidths\":{\"v1/tasks/state\":180},\"UnknownLegacyField\":{\"future\":true},\"ViewConfigurations\":[{\"SchemaVersion\":7,\"Name\":\"Future\",\"WorkspaceKey\":\"Tasks\"}]}";
        var settings = JsonConvert.DeserializeObject<GameSaveCenterSettings>(legacyJson)!;

        Assert.Empty(settings.ViewConfigurations);
        Assert.Equal(180, settings.DataGridColumnWidths["v1/tasks/state"]);

        const string configJson = "{\"SchemaVersion\":1,\"Id\":\"11111111111111111111111111111111\",\"Name\":\"Tasks\",\"WorkspaceKey\":\"Tasks\",\"UnknownFutureMember\":\"ignored\"}";
        var config = JsonConvert.DeserializeObject<ViewConfigurationDefinition>(configJson)!;

        Assert.Equal("Tasks", config.Name);
        Assert.Equal("Tasks", config.WorkspaceKey);
    }

    [Fact]
    public void SettingsRoundTripAndClonePreserveConfigurationWithoutSharingCollections()
    {
        var presetId = Guid.NewGuid().ToString("N");
        var source = new GameSaveCenterSettings
        {
            FilterPresets = new List<FilterPresetDefinition>
            {
                new() { Id = presetId, Name = "失败任务", Workspace = FilterPresetDefinition.TasksWorkspace }
            },
            DataGridColumnWidths = new Dictionary<string, double> { ["v1/tasks/state"] = 180 },
            ViewConfigurations = new List<ViewConfigurationDefinition>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "失败任务视图",
                    WorkspaceKey = "Tasks",
                    FilterPresetId = presetId,
                    SortColumnKey = "state",
                    SortDirection = "Descending",
                    VisibleColumnKeys = new List<string> { "state", "task" },
                    ColumnWidths = new Dictionary<string, double> { ["state"] = 180 },
                    DensityMode = "Compact"
                }
            }
        };

        var json = JsonConvert.SerializeObject(source);
        var imported = JsonConvert.DeserializeObject<GameSaveCenterSettings>(json)!;
        var config = Assert.Single(imported.ViewConfigurations);

        Assert.Single(imported.FilterPresets);
        Assert.Equal("失败任务视图", config.Name);
        Assert.Equal(presetId, config.FilterPresetId);
        Assert.Equal("state", config.SortColumnKey);
        Assert.Equal("Descending", config.SortDirection);
        Assert.Equal(new[] { "state", "task" }, config.VisibleColumnKeys);
        Assert.Equal(180, config.ColumnWidths["state"]);
        Assert.Equal("Compact", config.DensityMode);
        Assert.Equal(180, imported.DataGridColumnWidths["v1/tasks/state"]);

        imported.BeginEdit();
        config.VisibleColumnKeys.Add("mutated");
        config.ColumnWidths["state"] = 999;
        imported.CancelEdit();

        var restored = Assert.Single(imported.ViewConfigurations);
        Assert.Equal(new[] { "state", "task" }, restored.VisibleColumnKeys);
        Assert.Equal(180, restored.ColumnWidths["state"]);
    }
}
