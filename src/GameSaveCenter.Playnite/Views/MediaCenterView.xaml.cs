using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GameSaveCenter.Playnite.Views
{
    public partial class MediaCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private bool mediaInspectorOpen;

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

                // The inbox is a real two-pane workspace, not a table followed by a
                // hidden action strip. Keep the preview/classification controls in their
                // own measured column on wide hosts and move that complete inspector below
                // the table only at the compact breakpoint.
                // The inbox inspector is part of the primary task: at the normal
                // Playnite content width (~740 DIP after the sidebar) keep it beside
                // the table so a selected item immediately exposes its preview and
                // classification actions. Only genuinely narrow panes stack it.
                var inboxStack = width < 700;
                var inboxInspectorWidth = MediaInboxLayout.TryFindResource("GscInspectorWidth") is GridLength inboxLength
                    ? inboxLength
                    : new GridLength(360);
                MediaInboxLayout.ColumnDefinitions[1].Width = inboxStack ? new GridLength(0) : new GridLength(14);
                MediaInboxLayout.ColumnDefinitions[2].Width = inboxStack ? new GridLength(0) : inboxInspectorWidth;
                MediaInboxLayout.RowDefinitions[1].Height = inboxStack
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetColumn(MediaInboxInspectorScrollViewer, inboxStack ? 0 : 2);
                Grid.SetColumnSpan(MediaInboxInspectorScrollViewer, inboxStack ? 3 : 1);
                Grid.SetRow(MediaInboxInspectorScrollViewer, inboxStack ? 1 : 0);
                MediaInboxInspectorScrollViewer.Margin = inboxStack ? new Thickness(0, 10, 0, 0) : new Thickness(0);
                MediaInboxInspectorScrollViewer.MaxHeight = inboxStack
                    ? Math.Max(260, Math.Min(520, height * 0.62))
                    : double.PositiveInfinity;

                // Both media tables retain a bounded, readable viewport. The surrounding tab
                // surface scrolls the page-level info/actions when this viewport cannot fit
                // below the summary cards; the DataGrid/ListBox still own row virtualization
                // and their own internal scrolling.
                MediaInboxGrid.MinHeight = 236d;
                MediaInboxGrid.Height = double.NaN;
                MediaInboxGrid.MaxHeight = double.PositiveInfinity;
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

    }
}
