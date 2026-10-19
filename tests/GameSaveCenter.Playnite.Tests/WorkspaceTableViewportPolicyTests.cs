using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class WorkspaceTableViewportPolicyTests
{
    [Fact]
    public void MinimumSurfaceIncludesAutoRowsAndFrameChromeAroundTheTable()
    {
        RunSta(() =>
        {
            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new Border { Height = 40 };
            Grid.SetRow(header, 0);
            content.Children.Add(header);

            var table = new Border { MinHeight = 180 };
            Grid.SetRow(table, 1);
            content.Children.Add(table);

            var footer = new Border { Height = 30 };
            Grid.SetRow(footer, 2);
            content.Children.Add(footer);

            var surface = new Border
            {
                Padding = new Thickness(3, 4, 3, 4),
                BorderThickness = new Thickness(1),
                Child = content
            };
            surface.Measure(new Size(400, double.PositiveInfinity));

            var minimum = WorkspaceTableViewportPolicy.CalculateMinimumSurfaceHeight(
                content,
                table,
                minimumViewportHeight: 180,
                surface,
                autoRowFallbackHeight: 44,
                roundingAllowance: 0);

            Assert.Equal(260, minimum);
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
            throw new InvalidOperationException("Workspace table viewport policy test failed.", failure);
    }
}
