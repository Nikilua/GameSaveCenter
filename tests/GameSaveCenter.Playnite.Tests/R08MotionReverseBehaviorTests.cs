using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;
using UiButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08MotionReverseBehaviorTests
{
    private static readonly DependencyPropertyKey IsMouseOverPropertyKey =
        (DependencyPropertyKey)(typeof(UIElement).GetField(
            "IsMouseOverPropertyKey",
            BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
            ?? throw new InvalidOperationException("WPF UIElement.IsMouseOverPropertyKey is unavailable."));

    private readonly ITestOutputHelper output;

    public R08MotionReverseBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void TranslateReversalStartsAtRenderedValueAndFinishesAtLatestTarget()
    {
        Exception? exception = null;
        var firstMidpoint = 0d;
        var reversalStart = 0d;
        var finalValue = 0d;
        var midpointSampled = false;
        var translationCompleted = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var host = new Border { Width = 48, Height = 48, Background = Brushes.Transparent };
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(2));
                window = CreateWindow(host, 90, 90);
                window.Show();
                window.UpdateLayout();

                var translate = GscMotion.GetMutableTranslateTransform(host);
                GscMotion.AnimateTranslate(host, 12, 0, GscMotion.MotionDurationKind.Normal);
                midpointSampled = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        return translate.X > 0.2 && translate.X < 11.8;
                    },
                    TimeSpan.FromSeconds(1));
                firstMidpoint = translate.X;
                GscMotion.AnimateTranslate(host, -8, 0, GscMotion.MotionDurationKind.Normal);
                reversalStart = translate.X;
                translationCompleted = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        var xIsAnimated = DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.XProperty).IsAnimated;
                        var yIsAnimated = DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated;
                        return !xIsAnimated
                            && !yIsAnimated
                            && Math.Abs(translate.X + 8) < 0.001
                            && Math.Abs(translate.Y) < 0.001;
                    },
                    TimeSpan.FromSeconds(3));
                window.UpdateLayout();
                finalValue = translate.X;

                Assert.True(midpointSampled, "translation never exposed an in-flight sample within the one-second sampling window");
                Assert.True(translationCompleted, "translation did not release both animation clocks within 3 seconds");
                Assert.InRange(firstMidpoint, 0.2, 11.8);
                Assert.InRange(reversalStart, firstMidpoint - 0.8, firstMidpoint + 0.8);
                Assert.Equal(-8, finalValue, 3);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.XProperty).IsAnimated);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        output.WriteLine($"translate midpoint={firstMidpoint:0.###}; sampled={midpointSampled}; reversalStart={reversalStart:0.###}; final={finalValue:0.###}; completed={translationCompleted}");
        Assert.Null(exception);
    }

    [Fact]
    public void SidebarRapidReversalUsesLatestTargetAndReleasesOldClock()
    {
        Exception? exception = null;
        var collapseMidpoint = 0d;
        var reversalStart = 0d;
        var reverseMidpoint = 0d;
        var finalWidth = 0d;
        var finalWidthRoundingTolerance = 0d;
        var finalOpacity = 0d;
        var collapseProgressSampled = false;
        var sidebarTransitionCompleted = false;
        var reverseProgressSampled = false;
        var sidebarTrace = string.Empty;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView
                {
                    MotionEnabledProvider = () => true,
                    // Keep the animation premise deterministic when the runner disables
                    // client-area animations. Native preference handling has its own tests.
                    SystemMotionEnabledProviderForAudit = () => true,
                    SidebarCollapsedProvider = () => false
                };
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(2));
                window = CreateWindow(shell, 900, 640);
                window.Show();
                window.UpdateLayout();
                shell.UpdateLayout();
                Assert.True(shell.SidebarMotionEnabledForAudit,
                    "the reversal fixture must explicitly enable both app and system motion inputs");
                finalWidthRoundingTolerance = 0.5 / VisualTreeHelper.GetDpi(shell).DpiScaleX + 0.001;

                var sidebar = Assert.IsType<ColumnDefinition>(shell.FindName("SidebarColumn"));
                var layer = Assert.IsAssignableFrom<FrameworkElement>(shell.FindName("SidebarContentLayer"));
                shell.SidebarCollapseButtonForAudit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                collapseProgressSampled = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        shell.UpdateLayout();
                        var width = sidebar.ActualWidth;
                        return width < 269.5
                            && width > 72.5
                            && DependencyPropertyHelper.GetValueSource(sidebar, ColumnDefinition.WidthProperty).IsAnimated;
                    },
                    TimeSpan.FromMilliseconds(800));
                collapseMidpoint = sidebar.ActualWidth;
                sidebarTrace = $"after-collapse collapsed={shell.SidebarCollapsedForAudit}; running={shell.SidebarTransitionRunningForAudit}; base={shell.SidebarWidthForAudit:0.###}; actual={sidebar.ActualWidth:0.###}";

                shell.SidebarCollapseButtonForAudit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                reversalStart = sidebar.ActualWidth;
                reverseProgressSampled = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        shell.UpdateLayout();
                        return sidebar.ActualWidth > reversalStart + 0.5
                            && DependencyPropertyHelper.GetValueSource(sidebar, ColumnDefinition.WidthProperty).IsAnimated;
                    },
                    TimeSpan.FromMilliseconds(1000));
                reverseMidpoint = sidebar.ActualWidth;
                sidebarTrace += $" | after-reverse-progress collapsed={shell.SidebarCollapsedForAudit}; running={shell.SidebarTransitionRunningForAudit}; base={shell.SidebarWidthForAudit:0.###}; actual={sidebar.ActualWidth:0.###}";
                sidebarTransitionCompleted = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        shell.UpdateLayout();
                        return !shell.SidebarTransitionRunningForAudit;
                    },
                    TimeSpan.FromSeconds(3));
                window.UpdateLayout();
                shell.UpdateLayout();
                finalWidth = sidebar.ActualWidth;
                finalOpacity = layer.Opacity;

                Assert.True(collapseProgressSampled, "sidebar collapse never exposed an in-flight width sample within 800 ms");
                Assert.InRange(collapseMidpoint, 72.2, 269.8);
                Assert.InRange(reversalStart, collapseMidpoint - 1.5, collapseMidpoint + 1.5);
                Assert.True(reverseProgressSampled, "sidebar expansion never exposed an in-flight width sample within 1 second");
                Assert.True(reverseMidpoint > reversalStart, $"sidebar did not reverse toward expanded target: {reverseMidpoint} <= {reversalStart}");
                Assert.True(sidebarTransitionCompleted, "sidebar animation did not deliver its completion callback within 2 seconds");
                Assert.InRange(Math.Abs(270 - finalWidth), 0, finalWidthRoundingTolerance);
                Assert.False(shell.SidebarTransitionRunningForAudit);
                Assert.Equal(1, finalOpacity, 3);
                Assert.False(DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        output.WriteLine($"sidebar collapseMid={collapseMidpoint:0.###}; reversalStart={reversalStart:0.###}; reverseMid={reverseMidpoint:0.###}; final={finalWidth:0.###}; opacity={finalOpacity:0.###}; completed={sidebarTransitionCompleted}; trace={sidebarTrace}");
        Assert.Null(exception);
    }

    [Fact]
    public void ProductionButtonHoverAndPressReverseFromTheirRenderedValues()
    {
        Exception? exception = null;
        var hoverMidpoint = 0d;
        var hoverAfterLeave = 0d;
        var hoverExitProgress = 0d;
        var hoverBeforeReenter = 0d;
        var hoverAfterReenter = 0d;
        var hoverReenterProgress = 0d;
        var hoverFinal = 0d;
        var hoverMidpointSampled = false;
        var hoverExitProgressSampled = false;
        var hoverReenterProgressSampled = false;
        var pressMidpoint = 0d;
        var pressedOverlayMidpoint = 0d;
        var pressAfterRelease = 0d;
        var pressedOverlayAfterRelease = 0d;
        var pressedOverlayReleaseProgress = 0d;
        var pressBeforeRepress = 0d;
        var pressAfterRepress = 0d;
        var pressedOverlayBeforeRepress = 0d;
        var pressedOverlayAfterRepress = 0d;
        var pressedOverlayRepressProgress = 0d;
        var pressedOverlayFinal = 0d;
        var pressedOverlayReleasedFinal = 0d;
        var pressFinal = 0d;
        var pressMidpointSampled = false;
        var pressReleasedContinuously = false;
        var pressedOverlayReleasedContinuously = false;
        var pressReleaseProgressSampled = false;
        var pressReversedContinuously = false;
        var pressedOverlayReversedContinuously = false;
        var pressRepressProgressSampled = false;
        var pressReleasedFinal = 0d;
        var hoverSweepVisitedCount = 0;
        var hoverSweepMaximumResidualOpacity = 0d;
        var hoverSweepReentered = false;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplication();
                var resources = LoadProductionResources();
                var buttonStyle = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]);
                var animationSamplingWindow = ExtendProductionButtonMotionSamplingWindow(buttonStyle);
                var root = new Grid
                {
                    Width = 420,
                    Height = 128,
                    Resources = resources,
                    Background = Brushes.Transparent,
                    IsHitTestVisible = false
                };
                var palette = AdaptiveThemePaletteFactory.Create(root, true, 78, GameSaveCenterThemeMode.Light);
                AdaptiveThemePaletteFactory.SetHighContrastActive(root, palette.IsHighContrast);
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, !palette.IsHighContrast);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(root.Resources, palette, true, true);

                var button = new UiButton
                {
                    Content = "执行安全操作",
                    Style = buttonStyle,
                    Width = 220,
                    Height = 44,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                root.Children.Add(button);
                window = CreateWindow(root, 460, 156);
                window.ShowActivated = true;
                window.Show();
                window.UpdateLayout();
                root.UpdateLayout();
                button.ApplyTemplate();

                var hoverOverlay = Assert.IsType<Border>(button.Template!.FindName("HoverOverlay", button));
                var chrome = Assert.IsType<Border>(button.Template.FindName("ButtonChrome", button));
                var pressedOverlay = Assert.IsType<Border>(button.Template.FindName("PressedOverlay", button));
                var scale = Assert.IsType<ScaleTransform>(chrome.RenderTransform);

                PumpDispatcher(animationSamplingWindow + TimeSpan.FromMilliseconds(40));
                Assert.False(ReadMouseOverProbe(button), "button unexpectedly entered hover before the synthetic probe");
                Assert.Equal(0, hoverOverlay.Opacity, 3);

                Assert.True(SetMouseOverProbe(button, true));
                hoverMidpointSampled = PumpMouseOverProbeUntil(button, true,
                    () => hoverOverlay.Opacity > 0.05 && hoverOverlay.Opacity < 0.95,
                    TimeSpan.FromMilliseconds(500));
                hoverMidpoint = hoverOverlay.Opacity;
                Assert.True(SetMouseOverProbe(button, false));
                hoverAfterLeave = hoverOverlay.Opacity;
                hoverExitProgressSampled = PumpMouseOverProbeUntil(button, false,
                    () => hoverOverlay.Opacity < hoverAfterLeave - 0.002 && hoverOverlay.Opacity > 0,
                    TimeSpan.FromMilliseconds(250));
                hoverExitProgress = hoverOverlay.Opacity;
                Assert.True(SetMouseOverProbe(button, true));
                hoverBeforeReenter = hoverOverlay.Opacity;
                hoverAfterReenter = hoverOverlay.Opacity;
                hoverReenterProgressSampled = PumpMouseOverProbeUntil(button, true,
                    () => hoverOverlay.Opacity > hoverAfterReenter + 0.002 && hoverOverlay.Opacity < 1,
                    TimeSpan.FromMilliseconds(250));
                hoverReenterProgress = hoverOverlay.Opacity;
                PumpMouseOverProbeUntil(button, true, () => Math.Abs(hoverOverlay.Opacity - 1) < 0.001, TimeSpan.FromMilliseconds(500));
                hoverFinal = hoverOverlay.Opacity;

                SetPressedStateProbe(button, true);
                Assert.True(button.IsPressed, "synthetic state probe did not enter the production button's pressed state.");
                pressMidpointSampled = PumpDispatcherUntilFast(
                    () => scale.ScaleX <= 0.99 && scale.ScaleX > 0.9701
                        && pressedOverlay.Opacity > 0.01 && pressedOverlay.Opacity < 0.99,
                    TimeSpan.FromMilliseconds(1000));
                pressMidpoint = scale.ScaleX;
                pressedOverlayMidpoint = pressedOverlay.Opacity;
                SetPressedStateProbe(button, false);
                Assert.False(button.IsPressed, "synthetic state probe did not leave the production button's pressed state.");
                pressAfterRelease = scale.ScaleX;
                pressedOverlayAfterRelease = pressedOverlay.Opacity;
                pressReleasedContinuously = Math.Abs(pressAfterRelease - pressMidpoint) <= 0.01;
                pressedOverlayReleasedContinuously = Math.Abs(pressedOverlayAfterRelease - pressedOverlayMidpoint) <= 0.08;

                pressReleaseProgressSampled = PumpDispatcherUntilFast(
                    () => scale.ScaleX > pressAfterRelease + 0.002 && scale.ScaleX < 1
                        && pressedOverlay.Opacity < pressedOverlayAfterRelease - 0.002 && pressedOverlay.Opacity > 0,
                    TimeSpan.FromMilliseconds(250));
                pressedOverlayReleaseProgress = pressedOverlay.Opacity;
                pressBeforeRepress = scale.ScaleX;
                pressedOverlayBeforeRepress = pressedOverlay.Opacity;
                SetPressedStateProbe(button, true);
                pressAfterRepress = scale.ScaleX;
                pressedOverlayAfterRepress = pressedOverlay.Opacity;
                pressReversedContinuously = Math.Abs(pressAfterRepress - pressBeforeRepress) <= 0.01;
                pressedOverlayReversedContinuously = Math.Abs(pressedOverlayAfterRepress - pressedOverlayBeforeRepress) <= 0.08;
                Assert.True(button.IsPressed, "Second Space key down did not re-enter the pressed state.");
                pressRepressProgressSampled = PumpDispatcherUntilFast(
                    () => scale.ScaleX < pressAfterRepress - 0.002 && scale.ScaleX > 0.97
                        && pressedOverlay.Opacity > pressedOverlayAfterRepress + 0.002 && pressedOverlay.Opacity < 1,
                    TimeSpan.FromMilliseconds(250));
                pressedOverlayRepressProgress = pressedOverlay.Opacity;
                PumpDispatcherUntilFast(
                    () => Math.Abs(scale.ScaleX - 0.97) < 0.001 && Math.Abs(pressedOverlay.Opacity - 1) < 0.001,
                    TimeSpan.FromMilliseconds(500));
                pressFinal = scale.ScaleX;
                pressedOverlayFinal = pressedOverlay.Opacity;
                SetPressedStateProbe(button, false);
                PumpDispatcherUntil(() => Math.Abs(scale.ScaleX - 1) < 0.001, TimeSpan.FromMilliseconds(500));
                pressReleasedFinal = scale.ScaleX;
                PumpDispatcherUntil(() => Math.Abs(pressedOverlay.Opacity) < 0.001, TimeSpan.FromMilliseconds(500));
                pressedOverlayReleasedFinal = pressedOverlay.Opacity;

                hoverSweepReentered = RunProductionButtonHoverSweep(resources, buttonStyle,
                    out hoverSweepVisitedCount, out hoverSweepMaximumResidualOpacity);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        output.WriteLine($"button hover midpoint={hoverMidpoint:0.###}; exit={hoverExitProgress:0.###}/{hoverAfterLeave:0.###}; reenter={hoverReenterProgress:0.###}/{hoverBeforeReenter:0.###}; final={hoverFinal:0.###}; samples={hoverMidpointSampled}/{hoverExitProgressSampled}/{hoverReenterProgressSampled}; press midpoint={pressMidpoint:0.###}; release={pressAfterRelease:0.###}->{pressBeforeRepress:0.###}; re-press={pressAfterRepress:0.###}->{pressFinal:0.###}; overlay={pressedOverlayMidpoint:0.###}->{pressedOverlayAfterRelease:0.###}->{pressedOverlayFinal:0.###}; releaseFinal={pressReleasedFinal:0.###}/{pressedOverlayReleasedFinal:0.###}; transitions={pressMidpointSampled}/{pressReleaseProgressSampled}/{pressRepressProgressSampled}; sibling sweep visited={hoverSweepVisitedCount}; max residual={hoverSweepMaximumResidualOpacity:0.####}; hover re-entry={hoverSweepReentered}");
        Assert.Null(exception);
        Assert.True(hoverMidpointSampled, "production hover opacity did not expose an in-flight sample");
        Assert.InRange(Math.Abs(hoverAfterLeave - hoverMidpoint), 0, 0.08);
        Assert.InRange(Math.Abs(hoverAfterReenter - hoverBeforeReenter), 0, 0.08);
        Assert.True(hoverExitProgressSampled, "production hover exit did not expose an in-flight sample");
        Assert.True(hoverReenterProgressSampled, "production hover re-entry did not expose an in-flight sample");
        Assert.True(hoverExitProgress < hoverAfterLeave, "hover exit did not continue toward zero");
        Assert.True(hoverReenterProgress > hoverAfterReenter, "hover re-entry did not continue toward one");
        Assert.Equal(1, hoverFinal, 2);
        Assert.True(pressMidpointSampled, "production press scale did not expose an in-flight sample");
        Assert.True(pressReleasedContinuously, "production press release jumped away from the rendered scale");
        Assert.True(pressReleaseProgressSampled, "production press release did not expose an in-flight return sample");
        Assert.True(pressBeforeRepress > pressAfterRelease, "press release did not continue toward the neutral scale");
        Assert.True(pressReversedContinuously, "production rapid re-press jumped away from the rendered scale");
        Assert.True(pressRepressProgressSampled, "production rapid re-press did not expose an in-flight sample");
        Assert.Equal(0.97, pressFinal, 2);
        Assert.Equal(1, pressReleasedFinal, 2);
        Assert.True(pressedOverlayMidpoint > 0 && pressedOverlayMidpoint < 1, "production pressed overlay did not expose an in-flight sample");
        Assert.True(pressedOverlayReleasedContinuously, "production pressed overlay release jumped away from its rendered opacity");
        Assert.InRange(Math.Abs(pressedOverlayAfterRelease - pressedOverlayMidpoint), 0, 0.08);
        Assert.True(pressedOverlayReleaseProgress < pressedOverlayAfterRelease, "pressed overlay release did not continue toward zero");
        Assert.True(pressedOverlayReversedContinuously, "production pressed overlay re-press jumped away from its rendered opacity");
        Assert.InRange(Math.Abs(pressedOverlayAfterRepress - pressedOverlayBeforeRepress), 0, 0.08);
        Assert.True(pressedOverlayRepressProgress > pressedOverlayAfterRepress, "pressed overlay re-press did not continue toward one");
        Assert.Equal(1, pressedOverlayFinal, 2);
        Assert.Equal(0, pressedOverlayReleasedFinal, 2);
        Assert.Equal(24, hoverSweepVisitedCount);
        Assert.True(hoverSweepReentered, "production hover did not respond after a rapid sibling sweep");
        Assert.InRange(hoverSweepMaximumResidualOpacity, 0, 0.001);
    }

    private static bool RunProductionButtonHoverSweep(
        ResourceDictionary resources,
        Style buttonStyle,
        out int visitedCount,
        out double maximumResidualOpacity)
    {
        visitedCount = 0;
        maximumResidualOpacity = 0;
        Window? window = null;
        try
        {
            var root = new Grid
            {
                Width = 820,
                Height = 72,
                Resources = resources,
                Background = Brushes.Transparent,
                IsHitTestVisible = false
            };
            var palette = AdaptiveThemePaletteFactory.Create(root, true, 78, GameSaveCenterThemeMode.Light);
            AdaptiveThemePaletteFactory.SetHighContrastActive(root, palette.IsHighContrast);
            AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, !palette.IsHighContrast);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(root.Resources, palette, true, true);

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var buttons = new UiButton[8];
            var overlays = new Border[buttons.Length];
            for (var index = 0; index < buttons.Length; index++)
            {
                buttons[index] = new UiButton
                {
                    Content = $"操作 {index + 1}",
                    Style = buttonStyle,
                    Width = 96,
                    Height = 44,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                row.Children.Add(buttons[index]);
            }

            root.Children.Add(row);
            window = CreateWindow(root, 860, 112);
            window.Show();
            window.UpdateLayout();
            root.UpdateLayout();
            for (var index = 0; index < buttons.Length; index++)
            {
                buttons[index].ApplyTemplate();
                overlays[index] = Assert.IsType<Border>(buttons[index].Template!.FindName("HoverOverlay", buttons[index]));
            }

            PumpDispatcher(TimeSpan.FromMilliseconds(40));
            foreach (var overlay in overlays)
                Assert.Equal(0, overlay.Opacity, 3);

            UiButton? previous = null;
            for (var pass = 0; pass < 3; pass++)
            {
                for (var index = 0; index < buttons.Length; index++)
                {
                    if (previous != null)
                        Assert.True(SetMouseOverProbe(previous, false));

                    Assert.True(SetMouseOverProbe(buttons[index], true));
                    previous = buttons[index];
                    visitedCount++;
                    PumpDispatcher(TimeSpan.FromMilliseconds(4));
                }
            }

            if (previous != null)
                Assert.True(SetMouseOverProbe(previous, false));

            Assert.True(PumpSiblingHoverUntilRest(buttons, overlays, TimeSpan.FromSeconds(1)),
                "a synthetic or native hover state remained active after the sibling sweep");
            for (var index = 0; index < overlays.Length; index++)
                maximumResidualOpacity = Math.Max(maximumResidualOpacity, overlays[index].Opacity);

            var hoverReentered = PumpMouseOverProbeUntil(buttons[3], true,
                () => overlays[3].Opacity > 0.05 && overlays[3].Opacity < 0.95,
                TimeSpan.FromMilliseconds(500));
            Assert.True(SetMouseOverProbe(buttons[3], false));
            Assert.True(PumpSiblingHoverUntilRest(buttons, overlays, TimeSpan.FromSeconds(1)),
                "a hover overlay remained active after its final exit");
            for (var index = 0; index < overlays.Length; index++)
                maximumResidualOpacity = Math.Max(maximumResidualOpacity, overlays[index].Opacity);

            return hoverReentered;
        }
        finally
        {
            window?.Close();
        }
    }

    private static bool PumpSiblingHoverUntilRest(UiButton[] buttons, Border[] overlays, TimeSpan timeout)
    {
        var settledFor = Stopwatch.StartNew();
        return PumpDispatcherUntilFast(() =>
        {
            // A native WPF pointer sync can overwrite the synthetic IsMouseOver probe while
            // the fixture renders. Keep every sibling explicitly out of hover while exits run.
            foreach (var button in buttons)
            {
                if (ReadMouseOverProbe(button))
                    Assert.True(SetMouseOverProbe(button, false));
            }

            if (settledFor.Elapsed < TimeSpan.FromMilliseconds(300))
                return false;

            foreach (var overlay in overlays)
            {
                if (overlay.Opacity >= 0.001)
                    return false;
            }

            return true;
        }, timeout);
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

    private static TimeSpan ExtendProductionButtonMotionSamplingWindow(Style style)
    {
        Setter? templateSetter = null;
        for (var currentStyle = style; currentStyle != null && templateSetter == null; currentStyle = currentStyle.BasedOn)
        {
            templateSetter = currentStyle.Setters.OfType<Setter>()
                .FirstOrDefault(setter => setter.Property == Control.TemplateProperty);
        }

        Assert.NotNull(templateSetter);
        var template = Assert.IsType<ControlTemplate>(templateSetter.Value);
        var adjustedStoryboards = 0;
        var adjustedAnimations = 0;
        var animationSamplingWindow = TimeSpan.FromMilliseconds(400);

        foreach (TriggerBase trigger in template.Triggers)
        {
            foreach (var beginStoryboard in trigger.EnterActions.OfType<BeginStoryboard>())
            {
                var animations = beginStoryboard.Storyboard.Children.OfType<DoubleAnimation>().ToArray();
                if (animations.Length == 0)
                    continue;

                // Keep the production storyboards, targets and easing, but lengthen their
                // timelines in this fixture so reversals remain observable on coarse render ticks.
                var sampledStoryboard = beginStoryboard.Storyboard.Clone();
                foreach (var animation in sampledStoryboard.Children.OfType<DoubleAnimation>())
                {
                    animation.Duration = new Duration(animationSamplingWindow);
                    adjustedAnimations++;
                }

                beginStoryboard.Storyboard = sampledStoryboard;
                adjustedStoryboards++;
            }
        }

        Assert.Equal(4, adjustedStoryboards);
        Assert.Equal(8, adjustedAnimations);
        return animationSamplingWindow;
    }

    private static void EnsureApplication()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    private static bool SetMouseOverProbe(UIElement element, bool value)
    {
        // Drive the production IsMouseOver style trigger without mutating the global
        // MouseDevice. The fixture root is non-hit-testable to exclude desktop input.
        element.SetValue(IsMouseOverPropertyKey, value);
        return ReadMouseOverProbe(element) == value;
    }

    private static bool ReadMouseOverProbe(UIElement element)
        => (bool)element.GetValue(UIElement.IsMouseOverProperty);

    private static bool PumpMouseOverProbeUntil(UIElement element, bool value, Func<bool> condition, TimeSpan timeout)
        => PumpDispatcherUntilFast(() =>
        {
            // Native pointer synchronization can replace a synthetic hover while the
            // dispatcher renders. Keep the fixture's requested input state active.
            if (ReadMouseOverProbe(element) != value)
                Assert.True(SetMouseOverProbe(element, value));
            return condition();
        }, timeout);

    private static void SetPressedStateProbe(ButtonBase button, bool value)
    {
        var setIsPressed = typeof(ButtonBase).GetMethod("SetIsPressed", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("WPF ButtonBase.SetIsPressed is unavailable.");
        setIsPressed.Invoke(button, new object[] { value });
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = duration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static bool PumpDispatcherUntil(Func<bool> condition, TimeSpan timeout)
    {
        var timeoutWatch = Stopwatch.StartNew();
        while (timeoutWatch.Elapsed < timeout)
        {
            if (condition())
                return true;

            PumpDispatcher(TimeSpan.FromMilliseconds(40));
        }

        return condition();
    }

    private static bool PumpDispatcherUntilFast(Func<bool> condition, TimeSpan timeout)
    {
        var timeoutWatch = Stopwatch.StartNew();
        while (timeoutWatch.Elapsed < timeout)
        {
            if (condition())
                return true;

            PumpDispatcher(TimeSpan.FromMilliseconds(5));
        }

        return condition();
    }
}
