using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsPathEditorBehaviorTests
{
    [Fact]
    public void ProbeDistinguishesValidFileDirectoryMissingPathAndFileAsDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-settings-editor-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var executable = Path.Combine(root, "worker.exe");
            var directory = Path.Combine(root, "saves");
            var file = Path.Combine(root, "not-a-directory.dat");
            File.WriteAllText(executable, "worker");
            Directory.CreateDirectory(directory);
            File.WriteAllText(file, "file");

            var executableOption = new SettingsPathEditorOption("WorkerExecutable", "Worker", SettingsPathEditorKind.Executable);
            var directoryOption = new SettingsPathEditorOption("LudusaviBackupDirectory", "存档目录", SettingsPathEditorKind.Directory);

            Assert.True(SettingsPathEditorService.Probe(executableOption, executable).IsValid);
            Assert.True(SettingsPathEditorService.Probe(directoryOption, directory).IsValid);

            var missing = SettingsPathEditorService.Probe(directoryOption, Path.Combine(root, "missing"));
            Assert.False(missing.IsValid);
            Assert.Contains("不存在", missing.Message);

            var fileAsDirectory = SettingsPathEditorService.Probe(directoryOption, file);
            Assert.False(fileAsDirectory.IsValid);
            Assert.Contains("文件", fileAsDirectory.Message);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    [Fact]
    public void EditorCatalogKeepsRemoteTargetOutOfLocalOpenActions()
    {
        Assert.Equal(6, SettingsPathEditorCatalog.Options.Count);
        Assert.DoesNotContain(SettingsPathEditorCatalog.Options, option => option.Key == "RcloneDestination");
        Assert.Contains(SettingsPathEditorCatalog.Options, option => option.Kind == SettingsPathEditorKind.Directory);
        Assert.Contains(SettingsPathEditorCatalog.Options, option => option.Kind == SettingsPathEditorKind.Executable);
    }
}
