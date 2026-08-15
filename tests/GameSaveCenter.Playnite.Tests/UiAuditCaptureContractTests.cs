using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Playnite.Diagnostics;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiAuditCaptureContractTests
{
    [Fact]
    public void ControlledHostAppliesProfileToClientNotDashboardWidth()
    {
        var source = ReadAuditSource();

        Assert.Contains("dashboard.ClearValue(FrameworkElement.WidthProperty)", source);
        Assert.Contains("dashboard.ClearValue(FrameworkElement.HeightProperty)", source);
        Assert.Contains("dashboard.HorizontalAlignment = HorizontalAlignment.Stretch", source);
        Assert.DoesNotContain("dashboard.Width = size.Width", source);
        Assert.DoesNotContain("dashboard.Height = size.Height", source);
    }

    [Fact]
    public void EmbeddedCurrentNeverResizesDashboardOrOverridesTheme()
    {
        var source = ReadAuditSource();
        var start = source.IndexOf("CaptureEmbeddedCurrentAsync", StringComparison.Ordinal);
        var end = source.IndexOf("CaptureControlledAtSizeAsync", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "Embedded method not found in source.");
        var embedded = source.Substring(start, end - start);

        Assert.DoesNotContain("dashboard.Width =", embedded);
        Assert.DoesNotContain("dashboard.Height =", embedded);
        Assert.DoesNotContain("ApplyThemeForAudit", embedded);
    }

    [Fact]
    public void ViewportExpectedBitmapSizeFollowsDpiContract()
    {
        Exception? exception = null;
        var result = default((int Width, int Height));

        var thread = new Thread(() =>
        {
            try
            {
                var grid = new Grid { Width = 300, Height = 200 };
                grid.Measure(new Size(300, 200));
                grid.Arrange(new Rect(0, 0, 300, 200));
                grid.UpdateLayout();
                result = UiDiagnosticsExporters.ExpectedBitmapSize(grid, 1.5);
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
        Assert.Equal((450, 300), result);
    }

    [Fact]
    public void MultipleMeaningfulScrollSurfacesAreEnumerated()
    {
        Exception? exception = null;
        var count = 0;

        var thread = new Thread(() =>
        {
            try
            {
                var root = new Grid { Width = 420, Height = 120 };
                var first = CreateTallScroller();
                var second = CreateTallScroller();
                root.Children.Add(first);
                root.Children.Add(second);
                root.Measure(new Size(420, 120));
                root.Arrange(new Rect(0, 0, 420, 120));
                root.UpdateLayout();
                count = RealHostUiAuditService.FindMeaningfulScrollSurfaces(root).Count;
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
        Assert.True(count >= 2, $"Expected at least 2 scroll surfaces, found {count}.");
    }

    [Fact]
    public void BottomRightSentinelBoundsStayInsideViewport()
    {
        Assert.True(RealHostUiAuditService.IsBoundsWithinViewport(new Rect(0, 0, 1024, 768), 1024, 768, 2d));
        Assert.True(RealHostUiAuditService.IsBoundsWithinViewport(new Rect(0, 0, 1025, 769), 1024, 768, 2d));
        Assert.False(RealHostUiAuditService.IsBoundsWithinViewport(new Rect(0, 0, 1030, 769), 1024, 768, 2d));
        Assert.False(RealHostUiAuditService.IsBoundsWithinViewport(new Rect(-5, 0, 1000, 700), 1024, 768, 2d));
    }

    [Fact]
    public void CaptureManifestCarriesCompletenessFields()
    {
        var source = ReadAuditSource();
        Assert.Contains("CompletenessValidated", source);
        Assert.Contains("CaptureType", source);
        Assert.Contains("ScrollSurfaceFull", source);
        Assert.Contains("EmbeddedPlaynite", source);
        Assert.Contains("DedicatedAuditWindow", source);
    }

    private static ScrollViewer CreateTallScroller()
    {
        return new ScrollViewer
        {
            Width = 200,
            Height = 120,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Border { Height = 600, Background = Brushes.Gray }
        };
    }

    private static string ReadAuditSource()
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Diagnostics", "RealHostUiAuditService.cs"));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
