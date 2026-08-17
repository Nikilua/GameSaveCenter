using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    public partial class TrainerCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private bool trainerInspectorOpen;

        public TrainerCenterView()
        {
            InitializeComponent();
            TrainerToolsSettingsScrollViewer.IsVisibleChanged += OnTrainerInspectorIsVisibleChanged;
        }

        private void OnTrainerInspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
                return;

            ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnTrainerCatalogSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || DataContext is not DashboardViewModel viewModel)
                return;

            if (viewModel.LoadTrainerReleasesCommand.CanExecute(null))
                viewModel.LoadTrainerReleasesCommand.Execute(null);
        }

        private void OnToolDragOver(object sender, DragEventArgs e)
        {
            e.Effects = CanDropTool(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnToolDrop(object sender, DragEventArgs e)
        {
            if (!CanDropTool(e.Data) || DataContext is not DashboardViewModel viewModel)
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            var path = files[0];
            viewModel.ImportDroppedGameTool(path);
            e.Handled = true;
        }

        private static bool CanDropTool(IDataObject data)
        {
            if (!data.GetDataPresent(DataFormats.FileDrop)) return false;
            var files = data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length != 1) return false;
            var path = files[0];
            return File.Exists(path) || Directory.Exists(path);
        }

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
                responsiveWidth = width;
                responsiveHeight = height;
                // A stacked inspector must not consume the installed-tool list's entire
                // star row.  Keep roughly four readable rows in the virtualized list and
                // give the inspector the remaining finite budget for its own scroll bar.
                const double tableMinHeight = 236d;
                TrainerToolsTable.MinHeight = tableMinHeight;
                TrainerToolsList.MinHeight = tableMinHeight;
                TrainerCatalogResultsPanel.MinHeight = tableMinHeight;
                TrainerCatalogReleasesPanel.MinHeight = tableMinHeight;
                InstalledToolsLayout.HorizontalAlignment = HorizontalAlignment.Stretch;
                InstalledToolsLayout.VerticalAlignment = VerticalAlignment.Stretch;
                TrainerReleasesLayout.HorizontalAlignment = HorizontalAlignment.Stretch;
                TrainerReleasesLayout.VerticalAlignment = VerticalAlignment.Stretch;
                var inspectorWidth = InstalledToolsLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
                // Keep the demo-aligned 560–680 DIP search rhythm on desktop, but let the
                // input shrink before the host reaches the Narrow fallback.  The form is
                // presentation-only; the existing binding and search command are untouched.
                var searchWidth = Math.Max(260, Math.Min(680, width - (width < 960 ? 200 : 320)));
                TrainerSearchTextBox.Width = searchWidth;
                TrainerSearchTextBox.MinWidth = 0;
                var importWidth = Math.Max(240, Math.Min(520, width - 360));
                TrainerImportEntryComboBox.Width = importWidth;
                TrainerImportEntryComboBox.MinWidth = 0;
                var stackInstalled = width < 1080;

                // On compact hosts the selected-tool editor is a drawer, not a permanent
                // second row. The virtualized tool list keeps the star row by default;
                // every edit action remains one click away through the compact button.
                if (stackInstalled)
                {
                    var hasToolSelection = TrainerToolsList.SelectedItem != null;
                    if (hasToolSelection)
                    {
                        TrainerToolsSettingsScrollViewer.Visibility = trainerInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        TrainerToolsCompactDetailsButton.Content = trainerInspectorOpen
                            ? "收起工具详情 ›"
                            : "查看工具详情 ›";
                        TrainerToolsCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TrainerToolsCompactDetailsButton.Visibility = Visibility.Collapsed;
                        trainerInspectorOpen = false;
                    }
                }
                else
                {
                    TrainerToolsCompactDetailsButton.Visibility = Visibility.Collapsed;
                    TrainerToolsSettingsScrollViewer.Visibility = TrainerToolsList.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            // The inspector is a full-height secondary pane in the normal two-column layout.
            // Only the stacked compact layout receives a finite scroll channel; otherwise the
            // card would collapse into a short block and leave a large unused area beside it.
            var showInspector = TrainerToolsSettingsScrollViewer.Visibility == Visibility.Visible;
            TrainerToolsSettingsScrollViewer.MaxHeight = double.PositiveInfinity;
            InstalledToolsLayout.ColumnDefinitions[1].Width = stackInstalled
                ? new GridLength(0)
                : showInspector ? new GridLength(14) : new GridLength(0);
            InstalledToolsLayout.ColumnDefinitions[2].Width = !stackInstalled && showInspector
                ? inspectorWidth
                : new GridLength(0);
            InstalledToolsLayout.RowDefinitions[3].Height = showInspector && stackInstalled
                ? GridLength.Auto
                : new GridLength(0);
            Grid.SetColumn(TrainerToolsSettingsScrollViewer, stackInstalled ? 0 : 2);
            Grid.SetRow(TrainerToolsSettingsScrollViewer, stackInstalled ? 3 : 0);
            Grid.SetRowSpan(TrainerToolsSettingsScrollViewer, stackInstalled ? 1 : 4);
            var installedHeight = InstalledToolsLayout.ActualHeight > 0 ? InstalledToolsLayout.ActualHeight : Math.Max(320, height - 200);
            var installedInspectorHeight = Math.Max(160, Math.Min(420, installedHeight - tableMinHeight - 72));
            TrainerToolsSettingsScrollViewer.MaxHeight = showInspector && stackInstalled
                ? installedInspectorHeight
                : double.PositiveInfinity;
            TrainerToolsSettingsScrollViewer.Margin = stackInstalled
                ? showInspector ? new Thickness(0, 10, 0, 0) : new Thickness(0)
                : new Thickness(0);
            var stackReleases = width < 1080;
            TrainerReleasesLayout.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            TrainerReleasesLayout.RowDefinitions[1].Height = stackReleases
                ? GridLength.Auto
                : new GridLength(0);
            TrainerReleasesLayout.ColumnDefinitions[1].Width = stackReleases
                ? new GridLength(0)
                : new GridLength(14);
            TrainerReleasesLayout.ColumnDefinitions[2].Width = stackReleases
                ? new GridLength(0)
                : inspectorWidth;
            Grid.SetRow(TrainerCatalogReleasesPanel, 0);
            Grid.SetColumn(TrainerCatalogReleasesPanel, 0);
            Grid.SetColumnSpan(TrainerCatalogReleasesPanel, stackReleases ? 3 : 1);
            Grid.SetRow(TrainerReleaseInfoPanel, stackReleases ? 1 : 0);
            Grid.SetColumn(TrainerReleaseInfoPanel, stackReleases ? 0 : 2);
            Grid.SetColumnSpan(TrainerReleaseInfoPanel, stackReleases ? 3 : 1);
            TrainerCatalogReleasesPanel.Margin = new Thickness(0);
            var releasesHeight = TrainerReleasesLayout.ActualHeight > 0 ? TrainerReleasesLayout.ActualHeight : Math.Max(320, height - 200);
            var releaseInspectorHeight = Math.Max(160, Math.Min(420, releasesHeight - tableMinHeight - 10));
            TrainerReleaseInfoScrollViewer.MaxHeight = stackReleases
                ? releaseInspectorHeight
                : double.PositiveInfinity;
            TrainerReleaseInfoPanel.Margin = stackReleases
                ? new Thickness(0, 10, 0, 0)
                : new Thickness(0);
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private void OnTrainerToolsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            trainerInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnTrainerToolsCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (TrainerToolsList.SelectedItem == null) return;
            trainerInspectorOpen = !trainerInspectorOpen;
            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, responsiveHeight > 0 ? responsiveHeight : ActualHeight);
        }
    }
}
