using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class CompactWorkspaceTableViewportBehaviorTests
{
    [Fact]
    public void SaveHistoryReservesItsSummaryAndReadableRowsBeforeTheInspector()
    {
        RunSta(() =>
        {
            var view = new SaveCenterView();
            var type = typeof(SaveCenterView);
            var layout = GetField<Grid>(type, view, "SaveHistoryLayout");
            var grid = GetField<DataGrid>(type, view, "SaveHistoryGrid");
            var inspector = GetField<ScrollViewer>(type, view, "SaveHistoryActionsScrollViewer");
            var selected = new object();
            grid.ItemsSource = new[] { selected };
            grid.SelectedItem = selected;
            type.GetField("historyInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);

            view.ApplyResponsiveLayout(900, 600);
            MeasureInHost(view, 900, 600);
            view.ApplyResponsiveLayout(900, 600);

            Assert.True(grid.MinHeight >= TableDensityPolicy.CalculateMinimumGridHeight(view),
                "The history grid must reserve its header and four complete rows at the active table density.");
            Assert.True(layout.RowDefinitions[0].MinHeight > grid.MinHeight,
                "The outer table row must reserve height for its summary, four table rows, and card chrome.");
            Assert.True(grid.ActualHeight >= grid.MinHeight - 1,
                "The selected-version inspector must not measure the history grid below its four-row floor.");
            Assert.InRange(inspector.MaxHeight, 140, 220);
            Assert.Equal(Visibility.Visible, inspector.Visibility);
        });
    }

    [Fact]
    public void CurrentMediaReservesTheCardAndLimitsTheStackedInspector()
    {
        RunSta(() =>
        {
            var view = new MediaCenterView();
            var type = typeof(MediaCenterView);
            GetField<TabControl>(type, view, "MediaTabControl").SelectedIndex = 1;
            var layout = GetField<Grid>(type, view, "MediaCurrentLayout");
            var grid = GetField<ListBox>(type, view, "MediaGrid");
            var selected = new object();
            grid.ItemsSource = new[] { selected };
            grid.SelectedItem = selected;
            type.GetField("mediaInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);

            view.ApplyResponsiveLayout(900, 600);
            MeasureInHost(view, 900, 600);
            view.ApplyResponsiveLayout(900, 600);

            var inspector = view.MediaInspectorScrollViewerElement;
            Assert.True(layout.RowDefinitions[0].MinHeight >= 154,
                "The media card surface must reserve a complete 154-DIP card row when its inspector is stacked.");
            Assert.True(grid.ActualHeight >= 154,
                "The expanded media inspector must not collapse the media gallery below one complete card row.");
            Assert.Equal(180, inspector.MaxHeight);
            Assert.True(inspector.ActualHeight > 0 && inspector.ActualHeight <= inspector.MaxHeight + 1);
            Assert.Equal(Visibility.Visible, inspector.Visibility);
            Assert.True(layout.RowDefinitions[1].Height.IsAuto);
        });
    }

    private static T GetField<T>(Type type, object target, string name)
        => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    private static void MeasureInHost(UserControl view, double width, double height)
    {
        var host = new Grid { Width = width, Height = height };
        host.Children.Add(view);
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new InvalidOperationException("Compact workspace table layout test failed.", failure);
    }
}
