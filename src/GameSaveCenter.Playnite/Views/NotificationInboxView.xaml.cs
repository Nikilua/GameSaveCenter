using System;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>Global, bounded inbox for durable task and session notifications.</summary>
    public partial class NotificationInboxView : UserControl
    {
        private bool isApplyingResponsiveLayout;
        private bool? compactLayout;
        private bool compactInspectorOpen;

        public NotificationInboxView()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
            => ApplyResponsiveLayout(ActualWidth, ActualHeight);

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            => ApplyResponsiveLayout(e.NewSize.Width, e.NewSize.Height);

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingResponsiveLayout || double.IsNaN(width) || double.IsInfinity(width)) return;
            var useCompact = width < 980;
            if (compactLayout == useCompact) return;

            isApplyingResponsiveLayout = true;
            try
            {
                compactLayout = useCompact;
                if (useCompact)
                {
                    compactInspectorOpen = false;
                    NotificationListColumn.Width = new GridLength(1, GridUnitType.Star);
                    NotificationInspectorGap.Width = new GridLength(0);
                    NotificationInspectorColumn.Width = new GridLength(0);
                    NotificationFilterGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    NotificationFilterGrid.ColumnDefinitions[2].Width = new GridLength(0);
                    NotificationCompactInspectorRow.Height = new GridLength(0);
                    Grid.SetRowSpan(NotificationTableFrame, 1);
                    Grid.SetColumn(NotificationInspectorCard, 0);
                    Grid.SetColumnSpan(NotificationInspectorCard, 3);
                    Grid.SetRow(NotificationInspectorCard, 1);
                    Grid.SetRowSpan(NotificationInspectorCard, 1);
                    NotificationInspectorCard.Visibility = Visibility.Collapsed;
                    NotificationCompactDetailsButton.Visibility = Visibility.Visible;
                    NotificationCompactDetailsButton.Content = "查看详情";

                    NotificationInboxSearchHost.MinWidth = 0;
                    Grid.SetColumn(NotificationInboxSearchHost, 0);
                    Grid.SetColumnSpan(NotificationInboxSearchHost, 5);
                    Grid.SetRow(NotificationInboxSearchHost, 0);
                    NotificationSeverityFilterComboBox.MinWidth = 0;
                    NotificationTimeFilterComboBox.MinWidth = 0;
                    Grid.SetRow(NotificationSeverityFilterComboBox, 1);
                    Grid.SetColumn(NotificationSeverityFilterComboBox, 0);
                    Grid.SetRow(NotificationTimeFilterComboBox, 1);
                    Grid.SetColumn(NotificationTimeFilterComboBox, 1);
                    Grid.SetColumnSpan(NotificationTimeFilterComboBox, 1);
                    Grid.SetRow(NotificationInboxRefreshButton, 1);
                    Grid.SetColumn(NotificationInboxRefreshButton, 3);
                    Grid.SetRow(NotificationCompactDetailsButton, 1);
                    Grid.SetColumn(NotificationCompactDetailsButton, 4);
                    NotificationInboxSearchHost.Margin = new Thickness(0, 0, 0, 7);
                    NotificationTimeFilterComboBox.Margin = new Thickness(0, 0, 8, 0);
                }
                else
                {
                    NotificationListColumn.Width = new GridLength(1, GridUnitType.Star);
                    NotificationInspectorGap.Width = new GridLength(14);
                    NotificationInspectorColumn.Width = new GridLength(320);
                    NotificationFilterGrid.ColumnDefinitions[1].Width = new GridLength(170);
                    NotificationFilterGrid.ColumnDefinitions[2].Width = new GridLength(150);
                    NotificationCompactInspectorRow.Height = new GridLength(0);
                    Grid.SetRowSpan(NotificationTableFrame, 2);
                    NotificationInspectorCard.Visibility = Visibility.Visible;
                    Grid.SetColumn(NotificationInspectorCard, 2);
                    Grid.SetColumnSpan(NotificationInspectorCard, 1);
                    Grid.SetRow(NotificationInspectorCard, 0);
                    Grid.SetRowSpan(NotificationInspectorCard, 2);

                    NotificationInboxSearchHost.MinWidth = 160;
                    Grid.SetColumn(NotificationInboxSearchHost, 0);
                    Grid.SetColumnSpan(NotificationInboxSearchHost, 1);
                    Grid.SetRow(NotificationInboxSearchHost, 0);
                    NotificationSeverityFilterComboBox.MinWidth = 120;
                    NotificationTimeFilterComboBox.MinWidth = 120;
                    Grid.SetRow(NotificationSeverityFilterComboBox, 0);
                    Grid.SetColumn(NotificationSeverityFilterComboBox, 1);
                    Grid.SetRow(NotificationTimeFilterComboBox, 0);
                    Grid.SetColumn(NotificationTimeFilterComboBox, 2);
                    Grid.SetColumnSpan(NotificationTimeFilterComboBox, 1);
                    Grid.SetRow(NotificationInboxRefreshButton, 0);
                    Grid.SetColumn(NotificationInboxRefreshButton, 3);
                    NotificationCompactDetailsButton.Visibility = Visibility.Collapsed;
                    NotificationInboxSearchHost.Margin = new Thickness(0, 0, 10, 0);
                }
            }
            finally
            {
                isApplyingResponsiveLayout = false;
            }
        }

        private void OnCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (compactLayout != true) return;
            compactInspectorOpen = !compactInspectorOpen;
            NotificationInspectorCard.Visibility = compactInspectorOpen ? Visibility.Visible : Visibility.Collapsed;
            NotificationCompactInspectorRow.Height = compactInspectorOpen
                ? new GridLength(Math.Max(150, Math.Min(210, ActualHeight * 0.32)))
                : new GridLength(0);
            NotificationCompactDetailsButton.Content = compactInspectorOpen ? "收起详情" : "查看详情";
        }
    }
}
