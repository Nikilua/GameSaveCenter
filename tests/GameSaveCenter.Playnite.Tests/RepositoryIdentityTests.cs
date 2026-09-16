using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RepositoryIdentityTests
{
    [Fact]
    public void SourceTestsUseTheBuildBoundCheckoutAndCommit()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();

        var metadata = typeof(RepositoryIdentityTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(Path.GetFullPath(TestRepositoryContext.Root), Path.GetFullPath(metadata["GscSourceRoot"]));
        Assert.False(string.IsNullOrWhiteSpace(metadata["GscBuildCommit"]));
        Assert.False(string.Equals("unknown", metadata["GscBuildCommit"], StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SourceReadersDoNotWalkUpFromTheTestOutputDirectory()
    {
        var testsRoot = Path.Combine(TestRepositoryContext.Root, "tests", "GameSaveCenter.Playnite.Tests");
        var offenders = Directory.GetFiles(testsRoot, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith("TestRepositoryContext.cs", StringComparison.OrdinalIgnoreCase)
                           && !path.EndsWith("RepositoryIdentityTests.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path =>
            {
                var source = File.ReadAllText(path);
                return source.IndexOf("AppContext.BaseDirectory", StringComparison.Ordinal) >= 0
                       || source.IndexOf("new DirectoryInfo", StringComparison.Ordinal) >= 0
                       || source.IndexOf("GameSaveCenter.sln", StringComparison.Ordinal) >= 0;
            })
            .Select(Path.GetFileName)
            .ToArray();

        Assert.Empty(offenders);
    }
}
