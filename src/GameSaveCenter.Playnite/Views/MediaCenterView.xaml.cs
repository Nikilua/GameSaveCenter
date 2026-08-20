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

        public UniformGrid MediaSummaryPanelElement => MediaSummaryPanel;
        public UniformGrid MediaSourceFieldsElement => MediaSourceFields;
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
                // Keep the Demo's four metric cards in one row at normal workspace widths,
                // then reflow only when the available logical width is genuinely narrow.
                // Do not discard summary information at short heights. Local list/inspector
                // surfaces own overflow so the whole workspace does not become a scroll canvas.
                var metricColumns = width >= 700 ? 4 : width >= 520 ? 2 : 1;
                MediaSummaryPanel.Columns = metricColumns;
                MediaSummaryPanel.Visibility = Visibility.Visible;
                MediaSourceFields.Columns = width >= 820 ? 2 : 1;

                // The inbox is a real two-pane workspace, not a table followed by a
                // hidden action strip. Keep the preview/classification controls in their
                // own measured column on wide hosts and move that complete inspector below
                // the table only at the compact breakpoint.
                var inboxStack = width < 980;
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
