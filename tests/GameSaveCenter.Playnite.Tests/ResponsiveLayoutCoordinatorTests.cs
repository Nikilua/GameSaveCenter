using System;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class ResponsiveLayoutCoordinatorTests
{
    [Fact]
    public void WidthBoundariesPreserveTheExistingShellStates()
    {
        var cases = new[]
        {
            (width: 959d, mode: LayoutMode.Narrow, sidebar: 72d, gutter: 10d, pickerOnTopBar: false, pickerWidth: 330d, shellCompact: true, shellPicker: 220d),
            (width: 960d, mode: LayoutMode.Compact, sidebar: 78d, gutter: 10d, pickerOnTopBar: false, pickerWidth: 330d, shellCompact: true, shellPicker: 220d),
            (width: 979d, mode: LayoutMode.Compact, sidebar: 78d, gutter: 10d, pickerOnTopBar: false, pickerWidth: 330d, shellCompact: true, shellPicker: 220d),
            (width: 980d, mode: LayoutMode.Compact, sidebar: 78d, gutter: 10d, pickerOnTopBar: false, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1039d, mode: LayoutMode.Compact, sidebar: 78d, gutter: 10d, pickerOnTopBar: false, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1040d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1079d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1080d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1199d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1200d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1279d, mode: LayoutMode.Standard, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 330d, shellCompact: false, shellPicker: 300d),
            (width: 1280d, mode: LayoutMode.Expanded, sidebar: 228d, gutter: 0d, pickerOnTopBar: true, pickerWidth: 380d, shellCompact: false, shellPicker: 300d)
        };

        foreach (var testCase in cases)
        {
            var state = ResponsiveLayoutCoordinator.Calculate(testCase.width, 720);
            Assert.Equal(testCase.mode, state.Mode);
            Assert.Equal(testCase.sidebar, state.SidebarWidth);
            Assert.Equal(testCase.gutter, state.SidebarGutterWidth);
            Assert.Equal(testCase.pickerOnTopBar, state.SupportsTopBarPicker);
            Assert.Equal(testCase.pickerWidth, state.PickerWidth);
            Assert.Equal(testCase.shellCompact, state.IsCompactShellHeader);
            Assert.Equal(testCase.shellPicker, state.ShellPickerWidth);
            Assert.Equal(testCase.width < 1200, state.OverviewUsesStackedColumns);
        }
    }

    [Fact]
    public void HeightBoundariesPreserveTableAndFooterStates()
    {
        var cases = new[]
        {
            (height: 640d, tableMin: 0d, workspaceMin: 0d, comfortable: false, shortFooter: true, viewport: 601.6d),
            (height: 700d, tableMin: 96d, workspaceMin: 112d, comfortable: false, shortFooter: false, viewport: 665d),
            (height: 720d, tableMin: 96d, workspaceMin: 112d, comfortable: false, shortFooter: false, viewport: 684d),
            (height: 768d, tableMin: 140d, workspaceMin: 160d, comfortable: true, shortFooter: false, viewport: 729.6d),
            (height: 900d, tableMin: 140d, workspaceMin: 160d, comfortable: true, shortFooter: false, viewport: 820d)
        };

        foreach (var testCase in cases)
        {
            var state = ResponsiveLayoutCoordinator.Calculate(1200, testCase.height);
            Assert.Equal(testCase.tableMin, state.TableMinHeight);
            Assert.Equal(testCase.workspaceMin, state.WorkspaceTableMinHeight);
            Assert.Equal(testCase.comfortable, state.IsComfortableHeight);
            Assert.Equal(testCase.shortFooter, state.IsShortFooter);
            Assert.Equal(testCase.viewport, state.TableViewportHeight, 6);
        }
    }

    [Fact]
    public void HeaderAndSelectedGameThresholdsRemainIndependent()
    {
        var state = ResponsiveLayoutCoordinator.Calculate(980, 768);

        Assert.False(state.IsCompactShellHeader);
        Assert.True(state.ShouldStackGameHeader(1179));
        Assert.False(state.ShouldStackGameHeader(1180));
        Assert.True(state.OverviewUsesStackedColumns);
    }

    [Fact]
    public void CoordinatorIsDeterministicAcrossThemesAndResizeSamples()
    {
        foreach (var width in new[] { 980d, 1040d, 1200d })
        foreach (var height in new[] { 640d, 700d, 720d, 768d, 900d })
        {
            var first = ResponsiveLayoutCoordinator.Calculate(width, height);
            var second = ResponsiveLayoutCoordinator.Calculate(width, height);

            Assert.Equal(first.Mode, second.Mode);
            Assert.Equal(first.SidebarWidth, second.SidebarWidth);
            Assert.Equal(first.TableViewportHeight, second.TableViewportHeight);
            Assert.Equal(first.IsComfortableHeight, second.IsComfortableHeight);
        }
    }
}
