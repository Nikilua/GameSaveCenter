using System.Diagnostics;
using System.Reflection;

namespace GameSaveCenter.Worker.Tests;

internal static class TestRepositoryContext
{
    private const string SourceRootMetadataKey = "GscSourceRoot";
    private const string BuildCommitMetadataKey = "GscBuildCommit";
    private static readonly Lazy<RepositoryIdentity> Current = new(LoadIdentity);

    public static string Root => GetValidatedIdentity().Root;

    private static RepositoryIdentity GetValidatedIdentity()
    {
        var identity = Current.Value;
        if (string.IsNullOrWhiteSpace(identity.BuildCommit)
            || string.Equals(identity.BuildCommit, "unknown", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Worker 源码测试程序集缺少 GscBuildCommit；程序集：{identity.AssemblyPath}；源码根：{identity.Root}。请从 scripts/build.ps1 运行。 ");
        }

        var matches = string.Equals(identity.BuildCommit, identity.SourceCommit, StringComparison.OrdinalIgnoreCase)
                      || identity.SourceCommit.StartsWith(identity.BuildCommit, StringComparison.OrdinalIgnoreCase)
                      || identity.BuildCommit.StartsWith(identity.SourceCommit, StringComparison.OrdinalIgnoreCase);
        if (!matches)
        {
            throw new InvalidOperationException($"Worker 源码测试检测到 checkout/程序集身份不一致：程序集 commit={identity.BuildCommit}，源码根 HEAD={identity.SourceCommit}，源码根={identity.Root}，程序集={identity.AssemblyPath}。 ");
        }

        return identity;
    }

    private static RepositoryIdentity LoadIdentity()
    {
        var assembly = typeof(TestRepositoryContext).Assembly;
        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value, StringComparer.OrdinalIgnoreCase);
        var assemblyPath = assembly.Location;
        if (!metadata.TryGetValue(SourceRootMetadataKey, out var sourceRootValue)
            || string.IsNullOrWhiteSpace(sourceRootValue))
        {
            throw new InvalidOperationException($"Worker 源码测试程序集缺少 {SourceRootMetadataKey} 元数据，无法绑定源码根；程序集：{assemblyPath}。 ");
        }

        var root = Path.GetFullPath(sourceRootValue);
        if (!File.Exists(Path.Combine(root, "GameSaveCenter.sln"))
            || !Directory.Exists(Path.Combine(root, "src", "GameSaveCenter.Worker")))
        {
            throw new InvalidOperationException($"Worker 源码测试元数据指向无效 checkout：{root}；程序集：{assemblyPath}。 ");
        }

        var sourceCommit = ReadGitCommit(root);
        var buildCommit = metadata.TryGetValue(BuildCommitMetadataKey, out var value) ? value : string.Empty;
        return new RepositoryIdentity(root, sourceCommit, buildCommit ?? string.Empty, assemblyPath);
    }

    private static string ReadGitCommit(string root)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "git.exe",
            Arguments = "-C \"" + root.Replace("\"", "\\\"") + "\" rev-parse --verify HEAD",
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException($"无法启动 git 读取源码根身份：{root}。 ");
        var output = process.StandardOutput.ReadToEnd().Trim();
        var error = process.StandardError.ReadToEnd().Trim();
        if (!process.WaitForExit(5000) || process.ExitCode != 0 || output.Length < 7)
            throw new InvalidOperationException($"无法读取源码根 HEAD：{root}；git 输出：{output}；错误：{error}。 ");
        return output;
    }

    private sealed class RepositoryIdentity
    {
        public RepositoryIdentity(string root, string sourceCommit, string buildCommit, string assemblyPath)
        {
            Root = root;
            SourceCommit = sourceCommit;
            BuildCommit = buildCommit;
            AssemblyPath = assemblyPath;
        }

        public string Root { get; }
        public string SourceCommit { get; }
        public string BuildCommit { get; }
        public string AssemblyPath { get; }
    }
}
