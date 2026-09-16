using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GameSaveCenter.Playnite.Tests;

/// <summary>
/// Resolves source files from the checkout that built this test assembly. Tests must not
/// infer a repository by walking upward from an isolated bin directory: that can land in a
/// different checkout when OutputRoot is outside the source tree.
/// </summary>
internal static class TestRepositoryContext
{
    private const string SourceRootMetadataKey = "GscSourceRoot";
    private const string BuildCommitMetadataKey = "GscBuildCommit";
    private static readonly Lazy<RepositoryIdentity> Current = new Lazy<RepositoryIdentity>(LoadIdentity);

    public static string Root => GetValidatedIdentity().Root;

    public static void AssertAssemblyMatchesSource()
    {
        _ = GetValidatedIdentity();
    }

    private static RepositoryIdentity GetValidatedIdentity()
    {
        var identity = Current.Value;
        if (string.Equals(identity.BuildCommit, "unknown", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(identity.BuildCommit))
        {
            throw new InvalidOperationException(
                "Playnite 源码测试缺少 GscBuildCommit。请从 scripts/build.ps1 运行，或在构建时显式设置 GSC_BUILD_COMMIT；" +
                $"程序集位置：{identity.AssemblyPath}；源码根：{identity.Root}。");
        }

        var buildCommitMatchesSource = string.Equals(identity.BuildCommit, identity.SourceCommit, StringComparison.OrdinalIgnoreCase)
                                       || identity.SourceCommit.StartsWith(identity.BuildCommit, StringComparison.OrdinalIgnoreCase)
                                       || identity.BuildCommit.StartsWith(identity.SourceCommit, StringComparison.OrdinalIgnoreCase);
        if (!buildCommitMatchesSource)
        {
            throw new InvalidOperationException(
                "Playnite 源码测试检测到 checkout/程序集身份不一致，已阻止继续读取源码：" +
                $"程序集 commit={identity.BuildCommit}，源码根 HEAD={identity.SourceCommit}，源码根={identity.Root}，程序集={identity.AssemblyPath}。" +
                "请清理隔离输出并从同一 checkout 重新构建。");
        }

        return identity;
    }

    private static RepositoryIdentity LoadIdentity()
    {
        var assembly = typeof(TestRepositoryContext).Assembly;
        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value, StringComparer.OrdinalIgnoreCase);
        var assemblyPath = assembly.Location;
        var sourceRootValue = metadata.TryGetValue(SourceRootMetadataKey, out var metadataRoot)
            ? metadataRoot
            : string.Empty;
        if (string.IsNullOrWhiteSpace(sourceRootValue))
            throw new InvalidOperationException(
                $"Playnite 源码测试程序集缺少 {SourceRootMetadataKey} 元数据，无法绑定源码根；程序集：{assemblyPath}。" +
                "请使用当前测试项目重新构建，不要从另一个 checkout 的 bin 目录加载测试。 ");

        var root = Path.GetFullPath(sourceRootValue);
        if (!File.Exists(Path.Combine(root, "GameSaveCenter.sln"))
            || !Directory.Exists(Path.Combine(root, "src", "GameSaveCenter.Playnite")))
        {
            throw new InvalidOperationException(
                $"Playnite 源码测试元数据指向无效 checkout：{root}；程序集：{assemblyPath}。" +
                "需要包含 GameSaveCenter.sln 和 src/GameSaveCenter.Playnite 的源码根。");
        }

        var sourceCommit = ReadGitCommit(root);
        var buildCommit = metadata.TryGetValue(BuildCommitMetadataKey, out var metadataCommit)
            ? metadataCommit
            : ReadInformationalCommit(assembly);
        return new RepositoryIdentity(root, sourceCommit, buildCommit ?? string.Empty, assemblyPath);
    }

    private static string ReadGitCommit(string root)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git.exe",
            Arguments = $"-C \"{root.Replace("\"", "\\\"")}\" rev-parse --verify HEAD",
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"无法启动 git 读取源码根身份：{root}。");
        var output = process.StandardOutput.ReadToEnd().Trim();
        var error = process.StandardError.ReadToEnd().Trim();
        if (!process.WaitForExit(5000) || process.ExitCode != 0 || output.Length < 7)
            throw new InvalidOperationException(
                $"无法读取源码根 HEAD：{root}；git 输出：{output}；错误：{error}。" +
                "源码型测试已停止，避免把其他 checkout 的源码当作当前程序集的测试对象。");
        return output;
    }

    private static string? ReadInformationalCommit(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrWhiteSpace(version)) return null;
        var value = version!;
        var separator = value.LastIndexOf('+');
        return separator >= 0 && separator + 1 < value.Length ? value.Substring(separator + 1) : value;
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
