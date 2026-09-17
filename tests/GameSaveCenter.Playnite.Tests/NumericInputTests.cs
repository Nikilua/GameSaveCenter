using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NumericInputTests
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("1440", true)]
    [InlineData("0", false)]
    [InlineData("1441", false)]
    [InlineData("12x", false)]
    [InlineData("", false)]
    [InlineData("2147483648", false)]
    [InlineData("-2147483649", false)]
    public void IntegerRangeValidationRule_ValidatesCompleteMinuteValues(string text, bool expectedValid)
    {
        var rule = new IntegerRangeValidationRule { Minimum = 1, Maximum = 1440 };
        var result = rule.Validate(text, CultureInfo.InvariantCulture);
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void IntegerRangeValidationRule_ExplainsEmptyAndOverflowValues()
    {
        var rule = new IntegerRangeValidationRule { Minimum = 0, Maximum = 300 };

        Assert.Equal("请输入数值。", rule.Validate("  ", CultureInfo.InvariantCulture).ErrorContent);
        Assert.Equal("请输入整数，且不能超出整数范围。", rule.Validate("2147483648", CultureInfo.InvariantCulture).ErrorContent);
    }

    [Fact]
    public void WheelUsesTheSameRangeAndDoesNotClampAtTheBoundary()
    {
        RunSta(() =>
        {
            var source = new NumericValue { Value = 2 };
            var box = new TextBox();
            var binding = new Binding(nameof(NumericValue.Value))
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                ValidatesOnExceptions = true,
                NotifyOnValidationError = true
            };
            binding.ValidationRules.Add(new IntegerRangeValidationRule { Minimum = 1, Maximum = 3 });
            box.SetBinding(TextBox.TextProperty, binding);
            NumericInput.SetEnabled(box, true);

            using var host = new WindowHost(new Window
            {
                Content = box,
                Width = 180,
                Height = 80,
                ShowActivated = true,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            });
            box.Focus();
            Keyboard.Focus(box);

            var up = RaiseWheel(box, 120);
            Assert.True(up.Handled);
            Assert.Equal("3", box.Text);
            Assert.Equal(3, source.Value);
            Assert.False(Validation.GetHasError(box));

            var beyondMaximum = RaiseWheel(box, 120);
            Assert.False(beyondMaximum.Handled);
            Assert.Equal("3", box.Text);
            Assert.Equal(3, source.Value);
            Assert.False(Validation.GetHasError(box));

            box.Text = "2147483648";
            box.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
            Assert.True(Validation.GetHasError(box));
            Assert.Equal(3, source.Value);

            var invalidCurrent = RaiseWheel(box, -120);
            Assert.False(invalidCurrent.Handled);
            Assert.Equal("2147483648", box.Text);
            Assert.Equal(3, source.Value);
        });
    }

    [Fact]
    public void ProductionNumericEditorsAllDeclareTheSharedBehaviorAndAValidationRange()
    {
        var root = FindRepositoryRoot();
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var saves = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("<Setter Property=\"infra:NumericInput.Enabled\" Value=\"True\"/>", tokens);
        Assert.Equal(10, Count(settings, "infra:IntegerRangeValidationRule"));
        Assert.Equal(1, Count(dashboard, "infra:IntegerRangeValidationRule"));
        Assert.Equal(5, Count(saves, "infra:IntegerRangeValidationRule"));
        Assert.Equal(1, Count(trainer, "infra:IntegerRangeValidationRule"));

        Assert.Contains("Minimum=\"0\" Maximum=\"2147483647\"", saves);
        Assert.Contains("空值、负数和整数溢出会被拒绝", saves);
        Assert.Contains("空值、负数和溢出会被拒绝", trainer);
    }

    [Fact]
    public void KeyboardSelectAllDoesNotQueueWorkDuringDispatcherShutdown()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Infrastructure", "NumericInput.cs"));

        Assert.Contains("dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished", source);
        Assert.Contains("catch (InvalidOperationException)", source);
        Assert.Contains("dispatcher.BeginInvoke(new Action(textBox.SelectAll), DispatcherPriority.Input)", source);
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;

    private static int Count(string text, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private static MouseWheelEventArgs RaiseWheel(TextBox source, int delta)
    {
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
        {
            RoutedEvent = UIElement.PreviewMouseWheelEvent
        };
        source.RaiseEvent(args);
        return args;
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
        thread.Join(TimeSpan.FromSeconds(15));
        Assert.False(thread.IsAlive, "STA test did not finish.");
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class NumericValue
    {
        public int Value { get; set; }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window)
        {
            Window = window;
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose()
        {
            if (Window.Dispatcher.HasShutdownStarted || Window.Dispatcher.HasShutdownFinished) return;
            Window.Close();
        }
    }
}
