using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05PopupLifecycleBehaviorTests
{
    [Fact]
    public void OpenProductionSurfacesFollowThemeAndCloseWhenSettingsWindowHides()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
            var settings = new GameSaveCenterSettings
            {
                ThemeMode = GameSaveCenterThemeMode.Light
            };
            var view = new GameSaveCenterSettingsView { DataContext = settings };
            var window = new Window
            {
                Content = view,
                Width = 1040,
                Height = 700,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                var tabs = (ListBox)view.FindName("SettingsSectionTabs")!;
                tabs.SelectedIndex = 2;
                DrainDispatcher();
                window.UpdateLayout();

                var selector = (ComboBox)view.FindName("ThemeModeSelector")!;
                selector.ApplyTemplate();
                selector.IsDropDownOpen = true;
                DrainDispatcher();
                var popup = (Popup)selector.Template.FindName("PART_Popup", selector)!;
                Assert.NotNull(popup.Child);
                popup.StaysOpen = true;
                popup.IsOpen = true;
                DrainDispatcher();
                popup.Child!.UpdateLayout();

                var hint = (Border)view.FindName("SettingsSaveHint")!;
                var tooltip = new ToolTip
                {
                    Content = "主题切换中的合成提示",
                    PlacementTarget = hint,
                    Placement = PlacementMode.Bottom,
                    StaysOpen = true,
                    Style = (Style)view.FindResource(typeof(ToolTip))
                };
                hint.ToolTip = tooltip;
                tooltip.IsOpen = true;
                DrainDispatcher();
                tooltip.UpdateLayout();

                var lightPopup = ReadBackground(popup.Child);
                var lightTooltip = ReadBackground(tooltip);
                Assert.True(popup.IsOpen);
                Assert.True(tooltip.IsOpen);
                Assert.NotNull(lightPopup);
                Assert.NotNull(lightTooltip);

                view.ApplyThemeForAudit(GameSaveCenterThemeMode.Dark);
                selector.SelectedValue = GameSaveCenterThemeMode.Dark;
                DrainDispatcher();
                window.UpdateLayout();
                popup.Child.UpdateLayout();
                tooltip.UpdateLayout();

                var darkPopup = ReadBackground(popup.Child);
                var darkTooltip = ReadBackground(tooltip);
                var mirroredTooltipBrush = tooltip.Resources["FloatingFillBrush"] as Brush;
                Assert.True(popup.IsOpen);
                Assert.True(tooltip.IsOpen);
                Assert.True(!string.Equals(lightPopup, darkPopup, StringComparison.Ordinal),
                    $"Popup 背景未随主题切换：浅色={lightPopup}，深色={darkPopup}");
                Assert.True(!string.Equals(lightTooltip, darkTooltip, StringComparison.Ordinal),
                    $"ToolTip 背景未随主题切换：浅色={lightTooltip}，深色={darkTooltip}，属性={ReadBrushColor(tooltip.Background)}，资源={ReadBrushColor(mirroredTooltipBrush)}");

                window.Hide();
                DrainDispatcher();

                Assert.False(view.IsVisible);
                Assert.False(popup.IsOpen);
                Assert.False(tooltip.IsOpen);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static string? ReadBackground(DependencyObject? root)
    {
        if (root is Border border && border.Background is SolidColorBrush brush)
            return brush.Color.ToString();

        for (var index = 0; root != null && index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var background = ReadBackground(VisualTreeHelper.GetChild(root, index));
            if (background != null) return background;
        }

        return null;
    }

    private static string? ReadBrushColor(Brush? brush)
        => (brush as SolidColorBrush)?.Color.ToString();

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
