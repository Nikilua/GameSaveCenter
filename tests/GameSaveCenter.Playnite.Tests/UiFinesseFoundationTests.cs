using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xunit;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiFinesseFoundationTests
{
    [Fact]
    public void CSharpMotionMatchesTheMotionTokenContract()
    {
        Exception? exception = null;
        var fast = TimeSpan.Zero;
        var press = TimeSpan.Zero;
        var normal = TimeSpan.Zero;
        var slow = TimeSpan.Zero;

        var thread = new Thread(() =>
        {
            try
            {
                fast = GscMotion.Fast;
                press = GscMotion.Press;
                normal = GscMotion.Normal;
                slow = GscMotion.Slow;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        var tokens = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Themes", "MotionTokens.xaml"));
        Assert.Contains("<Duration x:Key=\"GscMotionFast\">0:0:0.12</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionPress\">0:0:0.10</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionNormal\">0:0:0.22</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionSlow\">0:0:0.30</Duration>", tokens);
        Assert.Equal(TimeSpan.FromMilliseconds(120), fast);
        Assert.Equal(TimeSpan.FromMilliseconds(100), press);
        Assert.Equal(TimeSpan.FromMilliseconds(220), normal);
        Assert.Equal(TimeSpan.FromMilliseconds(300), slow);
    }

    [Fact]
    public void MotionHostResourceOverridesTheCanonicalTokenWithoutChangingFallbacks()
    {
        Exception? exception = null;
        var hostDuration = TimeSpan.Zero;
        var canonicalDuration = TimeSpan.Zero;

        var thread = new Thread(() =>
        {
            try
            {
                var host = new FrameworkElement();
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(260));
                hostDuration = GscMotion.GetDuration(host, GscMotion.MotionDurationKind.Normal);
                canonicalDuration = GscMotion.GetDuration(null, GscMotion.MotionDurationKind.Normal);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(TimeSpan.FromMilliseconds(260), hostDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(220), canonicalDuration);
    }

    [Fact]
    public void MotionAnimationsReleaseClocksAtTheirFinalValues()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var host = new Border
                {
                    Width = 40,
                    Height = 40,
                    Background = Brushes.Transparent
                };
                host.Resources["GscMotionFast"] = new Duration(TimeSpan.FromMilliseconds(30));
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(30));
                host.Resources["GscMotionSlow"] = new Duration(TimeSpan.FromMilliseconds(30));

                window = new Window
                {
                    Content = host,
                    Width = 80,
                    Height = 80,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();

                var translate = GscMotion.GetMutableTranslateTransform(host);
                GscMotion.AnimateTranslate(host, 6, -2, GscMotion.MotionDurationKind.Fast);
                PumpDispatcher(TimeSpan.FromMilliseconds(120));
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.XProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(6, translate.X);
                Assert.Equal(-2, translate.Y);

                GscMotion.AnimateEntrance(host, 10);
                PumpDispatcher(TimeSpan.FromMilliseconds(120));
                Assert.False(DependencyPropertyHelper.GetValueSource(host, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, host.Opacity);
                Assert.Equal(0, translate.Y);
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

        Assert.Null(exception);
    }

    [Fact]
    public void ScaleTransformIsReusedInAStableCompositeTree()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                var first = new Border();
                var firstGroup = new TransformGroup();
                firstGroup.Children.Add(new TranslateTransform(12, -4));
                var nested = new TransformGroup();
                nested.Children.Add(new RotateTransform(7));
                firstGroup.Children.Add(nested);
                firstGroup.Freeze();
                first.RenderTransform = firstGroup;

                ScaleTransform? firstScale = null;
                var initialCount = 0;
                var initialDepth = 0;
                for (var iteration = 0; iteration < 1000; iteration++)
                {
                    var current = GscMotion.GetMutableScaleTransform(first);
                    firstScale ??= current;
                    Assert.Same(firstScale, current);
                    if (iteration == 0)
                    {
                        initialCount = CountTransformNodes(first.RenderTransform);
                        initialDepth = TransformDepth(first.RenderTransform);
                    }
                    else
                    {
                        Assert.Equal(initialCount, CountTransformNodes(first.RenderTransform));
                        Assert.Equal(initialDepth, TransformDepth(first.RenderTransform));
                    }
                }

                var resolvedGroup = Assert.IsType<TransformGroup>(first.RenderTransform);
                var translation = Assert.IsType<TranslateTransform>(resolvedGroup.Children[0]);
                Assert.Equal(12, translation.X);
                Assert.Equal(-4, translation.Y);
                var resolvedNested = Assert.IsType<TransformGroup>(resolvedGroup.Children[1]);
                Assert.Equal(7, Assert.IsType<RotateTransform>(resolvedNested.Children[0]).Angle);

                var second = new Border
                {
                    RenderTransform = new TransformGroup
                    {
                        Children = { new RotateTransform(-5) }
                    }
                };
                var secondScale = GscMotion.GetMutableScaleTransform(second);
                Assert.NotSame(firstScale, secondScale);
                firstScale!.ScaleX = 1.12;
                Assert.Equal(1, secondScale.ScaleX);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    [Fact]
    public void EntranceMotionTakesOverFromTheCurrentEffectiveValue()
    {
        var motion = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Infrastructure", "GscMotion.cs"));

        Assert.Contains("DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty)", motion);
        Assert.Contains("DependencyPropertyHelper.GetValueSource(element, UIElement.OpacityProperty)", motion);
        Assert.Contains("translate.BeginAnimation(TranslateTransform.YProperty, null);", motion);
        Assert.Contains("element.BeginAnimation(UIElement.OpacityProperty, null);", motion);
        Assert.Contains("new DoubleAnimation(currentOpacity, 1, GetDuration(element, MotionDurationKind.Normal))", motion);
        Assert.Contains("new DoubleAnimation(currentY, 0, GetDuration(element, MotionDurationKind.Slow))", motion);
        Assert.Contains("rapid re-entry", motion);
        Assert.Contains("FillBehavior = FillBehavior.HoldEnd", motion);
        Assert.Contains("translateAnimation.Completed", motion);
    }

    [Fact]
    public void ProductionMotionCallsResolveDurationsFromTheirVisualHost()
    {
        var root = FindRepositoryRoot();
        var motion = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "GscMotion.cs"));
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));

        Assert.Contains("=> AnimateTranslate(element, x, y, GetDuration(element, kind));", motion);
        Assert.Contains("GetDuration(element, MotionDurationKind.Normal)", motion);
        Assert.Contains("GscMotion.MotionDurationKind.Normal", overview);
        Assert.Contains("GscMotion.MotionDurationKind.Fast", dashboard);
        Assert.Contains("GscMotion.GetDuration(StatusPill, GscMotion.MotionDurationKind.Normal)", dashboard);
        Assert.Contains("GscMotion.GetDuration(DialogCard, GscMotion.MotionDurationKind.Normal)", dashboard);
        Assert.Contains("GscMotion.GetDuration(ToastHost, GscMotion.MotionDurationKind.Normal)", dashboard);
        Assert.Contains("StopDialogMotion();", dashboard);
        Assert.Contains("slide.Completed", dashboard);
        Assert.Contains("dialogMotionGeneration", dashboard);
        Assert.Contains("foreach (var card in cards)\n                RemoveToast(card);", dashboard);
        Assert.Contains("fade.Completed", dashboard);
        Assert.Contains("GscMotion.GetDuration(SidebarContentLayer, GscMotion.MotionDurationKind.Normal)", shell);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = duration };
        timer.Tick += (_, __) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static int CountTransformNodes(Transform? transform)
    {
        if (transform is not TransformGroup group)
            return transform == null ? 0 : 1;
        return 1 + group.Children.Cast<Transform>().Sum(CountTransformNodes);
    }

    private static int TransformDepth(Transform? transform)
    {
        if (transform is not TransformGroup group)
            return transform == null ? 0 : 1;
        return 1 + (group.Children.Count == 0 ? 0 : group.Children.Cast<Transform>().Max(TransformDepth));
    }
}
