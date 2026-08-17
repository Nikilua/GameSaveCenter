using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>
    /// Production-backed version of the AcrylicFork Design overview page.
    /// The page owns the Demo information architecture; the dashboard still owns
    /// navigation, global commands and the real ViewModel.
    /// </summary>
    public partial class OverviewView : UserControl
    {
        public bool UiAnimationsEnabled { get; set; } = true;

        public OverviewView() => InitializeComponent();

        private void OnStatCardMouseEnter(object sender, MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, -3, 160);

        private void OnStatCardMouseLeave(object sender, MouseEventArgs e)
            => AnimateTranslate(sender as FrameworkElement, 0, 0, 180);

        private void AnimateTranslate(FrameworkElement? element, double x, double y, int milliseconds)
        {
            if (element == null || !UiAnimationsEnabled || SystemParameters.HighContrast || !SystemParameters.ClientAreaAnimation) return;
            var transform = element.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }
            else if (transform.IsFrozen)
            {
                transform = (TranslateTransform)transform.CloneCurrentValue();
                element.RenderTransform = transform;
            }

            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(x, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
            transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(y, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = easing });
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

            OverviewPrimaryColumn.Width = new GridLength(1, GridUnitType.Star);
            OverviewGutterColumn.Width = new GridLength(stack ? 0 : 14);
            OverviewSecondaryColumn.Width = new GridLength(stack ? 0 : 330);

            OverviewBodyGutterColumn.Width = new GridLength(stack ? 0 : 14);
            OverviewBodySecondaryColumn.Width = new GridLength(stack ? 0 : 330);
            OverviewBodySecondaryRow.Height = stack ? GridLength.Auto : new GridLength(0);
            Grid.SetRow(OverviewSecondaryScrollViewer, stack ? 1 : 0);
            Grid.SetColumn(OverviewSecondaryScrollViewer, stack ? 0 : 2);
            Grid.SetColumnSpan(OverviewSecondaryScrollViewer, stack ? 3 : 1);
            OverviewSecondaryPanel.Margin = stack ? new Thickness(0, 14, 0, 0) : new Thickness(0);
        }

        public void ApplyResponsiveWidth(double width)
        {
            var primaryWidth = OverviewPrimaryPanel.ActualWidth > 0
                ? OverviewPrimaryPanel.ActualWidth
                : Math.Max(320d, width);

            OverviewStatStrip.Columns = primaryWidth >= 1120 ? 6 : primaryWidth >= 680 ? 3 : 2;

            var stackHeroAndGame = primaryWidth < 700;
            OverviewHeroGameCompactRow.Height = stackHeroAndGame ? GridLength.Auto : new GridLength(0);
            OverviewHeroGameGutterColumn.Width = new GridLength(stackHeroAndGame ? 0 : 14);
            OverviewHeroColumn.Width = new GridLength(1, GridUnitType.Star);
            OverviewCurrentGameColumn.Width = new GridLength(1, GridUnitType.Star);
            Grid.SetRow(OverviewTodayHeroCard, 0);
            Grid.SetColumn(OverviewTodayHeroCard, 0);
            Grid.SetColumnSpan(OverviewTodayHeroCard, stackHeroAndGame ? 3 : 1);
            Grid.SetRow(OverviewCurrentGameCard, stackHeroAndGame ? 1 : 0);
            Grid.SetColumn(OverviewCurrentGameCard, stackHeroAndGame ? 0 : 2);
            Grid.SetColumnSpan(OverviewCurrentGameCard, stackHeroAndGame ? 3 : 1);
            OverviewCurrentGameCard.Margin = stackHeroAndGame ? new Thickness(0, 14, 0, 0) : new Thickness(0);
            OverviewTodayHeroCard.Padding = primaryWidth < 560 ? new Thickness(16, 16, 16, 14) : new Thickness(22, 18, 22, 16);
        }

        public void ApplyResponsiveHeight(double height, bool stack)
        {
            OverviewStackScrollSurface.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }
}
