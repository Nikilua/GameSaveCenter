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
                // On narrow hosts the lock status is the essential per-row summary; the
                // note column can be read in full inside the version details inspector.
                // Hiding it keeps 状态 inside the viewport without enabling a horizontal bar.
                if (SaveHistoryNoteColumn != null)
                {
                    // The history table shares the normal desktop workspace with a
                    // 360-DIP inspector. At the 1116-DIP standard route the table
                    // itself is only about 739 DIP wide, so the compact column rhythm
                    // must begin before the full page crosses the compact breakpoint.
                    // This keeps the essential 状态 column inside the table without
                    // changing the project's DataGrid scrolling contract.
                    var narrowHistory = width < 1240;
                    SaveHistoryTimeColumn.Width = new DataGridLength(narrowHistory ? 96 : 150);
                    SaveHistoryTypeColumn.Width = new DataGridLength(narrowHistory ? 76 : 110);
                    SaveHistoryFileCountColumn.Width = new DataGridLength(narrowHistory ? 56 : 82);
                    SaveHistorySizeColumn.Width = new DataGridLength(narrowHistory ? 78 : 116);
                    SaveHistoryDeviceColumn.Width = new DataGridLength(narrowHistory ? 110 : 120);
                    SaveHistoryStateColumn.Width = new DataGridLength(narrowHistory ? 76 : 96);
                    SaveHistoryNoteColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    SaveHistoryNoteColumn.MinWidth = narrowHistory ? 116 : 180;
                }
                // Demo keeps the inspector beside the main surface at the normal
                // 1040x700 breakpoint.  The previous 1200 DIP cutoff pushed the
                // Playnite host into the drawer layout even when there was enough
                // room, which made the production page structurally different.
                var compact = width < 980;
                var ruleCardCompact = width < 700;
                if (SaveCurrentRuleActions != null)
                {
                    SaveCurrentRuleActionsRow.Height = ruleCardCompact
                        ? GridLength.Auto
                        : new GridLength(0);
                    SaveCurrentRuleActionsColumn.Width = ruleCardCompact
                        ? new GridLength(0)
                        : GridLength.Auto;
                    Grid.SetRow(SaveCurrentRuleActions, ruleCardCompact ? 1 : 0);
                    Grid.SetColumn(SaveCurrentRuleActions, ruleCardCompact ? 0 : 3);
                    Grid.SetColumnSpan(SaveCurrentRuleActions, ruleCardCompact ? 4 : 1);
                    SaveCurrentRuleActions.Margin = ruleCardCompact
                        ? new Thickness(0, 12, 0, 0)
                        : new Thickness(14, 0, 0, 0);
                    SaveCurrentRuleActions.HorizontalAlignment = ruleCardCompact
                        ? HorizontalAlignment.Stretch
                        : HorizontalAlignment.Right;
                }
                if (SaveHistorySummaryActions != null)
                {
                    SaveHistorySummaryActionsRow.Height = ruleCardCompact
                        ? GridLength.Auto
                        : new GridLength(0);
                    SaveHistorySummaryActionsColumn.Width = ruleCardCompact
                        ? new GridLength(0)
                        : GridLength.Auto;
                    Grid.SetRow(SaveHistorySummaryActions, ruleCardCompact ? 1 : 0);
                    Grid.SetColumn(SaveHistorySummaryActions, ruleCardCompact ? 0 : 1);
                    Grid.SetColumnSpan(SaveHistorySummaryActions, ruleCardCompact ? 2 : 1);
                    SaveHistorySummaryActions.Margin = ruleCardCompact
                        ? new Thickness(0, 10, 0, 0)
                        : new Thickness(14, 0, 0, 0);
                    SaveHistorySummaryActions.HorizontalAlignment = ruleCardCompact
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
            var stackPolicy = width < 980;
            // Keep the three Demo cards in one row when there is room. Below
            // the compact breakpoint, stack the cards into explicit rows so
            // the template controls never share measure space with the list.
            SavePolicyStack.Width = Math.Max(0, width - 4);
            SavePolicyCardsLayout.ColumnDefinitions[1].Width = stackPolicy ? new GridLength(0) : new GridLength(14);
            SavePolicyCardsLayout.ColumnDefinitions[2].Width = stackPolicy ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.ColumnDefinitions[3].Width = stackPolicy ? new GridLength(0) : new GridLength(14);
            SavePolicyCardsLayout.ColumnDefinitions[4].Width = stackPolicy ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.RowDefinitions[0].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.RowDefinitions[1].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            SavePolicyCardsLayout.RowDefinitions[2].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            SavePolicyCardsLayout.RowDefinitions[3].Height = new GridLength(0);

            Grid.SetColumn(SaveBackupAutomationCard, 0);
            Grid.SetColumnSpan(SaveBackupAutomationCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SaveBackupAutomationCard, 0);

            Grid.SetColumn(SavePolicyMediaCard, stackPolicy ? 0 : 2);
            Grid.SetColumnSpan(SavePolicyMediaCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SavePolicyMediaCard, stackPolicy ? 1 : 0);
            SavePolicyMediaCard.Margin = stackPolicy ? new Thickness(0, 14, 0, 0) : new Thickness(0);

            Grid.SetColumn(SavePolicyTemplatesCard, stackPolicy ? 0 : 4);
            Grid.SetColumnSpan(SavePolicyTemplatesCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SavePolicyTemplatesCard, stackPolicy ? 2 : 0);
            SavePolicyTemplatesCard.Margin = stackPolicy ? new Thickness(0, 14, 0, 0) : new Thickness(0);


            // The Demo keeps comparison and retention as two peer cards inside one
            // horizontally scrollable canvas. Do not stack the cards into the same
            // narrow vertical flow: that was the source of the production page's
            // large-height/empty-column mismatch at small widths.
            SaveCompareLayout.ColumnDefinitions[1].Width = new GridLength(14);
            SaveCompareLayout.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            SaveComparePageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            SaveComparePageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            SaveCompareRetentionScrollViewer.Margin = new Thickness(0, 10, 0, 0);
            SaveCompareRetentionScrollViewer.MaxHeight = double.PositiveInfinity;
            SaveCompareMainScrollViewer.MaxHeight = double.PositiveInfinity;
            SaveCompareMainScrollViewer.MinHeight = 0;
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
