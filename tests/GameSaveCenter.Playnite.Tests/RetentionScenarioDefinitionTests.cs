using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RetentionScenarioDefinitionTests
{
    [Fact]
    public void SettingsRoundTripAndEditClonePreserveNamedAssumptionsAndSnapshotTime()
    {
        var snapshotUtc = new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Utc);
        var savedScenario = ValidScenario();
        savedScenario.Name = "低增长方案";
        savedScenario.KeepDailyDays = 45;
        savedScenario.GrowthMinimumMiBPer30Days = 100;
        savedScenario.GrowthMaximumMiBPer30Days = 300;
        savedScenario.LastSnapshotUtc = snapshotUtc;
        var settings = new GameSaveCenterSettings
        {
            RetentionScenarioDrafts = new List<RetentionScenarioDefinition> { savedScenario }
        };

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(settings))!;
        var draft = Assert.Single(restored.RetentionScenarioDrafts);

        Assert.Equal("低增长方案", draft.Name);
        Assert.Equal(45, draft.KeepDailyDays);
        Assert.Equal(100, draft.GrowthMinimumMiBPer30Days);
        Assert.Equal(300, draft.GrowthMaximumMiBPer30Days);
        Assert.Equal(snapshotUtc, draft.LastSnapshotUtc);
        Assert.Equal(DateTimeKind.Utc, draft.LastSnapshotUtc!.Value.Kind);

        restored.BeginEdit();
        draft.KeepDailyDays = 99;
        restored.CancelEdit();

        var reverted = Assert.Single(restored.RetentionScenarioDrafts);
        Assert.Equal(45, reverted.KeepDailyDays);
        Assert.NotSame(draft, reverted);
    }

    [Fact]
    public void MissingCollectionAndUnsupportedFutureDraftSchemaRemainCompatible()
    {
        const string legacySettings = "{\"EnableCloudUpload\":true,\"UnknownSetting\":17}";
        var legacy = JsonConvert.DeserializeObject<GameSaveCenterSettings>(legacySettings)!;
        Assert.Empty(legacy.RetentionScenarioDrafts);

        var future = ValidScenario();
        future.SchemaVersion = RetentionScenarioDefinition.CurrentSchemaVersion + 1;
        var settings = new GameSaveCenterSettings { RetentionScenarioDrafts = new List<RetentionScenarioDefinition> { future } };
        Assert.Empty(settings.RetentionScenarioDrafts);
        Assert.True(legacy.EnableCloudUpload);
    }

    [Fact]
    public void NormalizationTrimsNamesRejectsInvalidRangesAndDeduplicatesIdsAndNames()
    {
        var first = ValidScenario();
        first.Name = "  安全方案  ";
        var sameName = ValidScenario();
        sameName.Name = "安全方案";
        var badRange = ValidScenario();
        badRange.Name = "错误方案";
        badRange.GrowthMinimumMiBPer30Days = 400;
        badRange.GrowthMaximumMiBPer30Days = 200;

        var result = RetentionScenarioDefinition.NormalizeMany(new[] { first, sameName, badRange });

        var only = Assert.Single(result);
        Assert.Equal("安全方案", only.Name);
        Assert.Equal(first.Id, only.Id);
    }

    [Fact]
    public void SavedDraftCountIsBoundedAndCopyGetsANewIdentity()
    {
        var items = Enumerable.Range(0, RetentionScenarioDefinition.MaximumSavedDrafts + 3)
            .Select(index =>
            {
                var item = ValidScenario();
                item.Id = Guid.NewGuid().ToString("N");
                item.Name = $"方案 {index:D2}";
                return item;
            })
            .ToArray();
        var normalized = RetentionScenarioDefinition.NormalizeMany(items);
        Assert.Equal(RetentionScenarioDefinition.MaximumSavedDrafts, normalized.Count);

        var source = normalized[0];
        var copy = source.CopyWithName("副本");
        Assert.NotEqual(source.Id, copy.Id);
        Assert.NotEqual(source.CreatedUtc, copy.CreatedUtc);
        Assert.Equal(source.KeepDailyDays, copy.KeepDailyDays);
        Assert.Equal(source.LastSnapshotUtc, copy.LastSnapshotUtc);
    }

    [Fact]
    public void RenameKeepsIdentityAndAssumptionsWhileRefreshingModificationTime()
    {
        var source = ValidScenario();
        source.LastSnapshotUtc = new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);

        var renamed = source.Rename("  长期保留  ");

        Assert.Equal(source.Id, renamed.Id);
        Assert.Equal(source.CreatedUtc, renamed.CreatedUtc);
        Assert.Equal("长期保留", renamed.Name);
        Assert.Equal(source.KeepDailyDays, renamed.KeepDailyDays);
        Assert.Equal(source.LastSnapshotUtc, renamed.LastSnapshotUtc);
        Assert.True(renamed.UpdatedUtc >= source.UpdatedUtc);
        Assert.Equal("方案", source.Name);
    }

    private static RetentionScenarioDefinition ValidScenario()
        => new()
        {
            Id = "11111111111111111111111111111111",
            Name = "方案",
            CreatedUtc = new DateTime(2026, 10, 18, 0, 0, 0, DateTimeKind.Utc),
            UpdatedUtc = new DateTime(2026, 10, 18, 0, 0, 0, DateTimeKind.Utc),
            KeepAllForHours = 24,
            KeepDailyDays = 30,
            KeepWeeklyWeeks = 12,
            KeepMonthlyMonths = 24,
            GrowthMinimumMiBPer30Days = 100,
            GrowthMaximumMiBPer30Days = 300
        };
}
