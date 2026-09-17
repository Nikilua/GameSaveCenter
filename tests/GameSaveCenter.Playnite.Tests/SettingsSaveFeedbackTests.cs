using System;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsSaveFeedbackTests
{
    [Fact]
    public void SaveFeedbackDistinguishesWriteFailureFromWorkerApplyFailure()
    {
        var state = new SettingsSaveFeedbackState();
        state.BeginSave();
        Assert.True(state.IsSaving);

        state.BeginApply();
        Assert.True(state.IsApplying);
        Assert.False(state.HasFailure);

        state.Fail(new SettingsSaveFailedEventArgs(
            new InvalidOperationException("pipe unavailable"),
            settingsPersisted: true));
        Assert.Equal(SettingsSaveFeedbackStage.ApplyFailed, state.Stage);
        Assert.Contains("已写入 Playnite", state.FailureMessage);

        state.ResetFailureAfterEdit();
        Assert.Equal(SettingsSaveFeedbackStage.Idle, state.Stage);
        Assert.False(state.HasFailure);

        state.BeginSave();
        state.Fail(new SettingsSaveFailedEventArgs(
            new InvalidOperationException("settings file locked"),
            settingsPersisted: false));
        Assert.Equal(SettingsSaveFeedbackStage.SaveFailed, state.Stage);
        Assert.Contains("当前编辑仍保留", state.FailureMessage);
    }

    [Fact]
    public void SuccessfulWorkerApplyReturnsToIdleAndClearsFailureText()
    {
        var state = new SettingsSaveFeedbackState();
        state.BeginSave();
        state.BeginApply();
        state.CompleteApply();

        Assert.Equal(SettingsSaveFeedbackStage.Idle, state.Stage);
        Assert.False(state.IsSaving);
        Assert.False(state.IsApplying);
        Assert.False(state.HasFailure);
        Assert.Equal(string.Empty, state.FailureMessage);
    }
}
