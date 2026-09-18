using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;
using UiButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class R09FocusOutlineBehaviorTests
{
    [Fact]
    public void FocusedPrimaryButtonKeepsRoundedOverlayAndSharedFocusRingWithNegativeBlurredState()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var button = new UiButton
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]),
                Content = "执行备份",
                Width = 150,
                Height = 48
            };
            var focusRingHost = new Control
            {
                Style = Assert.IsType<Style>(resources["GscSharedFocusVisual"]),
                Width = 150,
                Height = 48,
                IsHitTestVisible = false
            };
            var external = new TextBox { Width = 120, Height = 32 };
            var root = new StackPanel { Resources = resources };
            root.Children.Add(button);
            root.Children.Add(focusRingHost);
            root.Children.Add(external);
            var window = CreateWindow(root, 240, 140);

            try
            {
                window.Show();
                FlushLayout(window);
                Assert.Same(button, Keyboard.Focus(button));
                FlushLayout(window);

                var chrome = Assert.IsType<Border>(button.Template.FindName("ButtonChrome", button));
                var focusOverlay = Assert.IsType<Border>(button.Template.FindName("FocusOverlay", button));
                Assert.True(button.IsKeyboardFocusWithin);
                Assert.Equal(1d, focusOverlay.Opacity);
                Assert.Equal(new CornerRadius(14), focusOverlay.CornerRadius);
                Assert.Equal(chrome.CornerRadius, focusOverlay.CornerRadius);
                Assert.True(chrome.ClipToBounds);
                Assert.False(focusOverlay.IsHitTestVisible);
                Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                Assert.NotNull(button.FocusVisualStyle);

                focusRingHost.ApplyTemplate();
                var focusRing = Assert.IsType<Border>(FindDescendant<Border>(focusRingHost));
                Assert.Equal(new CornerRadius(13), focusRing.CornerRadius);
                Assert.Equal(new Thickness(2), focusRing.BorderThickness);
                Assert.False(focusRing.ClipToBounds);
                Assert.False(focusRing.IsHitTestVisible);

                Assert.Same(external, Keyboard.Focus(external));
                FlushLayout(window);
                Assert.False(button.IsKeyboardFocusWithin);
                Assert.Equal(0d, focusOverlay.Opacity);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var source = new ValidationProbe { Value = "2" };
            var textBox = new TextBox
            {
                Style = Assert.IsType<Style>(resources["GscWpfUiTextBox"]),
                Width = 180,
                Height = 40,
                Focusable = true,
                IsTabStop = true
            };
            var binding = new Binding(nameof(ValidationProbe.Value))
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                NotifyOnValidationError = true
            };
            binding.ValidationRules.Add(new IntegerRangeValidationRule { Minimum = 1, Maximum = 3 });
            textBox.SetBinding(TextBox.TextProperty, binding);

            var tab = new TabItem
            {
                Style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabItem"]),
                Header = "当前页",
                IsSelected = true,
                Width = 120,
                Height = 44
            };
            var tabs = new TabControl { Width = 180, Height = 52 };
            tabs.Items.Add(tab);
            var root = new StackPanel { Resources = resources };
            root.Children.Add(tabs);
            var window = CreateWindow(root, 260, 150);

            try
            {
                window.Show();
                FlushLayout(window);

                tab.ApplyTemplate();
                var tabChrome = Assert.IsType<Border>(tab.Template.FindName("Chrome", tab));
                Assert.True(tab.IsSelected);
                Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(tabChrome.BorderBrush));
                Assert.False(tabChrome.ClipToBounds);
                Assert.NotNull(tab.FocusVisualStyle);

                root.Children.Remove(tabs);
                root.Children.Add(textBox);
                FlushLayout(window);
                var focusedTextBox = Keyboard.Focus(textBox);
                Assert.True(
                    ReferenceEquals(textBox, focusedTextBox),
                    $"focus={focusedTextBox}; visible={textBox.IsVisible}; enabled={textBox.IsEnabled}; hitTest={textBox.IsHitTestVisible}; focusable={textBox.Focusable}; actual={textBox.ActualWidth}x{textBox.ActualHeight}; windowActive={window.IsActive}");
                FlushLayout(window);
                textBox.Text = "9";
                textBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                FlushLayout(window);
                Assert.True(Validation.GetHasError(textBox));
                Assert.True(textBox.IsKeyboardFocusWithin);
                FlushLayout(window);

                var errorChrome = Assert.IsType<Border>(textBox.Template.FindName("Chrome", textBox));
                Assert.Equal(BrushColor(resources["GscErrorTintBrush"]), BrushColor(errorChrome.Background));
                Assert.Equal(BrushColor(resources["GscErrorBrush"]), BrushColor(errorChrome.BorderBrush));
                Assert.Equal(new Thickness(2), errorChrome.BorderThickness);
                Assert.NotNull(textBox.FocusVisualStyle);

                textBox.Text = "2";
                textBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                FlushLayout(window);
                Assert.False(Validation.GetHasError(textBox));
                var recoveredChrome = Assert.IsType<Border>(textBox.Template.FindName("Chrome", textBox));
                Assert.Equal(BrushColor(resources["GscControlFocusFillBrush"]), BrushColor(recoveredChrome.Background));
                Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(recoveredChrome.BorderBrush));
                Assert.Equal(new Thickness(1), recoveredChrome.BorderThickness);
            }
            finally
            {
                window.Close();
            }
        });
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

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(System.Windows.Markup.XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = true,
            Opacity = 0.01
        };

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static Color BrushColor(object? value)
        => Assert.IsType<SolidColorBrush>(value).Color;

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

    private sealed class ValidationProbe
    {
        public string Value { get; set; } = string.Empty;
    }
}

[CollectionDefinition("R09FocusWpf", DisableParallelization = true)]
public sealed class R09FocusWpfCollection
{
}
