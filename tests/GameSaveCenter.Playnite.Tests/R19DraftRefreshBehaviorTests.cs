using System;
using System.Reflection;
using System.Runtime.Serialization;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R19DraftRefreshBehaviorTests
{
    [Fact]
    public void SaveRefreshFailureAndSameVersionSuccessKeepDirtyFieldsButApplyCleanFields()
    {
        var viewModel = CreateBareViewModel();
        SetField(viewModel, "backupComment", "用户存档草稿");
        SetField(viewModel, "lockSelectedBackup", false);
        SetField(viewModel, "backupCommentDirty", true);
        SetField(viewModel, "backupLockDirty", true);
        SetField(viewModel, "<Backups>k__BackingField", new BatchObservableCollection<BackupVersionDto>());
        SetField(viewModel, "<SaveCandidates>k__BackingField", new BatchObservableCollection<SavePathCandidateDto>());

        Invoke(viewModel, "FailSaveDetailsLoad", new InvalidOperationException("合成刷新失败"));
        Assert.Equal("用户存档草稿", GetField<string>(viewModel, "backupComment"));
        Assert.False(GetField<bool>(viewModel, "lockSelectedBackup"));
        Assert.True(GetField<bool>(viewModel, "backupCommentDirty"));
        Assert.True(GetField<bool>(viewModel, "backupLockDirty"));

        Invoke(viewModel, "SyncBackupEditor", new BackupVersionDto
        {
            BackupId = "backup-a",
            Comment = "服务端新备注",
            IsLocked = true
        }, true);

        Assert.Equal("用户存档草稿", GetField<string>(viewModel, "backupComment"));
        Assert.False(GetField<bool>(viewModel, "lockSelectedBackup"));
        Assert.True(GetField<bool>(viewModel, "backupCommentDirty"));
        Assert.True(GetField<bool>(viewModel, "backupLockDirty"));

        SetField(viewModel, "backupCommentDirty", false);
        SetField(viewModel, "backupLockDirty", false);
        Invoke(viewModel, "SyncBackupEditor", new BackupVersionDto
        {
            BackupId = "backup-a",
            Comment = "服务端新备注",
            IsLocked = true
        }, true);

        Assert.Equal("服务端新备注", GetField<string>(viewModel, "backupComment"));
        Assert.True(GetField<bool>(viewModel, "lockSelectedBackup"));
        Assert.False(GetField<bool>(viewModel, "backupCommentDirty"));
        Assert.False(GetField<bool>(viewModel, "backupLockDirty"));
    }

    [Fact]
    public void MediaRefreshFailureAndSameMediaSuccessKeepDirtyFieldsButApplyCleanFields()
    {
        var viewModel = CreateBareViewModel();
        SetField(viewModel, "mediaComment", "用户媒体草稿");
        SetField(viewModel, "mediaFavorite", false);
        SetField(viewModel, "mediaCommentDirty", true);
        SetField(viewModel, "mediaFavoriteDirty", true);
        SetField(viewModel, "<Media>k__BackingField", new BatchObservableCollection<MediaItemDto>());
        SetField(viewModel, "mediaDetailsStateCache", new MediaWorkspaceStateCache());
        SetField(viewModel, "gamePicker", new GamePickerViewModel());

        Invoke(viewModel, "FailMediaDetailsLoad", new InvalidOperationException("合成媒体刷新失败"), 0L);
        Assert.Equal("用户媒体草稿", GetField<string>(viewModel, "mediaComment"));
        Assert.False(GetField<bool>(viewModel, "mediaFavorite"));
        Assert.True(GetField<bool>(viewModel, "mediaCommentDirty"));
        Assert.True(GetField<bool>(viewModel, "mediaFavoriteDirty"));

        Invoke(viewModel, "SyncMediaEditor", new MediaItemDto
        {
            MediaId = "media-a",
            Comment = "服务端媒体备注",
            IsFavorite = true
        }, true);

        Assert.Equal("用户媒体草稿", GetField<string>(viewModel, "mediaComment"));
        Assert.False(GetField<bool>(viewModel, "mediaFavorite"));
        Assert.True(GetField<bool>(viewModel, "mediaCommentDirty"));
        Assert.True(GetField<bool>(viewModel, "mediaFavoriteDirty"));

        SetField(viewModel, "mediaCommentDirty", false);
        SetField(viewModel, "mediaFavoriteDirty", false);
        Invoke(viewModel, "SyncMediaEditor", new MediaItemDto
        {
            MediaId = "media-a",
            Comment = "服务端媒体备注",
            IsFavorite = true
        }, true);

        Assert.Equal("服务端媒体备注", GetField<string>(viewModel, "mediaComment"));
        Assert.True(GetField<bool>(viewModel, "mediaFavorite"));
        Assert.False(GetField<bool>(viewModel, "mediaCommentDirty"));
        Assert.False(GetField<bool>(viewModel, "mediaFavoriteDirty"));
    }

    private static DashboardViewModel CreateBareViewModel()
        => (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));

    private static void Invoke(object target, string methodName, params object?[] arguments)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(target, arguments);
    }

    private static T GetField<T>(object target, string name)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (T)field!.GetValue(target)!;
    }

    private static void SetField(object target, string name, object? value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }
}
