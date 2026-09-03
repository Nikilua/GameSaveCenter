using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>
    /// Production host using the AcrylicFork Demo shell geometry. The page content remains
    /// the real production views and therefore keeps the existing bindings and commands.
    /// </summary>
    public partial class AcrylicProductionShellView : UserControl
    {
        private readonly Dictionary<WorkspaceKind, UserControl> pages = new Dictionary<WorkspaceKind, UserControl>();
        private DashboardViewModel? viewModel;
        private bool viewModelSubscribed;
        private bool suppressNavigation;
        private bool sidebarCollapsed;
        private bool sidebarTransitionRunning;
        private bool responsiveLayoutPending;
        private double pendingResponsiveWidth;
        private bool pickerFilterRestorePending;

        public AcrylicProductionShellView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public IEnumerable<UserControl> WorkspaceViews => pages.Values;

        /// <summary>
        /// Returns the single page instance owned by the visible production PageHost.
        /// Dashboard compatibility code must use this registry instead of reaching into
        /// the collapsed legacy DashboardView tree.
        /// </summary>
        public UserControl? GetWorkspaceView(WorkspaceKind workspace)
            => pages.TryGetValue(workspace, out var page) ? page : null;

        public T? GetWorkspaceView<T>(WorkspaceKind workspace) where T : UserControl
            => GetWorkspaceView(workspace) as T;

        public FrameworkElement PageHostForAudit => PageHost;

        public TextBox GameSearchBoxForFocus => GameSearchTextBox;

        public Action? SettingsRequested { get; set; }

        /// <summary>
        /// The parent dashboard owns the persisted animation preference.  Keep the shell
        /// independent from the plugin instance while still respecting that preference.
        /// </summary>
        public Func<bool>? MotionEnabledProvider { get; set; }

        /// <summary>Reads the shell chrome preference without coupling this view to the plugin.</summary>
        public Func<bool>? SidebarCollapsedProvider { get; set; }

        /// <summary>Persists a changed shell chrome preference in the parent dashboard.</summary>
        public Action<bool>? SidebarCollapsedChanged { get; set; }

        private void OnClearGameSearchClick(object sender, RoutedEventArgs e)
        {
            GameSearchTextBox.Clear();
            GameSearchTextBox.Focus();
            Keyboard.Focus(GameSearchTextBox);
            e.Handled = true;
        }

        public void Attach(DashboardViewModel dashboardViewModel)
        {
            if (ReferenceEquals(viewModel, dashboardViewModel))
            {
                if (!viewModelSubscribed)
                {
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    viewModel.GamePicker.PlatformFilterOptions.CollectionChanged += OnGamePickerPlatformOptionsChanged;
                    viewModelSubscribed = true;
                }
                NavigateTo(dashboardViewModel.CurrentWorkspace);
                RestoreSidebarState();
                QueueGamePickerFilterDefaults();
                return;
            }

            if (viewModel != null && viewModelSubscribed)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModel.GamePicker.PlatformFilterOptions.CollectionChanged -= OnGamePickerPlatformOptionsChanged;
                viewModelSubscribed = false;
            }

            viewModel = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.GamePicker.PlatformFilterOptions.CollectionChanged += OnGamePickerPlatformOptionsChanged;
            viewModelSubscribed = true;
            CreatePages();
            NavigateTo(viewModel.CurrentWorkspace);
            RestoreSidebarState();
            QueueGamePickerFilterDefaults();
        }

        public void NavigateTo(WorkspaceKind workspace)
        {
            if (viewModel == null) return;
            var page = GetPage(workspace);
            PageHost.Content = page;
            UpdatePageHeader(workspace);
            var gameScoped = workspace != WorkspaceKind.Tasks && workspace != WorkspaceKind.Maintenance;
            GameContextButton.Visibility = gameScoped ? Visibility.Visible : Visibility.Collapsed;
            HeaderMediaButton.Visibility = workspace == WorkspaceKind.Media ? Visibility.Visible : Visibility.Collapsed;
            HeaderBackupSelectedButton.Visibility = workspace == WorkspaceKind.Saves ? Visibility.Visible : Visibility.Collapsed;
            HeaderBackupButton.Visibility = Visibility.Visible;
            HeaderRefreshButton.Visibility = Visibility.Visible;

            suppressNavigation = true;
            try
            {
                GetNavigation(workspace).IsChecked = true;
            }
            finally
            {
                suppressNavigation = false;
            }
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSidebarVersion();
            if (DataContext is DashboardViewModel dashboardViewModel)
                Attach(dashboardViewModel);
            RestoreSidebarState();
            QueueGamePickerFilterDefaults();
        }

        private void UpdateSidebarVersion()
        {
            var version = typeof(AcrylicProductionShellView).Assembly.GetName().Version;
            SidebarProductionVersionText.Text = version == null
                ? "开发预览"
                : "v" + version.ToString(3);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
            if (SidebarContentLayer.RenderTransform is TranslateTransform translate)
                translate.BeginAnimation(TranslateTransform.XProperty, null);
            SidebarContentLayer.Opacity = 1;
            sidebarTransitionRunning = false;
            responsiveLayoutPending = false;
            pickerFilterRestorePending = false;
            if (viewModel != null && viewModelSubscribed)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModel.GamePicker.PlatformFilterOptions.CollectionChanged -= OnGamePickerPlatformOptionsChanged;
                viewModelSubscribed = false;
            }
        }

        private void OnGamePickerPlatformOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            QueueGamePickerFilterDefaults();
        }

        private void RestoreGamePickerFilterDefaults()
        {
            if (viewModel?.GamePicker == null)
                return;

            UiFilterSelection.Synchronize(GamePickerStatusComboBox, viewModel.GamePicker.StatusFilter);
            UiFilterSelection.Synchronize(GamePickerPlatformComboBox, viewModel.GamePicker.PlatformFilter);
            UiFilterSelection.Synchronize(GamePickerSortComboBox, viewModel.GamePicker.SortMode);
        }

        private void OnGamePickerFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel?.GamePicker == null || !(sender is ComboBox combo) || !(combo.SelectedItem is string value))
                return;

            // These ComboBoxes are deliberately OneWay-bound because two copies of the
            // picker exist in the responsive shell. SelectionChanged is the only user-input
            // path, so a platform-list reset cannot write a stale default back into the VM.
            if (ReferenceEquals(combo, GamePickerStatusComboBox))
                viewModel.GamePicker.StatusFilter = value;
            else if (ReferenceEquals(combo, GamePickerPlatformComboBox))
                viewModel.GamePicker.PlatformFilter = value;
            else if (ReferenceEquals(combo, GamePickerSortComboBox))
                viewModel.GamePicker.SortMode = value;
        }

        private void QueueGamePickerFilterDefaults()
        {
            RestoreGamePickerFilterDefaults();
            if (pickerFilterRestorePending || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
                return;

            pickerFilterRestorePending = true;
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    pickerFilterRestorePending = false;
                    if (IsLoaded) RestoreGamePickerFilterDefaults();
                }), DispatcherPriority.Loaded);
            }
            catch (InvalidOperationException)
            {
                pickerFilterRestorePending = false;
            }
        }

        private void OnGamePickerFilterLoaded(object sender, RoutedEventArgs e)
        {
            QueueGamePickerFilterDefaults();
        }

        private void CreatePages()
        {
            pages.Clear();
            pages[WorkspaceKind.Overview] = CreatePage(new OverviewView());
            pages[WorkspaceKind.Saves] = CreatePage(new SaveCenterView());
            pages[WorkspaceKind.Trainers] = CreatePage(new TrainerCenterView());
            pages[WorkspaceKind.Media] = CreatePage(new MediaCenterView());
            pages[WorkspaceKind.Tasks] = CreatePage(new TaskCenterView());
            pages[WorkspaceKind.Maintenance] = CreatePage(new MaintenanceView());
        }

        private UserControl CreatePage(UserControl page)
        {
            page.DataContext = viewModel;
            return page;
        }

        private UserControl GetPage(WorkspaceKind workspace)
            => pages.TryGetValue(workspace, out var page) ? page : pages[WorkspaceKind.Overview];

        private RadioButton GetNavigation(WorkspaceKind workspace)
            => workspace switch
            {
                WorkspaceKind.Saves => NavSaves,
                WorkspaceKind.Trainers => NavTrainers,
                WorkspaceKind.Media => NavMedia,
                WorkspaceKind.Tasks => NavTasks,
                WorkspaceKind.Maintenance => NavMaintenance,
                _ => NavOverview,
            };

        private void OnNavChecked(object sender, RoutedEventArgs e)
        {
            if (suppressNavigation || viewModel == null || sender is not RadioButton button || button.Tag == null)
                return;
            if (ReferenceEquals(button, NavSettings))
            {
                suppressNavigation = true;
                try
                {
                    GetNavigation(viewModel.CurrentWorkspace).IsChecked = true;
                }
                finally
                {
                    suppressNavigation = false;
                }
                SettingsRequested?.Invoke();
                return;
            }
            if (!Enum.TryParse(button.Tag.ToString(), out WorkspaceKind workspace)) return;
            viewModel.CurrentWorkspace = workspace;
            viewModel.RequestWorkspaceLoad();
            NavigateTo(workspace);
        }

        private void OnSidebarCollapseClick(object sender, RoutedEventArgs e)
        {
            if (sidebarTransitionRunning)
            {
                e.Handled = true;
                return;
            }

            sidebarCollapsed = !sidebarCollapsed;
            SidebarCollapsedChanged?.Invoke(sidebarCollapsed);
            if (!IsSidebarMotionEnabled)
            {
                ApplySidebarLayout();
                SidebarCollapseButton.Focus();
                e.Handled = true;
                return;
            }

            sidebarTransitionRunning = true;
            var currentWidth = SidebarColumn.ActualWidth > 0
                ? SidebarColumn.ActualWidth
                : SidebarColumn.Width.Value;
            var targetWidth = sidebarCollapsed ? 72d : 270d;
            var translate = SidebarContentLayer.RenderTransform as TranslateTransform ?? new TranslateTransform();
            SidebarContentLayer.RenderTransform = translate;
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
            translate.BeginAnimation(TranslateTransform.XProperty, null);
            SidebarContentLayer.Opacity = 0;
            translate.X = sidebarCollapsed ? -4 : 4;
            ApplySidebarLayout(updateColumnWidth: false);
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
            SidebarColumn.Width = new GridLength(currentWidth, GridUnitType.Pixel);

            var widthAnimation = new GridLengthAnimation
            {
                From = new GridLength(currentWidth, GridUnitType.Pixel),
                To = new GridLength(targetWidth, GridUnitType.Pixel),
                Duration = new Duration(TimeSpan.FromMilliseconds(210)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            widthAnimation.Completed += (_, _) =>
            {
                SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
                ApplySidebarLayout();
                SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
                SidebarContentLayer.Opacity = 1;
                translate.BeginAnimation(TranslateTransform.XProperty, null);
                translate.X = 0;
                SidebarColumn.Width = new GridLength(targetWidth, GridUnitType.Pixel);
                sidebarTransitionRunning = false;
            };
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, widthAnimation);
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(190))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            translate.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(sidebarCollapsed ? -4 : 4, 0, TimeSpan.FromMilliseconds(190))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            SidebarCollapseButton.Focus();
            e.Handled = true;
        }

        private bool IsSidebarMotionEnabled
            => (MotionEnabledProvider?.Invoke() ?? true)
               && !SystemParameters.HighContrast
               && SystemParameters.ClientAreaAnimation;

        private void RestoreSidebarState()
        {
            sidebarCollapsed = SidebarCollapsedProvider?.Invoke() ?? false;
            ApplySidebarLayout();
        }

        private void ApplySidebarLayout(bool updateColumnWidth = true)
        {
            var expanded = !sidebarCollapsed;
            if (!sidebarTransitionRunning)
            {
                SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
                SidebarContentLayer.Opacity = 1;
                if (SidebarContentLayer.RenderTransform is TranslateTransform translate)
                {
                    translate.BeginAnimation(TranslateTransform.XProperty, null);
                    translate.X = 0;
                }
            }
            if (updateColumnWidth)
                SidebarColumn.Width = new GridLength(sidebarCollapsed ? 72 : 270, GridUnitType.Pixel);

            SidebarBrandText.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            SidebarProductionBadge.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            SidebarHeaderLayout.Margin = expanded
                ? new Thickness(14, 0, 8, 0)
                : new Thickness(0);
            SidebarBrandContent.HorizontalAlignment = expanded
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Center;
            SidebarBrandContent.Width = expanded ? double.NaN : 26;

            var labelVisibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            NavOverviewLabel.Visibility = labelVisibility;
            NavSavesLabel.Visibility = labelVisibility;
            NavTrainersLabel.Visibility = labelVisibility;
            NavMediaLabel.Visibility = labelVisibility;
            NavTasksLabel.Visibility = labelVisibility;
            NavMaintenanceLabel.Visibility = labelVisibility;
            NavSettingsLabel.Visibility = labelVisibility;

            foreach (var content in new[]
                     {
                         NavOverviewContent, NavSavesContent, NavTrainersContent, NavMediaContent,
                         NavTasksContent, NavMaintenanceContent, NavSettingsContent
                     })
            {
                content.HorizontalAlignment = expanded
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Center;
                content.Width = expanded ? double.NaN : 26;
                content.VerticalAlignment = VerticalAlignment.Center;
            }

            var navigationPadding = expanded ? new Thickness(12, 10, 12, 10) : new Thickness(0, 10, 0, 10);
            foreach (var item in new[] { NavOverview, NavSaves, NavTrainers, NavMedia, NavTasks, NavMaintenance, NavSettings })
            {
                item.Padding = navigationPadding;
                item.Width = expanded ? double.NaN : 48;
                item.HorizontalAlignment = expanded ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
                item.HorizontalContentAlignment = expanded ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
            }

            SidebarCollapseButton.Width = 32;
            SidebarCollapseButton.Height = 32;
            SidebarCollapseButton.HorizontalAlignment = HorizontalAlignment.Center;
            SidebarCollapseButton.VerticalAlignment = VerticalAlignment.Center;
            SidebarCollapseGlyph.Text = expanded ? "‹" : "›";
            SidebarCollapseButton.ToolTip = expanded ? "收起导航栏" : "展开导航栏";
            AutomationProperties.SetName(SidebarCollapseButton, expanded ? "收起导航栏" : "展开导航栏");

            // The column is part of the shell chrome, so let the existing page-aware
            // layout pass recompute the available workspace width after the toggle.
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnViewModelPropertyChanged(sender, e)));
                return;
            }
            if (e.PropertyName == nameof(DashboardViewModel.CurrentWorkspace) && viewModel != null)
                NavigateTo(viewModel.CurrentWorkspace);
            else if (e.PropertyName == nameof(DashboardViewModel.SelectedGame) && viewModel != null)
                UpdatePageHeader(viewModel.CurrentWorkspace);
        }

        private void UpdatePageHeader(WorkspaceKind workspace)
        {
            PageTitleText.Text = workspace switch
            {
                WorkspaceKind.Saves => "存档中心",
                WorkspaceKind.Trainers => "修改器中心",
                WorkspaceKind.Media => "媒体中心",
                WorkspaceKind.Tasks => "任务中心",
                WorkspaceKind.Maintenance => "维护中心",
                _ => "首页",
            };
            PageSubtitleText.Text = workspace switch
            {
                WorkspaceKind.Saves => $"{viewModel?.SelectedGame?.Name ?? "未选择游戏"} · 路径与恢复点状态",
                WorkspaceKind.Trainers => "修改器 · CT 表 · 自定义启动项",
                WorkspaceKind.Media => "截图与录像的自动归档",
                WorkspaceKind.Tasks => "备份 · 云端 · 媒体任务队列",
                WorkspaceKind.Maintenance => "诊断 · 设备 · 保留策略 · 审计",
                _ => "今日工作台 · 一切运行正常",
            };
        }

        private void OnGameContextClick(object sender, RoutedEventArgs e)
        {
            PickerOverlay.Visibility = PickerOverlay.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            if (PickerOverlay.Visibility == Visibility.Visible)
            {
                QueueGamePickerFilterDefaults();
                GameSearchTextBox.Focus();
            }
        }

        private void OnPickerScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            PickerOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void OnPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null || e.AddedItems.Count == 0) return;
            if (e.AddedItems[0] is GamePickerItem item)
            {
                viewModel.SelectedGame = item.Game;
                PickerOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void OnShellSizeChanged(object sender, SizeChangedEventArgs e)
        {
            pendingResponsiveWidth = e.NewSize.Width;
            if (responsiveLayoutPending || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
                return;

            responsiveLayoutPending = true;
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    responsiveLayoutPending = false;
                    if (!IsLoaded) return;
                    ApplyResponsiveLayout(pendingResponsiveWidth, ActualHeight);
                }), DispatcherPriority.Render);
            }
            catch (InvalidOperationException)
            {
                responsiveLayoutPending = false;
            }
        }

        private void ApplyHeaderLayout(double width)
        {
            if (width <= 0 || HeaderLayoutGrid == null)
                return;

            // The production shell can be hosted by Playnite in a smaller logical
            // viewport than the standalone RenderHarness. Move the action row below
            // the title before the header starts asking its parent for a width larger
            // than the host. This is an explicit second row, not a hidden overflow fix.
            var layout = ResponsiveLayoutCoordinator.Calculate(width, ActualHeight);
            var compact = layout.IsCompactShellHeader;
            HeaderRow.Height = compact ? GridLength.Auto : new GridLength(68);
            Grid.SetRow(HeaderActionsPanel, compact ? 1 : 0);
            Grid.SetColumn(HeaderActionsPanel, compact ? 0 : 1);
            Grid.SetColumnSpan(HeaderActionsPanel, compact ? 2 : 1);
            Grid.SetColumn(HeaderTitlePanel, 0);
            Grid.SetColumnSpan(HeaderTitlePanel, compact ? 2 : 1);
            HeaderTitlePanel.HorizontalAlignment = HorizontalAlignment.Left;
            HeaderActionsRow.Height = compact ? GridLength.Auto : new GridLength(0);
            HeaderActionsPanel.HorizontalAlignment = compact
                ? HorizontalAlignment.Stretch
                : HorizontalAlignment.Right;
            HeaderActionsPanel.Margin = compact
                ? new Thickness(0, 8, 0, 0)
                : new Thickness(14, 0, 0, 0);
            if (compact)
            {
                var sidebarWidth = SidebarColumn.ActualWidth > 0
                    ? SidebarColumn.ActualWidth
                    : SidebarColumn.Width.Value;
                var layoutWidth = HeaderLayoutGrid.ActualWidth > 0
                    ? HeaderLayoutGrid.ActualWidth
                    : Math.Max(0, width - 8 - sidebarWidth - 38);
                HeaderActionsPanel.Width = layoutWidth;
            }
            else
            {
                HeaderActionsPanel.Width = double.NaN;
            }

            // Keep the real game picker usable in the compact row while ensuring its
            // desired width plus the action buttons always fits the content column.
            var pickerWidth = layout.ShellPickerWidth;
            GameContextButton.Width = pickerWidth;
            GameContextButton.MinWidth = 0;
            GameContextButton.MaxWidth = pickerWidth;
        }

        /// <summary>
        /// Applies the responsive layout to the pages owned by the visible PageHost.
        /// The fallback dimensions are used only during the first measure pass.
        /// </summary>
        public void ApplyResponsiveLayout(double width, double height)
        {
            var effectiveWidth = width > 0 ? width : ActualWidth;
            var effectiveHeight = height > 0 ? height : ActualHeight;
            ApplyHeaderLayout(effectiveWidth);
            ApplyPageLayout(effectiveWidth, effectiveHeight);
        }

        private void ApplyPageLayout(double fallbackWidth, double fallbackHeight)
        {
            var width = PageHost.ActualWidth > 0
                ? PageHost.ActualWidth
                : fallbackWidth > 0 ? fallbackWidth : ActualWidth;
            var height = PageHost.ActualHeight > 0
                ? PageHost.ActualHeight
                : fallbackHeight > 0 ? fallbackHeight : ActualHeight;
            if (width <= 0 || height <= 0) return;
            var layout = ResponsiveLayoutCoordinator.Calculate(width, height);
            if (pages.TryGetValue(WorkspaceKind.Overview, out var overview))
            {
                var view = (OverviewView)overview;
                view.ApplyResponsiveColumns(layout.OverviewUsesStackedColumns);
                view.ApplyResponsiveWidth(width);
                view.ApplyResponsiveHeight(height, layout.OverviewUsesStackedColumns);
            }
            if (pages.TryGetValue(WorkspaceKind.Saves, out var saves)) ((SaveCenterView)saves).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Trainers, out var trainers)) ((TrainerCenterView)trainers).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Media, out var media)) ((MediaCenterView)media).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Tasks, out var tasks)) ((TaskCenterView)tasks).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Maintenance, out var maintenance)) ((MaintenanceView)maintenance).ApplyResponsiveLayout(width, height);
        }
    }
}
