using System;
using System.Diagnostics;
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
        private static readonly global::Playnite.SDK.ILogger Logger = global::Playnite.SDK.LogManager.GetLogger();
        private readonly Dictionary<WorkspaceKind, UserControl> pages = new Dictionary<WorkspaceKind, UserControl>();
        private readonly HashSet<WorkspaceKind> activatedWorkspaces = new HashSet<WorkspaceKind>();
        private IReadOnlyList<CommandDirectoryItem> commandDirectoryItems = Array.Empty<CommandDirectoryItem>();
        private DashboardViewModel? viewModel;
        private bool viewModelSubscribed;
        private bool suppressNavigation;
        private bool sidebarCollapsed;
        private bool sidebarTransitionRunning;
        private int sidebarTransitionGeneration;
        private bool responsiveLayoutPending;
        private double pendingResponsiveWidth;
        private bool pickerFilterRestorePending;
        private bool gameSearchCompositionActive;
        private bool commandSearchCompositionActive;
        private bool pickerKeyboardNavigationActive;
        private int pickerKeyboardNavigationGeneration;
        private Action? settingsRequested;
        private Action? focusWorkspaceSearchRequested;
        private readonly WrapPanelRowGapController headerActionRowGap;
        private int measurePasses;
        private int arrangePasses;

        public AcrylicProductionShellView()
        {
            InitializeComponent();
            headerActionRowGap = new WrapPanelRowGapController(HeaderActionsPanel);
            FocusWorkspaceSearchCommand = new RelayCommand(
                _ => focusWorkspaceSearchRequested?.Invoke(),
                _ => focusWorkspaceSearchRequested != null);
            NavigateWorkspaceCommand = new RelayCommand(
                NavigateWorkspaceFromCommand,
                value => viewModel != null && value is WorkspaceKind workspace
                    && Enum.IsDefined(typeof(WorkspaceKind), workspace));
            OpenSettingsCommand = new RelayCommand(
                _ => SettingsRequested?.Invoke(),
                _ => viewModel != null && SettingsRequested != null);
            OpenGamePickerCommand = new RelayCommand(
                _ => OpenGamePicker(),
                _ => viewModel != null && GameContextButton.IsVisible);
            TextCompositionManager.AddPreviewTextInputStartHandler(GameSearchTextBox, OnGameSearchCompositionStarted);
            TextCompositionManager.AddPreviewTextInputUpdateHandler(GameSearchTextBox, OnGameSearchCompositionUpdated);
            TextCompositionManager.AddPreviewTextInputHandler(GameSearchTextBox, OnGameSearchTextInput);
            TextCompositionManager.AddTextInputHandler(GameSearchTextBox, OnGameSearchTextInput);
            TextCompositionManager.AddPreviewTextInputStartHandler(CommandDirectorySearchTextBox, OnCommandSearchCompositionStarted);
            TextCompositionManager.AddPreviewTextInputUpdateHandler(CommandDirectorySearchTextBox, OnCommandSearchCompositionUpdated);
            TextCompositionManager.AddPreviewTextInputHandler(CommandDirectorySearchTextBox, OnCommandSearchTextInput);
            TextCompositionManager.AddTextInputHandler(CommandDirectorySearchTextBox, OnCommandSearchTextInput);
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
        internal bool IsGamePickerOpen => PickerOverlay.Visibility == Visibility.Visible;
        public ICommand FocusWorkspaceSearchCommand { get; }
        public ICommand NavigateWorkspaceCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenGamePickerCommand { get; }
        internal IReadOnlyList<CommandDirectoryItem> CommandDirectoryItems => commandDirectoryItems;
        public Action? FocusWorkspaceSearchRequested
        {
            get => focusWorkspaceSearchRequested;
            set
            {
                focusWorkspaceSearchRequested = value;
                NotifyCommandDirectoryCanExecuteChanged();
            }
        }
        internal IReadOnlyList<KeyboardShortcutHelpItem> KeyboardShortcutHelpItems
            => viewModel == null
                ? Array.Empty<KeyboardShortcutHelpItem>()
                : KeyboardShortcutHelpCatalog.Create(viewModel.CurrentWorkspace, FocusWorkspaceSearchCommand);

        public TextBox GameSearchBoxForFocus => GameSearchTextBox;

        public void OpenGamePicker()
        {
            if (viewModel == null || !GameContextButton.IsVisible)
                return;

            CommandDirectoryOverlay.Visibility = Visibility.Collapsed;
            PickerOverlay.Visibility = Visibility.Visible;
            QueueGamePickerFilterDefaults();
            GameSearchTextBox.Focus();
            Keyboard.Focus(GameSearchTextBox);
        }

        private void OnCommandDirectoryClick(object sender, RoutedEventArgs e)
        {
            if (CommandDirectoryOverlay.Visibility == Visibility.Visible)
            {
                CloseCommandDirectory(restoreFocus: true);
                e.Handled = true;
                return;
            }

            if (viewModel == null)
                return;

            if (PickerOverlay.Visibility == Visibility.Visible)
                ClosePickerAndRestoreFocus();
            KeyboardShortcutHelpPopup.IsOpen = false;
            CommandDirectorySearchTextBox.Clear();
            UpdateCommandDirectorySearchResults();
            CommandDirectoryOverlay.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (CommandDirectoryOverlay.Visibility != Visibility.Visible) return;
                CommandDirectorySearchTextBox.Focus();
                Keyboard.Focus(CommandDirectorySearchTextBox);
            }), DispatcherPriority.Input);
            e.Handled = true;
        }

        private void OnCommandDirectorySearchTextChanged(object sender, TextChangedEventArgs e)
            => UpdateCommandDirectorySearchResults();

        private void UpdateCommandDirectorySearchResults()
        {
            if (CommandDirectoryResultsListBox == null || CommandDirectorySearchTextBox == null)
                return;

            var query = CommandDirectorySearchTextBox.Text;
            var results = CommandDirectorySearch.Filter(commandDirectoryItems, query);
            CommandDirectoryResultsListBox.ItemsSource = results;
            CommandDirectoryResultsSummaryText.Text = string.IsNullOrWhiteSpace(query)
                ? $"常用命令 · {results.Count}"
                : results.Count == 0 ? "没有匹配的命令" : $"搜索结果 · {results.Count}";
        }

        private void OnCommandDirectoryPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CommandDirectoryOverlay.Visibility != Visibility.Visible
                || commandSearchCompositionActive
                || e.Key == Key.ImeProcessed
                || e.ImeProcessedKey != Key.None
                || e.Key != Key.Escape)
                return;

            CloseCommandDirectory(restoreFocus: true);
            e.Handled = true;
        }

        private void OnCommandDirectoryResultClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement)?.DataContext as CommandDirectoryItem;
            var commandOwnsFocus = item?.Id == "game-picker.open"
                || item?.Id == "workspace.focus-search"
                || item?.Id == "settings.open";
            CloseCommandDirectory(restoreFocus: !commandOwnsFocus);
            e.Handled = true;
        }

        private void OnCommandDirectoryScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseCommandDirectory(restoreFocus: false);
            e.Handled = true;
        }

        private void OnCommandDirectoryCloseClick(object sender, RoutedEventArgs e)
        {
            CloseCommandDirectory(restoreFocus: true);
            e.Handled = true;
        }

        private void OnCommandSearchCompositionStarted(object sender, TextCompositionEventArgs e)
            => commandSearchCompositionActive = true;

        private void OnCommandSearchCompositionUpdated(object sender, TextCompositionEventArgs e)
            => commandSearchCompositionActive = true;

        private void OnCommandSearchTextInput(object sender, TextCompositionEventArgs e)
            => commandSearchCompositionActive = false;

        private void CloseCommandDirectory(bool restoreFocus)
        {
            CommandDirectoryOverlay.Visibility = Visibility.Collapsed;
            if (!restoreFocus)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsLoaded) return;
                HeaderCommandDirectoryButton.Focus();
                Keyboard.Focus(HeaderCommandDirectoryButton);
            }), DispatcherPriority.Input);
        }

        public Action? SettingsRequested
        {
            get => settingsRequested;
            set
            {
                settingsRequested = value;
                NotifyCommandDirectoryCanExecuteChanged();
            }
        }

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
                    viewModel.Games.CollectionChanged += OnGamesCollectionChanged;
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
                viewModel.Games.CollectionChanged -= OnGamesCollectionChanged;
                viewModelSubscribed = false;
            }

            viewModel = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.GamePicker.PlatformFilterOptions.CollectionChanged += OnGamePickerPlatformOptionsChanged;
            viewModel.Games.CollectionChanged += OnGamesCollectionChanged;
            viewModelSubscribed = true;
            CreatePages();
            commandDirectoryItems = CommandDirectoryCatalog.Create(
                viewModel,
                NavigateWorkspaceCommand,
                OpenSettingsCommand,
                OpenGamePickerCommand,
                FocusWorkspaceSearchCommand);
            NavigateTo(viewModel.CurrentWorkspace);
            RestoreSidebarState();
            QueueGamePickerFilterDefaults();
        }

        public void NavigateTo(WorkspaceKind workspace)
        {
            if (viewModel == null) return;
            var timer = Stopwatch.StartNew();
            var page = GetPage(workspace);
            var firstActivation = activatedWorkspaces.Add(workspace);
            var contentChanged = !ReferenceEquals(PageHost.Content, page);
            // Keep a same-page navigation request on the existing visual tree. The
            // workspace command may be raised again while a refresh is completing;
            // reassigning the same cached page would otherwise make the host perform
            // an avoidable content transition and could disturb a nested scroll owner.
            if (contentChanged)
                PageHost.Content = page;
            UpdatePageHeader(workspace);
            var gameScoped = workspace != WorkspaceKind.Tasks
                && workspace != WorkspaceKind.Maintenance
                && workspace != WorkspaceKind.Notifications;
            GameContextButton.Visibility = gameScoped ? Visibility.Visible : Visibility.Collapsed;
            HeaderMediaButton.Visibility = workspace == WorkspaceKind.Media ? Visibility.Visible : Visibility.Collapsed;
            HeaderBackupSelectedButton.Visibility = workspace == WorkspaceKind.Saves ? Visibility.Visible : Visibility.Collapsed;
            HeaderBackupButton.Visibility = workspace == WorkspaceKind.Notifications ? Visibility.Collapsed : Visibility.Visible;
            HeaderRefreshButton.Visibility = workspace != WorkspaceKind.Trainers
                && workspace != WorkspaceKind.Maintenance
                && workspace != WorkspaceKind.Notifications
                ? Visibility.Visible : Visibility.Collapsed;
            UpdateNavigationReturnButton();

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
            timer.Stop();
            Logger.Debug($"[PERF] WorkspaceActivation workspace={workspace} phase={(firstActivation ? "first" : "revisit")} page={(contentChanged ? "attach" : "reuse")} layout={timer.Elapsed.TotalMilliseconds:F3}ms");
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
            sidebarTransitionGeneration++;
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
            if (SidebarContentLayer.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.XProperty, null);
                translate.X = 0;
            }
            SidebarContentLayer.Opacity = 1;
            sidebarTransitionRunning = false;
            responsiveLayoutPending = false;
            pickerFilterRestorePending = false;
            gameSearchCompositionActive = false;
            pickerKeyboardNavigationActive = false;
            pickerKeyboardNavigationGeneration++;
            if (viewModel != null && viewModelSubscribed)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModel.GamePicker.PlatformFilterOptions.CollectionChanged -= OnGamePickerPlatformOptionsChanged;
                viewModel.Games.CollectionChanged -= OnGamesCollectionChanged;
                viewModelSubscribed = false;
            }
        }

        private void OnGamePickerPlatformOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            QueueGamePickerFilterDefaults();
        }

        private void OnGamesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(NotifyCommandDirectoryCanExecuteChanged));
                return;
            }
            NotifyCommandDirectoryCanExecuteChanged();
        }

        private void RestoreGamePickerFilterDefaults()
        {
            if (viewModel?.GamePicker == null)
                return;

            UiFilterSelection.Synchronize(GamePickerStatusComboBox, viewModel.GamePicker.StatusFilter);
            UiFilterSelection.Synchronize(GamePickerPlatformComboBox, viewModel.GamePicker.PlatformFilter);
            UiFilterSelection.Synchronize(GamePickerSortComboBox, viewModel.GamePicker.SortMode);
        }

        private void OnGamePickerFilterDropDownClosed(object sender, EventArgs e)
        {
            if (viewModel?.GamePicker == null || !(sender is ComboBox combo) || !(combo.SelectedItem is string value))
                return;

            // These ComboBoxes are deliberately OneWay-bound because two copies of the
            // picker exist in the responsive shell. SelectionChanged also fires for binding
            // and ItemsSource changes, so only DropDownClosed is allowed to write a user choice
            // back into the shared VM.
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
            var timer = Stopwatch.StartNew();
            pages.Clear();
            activatedWorkspaces.Clear();
            pages[WorkspaceKind.Overview] = CreatePage(new OverviewView());
            pages[WorkspaceKind.Saves] = CreatePage(new SaveCenterView());
            pages[WorkspaceKind.Trainers] = CreatePage(new TrainerCenterView());
            pages[WorkspaceKind.Media] = CreatePage(new MediaCenterView());
            pages[WorkspaceKind.Tasks] = CreatePage(new TaskCenterView());
            pages[WorkspaceKind.Notifications] = CreatePage(new NotificationInboxView());
            pages[WorkspaceKind.Maintenance] = CreatePage(new MaintenanceView());
            timer.Stop();
            Logger.Debug($"[PERF] WorkspacePages created={pages.Count} binding={timer.Elapsed.TotalMilliseconds:F3}ms");
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
                WorkspaceKind.Notifications => NavNotifications,
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
                if (OpenSettingsCommand.CanExecute(null))
                    OpenSettingsCommand.Execute(null);
                return;
            }
            if (!Enum.TryParse(button.Tag.ToString(), out WorkspaceKind workspace)) return;
            if (NavigateWorkspaceCommand.CanExecute(workspace))
                NavigateWorkspaceCommand.Execute(workspace);
        }

        private void NavigateWorkspaceFromCommand(object? value)
        {
            if (viewModel == null || !(value is WorkspaceKind workspace)
                || !Enum.IsDefined(typeof(WorkspaceKind), workspace))
                return;

            viewModel.CurrentWorkspace = workspace;
            viewModel.RequestWorkspaceLoad();
            NavigateTo(workspace);
            NotifyCommandDirectoryCanExecuteChanged();
        }

        private void OnSidebarCollapseClick(object sender, RoutedEventArgs e)
        {
            sidebarCollapsed = !sidebarCollapsed;
            SidebarCollapsedChanged?.Invoke(sidebarCollapsed);
            if (!IsSidebarMotionEnabled)
            {
                NormalizeMotionIfDisabled();
                SidebarCollapseButton.Focus();
                e.Handled = true;
                return;
            }

            var currentWidth = SidebarColumn.ActualWidth > 0
                ? SidebarColumn.ActualWidth
                : SidebarColumn.Width.Value;
            var targetWidth = sidebarCollapsed ? 72d : 270d;
            // A second click while the previous transition is running becomes the new
            // target. Capture the currently rendered width before cancelling the old
            // clock so rapid input never gets stuck on an obsolete visual state.
            var transitionGeneration = ++sidebarTransitionGeneration;
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
            var translate = SidebarContentLayer.RenderTransform as TranslateTransform ?? new TranslateTransform();
            SidebarContentLayer.RenderTransform = translate;
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
            translate.BeginAnimation(TranslateTransform.XProperty, null);
            sidebarTransitionRunning = true;
            SidebarContentLayer.Opacity = 0;
            translate.X = sidebarCollapsed ? -4 : 4;
            ApplySidebarLayout(updateColumnWidth: false);
            SidebarColumn.Width = new GridLength(currentWidth, GridUnitType.Pixel);
            var motionDuration = GscMotion.GetDuration(SidebarContentLayer, GscMotion.MotionDurationKind.Normal);

            var widthAnimation = new GridLengthAnimation
            {
                From = new GridLength(currentWidth, GridUnitType.Pixel),
                To = new GridLength(targetWidth, GridUnitType.Pixel),
                Duration = new Duration(motionDuration),
                EasingFunction = GscMotion.CreateEaseOut(),
                FillBehavior = FillBehavior.HoldEnd
            };
            widthAnimation.Completed += (_, _) =>
            {
                if (transitionGeneration != sidebarTransitionGeneration)
                    return;
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
                new DoubleAnimation(0, 1, motionDuration)
                {
                    EasingFunction = GscMotion.CreateEaseOut(),
                    FillBehavior = FillBehavior.HoldEnd
                });
            translate.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(sidebarCollapsed ? -4 : 4, 0, motionDuration)
                {
                    EasingFunction = GscMotion.CreateEaseOut(),
                    FillBehavior = FillBehavior.HoldEnd
                });
            SidebarCollapseButton.Focus();
            e.Handled = true;
        }

        /// <summary>
        /// Cancels an in-flight sidebar transition when reduced motion becomes active.
        /// Settings and Windows animation preferences can change while the embedded page
        /// remains loaded, so waiting for the next click would leave a visual transition
        /// running after the user has explicitly disabled it.
        /// </summary>
        internal void NormalizeMotionIfDisabled()
        {
            if (IsSidebarMotionEnabled)
                return;

            sidebarTransitionGeneration++;
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
            SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);
            if (SidebarContentLayer.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.XProperty, null);
                translate.X = 0;
            }
            SidebarContentLayer.Opacity = 1;
            sidebarTransitionRunning = false;
            ApplySidebarLayout();
        }

        // Instance-scoped preference input lets STA tests exercise both policy branches
        // on headless runners without changing Windows settings or a global test flag.
        // Production leaves it unset and always uses the real system preferences.
        internal Func<bool>? SystemMotionEnabledProviderForAudit { get; set; }

        private bool IsSidebarMotionEnabled
            => (MotionEnabledProvider?.Invoke() ?? true)
               && (SystemMotionEnabledProviderForAudit?.Invoke() ?? GscMotion.IsEnabled(true));

        internal bool SidebarMotionEnabledForAudit => IsSidebarMotionEnabled;
        internal GameSaveCenter.Playnite.Controls.Button SidebarCollapseButtonForAudit => SidebarCollapseButton;
        internal bool SidebarCollapsedForAudit => sidebarCollapsed;
        internal bool SidebarTransitionRunningForAudit => sidebarTransitionRunning;
        internal double SidebarWidthForAudit => SidebarColumn.Width.Value;
        internal int MeasurePassesForAudit => measurePasses;
        internal int ArrangePassesForAudit => arrangePasses;

        protected override Size MeasureOverride(Size availableSize)
        {
            measurePasses++;
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            arrangePasses++;
            return base.ArrangeOverride(finalSize);
        }

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
            else if (e.PropertyName == nameof(DashboardViewModel.OverviewPriorityTitle)
                     && viewModel?.CurrentWorkspace == WorkspaceKind.Overview)
                UpdatePageHeader(WorkspaceKind.Overview);
            else if ((e.PropertyName == nameof(DashboardViewModel.HasNavigationReturnTarget)
                      || e.PropertyName == nameof(DashboardViewModel.NavigationReturnLabel)
                      || e.PropertyName == nameof(DashboardViewModel.NavigationReturnToolTip))
                     && viewModel != null)
                UpdateNavigationReturnButton();

            if (e.PropertyName == nameof(DashboardViewModel.CurrentWorkspace)
                || e.PropertyName == nameof(DashboardViewModel.SelectedGame)
                || e.PropertyName == nameof(DashboardViewModel.SelectedTask)
                || e.PropertyName == nameof(DashboardViewModel.IsBusy))
                NotifyCommandDirectoryCanExecuteChanged();
        }

        private void NotifyCommandDirectoryCanExecuteChanged()
        {
            foreach (var item in commandDirectoryItems)
                item.RaiseCanExecuteChanged();
        }

        private void OnKeyboardHelpClick(object sender, RoutedEventArgs e)
        {
            CommandDirectoryOverlay.Visibility = Visibility.Collapsed;
            KeyboardShortcutHelpItemsControl.ItemsSource = KeyboardShortcutHelpItems;
            KeyboardShortcutHelpPopup.IsOpen = !KeyboardShortcutHelpPopup.IsOpen;
            e.Handled = true;
        }

        private void UpdateNavigationReturnButton()
        {
            if (HeaderBackButton == null)
                return;

            var visible = viewModel?.HasNavigationReturnTarget == true;
            HeaderBackButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            HeaderBackButton.ToolTip = viewModel?.NavigationReturnToolTip ?? string.Empty;
            AutomationProperties.SetName(HeaderBackButton, viewModel?.NavigationReturnLabel ?? "返回来源");
        }

        private void UpdatePageHeader(WorkspaceKind workspace)
        {
            PageTitleText.Text = workspace switch
            {
                WorkspaceKind.Saves => "存档中心",
                WorkspaceKind.Trainers => "修改器中心",
                WorkspaceKind.Media => "媒体中心",
                WorkspaceKind.Tasks => "任务中心",
                WorkspaceKind.Notifications => "通知收件箱",
                WorkspaceKind.Maintenance => "维护中心",
                _ => "首页",
            };
            PageSubtitleText.Text = GetPageSubtitle(
                workspace,
                viewModel?.SelectedGame?.Name,
                viewModel?.OverviewPriorityTitle);
            AutomationProperties.SetHelpText(PageTitleText, PageSubtitleText.Text);
        }

        internal static string GetPageSubtitle(
            WorkspaceKind workspace,
            string? selectedGameName,
            string? overviewPriorityTitle)
        {
            return workspace switch
            {
                WorkspaceKind.Saves => $"{selectedGameName ?? "未选择游戏"} · 路径与恢复点状态",
                WorkspaceKind.Trainers => "修改器 · CT 表 · 自定义启动项",
                WorkspaceKind.Media => "截图与录像的自动归档",
                WorkspaceKind.Tasks => "备份 · 云端 · 媒体任务队列",
                WorkspaceKind.Notifications => "全局提醒 · 未读状态 · 时间与级别筛选",
                WorkspaceKind.Maintenance => "诊断 · 设备 · 保留策略 · 审计",
                _ => string.IsNullOrWhiteSpace(overviewPriorityTitle)
                    ? "今日工作台 · 正在读取概览状态"
                    : $"今日工作台 · {overviewPriorityTitle}",
            };
        }

        private void OnGameContextClick(object sender, RoutedEventArgs e)
        {
            var opening = PickerOverlay.Visibility != Visibility.Visible;
            if (!opening)
            {
                PickerOverlay.Visibility = Visibility.Collapsed;
                FocusGameContextButton();
                e.Handled = true;
                return;
            }

            if (OpenGamePickerCommand.CanExecute(null))
                OpenGamePickerCommand.Execute(null);
        }

        private void OnPickerScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClosePickerAndRestoreFocus();
            e.Handled = true;
        }

        private void OnPickerOverlaySizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.HeightChanged)
                return;

            // Preserve room for at least one complete virtualized game row when a short
            // shell leaves less than 200 DIP for the picker. Search remains available;
            // the optional three-filter row and authored card padding return as soon as
            // the overlay has room.
            var compact = e.NewSize.Height < 200d;
            PickerFilterRow.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            PickerPanel.Padding = compact ? new Thickness(8) : new Thickness(14);
        }

        private void OnPickerListPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // A pointer selection is the explicit commit gesture. Reset the short-lived
            // keyboard guard first so a click arriving before the dispatcher cleanup can
            // still use the existing selection-and-close behavior.
            pickerKeyboardNavigationActive = false;
            pickerKeyboardNavigationGeneration++;
        }

        private void OnPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null || gameSearchCompositionActive || pickerKeyboardNavigationActive || e.AddedItems.Count == 0) return;
            if (e.AddedItems[0] is GamePickerItem item)
            {
                viewModel.SelectedGame = item.Game;
                ClosePickerAndRestoreFocus();
            }
        }

        private void OnGameSearchCompositionStarted(object sender, TextCompositionEventArgs e)
        {
            gameSearchCompositionActive = true;
        }

        private void OnGameSearchCompositionUpdated(object sender, TextCompositionEventArgs e)
        {
            gameSearchCompositionActive = true;
        }

        private void OnGameSearchTextInput(object sender, TextCompositionEventArgs e)
        {
            gameSearchCompositionActive = false;
        }

        private void OnPickerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PickerOverlay.Visibility != Visibility.Visible)
                return;

            // An IME candidate confirmation is delivered as ImeProcessed. Let the
            // TextBox/IME consume it; closing here would commit the previously selected
            // game while the user is still composing a new search term.
            if (gameSearchCompositionActive || e.Key == Key.ImeProcessed || e.ImeProcessedKey != Key.None)
                return;

            if (IsPickerKeyboardNavigationKey(e.Key))
            {
                BeginPickerKeyboardNavigationGuard();
                return;
            }

            if (e.Key == Key.Escape)
            {
                ClosePickerAndRestoreFocus();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter)
                return;

            var candidate = PickerList.SelectedItem as GamePickerItem;
            if (candidate == null || viewModel?.GamePicker == null || !viewModel.GamePicker.ItemsView.Contains(candidate))
                return;

            viewModel.SelectedGame = candidate.Game;
            ClosePickerAndRestoreFocus();
            e.Handled = true;
        }

        private void BeginPickerKeyboardNavigationGuard()
        {
            pickerKeyboardNavigationActive = true;
            var generation = ++pickerKeyboardNavigationGeneration;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (generation == pickerKeyboardNavigationGeneration)
                    pickerKeyboardNavigationActive = false;
            }));
        }

        private static bool IsPickerKeyboardNavigationKey(Key key)
            => key == Key.Up || key == Key.Down
                || key == Key.Left || key == Key.Right
                || key == Key.PageUp || key == Key.PageDown
                || key == Key.Home || key == Key.End;

        private void ClosePickerAndRestoreFocus()
        {
            PickerOverlay.Visibility = Visibility.Collapsed;
            FocusGameContextButton();
        }

        private void FocusGameContextButton()
        {
            if (!GameContextButton.IsVisible || !GameContextButton.Focusable)
                return;

            GameContextButton.Focus();
            Keyboard.Focus(GameContextButton);
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
            // Compact hosts use a single-column header: title first, then the global
            // game picker and commands across the available width. A two-column header
            // leaves too little room for those controls while the action strip wraps.
            HeaderActionsRow.Height = compact ? GridLength.Auto : new GridLength(0);
            Grid.SetRow(HeaderTitlePanel, 0);
            Grid.SetRow(HeaderActionsPanel, compact ? 1 : 0);
            Grid.SetColumn(HeaderTitlePanel, 0);
            Grid.SetColumn(HeaderActionsPanel, compact ? 0 : 1);
            Grid.SetColumnSpan(HeaderTitlePanel, compact ? 2 : 1);
            Grid.SetColumnSpan(HeaderActionsPanel, compact ? 2 : 1);
            HeaderTitleColumn.Width = new GridLength(1, GridUnitType.Star);
            HeaderActionsColumn.Width = compact ? new GridLength(0) : GridLength.Auto;
            HeaderTitlePanel.HorizontalAlignment = HorizontalAlignment.Left;
            PageSubtitleText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            HeaderActionsPanel.HorizontalAlignment = compact ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            HeaderActionsPanel.Margin = compact
                ? new Thickness(0, 8, 0, 0)
                : new Thickness(14, 0, 0, 0);
            HeaderActionsPanel.Width = double.NaN;
            headerActionRowGap.SetRowGap(compact ? ResponsiveLayoutCoordinator.CompactActionRowGap : 0);

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
            if (pages.TryGetValue(WorkspaceKind.Notifications, out var notifications)) ((NotificationInboxView)notifications).ApplyResponsiveLayout(width, height);
            if (pages.TryGetValue(WorkspaceKind.Maintenance, out var maintenance)) ((MaintenanceView)maintenance).ApplyResponsiveLayout(width, height);
        }
    }
}
