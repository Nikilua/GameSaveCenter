using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public AcrylicProductionShellView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public IEnumerable<UserControl> WorkspaceViews => pages.Values;

        public FrameworkElement PageHostForAudit => PageHost;

        public TextBox GameSearchBoxForFocus => GameSearchTextBox;

        public void Attach(DashboardViewModel dashboardViewModel)
        {
            if (ReferenceEquals(viewModel, dashboardViewModel))
            {
                if (!viewModelSubscribed)
                {
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    viewModelSubscribed = true;
                }
                NavigateTo(dashboardViewModel.CurrentWorkspace);
                return;
            }

            if (viewModel != null && viewModelSubscribed)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModelSubscribed = false;
            }

            viewModel = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModelSubscribed = true;
            CreatePages();
            NavigateTo(viewModel.CurrentWorkspace);
        }

        public void NavigateTo(WorkspaceKind workspace)
        {
            if (viewModel == null) return;
            var page = GetPage(workspace);
            PageHost.Content = page;
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
                WorkspaceKind.Saves => "Elden Ring · 路径与恢复点状态",
                WorkspaceKind.Trainers => "修改器 · CT 表 · 自定义启动项",
                WorkspaceKind.Media => "截图与录像的自动归档",
                WorkspaceKind.Tasks => "备份 · 云端 · 媒体任务队列",
                WorkspaceKind.Maintenance => "诊断 · 设备 · 保留策略 · 审计",
                _ => "今日工作台 · 一切运行正常",
            };
            var gameScoped = workspace != WorkspaceKind.Tasks && workspace != WorkspaceKind.Maintenance;
            GameContextButton.Visibility = gameScoped ? Visibility.Visible : Visibility.Collapsed;
            HeaderMediaButton.Visibility = workspace == WorkspaceKind.Media ? Visibility.Visible : Visibility.Collapsed;
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
            ApplyHeaderLayout(ActualWidth);
            ApplyPageLayout();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel dashboardViewModel)
                Attach(dashboardViewModel);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModelSubscribed)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModelSubscribed = false;
            }
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
            if (!Enum.TryParse(button.Tag.ToString(), out WorkspaceKind workspace)) return;
            viewModel.CurrentWorkspace = workspace;
            viewModel.RequestWorkspaceLoad();
            NavigateTo(workspace);
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
        }

        private void OnGameContextClick(object sender, RoutedEventArgs e)
        {
            PickerOverlay.Visibility = PickerOverlay.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            if (PickerOverlay.Visibility == Visibility.Visible)
                GameSearchTextBox.Focus();
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
            ApplyHeaderLayout(e.NewSize.Width);
            ApplyPageLayout();
        }

        private void ApplyHeaderLayout(double width)
        {
            if (width <= 0 || HeaderLayoutGrid == null)
                return;

            // The production shell can be hosted by Playnite in a smaller logical
            // viewport than the standalone RenderHarness. Move the action row below
            // the title before the header starts asking its parent for a width larger
            // than the host. This is an explicit second row, not a hidden overflow fix.
            var compact = width < 980;
            var veryCompact = width < 720;
            Grid.SetRow(HeaderActionsPanel, compact ? 1 : 0);
            Grid.SetColumn(HeaderActionsPanel, compact ? 0 : 1);
            Grid.SetColumnSpan(HeaderActionsPanel, compact ? 2 : 1);
            Grid.SetColumn(HeaderTitlePanel, 0);
            Grid.SetColumnSpan(HeaderTitlePanel, compact ? 2 : 1);
            HeaderTitlePanel.HorizontalAlignment = HorizontalAlignment.Left;
            HeaderActionsRow.Height = compact ? GridLength.Auto : new GridLength(0);
            HeaderActionsPanel.HorizontalAlignment = compact
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
            HeaderActionsPanel.Margin = compact
                ? new Thickness(0, 8, 0, 0)
                : new Thickness(14, 0, 0, 0);

            // Keep the real game picker usable in the compact row while ensuring its
            // desired width plus the action buttons always fits the content column.
            var pickerWidth = compact ? (veryCompact ? 190d : 220d) : 300d;
            GameContextButton.Width = pickerWidth;
            GameContextButton.MinWidth = 0;
            GameContextButton.MaxWidth = pickerWidth;
        }

        private void ApplyPageLayout()
        {
            var width = PageHost.ActualWidth > 0 ? PageHost.ActualWidth : ActualWidth;
            var height = PageHost.ActualHeight > 0 ? PageHost.ActualHeight : ActualHeight;
            if (width <= 0 || height <= 0) return;
            if (pages.TryGetValue(WorkspaceKind.Overview, out var overview))
            {
                var view = (OverviewView)overview;
                view.ApplyResponsiveColumns(width < 1200);
                view.ApplyResponsiveWidth(width);
                view.ApplyResponsiveHeight(height, width < 1200);
            }
            if (pages.TryGetValue(WorkspaceKind.Saves, out var saves)) ((SaveCenterView)saves).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Trainers, out var trainers)) ((TrainerCenterView)trainers).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Media, out var media)) ((MediaCenterView)media).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Tasks, out var tasks)) ((TaskCenterView)tasks).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Maintenance, out var maintenance)) ((MaintenanceView)maintenance).ApplyResponsiveLayout(width, height);
        }
    }
}
