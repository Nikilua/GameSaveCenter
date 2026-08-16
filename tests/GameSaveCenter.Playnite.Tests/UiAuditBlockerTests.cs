using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GameSaveCenter.Playnite.Diagnostics;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiAuditBlockerTests
{
    [Fact]
    public void SafeFileNamePreservesChineseAndCollapsesInvalidChars()
    {
        Assert.Equal("历史版本", RealHostUiAuditService.SafeFileName("历史版本"));
        Assert.Equal("workspace-overview", RealHostUiAuditService.SafeFileName("workspace-overview"));
        Assert.Equal("a-b-c", RealHostUiAuditService.SafeFileName("a:b/c"));
        var twice = RealHostUiAuditService.SafeFileName(RealHostUiAuditService.SafeFileName("设置 迁移"));
        Assert.Equal("设置 迁移", twice);
        Assert.DoesNotContain(twice, ch => Path.GetInvalidFileNameChars().Contains(ch));
    }

    [Fact]
    public void AuditFallbackWindowComparisonIsExplicit()
    {
        RunSta(() =>
        {
            var window = new Window();
            var other = new Window();
            Assert.True(RealHostUiAuditService.IsAuditFallbackWindow(window, window));
            Assert.False(RealHostUiAuditService.IsAuditFallbackWindow(window, other));
            Assert.False(RealHostUiAuditService.IsAuditFallbackWindow(window, null));
        });
    }

    [Fact]
    public void FixedChildOverflowIsRealLayoutOverflow()
    {
        RunSta(() =>
        {
            var root = new Grid { Width = 100, Height = 100 };
            var child = new Border { Name = "FixedCard", Width = 200, Height = 100, Background = Brushes.Red };
            root.Children.Add(child);
            root.Measure(new Size(100, 100));
            root.Arrange(new Rect(0, 0, 100, 100));
            root.UpdateLayout();

            Assert.Equal(
                RealHostUiAuditService.OverflowClassification.RealFixedLayoutOverflow,
                RealHostUiAuditService.ClassifyOverflow(child));
        });
    }

    [Fact]
    public void ScrollViewerContentOverflowIsIntentional()
    {
        RunSta(() =>
        {
            var root = new Grid { Width = 200, Height = 200 };
            var scroller = new ScrollViewer { Width = 100, Height = 100, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var content = new Border { Name = "ScrollContent", Width = 300, Height = 300, Background = Brushes.Gray };
            scroller.Content = content;
            root.Children.Add(scroller);
            root.Measure(new Size(200, 200));
            root.Arrange(new Rect(0, 0, 200, 200));
            root.UpdateLayout();

            Assert.Equal(
                RealHostUiAuditService.OverflowClassification.IntentionalScrollableOverflow,
                RealHostUiAuditService.ClassifyOverflow(content));
        });
    }

    [Fact]
    public void DecorativeOverflowIsExcluded()
    {
        RunSta(() =>
        {
            var root = new Grid { Width = 100, Height = 100 };
            var child = new Border
            {
                Name = "Glow",
                Width = 200,
                Height = 100,
                Background = Brushes.Transparent,
                Effect = new BlurEffect { Radius = 20 }
            };
            root.Children.Add(child);
            root.Measure(new Size(100, 100));
            root.Arrange(new Rect(0, 0, 100, 100));
            root.UpdateLayout();

            Assert.Equal(
                RealHostUiAuditService.OverflowClassification.DecorativeOverflow,
                RealHostUiAuditService.ClassifyOverflow(child));
        });
    }

    [Fact]
    public void InternalTextBoxScrollerIsFiltered()
    {
        RunSta(() =>
        {
            var root = new Grid { Width = 300, Height = 200 };
            var textBox = new TextBox { Width = 200, Height = 100, Text = "hello" };
            root.Children.Add(textBox);
            root.Measure(new Size(300, 200));
            root.Arrange(new Rect(0, 0, 300, 200));
            root.UpdateLayout();
            var scroller = FindVisualChild<ScrollViewer>(textBox);
            if (scroller != null)
            {
                Assert.True(RealHostUiAuditService.IsInternalTemplateScroller(scroller));
            }
        });
    }

    [Fact]
    public void ManifestAndGatesCarryScopeAndHighGates()
    {
        var source = ReadAuditSource();
        Assert.Contains("Scope = metadata.Mode.IndexOf(\"settings\"", source);
        Assert.Contains("REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED", source);
        Assert.Contains("AUDIT_SOURCE_REVISION_MISSING", source);
        Assert.Contains("ProductionVisualSourceOfTruthAvailable", source);
    }

    [Fact]
    public void EmbeddedIdentityUsesHostWindowNotStaticNull()
    {
        var source = ReadAuditSource();
        Assert.Contains("PresentationSource.FromVisual(dashboard)", source);
        Assert.Contains("Window.GetWindow(dashboard)", source);
        Assert.Contains("ReferenceEquals(window, auditDashboardWindow)", source);
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
                return match;
            var nested = FindVisualChild<T>(child);
            if (nested != null)
                return nested;
        }
        return null;
    }

    private static string ReadAuditSource()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        var root = directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        return File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Diagnostics", "RealHostUiAuditService.cs"));
    }
}
