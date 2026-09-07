using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        public MediaCenterView()
        {
            InitializeComponent();
            MediaInspectorScrollViewer.IsVisibleChanged += OnMediaInspectorIsVisibleChanged;
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
            var count=MediaInboxGrid.SelectedItems.Count;
            var ignored=string.Equals(MediaInboxModeCombo.SelectedItem as string,"已忽略",StringComparison.Ordinal);
            MediaInboxBatchSelectionSummary.Text=count==0
                ? "可按住 Ctrl / Shift 多选"
                : ignored ? $"已选择 {count} 项；可恢复到待归类" : $"已选择 {count} 项；目标游戏可在此处调整";
            CommandManager.InvalidateRequerySuggested();
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnMediaInboxModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            mediaInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
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
