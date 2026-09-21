using System;
using System.Globalization;
using System.IO;
using GameSaveCenter.Playnite.Controls;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22StatusStyleIndexBehaviorTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    [Theory]
    [InlineData("成功", "✓ 成功")]
    [InlineData("需关注", "⚠ 需关注")]
    [InlineData("失败", "× 失败")]
    [InlineData("执行中", "ℹ 执行中")]
    [InlineData("上传中", "ℹ 上传中")]
    [InlineData("已暂停", "⚠ 已暂停")]
    public void SharedStatusGlyphMappingCoversEachVisualBucket(string status, string expected)
    {
        var converter = new StatusGlyphConverter();

        Assert.Equal(expected, converter.Convert(status, typeof(string), string.Empty, Culture));
    }

    [Fact]
    public void UnknownStatusDoesNotBorrowSuccessCueAndProductionPagesShareTheConverter()
    {
        var converter = new StatusGlyphConverter();
        var unknown = (string)converter.Convert("未知状态", typeof(string), string.Empty, Culture);

        Assert.StartsWith("⚠ ", unknown, StringComparison.Ordinal);
        Assert.DoesNotContain("✓", unknown, StringComparison.Ordinal);

        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var taskCenter = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("GscStatusGlyphConverter", taskCenter, StringComparison.Ordinal);
        Assert.Contains("GscStatusGlyphConverter", maintenance, StringComparison.Ordinal);
    }
}
