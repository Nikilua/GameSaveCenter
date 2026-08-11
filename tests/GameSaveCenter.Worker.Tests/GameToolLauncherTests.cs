using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolLauncherTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));

    public GameToolLauncherTests()
    {
        Directory.CreateDirectory(root);
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); }
        catch { }
    }

    [Fact]
    public void CustomExe_BuildsTrackableStartInfoWithArgumentsWorkingDirectoryAndAdmin()
    {
        var path = CreateFile("tool.exe");
        var plan = GameToolLauncher.Build(Custom(path, arguments: "--fix", workingDirectory: root, requiresAdmin: true), FakeResolver());

        Assert.True(plan.Trackable);
        Assert.Equal(path, plan.StartInfo.FileName);
        Assert.Equal("--fix", plan.StartInfo.Arguments);
        Assert.Equal(root, plan.StartInfo.WorkingDirectory);
        Assert.Equal("runas", plan.StartInfo.Verb);
        Assert.True(plan.StartInfo.UseShellExecute);
    }

    [Fact]
    public void Shortcut_ResolvesToExeAndTracksResolvedTarget()
    {
        var shortcut = CreateFile("launcher.lnk");
        var target = CreateFile("LosslessScaling.exe");
        var resolver = new FakeShortcutResolver(new ShortcutTarget(target, "--windowed", root));
        var plan = GameToolLauncher.Build(Custom(shortcut, arguments: "--user"), resolver);

        Assert.True(plan.Trackable);
        Assert.Equal(target, plan.StartInfo.FileName);
        Assert.Equal("--windowed --user", plan.StartInfo.Arguments);
    }

    [Fact]
    public void Shortcut_ToShellDocumentIsNotTrackable()
    {
        var shortcut = CreateFile("docs.lnk");
        var target = CreateFile("notes.txt");
        var resolver = new FakeShortcutResolver(new ShortcutTarget(target, string.Empty, root));
        var plan = GameToolLauncher.Build(Custom(shortcut), resolver);

        Assert.False(plan.Trackable);
        Assert.Equal(target, plan.StartInfo.FileName);
        Assert.True(plan.StartInfo.UseShellExecute);
    }

    [Theory]
    [InlineData("fix.bat")]
    [InlineData("fix.cmd")]
    public void BatchScript_UsesCmdAndIsNotTrackable(string fileName)
    {
        var path = CreateFile(fileName);
        var plan = GameToolLauncher.Build(Custom(path, arguments: "/quiet"), FakeResolver());

        Assert.False(plan.Trackable);
        Assert.Equal("cmd.exe", plan.StartInfo.FileName);
        Assert.Contains("/d /s /c", plan.StartInfo.Arguments, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(path, plan.StartInfo.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void PowerShellScript_UsesPowerShellAndIsNotTrackable()
    {
        var path = CreateFile("launch.ps1");
        var plan = GameToolLauncher.Build(Custom(path, arguments: "-Name Test"), FakeResolver());

        Assert.False(plan.Trackable);
        Assert.Equal("powershell.exe", plan.StartInfo.FileName);
        Assert.Contains("-NoProfile -ExecutionPolicy Bypass -File", plan.StartInfo.Arguments, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(path, plan.StartInfo.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellDocument_UsesDefaultProgramAndIsNotTrackable()
    {
        var path = CreateFile("notes.txt");
        var plan = GameToolLauncher.Build(Custom(path), FakeResolver());

        Assert.False(plan.Trackable);
        Assert.Equal(path, plan.StartInfo.FileName);
        Assert.True(plan.StartInfo.UseShellExecute);
    }

    [Fact]
    public void MissingFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => GameToolLauncher.Build(Custom(Path.Combine(root, "missing.exe")), FakeResolver()));
    }

    private static IShortcutResolver FakeResolver() => new FakeShortcutResolver(new ShortcutTarget(string.Empty, string.Empty, string.Empty));

    private static GameToolDto Custom(string path, string arguments = "", string workingDirectory = "", bool requiresAdmin = false)
    {
        var tool = new GameToolDto
        {
            ToolId = "tool",
            PlayniteId = "game",
            ToolType = GameToolType.CustomExecutable,
            DisplayName = "Custom",
            ActiveVersionId = "v1",
            RequiresAdmin = requiresAdmin
        };
        tool.Versions.Add(new GameToolVersionDto
        {
            VersionId = "v1",
            ToolId = tool.ToolId,
            EntryPath = path,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            IsAvailable = File.Exists(path)
        });
        return tool;
    }

    private string CreateFile(string name)
    {
        var path = Path.Combine(root, name);
        File.WriteAllText(path, "test");
        return path;
    }

    private sealed class FakeShortcutResolver : IShortcutResolver
    {
        private readonly ShortcutTarget target;

        public FakeShortcutResolver(ShortcutTarget target)
        {
            this.target = target;
        }

        public ShortcutTarget Resolve(string shortcutPath) => target;
    }
}
