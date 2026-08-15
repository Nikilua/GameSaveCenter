using System;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Views
{
    public partial class SaveCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private bool historyInspectorOpen;
        private bool candidateInspectorOpen;

        public SaveCenterView()
        {
            InitializeComponent();
            SaveHistoryActionsScrollViewer.IsVisibleChanged += InspectorIsVisibleChanged;
            SaveCandidateInspectorScrollViewer.IsVisibleChanged += InspectorIsVisibleChanged;
        }

        private void InspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
                return;

            ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
                responsiveWidth = width;
                responsiveHeight = height;
                // Keep the primary table readable when the selected-version inspector is
                // stacked below it.  The table still owns its internal virtualized scroll;
                // this floor only prevents the inspector's Auto row from reducing it to a
                // one-row strip during a short window resize.
                const double tableMinHeight = 236d;
                SaveHistoryGrid.MinHeight = tableMinHeight;
                SaveCandidateGrid.MinHeight = Math.Max(tableMinHeight, 252d);
                var compact = height < 760 || width < 1200;
                if (SaveCurrentRuleActions != null)
                {
                    SaveCurrentRuleActionsRow.Height = compact
                        ? GridLength.Auto
                        : new GridLength(0);
                    SaveCurrentRuleActionsColumn.Width = compact
                        ? new GridLength(0)
                        : GridLength.Auto;
                    Grid.SetRow(SaveCurrentRuleActions, compact ? 1 : 0);
                    Grid.SetColumn(SaveCurrentRuleActions, compact ? 0 : 3);
                    Grid.SetColumnSpan(SaveCurrentRuleActions, compact ? 4 : 1);
                    SaveCurrentRuleActions.Margin = compact
                        ? new Thickness(0, 12, 0, 0)
                        : new Thickness(14, 0, 0, 0);
                    SaveCurrentRuleActions.HorizontalAlignment = compact
                        ? HorizontalAlignment.Stretch
                        : HorizontalAlignment.Right;
                }
                var inspectorWidth = SaveHistoryLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);

                // On compact hosts the selected-version inspector is a drawer, not a
                // permanent second row. The table keeps the finite star row by default;
                // every action remains one click away through the compact details button.
                if (compact)
                {
                    var hasHistorySelection = SaveHistoryGrid.SelectedItem != null;
                    if (hasHistorySelection)
                    {
                        SaveHistoryActionsScrollViewer.Visibility = historyInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        SaveHistoryCompactDetailsButton.Content = historyInspectorOpen
                            ? "收起版本详情 ›"
                            : "查看版本详情 ›";
                        SaveHistoryCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SaveHistoryCompactDetailsButton.Visibility = Visibility.Collapsed;
                        historyInspectorOpen = false;
                    }

                    var hasCandidateSelection = SaveCandidateGrid.SelectedItem != null;
                    if (hasCandidateSelection)
                    {
                        SaveCandidateInspectorScrollViewer.Visibility = candidateInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        SaveCandidateCompactDetailsButton.Content = candidateInspectorOpen
                            ? "收起候选详情 ›"
                            : "查看候选详情 ›";
                        SaveCandidateCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SaveCandidateCompactDetailsButton.Visibility = Visibility.Collapsed;
                        candidateInspectorOpen = false;
                    }
                }
                else
                {
                    SaveHistoryCompactDetailsButton.Visibility = Visibility.Collapsed;
                    SaveCandidateCompactDetailsButton.Visibility = Visibility.Collapsed;
                    SaveHistoryActionsScrollViewer.Visibility = SaveHistoryGrid.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    SaveCandidateInspectorScrollViewer.Visibility = SaveCandidateGrid.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            // The demo keeps the history table and the selected-version inspector
            // side by side when there is room. On a compact host, stack the
            // inspector below the table so actions remain reachable without a
            // page-level scrollbar or clipped controls.
            var showHistoryInspector = SaveHistoryActionsScrollViewer.Visibility == Visibility.Visible;
            var historySideBySide = showHistoryInspector && !compact;
            SaveHistoryLayout.ColumnDefinitions[1].Width = historySideBySide ? new GridLength(14) : new GridLength(0);
            SaveHistoryLayout.ColumnDefinitions[2].Width = historySideBySide ? inspectorWidth : new GridLength(0);
            SaveHistoryLayout.RowDefinitions[1].Height = showHistoryInspector && compact ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SaveHistoryActionsScrollViewer, historySideBySide ? 2 : 0);
            Grid.SetColumnSpan(SaveHistoryActionsScrollViewer, compact ? 3 : 1);
            Grid.SetRow(SaveHistoryActionsScrollViewer, compact ? 1 : 0);
            SaveHistoryActionsScrollViewer.Margin = showHistoryInspector && compact ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            var historyHeight = SaveHistoryLayout.ActualHeight > 0 ? SaveHistoryLayout.ActualHeight : Math.Max(320, height - 200);
            var historyInspectorHeight = Math.Max(160, Math.Min(360, historyHeight - tableMinHeight - 10));
            SaveHistoryActionsScrollViewer.MaxHeight = showHistoryInspector && compact ? historyInspectorHeight : double.PositiveInfinity;
            var showCandidateInspector = SaveCandidateInspectorScrollViewer.Visibility == Visibility.Visible;
            var candidateSideBySide = showCandidateInspector && !compact;
            SaveCandidateLayout.ColumnDefinitions[1].Width = candidateSideBySide ? new GridLength(14) : new GridLength(0);
            SaveCandidateLayout.ColumnDefinitions[2].Width = candidateSideBySide ? inspectorWidth : new GridLength(0);
            SaveCandidateLayout.RowDefinitions[1].Height = showCandidateInspector && compact ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SaveCandidateInspectorScrollViewer, candidateSideBySide ? 2 : 0);
            Grid.SetColumnSpan(SaveCandidateInspectorScrollViewer, compact ? 3 : 1);
            Grid.SetRow(SaveCandidateInspectorScrollViewer, compact ? 1 : 0);
            SaveCandidateInspectorScrollViewer.Margin = showCandidateInspector && compact ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            var candidateHeight = SaveCandidateLayout.ActualHeight > 0 ? SaveCandidateLayout.ActualHeight : Math.Max(320, height - 200);
            var candidateInspectorHeight = Math.Max(160, Math.Min(360, candidateHeight - tableMinHeight - 10));
            SaveCandidateInspectorScrollViewer.MaxHeight = showCandidateInspector && compact ? candidateInspectorHeight : double.PositiveInfinity;
            var stackPolicy = width < 1080;
            // The policy page is a left-aligned form capped by the shared
            // GscFormMaxWidth token (1120). Give the StackPanel an explicit
            // viewport width so the reading cards fill the form instead of
            // collapsing to their content width, and so WPF never centers the
            // capped form inside the page scroll channel. The 4 is the right
            // padding of GscPageScrollViewer.
            SavePolicyStack.Width = Math.Max(0, Math.Min(width - 4, 1120));
            SavePolicyCardsLayout.ColumnDefinitions[1].Width = stackPolicy ? new GridLength(0) : new GridLength(14);
            SavePolicyCardsLayout.ColumnDefinitions[2].Width = stackPolicy ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.RowDefinitions[2].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SavePolicyMediaCard, stackPolicy ? 0 : 2);
            Grid.SetColumnSpan(SavePolicyMediaCard, stackPolicy ? 3 : 1);
            Grid.SetRow(SavePolicyMediaCard, stackPolicy ? 1 : 0);
            Grid.SetColumn(SavePolicySafetyCard, 0);
            Grid.SetColumnSpan(SavePolicySafetyCard, 3);
            Grid.SetRow(SavePolicySafetyCard, stackPolicy ? 2 : 1);
            SavePolicyMediaCard.Margin = stackPolicy ? new Thickness(0, 14, 0, 0) : new Thickness(0);

            var stackCompare = width < 1080 || height < 760;
            SaveCompareLayout.ColumnDefinitions[1].Width = stackCompare ? new GridLength(0) : new GridLength(14);
            SaveCompareLayout.ColumnDefinitions[2].Width = stackCompare ? new GridLength(0) : inspectorWidth;
            SaveCompareLayout.RowDefinitions[1].Height = stackCompare ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SaveCompareRetentionScrollViewer, stackCompare ? 0 : 2);
            Grid.SetColumnSpan(SaveCompareRetentionScrollViewer, stackCompare ? 3 : 1);
            Grid.SetRow(SaveCompareRetentionScrollViewer, stackCompare ? 1 : 0);
            SaveCompareRetentionScrollViewer.Margin = stackCompare ? new Thickness(0, 14, 0, 0) : new Thickness(0);
            SaveCompareRetentionScrollViewer.MaxHeight = stackCompare ? Math.Max(180, Math.Min(420, height * 0.42)) : double.PositiveInfinity;
            SaveCompareMainScrollViewer.MaxHeight = stackCompare ? Math.Max(220, Math.Min(420, height * 0.45)) : double.PositiveInfinity;
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private void OnSaveHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            historyInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnSaveCandidateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            candidateInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnSaveHistoryCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (SaveHistoryGrid.SelectedItem == null) return;
            historyInspectorOpen = !historyInspectorOpen;
            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, responsiveHeight > 0 ? responsiveHeight : ActualHeight);
        }

        private void OnSaveCandidateCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (SaveCandidateGrid.SelectedItem == null) return;
            candidateInspectorOpen = !candidateInspectorOpen;
            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, responsiveHeight > 0 ? responsiveHeight : ActualHeight);
        }
    }
}
