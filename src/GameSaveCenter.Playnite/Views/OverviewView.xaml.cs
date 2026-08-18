using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>
    /// Physical overview workspace extracted from DashboardView. The public layout
    /// accessors keep the existing responsive coordinator and command bindings intact
    /// while the remaining workspaces are migrated incrementally.
    /// </summary>
    public partial class OverviewView : UserControl
    {
        /// <summary>
        /// Mirrors the dashboard motion gate so hover feedback stays render-only and
        /// respects the user's animation/transparency preferences. The dashboard
        /// refreshes this flag whenever theme, settings or system parameters change.
        /// </summary>
        public bool UiAnimationsEnabled { get; set; } = true;

        public OverviewView() => InitializeComponent();

        private void OnStatCardMouseEnter(object sender, MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, -3, 160);

        private void OnStatCardMouseLeave(object sender, MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, 0, 180);

        private void AnimateTranslate(FrameworkElement? element, double x, double y, int milliseconds)
        {
            if (element == null || !UiAnimationsEnabled || SystemParameters.HighContrast || !SystemParameters.ClientAreaAnimation) return;
            var translate = GetMutableTranslateTransform(element);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(x, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
            translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(y, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
        }

        private static TranslateTransform GetMutableTranslateTransform(FrameworkElement element)
        {
            var translate = element.RenderTransform as TranslateTransform;
            if (translate == null)
            {
                translate = new TranslateTransform();
                element.RenderTransform = translate;
                return translate;
            }

            // Freezables declared in a Style setter are shared and frozen by WPF. They cannot be
            // animated directly, so every element must receive its own mutable clone first.
            if (translate.IsFrozen)
            {
                translate = (TranslateTransform)translate.CloneCurrentValue();
                element.RenderTransform = translate;
            }

            return translate;
        }

        public GridLength OverviewCompactSecondaryRowHeight
        {
            get => OverviewCompactSecondaryRow.Height;
            set => OverviewCompactSecondaryRow.Height = value;
        }

        public ColumnDefinition OverviewPrimaryColumnDefinition => OverviewPrimaryColumn;
        public ColumnDefinition OverviewGutterColumnDefinition => OverviewGutterColumn;
        public ColumnDefinition OverviewSecondaryColumnDefinition => OverviewSecondaryColumn;
        public UIElement OverviewPrimaryPanelElement => OverviewPrimaryPanel;
        public UIElement OverviewSecondaryPanelElement => OverviewSecondaryPanel;
        public Panel OverviewSecondaryScrollViewerElement => OverviewSecondaryScrollViewer;
        public Panel OverviewRiskScrollViewerElement => OverviewRiskScrollViewer;
        public Panel OverviewMetricPanelElement => OverviewMetricPanel;

        public void ApplyResponsiveColumns(bool stack)
        {
            OverviewPrimaryLayoutRow.Height = stack
                ? GridLength.Auto
                : new GridLength(1, GridUnitType.Star);
            // The top hero/metrics flow spans the complete workspace. The lower
            // activity/risk arrangement follows the UiLab's readable fixed rail:
            // flexible primary content plus a 330 DIP inspector column.
            OverviewPrimaryColumn.Width = new GridLength(1, GridUnitType.Star);
            OverviewGutterColumn.Width = new GridLength(stack ? 0 : 14);
            OverviewSecondaryColumn.Width = stack
                ? new GridLength(0)
                : new GridLength(330);
            OverviewFlowPrimaryColumn.Width = new GridLength(1, GridUnitType.Star);
            OverviewFlowGutterColumn.Width = new GridLength(stack ? 0 : 14);
            OverviewFlowSecondaryColumn.Width = stack
                ? new GridLength(0)
                : new GridLength(330);
            // The secondary column is a top-anchored inspector for the lower activity
            // row, not a vertically centered companion to the primary flow. Set this in
            // code as well as XAML because a Playnite host theme can replace inherited
            // alignment defaults during a live template refresh.
            OverviewSecondaryScrollViewer.VerticalAlignment = VerticalAlignment.Top;
            OverviewSecondaryPanel.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(OverviewPrimaryPanel, 0);
            Grid.SetColumn(OverviewPrimaryPanel, 0);
            Grid.SetColumnSpan(OverviewPrimaryPanel, 3);
            // In the wide layout the right rail starts beside recent tasks and spans
            // the recent-task/global-activity rows. In the compact layout it becomes
            // a single full-width row after the primary flow; keeping this in the same
            // measured grid prevents it from falling to an implicit/out-of-range row.
            Grid.SetRow(OverviewSecondaryScrollViewer, stack ? 4 : 2);
            Grid.SetColumn(OverviewSecondaryScrollViewer, stack ? 0 : 2);
            Grid.SetColumnSpan(OverviewSecondaryScrollViewer, stack ? 3 : 1);
            Grid.SetRowSpan(OverviewSecondaryScrollViewer, stack ? 1 : 2);
            OverviewSecondaryPanel.Margin = stack
                ? new Thickness(0, 14, 0, 0)
                : new Thickness(0);
        }

        public void ApplyResponsiveWidth(double width)
        {
            // The metric pills size to their content and wrap naturally, so a compact
            // window no longer needs to force fixed column counts that clip the values.
            // Use the measured primary column when available: the Overview page may have
            // a secondary risk column, so the full Dashboard width overstates the space
            // available to the workbench and would let the toolbar/current-game row clip.
            var primaryWidth = OverviewPrimaryPanel?.ActualWidth > 0
                ? OverviewPrimaryPanel.ActualWidth
                : Math.Max(320d, width);
            var activityWidth = OverviewFlowPrimaryColumn?.ActualWidth > 0
                ? OverviewFlowPrimaryColumn.ActualWidth
                : primaryWidth;

            // The six Snapshot metrics are a compact summary strip, not a card wall.
            // Wide workbenches keep the Demo's single-row rhythm; narrow hosts drop to
            // 3 then 2 columns so every real counter remains readable without wrapping
            // the whole page into a giant tile grid.
            if (OverviewStatStrip != null)
            {
                OverviewStatStrip.Columns = primaryWidth >= 1100 ? 6 : primaryWidth >= 620 ? 3 : 2;
            }

            if (OverviewActivityTimelineList != null)
            {
                var compactActivity = activityWidth < 900;
                OverviewActivityTimelineList.Tag = compactActivity ? "Compact" : "Wide";
            }

            // HomeView places TODAY and the selected-game context in a two-column row.
            // Keep that relationship whenever the primary workspace can support it; when
            // the real secondary risk column leaves less than 760 DIP, stack the two cards
            // as a single readable flow instead of compressing the context pills.
            if (OverviewHeroAndGameRow != null)
            {
                var stackHeroAndGame = primaryWidth < 700;
                OverviewHeroGameCompactRow.Height = stackHeroAndGame
                    ? GridLength.Auto
                    : new GridLength(0);
                OverviewHeroGameGutterColumn.Width = new GridLength(stackHeroAndGame ? 0 : 14);
                OverviewHeroColumn.Width = new GridLength(1.0, GridUnitType.Star);
                OverviewCurrentGameColumn.Width = new GridLength(1.0, GridUnitType.Star);

                Grid.SetRow(OverviewTodayHeroCard, 0);
                Grid.SetColumn(OverviewTodayHeroCard, 0);
                Grid.SetColumnSpan(OverviewTodayHeroCard, stackHeroAndGame ? 3 : 1);
                Grid.SetRow(OverviewCurrentGameCard, stackHeroAndGame ? 1 : 0);
                Grid.SetColumn(OverviewCurrentGameCard, stackHeroAndGame ? 0 : 2);
                Grid.SetColumnSpan(OverviewCurrentGameCard, stackHeroAndGame ? 3 : 1);
                OverviewCurrentGameCard.Margin = stackHeroAndGame
                    ? new Thickness(0, 14, 0, 0)
                    : new Thickness(0);

                // The hero owns a full-width title row and a full-width status row.
                // Keep the status row horizontal until the card is genuinely narrow;
                // this prevents status dots from being squeezed into an empty-looking
                // vertical strip at maximized 2K logical widths.
                var heroWidth = OverviewTodayHeroCard.ActualWidth > 0
                    ? OverviewTodayHeroCard.ActualWidth
                    : Math.Max(320d, primaryWidth * (stackHeroAndGame ? 1d : 0.5d));
                OverviewTodayHeroCard.Padding = heroWidth < 560
                    ? new Thickness(16, 16, 16, 14)
                    : new Thickness(22, 18, 22, 16);
            }
        }

        public void ApplyResponsiveHeight(double height, bool stack)
        {
            // The root page scroll surface owns cross-section overflow. The risk card
            // is intentionally bounded, and its long findings/protection lists own
            // only their local overflow so a large data set cannot grow the homepage
            // without limit or cover the controls below it.
            OverviewStackScrollSurface.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }
}
