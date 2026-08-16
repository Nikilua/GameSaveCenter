using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            Grid.SetRow(OverviewSecondaryScrollViewer, stack ? 5 : 3);
            Grid.SetColumn(OverviewSecondaryScrollViewer, stack ? 0 : 2);
            Grid.SetColumnSpan(OverviewSecondaryScrollViewer, stack ? 3 : 1);
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

            // The Demo keeps the Home workbench actions in the card header. At compact
            // widths move them to a second row, but keep a horizontal WrapPanel so the
            // four commands use the available width before wrapping. A forced vertical
            // stack consumed most of the workbench viewport on normal windowed laptops.
            if (OverviewHomeToolbarActions != null)
            {
                var stackActions = primaryWidth < 720;
                OverviewHomeToolbarActions.Orientation = Orientation.Horizontal;
                OverviewHomeToolbarActions.HorizontalAlignment = stackActions
                    ? HorizontalAlignment.Stretch
                    : HorizontalAlignment.Right;
                OverviewHomeToolbarActionsRow.Height = stackActions
                    ? GridLength.Auto
                    : new GridLength(0);
                Grid.SetRow(OverviewHomeToolbarActions, stackActions ? 1 : 0);
                Grid.SetColumn(OverviewHomeToolbarActions, stackActions ? 0 : 1);
                Grid.SetColumnSpan(OverviewHomeToolbarActions, stackActions ? 2 : 1);
                OverviewHomeToolbarActions.Margin = stackActions
                    ? new Thickness(0, 12, 0, 0)
                    : new Thickness(12, 0, 0, 0);
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
                // At the compact Playnite content width (~744 DIP) three 88-DIP
                // actions otherwise wrap inside the selected-game rail. UiLab keeps
                // that action row intact, so stack the two context cards slightly
                // earlier to preserve the same rhythm.
                if (primaryWidth >= 700 && primaryWidth < 800)
                    stackHeroAndGame = true;
                OverviewHeroGameCompactRow.Height = stackHeroAndGame
                    ? GridLength.Auto
                    : new GridLength(0);
                OverviewHeroGameGutterColumn.Width = new GridLength(stackHeroAndGame ? 0 : 14);
                OverviewHeroColumn.Width = new GridLength(1.0, GridUnitType.Star);
                if (!stackHeroAndGame)
                    OverviewHeroColumn.Width = new GridLength(1.35, GridUnitType.Star);
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
            // Visual Correction v2: the root page scroll surface is the only vertical
            // scroll owner in every layout. Primary/secondary columns and the risk card
            // grow naturally; none of them may create a competing wheel context.
            OverviewStackScrollSurface.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }
}
