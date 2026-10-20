using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    public partial class MediaCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private readonly ResponsiveDetailBreakpointLatch detailBreakpoint = new ResponsiveDetailBreakpointLatch();
        private bool mediaInspectorOpen;
        private bool mediaInboxInspectorOpen;
        private bool mediaInboxHistoryOpen;
        private bool mediaComparisonWorkspaceUserOverride;
        private readonly MediaComparisonViewportState mediaComparisonViewportState = new MediaComparisonViewportState();
        private bool suppressMediaComparisonScrollSync;
        private ScrollViewer? mediaComparisonPanViewer;
        private Point mediaComparisonPanStart;
        private double mediaComparisonPanStartHorizontal;
        private double mediaComparisonPanStartVertical;
        private DashboardViewModel? attachedViewModel;
        private readonly HashSet<string> selectedMediaIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> selectedInboxIdsByMode = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private readonly WrapPanelRowGapController mediaFilterPresetRowGap;
        private readonly WrapPanelRowGapController mediaCurrentBatchActionRowGap;
        private readonly WrapPanelRowGapController mediaInboxActionRowGap;
        private readonly WrapPanelRowGapController mediaInboxSecondaryActionRowGap;
        private ScrollAnchor? pendingMediaAnchor;
        private ScrollAnchor? pendingInboxAnchor;
        private string? pendingInboxAnchorMode;
        private bool restoringSelection;
        private bool selectionRestoreQueued;
        private long anchorRestoreGeneration;
        private string anchorDiagnostic = "none";
        private readonly DispatcherTimer pendingAnchorExpiryTimer;
        private DataGridColumnLayoutController? inboxColumnLayout;
        private DataGridStableSortController? inboxSort;

        public MediaCenterView()
        {
            InitializeComponent();
            MediaComparisonFirstImage.PreviewStateChanged += OnMediaComparisonPreviewStateChanged;
            MediaComparisonSecondImage.PreviewStateChanged += OnMediaComparisonPreviewStateChanged;
            mediaFilterPresetRowGap = new WrapPanelRowGapController(MediaFilterPresetRow);
            mediaCurrentBatchActionRowGap = new WrapPanelRowGapController(MediaCurrentBatchActions);
            mediaInboxActionRowGap = new WrapPanelRowGapController(MediaInboxBatchActionRow);
            mediaInboxSecondaryActionRowGap = new WrapPanelRowGapController(MediaInboxSecondaryActions);
            DataGridScrollDiagnostics.Attach(MediaInboxGrid, "MediaInboxGrid", GetScrollDiagnosticContext);
            MediaInspectorScrollViewer.IsVisibleChanged += OnMediaInspectorIsVisibleChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
            pendingAnchorExpiryTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            pendingAnchorExpiryTimer.Tick += OnPendingAnchorExpiry;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachViewModel(DataContext as DashboardViewModel);
            AttachColumnLayout();
            UpdateMediaSelectionSummary();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopMediaComparisonPan();
            InvalidatePendingAnchorRestore();
            pendingAnchorExpiryTimer.Stop();
            inboxColumnLayout?.Dispose();
            inboxSort?.Dispose();
            inboxColumnLayout = null;
            inboxSort = null;
            DetachViewModel();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AttachViewModel(e.NewValue as DashboardViewModel);
            if (IsLoaded)
            {
                inboxColumnLayout?.Dispose();
                inboxSort?.Dispose();
                inboxColumnLayout = null;
                inboxSort = null;
                AttachColumnLayout();
            }
        }

        private void AttachColumnLayout()
        {
            if (inboxColumnLayout != null || !(DataContext is DashboardViewModel viewModel)) return;
            inboxColumnLayout = new DataGridColumnLayoutController(
                MediaInboxGrid,
                "media-inbox",
                new[] { "captured-time", "type", "source", "file", "reason" },
                viewModel.PluginSettings,
                viewModel.PersistUiPreference);
            inboxSort = ProductionDataGridSortProfiles.AttachMediaInbox(MediaInboxGrid);
        }

        private void AttachViewModel(DashboardViewModel? viewModel)
        {
            if (ReferenceEquals(attachedViewModel, viewModel)) return;
            InvalidatePendingAnchorRestore();
            DetachViewModel();
            attachedViewModel = viewModel;
            if (viewModel == null) return;

            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.Media.CollectionChanged += OnMediaCollectionChanged;
            viewModel.MediaComparisonItems.CollectionChanged += OnMediaComparisonItemsChanged;
            viewModel.UnassignedMedia.CollectionChanged += OnInboxCollectionChanged;
            viewModel.IgnoredMedia.CollectionChanged += OnInboxCollectionChanged;
        }

        private void DetachViewModel()
        {
            if (attachedViewModel == null) return;
            attachedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            attachedViewModel.Media.CollectionChanged -= OnMediaCollectionChanged;
            attachedViewModel.MediaComparisonItems.CollectionChanged -= OnMediaComparisonItemsChanged;
            attachedViewModel.UnassignedMedia.CollectionChanged -= OnInboxCollectionChanged;
            attachedViewModel.IgnoredMedia.CollectionChanged -= OnInboxCollectionChanged;
            attachedViewModel = null;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaComparisonWorkspaceOpen), StringComparison.Ordinal)
                && !mediaComparisonWorkspaceUserOverride
                && responsiveHeight > 0
                && responsiveHeight < 680d
                && sender is DashboardViewModel comparisonViewModel
                && comparisonViewModel.MediaComparisonWorkspaceOpen)
            {
                // Keep the media list readable in short hosts. The user can still open
                // the comparison manually; larger hosts open it as soon as a pair forms.
                comparisonViewModel.SetMediaComparisonWorkspaceOpen(false);
            }

            if (string.IsNullOrEmpty(e.PropertyName)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.SelectedGame), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.CurrentWorkspace), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaInboxMode), StringComparison.Ordinal))
            {
                InvalidatePendingAnchorRestore();
            }

            if (string.Equals(e.PropertyName, nameof(DashboardViewModel.SelectedMedia), StringComparison.Ordinal))
            {
                ResetSelectedVideoPreview();
                QueueSelectedMediaIntoView();
            }

            if (string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaSearchText), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaFilter), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaLoadedSummary), StringComparison.Ordinal))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(UpdateMediaSelectionSummary));
            }
        }

        private void OnMediaComparisonItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var count = attachedViewModel?.MediaComparisonItems.Count ?? 0;
            if (count == 0)
            {
                mediaComparisonWorkspaceUserOverride = false;
                mediaComparisonViewportState.FitBoth();
            }
            else if (count == 2)
            {
                mediaComparisonViewportState.FitBoth();
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateMediaComparisonViewports));
            }
        }

        private void OnMediaComparisonWorkspaceToggleClick(object sender, RoutedEventArgs e)
        {
            mediaComparisonWorkspaceUserOverride = true;
        }

        private void InvalidatePendingAnchorRestore()
        {
            unchecked { anchorRestoreGeneration++; }
            anchorDiagnostic = $"invalidated:generation={anchorRestoreGeneration}";
            pendingMediaAnchor = null;
            pendingInboxAnchor = null;
            pendingInboxAnchorMode = null;
            selectionRestoreQueued = false;
            pendingAnchorExpiryTimer.Stop();
        }

        private void OnPendingAnchorExpiry(object? sender, EventArgs e)
        {
            InvalidatePendingAnchorRestore();
        }

        private void ArmPendingAnchorExpiry()
        {
            pendingAnchorExpiryTimer.Stop();
            pendingAnchorExpiryTimer.Start();
        }

        private void OnMediaInspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
                return;

            ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnMediaInboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MediaInboxGrid == null || MediaInboxBatchSelectionSummary == null)
                return;
            if (!restoringSelection && !selectionRestoreQueued)
                UpdateSelectionDelta(GetInboxSelectionSet(), e);
            // A new row is a new inspection context. Do not reopen the previous
            // compact drawer implicitly after a collection refresh or a mode switch;
            // the user must explicitly request details for the newly selected item.
            mediaInboxInspectorOpen = false;
            UpdateMediaInboxSelectionSummary();
            UpdateMediaSelectionSummary();
            CommandManager.InvalidateRequerySuggested();
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnClearMediaInboxSelectionClick(object sender, RoutedEventArgs e)
        {
            restoringSelection = true;
            try
            {
                MediaInboxGrid.SelectedItems.Clear();
                GetInboxSelectionSet().Clear();
            }
            finally
            {
                restoringSelection = false;
            }

            UpdateMediaInboxSelectionSummary();
            CommandManager.InvalidateRequerySuggested();
            e.Handled = true;
        }

        private void OnResetColumnLayoutClick(object sender, RoutedEventArgs e)
        {
            inboxColumnLayout?.ResetToDefaults();
            e.Handled = true;
        }

        private void UpdateMediaInboxSelectionSummary()
        {
            var visibleCount = MediaInboxGrid.SelectedItems.Count;
            var totalCount = GetInboxSelectionSet().Count;
            var unavailableCount = Math.Max(0, totalCount - visibleCount);
            var ignored = string.Equals(attachedViewModel?.MediaInboxMode, "已忽略", StringComparison.Ordinal);

            MediaInboxBatchSelectionSummary.Text = totalCount == 0
                ? "未选择媒体 · Ctrl / Shift 多选"
                : unavailableCount > 0
                    ? $"已选 {totalCount} 项 · 当前窗口 {visibleCount} 项可操作 · 另 {unavailableCount} 项暂不可见"
                    : ignored
                        ? $"已选 {totalCount} 项 · 可恢复到待归类"
                        : $"已选 {totalCount} 项 · 可批量处理";
            MediaInboxClearSelectionButton.Visibility = totalCount > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnMediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateMediaSelectionSummary();
            if (pendingMediaAnchor == null) return;
            var anchor = pendingMediaAnchor;
            pendingMediaAnchor = null;
            pendingAnchorExpiryTimer.Stop();
            selectionRestoreQueued = true;
            QueueRestore(MediaGrid, anchor, selectedMediaIds, isInbox: false, mode: null, generation: anchorRestoreGeneration);
        }

        private void OnInboxCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (pendingInboxAnchor == null || attachedViewModel == null) return;
            var mode = pendingInboxAnchorMode;
            if (mode == null) return;
            if (!string.Equals(mode, attachedViewModel.MediaInboxMode, StringComparison.Ordinal))
            {
                InvalidatePendingAnchorRestore();
                return;
            }
            var anchor = pendingInboxAnchor;
            pendingInboxAnchor = null;
            pendingInboxAnchorMode = null;
            pendingAnchorExpiryTimer.Stop();
            selectionRestoreQueued = true;
            MarkAnchorDiagnostic(MediaInboxGrid, $"queued:inbox:{e.Action}:mode={mode}");
            QueueRestore(MediaInboxGrid, anchor, GetInboxSelectionSet(mode), isInbox: true, mode: mode, generation: anchorRestoreGeneration);
        }

        private void OnLoadMoreMediaClick(object sender, RoutedEventArgs e)
        {
            if (attachedViewModel == null) return;
            CaptureSelection(MediaGrid.SelectedItems, selectedMediaIds);
            pendingMediaAnchor = CaptureAnchor(MediaGrid);
            ArmPendingAnchorExpiry();
            UpdateMediaSelectionSummary();
        }

        private void OnLoadMoreMediaInboxClick(object sender, RoutedEventArgs e)
        {
            if (attachedViewModel == null) return;
            DataGridScrollDiagnostics.MarkTrigger(MediaInboxGrid, "加载更多");
            var mode = attachedViewModel.MediaInboxMode;
            var selection = GetInboxSelectionSet(mode);
            CaptureSelection(MediaInboxGrid.SelectedItems, selection);
            pendingInboxAnchorMode = mode;
            pendingInboxAnchor = CaptureAnchor(MediaInboxGrid);
            ArmPendingAnchorExpiry();
        }

        private string GetScrollDiagnosticContext()
            => (attachedViewModel?.GetMediaScrollDiagnosticContext() ?? "vm=none")
                + $",anchorGen={anchorRestoreGeneration},anchor={anchorDiagnostic},pendingMedia={pendingMediaAnchor != null},pendingInbox={pendingInboxAnchor != null},selectionRestoreQueued={selectionRestoreQueued}";

        private void OnMediaInboxModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InvalidatePendingAnchorRestore();
            mediaInboxInspectorOpen = false;
            mediaInboxHistoryOpen = false;
            OnMediaInboxSelectionChanged(sender,e);
        }

        private void OnMediaClassificationHistoryStateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || DataContext is not DashboardViewModel viewModel)
                return;
            if (viewModel.RefreshMediaClassificationHistoryCommand.CanExecute(null))
                viewModel.RefreshMediaClassificationHistoryCommand.Execute(null);
        }

        private void OnClearSearchTextBoxClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement source && source.Tag is TextBox textBox)
            {
                textBox.Clear();
                textBox.Focus();
                Keyboard.Focus(textBox);
            }
            e.Handled = true;
        }

        public Border MediaSummaryPanelElement => MediaSummaryPanel;
        public UniformGrid MediaSourceFieldsElement => MediaSourceFields;
        public Grid MediaSourceLayoutElement => MediaSourceLayout;
        public ScrollViewer MediaSourceFormScrollerElement => MediaSourceFormScroller;
        public Border MediaInspectorPanelElement => MediaInspectorPanel;
        public Border MediaPreviewPanelElement => MediaPreviewPanel;
        public StackPanel MediaMetadataPanelElement => MediaMetadataPanel;
        public ScrollViewer MediaInspectorScrollViewerElement => MediaInspectorScrollViewer;
        public Border MediaInspectorFrameElement => MediaInspectorFrame;

        private void OnMediaComparisonWorkspaceSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MediaComparisonWorkspaceGrid == null || MediaComparisonSecondCard == null)
                return;

            // The current-game page keeps a 600-DIP inspector-adjacent lane on many
            // supported window sizes. Two 270-DIP cards still read side by side there;
            // stack only when the comparison surface itself becomes narrower than that.
            var stacked = e.NewSize.Width < 560d;
            MediaComparisonWorkspaceGrid.ColumnDefinitions[1].Width = stacked
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            MediaComparisonWorkspaceGrid.RowDefinitions[2].Height = stacked
                ? GridLength.Auto
                : new GridLength(0);
            Grid.SetRow(MediaComparisonSecondCard, stacked ? 2 : 1);
            Grid.SetColumn(MediaComparisonSecondCard, stacked ? 0 : 1);
            MediaComparisonFirstCard.Margin = stacked
                ? new Thickness(0, 0, 0, 5)
                : new Thickness(0, 0, 5, 0);
            MediaComparisonSecondCard.Margin = stacked
                ? new Thickness(0, 5, 0, 0)
                : new Thickness(5, 0, 0, 0);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateMediaComparisonViewports));
        }

        private void OnMediaComparisonPreviewStateChanged(object? sender, EventArgs e)
        {
            UpdateMediaComparisonViewports();
        }

        private void OnMediaComparisonViewportSizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateMediaComparisonViewports();

        private void OnMediaComparisonSyncClick(object sender, RoutedEventArgs e)
        {
            mediaComparisonViewportState.SetSynchronized(MediaComparisonSyncToggle.IsChecked == true);
            UpdateMediaComparisonViewports();
        }

        private void OnMediaComparisonFitClick(object sender, RoutedEventArgs e)
        {
            mediaComparisonViewportState.FitBoth();
            UpdateMediaComparisonViewports();
        }

        private void OnMediaComparisonOneToOneClick(object sender, RoutedEventArgs e)
        {
            mediaComparisonViewportState.SetOneToOne();
            UpdateMediaComparisonViewports();
        }

        private void OnMediaComparisonFirstZoomOutClick(object sender, RoutedEventArgs e)
            => AdjustMediaComparisonZoom(MediaComparisonPane.First, -1);

        private void OnMediaComparisonFirstZoomInClick(object sender, RoutedEventArgs e)
            => AdjustMediaComparisonZoom(MediaComparisonPane.First, 1);

        private void OnMediaComparisonSecondZoomOutClick(object sender, RoutedEventArgs e)
            => AdjustMediaComparisonZoom(MediaComparisonPane.Second, -1);

        private void OnMediaComparisonSecondZoomInClick(object sender, RoutedEventArgs e)
            => AdjustMediaComparisonZoom(MediaComparisonPane.Second, 1);

        private void AdjustMediaComparisonZoom(MediaComparisonPane pane, int direction)
        {
            var image = GetMediaComparisonImage(pane);
            var viewer = GetMediaComparisonScroller(pane);
            if (image.Source == null) return;
            mediaComparisonViewportState.AdjustZoom(pane, direction, GetMediaComparisonFitZoom(viewer, image));
            UpdateMediaComparisonViewports();
            viewer.Focus();
        }

        private void UpdateMediaComparisonViewports()
        {
            if (!IsLoaded || !MediaComparisonWorkspace.IsVisible) return;
            suppressMediaComparisonScrollSync = true;
            try
            {
                UpdateMediaComparisonViewport(MediaComparisonPane.First);
                UpdateMediaComparisonViewport(MediaComparisonPane.Second);
            }
            finally
            {
                suppressMediaComparisonScrollSync = false;
            }
            UpdateMediaComparisonZoomStatus();
        }

        private void UpdateMediaComparisonViewport(MediaComparisonPane pane)
        {
            if (!IsLoaded || !MediaComparisonWorkspace.IsVisible) return;
            var viewer = GetMediaComparisonScroller(pane);
            var content = pane == MediaComparisonPane.First
                ? MediaComparisonFirstViewportContent
                : MediaComparisonSecondViewportContent;
            var image = GetMediaComparisonImage(pane);
            var viewportWidth = Math.Max(0d, viewer.ViewportWidth);
            var viewportHeight = Math.Max(0d, viewer.ViewportHeight);
            if (viewportWidth <= 0d || viewportHeight <= 0d) return;

            var bitmap = image.Source as System.Windows.Media.Imaging.BitmapSource;
            if (bitmap == null)
            {
                content.Width = viewportWidth;
                content.Height = viewportHeight;
                image.Width = double.NaN;
                image.Height = double.NaN;
                return;
            }

            var fitZoom = GetMediaComparisonFitZoom(viewer, image);
            var zoom = mediaComparisonViewportState.GetZoom(pane, fitZoom);
            var displayWidth = bitmap.PixelWidth * zoom;
            var displayHeight = bitmap.PixelHeight * zoom;
            content.Width = Math.Max(viewportWidth, displayWidth);
            content.Height = Math.Max(viewportHeight, displayHeight);
            image.Width = displayWidth;
            image.Height = displayHeight;
            image.HorizontalAlignment = displayWidth > viewportWidth ? HorizontalAlignment.Left : HorizontalAlignment.Center;
            image.VerticalAlignment = displayHeight > viewportHeight ? VerticalAlignment.Top : VerticalAlignment.Center;
            viewer.UpdateLayout();

            viewer.ScrollToHorizontalOffset(MediaComparisonViewportState.GetOffset(
                mediaComparisonViewportState.GetCenterX(pane), viewer.ViewportWidth, viewer.ExtentWidth));
            viewer.ScrollToVerticalOffset(MediaComparisonViewportState.GetOffset(
                mediaComparisonViewportState.GetCenterY(pane), viewer.ViewportHeight, viewer.ExtentHeight));
        }

        private static double GetMediaComparisonFitZoom(ScrollViewer viewer, AsyncThumbnailImage image)
        {
            var bitmap = image.Source as System.Windows.Media.Imaging.BitmapSource;
            if (bitmap == null || bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0
                || viewer.ViewportWidth <= 0d || viewer.ViewportHeight <= 0d)
                return 1d;
            return Math.Min(viewer.ViewportWidth / bitmap.PixelWidth, viewer.ViewportHeight / bitmap.PixelHeight);
        }

        private AsyncThumbnailImage GetMediaComparisonImage(MediaComparisonPane pane)
            => pane == MediaComparisonPane.First ? MediaComparisonFirstImage : MediaComparisonSecondImage;

        private ScrollViewer GetMediaComparisonScroller(MediaComparisonPane pane)
            => pane == MediaComparisonPane.First ? MediaComparisonFirstScroller : MediaComparisonSecondScroller;

        private void UpdateMediaComparisonZoomStatus()
        {
            if (MediaComparisonZoomStatusText == null) return;
            var firstZoom = mediaComparisonViewportState.GetZoom(
                MediaComparisonPane.First,
                GetMediaComparisonFitZoom(MediaComparisonFirstScroller, MediaComparisonFirstImage));
            var secondZoom = mediaComparisonViewportState.GetZoom(
                MediaComparisonPane.Second,
                GetMediaComparisonFitZoom(MediaComparisonSecondScroller, MediaComparisonSecondImage));
            MediaComparisonZoomStatusText.Text = mediaComparisonViewportState.IsFit(MediaComparisonPane.First)
                && mediaComparisonViewportState.IsFit(MediaComparisonPane.Second)
                    ? "适应窗口 · 最高解码长边 2048 px"
                    : $"左 {firstZoom:P0} · 右 {secondZoom:P0} · 最高解码长边 2048 px";
        }

        private void OnMediaComparisonFirstScrollChanged(object sender, ScrollChangedEventArgs e)
            => OnMediaComparisonScrollChanged(MediaComparisonPane.First, e);

        private void OnMediaComparisonSecondScrollChanged(object sender, ScrollChangedEventArgs e)
            => OnMediaComparisonScrollChanged(MediaComparisonPane.Second, e);

        private void OnMediaComparisonScrollChanged(MediaComparisonPane pane, ScrollChangedEventArgs e)
        {
            if (suppressMediaComparisonScrollSync) return;
            var viewer = GetMediaComparisonScroller(pane);
            if (Math.Abs(e.ExtentWidthChange) > 0.01d || Math.Abs(e.ExtentHeightChange) > 0.01d
                || Math.Abs(e.ViewportWidthChange) > 0.01d || Math.Abs(e.ViewportHeightChange) > 0.01d)
            {
                ApplyMediaComparisonCenter(pane);
                return;
            }

            var centerX = viewer.ScrollableWidth > 0.01d
                ? MediaComparisonViewportState.GetNormalizedCenter(viewer.HorizontalOffset, viewer.ViewportWidth, viewer.ExtentWidth)
                : mediaComparisonViewportState.GetCenterX(pane);
            var centerY = viewer.ScrollableHeight > 0.01d
                ? MediaComparisonViewportState.GetNormalizedCenter(viewer.VerticalOffset, viewer.ViewportHeight, viewer.ExtentHeight)
                : mediaComparisonViewportState.GetCenterY(pane);
            if (!mediaComparisonViewportState.SetCenter(pane, centerX, centerY)
                || !mediaComparisonViewportState.IsSynchronized)
                return;

            ApplyMediaComparisonCenter(pane == MediaComparisonPane.First ? MediaComparisonPane.Second : MediaComparisonPane.First);
        }

        private void ApplyMediaComparisonCenter(MediaComparisonPane pane)
        {
            var viewer = GetMediaComparisonScroller(pane);
            suppressMediaComparisonScrollSync = true;
            try
            {
                viewer.ScrollToHorizontalOffset(MediaComparisonViewportState.GetOffset(
                    mediaComparisonViewportState.GetCenterX(pane), viewer.ViewportWidth, viewer.ExtentWidth));
                viewer.ScrollToVerticalOffset(MediaComparisonViewportState.GetOffset(
                    mediaComparisonViewportState.GetCenterY(pane), viewer.ViewportHeight, viewer.ExtentHeight));
            }
            finally
            {
                suppressMediaComparisonScrollSync = false;
            }
        }

        private void OnMediaComparisonPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
            var pane = ReferenceEquals(sender, MediaComparisonFirstScroller)
                ? MediaComparisonPane.First
                : MediaComparisonPane.Second;
            AdjustMediaComparisonZoom(pane, e.Delta > 0 ? 1 : -1);
            e.Handled = true;
        }

        private void OnMediaComparisonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            if (viewer == null || e.ChangedButton != MouseButton.Left
                || viewer.ScrollableWidth <= 0.01d && viewer.ScrollableHeight <= 0.01d
                || IsMediaComparisonScrollBar(e.OriginalSource as DependencyObject))
                return;

            mediaComparisonPanViewer = viewer;
            mediaComparisonPanStart = e.GetPosition(viewer);
            mediaComparisonPanStartHorizontal = viewer.HorizontalOffset;
            mediaComparisonPanStartVertical = viewer.VerticalOffset;
            viewer.Focus();
            viewer.CaptureMouse();
            viewer.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        private void OnMediaComparisonPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var viewer = mediaComparisonPanViewer;
            if (viewer == null || !viewer.IsMouseCaptured || e.LeftButton != MouseButtonState.Pressed) return;
            var current = e.GetPosition(viewer);
            viewer.ScrollToHorizontalOffset(mediaComparisonPanStartHorizontal + mediaComparisonPanStart.X - current.X);
            viewer.ScrollToVerticalOffset(mediaComparisonPanStartVertical + mediaComparisonPanStart.Y - current.Y);
            e.Handled = true;
        }

        private void OnMediaComparisonPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mediaComparisonPanViewer == null) return;
            StopMediaComparisonPan();
            e.Handled = true;
        }

        private void StopMediaComparisonPan()
        {
            var viewer = mediaComparisonPanViewer;
            mediaComparisonPanViewer = null;
            if (viewer == null) return;
            viewer.Cursor = Cursors.Arrow;
            if (viewer.IsMouseCaptured) viewer.ReleaseMouseCapture();
        }

        private void OnMediaComparisonPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            if (viewer == null) return;
            var pane = ReferenceEquals(viewer, MediaComparisonFirstScroller)
                ? MediaComparisonPane.First
                : MediaComparisonPane.Second;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (e.Key == Key.Add || e.Key == Key.OemPlus)
                    AdjustMediaComparisonZoom(pane, 1);
                else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
                    AdjustMediaComparisonZoom(pane, -1);
                else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
                    OnMediaComparisonFitClick(this, new RoutedEventArgs());
                else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                    OnMediaComparisonOneToOneClick(this, new RoutedEventArgs());
                else
                    return;
                e.Handled = true;
                return;
            }

            const double panStep = 48d;
            switch (e.Key)
            {
                case Key.Left: viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset - panStep); break;
                case Key.Right: viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset + panStep); break;
                case Key.Up: viewer.ScrollToVerticalOffset(viewer.VerticalOffset - panStep); break;
                case Key.Down: viewer.ScrollToVerticalOffset(viewer.VerticalOffset + panStep); break;
                default: return;
            }
            e.Handled = true;
        }

        private static bool IsMediaComparisonScrollBar(DependencyObject? source)
        {
            var current = source;
            while (current != null)
            {
                if (current is ScrollBar) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
                var pageViewportHeight = MediaCurrentPageScrollViewer.ViewportHeight > 0
                    ? MediaCurrentPageScrollViewer.ViewportHeight
                    : Math.Max(0, height);
                responsiveWidth = width;
                responsiveHeight = height;
                if (height > 0
                    && height < 680d
                    && !mediaComparisonWorkspaceUserOverride
                    && attachedViewModel?.MediaComparisonWorkspaceOpen == true)
                {
                    attachedViewModel.SetMediaComparisonWorkspaceOpen(false);
                }
                // The filter preset controls wrap below the measured 720 DIP page width.
                // Add room between wrapped rows and restore authored margins once they fit.
                mediaFilterPresetRowGap.SetRowGap(width < 720d ? ResponsiveLayoutCoordinator.CompactActionRowGap : 0d);
                // The secondary inbox actions wrap through 576 DIP. Keep the same
                // 20-DIP spacing as the other compact action rows in that range.
                mediaInboxSecondaryActionRowGap.SetRowGap(width < 577d ? ResponsiveLayoutCoordinator.CompactActionRowGap : 0d);
                var batchActionWidth = MediaInboxBatchActionRow.ActualWidth;
                var compactBatchActions = batchActionWidth > 0
                    ? batchActionWidth < 960
                    : width < 1040;
                mediaInboxActionRowGap.SetRowGap(compactBatchActions ? ResponsiveLayoutCoordinator.CompactActionRowGap : 0);
                // Keep the Demo's four metrics in one continuous strip. The page viewer
                // carries excess height while each table and inspector keeps a finite
                // inner scrolling surface.
                MediaSummaryPanel.Visibility = Visibility.Visible;
                // A themed 1366x768 host has less content height after shell chrome than
                // the raw window number suggests. Keep the primary inbox grid at its
                // four-row reading floor by entering the compact metric treatment before
                // the table star row has to surrender a row.
                var compactHeight = height > 0 && height < 800;
                MediaSummaryPanel.MinHeight = compactHeight ? 68 : 84;
                MediaSummaryPanel.Padding = compactHeight
                    ? new Thickness(6, 8, 6, 8)
                    : new Thickness(6, 14, 6, 14);
                // The page-level metric strip already supplies the inbox count. At the
                // short heights used by the production shell, the second title/count band
                // was duplicate context that pushed batch actions and five table rows below
                // the fold. Keep it for normal-height orientation, but release that space
                // before using the whole-page overflow fallback.
                // The inbox band repeats the metric strip's count/title. The themed
                // 1366x768 content host is only 528 DIP tall, so keeping that duplicate
                // band would reduce the virtualized table below its four-row floor.
                var compactInbox = compactHeight;
                MediaInboxInfoBand.Visibility = compactInbox ? Visibility.Collapsed : Visibility.Visible;
                MediaInboxInfoBand.Padding = compactHeight
                    ? new Thickness(10, 6, 10, 6)
                    : new Thickness(14, 11, 14, 11);
                MediaInboxInfoDescription.Visibility = compactHeight
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                // The inbox DataGrid and its inspector own the vertical scroll surfaces.
                // Keep the DataGrid finite even when the page-level viewer owns overflow;
                // an infinite MaxHeight lets the outer ScrollViewer hand Standard WPF's
                // DataGrid an unbounded measure and instantiate the complete inbox window.
                // A stale banner is part of the page content, not a bottom overlay.
                // When it appears at a short-but-not-fallback height, keeping the page
                // scroller disabled lets the banner, toolbar and footer consume the
                // entire table frame and measures the DataGrid viewport to zero. Let the
                // page surface own overflow in that state so the row viewport remains a
                // real finite surface and the footer stays below it.
                var staleInboxRequiresPageScroll = MediaInboxStaleBanner.Visibility == Visibility.Visible;
                // The production shell can leave the page host at roughly 577–597 DIP
                // even when the outer window is the supported 1040–1100 DIP layout.
                // Keep the inbox page finite in that band too, so the table retains a
                // readable row viewport and the footer remains reachable through the
                // page surface instead of compressing the star row below two rows.
                // Compact-height pages and panes below the existing 900 DIP width
                // breakpoint can wrap several footer rows. Keep the page scroll
                // channel available in those layouts instead of clipping the grid
                // inside a disabled frame; when the content fits, the viewer still
                // reports no scrollable extent.
                var useInboxPageFallbackScroll = compactHeight || width < 900 || staleInboxRequiresPageScroll;
                // The inner DataGrid owns the normal finite table viewport. Keep the
                // page-level overflow channel only for the short/stale fallback; a
                // nested Auto ScrollViewer otherwise measures Standard WPF rows as a
                // page surface and defeats the retained-row container budget.
                MediaInboxPageScrollViewer.VerticalScrollBarVisibility = useInboxPageFallbackScroll
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Disabled;
                MediaInboxPageScrollViewer.VerticalContentAlignment = VerticalAlignment.Top;
                MediaInboxScrollSurface.VerticalAlignment = VerticalAlignment.Top;
                var sourceStack = width < 900;
                MediaSourceFields.Columns = sourceStack ? 1 : 2;
                MediaSourceLayout.ColumnDefinitions[1].Width = sourceStack ? new GridLength(0) : new GridLength(14);
                MediaSourceFormRow.Height = sourceStack ? GridLength.Auto : new GridLength(1, GridUnitType.Star);
                MediaSourceRulesRow.Height = sourceStack ? GridLength.Auto : new GridLength(0);
                Grid.SetColumn(MediaSourceFormScroller, 0);
                Grid.SetColumnSpan(MediaSourceFormScroller, sourceStack ? 3 : 1);
                Grid.SetRow(MediaSourceFormScroller, 0);
                Grid.SetColumn(MediaSourceRulesFrame, sourceStack ? 0 : 2);
                Grid.SetColumnSpan(MediaSourceRulesFrame, sourceStack ? 3 : 1);
                Grid.SetRow(MediaSourceRulesFrame, sourceStack ? 1 : 0);
                MediaSourceFormScroller.MaxHeight = sourceStack
                    ? Math.Max(260, Math.Min(520, height * 0.60))
                    : double.PositiveInfinity;
                MediaSourceFormScroller.VerticalScrollBarVisibility = sourceStack
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Disabled;
                MediaSourceRulesFrame.Margin = sourceStack
                    ? new Thickness(0, 10, 0, 0)
                    : new Thickness(0);
                MediaSourceRulesFrame.MaxHeight = sourceStack
                    ? Math.Max(240, Math.Min(520, height * 0.60))
                    : 520;

                // The inbox inspector is useful, but it must not consume the list's
                // minimum readable width. Calculate the breakpoint from the table and
                // inspector budgets rather than from the outer window size. In a narrow
                // pane the inspector becomes an explicit, keyboard-reachable details
                // action; opening it moves the same inspector below the table.
                var inboxInspectorWidth = MediaInboxLayout.TryFindResource("GscInspectorWidth") is GridLength inboxLength
                    ? inboxLength
                    : new GridLength(360);
                var inboxAvailableWidth = MediaInboxLayout.ActualWidth > 0
                    ? MediaInboxLayout.ActualWidth
                    : width;
                const double inboxTableMinimumWidth = 520;
                var inboxStack = inboxAvailableWidth < inboxTableMinimumWidth + inboxInspectorWidth.Value + 14;
                var hasInboxSelection = MediaInboxGrid.SelectedItem != null;
                var showInboxInspector = inboxStack
                    ? mediaInboxHistoryOpen || (mediaInboxInspectorOpen && hasInboxSelection)
                    : hasInboxSelection || mediaInboxHistoryOpen;
                MediaInboxLayout.ColumnDefinitions[1].Width = inboxStack || !showInboxInspector
                    ? new GridLength(0)
                    : new GridLength(14);
                MediaInboxLayout.ColumnDefinitions[2].Width = inboxStack || !showInboxInspector
                    ? new GridLength(0)
                    : inboxInspectorWidth;
                MediaInboxLayout.RowDefinitions[1].Height = inboxStack && showInboxInspector
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetColumn(MediaInboxInspectorScrollViewer, inboxStack ? 0 : 2);
                Grid.SetColumnSpan(MediaInboxInspectorScrollViewer, inboxStack ? 3 : 1);
                Grid.SetRow(MediaInboxInspectorScrollViewer, inboxStack ? 1 : 0);
                MediaInboxInspectorScrollViewer.Margin = inboxStack && showInboxInspector
                    ? new Thickness(0, 10, 0, 0)
                    : new Thickness(0);
                MediaInboxInspectorScrollViewer.Visibility = showInboxInspector
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                var inboxInspectorMaxHeight = Math.Max(220, height - (compactHeight ? 190 : 230));
                MediaInboxInspectorScrollViewer.MaxHeight = inboxStack && showInboxInspector
                    ? Math.Min(520, inboxInspectorMaxHeight)
                    : inboxInspectorMaxHeight;
                MediaInboxSelectionDetails.Visibility = hasInboxSelection
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                MediaInboxNoSelectionHint.Visibility = hasInboxSelection
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                MediaInboxCompactDetailsButton.Visibility = inboxStack && hasInboxSelection
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                MediaInboxCompactDetailsButton.Content = mediaInboxInspectorOpen
                    ? "收起预览与归类 ›"
                    : "查看预览与归类 ›";

                // The inbox table is the star-sized row inside MediaInboxTableFrame. Its
                // floor is derived from the measured table chrome, not a stale single
                // constant: the header, four complete rows, and a possible horizontal
                // scrollbar all have to fit inside the DataGrid before page overflow is
                // used. The frame floor adds only its actual padding and border.
                var readableGridHeight = GetMediaInboxReadableGridHeight();
                var readableFrameHeight = MediaInboxGeometry.CalculateReadableFrameHeight(
                    readableGridHeight,
                    MediaInboxTableFrame.Padding,
                    MediaInboxTableFrame.BorderThickness);
                MediaInboxGrid.MinHeight = readableGridHeight;
                MediaInboxLayout.MinHeight = readableFrameHeight;
                MediaInboxTableFrame.MinHeight = readableFrameHeight;
                // The outer page viewer can measure its content with infinite height.
                // Give the inner grid an explicit finite viewport in every layout;
                // otherwise Standard row virtualization is defeated and every retained
                // inbox row becomes a live DataGridRow. The short-host fallback belongs
                // to the page surface and must never enlarge the DataGrid into its footer.
                // The footer now owns the filter preset, availability, summary,
                // secondary-action and failure rows. The old 220 DIP budget was
                // calibrated before those rows moved below the table and lets a
                // large page request more grid height than its table frame can
                // visibly expose. Reserve the current non-table chrome budget;
                // compact/fallback layouts still use the page surface below.
                const double inboxNonTableHeightBudget = 360d;
                var finiteInboxHeight = Math.Max(readableGridHeight, height - inboxNonTableHeightBudget);
                MediaInboxGrid.Height = finiteInboxHeight;
                MediaInboxGrid.MaxHeight = finiteInboxHeight;
                MediaCurrentScrollSurface.MinHeight = pageViewportHeight;
                var mediaViewportHeight = Math.Max(154d, Math.Min(560, pageViewportHeight * 0.60));
                MediaGrid.MinHeight = 154d;
                MediaGrid.Height = mediaViewportHeight;
                MediaGrid.MaxHeight = mediaViewportHeight;

                // Match the demo: the media table and its inspector share the main
                // work area on wide hosts; on compact hosts the inspector is a drawer
                // behind the compact details button instead of consuming the list row.
                // The normal Playnite workspace is narrower than the complete
                // window because of the sidebar. Keep the Demo's grid and inspector
                // side by side until the compact breakpoint instead of hiding the
                // inspector at ordinary 1040 DIP layouts.
                var wasWideDetailLayout = detailBreakpoint.IsInitialized && !detailBreakpoint.IsCompact;
                var stack = detailBreakpoint.Evaluate(width);
                // The current-game batch summary and three actions wrap in compact
                // workspaces below 700 DIP; restore authored margins once they fit.
                mediaCurrentBatchActionRowGap.SetRowGap(width < 700d
                    ? ResponsiveLayoutCoordinator.CompactActionRowGap
                    : 0d);
                if (wasWideDetailLayout && stack
                    && MediaGrid.SelectedItem != null
                    && MediaInspectorScrollViewer.Visibility == Visibility.Visible)
                {
                    // Keep the same selected inspector open when it moves below the
                    // table. Its ScrollViewer and keyboard target are not recreated.
                    mediaInspectorOpen = true;
                }
                if (stack)
                {
                    MediaCurrentActionRow.RowDefinitions[1].Height = GridLength.Auto;
                    Grid.SetRow(MediaCurrentActionHint, 0);
                    Grid.SetColumn(MediaCurrentActionHint, 0);
                    Grid.SetColumnSpan(MediaCurrentActionHint, 2);
                    Grid.SetRow(MediaCurrentBatchActions, 1);
                    Grid.SetColumn(MediaCurrentBatchActions, 0);
                    Grid.SetColumnSpan(MediaCurrentBatchActions, 1);
                    MediaCurrentBatchActions.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Grid.SetRow(MediaCompactDetailsButton, 1);
                    Grid.SetColumn(MediaCompactDetailsButton, 1);
                    MediaCompactDetailsButton.HorizontalAlignment = HorizontalAlignment.Right;

                    var hasMediaSelection = MediaGrid.SelectedItem != null;
                    if (hasMediaSelection)
                    {
                        MediaInspectorScrollViewer.Visibility = mediaInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        MediaCompactDetailsButton.Content = mediaInspectorOpen
                            ? "收起媒体详情 ›"
                            : "查看媒体详情 ›";
                        MediaCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MediaCompactDetailsButton.Visibility = Visibility.Collapsed;
                        mediaInspectorOpen = false;
                    }
                }
                else
                {
                    MediaCurrentActionRow.RowDefinitions[1].Height = new GridLength(0);
                    Grid.SetRow(MediaCurrentActionHint, 0);
                    Grid.SetColumn(MediaCurrentActionHint, 0);
                    Grid.SetColumnSpan(MediaCurrentActionHint, 1);
                    Grid.SetRow(MediaCurrentBatchActions, 0);
                    Grid.SetColumn(MediaCurrentBatchActions, 1);
                    Grid.SetColumnSpan(MediaCurrentBatchActions, 1);
                    MediaCurrentBatchActions.HorizontalAlignment = HorizontalAlignment.Right;
                    Grid.SetRow(MediaCompactDetailsButton, 0);
                    Grid.SetColumn(MediaCompactDetailsButton, 1);
                    MediaCompactDetailsButton.HorizontalAlignment = HorizontalAlignment.Right;

                    MediaCompactDetailsButton.Visibility = Visibility.Collapsed;
                    // A compact layout temporarily collapses the inspector to make room
                    // for the list. When the host grows back to the wide layout, restore
                    // the inspector for the still-selected media item instead of leaving
                    // the right column permanently measured as 0x0.
                    if (MediaGrid.SelectedItem != null)
                        MediaInspectorScrollViewer.Visibility = Visibility.Visible;
                }
                var showInspector = MediaInspectorScrollViewer.Visibility == Visibility.Visible;
                var inspectorWidth = MediaCurrentLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
                MediaCurrentLayout.ColumnDefinitions[1].Width = showInspector && !stack ? new GridLength(14) : new GridLength(0);
                MediaCurrentLayout.ColumnDefinitions[2].Width = showInspector && !stack ? inspectorWidth : new GridLength(0);
                MediaCurrentLayout.RowDefinitions[1].Height = showInspector && stack
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetColumn(MediaInspectorFrame, stack ? 0 : 2);
                Grid.SetColumnSpan(MediaInspectorFrame, stack ? 3 : 1);
                Grid.SetRow(MediaInspectorFrame, stack ? 1 : 0);
                MediaInspectorFrame.Margin = showInspector && stack ? new Thickness(0, 10, 0, 0) : new Thickness(0);
                // The inspector itself always uses the demo's details-first layout:
                // 媒体详情 -> 文件名/路径 -> 预览 -> 收藏/备注/保存/打开.
                // Responsive work only moves that complete inspector beside/below the media list;
                // it never rewrites the inspector's internal visual tree during a resize.
                // Give a real inspector its own finite scroll channel only when it is stacked;
                // an empty selection must not retain a hidden capped surface.
                MediaInspectorScrollViewer.MaxHeight = showInspector
                    ? (stack
                        ? Math.Min(220, Math.Max(0, height * 0.30))
                        : mediaViewportHeight)
                    : double.PositiveInfinity;
                MediaPreviewPanel.Margin = new Thickness(0, 14, 0, 14);
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private double GetMediaInboxReadableGridHeight()
        {
            var headerHeight = MediaInboxGrid.ColumnHeaderHeight;
            if (double.IsNaN(headerHeight) || double.IsInfinity(headerHeight) || headerHeight <= 0)
                headerHeight = ReadDensityMetric("GscTableHeaderHeight", MediaInboxGeometry.DefaultHeaderHeight);

            var rowHeight = FindVisualDescendants(MediaInboxGrid)
                .OfType<DataGridRow>()
                .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
                .Select(row => row.ActualHeight)
                .FirstOrDefault();
            if (rowHeight <= 0)
                rowHeight = ReadDensityMetric("GscTableRowHeight", MediaInboxGeometry.DefaultRowHeight);

            var horizontalScrollBarHeight = FindVisualDescendants(MediaInboxGrid)
                .OfType<ScrollBar>()
                .Where(scrollBar => scrollBar.Orientation == Orientation.Horizontal
                    && scrollBar.Visibility == Visibility.Visible
                    && scrollBar.ActualHeight > 0)
                .Select(scrollBar => scrollBar.ActualHeight)
                .FirstOrDefault();
            if (horizontalScrollBarHeight <= 0
                && MediaInboxGrid.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                // Auto scrollbars are created after the first arrange. Reserve the
                // system metric on the first pass so a narrow table cannot briefly
                // surrender its fourth row before the scrollbar is realized.
                horizontalScrollBarHeight = SystemParameters.HorizontalScrollBarHeight;
                if (horizontalScrollBarHeight <= 0)
                    horizontalScrollBarHeight = MediaInboxGeometry.DefaultHorizontalScrollBarHeight;
            }

            return MediaInboxGeometry.CalculateReadableGridHeight(
                headerHeight,
                rowHeight,
                horizontalScrollBarHeight);
        }

        private double ReadDensityMetric(string key, double fallback)
        {
            var value = TryFindResource(key);
            if (value == null) return fallback;
            try
            {
                var metric = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                return double.IsNaN(metric) || double.IsInfinity(metric) || metric <= 0 ? fallback : metric;
            }
            catch (InvalidCastException)
            {
                return fallback;
            }
            catch (FormatException)
            {
                return fallback;
            }
        }

        private void OnMediaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!restoringSelection && !selectionRestoreQueued)
                UpdateSelectionDelta(selectedMediaIds, e);
            UpdateMediaSelectionSummary();
            mediaInspectorOpen = false;
            ResetSelectedVideoPreview();
            QueueSelectedMediaIntoView();
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void UpdateMediaSelectionSummary()
        {
            if (MediaGrid == null || MediaCurrentBatchSelectionSummary == null)
                return;

            var visibleCount = MediaGrid.SelectedItems.Count;
            var totalCount = selectedMediaIds.Count;
            var hiddenCount = Math.Max(0, totalCount - visibleCount);
            var resultCount = MediaGrid.Items.Count;
            MediaCurrentBatchSelectionSummary.Text = totalCount == 0
                ? $"当前结果 {resultCount} 项 · 未选择媒体 · Ctrl / Shift 多选"
                : hiddenCount > 0
                    ? $"已选 {totalCount} 项 · 当前结果 {resultCount} 项 · 当前窗口 {visibleCount} 项可操作 · 另 {hiddenCount} 项暂不可见"
                    : $"已选 {totalCount} 项 · 当前结果 {resultCount} 项 · 可批量处理";
        }

        private void QueueSelectedMediaIntoView()
        {
            if (!IsLoaded || MediaGrid == null)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var selected = attachedViewModel?.SelectedMedia;
                if (selected == null || MediaGrid.Items.IndexOf(selected) < 0)
                    return;

                MediaGrid.ScrollIntoView(selected);
                MediaGrid.UpdateLayout();
                (MediaGrid.ItemContainerGenerator.ContainerFromItem(selected) as FrameworkElement)?.BringIntoView();
            }));
        }

        private void ResetSelectedVideoPreview()
        {
            if (MediaSelectedVideoFallback == null)
                return;

            MediaSelectedVideoFallback.Visibility = Visibility.Collapsed;
            MediaSelectedVideo?.ClearValue(UIElement.VisibilityProperty);
            var selected = attachedViewModel?.SelectedMedia;
            if (selected?.Kind != MediaKind.VideoClip)
                return;

            var path = selected.ArchivePath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsSupportedVideoPath(path))
            {
                MediaSelectedVideoFallback.Visibility = Visibility.Visible;
                if (MediaSelectedVideo != null)
                    MediaSelectedVideo.Visibility = Visibility.Collapsed;
            }
        }

        private void OnSelectedMediaVideoFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MediaSelectedVideo.Visibility = Visibility.Collapsed;
            MediaSelectedVideoFallback.Visibility = Visibility.Visible;
        }

        private static bool IsSupportedVideoPath(string path)
        {
            var extension = Path.GetExtension(path);
            return string.Equals(extension, ".mp4", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".m4v", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".wmv", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".avi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".mov", StringComparison.OrdinalIgnoreCase);
        }

        private void OnReloadMediaWindowClick(object sender, RoutedEventArgs e)
        {
            pendingMediaAnchor = null;
            HideAnchorNotice(isInbox: false);
        }

        private void OnReloadMediaInboxClick(object sender, RoutedEventArgs e)
        {
            pendingInboxAnchor = null;
            pendingInboxAnchorMode = null;
            HideAnchorNotice(isInbox: true);
        }

        private HashSet<string> GetInboxSelectionSet(string? mode = null)
        {
            var key = mode ?? attachedViewModel?.MediaInboxMode ?? "待归类";
            if (!selectedInboxIdsByMode.TryGetValue(key, out var selection))
            {
                selection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                selectedInboxIdsByMode[key] = selection;
            }
            return selection;
        }

        private static void UpdateSelectionDelta(HashSet<string> selection, SelectionChangedEventArgs args)
        {
            foreach (var item in args.RemovedItems)
            {
                var id = GetMediaId(item);
                if (!string.IsNullOrWhiteSpace(id)) selection.Remove(id!);
            }
            foreach (var item in args.AddedItems)
            {
                var id = GetMediaId(item);
                if (!string.IsNullOrWhiteSpace(id)) selection.Add(id!);
            }
        }

        private static void CaptureSelection(System.Collections.IList selectedItems, HashSet<string> selection)
        {
            selection.Clear();
            foreach (var item in selectedItems)
            {
                var id = GetMediaId(item);
                if (!string.IsNullOrWhiteSpace(id)) selection.Add(id!);
            }
        }

        private void QueueRestore(ItemsControl itemsControl, ScrollAnchor anchor, HashSet<string> selection, bool isInbox, string? mode, long generation)
        {
            MarkAnchorDiagnostic(itemsControl, $"queued:id={anchor.ItemId}:generation={generation}");
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => RestoreAnchor(itemsControl, anchor, selection, isInbox, mode, generation, 0)));
        }

        private void RestoreAnchor(
            ItemsControl itemsControl,
            ScrollAnchor anchor,
            HashSet<string> selection,
            bool isInbox,
            string? mode,
            long generation,
            int attempt)
        {
            if (!IsCurrentAnchorRestore(generation, isInbox, mode))
            {
                MarkAnchorDiagnostic(itemsControl, $"skipped:stale-generation={generation}");
                return;
            }

            RestoreSelection(itemsControl, selection);
            var itemIndex = FindItemIndex(itemsControl, anchor.ItemId);
            if (itemIndex < 0)
            {
                MarkAnchorDiagnostic(itemsControl, $"failed:item-not-found:id={anchor.ItemId}");
                ShowAnchorNotice(isInbox, selection.Count);
                return;
            }

            var viewer = FindScrollViewer(itemsControl);
            if (viewer == null)
            {
                MarkAnchorDiagnostic(itemsControl, $"retry:scrollviewer-missing:attempt={attempt}");
                RetryRestore(itemsControl, anchor, selection, isInbox, mode, generation, attempt);
                return;
            }

            MarkAnchorDiagnostic(itemsControl, $"executing:{anchor.Mode}:attempt={attempt}:reason=collection-refresh");
            if (itemsControl is DataGrid dataGrid)
                dataGrid.ScrollIntoView(itemsControl.Items[itemIndex]);
            else if (itemsControl is ListBox listBox)
                listBox.ScrollIntoView(itemsControl.Items[itemIndex]);
            itemsControl.UpdateLayout();

            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(itemIndex) as FrameworkElement;
            if (container == null)
            {
                MarkAnchorDiagnostic(itemsControl, $"retry:container-missing:attempt={attempt}");
                RetryRestore(itemsControl, anchor, selection, isInbox, mode, generation, attempt);
                return;
            }

            if (anchor.Mode == ScrollAnchorMode.LogicalItems)
            {
                // This branch is only entered when the actual visual tree contains a
                // VirtualizingStackPanel and ScrollUnit=Item. CanContentScroll by itself
                // is not proof that VerticalOffset is an item index.
                var remainder = anchor.LogicalOffset - anchor.ItemIndex;
                viewer.ScrollToVerticalOffset(itemIndex + Math.Max(0, remainder));
            }
            else
            {
                // Wrap panels and pixel-based ScrollInfo use DIP geometry. Keep the row at
                // the captured position relative to the real content presenter, rather
                // than comparing its Y coordinate with a logical ViewportHeight.
                var currentTop = GetRelativeTop(container, viewer);
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + currentTop - anchor.RelativeTop);
            }

            HideAnchorNotice(isInbox);
            MarkAnchorDiagnostic(itemsControl, $"completed:{anchor.Mode}:reason=collection-refresh");
            selectionRestoreQueued = false;
        }

        private bool IsCurrentAnchorRestore(long generation, bool isInbox, string? mode)
            => IsLoaded
                && generation == anchorRestoreGeneration
                && (!isInbox
                    || attachedViewModel == null
                    || string.Equals(attachedViewModel.MediaInboxMode, mode, StringComparison.Ordinal));

        private void RetryRestore(
            ItemsControl itemsControl,
            ScrollAnchor anchor,
            HashSet<string> selection,
            bool isInbox,
            string? mode,
            long generation,
            int attempt)
        {
            if (attempt >= 4)
            {
                MarkAnchorDiagnostic(itemsControl, $"failed:retry-exhausted:{anchor.Mode}");
                ShowAnchorNotice(isInbox, selection.Count);
                return;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => RestoreAnchor(itemsControl, anchor, selection, isInbox, mode, generation, attempt + 1)));
        }

        private void MarkAnchorDiagnostic(ItemsControl itemsControl, string state)
        {
            anchorDiagnostic = string.IsNullOrWhiteSpace(state) ? "unknown" : state;
            if (itemsControl is DataGrid dataGrid)
                DataGridScrollDiagnostics.MarkTrigger(dataGrid, "锚点恢复:" + anchorDiagnostic);
        }

        private void RestoreSelection(ItemsControl itemsControl, HashSet<string> selection)
        {
            restoringSelection = true;
            try
            {
                var selectedItems = GetSelectedItems(itemsControl);
                selectedItems.Clear();
                foreach (var item in itemsControl.Items.Cast<object>())
                {
                    if (selection.Contains(GetMediaId(item) ?? string.Empty))
                        selectedItems.Add(item);
                }
            }
            finally
            {
                restoringSelection = false;
            }
        }

        private static System.Collections.IList GetSelectedItems(ItemsControl itemsControl)
            => itemsControl switch
            {
                DataGrid dataGrid => dataGrid.SelectedItems,
                ListBox listBox => listBox.SelectedItems,
                _ => throw new NotSupportedException($"不支持的多选控件：{itemsControl.GetType().Name}")
            };

        private void ShowAnchorNotice(bool isInbox, int trackedSelectionCount)
        {
            selectionRestoreQueued = false;
            var notice = isInbox ? MediaInboxWindowAnchorNotice : MediaWindowAnchorNotice;
            var button = isInbox ? ReloadMediaInboxButton : ReloadMediaWindowButton;
            var suffix = trackedSelectionCount > 0
                ? $"；已选择 {trackedSelectionCount} 项中仅当前保留项参与操作"
                : string.Empty;
            notice.Text = $"列表窗口已前移，当前位置不可恢复{suffix}";
            notice.Visibility = Visibility.Visible;
            button.Visibility = Visibility.Visible;
        }

        private void HideAnchorNotice(bool isInbox)
        {
            (isInbox ? MediaInboxWindowAnchorNotice : MediaWindowAnchorNotice).Visibility = Visibility.Collapsed;
            (isInbox ? ReloadMediaInboxButton : ReloadMediaWindowButton).Visibility = Visibility.Collapsed;
        }

        private static int FindItemIndex(ItemsControl itemsControl, string itemId)
        {
            for (var index = 0; index < itemsControl.Items.Count; index++)
            {
                if (string.Equals(GetMediaId(itemsControl.Items[index]), itemId, StringComparison.OrdinalIgnoreCase))
                    return index;
            }
            return -1;
        }

        private static ScrollAnchor? CaptureAnchor(ItemsControl itemsControl)
        {
            var viewer = FindScrollViewer(itemsControl);
            if (viewer == null || itemsControl.Items.Count == 0) return null;

            var mode = GetScrollAnchorMode(itemsControl, viewer);
            var viewport = GetContentViewport(viewer);
            if (viewport.IsEmpty || viewport.Height <= 0)
                return null;

            var firstIndex = -1;
            var firstTop = double.MaxValue;
            for (var index = 0; index < itemsControl.Items.Count; index++)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromIndex(index) is not FrameworkElement container)
                    continue;
                var top = GetRelativeTop(container, viewer);
                var bottom = top + Math.Max(1, container.ActualHeight);
                if (bottom <= viewport.Top || top >= viewport.Bottom)
                    continue;
                if (top < firstTop)
                {
                    firstTop = top;
                    firstIndex = index;
                }
            }

            if (firstIndex < 0)
                return null;

            var itemId = GetMediaId(itemsControl.Items[firstIndex]);
            return string.IsNullOrWhiteSpace(itemId)
                ? null
                : new ScrollAnchor(itemId!, firstIndex, firstTop, viewer.VerticalOffset, mode);
        }

        private static ScrollAnchorMode GetScrollAnchorMode(ItemsControl itemsControl, ScrollViewer viewer)
        {
            if (FindDescendant<VirtualizingWrapPanel>(itemsControl) != null)
                return ScrollAnchorMode.Pixels;

            var scrollUnit = VirtualizingPanel.GetScrollUnit(itemsControl);
            var hasVirtualizingStackPanel = FindVisualDescendants(viewer)
                .OfType<VirtualizingStackPanel>()
                .Any();
            var canContentScroll = ScrollViewer.GetCanContentScroll(itemsControl) && viewer.CanContentScroll;
            return canContentScroll && scrollUnit == ScrollUnit.Item && hasVirtualizingStackPanel
                ? ScrollAnchorMode.LogicalItems
                : ScrollAnchorMode.Pixels;
        }

        private static Rect GetContentViewport(ScrollViewer viewer)
        {
            var presenter = FindDescendant<ScrollContentPresenter>(viewer);
            if (presenter != null)
            {
                var rect = presenter.TransformToAncestor(viewer).TransformBounds(
                    new Rect(0, 0, presenter.ActualWidth, presenter.ActualHeight));
                if (!rect.IsEmpty && rect.Width > 0 && rect.Height > 0)
                    return rect;
            }

            return new Rect(0, 0, viewer.ViewportWidth, viewer.ViewportHeight);
        }

        private static double GetRelativeTop(FrameworkElement element, Visual ancestor)
            => element.TransformToAncestor(ancestor).Transform(new Point(0, 0)).Y;

        private static string? GetMediaId(object? item) => (item as MediaItemDto)?.MediaId;

        private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                var child = VisualTreeHelper.GetChild(root, index);
                if (child is T match) return match;
                var nested = FindDescendant<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private static ScrollViewer? FindScrollViewer(ItemsControl itemsControl)
            => FindVisualDescendants(itemsControl)
                .OfType<ScrollViewer>()
                .OrderByDescending(viewer => FindDescendant<DataGridRowsPresenter>(viewer) != null)
                .ThenByDescending(viewer => viewer.ViewportHeight)
                .ThenByDescending(viewer => viewer.ViewportWidth)
                .FirstOrDefault();

        private static IEnumerable<DependencyObject> FindVisualDescendants(DependencyObject root)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                var child = VisualTreeHelper.GetChild(root, index);
                yield return child;
                foreach (var nested in FindVisualDescendants(child))
                    yield return nested;
            }
        }

        private enum ScrollAnchorMode
        {
            Pixels,
            LogicalItems
        }

        private sealed class ScrollAnchor
        {
            public ScrollAnchor(string itemId, int itemIndex, double relativeTop, double logicalOffset)
                : this(itemId, itemIndex, relativeTop, logicalOffset, ScrollAnchorMode.Pixels)
            {
            }

            public ScrollAnchor(string itemId, int itemIndex, double relativeTop, double logicalOffset, ScrollAnchorMode mode)
            {
                ItemId = itemId;
                ItemIndex = itemIndex;
                RelativeTop = relativeTop;
                LogicalOffset = logicalOffset;
                Mode = mode;
            }

            public string ItemId { get; }
            public int ItemIndex { get; }
            public double RelativeTop { get; }
            public double LogicalOffset { get; }
            public ScrollAnchorMode Mode { get; }
        }

        private void OnMediaCompactDetailsClick(object sender, RoutedEventArgs e)
        {
             if (MediaGrid.SelectedItem == null) return;
             mediaInspectorOpen = !mediaInspectorOpen;
             ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            if (mediaInspectorOpen)
                FocusElement(MediaInspectorScrollViewer);
            else
                FocusSelectedMediaRow();
        }

        private void OnMediaInboxCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (MediaInboxGrid.SelectedItem == null) return;
            mediaInboxInspectorOpen = !mediaInboxInspectorOpen;
            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(mediaInboxInspectorOpen ? MediaInboxInspectorScrollViewer : MediaInboxCompactDetailsButton);
        }

        private void OnMediaInspectorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || !mediaInspectorOpen)
                return;

            mediaInspectorOpen = false;
             ApplyResponsiveLayout(
                 responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                 responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusSelectedMediaRow();
             e.Handled = true;
         }

        private void FocusSelectedMediaRow()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var selected = attachedViewModel?.SelectedMedia;
                if (selected == null || MediaGrid.Items.IndexOf(selected) < 0)
                    return;

                MediaGrid.ScrollIntoView(selected);
                MediaGrid.UpdateLayout();
                if (MediaGrid.ItemContainerGenerator.ContainerFromItem(selected) is UIElement row)
                    FocusElement(row);
            }));
        }

        private void OnMediaInboxInspectorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            var closedHistory = false;
            if (mediaInboxHistoryOpen)
            {
                mediaInboxHistoryOpen = false;
                closedHistory = true;
            }
            else if (mediaInboxInspectorOpen)
                mediaInboxInspectorOpen = false;
            else
                return;

            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(closedHistory ? MediaInboxHistoryButton : MediaInboxCompactDetailsButton);
            e.Handled = true;
        }

        private void OnMediaInboxHistoryClick(object sender, RoutedEventArgs e)
        {
            mediaInboxHistoryOpen = !mediaInboxHistoryOpen;
            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(mediaInboxHistoryOpen ? MediaInboxInspectorScrollViewer : MediaInboxHistoryButton);
        }

        private static void FocusElement(UIElement element)
        {
            if (!element.IsVisible || !element.IsEnabled)
                return;

            element.Focus();
            Keyboard.Focus(element);
        }

    }
}
