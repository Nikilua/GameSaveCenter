using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q10ToggleSwitchStateBehaviorTests
{
    [Fact]
    public void ProductionToggleHonorsRejectedDisabledReversedAndReducedMotionStates()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        foreach (var mode in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
        {
            RunSta(() => VerifyRejectedUpdate(mode));
            RunSta(() => VerifyDisabledToggle(mode));
            RunSta(() => VerifyReversedChanges(mode));
            RunSta(() => VerifyMotionOffDuringTransition(mode));
        }
    }

    private static void VerifyRejectedUpdate(GameSaveCenterThemeMode mode)
    {
        var state = new ToggleState { RejectEnable = true };
        var host = CreateToggle(mode, motionEnabled: true, state);
        try
        {
            ShowAndLayout(host.Window, host.Toggle);

            var attemptsBefore = state.SetAttempts;
            ToggleViaAutomation(host.Toggle);
            Assert.Equal(attemptsBefore + 1, state.SetAttempts);
            Assert.True(state.LastAttemptedValue);
            DrainDispatcher();

            Assert.False(state.Value);
            Assert.False(host.Toggle.IsChecked);
            WaitFor(() => Math.Abs(GetThumbX(host.Thumb)) < 0.1, 750, "The thumb must return to the rejected source value.");
        }
        finally
        {
            host.Window.Close();
        }
    }

    private static void VerifyDisabledToggle(GameSaveCenterThemeMode mode)
    {
        var state = new ToggleState();
        var host = CreateToggle(mode, motionEnabled: true, state);
        try
        {
            ShowAndLayout(host.Window, host.Toggle);
            host.Window.Activate();
            Keyboard.ClearFocus();
            host.Toggle.IsEnabled = false;

            Assert.NotSame(host.Toggle, Keyboard.Focus(host.Toggle));
            var peer = UIElementAutomationPeer.CreatePeerForElement(host.Toggle);
            var toggle = Assert.IsAssignableFrom<IToggleProvider>(peer!.GetPattern(PatternInterface.Toggle));
            Assert.Throws<ElementNotEnabledException>(() => toggle.Toggle());
            Assert.False(host.Toggle.IsChecked);
            Assert.False(state.Value);
        }
        finally
        {
            host.Window.Close();
        }
    }

    private static void VerifyReversedChanges(GameSaveCenterThemeMode mode)
    {
        var state = new ToggleState();
        var host = CreateToggle(mode, motionEnabled: true, state);
        try
        {
            var motionAvailable = Assert.IsType<bool>(host.Window.Resources["GscToggleMotionEnabled"]);
            if (motionAvailable)
                host.Window.Resources["GscToggleMotionFast"] = new Duration(TimeSpan.FromMilliseconds(500));
            ShowAndLayout(host.Window, host.Toggle);

            ToggleViaAutomation(host.Toggle);
            if (motionAvailable)
            {
                Assert.True(state.Value);
                Assert.True(host.Toggle.IsChecked);
                AssertToggleAnimationActive(host);
                WaitFor(
                    () =>
                    {
                        host.Window.UpdateLayout();
                        return GetThumbX(host.Thumb) > 1 && GetThumbX(host.Thumb) < 16;
                    },
                    400,
                    $"The production toggle animation must expose an in-flight visual value. X={GetThumbX(host.Thumb):0.###}, transform={GetMotionTransformState(host.Toggle, host.Thumb)}.");
            }
            ToggleViaAutomation(host.Toggle);
            Assert.False(state.Value);
            Assert.False(host.Toggle.IsChecked);
            if (motionAvailable) AssertToggleAnimationActive(host);
            ToggleViaAutomation(host.Toggle);
            DrainDispatcher();

            Assert.True(state.Value);
            Assert.True(host.Toggle.IsChecked);
            WaitFor(() =>
            {
                host.Window.UpdateLayout();
                return Math.Abs(GetThumbX(host.Thumb) - 17) < 0.1
                    && !host.Thumb.RenderTransform.HasAnimatedProperties;
            }, 1200, "The final transition must reach the latest checked target and release its animation clock.");
        }
        finally
        {
            host.Window.Close();
        }
    }

    private static void VerifyMotionOffDuringTransition(GameSaveCenterThemeMode mode)
    {
        var state = new ToggleState();
        var host = CreateToggle(mode, motionEnabled: true, state);
        try
        {
            var motionAvailable = Assert.IsType<bool>(host.Window.Resources["GscToggleMotionEnabled"]);
            if (motionAvailable)
                host.Window.Resources["GscToggleMotionFast"] = new Duration(TimeSpan.FromSeconds(2));
            ShowAndLayout(host.Window, host.Toggle);

            ToggleViaAutomation(host.Toggle);
            if (motionAvailable)
                AssertToggleAnimationActive(host);

            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Window.Resources, host.Palette, glassEnabled: true, motionEnabled: false);
            DrainDispatcher();

            Assert.Equal(TimeSpan.Zero, Assert.IsType<Duration>(host.Window.Resources["GscToggleMotionFast"]).TimeSpan);
            Assert.True(state.Value);
            Assert.True(host.Toggle.IsChecked);
            WaitFor(() =>
            {
                host.Window.UpdateLayout();
                return Math.Abs(GetThumbX(host.Thumb) - 17) < 0.1
                    && !host.Thumb.RenderTransform.HasAnimatedProperties;
            }, 500, "Disabling motion must snap to the latest checked target and release its animation clock.");
        }
        finally
        {
            host.Window.Close();
        }
    }

    private static ToggleHost CreateToggle(GameSaveCenterThemeMode mode, bool motionEnabled, ToggleState state)
    {
        var resources = (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
        var paletteHost = new Border();
        var palette = AdaptiveThemePaletteFactory.Create(paletteHost, true, 78, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, palette, glassEnabled: true, motionEnabled: motionEnabled);

        var toggle = new ToggleSwitch
        {
            Content = "隔离选项",
            OnContent = "开",
            OffContent = "关",
            Style = (Style)resources["GscWpfUiToggleSwitch"]
        };
        toggle.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(ToggleState.Value))
        {
            Source = state,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        var window = new Window
        {
            Content = toggle,
            Resources = resources,
            Width = 320,
            Height = 180,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        return new ToggleHost(window, toggle, palette);
    }

    private static void ShowAndLayout(Window window, ToggleSwitch toggle)
    {
        window.Show();
        window.UpdateLayout();
        toggle.ApplyTemplate();
        window.UpdateLayout();
    }

    private static void ToggleViaAutomation(ToggleSwitch toggle)
    {
        var peer = UIElementAutomationPeer.CreatePeerForElement(toggle);
        var provider = Assert.IsAssignableFrom<IToggleProvider>(peer!.GetPattern(PatternInterface.Toggle));
        provider.Toggle();
    }

    private static double GetThumbX(Ellipse thumb)
        => ((TranslateTransform)thumb.RenderTransform).X;

    private static void AssertToggleAnimationActive(ToggleHost host)
    {
        Assert.True(host.Thumb.RenderTransform.HasAnimatedProperties, $"The production toggle thumb must have an active WPF animation clock. X={GetThumbX(host.Thumb):0.###}, transform={GetMotionTransformState(host.Toggle, host.Thumb)}.");
    }

    private static string GetMotionTransformState(ToggleSwitch toggle, Ellipse thumb)
    {
        var field = typeof(ToggleSwitch).GetField("thumbTransform", BindingFlags.Instance | BindingFlags.NonPublic);
        var motionTransform = field?.GetValue(toggle);
        return $"same={ReferenceEquals(motionTransform, thumb.RenderTransform)}, frozen={(thumb.RenderTransform as Freezable)?.IsFrozen}, animated={thumb.RenderTransform.HasAnimatedProperties}";
    }

    private static void WaitFor(Func<bool> condition, int timeoutMilliseconds, string message)
    {
        var timer = Stopwatch.StartNew();
        while (!condition() && timer.ElapsedMilliseconds < timeoutMilliseconds)
            PumpDispatcher(TimeSpan.FromMilliseconds(40));
        Assert.True(condition(), message);
    }

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
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                try { Dispatcher.CurrentDispatcher.InvokeShutdown(); } catch { }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class ToggleHost
    {
        public ToggleHost(Window window, ToggleSwitch toggle, AdaptiveThemePalette palette)
        {
            Window = window;
            Toggle = toggle;
            Palette = palette;
        }

        public Window Window { get; }
        public ToggleSwitch Toggle { get; }
        public AdaptiveThemePalette Palette { get; }
        public Ellipse Thumb => (Ellipse)Toggle.Template!.FindName("Thumb", Toggle)!;
    }

    private sealed class ToggleState : INotifyPropertyChanged
    {
        private bool value;

        public bool RejectEnable { get; set; }
        public int SetAttempts { get; private set; }
        public bool LastAttemptedValue { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Value
        {
            get => value;
            set
            {
                SetAttempts++;
                LastAttemptedValue = value;
                if (RejectEnable && value)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    return;
                }
                if (this.value == value) return;
                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }
}
