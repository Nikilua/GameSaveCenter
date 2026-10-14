using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DataGridHorizontalWheelBehaviorTests
{
    private const int WmMouseHorizontalWheel = 0x020E;
    private readonly ITestOutputHelper output;

    public DataGridHorizontalWheelBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void ShiftWheelAndNativeHorizontalWheelMoveProductionMediaGridAndClampAtBothEnds()
    {
        Exception? exception = null;
        var observations = string.Empty;
        var hookDetachedAfterClose = false;
        var thread = new Thread(() =>
        {
            Window? window = null;
            DataGrid? testGrid = null;
            try
            {
                var view = new MediaCenterView();
                var tabs = (TabControl)typeof(MediaCenterView)
                    .GetField("MediaTabControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                BindingOperations.ClearBinding(tabs, TabControl.SelectedIndexProperty);
                tabs.SelectedIndex = 0;

                var grid = (DataGrid)typeof(MediaCenterView)
                    .GetField("MediaInboxGrid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                testGrid = grid;
                grid.ItemsSource = new BatchObservableCollection<MediaItemDto>(CreateSyntheticMedia(2000));

                window = new Window
                {
                    Content = view,
                    Width = 760,
                    Height = 560,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.SizeChanged += (_, _) => view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                view.ApplyResponsiveLayout(window.Width, window.Height);
                window.Show();
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                DrainDispatcher(window);

                Assert.True(grid.IsLoaded && grid.ActualWidth > 0 && grid.ActualHeight > 0,
                    $"the production Media grid must have a finite loaded viewport: {grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}");
                Assert.True(DataGridHorizontalWheelBehavior.GetEnabled(grid),
                    "the production Media grid must inherit the shared horizontal wheel behavior");
                Assert.True(DataGridHorizontalWheelBehavior.HasHookRegistration(grid),
                    "the loaded production Media grid must attach its native horizontal-wheel hook");
                Assert.True(grid.EnableRowVirtualization);
                Assert.Equal(VirtualizationMode.Standard, VirtualizingPanel.GetVirtualizationMode(grid));
                Assert.Equal(ScrollUnit.Item, VirtualizingPanel.GetScrollUnit(grid));
                Assert.False(grid.EnableColumnVirtualization);

                foreach (var column in grid.Columns)
                    column.Width = new DataGridLength(Math.Max(240, column.MinWidth + 96), DataGridLengthUnitType.Pixel);
                DrainDispatcher(window);

                var viewer = FindGridViewer(grid);
                Assert.True(viewer.ScrollableWidth > 1,
                    $"the long production table must exercise horizontal scrolling: extent={viewer.ExtentWidth:0.##},viewport={viewer.ViewportWidth:0.##}");
                Assert.True(FindVisualChildren<DataGridRow>(grid).Count() < 40,
                    $"2,000 synthetic inbox items must remain virtualized; realized={FindVisualChildren<DataGridRow>(grid).Count()}");
                viewer.ScrollToHorizontalOffset(0);
                DrainDispatcher(window);

                var start = viewer.HorizontalOffset;
                Assert.True(DataGridHorizontalWheelBehavior.TryHandleShiftWheel(grid, -120, ModifierKeys.Shift));
                DrainDispatcher(window);
                var afterShiftRight = viewer.HorizontalOffset;
                Assert.True(afterShiftRight > start + 1,
                    $"Shift+wheel down must reveal columns to the right: {start:0.##}->{afterShiftRight:0.##}");

                Assert.False(DataGridHorizontalWheelBehavior.TryHandleShiftWheel(grid, -120, ModifierKeys.Control));
                Assert.False(DataGridHorizontalWheelBehavior.TryHandleShiftWheel(grid, -120, ModifierKeys.Control | ModifierKeys.Shift));
                Assert.False(DataGridHorizontalWheelBehavior.TryHandleShiftWheel(grid, -120, ModifierKeys.None));
                DrainDispatcher(window);
                Assert.Equal(afterShiftRight, viewer.HorizontalOffset, 2);

                Assert.True(DataGridHorizontalWheelBehavior.TryHandleShiftWheel(grid, 120, ModifierKeys.Shift));
                DrainDispatcher(window);
                Assert.InRange(Math.Abs(viewer.HorizontalOffset - start), 0, 0.5);

                var screenPoint = grid.PointToScreen(new Point(grid.ActualWidth / 2, grid.ActualHeight / 2));
                RouteHorizontalWheel(grid, screenPoint, 12);
                DrainDispatcher(window);
                var afterFineRight = viewer.HorizontalOffset;
                Assert.True(afterFineRight > start,
                    $"a high-resolution horizontal-wheel delta must move the production grid: {start:0.##}->{afterFineRight:0.##}");
                RouteHorizontalWheel(grid, screenPoint, -12);
                DrainDispatcher(window);
                Assert.InRange(Math.Abs(viewer.HorizontalOffset - start), 0, 0.5);

                RouteHorizontalWheel(grid, screenPoint, 30000);
                DrainDispatcher(window);
                var rightEnd = viewer.HorizontalOffset;
                Assert.InRange(Math.Abs(rightEnd - viewer.ScrollableWidth), 0, 0.5);
                RouteHorizontalWheel(grid, screenPoint, 120);
                DrainDispatcher(window);
                Assert.InRange(Math.Abs(viewer.HorizontalOffset - rightEnd), 0, 0.5);

                RouteHorizontalWheel(grid, screenPoint, -30000);
                DrainDispatcher(window);
                var leftEnd = viewer.HorizontalOffset;
                Assert.InRange(Math.Abs(leftEnd), 0, 0.5);
                RouteHorizontalWheel(grid, screenPoint, -120);
                DrainDispatcher(window);
                Assert.InRange(Math.Abs(viewer.HorizontalOffset - leftEnd), 0, 0.5);

                var outsidePoint = grid.PointToScreen(new Point(grid.ActualWidth + 20, grid.ActualHeight / 2));
                Assert.False(RouteHorizontalWheel(grid, outsidePoint, 120, expectHandled: false));
                Assert.InRange(Math.Abs(viewer.HorizontalOffset - leftEnd), 0, 0.5);

                DataGridHorizontalWheelBehavior.SetEnabled(grid, false);
                DrainDispatcher(window);
                Assert.False(DataGridHorizontalWheelBehavior.HasHookRegistration(grid),
                    "disabling the shared behavior must remove the window hook");
                DataGridHorizontalWheelBehavior.SetEnabled(grid, true);
                DrainDispatcher(window);
                Assert.True(DataGridHorizontalWheelBehavior.HasHookRegistration(grid),
                    "re-enabling the behavior must attach exactly one window hook");

                observations = $"items={grid.Items.Count},realizedRows={FindVisualChildren<DataGridRow>(grid).Count()},"
                    + $"virtualization={VirtualizingPanel.GetVirtualizationMode(grid)}/{VirtualizingPanel.GetScrollUnit(grid)}/column={grid.EnableColumnVirtualization},"
                    + $"range={viewer.ScrollableWidth:0.##},shift={start:0.##}->{afterShiftRight}->{viewer.HorizontalOffset:0.##},"
                    + $"fineDelta={afterFineRight:0.##},ends=0->{rightEnd:0.##}->0";
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
                if (testGrid != null)
                {
                    DrainDispatcher(testGrid);
                    hookDetachedAfterClose = !DataGridHorizontalWheelBehavior.HasHookRegistration(testGrid);
                }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        output.WriteLine(observations);
        Assert.Null(exception);
        Assert.True(hookDetachedAfterClose, "closing the host must remove the DataGrid hook registration");
    }

    private static bool RouteHorizontalWheel(DataGrid grid, Point screenPoint, int delta, bool expectHandled = true)
    {
        var x = unchecked((short)Math.Round(screenPoint.X));
        var y = unchecked((short)Math.Round(screenPoint.Y));
        var packedCoordinates = unchecked((uint)(ushort)x | ((uint)(ushort)y << 16));
        var packedDelta = unchecked((uint)(ushort)(short)delta << 16);
        var handled = false;
        var source = PresentationSource.FromVisual(grid) as HwndSource
            ?? throw new InvalidOperationException("the production DataGrid is not connected to an HWND source");
        var result = DataGridHorizontalWheelBehavior.OnWindowMessage(grid, source.Handle, WmMouseHorizontalWheel,
            new IntPtr(unchecked((int)packedDelta)), new IntPtr(unchecked((int)packedCoordinates)), ref handled);
        Assert.Equal(expectHandled, handled);
        Assert.Equal(expectHandled ? new IntPtr(1) : IntPtr.Zero, result);
        return handled;
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static ScrollViewer FindGridViewer(DataGrid grid)
        => FindVisualChildren<ScrollViewer>(grid).First(viewer => viewer.Name == "DG_ScrollViewer");

    private static MediaItemDto[] CreateSyntheticMedia(int count)
        => Enumerable.Range(0, count)
            .Select(index => new MediaItemDto
            {
                MediaId = $"synthetic-media-{index:D5}",
                OriginalPath = $@"C:\SyntheticMedia\screenshot-{index:D5}.png",
                ArchivePath = $@"C:\SyntheticMedia\archive-{index:D5}.zip",
                CapturedUtc = DateTime.UtcNow.AddMinutes(-index),
                Kind = MediaKind.Screenshot,
                Source = MediaSourceKind.WindowsScreenshot,
                ClassificationState = "Inbox",
                ClassificationReason = "synthetic scroll fixture"
            })
            .ToArray();

    private static void DrainDispatcher(DispatcherObject dispatcherObject)
    {
        var dispatcher = dispatcherObject.Dispatcher;
        var frame = new DispatcherFrame();
        dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        if (dispatcherObject is FrameworkElement element)
            element.UpdateLayout();
    }

}
