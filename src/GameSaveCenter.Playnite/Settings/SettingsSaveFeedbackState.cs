using System;

namespace GameSaveCenter.Playnite.Settings
{
    internal enum SettingsSaveFeedbackStage
    {
        Idle,
        Saving,
        Applying,
        SaveFailed,
        ApplyFailed
    }

    /// <summary>
    /// Small state model for the host save and post-write Worker apply phases.
    /// </summary>
    internal sealed class SettingsSaveFeedbackState
    {
        internal SettingsSaveFeedbackStage Stage { get; private set; }
        internal string FailureMessage { get; private set; } = string.Empty;
        internal bool IsSaving => Stage == SettingsSaveFeedbackStage.Saving;
        internal bool IsApplying => Stage == SettingsSaveFeedbackStage.Applying;
        internal bool HasFailure => Stage == SettingsSaveFeedbackStage.SaveFailed
            || Stage == SettingsSaveFeedbackStage.ApplyFailed;

        internal void BeginSave()
        {
            Stage = SettingsSaveFeedbackStage.Saving;
            FailureMessage = string.Empty;
        }

        internal void BeginApply()
        {
            Stage = SettingsSaveFeedbackStage.Applying;
        }

        internal void CompleteApply()
        {
            Stage = SettingsSaveFeedbackStage.Idle;
            FailureMessage = string.Empty;
        }

        internal void Fail(SettingsSaveFailedEventArgs failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));
            Stage = failure.SettingsPersisted
                ? SettingsSaveFeedbackStage.ApplyFailed
                : SettingsSaveFeedbackStage.SaveFailed;
            FailureMessage = failure.SettingsPersisted
                ? "设置已写入 Playnite，但 Worker 应用失败：" + failure.Exception.Message
                : "设置写入失败，当前编辑仍保留：" + failure.Exception.Message;
        }

        internal void Reset()
        {
            Stage = SettingsSaveFeedbackStage.Idle;
            FailureMessage = string.Empty;
        }

        internal void ResetFailureAfterEdit()
        {
            if (HasFailure) Reset();
        }
    }
}
