namespace GameSaveCenter.Playnite.XamlValues
{
    /// <summary>
    /// BAML-safe enum values for Playnite-hosted views.
    ///
    /// Playnite can load an extension assembly from its private directory while WPF's
    /// BAML resolver uses the default AppDomain context for an assembly-qualified
    /// x:Static reference. Exposing the values as object-returning properties keeps the
    /// XAML tied to the already-loaded plugin assembly; the actual enum objects still
    /// preserve the existing binding and DataTrigger semantics.
    /// </summary>
    public static class BackupStorageFormat
    {
        public static object Simple => global::GameSaveCenter.Contracts.BackupStorageFormat.Simple;
        public static object Zip => global::GameSaveCenter.Contracts.BackupStorageFormat.Zip;
    }

    public static class NotificationLevel
    {
        public static object ImportantOnly => global::GameSaveCenter.Contracts.NotificationLevel.ImportantOnly;
        public static object Summary => global::GameSaveCenter.Contracts.NotificationLevel.Summary;
        public static object Verbose => global::GameSaveCenter.Contracts.NotificationLevel.Verbose;
    }

    public static class MediaKind
    {
        public static object Screenshot => global::GameSaveCenter.Contracts.MediaKind.Screenshot;
        public static object VideoClip => global::GameSaveCenter.Contracts.MediaKind.VideoClip;
    }

    public static class GameToolRiskCategory
    {
        public static object Unknown => global::GameSaveCenter.Contracts.GameToolRiskCategory.Unknown;
    }
}
