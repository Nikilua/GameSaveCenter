using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsEffectSourceTests
{
    [Fact]
    public void SettingsHintsMatchTheExistingSaveAndApplyBoundaries()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));

        Assert.Contains("SettingsGeneralEffectHint", view);
        Assert.Contains("SettingsBackupEffectHint", view);
        Assert.Contains("SettingsAppearanceEffectHint", view);
        Assert.Contains("SettingsAutomationEffectHint", view);
        Assert.Contains("下一次新任务", view);
        Assert.Contains("下一次 Playnite 启动", view);
        Assert.Contains("不需要重启 Playnite", view);
        Assert.Contains("本页没有笼统的需重启项", view);

        var endEdit = Slice(settings, "public void EndEdit()", "public string CreateSettingsFingerprint()");
        Assert.Contains("plugin.SavePluginSettings(this)", endEdit);
        Assert.Contains("plugin.NotifyVisualSettingsChanged()", endEdit);
        Assert.Contains("plugin.ApplySettingsAsync", endEdit);
        Assert.Contains("MessageTypes.UpdateSettings", plugin);
        Assert.Contains("_options.Apply(settings,persist:true)", worker);
        Assert.Contains("_healthInspection.SyncPlanAsync", worker);
    }

    [Fact]
    public void EffectCopyDoesNotInventAPlayniteRestartRequirement()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        Assert.DoesNotContain("所有修改都需要重启", view, StringComparison.Ordinal);
        Assert.DoesNotContain("保存后必须重启", view, StringComparison.Ordinal);
    }

    private static string Slice(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(start >= 0);
        Assert.True(end > start);
        return source.Substring(start, end - start);
    }
}
