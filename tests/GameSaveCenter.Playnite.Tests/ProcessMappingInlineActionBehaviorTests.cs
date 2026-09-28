using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using GscButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R23ProductionResourcesWpf")]
public sealed class ProcessMappingInlineActionBehaviorTests
{
    [Fact]
    public void InlineDeleteTargetsTheHitRowInsteadOfASeparateSelectedRowInBothThemes()
    {
        RunSta(() =>
        {
            foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
            {
                var selected = Mapping("launcher-a.exe", "game-a", "Selected Game");
                var clicked = Mapping("launcher-b.exe", "game-b", "Clicked Game");
                var command = new CapturingCommand();
                var page = new MaintenanceView();
                var tabControl = Field<TabControl>(page, "MaintenanceTabControl");
                var processTab = Assert.Single(tabControl.Items.Cast<TabItem>(), item => Equals(item.Header, "进程映射"));
                var context = new ProcessMappingPageContext(
                    new[] { selected, clicked }, selected, command, tabControl.Items.IndexOf(processTab));
                page.DataContext = context;
                ApplyTheme(page, theme);

                var window = new Window
                {
                    Content = page,
                    Width = 1440,
                    Height = 1080,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 0.01
                };

                try
                {
                    window.Show();
                    PumpLayout(window);

                    var grid = Field<DataGrid>(page, "MaintenanceProcessGrid");
                    Assert.True(grid.IsVisible && grid.ActualWidth > 0 && grid.ActualHeight > 0,
                        $"{theme}: process-mapping table was not realized; tab={tabControl.SelectedIndex}/{tabControl.SelectedItem}; page={page.IsVisible}/{page.ActualWidth:0.##}x{page.ActualHeight:0.##}; grid={grid.Visibility}/{grid.IsVisible}/{grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}; items={grid.Items.Count}.");
                    Assert.Same(selected, grid.SelectedItem);
                    Assert.Same(selected, context.SelectedProcessMapping);

                    var clickedRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromItem(clicked));
                    var rowButton = Assert.Single(FindVisualChildren<GscButton>(clickedRow));
                    Assert.Same(command, rowButton.Command);
                    Assert.Same(clicked, rowButton.CommandParameter);
                    Assert.True(rowButton.IsEnabled, $"{theme}: clicked row action was disabled despite a valid row target.");
                    Assert.Equal("launcher-b.exe", AutomationProperties.GetName(rowButton));
                    Assert.False(command.CanExecute(null), "The row-delete command must reject a missing target instead of falling back to the selected row.");

                    var hitPoint = new Point(rowButton.ActualWidth / 2, rowButton.ActualHeight / 2);
                    var hitVisual = VisualTreeHelper.HitTest(rowButton, hitPoint)?.VisualHit;
                    Assert.Same(rowButton, FindVisualAncestor<GscButton>(hitVisual));

                    Invoke(rowButton);
                    Assert.Same(clicked, Assert.Single(command.ExecutedParameters));
                    Assert.Equal("launcher-b.exe", Assert.IsType<ProcessMappingDto>(command.ExecutedParameters[0]).ExecutableName);
                    Assert.Same(selected, context.SelectedProcessMapping);

                    var inspector = Field<Border>(page, "MaintenanceProcessInspector");
                    var inspectorButton = Assert.Single(FindVisualChildren<GscButton>(inspector),
                        button => Equals(button.Content, "删除选中映射"));
                    Assert.Same(selected, inspectorButton.CommandParameter);
                    Invoke(inspectorButton);
                    Assert.Collection(
                        command.ExecutedParameters,
                        parameter => Assert.Same(clicked, parameter),
                        parameter => Assert.Same(selected, parameter));
                }
                finally
                {
                    window.Close();
                }
            }
        });
    }

    private static ProcessMappingDto Mapping(string executableName, string playniteId, string gameName)
        => new()
        {
            ExecutableName = executableName,
            PlayniteId = playniteId,
            GameName = gameName
        };

    private static T Field<T>(object owner, string name)
        where T : class
        => Assert.IsType<T>(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner));

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static void Invoke(ButtonBase button)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);
        Assert.NotNull(peer);
        var invoke = Assert.IsAssignableFrom<IInvokeProvider>(peer!.GetPattern(PatternInterface.Invoke));
        invoke.Invoke();
        button.Dispatcher.Invoke(DispatcherPriority.Background, new Action(button.UpdateLayout));
    }

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

    private static T? FindVisualAncestor<T>(DependencyObject? element)
        where T : DependencyObject
    {
        for (var current = element; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
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

    private sealed class ProcessMappingPageContext : INotifyPropertyChanged
    {
        private ProcessMappingDto selectedProcessMapping;
        private int maintenanceTabIndex;

        public ProcessMappingPageContext(
            IEnumerable<ProcessMappingDto> mappings,
            ProcessMappingDto selected,
            ICommand deleteProcessMappingCommand,
            int maintenanceTabIndex)
        {
            ProcessMappings = new ObservableCollection<ProcessMappingDto>(mappings);
            selectedProcessMapping = selected;
            DeleteProcessMappingCommand = deleteProcessMappingCommand;
            this.maintenanceTabIndex = maintenanceTabIndex;
        }

        public ObservableCollection<ProcessMappingDto> ProcessMappings { get; }
        public ICommand DeleteProcessMappingCommand { get; }
        public int MaintenanceTabIndex
        {
            get => maintenanceTabIndex;
            set
            {
                maintenanceTabIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaintenanceTabIndex)));
            }
        }

        public ProcessMappingDto SelectedProcessMapping
        {
            get => selectedProcessMapping;
            set
            {
                selectedProcessMapping = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProcessMapping)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class CapturingCommand : ICommand
    {
        public List<object?> ExecutedParameters { get; } = new();
        public bool CanExecute(object? parameter) => parameter is ProcessMappingDto;
        public void Execute(object? parameter) => ExecutedParameters.Add(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
