using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;
using UiButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

[Collection("Q06ButtonStateSequenceWpf")]
public sealed class Q06ButtonStateSequenceBehaviorTests
{
    private static readonly IReadOnlyList<ButtonStateSpec> States = new[]
    {
        new ButtonStateSpec("normal", "默认可用，未输入"),
        new ButtonStateSpec("hover", "WPF 合成悬停状态探针（非物理鼠标）"),
        new ButtonStateSpec("pressed", "合成 Space 键按下"),
        new ButtonStateSpec("focus", "Keyboard.Focus 程序化焦点"),
        new ButtonStateSpec("disabled", "假命令 CanExecute=false")
    };

    [Fact]
    public void ProductionButtonCapturesAndChecksFiveStatesInLightAndDark()
    {
        var captures = new List<StateCapture>();
        var outputRoot = Environment.GetEnvironmentVariable("GSC_Q06_STATE_OUTPUT");

        RunSta(() =>
        {
            EnsureApplication();
            var resources = LoadProductionResources();
            foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
            {
                foreach (var state in States)
                    captures.Add(CaptureState(resources, theme, state, outputRoot));
            }

            if (!string.IsNullOrWhiteSpace(outputRoot))
            {
                Directory.CreateDirectory(outputRoot!);
                var report = new StringBuilder();
                report.AppendLine("Q06-08 production button state probe; STA WPF Window; 96-DPI logical capture.");
                report.AppendLine("Hover uses WPF MouseDevice.ChangeMouseOver(button); it is a controlled state probe, not physical mouse input.");
                foreach (var capture in captures)
                    report.AppendLine(capture.ToReportLine());
                File.WriteAllText(Path.Combine(outputRoot!, "button-state-probe-report.txt"), report.ToString());
            }
        });

        Assert.Equal(10, captures.Count);
        Assert.Equal(10, captures.Select(capture => capture.Theme + "/" + capture.State).Distinct().Count());
        Assert.All(captures, capture => Assert.True(capture.StatePassed, capture.ToReportLine()));

        if (!string.IsNullOrWhiteSpace(outputRoot))
        {
            Assert.Equal(10, Directory.GetFiles(outputRoot!, "*.png").Length);
            Assert.True(File.Exists(Path.Combine(outputRoot!, "button-state-probe-report.txt")));
            Assert.All(captures, capture => Assert.True(new FileInfo(capture.Path!).Length > 0, capture.ToReportLine()));
        }
    }

