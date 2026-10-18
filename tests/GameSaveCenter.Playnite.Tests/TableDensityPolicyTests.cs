using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TableDensityPolicyTests
{
    [Theory]
    [InlineData("Compact", "Compact")]
    [InlineData(" compact ", "Compact")]
    [InlineData("comfortable", "Comfortable")]
    [InlineData("unknown", "Comfortable")]
    [InlineData(null, "Comfortable")]
    public void NormalizeModeUsesOnlySupportedValues(string? value, string expected)
    {
        Assert.Equal(expected, TableDensityPolicy.NormalizeMode(value));
    }

    [Theory]
    [InlineData(52d, 42d, 274d)]
    [InlineData(44d, 38d, 238d)]
    public void MinimumGridHeightReservesFourRowsFromActiveTokens(double rowHeight, double headerHeight, double expected)
    {
        Assert.Equal(expected, TableDensityPolicy.CalculateMinimumGridHeight(rowHeight, headerHeight));
    }

    [Fact]
    public void MinimumGridHeightNormalizesInvalidMeasurements()
    {
        Assert.Equal(94d, TableDensityPolicy.CalculateMinimumGridHeight(
            double.NaN,
            0,
            double.PositiveInfinity,
            minimumRows: 0,
            roundingAllowance: double.NaN));
    }

    [Fact]
    public void ApplyResourcesSetsLocalDensityValuesWithoutMutatingSharedTokens()
    {
        RunSta(() =>
        {
            var owner = new Grid();
            var tokens = new ResourceDictionary
            {
                ["GscTableRowHeightComfortable"] = 52d,
                ["GscTableRowHeightCompact"] = 44d,
                ["GscTableHeaderHeightComfortable"] = 42d,
                ["GscTableHeaderHeightCompact"] = 38d,
                ["GscTableCellPaddingComfortable"] = new Thickness(12, 0, 12, 0),
                ["GscTableCellPaddingCompact"] = new Thickness(10, 0, 10, 0)
            };
            owner.Resources.MergedDictionaries.Add(tokens);

            TableDensityPolicy.ApplyResources(owner, "Compact");

            Assert.Equal(44d, owner.Resources["GscTableRowHeight"]);
            Assert.Equal(38d, owner.Resources["GscTableHeaderHeight"]);
            Assert.Equal(new Thickness(10, 0, 10, 0), owner.Resources["GscTableCellPadding"]);
            Assert.Equal(52d, tokens["GscTableRowHeightComfortable"]);
            Assert.Equal(44d, tokens["GscTableRowHeightCompact"]);

            TableDensityPolicy.ApplyResources(owner, "unsupported");

            Assert.Equal(52d, owner.Resources["GscTableRowHeight"]);
            Assert.Equal(42d, owner.Resources["GscTableHeaderHeight"]);
            Assert.Equal(new Thickness(12, 0, 12, 0), owner.Resources["GscTableCellPadding"]);
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
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "density resource test STA did not finish");
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
