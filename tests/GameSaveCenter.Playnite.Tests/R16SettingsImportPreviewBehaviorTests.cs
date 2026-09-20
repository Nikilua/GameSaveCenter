using System;
using System.IO;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsImportPreviewBehaviorTests
{
    [Fact]
    public void PreviewShowsChangesAndUnknownFieldsWithoutMutatingTheDraft()
    {
        var current = new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\Before\GameSaveCenter.Worker.exe",
            ThemeMode = GameSaveCenterThemeMode.FollowPlaynite
        };
        var incoming = new GameSaveCenterSettings
        {
            WorkerExecutable = @"D:\Portable\GameSaveCenter.Worker.exe",
            ThemeMode = GameSaveCenterThemeMode.Dark
        };
        var package = JObject.Parse(incoming.ExportPortableJson());
        ((JObject)package["Settings"]!)["LegacyUnknownField"] = "ignored";
        var json = package.ToString();
        var before = current.CreateSettingsFingerprint();

        var preview = current.PreviewPortableJson(json);

        Assert.True(preview.IsCompatible);
        Assert.Contains(preview.FieldsToOverwrite, field => field.PropertyName == nameof(GameSaveCenterSettings.WorkerExecutable));
        Assert.Contains(preview.FieldsToOverwrite, field => field.PropertyName == nameof(GameSaveCenterSettings.ThemeMode));
        Assert.Contains("Settings.LegacyUnknownField", preview.UnknownFields);
        Assert.Equal(before, current.CreateSettingsFingerprint());
    }

    [Fact]
    public void ApplyUsesConfirmedPreviewAndUnknownFieldsDoNotDamageTheDraft()
    {
        var incoming = new GameSaveCenterSettings
        {
            WorkerExecutable = @"D:\Portable\GameSaveCenter.Worker.exe",
            ThemeMode = GameSaveCenterThemeMode.Dark
        };
        var package = JObject.Parse(incoming.ExportPortableJson());
        ((JObject)package["Settings"]!)["LegacyUnknownField"] = "ignored";
        var current = new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\Before\GameSaveCenter.Worker.exe"
        };

        var report = current.ApplyPortableJson(current.PreviewPortableJson(package.ToString()));

        Assert.Equal(1, report.SchemaVersion);
        Assert.Equal(incoming.WorkerExecutable, current.WorkerExecutable);
        Assert.Equal(incoming.ThemeMode, current.ThemeMode);
    }

    [Fact]
    public void UnsupportedOrInvalidPreviewCannotChangeTheOriginalSettings()
    {
        var current = new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\Before\GameSaveCenter.Worker.exe",
            DefaultBackupIntervalMinutes = 45
        };
        var before = current.CreateSettingsFingerprint();

        var unsupported = JObject.Parse(current.ExportPortableJson());
        unsupported["SchemaVersion"] = 99;
        var unsupportedPreview = current.PreviewPortableJson(unsupported.ToString());
        Assert.False(unsupportedPreview.IsCompatible);
        Assert.Throws<InvalidDataException>(() => current.ApplyPortableJson(unsupportedPreview));
        Assert.Equal(before, current.CreateSettingsFingerprint());

        var invalid = JObject.Parse(current.ExportPortableJson());
        invalid.SelectToken("Settings.DefaultBackupIntervalMinutes")!.Replace(0);
        var invalidPreview = current.PreviewPortableJson(invalid.ToString());
        Assert.False(invalidPreview.IsCompatible);
        Assert.Throws<InvalidDataException>(() => current.ApplyPortableJson(invalidPreview));
        Assert.Equal(before, current.CreateSettingsFingerprint());
    }

    [Fact]
    public void PortableExportHasNoCredentialFieldsOrDeviceIdentity()
    {
        var settings = new GameSaveCenterSettings { DeviceId = "11111111111111111111111111111111" };

        var package = JObject.Parse(settings.ExportPortableJson());
        var serialized = package.ToString();

        Assert.Null(package.SelectToken("Settings.RclonePassword"));
        Assert.DoesNotContain("Password", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Token", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, package.SelectToken("Settings.DeviceId")!.Value<string>());
    }
}
