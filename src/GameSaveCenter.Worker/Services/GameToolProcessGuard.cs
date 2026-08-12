using System.Diagnostics;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

/// <summary>Path-based process guard for custom tools. It never kills by name alone.</summary>
public static class GameToolProcessGuard
{
    public sealed record ScanResult(IReadOnlyList<int> MatchingProcessIds, bool HasUnreadableCandidate);

    public static string? ResolveExecutableTarget(GameToolDto tool, IShortcutResolver shortcutResolver)
    {
        if (tool.ToolType != GameToolType.CustomExecutable)
            return null;

        var version = tool.ActiveVersion;
        var candidate = string.IsNullOrWhiteSpace(version.ResolvedTargetPath)
            ? version.EntryPath
            : version.ResolvedTargetPath;
        if (GameToolLaunchKinds.FromPath(candidate) == GameToolLaunchKind.Executable)
            return NormalizePath(candidate);
        if (GameToolLaunchKinds.FromPath(version.EntryPath) != GameToolLaunchKind.Shortcut)
            return null;

        var target = shortcutResolver.Resolve(version.EntryPath).TargetPath;
        return GameToolLaunchKinds.FromPath(target) == GameToolLaunchKind.Executable
            ? NormalizePath(target)
            : null;
    }

    public static ScanResult Scan(string targetPath)
    {
        var normalizedTarget = NormalizePath(targetPath);
        var processName = Path.GetFileNameWithoutExtension(normalizedTarget);
        if (string.IsNullOrWhiteSpace(processName))
            return new ScanResult(Array.Empty<int>(), true);

        var matching = new List<int>();
        var unreadable = false;
        foreach (var process in Process.GetProcessesByName(processName))
        {
            try
            {
                var imagePath = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    unreadable = true;
                }
                else if (PathsEqual(imagePath, normalizedTarget))
                {
                    matching.Add(process.Id);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                // A same-name process whose image path cannot be inspected is ambiguous.
                unreadable = true;
            }
            finally
            {
                process.Dispose();
            }
        }
        return new ScanResult(matching, unreadable);
    }

    public static void RestartExact(string targetPath, ScanResult scan, TimeSpan wait)
    {
        foreach (var processId in scan.MatchingProcessIds)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                if (!IsExactProcess(process, targetPath))
                    continue;
                if (!process.HasExited)
                {
                    _ = process.CloseMainWindow();
                    if (!process.WaitForExit((int)wait.TotalMilliseconds) && !process.HasExited)
                    {
                        // Re-check the image path immediately before the only forceful action.
                        if (!IsExactProcess(process, targetPath))
                            throw new InvalidOperationException("进程路径在重启前发生变化，已停止操作。");
                        process.Kill(entireProcessTree: false);
                        process.WaitForExit((int)wait.TotalMilliseconds);
                    }
                }
            }
            catch (ArgumentException)
            {
                // The instance exited during the guarded operation.
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("exited", StringComparison.OrdinalIgnoreCase))
            {
                // The instance exited during the guarded operation.
            }
        }
    }

    public static bool IsExactProcess(Process process, string targetPath)
    {
        try
        {
            return !process.HasExited
                && !string.IsNullOrWhiteSpace(process.MainModule?.FileName)
                && PathsEqual(process.MainModule!.FileName, targetPath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    public static bool PathsEqual(string left, string right)
        => string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path)
        => Path.GetFullPath(Environment.ExpandEnvironmentVariables(path ?? string.Empty))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
