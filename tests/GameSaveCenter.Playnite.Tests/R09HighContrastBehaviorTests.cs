using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;
using UiButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09HighContrastBehaviorTests
{
    [Fact]
    public void HighContrastUsesSystemSemanticTokensAndRestoresMaterialAfterRecovery()
    {
        RunSta(() =>
        {
            var root = CreateProbeRoot();
            var highContrast = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                root,
                glassEnabled: true,
                strengthPercent: 78,
                GameSaveCenterThemeMode.FollowPlaynite,
                highContrastOverride: true);

            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                root.Resources, highContrast, glassEnabled: true, motionEnabled: true);
            root.UpdateLayout();

            Assert.True(highContrast.IsHighContrast);
            Assert.False(highContrast.GlassEnabled);
            Assert.Equal(SystemColors.WindowColor, highContrast.Background);
            Assert.Equal(SystemColors.WindowTextColor, highContrast.PrimaryText);
            Assert.Equal(SystemColors.GrayTextColor, highContrast.DisabledText);
            Assert.Equal(SystemColors.HighlightTextColor, BrushColor(root, "GscSelectionTextBrush"));
            Assert.Equal(SystemColors.ControlDarkColor, BrushColor(root, "GscProgressTrackBrush"));
            Assert.Equal(SystemColors.HighlightColor, BrushColor(root, "GscProgressFillBrush"));
            Assert.Equal(SystemColors.GrayTextColor, BrushColor(root, "GscDisabledTextBrush"));
            Assert.Equal(SystemColors.HighlightColor, BrushColor(root, "GscRowHoverStrongBrush"));
            Assert.Equal(highContrast.Accent, BrushColor(root, "GscAccentIconFillBrush"));
            Assert.All(GradientStops(root, "GscButtonGlassBrush"), stop => Assert.Equal(255, stop.Color.A));
            Assert.All(GradientStops(root, "GscPrimaryButtonBrush"), stop => Assert.Equal(255, stop.Color.A));
            Assert.All(GradientStops(root, "GscAmbientWideWashBrush"), stop => Assert.Equal(0, stop.Color.A));
            Assert.Null(root.Resources["GscSurfaceEffect"]);
            Assert.Null(root.Resources["GscGameBackgroundEffect"]);
            Assert.Equal(false, root.Resources["GscPopupAllowsTransparency"]);
            Assert.Equal(PopupAnimation.None, root.Resources["GscPopupAnimation"]);
            Assert.Equal(0d, root.Resources["GscGameBackgroundOpacity"]);
            var probePanel = Assert.IsType<StackPanel>(root.Children[0]);
            var progress = Assert.IsType<ProgressBar>(probePanel.Children[0]);
            var icon = Assert.IsType<Path>(probePanel.Children[1]);
            var disabledText = Assert.IsType<TextBlock>(probePanel.Children[2]);
            Assert.Equal(SystemColors.HighlightColor, Assert.IsType<SolidColorBrush>(progress.Foreground).Color);
            Assert.Equal(highContrast.Accent, Assert.IsType<SolidColorBrush>(icon.Fill).Color);
            Assert.Equal(SystemColors.GrayTextColor, Assert.IsType<SolidColorBrush>(disabledText.Foreground).Color);

            var normal = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                root,
                glassEnabled: true,
                strengthPercent: 78,
                GameSaveCenterThemeMode.FollowPlaynite,
                highContrastOverride: false);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                root.Resources, normal, glassEnabled: true, motionEnabled: true);
            root.UpdateLayout();

            Assert.False(normal.IsHighContrast);
            Assert.True(normal.GlassEnabled);
            var normalAmbientStops = GradientStops(root, "GscAmbientWideWashBrush");
            Assert.Contains(normalAmbientStops, stop => stop.Color.A > 0);
            var normalButtonStops = GradientStops(root, "GscButtonGlassBrush");
            Assert.Contains(normalButtonStops, stop => stop.Color.A < 255);
            Assert.NotEqual(SystemColors.ControlDarkColor, BrushColor(root, "GscProgressTrackBrush"));
        });
    }

    [Fact]
    public void HighContrastCombinationsKeepDisabledSelectedFocusAndPopupReadableAndRecoverMotion()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new StackPanel { Resources = resources };
            var disabledButton = new UiButton
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiSecondaryButton"]),
                Content = "不可用的合成操作",
                IsEnabled = false,
                Width = 210,
                Height = 42
            };
            var focusButton = new HighContrastStateProbeButton
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiSecondaryButton"]),
                Content = "键盘焦点目标",
                Width = 210,
                Height = 42
            };
            var combo = new ComboBox
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiComboBox"]),
                Width = 230,
                Height = 42
            };
            var disabledItem = new ComboBoxItem { Content = "不可选的合成选项", IsEnabled = false };
            var selectedItem = new ComboBoxItem { Content = "当前合成选项" };
            combo.Items.Add(disabledItem);
            combo.Items.Add(selectedItem);
            combo.SelectedIndex = 1;
            var focusRingHost = new Control
            {
                Style = Assert.IsType<Style>(resources["GscSharedFocusVisual"]),
                Width = 210,
                Height = 42,
                IsHitTestVisible = false
            };
            root.Children.Add(disabledButton);
            root.Children.Add(focusButton);
            root.Children.Add(combo);
            root.Children.Add(focusRingHost);
            var window = new Window
            {
                Content = root,
                Width = 330,
                Height = 250,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = true,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                FlushLayout(window);

                var modes = new[]
                {
                    GameSaveCenterThemeMode.FollowPlaynite,
                    GameSaveCenterThemeMode.Light,
                    GameSaveCenterThemeMode.Dark
                };
                foreach (var mode in modes)
                foreach (var hostIsDark in new[] { false, true })
                foreach (var glassRequested in new[] { false, true })
                foreach (var motionRequested in new[] { false, true })
                {
                    var host = CreateModeHost(hostIsDark);
                    var highContrast = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                        host,
                        glassRequested,
                        strengthPercent: 78,
                        mode,
                        highContrastOverride: true);
                    AdaptiveThemePaletteFactory.SetHighContrastActive(root, true);
                    AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, false);
                    AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                        resources, highContrast, glassRequested, motionRequested);
                    FlushLayout(window);

                    Assert.True(highContrast.IsHighContrast);
                    Assert.False(highContrast.GlassEnabled);
                    Assert.Equal(SystemColors.WindowColor, highContrast.Background);
                    Assert.Equal(SystemColors.WindowTextColor, highContrast.PrimaryText);
                    Assert.Equal(SystemColors.GrayTextColor, highContrast.DisabledText);
                    Assert.False(Assert.IsType<bool>(resources["GscPopupAllowsTransparency"]));
                    Assert.Equal(PopupAnimation.None, Assert.IsType<PopupAnimation>(resources["GscPopupAnimation"]));
                    Assert.False(Assert.IsType<bool>(resources["GscToggleMotionEnabled"]));
                    Assert.False(AdaptiveThemePaletteFactory.GetMotionEnabledActive(disabledButton));
                    Assert.Equal(TimeSpan.Zero, Assert.IsType<Duration>(resources["GscMotionFast"]).TimeSpan);
                    Assert.Equal(TimeSpan.Zero, Assert.IsType<Duration>(resources["GscMotionPress"]).TimeSpan);
                    Assert.Null(resources["GscPopupEffect"]);

                    Assert.True(AdaptiveThemePaletteFactory.GetHighContrastActive(disabledButton));
                    disabledButton.ApplyTemplate();
                    var disabledChrome = Assert.IsType<Border>(disabledButton.Template.FindName("ButtonChrome", disabledButton));
                    Assert.Equal(1d, disabledChrome.Opacity);
                    Assert.Equal(SystemColors.ControlColor, BrushColor(disabledChrome.Background));
                    Assert.Equal(SystemColors.ControlDarkColor, BrushColor(disabledChrome.BorderBrush));
                    Assert.Equal(SystemColors.GrayTextColor, BrushColor(disabledButton.Foreground));

                    Assert.Same(focusButton, Keyboard.Focus(focusButton));
                    FlushLayout(window);
                    Assert.True(focusButton.IsKeyboardFocusWithin);
                    var focusedChrome = Assert.IsType<Border>(focusButton.Template.FindName("ButtonChrome", focusButton));
                    Assert.Equal(SystemColors.HighlightColor, BrushColor(focusedChrome.BorderBrush));
                    // This case verifies the production template's pressed visuals under
                    // high contrast. Drive ButtonBase's read-only state through the probe
                    // instead of relying on a fabricated KeyDown to update WPF input state.
                    focusButton.SetPressedStateForProbe(true);
                    FlushLayout(window);
                    Assert.True(focusButton.IsPressed);
                    var pressedOverlay = Assert.IsType<Border>(focusButton.Template.FindName("PressedOverlay", focusButton));
                    Assert.True(pressedOverlay.Opacity == 1d,
                        $"mode={mode}; hostDark={hostIsDark}; glass={glassRequested}; opacity={pressedOverlay.Opacity}; pressed={focusButton.IsPressed}; activeMotion={AdaptiveThemePaletteFactory.GetMotionEnabledActive(focusButton)}; opacityToken={resources["GscButtonPressedOverlayOpacity"]}; source={DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty).BaseValueSource}/{DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty).IsAnimated}");
                    var highContrastPressSource = DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty);
                    if (highContrastPressSource.IsAnimated)
                    {
                        throw new InvalidOperationException(
                            $"High-contrast press kept an animation: mode={mode}; hostDark={hostIsDark}; glass={glassRequested}; requestedMotion={motionRequested}; pressed={focusButton.IsPressed}; opacity={pressedOverlay.Opacity}; motionActive={AdaptiveThemePaletteFactory.GetMotionEnabledActive(focusButton)}; source={highContrastPressSource.BaseValueSource}/{highContrastPressSource.IsAnimated}");
                    }
                    // This STA probe routes KeyDown to confirm the real template press state. WPF's
                    // KeyboardDevice is not updated by a directly raised KeyUpEvent, so release the
                    // read-only state through the test subclass instead of claiming native input.
                    focusButton.SetPressedStateForProbe(false);
                    Assert.False(focusButton.IsPressed);
                    focusRingHost.ApplyTemplate();
                    var focusRing = Assert.IsType<Border>(FindDescendant<Border>(focusRingHost));
                    Assert.Equal(SystemColors.HighlightColor, BrushColor(focusRing.BorderBrush));
                    Assert.Equal(new Thickness(2), focusRing.BorderThickness);

                    combo.IsDropDownOpen = true;
                    FlushLayout(window);
                    var popup = Assert.IsType<Popup>(combo.Template.FindName("PART_Popup", combo));
                    Assert.True(popup.IsOpen);
                    Assert.False(popup.AllowsTransparency);
                    Assert.Equal(PopupAnimation.None, popup.PopupAnimation);
                    var popupChrome = Assert.IsType<Border>(popup.Child);
                    Assert.Equal(SystemColors.ControlColor, BrushColor(popupChrome.Background));
                    Assert.Equal(SystemColors.ControlDarkColor, BrushColor(popupChrome.BorderBrush));
                    Assert.Null(popupChrome.Effect);

                    disabledItem.ApplyTemplate();
                    var disabledItemChrome = Assert.IsType<Border>(disabledItem.Template.FindName("ItemChrome", disabledItem));
                    Assert.Equal(1d, disabledItemChrome.Opacity);
                    Assert.Equal(SystemColors.ControlColor, BrushColor(disabledItemChrome.Background));
                    Assert.Equal(SystemColors.ControlDarkColor, BrushColor(disabledItemChrome.BorderBrush));
                    Assert.Equal(SystemColors.GrayTextColor, BrushColor(disabledItem.Foreground));

                    selectedItem.ApplyTemplate();
                    var selectedChrome = Assert.IsType<Border>(selectedItem.Template.FindName("ItemChrome", selectedItem));
                    Assert.Equal(SystemColors.HighlightColor, BrushColor(selectedChrome.Background));
                    Assert.Equal(SystemColors.HighlightTextColor, BrushColor(selectedItem.Foreground));

                    combo.IsDropDownOpen = false;
                    var normal = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                        host,
                        glassRequested,
                        strengthPercent: 78,
                        mode,
                        highContrastOverride: false);
                    AdaptiveThemePaletteFactory.SetHighContrastActive(root, false);
                    AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, motionRequested);
                    AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                        resources, normal, glassRequested, motionRequested);
                    FlushLayout(window);
                    Assert.Same(focusButton, Keyboard.Focus(focusButton));
                    focusButton.SetPressedStateForProbe(true);
                    FlushLayout(window);
                    Assert.True(focusButton.IsPressed);
                    if (motionRequested)
                    {
                        var opacitySource = DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty);
                        Assert.True(opacitySource.IsAnimated,
                            $"motion={motionRequested}; inheritedMotion={AdaptiveThemePaletteFactory.GetMotionEnabledActive(focusButton)}; pressed={focusButton.IsPressed}; opacity={pressedOverlay.Opacity}; source={opacitySource.BaseValueSource}/{opacitySource.IsExpression}");
                    }
                    else
                    {
                        Assert.Equal(1d, pressedOverlay.Opacity);
                        Assert.False(DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty).IsAnimated);
                    }
                    focusButton.SetPressedStateForProbe(false);
                    Assert.False(focusButton.IsPressed);
                    if (motionRequested)
                    {
                        PumpDispatcher(window, TimeSpan.FromMilliseconds(160));
                        FlushLayout(window);
                    }

                    var expectedDark = mode == GameSaveCenterThemeMode.Dark
                        || (mode == GameSaveCenterThemeMode.FollowPlaynite && hostIsDark);
                    Assert.False(normal.IsHighContrast);
                    Assert.Equal(expectedDark, normal.IsDark);
                    Assert.Equal(glassRequested, normal.GlassEnabled);
                    Assert.Equal(glassRequested, Assert.IsType<bool>(resources["GscPopupAllowsTransparency"]));
                    Assert.Equal(motionRequested ? PopupAnimation.Fade : PopupAnimation.None,
                        Assert.IsType<PopupAnimation>(resources["GscPopupAnimation"]));
                    Assert.Equal(motionRequested ? TimeSpan.FromMilliseconds(120) : TimeSpan.Zero,
                        Assert.IsType<Duration>(resources["GscMotionFast"]).TimeSpan);
                    Assert.Equal(1d, disabledChrome.Opacity);
                    Assert.Equal(BrushColor(Assert.IsAssignableFrom<Brush>(resources["GscControlFillBrush"])), BrushColor(disabledChrome.Background));
                    Assert.Equal(BrushColor(Assert.IsAssignableFrom<Brush>(resources["GscControlStrokeBrush"])), BrushColor(disabledChrome.BorderBrush));
                    Assert.Equal(BrushColor(Assert.IsAssignableFrom<Brush>(resources["GscDisabledTextBrush"])), BrushColor(disabledButton.Foreground));
                    Assert.False(AdaptiveThemePaletteFactory.GetHighContrastActive(disabledButton));
                    Assert.Equal(motionRequested, AdaptiveThemePaletteFactory.GetMotionEnabledActive(disabledButton));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void HighContrastStopsActiveButtonAndDisclosureAnimationsAndRestoresThemAfterRecovery()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new StackPanel { Resources = resources };
            var button = new HighContrastStateProbeButton
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiSecondaryButton"]),
                Content = "按压动画状态",
                Width = 210,
                Height = 42
            };
            var expander = new Expander
            {
                Style = Assert.IsType<Style>(resources["GscDisclosureCardExpander"]),
                Header = "展开动画状态",
                Width = 280
            };
            root.Children.Add(button);
            root.Children.Add(expander);
            var window = new Window
            {
                Content = root,
                Width = 330,
                Height = 180,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = true,
                Opacity = 0.01
            };

            try
            {
                var host = CreateModeHost(isDark: false);
                var normal = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    host,
                    glassEnabled: true,
                    strengthPercent: 78,
                    GameSaveCenterThemeMode.FollowPlaynite,
                    highContrastOverride: false);
                AdaptiveThemePaletteFactory.SetHighContrastActive(root, false);
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, true);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, normal, glassEnabled: true, motionEnabled: true);

                window.Show();
                FlushLayout(window);
                button.ApplyTemplate();
                expander.ApplyTemplate();
                var header = Assert.IsType<ToggleButton>(expander.Template.FindName("HeaderSite", expander));
                header.ApplyTemplate();
                var chevron = Assert.IsType<Path>(header.Template.FindName("Chevron", header));
                var buttonChrome = Assert.IsType<Border>(button.Template.FindName("ButtonChrome", button));
                var pressedOverlay = Assert.IsType<Border>(button.Template.FindName("PressedOverlay", button));

                expander.IsExpanded = true;
                Assert.Same(button, Keyboard.Focus(button));
                RaiseSpaceKey(button, window, isKeyDown: true);
                FlushLayout(window);

                var normalChevron = Assert.IsType<RotateTransform>(chevron.RenderTransform);
                Assert.True(DependencyPropertyHelper.GetValueSource(normalChevron, RotateTransform.AngleProperty).IsAnimated);
                Assert.True(DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty).IsAnimated);
                Assert.True(button.IsPressed);

                var highContrast = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    host,
                    glassEnabled: true,
                    strengthPercent: 78,
                    GameSaveCenterThemeMode.FollowPlaynite,
                    highContrastOverride: true);
                AdaptiveThemePaletteFactory.SetHighContrastActive(root, true);
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, false);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, highContrast, glassEnabled: true, motionEnabled: true);
                FlushLayout(window);

                Assert.True(button.IsPressed);
                Assert.Equal(1d, pressedOverlay.Opacity);
                var highContrastOverlaySource = DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty);
                if (highContrastOverlaySource.IsAnimated)
                {
                    throw new InvalidOperationException(
                        $"High-contrast transition kept the button overlay clock: pressed={button.IsPressed}; opacity={pressedOverlay.Opacity}; motion={AdaptiveThemePaletteFactory.GetMotionEnabledActive(button)}; source={highContrastOverlaySource.BaseValueSource}/{highContrastOverlaySource.IsAnimated}");
                }
                var reducedMotionScale = Assert.IsType<ScaleTransform>(buttonChrome.RenderTransform);
                Assert.Equal(1d, reducedMotionScale.ScaleX);
                Assert.Equal(1d, reducedMotionScale.ScaleY);
                var highContrastScaleSource = DependencyPropertyHelper.GetValueSource(reducedMotionScale, ScaleTransform.ScaleXProperty);
                if (highContrastScaleSource.IsAnimated)
                {
                    throw new InvalidOperationException(
                        $"High-contrast transition kept the button scale clock: x={reducedMotionScale.ScaleX}; y={reducedMotionScale.ScaleY}; motion={AdaptiveThemePaletteFactory.GetMotionEnabledActive(button)}; source={highContrastScaleSource.BaseValueSource}/{highContrastScaleSource.IsAnimated}");
                }
                var reducedMotionChevron = Assert.IsType<RotateTransform>(chevron.RenderTransform);
                Assert.Equal(0d, reducedMotionChevron.Angle);
                var highContrastChevronSource = DependencyPropertyHelper.GetValueSource(reducedMotionChevron, RotateTransform.AngleProperty);
                if (highContrastChevronSource.IsAnimated)
                {
                    throw new InvalidOperationException(
                        $"High-contrast transition kept the disclosure clock: angle={reducedMotionChevron.Angle}; motion={AdaptiveThemePaletteFactory.GetMotionEnabledActive(button)}; source={highContrastChevronSource.BaseValueSource}/{highContrastChevronSource.IsAnimated}");
                }

                button.SetPressedStateForProbe(false);
                Assert.False(button.IsPressed);
                Assert.Equal(0d, pressedOverlay.Opacity);
                expander.IsExpanded = false;
                FlushLayout(window);
                var collapsedChevron = Assert.IsType<RotateTransform>(chevron.RenderTransform);
                Assert.Equal(-90d, collapsedChevron.Angle);
                Assert.False(DependencyPropertyHelper.GetValueSource(collapsedChevron, RotateTransform.AngleProperty).IsAnimated);

                AdaptiveThemePaletteFactory.SetHighContrastActive(root, false);
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, true);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, normal, glassEnabled: true, motionEnabled: true);
                FlushLayout(window);
                expander.IsExpanded = true;
                RaiseSpaceKey(button, window, isKeyDown: true);
                FlushLayout(window);
                Assert.True(DependencyPropertyHelper.GetValueSource(
                    Assert.IsType<RotateTransform>(chevron.RenderTransform), RotateTransform.AngleProperty).IsAnimated);
                Assert.True(DependencyPropertyHelper.GetValueSource(pressedOverlay, UIElement.OpacityProperty).IsAnimated);
                button.SetPressedStateForProbe(false);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static Grid CreateModeHost(bool isDark)
    {
        var background = isDark ? Color.FromRgb(17, 19, 25) : Color.FromRgb(248, 249, 252);
        var host = new Grid();
        host.Resources["WindowBackgroundBrush"] = AdaptiveThemePaletteFactory.Brush(background);
        host.Resources["DarkWindowBackgroundBrush"] = AdaptiveThemePaletteFactory.Brush(background);
        host.Resources["ThemeDarkStyle"] = isDark;
        host.Resources["TextBrush"] = AdaptiveThemePaletteFactory.Brush(isDark ? Colors.White : Colors.Black);
        host.Resources["TextBrushDark"] = AdaptiveThemePaletteFactory.Brush(isDark ? Colors.Black : Colors.White);
        return host;
    }

    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                return match;

            var nested = FindDescendant<T>(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private static Color BrushColor(Brush? brush)
        => Assert.IsType<SolidColorBrush>(brush).Color;

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void PumpDispatcher(Window window, TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
        {
            Interval = duration
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private sealed class HighContrastStateProbeButton : UiButton
    {
        public void SetPressedStateForProbe(bool value) => IsPressed = value;
    }

    private static void RaiseSpaceKey(UiButton button, Window window, bool isKeyDown)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(window),
            Environment.TickCount,
            Key.Space)
        {
            RoutedEvent = isKeyDown ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent
        };
        button.RaiseEvent(args);
    }

    private static Grid CreateProbeRoot()
    {
        var root = new Grid();
        var progress = new ProgressBar();
        progress.SetResourceReference(ProgressBar.ForegroundProperty, "GscProgressFillBrush");
        progress.SetResourceReference(ProgressBar.BackgroundProperty, "GscProgressTrackBrush");
        var icon = new Path();
        icon.SetResourceReference(Shape.FillProperty, "GscAccentIconFillBrush");
        var disabledText = new TextBlock { Text = "disabled" };
        disabledText.SetResourceReference(TextBlock.ForegroundProperty, "GscDisabledTextBrush");
        root.Children.Add(new StackPanel
        {
            Children = { progress, icon, disabledText }
        });
        return root;
    }

    private static Color BrushColor(FrameworkElement root, string key)
        => Assert.IsType<SolidColorBrush>(root.Resources[key]).Color;

    private static GradientStopCollection GradientStops(FrameworkElement root, string key)
        => Assert.IsType<LinearGradientBrush>(root.Resources[key]).GradientStops;

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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
