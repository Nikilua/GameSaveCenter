using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22ConfirmationStructureBehaviorTests
{
    [Fact]
    public void ProductionDialogKeepsLongMessageScrollAndConsistentActionSpacing()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var source = File.ReadAllText(Path.Combine(TestRepositoryContext.Root,
            "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var document = XDocument.Parse(source);
        XNamespace names = "http://schemas.microsoft.com/winfx/2006/xaml";
        var card = document.Descendants().Single(node => (string?)node.Attribute(names + "Name") == "DialogCard");
        var message = card.Descendants().Single(node => (string?)node.Attribute(names + "Name") == "DialogMessageText");
        Assert.Equal("ScrollViewer", message.Parent!.Name.LocalName);
        Assert.Equal("Auto", (string?)message.Parent.Attribute("VerticalScrollBarVisibility"));
        var confirm = card.Descendants().Single(node => (string?)node.Attribute(names + "Name") == "DialogConfirmButton");
        var actions = confirm.Parent!;
        Assert.Equal("WrapPanel", actions.Name.LocalName);
        var buttons = actions.Elements().ToArray();
        Assert.Equal(new[] { "DialogCopyButton", "DialogNeverButton", "DialogLaterButton", "DialogCancelButton", "DialogConfirmButton" },
            buttons.Select(node => (string?)node.Attribute(names + "Name")).ToArray());
        Assert.All(buttons.Take(4), node => Assert.Equal("0,0,8,8", (string?)node.Attribute("Margin")));
        Assert.Equal("0,0,0,8", (string?)confirm.Attribute("Margin"));
    }

    [Fact]
    public void DialogActionMarginsWrapLongLabelsWithoutOverlappingOrClipping()
    {
        RunSta(() =>
        {
            var document = XDocument.Parse(File.ReadAllText(Path.Combine(TestRepositoryContext.Root,
                "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml")));
            XNamespace names = "http://schemas.microsoft.com/winfx/2006/xaml";
            var confirm = document.Descendants().Single(node => (string?)node.Attribute(names + "Name") == "DialogConfirmButton");
            var elements = confirm.Parent!.Elements().ToArray();
            var resourceHost = new GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView();
            var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            actions.Resources.MergedDictionaries.Add(resourceHost.Resources);
            foreach (var element in elements.Skip(1).Where(node => (string?)node.Attribute(names + "Name") != "DialogCancelButton"))
            {
                var margin = (Thickness)new ThicknessConverter().ConvertFromInvariantString((string)element.Attribute("Margin")!)!;
                actions.Children.Add(new Button
                {
                    Content = (string?)element.Attribute(names + "Name") == "DialogConfirmButton"
                        ? "确认执行所选操作" : (string?)element.Attribute("Content"),
                    Margin = margin,
                    Style = (Style)resourceHost.Resources[(string?)element.Attribute(names + "Name") == "DialogConfirmButton" ? "GscPrimaryButton" : "GscButtonBase"]
                });
            }
            var root = new Grid { Width = 220, Children = { actions } };
            var window = new Window { Content = root, Width = 260, Height = 220, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Opacity = 0.01 };
            System.Windows.Automation.AutomationProperties.SetName(window, "Dialog action wrapping fixture");
            try
            {
                window.Show(); window.UpdateLayout();
                var buttons = actions.Children.OfType<Button>().ToArray();
                Assert.Equal(3, buttons.Length);
                var bounds = buttons.Select(button => button.TransformToAncestor(actions).TransformBounds(new Rect(button.RenderSize))).ToArray();
                for (var index = 0; index < bounds.Length; index++)
                {
                    Assert.True(bounds[index].Width > 0 && bounds[index].Height > 0);
                    Assert.True(bounds[index].Left >= -0.5 && bounds[index].Right <= actions.ActualWidth + 0.5);
                    for (var next = index + 1; next < bounds.Length; next++)
                        Assert.False(bounds[index].IntersectsWith(bounds[next]), $"Actions overlap: {bounds[index]} and {bounds[next]}");
                }
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void DangerousConfirmationMakesCancelSafeDefaultAndRequiresExplicitConfirmFocus()
    {
        RunSta(() =>
        {
            var confirm = new Button();
            var cancel = new Button();

            DialogConfirmationPolicy.ApplyConfirmationButtons(confirm, cancel, isDangerous: true);

            Assert.False(confirm.IsDefault);
            Assert.False(confirm.IsCancel);
            Assert.False(cancel.IsDefault);
            Assert.True(cancel.IsCancel);
        });
    }

    [Fact]
    public void OrdinaryConfirmationRetainsEnterDefaultButChoiceAndResultPoliciesAreExplicit()
    {
        RunSta(() =>
        {
            var confirm = new Button();
            var cancel = new Button();

            DialogConfirmationPolicy.ApplyConfirmationButtons(confirm, cancel, isDangerous: false);
            Assert.True(confirm.IsDefault);
            Assert.True(cancel.IsCancel);

            DialogConfirmationPolicy.ApplyChoiceButtons(confirm, cancel);
            Assert.True(confirm.IsDefault);
            Assert.True(cancel.IsCancel);

            DialogConfirmationPolicy.ApplyResultButton(confirm, cancel);
            Assert.True(confirm.IsDefault);
            Assert.False(cancel.IsCancel);
        });
    }

    [Fact]
    public void ConfirmationRequestKeepsExplicitActionLabelsAndSafePendingCompletion()
    {
        var request = new UiConfirmationEventArgs(
            "清理候选版本",
            "对象：合成游戏；范围：3 个未锁定候选；后果：删除 ZIP 与索引。",
            "清理候选版本",
            "取消",
            isDangerous: true);

        Assert.Equal("清理候选版本", request.ConfirmText);
        Assert.Equal("取消", request.CancelText);
        Assert.True(request.IsDangerous);
        Assert.False(request.Completion.Task.IsCompleted);
        Assert.Equal(TaskStatus.WaitingForActivation, request.Completion.Task.Status);
    }

    [Fact]
    public void RestoreFlowsOptOutOfTheEnterDefaultBecauseTheyReplaceCurrentState()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var source = File.ReadAllText(Path.Combine(
            TestRepositoryContext.Root,
            "src",
            "GameSaveCenter.Playnite",
            "ViewModels",
            "DashboardViewModel.cs"));

        var localRestore = source.IndexOf("GameSaveCenter 安全恢复", StringComparison.Ordinal);
        var undoRestore = source.IndexOf("撤销恢复", StringComparison.Ordinal);
        var remoteRestore = source.IndexOf("从已校验的远端备份恢复", StringComparison.Ordinal);

        Assert.True(localRestore >= 0);
        Assert.True(undoRestore >= 0);
        Assert.True(remoteRestore >= 0);
        Assert.Contains("isDangerous: true", source.Substring(localRestore, 700), StringComparison.Ordinal);
        Assert.Contains("isDangerous: true", source.Substring(undoRestore, 500), StringComparison.Ordinal);
        Assert.Contains("isDangerous: true", source.Substring(remoteRestore, 650), StringComparison.Ordinal);
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception != null)
            throw new Xunit.Sdk.XunitException(exception.ToString());
    }
}
