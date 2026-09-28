using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q13ColumnResizeBehaviorTests
{
    [Fact]
    public void ProductionGrippersResizeWithoutSortingClampToMinimumAndDoubleClickRestoresAutoWidth()
    {
        RunSta(() =>
        {
            EnsureApplication();
            foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                VerifyResizeAndAutoFit(theme);
            VerifyResizeDisabledState();
        });
    }

    private static void VerifyResizeDisabledState()
    {
        var grid = CreateProductionGrid(GameSaveCenterThemeMode.Light);
        var description = new DataGridTextColumn
        {
            Header = "说明",
            Binding = new Binding(nameof(ResizeRow.Description)),
            Width = new DataGridLength(180, DataGridLengthUnitType.Pixel)
        };
        var name = new DataGridTextColumn
        {
            Header = "名称",
            Binding = new Binding(nameof(ResizeRow.Name)),
            CanUserResize = false,
            Width = new DataGridLength(180, DataGridLengthUnitType.Pixel)
        };
        grid.Columns.Add(description);
        grid.Columns.Add(name);
        var window = CreateWindow(grid);

        try
        {
            window.Show();
            FlushLayout(window);

            var descriptionHeader = FindHeader(grid, description);
            var nameHeader = FindHeader(grid, name);
            var descriptionRight = FindGripper(descriptionHeader, "PART_RightHeaderGripper");
            var nameRight = FindGripper(nameHeader, "PART_RightHeaderGripper");
            var nameLeft = FindGripper(nameHeader, "PART_LeftHeaderGripper");

            Assert.Equal(Visibility.Visible, descriptionRight.Visibility);
            Assert.Equal(Visibility.Collapsed, nameRight.Visibility);
            Assert.Equal(Visibility.Visible, nameLeft.Visibility);

            grid.CanUserResizeColumns = false;
            FlushLayout(window);

            Assert.Equal(Visibility.Collapsed, descriptionRight.Visibility);
            Assert.Equal(Visibility.Collapsed, nameLeft.Visibility);
            Assert.Equal(180, description.ActualWidth, 1);
            Assert.Equal(180, name.ActualWidth, 1);
        }
        finally
        {
            window.Close();
        }
    }

    private static void VerifyResizeAndAutoFit(GameSaveCenterThemeMode theme)
    {
        var grid = CreateProductionGrid(theme);
        var items = new ObservableCollection<ResizeRow>
        {
            new ResizeRow("A", "短说明"),
            new ResizeRow("B", string.Concat(Enumerable.Repeat("很长的合成说明内容用于验证双击列边界后可以重新适配到内容。", 12)))
        };
        grid.ItemsSource = items;

        var description = new DataGridTextColumn
        {
            Header = "说明",
            Binding = new Binding(nameof(ResizeRow.Description)),
            SortMemberPath = nameof(ResizeRow.Description),
            Width = new DataGridLength(180, DataGridLengthUnitType.Pixel)
        };
        var name = new DataGridTextColumn
        {
            Header = "名称",
            Binding = new Binding(nameof(ResizeRow.Name)),
            SortMemberPath = nameof(ResizeRow.Name),
            Width = new DataGridLength(220, DataGridLengthUnitType.Pixel)
        };
        grid.Columns.Add(description);
        grid.Columns.Add(name);

        var settings = new GameSaveCenterSettings();
        settings.SetDataGridColumnWidth("other-view", "description", 512);
        settings.SetDataGridColumnWidth("q13-column-resize", "name", 190);
        var persistCount = 0;
        using var controller = new DataGridColumnLayoutController(
            grid,
            "q13-column-resize",
            new[] { "description", "name" },
            settings,
            () => persistCount++);

        var sortingEvents = 0;
        grid.Sorting += (_, _) => sortingEvents++;
        var window = CreateWindow(grid);

        try
        {
            window.Show();
            FlushLayout(window);

            Assert.True(grid.CanUserResizeColumns);
            Assert.Equal(64, grid.MinColumnWidth, 0.1);
            Assert.Equal(2, grid.Items.Count);

            var firstHeader = FindHeader(grid, description);
            var secondHeader = FindHeader(grid, name);
            var rightGripper = FindGripper(firstHeader, "PART_RightHeaderGripper");
            var leftGripper = FindGripper(secondHeader, "PART_LeftHeaderGripper");

            AssertGripperIsAnEightDipResizeTarget(firstHeader, rightGripper);
            AssertGripperIsAnEightDipResizeTarget(secondHeader, leftGripper);
            Assert.True(IsWithin(grid.InputHitTest(GetCenterInGrid(rightGripper, grid)) as DependencyObject, rightGripper),
                $"{theme}: the right resize edge did not hit its production Thumb.");
            Assert.True(IsWithin(grid.InputHitTest(GetCenterInGrid(leftGripper, grid)) as DependencyObject, leftGripper),
                $"{theme}: the left resize edge did not hit its production Thumb.");

            var originalWidth = description.ActualWidth;
            RaiseDrag(rightGripper, 48);
            FlushLayout(window);
            var widenedWidth = description.ActualWidth;
            Assert.True(widenedWidth >= originalWidth + 40,
                $"{theme}: right-edge drag changed width from {originalWidth:0.##} to {widenedWidth:0.##} DIP.");
            Assert.Equal(DataGridLengthUnitType.Pixel, description.Width.UnitType);
            controller.FlushPendingPersistence();
            Assert.True(settings.DataGridColumnWidths.ContainsKey("v1/q13-column-resize/description"));

            // WPF maps the next header's left gripper to the previous visible column.
            secondHeader = FindHeader(grid, name);
            leftGripper = FindGripper(secondHeader, "PART_LeftHeaderGripper");
            var beforeLeftResize = description.ActualWidth;
            RaiseDrag(leftGripper, -24);
            FlushLayout(window);
            Assert.True(description.ActualWidth <= beforeLeftResize - 18,
                $"{theme}: the next header's left edge did not shrink the preceding column.");

            firstHeader = FindHeader(grid, description);
            rightGripper = FindGripper(firstHeader, "PART_RightHeaderGripper");
            RaiseDrag(rightGripper, -4096);
            FlushLayout(window);
            Assert.True(description.ActualWidth >= grid.MinColumnWidth - 0.5,
                $"{theme}: resized column fell below MinColumnWidth: {description.ActualWidth:0.##} DIP.");

            controller.FlushPendingPersistence();
            rightGripper = FindGripper(FindHeader(grid, description), "PART_RightHeaderGripper");
            RaiseDoubleClick(rightGripper);
            FlushLayout(window);

            Assert.Equal(DataGridLengthUnitType.Auto, description.Width.UnitType);
            Assert.True(description.ActualWidth > 300,
                $"{theme}: Auto width did not measure the long synthetic cell content ({description.ActualWidth:0.##} DIP).");
            Assert.False(settings.DataGridColumnWidths.ContainsKey("v1/q13-column-resize/description"),
                "Restoring native Auto width must remove the old saved pixel width.");
            Assert.Equal(190, settings.DataGridColumnWidths["v1/q13-column-resize/name"], 0.1);
            Assert.Equal(512, settings.DataGridColumnWidths["v1/other-view/description"], 0.1);

            controller.FlushPendingPersistence();
            controller.BeginLayoutPass();
            controller.EndLayoutPass();
            FlushLayout(window);
            Assert.Equal(DataGridLengthUnitType.Auto, description.Width.UnitType);
            Assert.True(description.ActualWidth > 300,
                $"{theme}: a later column layout pass reapplied the stale pixel width.");

            Assert.Equal(0, sortingEvents);
            Assert.Null(description.SortDirection);
            Assert.Null(name.SortDirection);
            Assert.Equal(new[] { "A", "B" }, items.Select(item => item.Name).ToArray());
            Assert.True(persistCount >= 2, "User resizing and restoring Auto width should both be persisted.");
        }
        finally
        {
            window.Close();
        }
    }

    private static DataGrid CreateProductionGrid(GameSaveCenterThemeMode theme)
    {
        var grid = new DataGrid
        {
            Resources = LoadProductionResources(),
            AutoGenerateColumns = false,
            CanUserAddRows = false
        };
        var palette = AdaptiveThemePaletteFactory.Create(grid, true, 78, theme);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(grid.Resources, palette, glassEnabled: true, motionEnabled: true);
        return grid;
    }

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static DataGridColumnHeader FindHeader(DataGrid grid, DataGridColumn column)
        => FindVisualChildren<DataGridColumnHeader>(grid)
            .Single(header => ReferenceEquals(header.Column, column));

    private static Thumb FindGripper(DataGridColumnHeader header, string name)
        => Assert.IsType<Thumb>(header.Template.FindName(name, header));

    private static void AssertGripperIsAnEightDipResizeTarget(DataGridColumnHeader header, Thumb gripper)
    {
        Assert.Equal(8, gripper.ActualWidth, 0.1);
        Assert.Equal(Cursors.SizeWE, gripper.Cursor);
        Assert.False(gripper.Focusable);
        Assert.True(gripper.IsHitTestVisible);
        Assert.True(gripper.ActualHeight >= header.ActualHeight - 1.1,
            $"Resize Thumb height {gripper.ActualHeight:0.##} DIP did not span header height {header.ActualHeight:0.##} DIP.");
    }

    private static Point GetCenterInGrid(Thumb gripper, DataGrid grid)
        => gripper.TranslatePoint(new Point(gripper.ActualWidth / 2, gripper.ActualHeight / 2), grid);

    private static bool IsWithin(DependencyObject? child, DependencyObject ancestor)
    {
        while (child != null)
        {
            if (ReferenceEquals(child, ancestor)) return true;
            child = VisualTreeHelper.GetParent(child);
        }
        return false;
    }

    private static void RaiseDrag(Thumb gripper, double horizontalChange)
    {
        gripper.RaiseEvent(new DragStartedEventArgs(0, 0) { RoutedEvent = Thumb.DragStartedEvent });
        gripper.RaiseEvent(new DragDeltaEventArgs(horizontalChange, 0) { RoutedEvent = Thumb.DragDeltaEvent });
        gripper.RaiseEvent(new DragCompletedEventArgs(horizontalChange, 0, false) { RoutedEvent = Thumb.DragCompletedEvent });
    }

    private static void RaiseDoubleClick(Thumb gripper)
        => gripper.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = Control.MouseDoubleClickEvent
        });

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
    }

    private static Window CreateWindow(UIElement content)
        => new Window
        {
            Content = content,
            Width = 820,
            Height = 340,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void EnsureApplication()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class ResizeRow
    {
        public ResizeRow(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}
