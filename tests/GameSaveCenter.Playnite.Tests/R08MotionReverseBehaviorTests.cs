using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
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
    private static readonly TimeSpan ButtonMotionSamplingWindow = TimeSpan.FromMilliseconds(800);
    private static readonly TimeSpan ButtonMotionTransitionTimeout = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan ButtonMotionCompletionTimeout = TimeSpan.FromMilliseconds(2000);

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
    public void SamplingDeadlineReturnsBeforeQueuedRenderWorkDrains()
    {
        Exception? exception = null;
        var renderTurns = 0;
        var thread = new Thread(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            DispatcherOperation? pendingRender = null;
            var stop = false;
            Action? renderWork = null;
            renderWork = () =>
            {
                if (stop)
                    return;

                renderTurns++;
                // Ensure the one-millisecond sampling deadline becomes due while
                // a finite render queue still has work. This tests dispatcher
                // ordering rather than imposing a machine-speed threshold.
                Thread.Sleep(5);
                if (renderTurns < 40)
                    pendingRender = dispatcher.BeginInvoke(DispatcherPriority.Render, renderWork!);
            };
            try
            {
                pendingRender = dispatcher.BeginInvoke(DispatcherPriority.Render, renderWork);
                PumpDispatcher(TimeSpan.FromMilliseconds(1));
                Assert.True(renderTurns < 40,
                    $"the sampling deadline waited for all {renderTurns} queued render turns");
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                stop = true;
                pendingRender?.Abort();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        output.WriteLine($"render turns before sampling deadline: {renderTurns}/40");
        Assert.Null(exception);
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
        MouseOverSamplingTrace hoverEntryTrace = default;
        MouseOverSamplingTrace hoverExitTrace = default;
        MouseOverSamplingTrace hoverReentryTrace = default;
        var pressedOverlayMidpoint = 0d;
        var pressedOverlayAfterRelease = 0d;
        var pressedOverlayReleaseProgress = 0d;
        var pressedOverlayBeforeRepress = 0d;
        var pressedOverlayAfterRepress = 0d;
        var pressedOverlayRepressProgress = 0d;
        var pressedOverlayFinal = 0d;
        var pressedOverlayReleasedFinal = 0d;
        var pressMidpointSampled = false;
        var pressReleasedContinuously = false;
        var pressReleaseProgressSampled = false;
        var pressReversedContinuously = false;
        var pressRepressProgressSampled = false;
        var pressTransformStayedIdentity = false;
        PressedRenderingTrace pressEntryTrace = default;
        PressedRenderingTrace pressExitTrace = default;
        PressedRenderingTrace pressReentryTrace = default;
        PressedRenderingTrace pressFinalPressTrace = default;
        PressedRenderingTrace pressFinalReleaseTrace = default;
        var pressFinalPressCompleted = false;
        var pressFinalReleaseCompleted = false;
        var hoverSweepVisitedCount = 0;
        var hoverSweepMaximumResidualOpacity = 0d;
        var hoverSweepLightReentered = false;
        var hoverSweepDarkReentered = false;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
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

                PumpDispatcher(animationSamplingWindow + TimeSpan.FromMilliseconds(40));
                Assert.False(ReadMouseOverProbe(button), "button unexpectedly entered hover before the synthetic probe");
                Assert.Equal(0, hoverOverlay.Opacity, 3);

                hoverMidpointSampled = ObserveMouseOverTransition(button, true, () => { },
                    () => hoverOverlay.Opacity > 0.05 && hoverOverlay.Opacity < 0.95,
                    ButtonMotionTransitionTimeout, out hoverEntryTrace);
                hoverMidpoint = hoverOverlay.Opacity;
                hoverExitProgressSampled = ObserveMouseOverTransition(button, false,
                    () => hoverAfterLeave = hoverOverlay.Opacity,
                    () => hoverOverlay.Opacity < hoverAfterLeave - 0.002 && hoverOverlay.Opacity > 0,
                    ButtonMotionTransitionTimeout, out hoverExitTrace);
                hoverExitProgress = hoverOverlay.Opacity;
                hoverBeforeReenter = hoverOverlay.Opacity;
                hoverReenterProgressSampled = ObserveMouseOverTransition(button, true,
                    () => hoverAfterReenter = hoverOverlay.Opacity,
                    () => hoverOverlay.Opacity > hoverAfterReenter + 0.002 && hoverOverlay.Opacity < 1,
                    ButtonMotionTransitionTimeout, out hoverReentryTrace);
                hoverReenterProgress = hoverOverlay.Opacity;
                PumpMouseOverProbeUntil(button, true, () => Math.Abs(hoverOverlay.Opacity - 1) < 0.001, ButtonMotionCompletionTimeout);
                hoverFinal = hoverOverlay.Opacity;

                pressMidpointSampled = ObserveButtonPressedTransition(button, pressedOverlay, true, () => { },
                    () => pressedOverlay.Opacity > 0.01 && pressedOverlay.Opacity < 0.99,
                    ButtonMotionCompletionTimeout, out pressEntryTrace);
                pressedOverlayMidpoint = pressedOverlay.Opacity;
                Assert.True(button.IsPressed, "synthetic state probe did not enter the production button's pressed state.");
                pressTransformStayedIdentity = chrome.RenderTransform.Value.IsIdentity
                    && !DependencyPropertyHelper.GetValueSource(chrome, UIElement.RenderTransformProperty).IsAnimated;
                pressReleaseProgressSampled = ObserveButtonPressedTransition(button, pressedOverlay, false,
                    () =>
                    {
                        pressedOverlayAfterRelease = pressedOverlay.Opacity;
                        pressReleasedContinuously = Math.Abs(pressedOverlayAfterRelease - pressedOverlayMidpoint) <= 0.08;
                    },
                    () => pressedOverlay.Opacity < pressedOverlayAfterRelease - 0.002 && pressedOverlay.Opacity > 0,
                    ButtonMotionTransitionTimeout, out pressExitTrace);
                Assert.False(button.IsPressed, "synthetic state probe did not leave the production button's pressed state.");
                pressedOverlayReleaseProgress = pressedOverlay.Opacity;
                pressedOverlayBeforeRepress = pressedOverlay.Opacity;
                pressRepressProgressSampled = ObserveButtonPressedTransition(button, pressedOverlay, true,
                    () =>
                    {
                        pressedOverlayAfterRepress = pressedOverlay.Opacity;
                        pressReversedContinuously = Math.Abs(pressedOverlayAfterRepress - pressedOverlayBeforeRepress) <= 0.08;
                        pressTransformStayedIdentity &= chrome.RenderTransform.Value.IsIdentity
                            && !DependencyPropertyHelper.GetValueSource(chrome, UIElement.RenderTransformProperty).IsAnimated;
                    },
                    () => pressedOverlay.Opacity > pressedOverlayAfterRepress + 0.002 && pressedOverlay.Opacity < 1,
                    ButtonMotionTransitionTimeout, out pressReentryTrace);
                Assert.True(button.IsPressed, "Second Space key down did not re-enter the pressed state.");
                pressedOverlayRepressProgress = pressedOverlay.Opacity;
                pressFinalPressCompleted = ObserveButtonPressedTransition(button, pressedOverlay, true, () => { },
                    () => Math.Abs(pressedOverlay.Opacity - 1) < 0.001,
                    ButtonMotionCompletionTimeout, out pressFinalPressTrace);
                pressedOverlayFinal = pressedOverlay.Opacity;
                pressFinalReleaseCompleted = ObserveButtonPressedTransition(button, pressedOverlay, false, () => { },
                    () => Math.Abs(pressedOverlay.Opacity) < 0.001,
                    ButtonMotionCompletionTimeout, out pressFinalReleaseTrace);
                Assert.False(button.IsPressed, "second synthetic press did not leave the production button's pressed state.");
                pressedOverlayReleasedFinal = pressedOverlay.Opacity;
                pressTransformStayedIdentity &= chrome.RenderTransform.Value.IsIdentity
                    && !DependencyPropertyHelper.GetValueSource(chrome, UIElement.RenderTransformProperty).IsAnimated;

                hoverSweepLightReentered = RunProductionButtonHoverSweep(resources, buttonStyle,
                    GameSaveCenterThemeMode.Light, out var lightVisitedCount, out var lightResidualOpacity);
                hoverSweepDarkReentered = RunProductionButtonHoverSweep(resources, buttonStyle,
                    GameSaveCenterThemeMode.Dark, out var darkVisitedCount, out var darkResidualOpacity);
                hoverSweepVisitedCount = lightVisitedCount + darkVisitedCount;
                hoverSweepMaximumResidualOpacity = Math.Max(lightResidualOpacity, darkResidualOpacity);
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

        output.WriteLine($"button hover midpoint={hoverMidpoint:0.###}; exit={hoverExitProgress:0.###}/{hoverAfterLeave:0.###}; reenter={hoverReenterProgress:0.###}/{hoverBeforeReenter:0.###}; final={hoverFinal:0.###}; samples={hoverMidpointSampled}/{hoverExitProgressSampled}/{hoverReenterProgressSampled}; pressed overlay midpoint={pressedOverlayMidpoint:0.###}; release={pressedOverlayAfterRelease:0.###}->{pressedOverlayBeforeRepress:0.###}; re-press={pressedOverlayAfterRepress:0.###}->{pressedOverlayFinal:0.###}; releaseProgress={pressedOverlayReleaseProgress:0.###}; re-pressProgress={pressedOverlayRepressProgress:0.###}; releasedFinal={pressedOverlayReleasedFinal:0.###}; transformIdentity={pressTransformStayedIdentity}; transitions={pressMidpointSampled}/{pressReleaseProgressSampled}/{pressRepressProgressSampled}; light/dark sibling sweep visited={hoverSweepVisitedCount}; max residual={hoverSweepMaximumResidualOpacity:0.####}; re-entry={hoverSweepLightReentered}/{hoverSweepDarkReentered}");
        Assert.Null(exception);
        output.WriteLine($"hover rendering sampler entry={hoverEntryTrace}; exit={hoverExitTrace}; reentry={hoverReentryTrace}");
        output.WriteLine($"pressed rendering sampler entry={pressEntryTrace}; release={pressExitTrace}; re-entry={pressReentryTrace}; final press={pressFinalPressTrace}; final release={pressFinalReleaseTrace}");
        Assert.True(hoverMidpointSampled, "production hover opacity did not expose an in-flight sample");
        Assert.InRange(Math.Abs(hoverAfterLeave - hoverMidpoint), 0, 0.08);
        Assert.InRange(Math.Abs(hoverAfterReenter - hoverBeforeReenter), 0, 0.08);
        Assert.True(hoverExitProgressSampled, "production hover exit did not expose an in-flight sample");
        Assert.True(hoverReenterProgressSampled, "production hover re-entry did not expose an in-flight sample");
        Assert.True(hoverExitProgress < hoverAfterLeave, "hover exit did not continue toward zero");
        Assert.True(hoverReenterProgress > hoverAfterReenter, "hover re-entry did not continue toward one");
        Assert.Equal(1, hoverFinal, 2);
        Assert.True(pressMidpointSampled, "production press opacity did not expose an in-flight sample");
        Assert.True(pressReleasedContinuously, "production press release jumped away from the rendered overlay opacity");
        Assert.True(pressReleaseProgressSampled, "production press release did not expose an in-flight return sample");
        Assert.True(pressedOverlayReleaseProgress < pressedOverlayAfterRelease, "press release did not continue toward the neutral overlay opacity");
        Assert.True(pressReversedContinuously, "production rapid re-press jumped away from the rendered overlay opacity");
        Assert.True(pressRepressProgressSampled, "production rapid re-press did not expose an in-flight sample");
        Assert.True(pressTransformStayedIdentity, "shared press feedback must not scale or translate the button while its overlay animates");
        Assert.True(pressedOverlayMidpoint > 0 && pressedOverlayMidpoint < 1, "production pressed overlay did not expose an in-flight sample");
        Assert.InRange(Math.Abs(pressedOverlayAfterRelease - pressedOverlayMidpoint), 0, 0.08);
        Assert.True(pressedOverlayReleaseProgress < pressedOverlayAfterRelease, "pressed overlay release did not continue toward zero");
        Assert.InRange(Math.Abs(pressedOverlayAfterRepress - pressedOverlayBeforeRepress), 0, 0.08);
        Assert.True(pressedOverlayRepressProgress > pressedOverlayAfterRepress, "pressed overlay re-press did not continue toward one");
        Assert.True(pressFinalPressCompleted, "production final press did not render to its pressed value");
        Assert.Equal(1, pressedOverlayFinal, 2);
        Assert.True(pressFinalReleaseCompleted, "production final press release did not render to its neutral value");
        Assert.Equal(0, pressedOverlayReleasedFinal, 2);
        Assert.Equal(48, hoverSweepVisitedCount);
        Assert.True(hoverSweepLightReentered, "light theme production hover did not respond after a rapid sibling sweep");
        Assert.True(hoverSweepDarkReentered, "dark theme production hover did not respond after a rapid sibling sweep");
        Assert.InRange(hoverSweepMaximumResidualOpacity, 0, 0.001);
    }

    private static bool RunProductionButtonHoverSweep(
        ResourceDictionary resources,
        Style buttonStyle,
        GameSaveCenterThemeMode themeMode,
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
            var palette = AdaptiveThemePaletteFactory.Create(root, true, 78, themeMode);
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

            Assert.True(PumpSiblingHoverUntilRest(buttons, overlays, ButtonMotionCompletionTimeout),
                "a synthetic or native hover state remained active after the sibling sweep");
            for (var index = 0; index < overlays.Length; index++)
                maximumResidualOpacity = Math.Max(maximumResidualOpacity, overlays[index].Opacity);

            var hoverReentered = PumpMouseOverProbeUntil(buttons[3], true,
                () => overlays[3].Opacity > 0.05 && overlays[3].Opacity < 0.95,
                ButtonMotionTransitionTimeout);
            Assert.True(SetMouseOverProbe(buttons[3], false));
            Assert.True(PumpSiblingHoverUntilRest(buttons, overlays, ButtonMotionCompletionTimeout),
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
        var animationSamplingWindow = ButtonMotionSamplingWindow;

        foreach (TriggerBase trigger in template.Triggers)
        {
            foreach (var beginStoryboard in trigger.EnterActions.OfType<BeginStoryboard>())
            {
                var animations = beginStoryboard.Storyboard.Children.OfType<DoubleAnimation>().ToArray();
                if (animations.Length == 0)
                    continue;

                // Keep production storyboard targets and easing, but lengthen only this fixture's
                // timelines so in-flight reversal samples survive delayed offscreen render ticks.
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
        Assert.Equal(4, adjustedAnimations);
        return animationSamplingWindow;
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

    private struct MouseOverSamplingTrace
    {
        public int Frames;
        public int Resets;
        public double ElapsedMilliseconds;
        public override string ToString() => $"frames={Frames},resets={Resets},elapsed={ElapsedMilliseconds:0.###}ms";
    }

    private struct PressedRenderingTrace
    {
        public int Frames;
        public int Resets;
        public double ElapsedMilliseconds;
        public double LastOpacity;
        public override string ToString() => $"frames={Frames},resets={Resets},elapsed={ElapsedMilliseconds:0.###}ms,opacity={LastOpacity:0.###}";
    }

    private static bool ObserveMouseOverTransition(UIElement element, bool value, Action afterSet,
        Func<bool> condition, TimeSpan timeout, out MouseOverSamplingTrace trace)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = timeout };
        var elapsed = new Stopwatch();
        var armed = false;
        var sampled = false;
        var frames = 0;
        var resets = 0;
        EventHandler rendering = (_, _) =>
        {
            if (!armed) return;
            frames++;
            if (ReadMouseOverProbe(element) != value)
            {
                resets++;
                Assert.True(SetMouseOverProbe(element, value));
            }
            if (condition())
            {
                sampled = true;
                frame.Continue = false;
            }
        };
        EventHandler deadline = (_, _) => frame.Continue = false;
        CompositionTarget.Rendering += rendering;
        timer.Tick += deadline;
        try
        {
            // Attach the observer before changing state so setup and cold sampler work
            // cannot consume the first rendered intermediate value between polling turns.
            Assert.True(SetMouseOverProbe(element, value));
            afterSet();
            elapsed.Start();
            armed = true;
            if (condition()) sampled = true;
            else
            {
                timer.Start();
                Dispatcher.PushFrame(frame);
            }
            return sampled;
        }
        finally
        {
            armed = false;
            timer.Stop();
            timer.Tick -= deadline;
            CompositionTarget.Rendering -= rendering;
            trace = new MouseOverSamplingTrace { Frames = frames, Resets = resets, ElapsedMilliseconds = elapsed.Elapsed.TotalMilliseconds };
        }
    }

    private static bool ObserveButtonPressedTransition(ButtonBase button, Border overlay, bool value, Action afterSet,
        Func<bool> condition, TimeSpan timeout, out PressedRenderingTrace trace)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = timeout };
        var elapsed = new Stopwatch();
        var armed = false;
        var sampled = false;
        var frames = 0;
        var resets = 0;
        var lastOpacity = overlay.Opacity;
        EventHandler rendering = (_, _) =>
        {
            if (!armed) return;
            frames++;
            if (button.IsPressed != value)
            {
                resets++;
                SetPressedStateProbe(button, value);
            }
            lastOpacity = overlay.Opacity;
            if (condition())
            {
                sampled = true;
                frame.Continue = false;
            }
        };
        EventHandler deadline = (_, _) => frame.Continue = false;
        CompositionTarget.Rendering += rendering;
        timer.Tick += deadline;
        try
        {
            SetPressedStateProbe(button, value);
            afterSet();
            elapsed.Start();
            armed = true;
            lastOpacity = overlay.Opacity;
            if (condition()) sampled = true;
            else
            {
                timer.Start();
                Dispatcher.PushFrame(frame);
            }
            return sampled;
        }
        finally
        {
            armed = false;
            timer.Stop();
            timer.Tick -= deadline;
            CompositionTarget.Rendering -= rendering;
            trace = new PressedRenderingTrace { Frames = frames, Resets = resets, ElapsedMilliseconds = elapsed.Elapsed.TotalMilliseconds, LastOpacity = lastOpacity };
        }
    }

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
    {
        var window = new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        AutomationProperties.SetName(window, "动效反转行为测试宿主");
        return window;
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        // A Background deadline can wait behind every pending Render operation,
        // turning a five-millisecond sample into an entire animation. End the
        // nested frame as soon as the deadline is due; render work can still run
        // between samples. Production timelines and assertions stay unchanged.
        var timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = duration };
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
