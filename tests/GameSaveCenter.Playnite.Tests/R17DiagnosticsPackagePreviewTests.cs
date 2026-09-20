using System;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17DiagnosticsPackagePreviewTests
{
    [Fact]
    public void GeneratedResultKeepsReviewableLocationAndSize()
    {
        var result = new DiagnosticsPackageResultDto
        {
            PackagePath = @"C:\Temp\gsc-diagnostics.zip",
            PackageBytes = 2 * 1024 * 1024,
            IncludedFileCount = 10,
            Summary = "已生成诊断包"
        };

        Assert.Contains("位置：C:\\Temp\\gsc-diagnostics.zip", result.ResultDisplay, StringComparison.Ordinal);
        Assert.Contains("大小：2 MiB", result.ResultDisplay, StringComparison.Ordinal);
        Assert.Contains("已生成诊断包", result.ResultDisplay, StringComparison.Ordinal);
    }
}
