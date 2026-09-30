using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsConflictBehaviorTests
{
    [Fact]
    public void ThreeWayMergeKeepsDraftAndMergesNonConflictingPersistedChanges()
    {
        var baseline = CreateSettings();
        var draft = Clone(baseline);
        var persisted = Clone(baseline);
        draft.WorkerExecutable = @"C:\User\GameSaveCenter.Worker.exe";
        persisted.ThemeMode = GameSaveCenterThemeMode.Dark;

        var resolution = SettingsConflictResolver.Merge(baseline, draft, persisted);

        Assert.False(resolution.HasConflicts);
        Assert.True(resolution.HasExternalChanges);
        Assert.Equal(@"C:\User\GameSaveCenter.Worker.exe", draft.WorkerExecutable);
        Assert.Equal(GameSaveCenterThemeMode.Dark, draft.ThemeMode);
    }

    [Fact]
    public void ThreeWayMergeReportsSameFieldConflictWithoutPartiallyOverwritingDraft()
    {
        var baseline = CreateSettings();
        var draft = Clone(baseline);
        var persisted = Clone(baseline);
        draft.RcloneDestination = "user-remote:GameSaveCenter";
        persisted.RcloneDestination = "background-remote:GameSaveCenter";

        var resolution = SettingsConflictResolver.Merge(baseline, draft, persisted);

        Assert.True(resolution.HasConflicts);
        Assert.Contains(resolution.Conflicts, field => field.PropertyName == nameof(GameSaveCenterSettings.RcloneDestination));
        Assert.Equal("user-remote:GameSaveCenter", draft.RcloneDestination);
    }

    [Fact]
    public void ConflictSummaryNamesTheChangedFieldAndRefusesSilentSave()
    {
        var resolution = new SettingsConflictResolution(
            new[] { new SettingsConflictField(nameof(GameSaveCenterSettings.ThemeMode), "主题") },
            new SettingsConflictField[0]);
        var args = new SettingsConflictDetectedEventArgs(resolution);

        Assert.Equal("设置保存冲突：后台或另一个设置入口已修改字段「主题」。当前草稿未写入，请检查后再次保存或取消。", args.Summary);
        Assert.Throws<SettingsConflictException>(() => ThrowConflict(args.Summary));
    }

    private static GameSaveCenterSettings CreateSettings()
        => new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\Before\GameSaveCenter.Worker.exe",
            RcloneDestination = "before-remote:GameSaveCenter",
            ThemeMode = GameSaveCenterThemeMode.FollowPlaynite
        };

    private static GameSaveCenterSettings Clone(GameSaveCenterSettings source)
        => JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(source))!;

    private static void ThrowConflict(string summary)
        => throw new SettingsConflictException(summary);
}
