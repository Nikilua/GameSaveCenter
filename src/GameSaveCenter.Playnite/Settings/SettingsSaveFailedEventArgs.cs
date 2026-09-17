using System;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Describes a settings write or post-write Worker apply failure.</summary>
    public sealed class SettingsSaveFailedEventArgs : EventArgs
    {
        public SettingsSaveFailedEventArgs(Exception exception, bool settingsPersisted)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            SettingsPersisted = settingsPersisted;
        }

        public Exception Exception { get; }
        public bool SettingsPersisted { get; }
    }
}
