using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DragDropImportSourceTests
{
    [Fact]
    public void TrainerCenterSupportsSingleFileAndDirectoryDrop()
    {
        var root = FindRepositoryRoot();
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml.cs"));

        Assert.Contains("AllowDrop=\"True\"", view);
        Assert.Contains("PreviewDragOver=\"OnToolDragOver\"", view);
        Assert.Contains("Drop=\"OnToolDrop\"", view);
        Assert.Contains("拖入 EXE / CT / LNK / BAT / CMD / PS1 / ZIP 到此页导入", view);
        Assert.Contains("DataFormats.FileDrop", code);
        Assert.Contains("ImportDroppedGameTool", code);
        Assert.Contains("File.Exists(path) || Directory.Exists(path)", code);
    }

    [Fact]
    public void ViewModelClassifiesDroppedFilesAndRequiresSelectedGame()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("public void ImportDroppedGameTool(string? path)", viewModel);
        Assert.Contains("请先选择游戏，再拖入工具文件", viewModel);
        Assert.Contains("case \".ct\":", viewModel);
        Assert.Contains("case \".lnk\":", viewModel);
        Assert.Contains("case \".bat\":", viewModel);
        Assert.Contains("case \".cmd\":", viewModel);
        Assert.Contains("case \".ps1\":", viewModel);
        Assert.Contains("case \".exe\":", viewModel);
        Assert.Contains("case \".zip\":", viewModel);
        Assert.Contains("GameToolType.CustomExecutable", viewModel);
        Assert.Contains("GameToolType.Trainer", viewModel);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
