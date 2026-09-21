using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21DisabledHiddenBehaviorTests
{
    [Fact]
    public void SaveAvailabilitySurfaceSeparatesHiddenHandoffFromReadableReason()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        Assert.Contains("AutomationProperties.HelpText=\"{Binding RestoreAvailabilityHint, Mode=OneWay}\"", save);
        Assert.Contains("Binding=\"{Binding RestoreAvailabilityNeedsMaintenance}\"", save);
        Assert.Contains("<Setter Property=\"Visibility\" Value=\"Collapsed\"/>", save);

        RunSta(() =>
        {
            const string hintText = "当前不能安全恢复：Ludusavi 不可用。";
            var view = new SaveCenterView();
            using var host = new WindowHost(view);

            view.DataContext = new AvailabilityContext(hintText, false, _ => { });
            host.Pump();

            var handoff = FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(view)
                .Single(button => string.Equals(button.Content as string, "去维护中心检查", StringComparison.Ordinal));
            var hint = FindVisualChildren<TextBlock>(view)
                .Single(text => string.Equals(text.Text, hintText, StringComparison.Ordinal));

            Assert.Equal(Visibility.Collapsed, handoff.Visibility);
            Assert.False(handoff.IsVisible);
            Assert.False(handoff.Focus());
            Assert.True(hint.Focusable);
            Assert.True(KeyboardNavigation.GetIsTabStop(hint));
            Assert.True(hint.Focus());
            Assert.Equal(hintText, AutomationProperties.GetName(hint));
            Assert.Equal(hintText, AutomationProperties.GetHelpText(hint));

            var invoked = 0;
            view.DataContext = new AvailabilityContext(hintText, true, _ => invoked++);
            host.Pump();

            Assert.Equal(Visibility.Visible, handoff.Visibility);
            Assert.True(handoff.IsVisible);
            Assert.True(handoff.IsEnabled);
            Assert.NotNull(handoff.Command);
            Assert.True(handoff.Command!.CanExecute(null));
            handoff.Command.Execute(null);
            Assert.Equal(1, invoked);
        });
    }

    [Fact]
    public void DisabledContextActionStaysVisibleAndAutomationInvokeIsRejected()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var reason = "请先选择目标游戏；归类按钮保持禁用。";
            var invoked = 0;
            var action = new GameSaveCenter.Playnite.Controls.Button
            {
                Content = "安全动作",
                Style = (Style)resources["GscWpfUiContextButton"],
                Command = new TestCommand(_ => invoked++, _ => false)
            };
            AutomationProperties.SetName(action, "安全动作");
            AutomationProperties.SetHelpText(action, reason);

            var hint = new TextBlock
            {
                Text = reason,
                Style = (Style)resources["GscActionAvailabilityHintText"]
            };
            AutomationProperties.SetName(hint, reason);
            AutomationProperties.SetHelpText(hint, reason);

            using var host = new WindowHost(new StackPanel { Children = { hint, action } }, resources);
            Assert.True(action.IsVisible);
            Assert.False(action.IsEnabled);
            Assert.Equal(reason, FrameworkElementAutomationPeer.CreatePeerForElement(action)!.GetHelpText());

            var invoke = Assert.IsAssignableFrom<IInvokeProvider>(
                FrameworkElementAutomationPeer.CreatePeerForElement(action)!.GetPattern(PatternInterface.Invoke));
            Assert.Throws<ElementNotEnabledException>(() => invoke.Invoke());
            Assert.Equal(0, invoked);
        });
    }

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    xmlns:ui=""clr-namespace:GameSaveCenter.Playnite.Controls;assembly=GameSaveCenter.Playnite""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class WindowHost : IDisposable
    {
        private readonly Window window;

        public WindowHost(FrameworkElement content, ResourceDictionary? resources = null)
        {
            if (resources != null)
                content.Resources = resources;
            window = new Window
            {
                Content = content,
                Width = 760,
                Height = 620,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            window.Show();
            Pump();
        }

        public void Pump()
        {
            window.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            window.UpdateLayout();
        }

        public void Dispose()
        {
            if (window.IsVisible)
                window.Close();
        }
    }

    private sealed class AvailabilityContext
    {
        public AvailabilityContext(string hint, bool needsMaintenance, Action<object?> openMaintenance)
        {
            RestoreAvailabilityHint = hint;
            RestoreAvailabilityNeedsMaintenance = needsMaintenance;
            OpenMaintenanceCommand = new TestCommand(openMaintenance, _ => true);
        }

        public string RestoreAvailabilityHint { get; }
        public bool RestoreAvailabilityNeedsMaintenance { get; }
        public ICommand OpenMaintenanceCommand { get; }
        public int SaveTabIndex { get; set; }
        public List<object> Backups { get; } = new List<object>();
    }

    private sealed class TestCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool> canExecute;

        public TestCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => canExecute(parameter);
        public void Execute(object? parameter) => execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
