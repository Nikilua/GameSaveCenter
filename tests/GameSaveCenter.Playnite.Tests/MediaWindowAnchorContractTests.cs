using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MediaWindowAnchorContractTests
{
    [Fact]
    public void LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var codeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");

        Assert.Contains("Click=\"OnLoadMoreMediaClick\"", view);
        Assert.Contains("Click=\"OnLoadMoreMediaInboxClick\"", view);
        Assert.Contains("CaptureAnchor(MediaGrid)", codeBehind);
        Assert.Contains("CaptureAnchor(MediaInboxGrid)", codeBehind);
        Assert.Contains("ScrollToVerticalOffset", codeBehind);
        Assert.Contains("ScrollIntoView", codeBehind);
    }

    [Fact]
    public void EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var codeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");
        var viewModel = Read("src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs");

        Assert.Contains("ReloadMediaWindowCommand", view);
        Assert.Contains("ReloadMediaInboxCommand", view);
        Assert.Contains("仅当前保留项参与操作", codeBehind);
        Assert.Contains("ReloadMediaWindowAsync", viewModel);
        Assert.Contains("ReloadMediaInboxWindowAsync", viewModel);
    }

    private static string Read(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null) throw new DirectoryNotFoundException("无法定位仓库根目录。");
        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray()));
    }
}
