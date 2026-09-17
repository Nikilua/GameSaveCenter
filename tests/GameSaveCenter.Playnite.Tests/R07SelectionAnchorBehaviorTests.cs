using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07SelectionAnchorBehaviorTests
{
    [Fact]
    public void StableIdentityWinsOverChangedRowPosition()
    {
        var refreshed = new List<Row>
        {
            new Row("before"),
            new Row("selected"),
            new Row("after")
        };

        var restored = SelectionAnchorResolver.Restore(
            refreshed,
            "selected",
            previousIndex: 0,
            row => row.Id);

        Assert.Equal("selected", restored?.Id);
    }

    [Fact]
    public void MissingIdentityClampsToNeighborInsteadOfFirstRow()
    {
        var refreshed = new List<Row>
        {
            new Row("before"),
            new Row("after")
        };

        var restored = SelectionAnchorResolver.Restore(
            refreshed,
            "deleted",
            previousIndex: 1,
            row => row.Id);

        Assert.Equal("after", restored?.Id);
        Assert.NotEqual("before", restored?.Id);
    }

    [Fact]
    public void DataGridSelectionUsesResolvedNeighborAfterRefresh()
    {
        Exception? exception = null;
        string? selectedId = null;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var rows = new ObservableCollection<Row>
                {
                    new Row("first"),
                    new Row("deleted"),
                    new Row("neighbor")
                };
                var grid = new DataGrid
                {
                    Width = 260,
                    Height = 120,
                    AutoGenerateColumns = false,
                    CanUserAddRows = false,
                    ItemsSource = rows
                };
                grid.Columns.Add(new DataGridTextColumn { Header = "Id", Binding = new System.Windows.Data.Binding(nameof(Row.Id)) });
                window = new Window
                {
                    Content = grid,
                    Width = 280,
                    Height = 150,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();
                grid.SelectedItem = rows[1];
                var previousIndex = grid.SelectedIndex;
                var stableId = ((Row)grid.SelectedItem).Id;

                rows.RemoveAt(1);
                var restored = SelectionAnchorResolver.Restore(rows, stableId, previousIndex, row => row.Id);
                grid.SelectedItem = restored;
                window.UpdateLayout();
                var selected = grid.SelectedItem as Row ?? throw new InvalidOperationException("DataGrid did not keep the resolved selection.");
                selectedId = selected.Id;
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
        Assert.Equal("neighbor", selectedId);
    }

    private sealed class Row
    {
        public Row(string id) => Id = id;

        public string Id { get; }
    }
}
