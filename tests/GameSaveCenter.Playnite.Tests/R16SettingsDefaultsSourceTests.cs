using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsDefaultsSourceTests
{
    [Fact]
    public void SettingsSurfaceWiresAllResetScopesWithoutEndingPlayniteEdit()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var catalog = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "SettingsResetCatalog.cs"));

        Assert.Contains("SettingsResetFieldComboBox", view);
        Assert.Contains("OnResetSingleFieldClick", view);
        Assert.Contains("OnResetGeneralDefaultsClick", view);
        Assert.Contains("OnResetBackupDefaultsClick", view);
        Assert.Contains("OnResetAppearanceDefaultsClick", view);
        Assert.Contains("OnResetAutomationDefaultsClick", view);
        Assert.Contains("OnResetAllDefaultsClick", view);
        Assert.Contains("SettingsResetCatalog.ResetField", code);
        Assert.Contains("SettingsResetCatalog.ResetCategory", code);
        Assert.Contains("SettingsResetCatalog.ResetAll", code);
        Assert.Contains("DataContext = null", code);
        Assert.Contains("不会被清空", catalog);

        var refreshStart = code.IndexOf("private void RefreshSettingsAfterDraftReset", StringComparison.Ordinal);
        var refreshEnd = code.IndexOf("private ValidationFieldTarget? ResolveValidationTarget", refreshStart, StringComparison.Ordinal);
        Assert.True(refreshStart >= 0);
        Assert.True(refreshEnd > refreshStart);
        Assert.DoesNotContain("EndEdit", code.Substring(refreshStart, refreshEnd - refreshStart), StringComparison.Ordinal);
    }
}
