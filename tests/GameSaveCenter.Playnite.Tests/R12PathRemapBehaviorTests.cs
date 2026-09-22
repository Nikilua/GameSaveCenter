using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12PathRemapBehaviorTests
{
    [Fact]
    public void PreviewItemKeepsSimilarPathsDistinctAndExplainsMissingTarget()
    {
        var oldPath = @"C:\Saves\Profile\long-name\slot-01.sav";
        var newPath = @"D:\Archive\Profile\long-name\slot-01.sav";
        var item = new PathRemapPreviewItemDto
        {
            Category = "存档候选",
            OldPath = oldPath,
            NewPath = newPath,
            TargetExists = false
        };

        Assert.Equal(oldPath, item.OldPathDisplay);
        Assert.Equal(newPath, item.NewPathDisplay);
        Assert.Equal("目标不存在（需明确确认）", item.TargetStateDisplay);
        Assert.NotEqual(oldPath.Replace("C:", "D:"), item.OldPathDisplay);
    }

    [Fact]
    public void MaintenanceViewUsesBoundedVirtualizedPreviewWithCopyableFullPaths()
    {
        var path = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml");
        var document = XDocument.Load(path);
        var grid = document.Descendants().Single(element =>
            element.Name.LocalName == "DataGrid" && element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "PathRemapPreviewGrid");
        var source = File.ReadAllText(path);

        Assert.Equal("{Binding PathRemapPreview.Items}", grid.Attribute("ItemsSource")?.Value);
        Assert.Equal("True", grid.Attribute("EnableRowVirtualization")?.Value);
        Assert.Equal("260", grid.Attribute("MaxHeight")?.Value);
        Assert.Contains("将写入路径（仅预览）", source);
        Assert.Contains("OldPathDisplay", source);
        Assert.Contains("NewPathDisplay", source);
        Assert.Contains("TargetStateDisplay", source);
        Assert.Contains("IsReadOnly=\"True\"", source);
        Assert.Contains("不会写入用户目录", source);
        Assert.DoesNotContain("File.Move", source);
        Assert.DoesNotContain("Directory.Move", source);
    }
}
