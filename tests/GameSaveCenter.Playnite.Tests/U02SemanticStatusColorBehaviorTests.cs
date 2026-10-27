using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class U02SemanticStatusColorBehaviorTests
{
    private static readonly StatusCase[] Cases =
    {
        new(TaskState.Queued, "GscIconStatusInfo", "GscInfoBrush", "GscInfoIconFillBrush"),
        new(TaskState.Running, "GscIconActionRun", "GscInfoBrush", "GscInfoIconFillBrush"),
        new(TaskState.Succeeded, "GscIconStatusSuccess", "GscSuccessBrush", "GscSuccessIconFillBrush"),
        new(TaskState.Failed, "GscIconStatusError", "GscErrorBrush", "GscErrorIconFillBrush"),
        new(TaskState.Cancelled, "GscIconStatusInfo", "GscMutedStatusBrush", "GscControlFillBrush"),
        new(TaskState.WaitingForUser, "GscIconStatusWarning", "GscWarningBrush", "GscWarningIconFillBrush")
    };

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void OverviewTaskRowsUseTextIconAndContrastingSemanticColor(GameSaveCenterThemeMode theme)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var data = new OverviewData();
                foreach (var item in Cases)
                {
                    data.OverviewTasks.Add(new TaskStatusDto
                    {
                        TaskId = $"u02-{item.State}",
                        TaskType = "Backup",
                        GameName = "颜色对比样本",
                        State = item.State,
                        Message = "使用当前任务状态",
                        CreatedUtc = new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc)
                    });
                }

                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 980
                };
                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    overview,
                    glassEnabled: false,
                    strengthPercent: 78,
                    themeMode: theme,
                    highContrastOverride: false);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                    overview.Resources,
                    palette,
                    glassEnabled: false,
                    motionEnabled: true);

                using var host = new WindowHost(overview, palette.Background);
                var list = Assert.IsType<ListBox>(overview.FindName("OverviewActivityList"));
                Assert.Equal(Cases.Length, list.Items.Count);

                var cardColor = Composite(BrushColor(overview.TryFindResource("GscGlassStrongBrush")), palette.Background);
                var rowSurface = Composite(BrushColor(overview.TryFindResource("GscControlFillBrush")), cardColor);
                for (var index = 0; index < Cases.Length; index++)
                {
                    var item = Cases[index];
                    var task = data.OverviewTasks[index];
                    var container = Assert.IsType<ListBoxItem>(list.ItemContainerGenerator.ContainerFromIndex(index));
                    var tile = FindNamedVisual<Border>(container, "OverviewTaskStatusPill");
                    var glyph = FindNamedVisual<ThemeAwareIcon>(container, "RecentActivityGlyph");
                    var chip = FindNamedVisual<Border>(container, "OverviewTaskResultChip");
                    var label = FindNamedVisual<TextBlock>(container, "OverviewTaskResultText");

                    Assert.Equal(task.StateDisplay, label.Text);
                    Assert.Same(overview.TryFindResource(item.IconKey), glyph.IconData);
                    Assert.Equal(BrushColor(overview.TryFindResource(item.ForegroundKey)), BrushColor(glyph.Foreground));
                    Assert.Equal(BrushColor(overview.TryFindResource(item.FillKey)), BrushColor(tile.Background));
                    Assert.Equal(BrushColor(overview.TryFindResource(item.FillKey)), BrushColor(chip.Background));
                    Assert.Equal(BrushColor(overview.TryFindResource(item.ForegroundKey)), BrushColor(label.Foreground));

                    var chipBackground = Composite(BrushColor(chip.Background), rowSurface);
                    var effectiveLabel = Composite(BrushColor(label.Foreground), chipBackground);
                    var contrast = ContrastRatio(effectiveLabel, chipBackground);
                    Assert.True(contrast >= 4.5, $"{theme}/{item.State} status text contrast was {contrast:0.00}:1.");
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
    }

    private static T FindNamedVisual<T>(DependencyObject root, string name)
        where T : DependencyObject
        => FindVisualDescendants<T>(root)
            .Single(element => element is FrameworkElement frameworkElement && frameworkElement.Name == name);

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null) return;
        if (current is T match) matches.Add(match);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            Visit(VisualTreeHelper.GetChild(current, index), matches);
    }

    private static Color BrushColor(object? brush)
        => Assert.IsType<SolidColorBrush>(brush).Color;

    private static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255.0;
        return Color.FromRgb(
            (byte)Math.Round(foreground.R * alpha + background.R * (1 - alpha)),
            (byte)Math.Round(foreground.G * alpha + background.G * (1 - alpha)),
            (byte)Math.Round(foreground.B * alpha + background.B * (1 - alpha)));
    }

    private static double ContrastRatio(Color first, Color second)
    {
        var firstLuminance = RelativeLuminance(first);
        var secondLuminance = RelativeLuminance(second);
        var lighter = Math.Max(firstLuminance, secondLuminance);
        var darker = Math.Min(firstLuminance, secondLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(Color color)
    {
        static double Channel(byte value)
        {
            var normalized = value / 255.0;
            return normalized <= 0.03928
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    private sealed class StatusCase
    {
        public StatusCase(TaskState state, string iconKey, string foregroundKey, string fillKey)
        {
            State = state;
            IconKey = iconKey;
            ForegroundKey = foregroundKey;
            FillKey = fillKey;
        }

        public TaskState State { get; }
        public string IconKey { get; }
        public string ForegroundKey { get; }
        public string FillKey { get; }
    }

    private sealed class OverviewData
    {
        public DashboardSnapshotDto Snapshot { get; } = new();
        public ObservableCollection<RecentAccessItem> RecentAccessItems { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ICommand OpenRecentAccessCommand { get; } = new EmptyCommand();
        public ICommand OpenActivityCommand { get; } = new EmptyCommand();
        public ICommand OpenCloudQueueCommand { get; } = new EmptyCommand();
        public ICommand RefreshCommand { get; } = new EmptyCommand();
        public TaskStatusDto? SelectedTask { get; set; }
        public bool IsDashboardSnapshotLoaded => true;
        public string OverviewTasksCountDisplay => $"全部 {OverviewTasks.Count} 个";
    }

    private sealed class EmptyCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content, Color background)
        {
            Window = new Window
            {
                Content = content,
                Width = 1280,
                Height = 980,
                Background = new SolidColorBrush(background),
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
            content.UpdateLayout();
        }

        public Window Window { get; }
        public void Dispose() => Window.Close();
    }
}
