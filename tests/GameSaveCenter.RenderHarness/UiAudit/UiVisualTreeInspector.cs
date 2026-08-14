using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiVisualTreeInspector
{
    private const int MaxNodes = 5000;

    public static List<UiVisualNode> Inspect(Visual root)
    {
        var nodes = new List<UiVisualNode>();
        Traverse(root, null, 0, nodes);
        return nodes;
    }

    private static void Traverse(DependencyObject current, DependencyObject? parent, int depth, List<UiVisualNode> nodes)
    {
        if (nodes.Count >= MaxNodes)
            return;

        if (current is FrameworkElement element && element.IsVisible && element.ActualWidth > 0 && element.ActualHeight > 0)
        {
            nodes.Add(new UiVisualNode
            {
                Type = element.GetType().Name,
                Name = element.Name,
                AutomationName = AutomationProperties.GetName(element),
                Visibility = element.Visibility.ToString(),
                IsVisible = element.IsVisible,
                IsEnabled = element.IsEnabled,
                ActualWidth = Math.Round(element.ActualWidth, 2),
                ActualHeight = Math.Round(element.ActualHeight, 2),
                DesiredWidth = Math.Round(element.DesiredSize.Width, 2),
                DesiredHeight = Math.Round(element.DesiredSize.Height, 2),
                RenderWidth = Math.Round(element.RenderSize.Width, 2),
                RenderHeight = Math.Round(element.RenderSize.Height, 2),
                Margin = element.Margin.ToString(),
                HorizontalAlignment = element.HorizontalAlignment.ToString(),
                VerticalAlignment = element.VerticalAlignment.ToString(),
                GridRow = Grid.GetRow(element),
                GridColumn = Grid.GetColumn(element),
                GridRowSpan = Grid.GetRowSpan(element),
                GridColumnSpan = Grid.GetColumnSpan(element),
                MinWidth = element.MinWidth,
                MaxWidth = element.MaxWidth,
                MinHeight = element.MinHeight,
                MaxHeight = element.MaxHeight,
                Opacity = element.Opacity,
                Text = ExtractText(element),
                ParentType = parent == null ? string.Empty : parent.GetType().Name,
                ParentName = (parent as FrameworkElement)?.Name ?? string.Empty,
                Depth = depth
            });
        }

        var childCount = VisualTreeHelper.GetChildrenCount(current);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(current, i);
            Traverse(child, current, depth + 1, nodes);
        }
    }

    private static string ExtractText(FrameworkElement element)
    {
        if (element is TextBlock textBlock)
            return textBlock.Text ?? string.Empty;
        if (element is ContentControl contentControl && contentControl.Content is string text)
            return text;
        if (element is HeaderedContentControl headered && headered.Header is string header)
            return header;
        return string.Empty;
    }
}