    private static StateCapture CaptureState(
        ResourceDictionary resources,
        GameSaveCenterThemeMode theme,
        ButtonStateSpec state,
        string? outputRoot)
    {
        var root = new Grid
        {
            Width = 420,
            Height = 126,
            Resources = resources,
            Background = Brushes.Transparent
        };
        var palette = AdaptiveThemePaletteFactory.Create(root, true, 78, theme);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(root.Resources, palette, true, true);
        root.Background = CreateOpaqueSampleBackground(palette);

        var activationText = $"主题 {theme} · 状态 {state.Name} · 激活方式：{state.Activation}";
        var activation = new TextBlock
        {
            Text = activationText,
            Margin = new Thickness(14, 8, 14, 8),
            FontSize = 13,
            Foreground = root.TryFindResource("GscPrimaryTextBrush") as Brush ?? Brushes.White,
            TextWrapping = TextWrapping.Wrap
        };
        var button = new UiButton
        {
            Content = "执行安全操作",
            Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]),
            Width = 210,
            Height = 40,
            Margin = new Thickness(14, 0, 14, 8)
        };
        AutomationProperties.SetName(button, "执行安全操作");
        var focusProxy = new Border
        {
            Width = 1,
            Height = 1,
            Opacity = 0,
            Focusable = true,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        var content = new StackPanel();
        content.Children.Add(activation);
        content.Children.Add(button);
        root.Children.Add(content);
        root.Children.Add(focusProxy);

        var window = new Window
        {
            Content = root,
            Width = 436,
            Height = 166,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = true,
            Opacity = 0.01
        };
        try
        {
            if (state.Name == "disabled")
            {
                button.Command = new DeniedCommand();
                AutomationProperties.SetHelpText(button, "此夹具验证 CanExecute=false 的禁用状态。");
            }

            window.Show();
            FlushLayout(window);
            button.ApplyTemplate();
            Assert.True(Keyboard.Focus(focusProxy) == focusProxy);
            FlushLayout(window);

            var hoverSet = false;
            switch (state.Name)
            {
                case "normal":
                    SetMouseOverProbe(button, false);
                    break;
                case "hover":
                    hoverSet = SetMouseOverProbe(button, true);
                    break;
                case "pressed":
                    Assert.True(Keyboard.Focus(button) == button);
                    var source = PresentationSource.FromVisual(button)
                        ?? throw new InvalidOperationException("Button is not connected to a WPF PresentationSource.");
                    button.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Space)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    });
                    break;
                case "focus":
                    Assert.True(Keyboard.Focus(button) == button);
                    break;
                case "disabled":
                    Assert.True(Keyboard.Focus(focusProxy) == focusProxy);
                    button.Command!.CanExecute(null);
                    break;
                default:
                    throw new InvalidOperationException("Unknown button state: " + state.Name);
            }

            PumpDispatcher(TimeSpan.FromMilliseconds(180));
            FlushLayout(window);
            if (state.Name == "hover")
            {
                // Reapply WPF's own mouse-over transition after layout has recomputed device state.
                hoverSet = SetMouseOverProbe(button, true) && hoverSet;
            }

            var chrome = Assert.IsType<Border>(button.Template!.FindName("ButtonChrome", button));
            var hoverOverlay = Assert.IsType<Border>(button.Template.FindName("HoverOverlay", button));
            var pressedOverlay = Assert.IsType<Border>(button.Template.FindName("PressedOverlay", button));
            var focusOverlay = Assert.IsType<Border>(button.Template.FindName("FocusOverlay", button));
            var scale = Assert.IsType<ScaleTransform>(chrome.RenderTransform);

            var statePassed = state.Name switch
            {
                "normal" => button.IsEnabled && !button.IsMouseOver && !button.IsPressed
                    && !button.IsKeyboardFocusWithin && hoverOverlay.Opacity == 0
                    && pressedOverlay.Opacity == 0 && focusOverlay.Opacity == 0
                    && Math.Abs(scale.ScaleX - 1) < 0.01 && Math.Abs(chrome.Opacity - 1) < 0.01,
                "hover" => hoverSet && button.IsMouseOver && hoverOverlay.Opacity > 0.9
                    && !button.IsPressed && chrome.BorderBrush != null,
                "pressed" => button.IsPressed && pressedOverlay.Opacity > 0.9
                    && scale.ScaleX < 0.99 && button.IsKeyboardFocusWithin,
                "focus" => button.IsKeyboardFocusWithin && focusOverlay.Opacity == 1
                    && button.FocusVisualStyle != null,
                "disabled" => !button.IsEnabled && !button.Command!.CanExecute(null)
                    && Math.Abs(chrome.Opacity - 0.72) < 0.01,
                _ => false
            };

            if (state.Name == "disabled")
            {
                InvokeFrameworkClick(button);
                Assert.Equal(0, ((DeniedCommand)button.Command!).ExecutionCount);
            }

            string? path = null;
            if (!string.IsNullOrWhiteSpace(outputRoot))
            {
                Directory.CreateDirectory(outputRoot!);
                path = Path.Combine(outputRoot!, $"{theme.ToString().ToLowerInvariant()}-{state.Name}.png");
                SavePng(root, path);
            }

            return new StateCapture(
                theme.ToString(),
                state.Name,
                state.Activation,
                statePassed,
                button.IsEnabled,
                button.IsMouseOver,
                button.IsPressed,
                button.IsKeyboardFocusWithin,
                hoverOverlay.Opacity,
                pressedOverlay.Opacity,
                focusOverlay.Opacity,
                chrome.Opacity,
                scale.ScaleX,
                path);
        }
        finally
        {
            window.Close();
        }
    }

    private static bool SetMouseOverProbe(UIElement element, bool value)
    {
        // Drive WPF's own hover state transition without moving the user's OS cursor.
        var changeMouseOver = typeof(MouseDevice).GetMethod("ChangeMouseOver", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("WPF MouseDevice.ChangeMouseOver is unavailable.");
        changeMouseOver.Invoke(Mouse.PrimaryDevice, new object?[] { value ? element : null, Environment.TickCount });
        return element is FrameworkElement frameworkElement
            && frameworkElement.IsMouseOver == value;
    }

    private static Brush CreateOpaqueSampleBackground(AdaptiveThemePalette palette)
    {
        var color = palette.Backdrop;
        return new SolidColorBrush(Color.FromArgb(byte.MaxValue, color.R, color.G, color.B));
    }

    private static void SavePng(FrameworkElement element, string path)
    {
        var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void InvokeFrameworkClick(UiButton button)
        => typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, Array.Empty<object>());

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static void EnsureApplication()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher)
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
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class ButtonStateSpec
    {
        public ButtonStateSpec(string name, string activation)
        {
            Name = name;
            Activation = activation;
        }

        public string Name { get; }
        public string Activation { get; }
    }

    private sealed class StateCapture
    {
        public StateCapture(
            string theme,
            string state,
            string activation,
            bool statePassed,
            bool enabled,
            bool mouseOver,
            bool pressed,
            bool focusWithin,
            double hoverOpacity,
            double pressedOpacity,
            double focusOpacity,
            double chromeOpacity,
            double scaleX,
            string? path)
        {
            Theme = theme;
            State = state;
            Activation = activation;
            StatePassed = statePassed;
            Enabled = enabled;
            MouseOver = mouseOver;
            Pressed = pressed;
            FocusWithin = focusWithin;
            HoverOpacity = hoverOpacity;
            PressedOpacity = pressedOpacity;
            FocusOpacity = focusOpacity;
            ChromeOpacity = chromeOpacity;
            ScaleX = scaleX;
            Path = path;
        }

        public string Theme { get; }
        public string State { get; }
        public string Activation { get; }
        public bool StatePassed { get; }
        public bool Enabled { get; }
        public bool MouseOver { get; }
        public bool Pressed { get; }
        public bool FocusWithin { get; }
        public double HoverOpacity { get; }
        public double PressedOpacity { get; }
        public double FocusOpacity { get; }
        public double ChromeOpacity { get; }
        public double ScaleX { get; }
        public string? Path { get; }

        public string ToReportLine()
            => $"theme={Theme}; state={State}; activation={Activation}; passed={StatePassed}; enabled={Enabled}; "
                + $"mouseOver={MouseOver}; pressed={Pressed}; focus={FocusWithin}; hoverOverlay={HoverOpacity:0.###}; "
                + $"pressedOverlay={PressedOpacity:0.###}; focusOverlay={FocusOpacity:0.###}; "
                + $"chromeOpacity={ChromeOpacity:0.###}; scaleX={ScaleX:0.###}; png={Path ?? "not-requested"}";
    }

    private sealed class DeniedCommand : ICommand
    {
        public int ExecutionCount { get; private set; }

        public bool CanExecute(object? parameter) => false;
        public void Execute(object? parameter) => ExecutionCount++;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}

[CollectionDefinition("Q06ButtonStateSequenceWpf", DisableParallelization = true)]
public sealed class Q06ButtonStateSequenceWpfCollection
{
}
