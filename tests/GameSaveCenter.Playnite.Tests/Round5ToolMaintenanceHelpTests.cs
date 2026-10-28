using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Round5ToolMaintenanceHelpTests
{
    [Fact]
    public void DisabledTrackingAndDiagnosticCopyExplainTheirActualScope()
    {
        var root = TestRepositoryContext.Root;
        var trainer = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var toggle = trainer.Descendants().Single(e => (string?)e.Attribute("AutomationProperties.Name") == "退出游戏后关闭所选工具");
        Assert.Equal("{Binding SelectedGameTool.CanTrackProcess}", (string?)toggle.Attribute("IsEnabled"));
        Assert.Equal("True", (string?)toggle.Attribute("ToolTipService.ShowOnDisabled"));
        Assert.Contains("脚本和系统默认程序无法安全关闭", (string?)toggle.Attribute("AutomationProperties.HelpText"));

        var maintenance = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var copy = maintenance.Descendants().Single(e => (string?)e.Attribute("AutomationProperties.Name") == "复制诊断");
        Assert.Contains("CopyDiagnosticsCommand", (string?)copy.Attribute("Command"));
        Assert.Contains("不仅限于当前所选问题", (string?)copy.Attribute("AutomationProperties.HelpText"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        Assert.Contains("CopyTextWithRetryAsync(DiagnosticSummary ?? string.Empty", code, StringComparison.Ordinal);
    }
}
