using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsConflictSourceTests
{
    [Fact]
    public void EndEditChecksPersistedBaselineAndTheViewPresentsConflictInsteadOfSaving()
    {
        var root = TestRepositoryContext.Root;
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var resolver = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "SettingsConflictResolver.cs"));

        Assert.Contains("DetectAndReportSettingsConflict()", settings);
        Assert.Contains("plugin.LoadPluginSettings<GameSaveCenterSettings>()", settings);
        Assert.Contains("SettingsConflictDetected?.Invoke(this, args)", settings);
        Assert.Contains("throw new SettingsConflictException(args.Summary)", settings);
        Assert.Contains("SettingsConflictDetected += OnSettingsConflictDetected", view);
        Assert.Contains("设置保存冲突", view);
        Assert.Contains("internal static SettingsConflictResolution Merge", resolver);
        Assert.Contains("if (conflicts.Count == 0)", resolver);
        Assert.DoesNotContain("CopyFrom(persisted)", settings, StringComparison.Ordinal);
    }
}
