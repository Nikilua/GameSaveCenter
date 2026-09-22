using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsPathEditorSourceTests
{
    [Fact]
    public void SettingsSurfaceUsesOneCurrentFieldEditorAndStrictOpenGuard()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var service = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "SettingsPathEditorService.cs"));

        Assert.Contains("SettingsPathEditorComboBox", view);
        Assert.Contains("OnSettingsPathBrowseClick", view);
        Assert.Contains("OnSettingsPathValidateClick", view);
        Assert.Contains("OnSettingsPathOpenClick", view);
        Assert.Contains("OnSettingsPathCopyClick", view);
        Assert.Contains("FolderBrowserDialog", code);
        Assert.Contains("ClipboardRetry.TrySetTextAsync", code);
        Assert.Contains("SettingsPathEditorService.Probe", code);
        Assert.Contains("if (!probe.IsValid)", code);
        Assert.Contains("UnauthorizedAccessException", service);
        Assert.Contains("Directory.GetFileSystemEntries", service);

        var openStart = code.IndexOf("private void OnSettingsPathOpenClick", StringComparison.Ordinal);
        var openEnd = code.IndexOf("private void OnSettingsPathCopyClick", openStart, StringComparison.Ordinal);
        Assert.True(openStart >= 0);
        Assert.True(openEnd > openStart);
        var openMethod = code.Substring(openStart, openEnd - openStart);
        Assert.DoesNotContain("Path.GetDirectoryName", openMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("parent", openMethod, StringComparison.OrdinalIgnoreCase);
    }
}
