using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.Settings
{
    internal enum SettingsPathEditorKind
    {
        Executable,
        Directory
    }

    internal sealed class SettingsPathEditorOption
    {
        internal SettingsPathEditorOption(string key, string displayName, SettingsPathEditorKind kind)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Kind = kind;
        }

        internal string Key { get; }
        internal string DisplayName { get; }
        internal SettingsPathEditorKind Kind { get; }
    }

    internal static class SettingsPathEditorCatalog
    {
        private static readonly IReadOnlyList<SettingsPathEditorOption> options = new[]
        {
            new SettingsPathEditorOption("WorkerExecutable", "Worker 可执行文件", SettingsPathEditorKind.Executable),
            new SettingsPathEditorOption("LudusaviExecutable", "Ludusavi 可执行文件", SettingsPathEditorKind.Executable),
            new SettingsPathEditorOption("LudusaviBackupDirectory", "存档目录", SettingsPathEditorKind.Directory),
            new SettingsPathEditorOption("RcloneExecutable", "Rclone 可执行文件", SettingsPathEditorKind.Executable),
            new SettingsPathEditorOption("MediaArchiveDirectory", "媒体目录", SettingsPathEditorKind.Directory),
            new SettingsPathEditorOption("LocalMirrorPath", "本地镜像目录", SettingsPathEditorKind.Directory)
        };

        internal static IReadOnlyList<SettingsPathEditorOption> Options => options;
    }
}
