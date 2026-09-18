using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02ActionCopyTests
{
    [Fact]
    public void ReloadAndValidationCommandsNameTheirActualOperation()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var dashboard = Load(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var overview = Load(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml");
        var save = Load(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml");

        var dashboardValidation = SingleCommand(dashboard, "ValidateCommand");
        Assert.Equal("重新校验", dashboardValidation.Attribute("Content")?.Value);
        Assert.Contains("重新校验", dashboardValidation.Attribute("ToolTip")?.Value);

        var reloadButtons = dashboard
            .Concat(overview)
            .Concat(save)
            .Where(element => string.Equals(element.Attribute("Command")?.Value, "{Binding LoadDetailsCommand}", StringComparison.Ordinal)
                && !string.Equals(element.Attribute("Content")?.Value, "重试", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(4, reloadButtons.Count);
        Assert.All(reloadButtons, button =>
        {
            var label = button.Attribute("Content")?.Value;
            var toolTip = button.Attribute("ToolTip")?.Value;
            var name = button.Attribute("AutomationProperties.Name")?.Value;
            if (label != null)
                Assert.Equal("重新加载详情", label);
            if (toolTip != null)
                Assert.Contains("重新加载", toolTip);
            if (name != null)
                Assert.Contains("重新加载", name);
        });

        var retryButtons = dashboard
            .Concat(overview)
            .Concat(save)
            .Where(element => string.Equals(element.Attribute("Command")?.Value, "{Binding LoadDetailsCommand}", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Content")?.Value, "重试", StringComparison.Ordinal))
            .ToList();
        Assert.Single(retryButtons);
        Assert.Contains("重试", retryButtons[0].Attribute("AutomationProperties.Name")?.Value);
    }

    [Fact]
    public void RemoteStageAndRestoreActionsExposeDifferentTargets()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var maintenance = Load(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml");
        var stage = SingleCommand(maintenance, "StageRemoteBackupCommand");
        var restore = SingleCommand(maintenance, "RestoreStagedRemoteBackupCommand");

        Assert.Equal("1 · 下载到隔离区并校验", stage.Attribute("Content")?.Value);
        Assert.Contains("隔离区", stage.Attribute("ToolTip")?.Value);
        Assert.Contains("校验", stage.Attribute("ToolTip")?.Value);
        Assert.Contains("下载到隔离区", stage.Attribute("AutomationProperties.Name")?.Value);
        Assert.DoesNotContain("恢复", stage.Attribute("Content")?.Value);

        Assert.Equal("2 · 快照并恢复", restore.Attribute("Content")?.Value);
        Assert.Contains("恢复", restore.Attribute("ToolTip")?.Value);
        Assert.Contains("快照", restore.Attribute("ToolTip")?.Value);
        Assert.Contains("恢复", restore.Attribute("AutomationProperties.Name")?.Value);
    }

    [Fact]
    public void CloudVerificationAndUploadRetryUseSeparateActionTerms()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var maintenance = Load(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml");
        var verify = SingleCommand(maintenance, "VerifyCloudTransferCommand");
        var retry = SingleCommand(maintenance, "RetryCloudUploadCommand");

        Assert.Equal("校验远端内容", verify.Attribute("Content")?.Value);
        Assert.Contains("只读校验", verify.Attribute("ToolTip")?.Value);
        Assert.Contains("远端内容", verify.Attribute("AutomationProperties.Name")?.Value);
        Assert.Equal("重试上传", retry.Attribute("Content")?.Value);
        Assert.Contains("上传", retry.Attribute("ToolTip")?.Value);
        Assert.DoesNotContain("校验", retry.Attribute("Content")?.Value);

        var dto = new CloudTransferStatusDto { State = "CheckFailed" };
        Assert.Equal("远端校验未通过", dto.GuaranteeLevelDisplay);
        Assert.DoesNotContain("check", dto.GuaranteeLevelDisplay, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductionCloudMessagesDoNotExposeTheMixedCheckTerm()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var sourceRoot = Path.Combine(TestRepositoryContext.Root, "src");
        var files = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
            .Where(file => string.Equals(Path.GetExtension(file), ".cs", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetExtension(file), ".xaml", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("远端 check", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("只读 check", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static XElement[] Load(string root, params string[] parts)
        => XDocument.Parse(File.ReadAllText(Path.Combine(new[] { root }.Concat(parts).ToArray()))).Descendants().ToArray();

    private static XElement SingleCommand(XElement[] elements, string command)
        => elements.Single(element => string.Equals(
            element.Attribute("Command")?.Value,
            "{Binding " + command + "}",
            StringComparison.Ordinal));
}
