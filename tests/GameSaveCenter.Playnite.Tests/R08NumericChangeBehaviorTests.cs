using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08NumericChangeBehaviorTests
{
    [Fact]
    public void CounterChangeUsesRealRenderPulseAndSuppressesHighFrequencyBounce()
    {
        Exception? failure = null;
        int feedbackCount = 0;
        double siblingLeftBefore = 0;
        double siblingLeftAfter = 0;
        bool scaleWasAnimated = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var grid = new Grid { Width = 220, Height = 70 };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var value = new TextBlock
                {
                    Text = "99",
                    Width = 96,
                    Height = 40,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                value.Resources["GscMotionFast"] = new Duration(TimeSpan.FromSeconds(1));
                NumericChangeFeedback.SetIsEnabled(value, true);
                NumericChangeFeedback.SetSystemMotionEnabledForAudit(value, true);
                Grid.SetColumn(value, 0);
                grid.Children.Add(value);

                var sibling = new Border { Width = 40, Height = 20 };
                Grid.SetColumn(sibling, 1);
                grid.Children.Add(sibling);

                window = CreateWindow(grid);
                window.Show();
                FlushLayout(window);
                siblingLeftBefore = sibling.TransformToAncestor(grid).Transform(new Point(0, 0)).X;

                value.Text = "100";
                feedbackCount = NumericChangeFeedback.GetFeedbackCountForAudit(value);
                var scale = FindScale(value.RenderTransform);
                FlushLayout(window);
                scaleWasAnimated = WaitForAnimatedScale(window, scale);

                // A burst of refreshes must not restart a pulse on every sample.
                value.Text = "101";
                value.Text = "102";
                FlushLayout(window);
                siblingLeftAfter = sibling.TransformToAncestor(grid).Transform(new Point(0, 0)).X;

                GscMotion.NormalizeAll();
                FlushLayout(window);
                Assert.Equal(1, NumericChangeFeedback.GetFeedbackCountForAudit(value));
                Assert.NotNull(scale);
                Assert.True(scaleWasAnimated, "The numeric change must exercise a real WPF render animation.");
                Assert.Equal(1d, scale!.ScaleX, 3);
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(1, feedbackCount);
        Assert.Equal(siblingLeftBefore, siblingLeftAfter, 3);
    }

    [Fact]
    public void ReducedMotionUpdatesImmediatelyWithoutNumericPulse()
    {
        Exception? failure = null;
        int feedbackCount = 0;
        bool animated = false;
        string? finalText = null;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var value = new TextBlock
                {
                    Text = "99",
                    Width = 96,
                    Height = 40,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                NumericChangeFeedback.SetIsEnabled(value, true);
                NumericChangeFeedback.SetMotionEnabled(value, false);
                window = CreateWindow(value);
                window.Show();
                FlushLayout(window);

                value.Text = "100";
                FlushLayout(window);
                feedbackCount = NumericChangeFeedback.GetFeedbackCountForAudit(value);
                finalText = value.Text;
                var scale = FindScale(value.RenderTransform);
                animated = scale != null
                    && DependencyPropertyHelper.GetValueSource(scale, ScaleTransform.ScaleXProperty).IsAnimated;
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal("100", finalText);
        Assert.Equal(0, feedbackCount);
        Assert.False(animated);
    }

    [Fact]
    public void SystemMotionPreferenceSuppressesNumericPulse()
    {
        Exception? failure = null;
        int feedbackCount = 0;
        bool animated = false;
        string? finalText = null;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var value = new TextBlock
                {
                    Text = "99",
                    Width = 96,
                    Height = 40,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                NumericChangeFeedback.SetIsEnabled(value, true);
                NumericChangeFeedback.SetSystemMotionEnabledForAudit(value, false);
                window = CreateWindow(value);
                window.Show();
                FlushLayout(window);

                value.Text = "100";
                FlushLayout(window);
                feedbackCount = NumericChangeFeedback.GetFeedbackCountForAudit(value);
                finalText = value.Text;
                var scale = FindScale(value.RenderTransform);
                animated = scale != null
                    && DependencyPropertyHelper.GetValueSource(scale, ScaleTransform.ScaleXProperty).IsAnimated;
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal("100", finalText);
        Assert.Equal(0, feedbackCount);
        Assert.False(animated);
    }

    [Fact]
    public void DefaultSystemMotionPreferenceControlsNumericPulse()
    {
        Exception? failure = null;
        int feedbackCount = 0;
        bool animated = false;
        bool systemMotionEnabled = false;
        string? finalText = null;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var value = new TextBlock
                {
                    Text = "99",
                    Width = 96,
                    Height = 40,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                NumericChangeFeedback.SetIsEnabled(value, true);
                window = CreateWindow(value);
                window.Show();
                FlushLayout(window);
                systemMotionEnabled = GscMotion.IsEnabled(true);

                value.Text = "100";
                FlushLayout(window);
                feedbackCount = NumericChangeFeedback.GetFeedbackCountForAudit(value);
                finalText = value.Text;
                var scale = FindScale(value.RenderTransform);
                animated = scale != null
                    && DependencyPropertyHelper.GetValueSource(scale, ScaleTransform.ScaleXProperty).IsAnimated;
                GscMotion.NormalizeAll();
                FlushLayout(window);
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal("100", finalText);
        Assert.Equal(systemMotionEnabled ? 1 : 0, feedbackCount);
        if (systemMotionEnabled)
            Assert.True(animated, "The default system motion preference must allow the real numeric pulse when enabled.");
        else
            Assert.False(animated, "The default system motion preference must suppress the numeric pulse when disabled.");
    }

    [Fact]
    public void OverviewOptInScopeExcludesProgressAndTechnicalText()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var overview = File.ReadAllText(Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));

        Assert.Equal(6, CountOccurrences(overview, "infra:NumericChangeFeedback.IsEnabled=\"True\""));
        Assert.Contains("Width=\"96\"", overview);
        Assert.Contains("x:Name=\"OverviewTaskProgressBar\"", overview);
        Assert.DoesNotContain("OverviewTaskProgressBar\" infra:NumericChangeFeedback", overview);
        Assert.DoesNotContain("GscTypographyTimeCell", overview);
    }

    private static Window CreateWindow(UIElement content)
        => new Window
        {
            Content = content,
            Width = 240,
            Height = 100,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static bool WaitForAnimatedScale(Window window, ScaleTransform? scale)
    {
        if (scale == null)
            return false;

        var deadline = DateTime.UtcNow.AddMilliseconds(250);
        while (DateTime.UtcNow < deadline)
        {
            if (DependencyPropertyHelper.GetValueSource(scale, ScaleTransform.ScaleXProperty).IsAnimated)
                return true;

            window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
            Thread.Sleep(5);
        }

        return DependencyPropertyHelper.GetValueSource(scale, ScaleTransform.ScaleXProperty).IsAnimated;
    }

    private static ScaleTransform? FindScale(Transform? transform)
    {
        if (transform is ScaleTransform scale)
            return scale;
        if (!(transform is TransformGroup group))
            return null;

        foreach (var child in group.Children)
        {
            var nested = FindScale(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }
}
