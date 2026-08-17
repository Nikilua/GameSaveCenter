using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>
    /// Production overview using the AcrylicFork Demo's single-flow geometry. The data
    /// remains bound to DashboardViewModel; this class only changes measurement and motion.
    /// </summary>
    public partial class OverviewView : UserControl
    {
        public bool UiAnimationsEnabled { get; set; } = true;

        public OverviewView() => InitializeComponent();

        // Kept as a compatibility property for the Dashboard responsive coordinator. The
        // migrated page no longer has a detached secondary row to resize.
        public GridLength OverviewCompactSecondaryRowHeight
        {
            get => new GridLength(0);
            set { }
        }

        public void ApplyResponsiveColumns(bool stack)
        {
            OverviewHeroGameCompactRow.Height = stack ? GridLength.Auto : new GridLength(0);
            OverviewHeroGameGutterColumn.Width = new GridLength(stack ? 0 : 14);
            OverviewHeroColumn.Width = new GridLength(1.35, GridUnitType.Star);
            OverviewCurrentGameColumn.Width = new GridLength(1, GridUnitType.Star);

            Grid.SetRow(OverviewTodayHeroCard, 0);
            Grid.SetColumn(OverviewTodayHeroCard, 0);
            Grid.SetColumnSpan(OverviewTodayHeroCard, stack ? 3 : 1);
            Grid.SetRow(OverviewCurrentGameCard, stack ? 1 : 0);
            Grid.SetColumn(OverviewCurrentGameCard, stack ? 0 : 2);
            Grid.SetColumnSpan(OverviewCurrentGameCard, stack ? 3 : 1);
            OverviewCurrentGameCard.Margin = stack
                ? new Thickness(0, 14, 0, 0)
                : new Thickness(0);

            if (stack)
                OverviewStatStrip.LayoutTransform = new ScaleTransform(1, 1);
        }

        public void ApplyResponsiveWidth(double width)
        {
            // The grid has dedicated separator columns, so every metric keeps the same
            // available width. At compact widths the stat strip remains a readable 3x2
            // matrix rather than allowing text to compress into its neighbours.
            var compact = width < 620;
            var veryCompact = width < 430;
            var primaryWidth = Math.Max(320d, width);
            OverviewStatStrip.Columns = primaryWidth >= 1100 ? 6 : primaryWidth >= 620 ? 3 : 2;
            if (compact)
            {
                OverviewStatStrip.LayoutTransform = new ScaleTransform(1, 1);
                OverviewStatStrip.Margin = new Thickness(0, 0, 0, 0);
            }

            if (veryCompact)
            {
                OverviewTodayHeroCard.Padding = new Thickness(16);
                OverviewCurrentGameCard.Padding = new Thickness(14);
            }
            else
            {
                OverviewTodayHeroCard.Padding = new Thickness(22, 20, 22, 20);
                OverviewCurrentGameCard.Padding = new Thickness(18, 16, 18, 16);
            }

            var compactActivity = width < 720;
            OverviewActivityTimelineList.Tag = compactActivity ? "Compact" : "Wide";
            OverviewActivityHeaderRow.Visibility = compactActivity ? Visibility.Collapsed : Visibility.Visible;
        }

        public void ApplyResponsiveHeight(double height, bool stack)
        {
            // The root page is the only vertical scroll owner. Inner task/activity lists
            // keep virtualization but do not create a second wheel viewport.
            OverviewStackScrollSurface.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void OnStatCardMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, -2, 150);

        private void OnStatCardMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, 0, 180);

        private void AnimateTranslate(FrameworkElement? element, double x, double y, int milliseconds)
        {
            if (element == null || !UiAnimationsEnabled || SystemParameters.HighContrast || !SystemParameters.ClientAreaAnimation)
                return;
            var translate = element.RenderTransform as TranslateTransform;
            if (translate == null)
            {
                translate = new TranslateTransform();
                element.RenderTransform = translate;
            }
            else if (translate.IsFrozen)
            {
                translate = (TranslateTransform)translate.CloneCurrentValue();
                element.RenderTransform = translate;
            }
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(x, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
            translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(y, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
        }
    }
}
