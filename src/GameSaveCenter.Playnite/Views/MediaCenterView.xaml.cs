using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    public partial class MediaCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private bool mediaInspectorOpen;
        private bool mediaInboxInspectorOpen;
        private DashboardViewModel? attachedViewModel;
        private readonly HashSet<string> selectedMediaIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> selectedInboxIdsByMode = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private ScrollAnchor? pendingMediaAnchor;
        private ScrollAnchor? pendingInboxAnchor;
        private string? pendingInboxAnchorMode;
        private bool restoringSelection;
        private bool selectionRestoreQueued;
        private long anchorRestoreGeneration;
        private readonly DispatcherTimer pendingAnchorExpiryTimer;

        public MediaCenterView()
        {
            InitializeComponent();
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

        private void OnLoaded(object sender, RoutedEventArgs e) => AttachViewModel(DataContext as DashboardViewModel);

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InvalidatePendingAnchorRestore();
            pendingAnchorExpiryTimer.Stop();
            DetachViewModel();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => AttachViewModel(e.NewValue as DashboardViewModel);

        private void AttachViewModel(DashboardViewModel? viewModel)
        {
            if (ReferenceEquals(attachedViewModel, viewModel)) return;
            InvalidatePendingAnchorRestore();
            DetachViewModel();
            attachedViewModel = viewModel;
            if (viewModel == null) return;

            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.Media.CollectionChanged += OnMediaCollectionChanged;
            viewModel.UnassignedMedia.CollectionChanged += OnInboxCollectionChanged;
            viewModel.IgnoredMedia.CollectionChanged += OnInboxCollectionChanged;
        }

        private void DetachViewModel()
        {
            if (attachedViewModel == null) return;
            attachedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            attachedViewModel.Media.CollectionChanged -= OnMediaCollectionChanged;
            attachedViewModel.UnassignedMedia.CollectionChanged -= OnInboxCollectionChanged;
            attachedViewModel.IgnoredMedia.CollectionChanged -= OnInboxCollectionChanged;
            attachedViewModel = null;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.SelectedGame), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.CurrentWorkspace), StringComparison.Ordinal)
                || string.Equals(e.PropertyName, nameof(DashboardViewModel.MediaInboxMode), StringComparison.Ordinal))
            {
                InvalidatePendingAnchorRestore();
            }
        }

        private void InvalidatePendingAnchorRestore()
        {
            unchecked { anchorRestoreGeneration++; }
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
            var count=MediaInboxGrid.SelectedItems.Count;
            var ignored=string.Equals(attachedViewModel?.MediaInboxMode, "已忽略", StringComparison.Ordinal);
            var tracked = GetInboxSelectionSet().Count;
            var evicted = Math.Max(0, tracked - count);
            MediaInboxBatchSelectionSummary.Text=count==0 && evicted == 0
                ? "可按住 Ctrl / Shift 多选"
                : ignored
                    ? evicted > 0 ? $"当前窗口 {count} 项；另有 {evicted} 项已裁掉，不参与本次操作" : $"已选择 {count} 项；可恢复到待归类"
                    : evicted > 0 ? $"当前窗口 {count} 项；另有 {evicted} 项已裁掉，不参与本次操作" : $"已选择 {count} 项；目标游戏可在此处调整";
            CommandManager.InvalidateRequerySuggested();
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnMediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
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
            QueueRestore(MediaInboxGrid, anchor, GetInboxSelectionSet(mode), isInbox: true, mode: mode, generation: anchorRestoreGeneration);
        }

        private void OnLoadMoreMediaClick(object sender, RoutedEventArgs e)
        {
            if (attachedViewModel == null) return;
            CaptureSelection(MediaGrid.SelectedItems, selectedMediaIds);
            pendingMediaAnchor = CaptureAnchor(MediaGrid);
            ArmPendingAnchorExpiry();
        }

        private void OnLoadMoreMediaInboxClick(object sender, RoutedEventArgs e)
        {
            if (attachedViewModel == null) return;
            var mode = attachedViewModel.MediaInboxMode;
            var selection = GetInboxSelectionSet(mode);
            CaptureSelection(MediaInboxGrid.SelectedItems, selection);
            pendingInboxAnchorMode = mode;
            pendingInboxAnchor = CaptureAnchor(MediaInboxGrid);
            ArmPendingAnchorExpiry();
        }

        private void OnMediaInboxModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InvalidatePendingAnchorRestore();
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

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
                responsiveWidth = width;
                responsiveHeight = height;
                // Keep the Demo's four metrics in one continuous strip. Do not discard
                // summary information at short heights. Local list/inspector surfaces own
                // overflow so the whole workspace does not become a scroll canvas.
                MediaSummaryPanel.Visibility = Visibility.Visible;
                var compactHeight = height < 760;
                MediaSummaryPanel.MinHeight = compactHeight ? 68 : 84;
                MediaSummaryPanel.Padding = compactHeight
                    ? new Thickness(6, 8, 6, 8)
                    : new Thickness(6, 14, 6, 14);
                MediaInboxInfoBand.Padding = compactHeight
                    ? new Thickness(10, 6, 10, 6)
                    : new Thickness(14, 11, 14, 11);
                MediaInboxInfoDescription.Visibility = compactHeight
                    ? Visibility.Collapsed
                    : Visibility.Visible;
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
                var showInboxInspector = !inboxStack || (mediaInboxInspectorOpen && hasInboxSelection);
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
                MediaInboxInspectorScrollViewer.MaxHeight = inboxStack && showInboxInspector
                    ? Math.Max(260, Math.Min(520, height * 0.62))
                    : double.PositiveInfinity;
                MediaInboxCompactDetailsButton.Visibility = inboxStack && hasInboxSelection
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                MediaInboxCompactDetailsButton.Content = mediaInboxInspectorOpen
                    ? "收起预览与归类 ›"
                    : "查看预览与归类 ›";

                // Both media tables retain a bounded, readable viewport. The surrounding tab
                // surface scrolls the page-level info/actions when this viewport cannot fit
                // below the summary cards; the DataGrid/ListBox still own row virtualization
                // and their own internal scrolling.
                MediaInboxGrid.MinHeight = 236d;
                var inboxGridHeight = Math.Max(236, Math.Min(420, height - (compactHeight ? 220 : 300)));
                MediaInboxGrid.Height = inboxGridHeight;
                MediaInboxGrid.MaxHeight = inboxGridHeight;
                MediaGrid.MinHeight = 236d;
                MediaGrid.Height = double.NaN;
                MediaGrid.MaxHeight = double.PositiveInfinity;

                // Match the demo: the media table and its inspector share the main
                // work area on wide hosts; on compact hosts the inspector is a drawer
                // behind the compact details button instead of consuming the list row.
                // The normal Playnite workspace is narrower than the complete
                // window because of the sidebar. Keep the Demo's grid and inspector
                // side by side until the compact breakpoint instead of hiding the
                // inspector at ordinary 1040 DIP layouts.
                var stack = width < 980;
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
                MediaInspectorScrollViewer.MaxHeight = showInspector && stack
                    ? Math.Max(220, Math.Min(420, height * 0.56))
                    : double.PositiveInfinity;
                MediaPreviewPanel.Margin = new Thickness(0, 14, 0, 14);
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private void OnMediaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!restoringSelection && !selectionRestoreQueued)
                UpdateSelectionDelta(selectedMediaIds, e);
            mediaInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
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
                return;

            RestoreSelection(itemsControl, selection);
            var itemIndex = FindItemIndex(itemsControl, anchor.ItemId);
            if (itemIndex < 0)
            {
                ShowAnchorNotice(isInbox, selection.Count);
                return;
            }

            var viewer = FindDescendant<ScrollViewer>(itemsControl);
            if (viewer == null)
            {
                RetryRestore(itemsControl, anchor, selection, isInbox, mode, generation, attempt);
                return;
            }

            if (itemsControl is DataGrid dataGrid)
                dataGrid.ScrollIntoView(itemsControl.Items[itemIndex]);
            else if (itemsControl is ListBox listBox)
                listBox.ScrollIntoView(itemsControl.Items[itemIndex]);
            itemsControl.UpdateLayout();

            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(itemIndex) as FrameworkElement;
            if (container == null)
            {
                RetryRestore(itemsControl, anchor, selection, isInbox, mode, generation, attempt);
                return;
            }

            if (FindDescendant<VirtualizingWrapPanel>(itemsControl) != null)
            {
                var currentTop = GetRelativeTop(container, viewer);
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + currentTop - anchor.RelativeTop);
            }
            else if (viewer.CanContentScroll)
            {
                var remainder = anchor.LogicalOffset - anchor.ItemIndex;
                viewer.ScrollToVerticalOffset(itemIndex + Math.Max(0, remainder));
            }
            else
            {
                var currentTop = GetRelativeTop(container, viewer);
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + currentTop - anchor.RelativeTop);
            }

            HideAnchorNotice(isInbox);
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
                ShowAnchorNotice(isInbox, selection.Count);
                return;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => RestoreAnchor(itemsControl, anchor, selection, isInbox, mode, generation, attempt + 1)));
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
            var viewer = FindDescendant<ScrollViewer>(itemsControl);
            if (viewer == null || itemsControl.Items.Count == 0) return null;

            var firstIndex = -1;
            var firstTop = double.MaxValue;
            for (var index = 0; index < itemsControl.Items.Count; index++)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromIndex(index) is not FrameworkElement container)
                    continue;
                var top = GetRelativeTop(container, viewer);
                if (top + Math.Max(1, container.ActualHeight) <= 0 || top >= viewer.ViewportHeight)
                    continue;
                if (top < firstTop)
                {
                    firstTop = top;
                    firstIndex = index;
                }
            }

            if (firstIndex < 0)
            {
                firstIndex = Math.Min(itemsControl.Items.Count - 1, Math.Max(0, (int)Math.Floor(viewer.VerticalOffset)));
                firstTop = 0;
            }

            var itemId = GetMediaId(itemsControl.Items[firstIndex]);
            return string.IsNullOrWhiteSpace(itemId)
                ? null
                : new ScrollAnchor(itemId!, firstIndex, firstTop, viewer.VerticalOffset);
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

        private sealed class ScrollAnchor
        {
            public ScrollAnchor(string itemId, int itemIndex, double relativeTop, double logicalOffset)
            {
                ItemId = itemId;
                ItemIndex = itemIndex;
                RelativeTop = relativeTop;
                LogicalOffset = logicalOffset;
            }

            public string ItemId { get; }
            public int ItemIndex { get; }
            public double RelativeTop { get; }
            public double LogicalOffset { get; }
        }

        private void OnMediaCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (MediaGrid.SelectedItem == null) return;
            mediaInspectorOpen = !mediaInspectorOpen;
                ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, responsiveHeight > 0 ? responsiveHeight : ActualHeight);
        }

        private void OnMediaInboxCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (MediaInboxGrid.SelectedItem == null) return;
            mediaInboxInspectorOpen = !mediaInboxInspectorOpen;
            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            if (mediaInboxInspectorOpen)
                MediaInboxInspectorScrollViewer.Focus();
            e.Handled = true;
        }

    }
}
