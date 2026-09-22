using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsImportPreviewSourceTests
{
    [Fact]
    public void SettingsImportShowsPreviewBeforeAnyLiveCopy()
    {
        var root = TestRepositoryContext.Root;
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var preview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "SettingsImportPreview.cs"));

        var previewIndex = code.IndexOf("var preview = settings.PreviewPortableJson(json)", StringComparison.Ordinal);
        var confirmIndex = code.IndexOf("ConfirmSettingsImport(preview)", previewIndex, StringComparison.Ordinal);
        var applyIndex = code.IndexOf("settings.ApplyPortableJson(preview)", confirmIndex, StringComparison.Ordinal);
        Assert.True(previewIndex >= 0);
        Assert.True(confirmIndex > previewIndex);
        Assert.True(applyIndex > confirmIndex);
        Assert.DoesNotContain("var report = settings.ImportPortableJson(json)", code, StringComparison.Ordinal);

        Assert.Contains("未知字段将被忽略", preview);
        Assert.Contains("可分享导出不包含凭据", preview);
        Assert.Contains("var snapshot = Clone()", settings);
        Assert.Contains("CopyFrom(snapshot)", settings);
    }
}
