using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    [Collection("R09FocusWpf")]
    public sealed class MediaComparisonSelectionTests
    {
        [Fact]
        public void ProductionButtonsKeepSelectedPairAcrossFilterChangesAndClearItOnRequest()
        {
            RunSta(() =>
            {
                var tempRoot = Path.Combine(Path.GetTempPath(), "gsc-s04-01-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempRoot);
                try
                {
                    var firstPath = Path.Combine(tempRoot, "first.png");
                    var secondPath = Path.Combine(tempRoot, "second.png");
                    File.WriteAllBytes(firstPath, Array.Empty<byte>());
                    File.WriteAllBytes(secondPath, Array.Empty<byte>());

                    var selection = new MediaComparisonSelection();
                    var dashboard = CreateDashboard(selection);
                    var view = new MediaCenterView { DataContext = dashboard };
                    var viewType = typeof(MediaCenterView);
                    var tabControl = GetField<TabControl>(viewType, view, "MediaTabControl");
                    var grid = GetField<ListBox>(viewType, view, "MediaGrid");
                    var addButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonAddButton");
                    var clearButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonClearButton");
                    var toggleButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonWorkspaceToggleButton");
                    var workspace = GetField<Border>(viewType, view, "MediaComparisonWorkspace");
                    var workspaceGrid = GetField<Grid>(viewType, view, "MediaComparisonWorkspaceGrid");
                    var secondCard = GetField<Border>(viewType, view, "MediaComparisonSecondCard");
                    var summary = GetField<TextBlock>(viewType, view, "MediaComparisonSummaryText");
                    var status = GetField<TextBlock>(viewType, view, "MediaComparisonStatusText");

                    using (var host = new WindowHost(new Window
                    {
                        Content = view,
                        // The current-media star ratio keeps enough comparison width here.
                        // The inner 500-DIP case below still proves stacked behavior.
                        Width = 1280,
                        Height = 700,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    }))
                    {
                        tabControl.SelectedIndex = 1;
                        BindingOperations.ClearBinding(grid, ItemsControl.ItemsSourceProperty);
                        view.ApplyResponsiveLayout(1280, 900);
                        var visibleItems = new ObservableCollection<MediaItemDto>
                        {
                            Screenshot("media-a", firstPath),
                            new MediaItemDto { MediaId = "video", Kind = MediaKind.VideoClip, ArchivePath = Path.Combine(tempRoot, "clip.mp4") },
                            Screenshot("media-missing", Path.Combine(tempRoot, "missing.png")),
                            Screenshot("media-b", secondPath)
                        };
                        grid.ItemsSource = visibleItems;
                        grid.SelectedItems.Add(visibleItems[0]);
                        grid.SelectedItems.Add(visibleItems[1]);
                        grid.SelectedItems.Add(visibleItems[2]);
                        grid.SelectedItems.Add(visibleItems[3]);
                        host.UpdateLayout();

                        Assert.Same(dashboard.AddSelectedMediaToComparisonCommand, addButton.Command);
                        Assert.Same(grid.SelectedItems, addButton.CommandParameter);
                        Assert.Equal(4, grid.SelectedItems.Count);
                        ExecuteButtonCommand(addButton);
                        host.UpdateLayout();

                        Assert.Equal(new[] { "media-a", "media-b" }, selection.Items.Select(item => item.MediaId));
                        Assert.Contains("不是截图", status.Text);
                        Assert.Contains("归档文件不存在", status.Text);
                        Assert.Contains("first.png", summary.Text);
                        Assert.Contains("second.png", summary.Text);
                        Assert.True(dashboard.MediaComparisonWorkspaceOpen);
                        Assert.Equal(Visibility.Visible, workspace.Visibility);
                        view.ApplyResponsiveLayout(1280, 600);
                        host.UpdateLayout();
                        Assert.False(dashboard.MediaComparisonWorkspaceOpen);
                        Assert.Equal(Visibility.Collapsed, workspace.Visibility);
                        toggleButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, toggleButton));
                        ExecuteButtonCommand(toggleButton);
                        host.UpdateLayout();
                        Assert.True(dashboard.MediaComparisonWorkspaceOpen);
                        Assert.Equal(Visibility.Visible, workspace.Visibility);
                        // Exercise the production breakpoint at a controlled element width. The
                        // hosted runner may clamp the top-level window to its desktop resolution,
                        // even when the test requests a 1280-DIP window.
                        workspaceGrid.Width = 640;
                        host.UpdateLayout();
                        Assert.True(workspaceGrid.ActualWidth >= 560, $"Expected the controlled comparison workspace to remain wide; actual width was {workspaceGrid.ActualWidth:F1} DIP.");
                        Assert.Equal(1, Grid.GetRow(secondCard));
                        Assert.Equal(1, Grid.GetColumn(secondCard));
                        workspaceGrid.Width = 500;
                        host.UpdateLayout();
                        Assert.True(workspaceGrid.ActualWidth < 560, $"Expected the controlled comparison workspace to stack; actual width was {workspaceGrid.ActualWidth:F1} DIP.");
                        Assert.Equal(2, Grid.GetRow(secondCard));
                        Assert.Equal(0, Grid.GetColumn(secondCard));
                        workspaceGrid.Width = 640;
                        host.UpdateLayout();
                        Assert.Equal(1, Grid.GetRow(secondCard));
                        Assert.Equal(1, Grid.GetColumn(secondCard));
                        Assert.True(toggleButton.Command!.CanExecute(null));
                        ExecuteButtonCommand(toggleButton);
                        host.UpdateLayout();
                        Assert.False(dashboard.MediaComparisonWorkspaceOpen);
                        Assert.Equal(Visibility.Collapsed, workspace.Visibility);
                        ExecuteButtonCommand(toggleButton);
                        host.UpdateLayout();
                        Assert.Equal(Visibility.Visible, workspace.Visibility);

                        grid.ItemsSource = new ObservableCollection<MediaItemDto> { visibleItems[1] };
                        host.UpdateLayout();
                        Assert.Equal(new[] { "media-a", "media-b" }, selection.Items.Select(item => item.MediaId));
                        Assert.Contains("first.png", summary.Text);
                        Assert.Contains("second.png", summary.Text);

                        Assert.True(clearButton.Command.CanExecute(null));
                        ExecuteButtonCommand(clearButton);
                        host.UpdateLayout();
                        Assert.Empty(selection.Items);
                        Assert.Contains("0/2", summary.Text);
                        Assert.False(clearButton.Command.CanExecute(null));
                    }
                }
                finally
                {
                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, recursive: true);
                }
            });
        }

        [Fact]
        public void AddKeepsAtMostTwoStableIdsAndDoesNotDependOnFilteredSourceCollection()
        {
            var selection = new MediaComparisonSelection();
            var currentMedia = new ObservableCollection<MediaItemDto>
            {
                Screenshot("media-a", @"C:\archive\a.png"),
                Screenshot("media-b", @"C:\archive\b.png"),
                Screenshot("media-c", @"C:\archive\c.png")
            };

            selection.Add(currentMedia, _ => true);
            currentMedia.Clear(); // A later filter or page refresh can replace the visible collection.

            Assert.Equal(new[] { "media-a", "media-b" }, selection.Items.Select(item => item.MediaId));
            Assert.Contains("图片比较对象 2/2", selection.Summary);
            Assert.Contains("超过两张上限", selection.Status);
        }

        [Fact]
        public void AddExplainsVideosAndMissingFilesAndChecksOnlySelectedScreenshotPaths()
        {
            var selection = new MediaComparisonSelection();
            var checkedPaths = new System.Collections.Generic.List<string>();
            var selected = new[]
            {
                new MediaItemDto { MediaId = "video", Kind = MediaKind.VideoClip, ArchivePath = @"C:\archive\clip.mp4" },
                Screenshot("missing", @"C:\archive\missing.png"),
                Screenshot("valid", @"C:\archive\valid.png")
            };

            selection.Add(selected, path =>
            {
                checkedPaths.Add(path);
                return path.EndsWith("valid.png", System.StringComparison.OrdinalIgnoreCase);
            });

            Assert.Equal(new[] { @"C:\archive\missing.png", @"C:\archive\valid.png" }, checkedPaths);
            Assert.Equal(new[] { "valid" }, selection.Items.Select(item => item.MediaId));
            Assert.Contains("不是截图", selection.Status);
            Assert.Contains("归档文件不存在", selection.Status);
        }

        [Fact]
        public void AddRejectsDuplicateIdsWithoutReplacingTheExistingComparisonObject()
        {
            var selection = new MediaComparisonSelection();
            var original = Screenshot("media-a", @"C:\archive\original.png");

            selection.Add(new[] { original }, _ => true);
            selection.Add(new[] { Screenshot("MEDIA-A", @"C:\archive\replacement.png") }, _ => true);

            Assert.Single(selection.Items);
            Assert.Same(original, selection.Items[0]);
            Assert.Contains("已在比较集合中", selection.Status);
        }

        [Fact]
        public void ClearAllowsAReplacementPairAndReportsTheEmptySelection()
        {
            var selection = new MediaComparisonSelection();
            selection.Add(new[] { Screenshot("media-a", @"C:\archive\a.png") }, _ => true);

            selection.Clear();

            Assert.Empty(selection.Items);
            Assert.Contains("0/2", selection.Summary);
            Assert.Contains("已清空", selection.Status);
        }

        private static MediaItemDto Screenshot(string id, string path)
            => new MediaItemDto
            {
                MediaId = id,
                Kind = MediaKind.Screenshot,
                ArchivePath = path,
                OriginalPath = path
            };

        private static DashboardViewModel CreateDashboard(MediaComparisonSelection selection)
        {
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
            SetAutoProperty(plugin, nameof(GameSaveCenterPlugin.Settings), new GameSaveCenterSettings());
            SetField(dashboard, "plugin", plugin);
            SetField(dashboard, "mediaComparisonSelection", selection);
            SetField(dashboard, "mediaComparisonCandidateNavigator", new MediaComparisonCandidateNavigator());
            SetAutoProperty(dashboard, nameof(DashboardViewModel.Media), new BatchObservableCollection<MediaItemDto>());
            SetField(dashboard, "unassignedMedia", new BatchObservableCollection<MediaItemDto>());
            SetField(dashboard, "ignoredMedia", new BatchObservableCollection<MediaItemDto>());
            var addMethod = typeof(DashboardViewModel).GetMethod("AddSelectedMediaToComparison", BindingFlags.Instance | BindingFlags.NonPublic);
            var clearMethod = typeof(DashboardViewModel).GetMethod("ClearMediaComparison", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(addMethod);
            Assert.NotNull(clearMethod);
            var add = (Action<object?>)addMethod!.CreateDelegate(typeof(Action<object?>), dashboard);
            var clear = (Action)clearMethod!.CreateDelegate(typeof(Action), dashboard);
            var toggleMethod = typeof(DashboardViewModel).GetMethod("ToggleMediaComparisonWorkspace", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(toggleMethod);
            var toggle = (Action)toggleMethod!.CreateDelegate(typeof(Action), dashboard);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.AddSelectedMediaToComparisonCommand), new RelayCommand(add));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.ClearMediaComparisonCommand), new RelayCommand(_ => clear(), _ => selection.Items.Count > 0));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.ToggleMediaComparisonWorkspaceCommand), new RelayCommand(_ => toggle(), _ => selection.Items.Count == MediaComparisonSelection.MaximumItems));
            return dashboard;
        }

        private static void ExecuteButtonCommand(System.Windows.Controls.Button button)
        {
            Assert.NotNull(button.Command);
            Assert.True(button.Command.CanExecute(button.CommandParameter));
            button.Command.Execute(button.CommandParameter);
        }

        private static T GetField<T>(Type type, object target, string name)
            => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

        private static void SetField(object target, string name, object value)
            => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

        private static void SetAutoProperty(object target, string propertyName, object value)
            => SetField(target, $"<{propertyName}>k__BackingField", value);

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                finally
                {
                    if (!Dispatcher.CurrentDispatcher.HasShutdownStarted)
                        Dispatcher.CurrentDispatcher.InvokeShutdown();
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

            public WindowHost(Window window)
            {
                this.window = window;
                this.window.Show();
                UpdateLayout();
            }

            public void UpdateLayout()
            {
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                window.UpdateLayout();
            }

            public void Dispose()
            {
                if (window.IsVisible)
                    window.Close();
            }
        }

    }
}
