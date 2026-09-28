using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q10CheckBoxHitBehaviorTests
{
    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SharedCheckboxWrapsLongLabelsAndKeepsBoxTextAndGapInOneActivationSurface(GameSaveCenterThemeMode mode)
    {
        RunSta(() =>
        {
            TestRepositoryContext.AssertAssemblyMatchesSource();
            const string longLabel = "启用第二本地镜像并在备份执行后进行完整性校验，保留另一份本地副本以便在主目录不可用时恢复";
            var checkBox = new CheckBox
            {
                Content = longLabel,
                Width = 220,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            var window = CreateWindow(checkBox, mode);
            var clickCount = 0;
            checkBox.Click += (_, _) => clickCount++;

            try
            {
                ShowAndLayout(window, checkBox);

                var root = Assert.IsAssignableFrom<Visual>(VisualTreeHelper.GetChild(checkBox, 0));
                var box = Assert.IsType<Border>(checkBox.Template!.FindName("Box", checkBox));
                var contentPresenter = FindVisualChild<ContentPresenter>(checkBox);
                var label = FindVisualChild<AccessText>(contentPresenter);

                Assert.True(label.ActualHeight > label.FontSize * 2, "A long plain-text label must wrap within the remaining checkbox width.");

                var boxBounds = box.TransformToAncestor(root).TransformBounds(new Rect(0, 0, box.ActualWidth, box.ActualHeight));
                var labelBounds = label.TransformToAncestor(root).TransformBounds(new Rect(0, 0, label.ActualWidth, label.ActualHeight));
                var points = new[]
                {
                    new Point(boxBounds.Left + boxBounds.Width / 2, boxBounds.Top + boxBounds.Height / 2),
                    new Point(labelBounds.Left + labelBounds.Width / 2, labelBounds.Top + labelBounds.Height / 2),
                    new Point((boxBounds.Right + labelBounds.Left) / 2, boxBounds.Top + boxBounds.Height / 2)
                };
                var invokeClick = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new Xunit.Sdk.XunitException("WPF ButtonBase.OnClick was not found.");

                foreach (var point in points)
                {
                    var hit = VisualTreeHelper.HitTest(root, point)?.VisualHit;
                    Assert.NotNull(hit);
                    Assert.True(IsDescendantOrSelf(hit!, root), "The checkbox box, label, and intervening gap must remain inside the same control hit surface.");

                    var clicksBefore = clickCount;
                    checkBox.IsChecked = false;
                    invokeClick.Invoke(checkBox, null);

                    Assert.True(checkBox.IsChecked);
                    Assert.Equal(clicksBefore + 1, clickCount);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SpaceTogglesOncePerKeyPressAndDisabledCheckboxRejectsTheInput(GameSaveCenterThemeMode mode)
    {
        RunSta(() =>
        {
            TestRepositoryContext.AssertAssemblyMatchesSource();
            var checkBox = new CheckBox { Content = "合成选项", Width = 180 };
            var window = CreateWindow(checkBox, mode);
            var clickCount = 0;
            checkBox.Click += (_, _) => clickCount++;

            try
            {
                ShowAndLayout(window, checkBox);
                window.Activate();
                Assert.Same(checkBox, Keyboard.Focus(checkBox));

                RaiseSpaceKeyPress(checkBox, window);
                Assert.True(checkBox.IsChecked);
                Assert.Equal(1, clickCount);

                RaiseSpaceKeyPress(checkBox, window);
                Assert.False(checkBox.IsChecked);
                Assert.Equal(2, clickCount);

                Keyboard.ClearFocus();
                checkBox.IsEnabled = false;
                Assert.False(checkBox.IsKeyboardFocused);
                Assert.NotSame(checkBox, Keyboard.Focus(checkBox));
                var peer = UIElementAutomationPeer.CreatePeerForElement(checkBox);
                var toggle = Assert.IsAssignableFrom<IToggleProvider>(peer!.GetPattern(PatternInterface.Toggle));
                Assert.Throws<ElementNotEnabledException>(() => toggle.Toggle());
                Assert.False(checkBox.IsChecked);
                Assert.Equal(2, clickCount);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Window CreateWindow(CheckBox checkBox, GameSaveCenterThemeMode mode)
    {
        var window = new Window
        {
            Content = checkBox,
            Width = 260,
            Height = 180,
            ShowInTaskbar = false,
            ShowActivated = true,
            WindowStyle = WindowStyle.ToolWindow,
            Opacity = 0.01
        };
        var resources = new ResourceDictionary();
        resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml", UriKind.Relative)
        });
        var palette = AdaptiveThemePaletteFactory.Create(new Border(), true, 78, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, palette, true, true);
        window.Resources = resources;
        checkBox.Style = (Style)resources["GscCheckBox"];
        return window;
    }

    private static void ShowAndLayout(Window window, CheckBox checkBox)
    {
        window.Show();
        window.UpdateLayout();
        checkBox.ApplyTemplate();
        window.UpdateLayout();
    }

    private static T FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                return match;

            try
            {
                return FindVisualChild<T>(child);
            }
            catch (InvalidOperationException)
            {
            }
        }

        throw new InvalidOperationException("Expected visual child " + typeof(T).Name + " was not found.");
    }

    private static bool IsDescendantOrSelf(DependencyObject candidate, DependencyObject root)
    {
        for (var current = candidate; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, root))
                return true;
        }

        return false;
    }

    private static void RaiseSpaceKeyPress(CheckBox checkBox, Window window)
    {
        var source = PresentationSource.FromVisual(window)
            ?? throw new InvalidOperationException("Checkbox is not connected to a WPF presentation source.");

        checkBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Space)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        });
        checkBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Space)
        {
            RoutedEvent = Keyboard.KeyUpEvent
        });
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
