using System;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// Immutable path values captured for one settings validation request.
    /// </summary>
    internal sealed class SettingsPathValidationSnapshot
    {
        internal SettingsPathValidationSnapshot(
            string workerExecutable,
            string ludusaviExecutable,
            string ludusaviBackupDirectory,
            string rcloneExecutable,
            string mediaArchiveDirectory,
            bool enableLocalMirror,
            string localMirrorPath)
        {
            WorkerExecutable = workerExecutable ?? string.Empty;
            LudusaviExecutable = ludusaviExecutable ?? string.Empty;
            LudusaviBackupDirectory = ludusaviBackupDirectory ?? string.Empty;
            RcloneExecutable = rcloneExecutable ?? string.Empty;
            MediaArchiveDirectory = mediaArchiveDirectory ?? string.Empty;
            EnableLocalMirror = enableLocalMirror;
            LocalMirrorPath = localMirrorPath ?? string.Empty;
        }

        internal string WorkerExecutable { get; }
        internal string LudusaviExecutable { get; }
        internal string LudusaviBackupDirectory { get; }
        internal string RcloneExecutable { get; }
        internal string MediaArchiveDirectory { get; }
        internal bool EnableLocalMirror { get; }
        internal string LocalMirrorPath { get; }
    }
}
