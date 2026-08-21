using System;
using System.Collections.Generic;
using System.IO;

namespace GameSaveCenter.Playnite.Tests;

/// <summary>
/// Resolves the optional AcrylicFork design source without depending on one developer's
/// absolute checkout path. The production repository remains self-contained; when the
/// visual reference checkout is not present, only the tests which compare against it are
/// dynamically skipped.
/// </summary>
internal static class AcrylicForkDesignSource
{
    private const string RootEnvironmentVariable = "GSC_ACRYLICFORK_ROOT";

    public static bool Exists(string fileName)
    {
        try
        {
            foreach (var directory in GetCandidateDesignDirectories(Environment.GetEnvironmentVariable(RootEnvironmentVariable)))
            {
                if (File.Exists(Path.Combine(directory, fileName)))
                    return true;
            }
        }
        catch (DirectoryNotFoundException)
        {
        }

        return false;
    }

    public static string MissingMessage(string fileName)
    {
        var explicitRoot = Environment.GetEnvironmentVariable(RootEnvironmentVariable);
        return string.IsNullOrWhiteSpace(explicitRoot)
            ? $"未找到 AcrylicFork Demo 基准文件 {fileName}。默认测试不依赖外部 Demo；如需运行该视觉对照测试，请设置 {RootEnvironmentVariable} 指向 GameSaveCenter.AcrylicFork 根目录。"
            : $"环境变量 {RootEnvironmentVariable} 指向的目录中未找到 AcrylicFork Demo 基准文件 {fileName}：{explicitRoot}";
    }

    public static string Read(string fileName)
    {
        var explicitRoot = Environment.GetEnvironmentVariable(RootEnvironmentVariable);
        var candidateDirectories = GetCandidateDesignDirectories(explicitRoot);

        foreach (var directory in candidateDirectories)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
                return File.ReadAllText(path);
        }

        var message = MissingMessage(fileName);

        // Discovery is responsible for skipping this optional comparison in the
        // normal xUnit 2 run. Reaching Read without a matching fact attribute is
        // a test-maintenance error and must fail loudly instead of depending on
        // xUnit 3-only dynamic-skip APIs.
        throw new DirectoryNotFoundException(message);
    }

    private static IEnumerable<string> GetCandidateDesignDirectories(string explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
        {
            yield return Path.Combine(explicitRoot, "src", "GameSaveCenter.Playnite", "Design");
            yield return Path.Combine(explicitRoot, "Design");
            yield break;
        }

        var repositoryRoot = FindRepositoryRoot();
        yield return Path.Combine(repositoryRoot, "..", "GameSaveCenter.AcrylicFork", "src", "GameSaveCenter.Playnite", "Design");
        yield return Path.Combine(repositoryRoot, "GameSaveCenter.AcrylicFork", "src", "GameSaveCenter.Playnite", "Design");
    }

    private static string FindRepositoryRoot()
    {
        foreach (var initialDirectory in new[]
                 {
                     new DirectoryInfo(Directory.GetCurrentDirectory()),
                     new DirectoryInfo(AppContext.BaseDirectory)
                 })
        {
            for (var directory = initialDirectory; directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                    return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("无法定位 GameSaveCenter 仓库根目录，无法解析 AcrylicFork Demo 基准。");
    }
}
