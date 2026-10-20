using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R23ProductionResourcesWpf")]
public sealed class RetentionScenarioInputBehaviorTests
{
    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void NamedDraftControlsStaySeparateFromEffectivePolicyAndShowInvalidFieldErrors(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                var draft = new RetentionScenarioDraft();
                var effectivePolicy = new RetentionPolicy { KeepDailyDays = 14 };
                var context = new ScenarioContext(draft, effectivePolicy);
                var page = new MaintenanceView { DataContext = context };
                var tabs = Field<TabControl>(page, "MaintenanceTabControl");
                var retentionTab = Assert.Single(
                    System.Linq.Enumerable.Cast<TabItem>(tabs.Items),
                    item => Equals(item.Header, "保留策略"));
                tabs.SelectedItem = retentionTab;

                var window = new Window
                {
                    Content = page,
                    Width = 1040,
                    Height = 900,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 0.01
                };

                try
                {
                    ApplyTheme(page, theme);
                    window.Show();
                    page.ApplyResponsiveLayout(1040, 900);
                    PumpLayout(window);

                    var card = Field<Border>(page, "MaintenanceRetentionScenarioCard");
                    Assert.True(card.IsVisible && card.ActualWidth > 700, "The scenario editor must be part of the visible retention page.");
                    var scenarioSelector = Field<ComboBox>(page, "RetentionScenarioDraftSelector");
                    Assert.Same(context.RetentionScenarioDrafts, scenarioSelector.ItemsSource);
                    Assert.Same(context.SelectedRetentionScenarioDraft, scenarioSelector.SelectedItem);
                    var scenarioName = Field<TextBox>(page, "RetentionScenarioNameInput");
                    Assert.Equal(context.RetentionScenarioNameDraft, scenarioName.Text);
                    Assert.Equal(context.RetentionScenarioDraftStatusDisplay, Field<TextBlock>(page, "RetentionScenarioDraftStatus").Text);
                    AssertScenarioCommand(page, "SaveRetentionScenarioDraftButton", context.SaveRetentionScenarioDraftCommand);
                    AssertScenarioCommand(page, "CopyRetentionScenarioDraftButton", context.CopyRetentionScenarioDraftCommand);
                    AssertScenarioCommand(page, "RenameRetentionScenarioDraftButton", context.RenameRetentionScenarioDraftCommand);
                    AssertScenarioCommand(page, "DeleteRetentionScenarioDraftButton", context.DeleteRetentionScenarioDraftCommand);
                    var handoffButton = Assert.IsAssignableFrom<ButtonBase>(page.GetType().GetField("HandoffRetentionScenarioToPolicyTemplateButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
                    Assert.Same(context.HandoffRetentionScenarioToPolicyTemplateCommand, handoffButton.Command);
                    Assert.True(handoffButton.IsVisible);
                    Assert.Contains("不保存、不应用", handoffButton.ToolTip?.ToString());
                    var pageScrollViewer = FindVisualAncestor<ScrollViewer>(card);
                    Assert.NotNull(pageScrollViewer);
                    Assert.True(pageScrollViewer!.ViewportHeight > 0);
                    Assert.True(pageScrollViewer.ScrollableHeight > 0, "The page scroll must expose content below the initial viewport.");
                    Assert.Equal(3, Field<UniformGrid>(page, "MaintenanceRetentionScenarioInputs").Columns);

                    var comparisonCard = Field<Border>(page, "MaintenanceRetentionScenarioComparisonCard");
                    Assert.True(comparisonCard.IsVisible && comparisonCard.ActualWidth > 700);
                    Assert.Same(pageScrollViewer, FindVisualAncestor<ScrollViewer>(comparisonCard));
                    var compareButton = Assert.IsAssignableFrom<ButtonBase>(page.GetType().GetField("RetentionScenarioCompareButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
                    Assert.Same(context.CompareRetentionScenarioCommand, compareButton.Command);
                    Assert.True(compareButton.Command.CanExecute(null));
                    Assert.Equal(context.RetentionScenarioComparison.Summary, Field<TextBlock>(page, "RetentionScenarioComparisonSummary").Text);
                    Assert.Equal(context.RetentionScenarioComparison.GrowthAssumptionDisplay, Field<TextBlock>(page, "RetentionScenarioGrowthAssumptionDisplay").Text);
                    Assert.Equal(context.RetentionScenarioComparison.DetailSummaryDisplay, Field<TextBlock>(page, "RetentionScenarioDetailSummary").Text);
                    var gameGrid = Field<DataGrid>(page, "RetentionScenarioGameGrid");
                    Assert.True(gameGrid.IsVisible);
                    Assert.Equal(context.RetentionScenarioComparison.Games.Count, gameGrid.Items.Count);
                    Assert.Same(context.SelectedRetentionScenarioGame, gameGrid.SelectedItem);
                    Assert.True(VirtualizingPanel.GetIsVirtualizing(gameGrid));
                    Assert.Equal(250, gameGrid.MaxHeight);
                    var versionGrid = Field<DataGrid>(page, "RetentionScenarioVersionGrid");
                    Assert.True(versionGrid.IsVisible);
                    Assert.Same(context.SelectedRetentionScenarioGame!.VersionDetails, versionGrid.ItemsSource);
                    Assert.Equal(context.SelectedRetentionScenarioGame.VersionDetails.Count, versionGrid.Items.Count);
                    Assert.True(VirtualizingPanel.GetIsVirtualizing(versionGrid));
                    Assert.Equal(300, versionGrid.MaxHeight);
                    var versionGridOffset = versionGrid.TransformToAncestor(pageScrollViewer).Transform(new Point(0, 0)).Y;
                    pageScrollViewer.ScrollToVerticalOffset(pageScrollViewer.VerticalOffset + versionGridOffset);
                    PumpLayout(window);
                    var detailViewport = FindVisualDescendant<ScrollViewer>(versionGrid);
                    Assert.NotNull(detailViewport);
                    Assert.True(detailViewport!.ScrollableHeight > 0, "Version details keep their own bounded virtualized scroll viewport.");
                    var detailBounds = versionGrid.TransformToAncestor(window).TransformBounds(new Rect(new Point(0, 0), versionGrid.RenderSize));
                    Assert.True(Rect.Intersect(detailBounds, new Rect(0, 0, window.ActualWidth, window.ActualHeight)).Height > 0,
                        "The page scroll must expose the version detail grid within the host window.");
                    Assert.Equal(context.StorageAnalysis.VolumeFreeDisplay, Field<TextBlock>(page, "MaintenanceStorageLocalVolume").Text);
                    Assert.Equal(context.StorageAnalysis.HistoricalGrowthRangeDisplay, Field<TextBlock>(page, "MaintenanceStorageGrowthRangeSummary").Text);
                    Assert.Equal(context.StorageAnalysis.HistoricalGrowthRangeDisplay, Field<TextBlock>(page, "MaintenanceStorageDemoGrowthRangeSummary").Text);
                    Assert.Equal(context.StorageAnalysis.RemoteCapacitySummary, Field<TextBlock>(page, "MaintenanceStorageRemoteCapacitySummary").Text);
                    Assert.Equal(context.StorageAnalysis.PredictionSummary, Field<TextBlock>(page, "MaintenanceStoragePredictionSummary").Text);
                    Assert.Equal(context.RetentionScenarioInput.GrowthAssumptionDisplay, Field<TextBlock>(page, "MaintenanceStorageUserGrowthAssumption").Text);
                    var rangeBars = Field<UniformGrid>(page, "MaintenanceStorageGrowthRangeBars");
                    Assert.True(context.StorageAnalysis.HasHistoricalGrowthRange, "The controlled UI sample must expose an observed range.");
                    Assert.Equal(Visibility.Visible, rangeBars.Visibility);
                    Assert.Equal((double)context.StorageAnalysis.HistoricalGrowthMinimumBytesPer30Days!.Value, Field<ProgressBar>(page, "MaintenanceStorageGrowthMinimumBar").Value);
                    Assert.Equal((double)context.StorageAnalysis.HistoricalGrowthMaximumBytesPer30Days!.Value, Field<ProgressBar>(page, "MaintenanceStorageGrowthMaximumBar").Value);

                    var minimum = Field<TextBox>(page, "RetentionScenarioGrowthMinimumInput");
                    var maximum = Field<TextBox>(page, "RetentionScenarioGrowthMaximumInput");
                    SetAndCommit(minimum, "500");
                    SetAndCommit(maximum, "200");
                    PumpLayout(window);

                    Assert.True(Validation.GetHasError(minimum));
                    Assert.True(Validation.GetHasError(maximum));
                    Assert.Contains("不能", Field<TextBlock>(page, "RetentionScenarioGrowthMaximumError").Text);

                    SetAndCommit(maximum, "750");
                    var daily = Field<TextBox>(page, "RetentionScenarioKeepDailyDaysInput");
                    SetAndCommit(daily, "42");
                    PumpLayout(window);

                    Assert.False(Validation.GetHasError(minimum));
                    Assert.False(Validation.GetHasError(maximum));
                    Assert.Equal(500, draft.GrowthMinimumMiBPer30Days);
                    Assert.Equal(750, draft.GrowthMaximumMiBPer30Days);
                    Assert.Equal(42, draft.CandidatePolicy.KeepDailyDays);
                    Assert.Equal(14, effectivePolicy.KeepDailyDays);
                    Assert.True(draft.IsValid);
                    Assert.Equal(draft.GrowthAssumptionDisplay, Field<TextBlock>(page, "MaintenanceStorageUserGrowthAssumption").Text);

                    SetAndCommit(daily, "-1");
                    PumpLayout(window);
                    Assert.True(Validation.GetHasError(daily));
                    Assert.Contains("0–36500", Field<TextBlock>(page, "RetentionScenarioKeepDailyDaysError").Text);
                    Assert.Equal(42, draft.CandidatePolicy.KeepDailyDays);

                    page.ApplyResponsiveLayout(600, 900);
                    PumpLayout(window);
                    var comparisonLayout = Field<Grid>(page, "MaintenanceRetentionScenarioComparisonLayout");
                    Assert.Equal(0, comparisonLayout.ColumnDefinitions[2].Width.Value);
                    Assert.Equal(1, Grid.GetRow(comparisonLayout.Children[1]));
                }
                finally
                {
                    Keyboard.ClearFocus();
                    window.Close();
                    window.Content = null;
                    window.Dispatcher.InvokeShutdown();
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static void SetAndCommit(TextBox input, string value)
    {
        input.Text = value;
        BindingOperations.GetBindingExpression(input, TextBox.TextProperty)?.UpdateSource();
        input.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        var frame = new DispatcherFrame();
        window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        window.UpdateLayout();
    }

    private static T Field<T>(object owner, string name)
        where T : class
        => Assert.IsType<T>(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner));

    private static void AssertScenarioCommand(MaintenanceView page, string fieldName, ICommand command)
    {
        var button = Assert.IsAssignableFrom<ButtonBase>(page.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
        Assert.Same(command, button.Command);
        Assert.True(button.IsVisible);
    }

    private static T? FindVisualAncestor<T>(DependencyObject child)
        where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(child); current != null; current = VisualTreeHelper.GetParent(current))
            if (current is T match)
                return match;
        return null;
    }

    private static T? FindVisualDescendant<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                return match;
            var nested = FindVisualDescendant<T>(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private sealed class ScenarioContext
    {
        public ScenarioContext(RetentionScenarioDraft input, RetentionPolicy effectivePolicy)
        {
            RetentionScenarioInput = input;
            EffectivePolicy = effectivePolicy;
            var savedScenario = new RetentionScenarioDefinition
            {
                Id = "11111111111111111111111111111111",
                Name = "稳妥保留",
                KeepAllForHours = input.KeepAllForHours,
                KeepDailyDays = input.KeepDailyDays,
                KeepWeeklyWeeks = input.KeepWeeklyWeeks,
                KeepMonthlyMonths = input.KeepMonthlyMonths,
                GrowthMinimumMiBPer30Days = input.GrowthMinimumMiBPer30Days,
                GrowthMaximumMiBPer30Days = input.GrowthMaximumMiBPer30Days,
                LastSnapshotUtc = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc)
            };
            RetentionScenarioDrafts.Add(savedScenario);
            SelectedRetentionScenarioDraft = savedScenario;
            RetentionScenarioNameDraft = savedScenario.Name;
            RetentionScenarioComparison = new RetentionScenarioComparisonDto
            {
                Summary = "测试对比：同一基线 25 个版本。",
                BaselineVersionCount = 25,
                GeneratedUtc = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc),
                GrowthMinimumMiBPer30Days = 500,
                GrowthMaximumMiBPer30Days = 750,
                Current = new RetentionScenarioPlanSummaryDto { ExistingVersionCount = 25, KeepVersionCount = 23, ProtectedVersionCount = 20, DeleteCandidateCount = 2 },
                Candidate = new RetentionScenarioPlanSummaryDto { ExistingVersionCount = 25, KeepVersionCount = 24, ProtectedVersionCount = 20, DeleteCandidateCount = 1 },
                Games =
                {
                    new RetentionScenarioGameSummaryDto
                    {
                        PlayniteId = "game-1",
                        GameName = "测试游戏",
                        ExistingVersionCount = 25,
                        CurrentKeepVersionCount = 23,
                        CurrentProtectedVersionCount = 20,
                        CurrentDeleteCandidateCount = 2,
                        CandidateKeepVersionCount = 24,
                        CandidateProtectedVersionCount = 20,
                        CandidateDeleteCandidateCount = 1,
                        ChangedVersionCount = 1,
                        VersionDetails =
                        {
                            new RetentionScenarioVersionDetailDto
                            {
                                PlayniteId = "game-1",
                                GameName = "测试游戏",
                                BackupId = "backup-1",
                                CreatedUtc = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
                                CurrentDispositionDisplay = "策略淘汰候选",
                                CandidateDispositionDisplay = "策略保留",
                                ProtectionReasonDisplay = "无额外保护标记",
                                ArchiveAvailabilityDisplay = "归档存在（只读检查；执行时仍会复核）"
                            }
                        }
                    }
                }
            };
            for (var index = 2; index <= 21; index++)
            {
                RetentionScenarioComparison.Games[0].VersionDetails.Add(new RetentionScenarioVersionDetailDto
                {
                    PlayniteId = "game-1",
                    GameName = "测试游戏",
                    BackupId = $"locked-backup-{index:D2}",
                    CreatedUtc = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc).AddMinutes(-index),
                    IsProtected = true,
                    CurrentDispositionDisplay = "保护保留",
                    CandidateDispositionDisplay = "保护保留",
                    ProtectionReasonDisplay = "用户锁定",
                    ArchiveAvailabilityDisplay = "不适用：保护项不进入淘汰候选"
                });
            }
            SelectedRetentionScenarioGame = RetentionScenarioComparison.Games[0];
        }

        public RetentionScenarioDraft RetentionScenarioInput { get; }
        public ObservableCollection<RetentionScenarioDefinition> RetentionScenarioDrafts { get; } = new ObservableCollection<RetentionScenarioDefinition>();
        public RetentionScenarioDefinition? SelectedRetentionScenarioDraft { get; set; }
        public string RetentionScenarioNameDraft { get; set; } = string.Empty;
        public string RetentionScenarioDraftStatusDisplay { get; } = "已保存方案 · 可重新比较";
        public ICommand SaveRetentionScenarioDraftCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public ICommand CopyRetentionScenarioDraftCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public ICommand RenameRetentionScenarioDraftCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public ICommand DeleteRetentionScenarioDraftCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public ICommand HandoffRetentionScenarioToPolicyTemplateCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public RetentionPolicy EffectivePolicy { get; }
        public RetentionScenarioComparisonDto RetentionScenarioComparison { get; }
        public RetentionScenarioGameSummaryDto? SelectedRetentionScenarioGame { get; set; }
        public StorageAnalysisDto StorageAnalysis { get; } = new StorageAnalysisDto
        {
            VolumeTotalBytes = 1024L * 1024 * 1024 * 1024,
            VolumeFreeBytes = 512L * 1024 * 1024 * 1024,
            GrowthObservationSpanDays = 60,
            GrowthObservationDayCount = 5,
            HistoricalGrowthMinimumBytesPer30Days = 64L * 1024 * 1024,
            HistoricalGrowthMaximumBytesPer30Days = 256L * 1024 * 1024,
            PredictionSummary = "历史逻辑新增无法直接换算成磁盘实占，不输出耗尽日期。",
            Trends =
            {
                new StorageTrendDto { Days = 7, AddedBytes = 64L * 1024 * 1024, AddedVersionCount = 2 },
                new StorageTrendDto { Days = 30, AddedBytes = 128L * 1024 * 1024, AddedVersionCount = 5 },
                new StorageTrendDto { Days = 90, AddedBytes = 256L * 1024 * 1024, AddedVersionCount = 11 }
            }
        };
        public ICommand CompareRetentionScenarioCommand { get; } = new RelayCommand(_ => { }, _ => true);
        public int MaintenanceTabIndex { get; set; } = 3;
    }
}
