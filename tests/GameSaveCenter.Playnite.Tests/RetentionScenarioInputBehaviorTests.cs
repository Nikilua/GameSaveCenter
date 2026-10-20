using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
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
    public void MaintenanceInputsStayInSessionAndShowInvalidFieldErrors(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                var draft = new RetentionScenarioDraft();
                var effectivePolicy = new RetentionPolicy { KeepDailyDays = 14 };
                var page = new MaintenanceView { DataContext = new ScenarioContext(draft, effectivePolicy) };
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
                    var pageScrollViewer = FindVisualAncestor<ScrollViewer>(card);
                    Assert.NotNull(pageScrollViewer);
                    Assert.True(pageScrollViewer!.ViewportHeight > 0);
                    Assert.True(pageScrollViewer.ScrollableHeight > 0, "The page scroll must expose content below the initial viewport.");
                    Assert.Equal(3, Field<UniformGrid>(page, "MaintenanceRetentionScenarioInputs").Columns);

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

                    SetAndCommit(daily, "-1");
                    PumpLayout(window);
                    Assert.True(Validation.GetHasError(daily));
                    Assert.Contains("0–36500", Field<TextBlock>(page, "RetentionScenarioKeepDailyDaysError").Text);
                    Assert.Equal(42, draft.CandidatePolicy.KeepDailyDays);
                }
                finally
                {
                    window.Close();
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

    private static T? FindVisualAncestor<T>(DependencyObject child)
        where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(child); current != null; current = VisualTreeHelper.GetParent(current))
            if (current is T match)
                return match;
        return null;
    }

    private sealed class ScenarioContext
    {
        public ScenarioContext(RetentionScenarioDraft input, RetentionPolicy effectivePolicy)
        {
            RetentionScenarioInput = input;
            EffectivePolicy = effectivePolicy;
        }

        public RetentionScenarioDraft RetentionScenarioInput { get; }
        public RetentionPolicy EffectivePolicy { get; }
        public int MaintenanceTabIndex { get; set; } = 3;
    }
}
