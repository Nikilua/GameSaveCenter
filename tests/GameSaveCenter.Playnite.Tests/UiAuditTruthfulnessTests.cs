using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Playnite.Diagnostics;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiAuditTruthfulnessTests
{
    [Fact]
    public void ViewSidebarAuditNeverInvokesActivated()
    {
        var plugin = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));

        Assert.DoesNotContain("auditSidebarItem?.Activated", plugin);
        Assert.DoesNotContain(".Activated?.Invoke", plugin);
        Assert.Contains("NotifyUserToOpenDashboard", plugin);
    }

    [Fact]
    public void FallbackDashboardIsExplicitlyControlledOrigin()
    {
        var plugin = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
        var dashboard = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));

        Assert.Contains("AuditHostKind.ControlledAuditWindow", plugin);
        Assert.Contains("AuditHostKind.EmbeddedPlaynite", dashboard);
    }

    [Fact]
    public void CompletedRootsSplitByCaptureKind()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Diagnostics", "RealHostUiAuditService.cs"));

        Assert.Contains("+ \"|\" + kind.ToString().ToLowerInvariant() + \"-dashboard\"", source);
        Assert.Contains("settingsRoot + \"|settings\"", source);
    }

    [Fact]
    public void SessionManifestsAreIsolated()
    {
        var first = new RealHostUiAuditService.AuditCaptureSession();
        var second = new RealHostUiAuditService.AuditCaptureSession();

        first.Settings.Add(new RealHostUiAuditService.CaptureManifestEntry { File = "settings/one.png" });

        Assert.Single(first.Settings);
        Assert.Empty(second.Settings);
        Assert.Empty(second.EmbeddedDashboard);
        Assert.Empty(second.ControlledDashboard);
    }

    [Fact]
    public void VirtualizedDataGridScrollerIsSkipped()
    {
        Exception? exception = null;
        var skipped = false;

        var thread = new Thread(() =>
        {
            try
            {
                var grid = new DataGrid
                {
                    Width = 300,
                    Height = 200,
                    CanUserAddRows = false,
                    ItemsSource = new ObservableCollection<string>(Enumerable.Range(1, 200).Select(i => "row-" + i))
                };
                grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding(".") });
                ScrollViewer.SetCanContentScroll(grid, true);
                VirtualizingPanel.SetScrollUnit(grid, ScrollUnit.Item);
                var host = new Grid();
                host.Children.Add(grid);
                host.Measure(new Size(300, 200));
                host.Arrange(new Rect(0, 0, 300, 200));
                host.UpdateLayout();
                var scroller = FindVisualChild<ScrollViewer>(grid);
                if (scroller == null)
                    return;
                skipped = RealHostUiAuditService.IsVirtualizedDataGridScroller(scroller)
                    && RealHostUiAuditService.DecideScrollSurfaceStatus(scroller, out _) == RealHostUiAuditService.ScrollSurfaceStatus.SkippedVirtualized;
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
        Assert.True(skipped, "DataGrid logical scroller was not skipped as virtualized.");
    }

    [Fact]
    public void NonVirtualizedPageScrollerCapturesRealDimensions()
    {
        Exception? exception = null;
        var captured = false;
        var tempDir = Path.Combine(Path.GetTempPath(), "gsc-audit-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var thread = new Thread(() =>
            {
                try
                {
                    var scroller = new ScrollViewer
                    {
                        Width = 200,
                        Height = 120,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new StackPanel
                        {
                            Children =
                            {
                                new Border { Height = 80, Background = Brushes.Gray, Child = new TextBlock { Text = "FIRST" } },
                                new Border { Height = 80, Background = Brushes.Gray, Child = new TextBlock { Text = "LAST" } }
                            }
                        }
                    };
                    var host = new Grid();
                    host.Children.Add(scroller);
                    host.Measure(new Size(200, 120));
                    host.Arrange(new Rect(0, 0, 200, 120));
                    host.UpdateLayout();
                    if (RealHostUiAuditService.DecideScrollSurfaceStatus(scroller, out _) != RealHostUiAuditService.ScrollSurfaceStatus.CapturedAndValidated)
                        return;
                    var path = Path.Combine(tempDir, "page.png");
                    UiDiagnosticsExporters.SaveScrollViewerFull(scroller, path);
                    var size = UiDiagnosticsExporters.ReadPngSize(path);
                    captured = size.HasValue && size.Value.Width > 0 && size.Value.Height > 120;
                }
                catch (Exception caught)
                {
                    exception = caught;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }

        Assert.Null(exception);
        Assert.True(captured, "Non-virtualized page scroller did not produce a full-height PNG.");
    }

    [Fact]
    public void ChildLayoutOverflowWritesGate()
    {
        Exception? exception = null;
        var gateExists = false;
        var tempDir = Path.Combine(Path.GetTempPath(), "gsc-audit-overflow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var thread = new Thread(() =>
            {
                try
                {
                    var root = new Grid { Width = 100, Height = 100, Name = "Root" };
                    var child = new Border { Width = 200, Height = 100, Name = "OverflowChild", Background = Brushes.Red };
                    root.Children.Add(child);
                    root.Measure(new Size(100, 100));
                    root.Arrange(new Rect(0, 0, 100, 100));
                    root.UpdateLayout();
                    RealHostUiAuditService.CheckChildLayoutOverflow(root, tempDir);
                    gateExists = File.Exists(Path.Combine(tempDir, "gates", "CHILD_LAYOUT_OVERFLOW.json"));
                }
                catch (Exception caught)
                {
                    exception = caught;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }

        Assert.Null(exception);
        Assert.True(gateExists, "CHILD_LAYOUT_OVERFLOW gate was not written for an overflowing child.");
    }

    [Fact]
    public void ChildLayoutOverflowClearsStaleGateWhenLayoutIsClean()
    {
        Exception? exception = null;
        var gateExists = true;
        var tempDir = Path.Combine(Path.GetTempPath(), "gsc-audit-clean-overflow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tempDir, "gates"));
        File.WriteAllText(Path.Combine(tempDir, "gates", "CHILD_LAYOUT_OVERFLOW.json"), "stale");

        try
        {
            var thread = new Thread(() =>
            {
                try
                {
                    var root = new Grid { Width = 100, Height = 100, Name = "Root" };
                    var child = new Border { Width = 80, Height = 80, Name = "ContainedChild", Background = Brushes.Red };
                    root.Children.Add(child);
                    root.Measure(new Size(100, 100));
                    root.Arrange(new Rect(0, 0, 100, 100));
                    root.UpdateLayout();
                    RealHostUiAuditService.CheckChildLayoutOverflow(root, tempDir);
                    gateExists = File.Exists(Path.Combine(tempDir, "gates", "CHILD_LAYOUT_OVERFLOW.json"));
                }
                catch (Exception caught)
                {
                    exception = caught;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }

        Assert.Null(exception);
        Assert.False(gateExists, "A clean responsive pass must remove a stale overflow gate.");
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
