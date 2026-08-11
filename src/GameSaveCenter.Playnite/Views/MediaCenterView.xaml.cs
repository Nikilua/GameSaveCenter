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

        public MediaCenterView()
        {
            InitializeComponent();
            MediaInspectorScrollViewer.IsVisibleChanged += OnMediaInspectorIsVisibleChanged;
        }

        private void OnMediaInspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
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
            responsiveWidth = width;
            responsiveHeight = height;
            // Keep the demo's four-card metric strip throughout normal windowed
            // workspaces. The Dashboard's content width is already smaller than the
            // complete window after the sidebar and shell insets, so a 1180-DIP
            // breakpoint incorrectly collapsed the cards to two or one column and
            // pushed the media table below the fold. These are logical-DIP thresholds:
            // 1080p, 2K and 4K at ordinary DPI all keep the primary table reachable.
            var metricColumns = width >= 700 ? 4 : width >= 520 ? 2 : 1;
            MediaSummaryPanel.Columns = metricColumns;
            // Do not discard summary information at short heights. Local list/inspector
            // surfaces own overflow so the whole workspace does not become a scroll canvas.
            MediaSummaryPanel.Visibility = Visibility.Visible;
            MediaSourceFields.Columns = width >= 820 ? 2 : 1;

            // Both media tables retain a bounded, readable viewport. The surrounding tab
            // surface scrolls the page-level info/actions when this viewport cannot fit
            // below the summary cards; the DataGrid/ListBox still own row virtualization
            // and their own internal scrolling.
            var tableViewportHeight = Math.Max(236d, Math.Min(460d, height * 0.50));
            MediaInboxGrid.Height = tableViewportHeight;
            MediaGrid.Height = tableViewportHeight;

            // Match the demo: the media table and its inspector share the main
            // work area on wide hosts; on compact hosts the inspector moves
            // below the table instead of becoming a narrow strip.
            var stack = width < 1080;
            var showInspector = MediaInspectorScrollViewer.Visibility == Visibility.Visible;
            var inspectorWidth = MediaCurrentLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
            MediaCurrentLayout.ColumnDefinitions[1].Width = showInspector && !stack ? new GridLength(14) : new GridLength(0);
            MediaCurrentLayout.ColumnDefinitions[2].Width = showInspector && !stack ? inspectorWidth : new GridLength(0);
            MediaCurrentLayout.RowDefinitions[3].Height = showInspector && stack
                ? new GridLength(1, GridUnitType.Auto)
                : new GridLength(0);
            Grid.SetColumn(MediaInspectorFrame, stack ? 0 : 2);
            Grid.SetColumnSpan(MediaInspectorFrame, stack ? 3 : 1);
            Grid.SetRow(MediaInspectorFrame, stack ? 3 : 2);
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
    }
}
