using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

/// <summary>Resolved shortcut target so launch and process tracking use the real executable.</summary>
public sealed record ShortcutTarget(string TargetPath, string Arguments, string WorkingDirectory);

public interface IShortcutResolver
{
    ShortcutTarget Resolve(string shortcutPath);
}

/// <summary>Reads .lnk metadata through Windows Script Host without a third-party package.</summary>
public sealed class WindowsShortcutResolver : IShortcutResolver
{
    public ShortcutTarget Resolve(string shortcutPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows Script Host 不可用，无法解析快捷方式。");
        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("无法创建 Windows Script Host 实例。");
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        try
        {
            return new ShortcutTarget(
                shortcut.TargetPath as string ?? string.Empty,
                shortcut.Arguments as string ?? string.Empty,
                shortcut.WorkingDirectory as string ?? string.Empty);
        }
        finally
        {
            Marshal.FinalReleaseComObject(shortcut);
        }
    }
}

/// <summary>Ready-to-start launch plan with an explicit process-tracking capability flag.</summary>
public sealed record LaunchPlan(ProcessStartInfo StartInfo, bool Trackable, string Kind);

/// <summary>
/// Builds Windows launch strategies for trainers, Cheat Engine tables and custom launch
/// items. Custom items keep their external path reference; EXEs and resolved shortcut
/// targets are trackable, while scripts and shell documents are not reliably closeable.
/// </summary>
public static class GameToolLauncher
{
    public static LaunchPlan Build(GameToolDto tool, IShortcutResolver? shortcutResolver = null)
    {
        var version = tool.ActiveVersion;
        if (version == null || string.IsNullOrWhiteSpace(version.EntryPath) || !File.Exists(version.EntryPath))
            throw new FileNotFoundException("工具文件不存在，可能已被移动或安全软件隔离。", version?.EntryPath ?? string.Empty);

        if (tool.ToolType != GameToolType.CustomExecutable)
        {
            if (tool.ToolType != GameToolType.CheatTable
                && !version.EntryPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("修改器启动文件必须是 EXE。");
            return new LaunchPlan(
                CreateExecutableStartInfo(version.EntryPath, version.Arguments, ResolveWorkingDirectory(version.WorkingDirectory, version.EntryPath), tool.RequiresAdmin),
                true,
                tool.ToolType == GameToolType.CheatTable ? "cheat-table" : "trainer");
        }

        var kind = GameToolLaunchKinds.FromPath(version.EntryPath);
        var workingDirectory = ResolveWorkingDirectory(version.WorkingDirectory, version.EntryPath);
        switch (kind)
        {
            case GameToolLaunchKind.Executable:
                return new LaunchPlan(
                    CreateExecutableStartInfo(version.EntryPath, version.Arguments, workingDirectory, tool.RequiresAdmin),
                    true,
                    "custom-exe");
            case GameToolLaunchKind.Shortcut:
            {
                var resolver = shortcutResolver ?? new WindowsShortcutResolver();
                var target = resolver.Resolve(version.EntryPath);
                if (string.IsNullOrWhiteSpace(target.TargetPath))
                    throw new InvalidOperationException("快捷方式目标为空，可能已经损坏。");
                if (GameToolLaunchKinds.FromPath(target.TargetPath) == GameToolLaunchKind.Shortcut)
                    throw new InvalidOperationException("快捷方式不能再次指向快捷方式。");
                var targetWorkingDirectory = ResolveWorkingDirectory(version.WorkingDirectory, target.WorkingDirectory, target.TargetPath);
                var combinedArguments = CombineArguments(target.Arguments, version.Arguments);
                if (GameToolLaunchKinds.FromPath(target.TargetPath) == GameToolLaunchKind.Executable)
                {
                    return new LaunchPlan(
                        CreateExecutableStartInfo(target.TargetPath, combinedArguments, targetWorkingDirectory, tool.RequiresAdmin),
                        true,
                        "shortcut-exe");
                }
                return new LaunchPlan(
                    CreateShellStartInfo(target.TargetPath, combinedArguments, targetWorkingDirectory),
                    false,
                    "shortcut-shell");
            }
            case GameToolLaunchKind.BatchScript:
                return new LaunchPlan(
                    CreateScriptStartInfo("cmd.exe", BuildCmdArguments(version.EntryPath, version.Arguments), workingDirectory, tool.RequiresAdmin),
                    false,
                    "batch");
            case GameToolLaunchKind.PowerShellScript:
                return new LaunchPlan(
                    CreateScriptStartInfo("powershell.exe", BuildPowerShellArguments(version.EntryPath, version.Arguments), workingDirectory, tool.RequiresAdmin),
                    false,
                    "powershell");
            default:
                return new LaunchPlan(
                    CreateShellStartInfo(version.EntryPath, version.Arguments, workingDirectory),
                    false,
                    "shell-document");
        }
    }

    private static ProcessStartInfo CreateExecutableStartInfo(string path, string arguments, string workingDirectory, bool requiresAdmin)
    {
        var start = new ProcessStartInfo
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };
        if (requiresAdmin) start.Verb = "runas";
        return start;
    }

    private static ProcessStartInfo CreateShellStartInfo(string path, string arguments, string workingDirectory)
        => new()
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };

    private static ProcessStartInfo CreateScriptStartInfo(string fileName, string arguments, string workingDirectory, bool requiresAdmin)
    {
        var start = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };
        if (requiresAdmin) start.Verb = "runas";
        return start;
    }

    private static string BuildCmdArguments(string scriptPath, string? arguments)
    {
        var commandLine = QuotePath(scriptPath);
        if (!string.IsNullOrWhiteSpace(arguments)) commandLine += " " + arguments;
        return "/d /s /c \"" + commandLine + "\"";
    }

    private static string BuildPowerShellArguments(string scriptPath, string? arguments)
    {
        var builder = new StringBuilder();
        builder.Append("-NoProfile -ExecutionPolicy Bypass -File ").Append(QuotePath(scriptPath));
        if (!string.IsNullOrWhiteSpace(arguments)) builder.Append(' ').Append(arguments);
        return builder.ToString();
    }

    private static string QuotePath(string path)
        => "\"" + (path ?? string.Empty).Replace("\"", "\"\"") + "\"";

    private static string CombineArguments(string first, string second)
    {
        var left = (first ?? string.Empty).Trim();
        var right = (second ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(left)) return right;
        if (string.IsNullOrEmpty(right)) return left;
        return left + " " + right;
    }

    private static string ResolveWorkingDirectory(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            var expanded = Environment.ExpandEnvironmentVariables(candidate);
            if (Directory.Exists(expanded)) return expanded;
        }
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            var expanded = Environment.ExpandEnvironmentVariables(candidate);
            if (File.Exists(expanded)) return Path.GetDirectoryName(expanded) ?? string.Empty;
            if (expanded.Contains(Path.DirectorySeparatorChar)) return Path.GetDirectoryName(expanded) ?? string.Empty;
        }
        return string.Empty;
    }
}
