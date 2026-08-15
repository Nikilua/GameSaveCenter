using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiScreenshotService
{
    public static void SavePng(Visual visual, string path)
    {
        var actual = visual as FrameworkElement;
        var width = actual?.ActualWidth ?? 0;
        var height = actual?.ActualHeight ?? 0;
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException($"Cannot render {path}: empty size {width}x{height}");

        var bitmap = RenderVisual(visual, width, height);
        SaveBitmap(bitmap, path);
    }

    public static double ProbeHeaderWhiteRatio(DataGrid grid)
    {
        if (grid.ActualWidth <= 0 || grid.ActualHeight <= 0)
            return 0;

        var bitmap = RenderVisual(grid, grid.ActualWidth, grid.ActualHeight);
        var headerHeight = grid.ColumnHeaderHeight > 0
            ? Math.Min((int)Math.Ceiling(grid.ColumnHeaderHeight), bitmap.PixelHeight)
            : Math.Min(42, bitmap.PixelHeight);
        if (headerHeight <= 0 || bitmap.PixelWidth <= 0)
            return 0;

        var width = bitmap.PixelWidth;
        var stride = width * 4;
        var pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);

        var white = 0;
        var total = 0;
        for (var y = 0; y < headerHeight; y += 2)
        {
            for (var x = 0; x < width; x += 4)
            {
                var offset = y * stride + x * 4;
                var b = pixels[offset];
                var g = pixels[offset + 1];
                var r = pixels[offset + 2];
                var a = pixels[offset + 3];
                if (a > 200 && r > 245 && g > 245 && b > 245)
                    white++;
                total++;
            }
        }

        return total > 0 ? white / (double)total : 0;
    }

    /// <summary>
    /// Captures a ScrollViewer from its top to its bottom by scrolling the real control
    /// through every viewport position and stitching the slices into one long PNG.
    /// </summary>
    public static StitchedScrollCapture? CaptureScrollViewerFull(ScrollViewer scroller, string path)
    {
        var originalVertical = scroller.VerticalOffset;
        var originalHorizontal = scroller.HorizontalOffset;
        var originalVBar = scroller.VerticalScrollBarVisibility;
        var originalHBar = scroller.HorizontalScrollBarVisibility;

        try
        {
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            scroller.UpdateLayout();

            var width = scroller.ActualWidth;
            var viewportHeight = scroller.ViewportHeight;
            var extentHeight = scroller.ExtentHeight;
            if (width <= 0 || viewportHeight <= 0)
                return null;

            if (extentHeight <= viewportHeight + 0.5)
            {
                SavePng(scroller, path);
                return new StitchedScrollCapture
                {
                    Width = (int)Math.Ceiling(width),
                    Height = (int)Math.Ceiling(viewportHeight),
                    SliceCount = 1,
                    Path = path
                };
            }

            // Non-virtualized page content is already arranged to its full extent by WPF.
            // Rendering that content directly gives a clean top-to-bottom page image and
            // avoids slicing templates such as SettingsScroller's ContentPresenter.
            if (scroller.Content is FrameworkElement content
                && !(content is DataGrid)
                && !(content is ListBox)
                && !(content is ItemsPresenter)
                && content.ActualHeight + content.Margin.Top + content.Margin.Bottom >= extentHeight - 2
                && content.ActualHeight > viewportHeight + 0.5)
            {
                SavePng(content, path);
                return new StitchedScrollCapture
                {
                    Width = (int)Math.Ceiling(content.ActualWidth),
                    Height = (int)Math.Ceiling(content.ActualHeight),
                    SliceCount = 1,
                    Path = path
                };
            }

            var scrollable = scroller.ScrollableHeight;
            var offsets = new List<double>();
            var current = 0d;
            while (current < scrollable - 0.5)
            {
                offsets.Add(current);
                var next = Math.Min(scrollable, current + viewportHeight);
                if (next <= current + 0.5)
                    break;
                current = next;
            }
            if (scrollable > 0.5
                && (offsets.Count == 0 || Math.Abs(offsets[offsets.Count - 1] - scrollable) > 0.5))
            {
                offsets.Add(scrollable);
            }

            var slices = new List<CroppedBitmap>();
            var previousEnd = 0d;
            foreach (var offset in offsets)
            {
                scroller.ScrollToVerticalOffset(offset);
                scroller.UpdateLayout();
                var slice = RenderVisual(scroller, width, scroller.ActualHeight);
                var rangeStart = Math.Max(offset, previousEnd);
                var rangeEnd = Math.Min(extentHeight, offset + viewportHeight);
                var cropTop = Math.Max(0, (int)Math.Ceiling(rangeStart - offset));
                var cropHeight = Math.Max(1, (int)Math.Ceiling(rangeEnd - rangeStart));
                cropHeight = Math.Min(cropHeight, slice.PixelHeight - cropTop);
                slices.Add(new CroppedBitmap(
                    slice,
                    new Int32Rect(0, cropTop, slice.PixelWidth, cropHeight)));
                previousEnd = rangeEnd;
            }

            var totalWidth = slices.Max(slice => slice.PixelWidth);
            var totalHeight = slices.Sum(slice => slice.PixelHeight);
            var stitched = new WriteableBitmap(totalWidth, totalHeight, 96, 96, PixelFormats.Pbgra32, null);
            var y = 0;
            foreach (var slice in slices)
            {
                var stride = slice.PixelWidth * 4;
                var pixels = new byte[stride * slice.PixelHeight];
                slice.CopyPixels(pixels, stride, 0);
                stitched.WritePixels(
                    new Int32Rect(0, y, slice.PixelWidth, slice.PixelHeight),
                    pixels,
                    stride,
                    0);
                y += slice.PixelHeight;
            }

            SaveBitmap(stitched, path);
            return new StitchedScrollCapture
            {
                Width = totalWidth,
                Height = totalHeight,
                SliceCount = slices.Count,
                Path = path
            };
        }
        finally
        {
            scroller.VerticalScrollBarVisibility = originalVBar;
            scroller.HorizontalScrollBarVisibility = originalHBar;
            scroller.ScrollToVerticalOffset(originalVertical);
            scroller.ScrollToHorizontalOffset(originalHorizontal);
            scroller.UpdateLayout();
        }
    }

    public static StitchedScrollCapture? CaptureDataGridFull(DataGrid grid, string path)
    {
        if (grid.Items.Count == 0 || grid.ActualWidth <= 0 || grid.ActualHeight <= 0)
            return null;

        var scroller = FindVisualChildren<ScrollViewer>(grid)
            .OrderByDescending(candidate => candidate.ActualHeight)
            .FirstOrDefault();
        var originalOffset = scroller?.VerticalOffset ?? 0;
        try
        {
            var headerHeight = grid.ColumnHeaderHeight;
            if (double.IsNaN(headerHeight) || headerHeight <= 0)
            {
                var headers = FindVisualChildren<System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter>(grid).FirstOrDefault();
                headerHeight = headers != null && headers.ActualHeight > 0 ? headers.ActualHeight : 42;
            }

            var firstRow = FindVisualChildren<System.Windows.Controls.DataGridRow>(grid).FirstOrDefault();
            var rowHeight = firstRow != null && firstRow.ActualHeight > 0 ? firstRow.ActualHeight : 48;
            var viewportHeight = grid.ActualHeight;
            var extentHeight = headerHeight + rowHeight * grid.Items.Count;
            if (scroller == null || extentHeight <= viewportHeight + 0.5)
            {
                SavePng(grid, path);
                return new StitchedScrollCapture
                {
                    Width = (int)Math.Ceiling(grid.ActualWidth),
                    Height = (int)Math.Ceiling(grid.ActualHeight),
                    SliceCount = 1,
                    Path = path
                };
            }

            var scrollablePixels = extentHeight - viewportHeight;
            var offsets = BuildPixelOffsets(viewportHeight, scrollablePixels);
            var slices = new List<CroppedBitmap>();
            var previousEnd = 0d;
            foreach (var pixelOffset in offsets)
            {
                var itemOffset = Math.Max(0, (pixelOffset - headerHeight) / rowHeight);
                itemOffset = Math.Min(scroller.ScrollableHeight, itemOffset);
                scroller.ScrollToVerticalOffset(itemOffset);
                scroller.UpdateLayout();
                var slice = RenderVisual(grid, grid.ActualWidth, viewportHeight);
                var rangeStart = Math.Max(pixelOffset, previousEnd);
                var rangeEnd = Math.Min(extentHeight, pixelOffset + viewportHeight);
                var cropTop = Math.Max(0, (int)Math.Ceiling(rangeStart - pixelOffset));
                var cropHeight = Math.Max(1, (int)Math.Ceiling(rangeEnd - rangeStart));
                cropHeight = Math.Min(cropHeight, slice.PixelHeight - cropTop);
                slices.Add(new CroppedBitmap(slice, new Int32Rect(0, cropTop, slice.PixelWidth, cropHeight)));
                previousEnd = rangeEnd;
            }
            return Stitch(slices, path);
        }
        finally
        {
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(originalOffset);
                scroller.UpdateLayout();
            }
        }
    }

    public static StitchedScrollCapture? CaptureListBoxFull(ListBox list, string path)
    {
        if (list.Items.Count == 0 || list.ActualWidth <= 0 || list.ActualHeight <= 0)
            return null;

        var scroller = FindVisualChildren<ScrollViewer>(list)
            .OrderByDescending(candidate => candidate.ActualHeight)
            .FirstOrDefault();
        var originalOffset = scroller?.VerticalOffset ?? 0;
        try
        {
            var firstItem = FindVisualChildren<ListBoxItem>(list).FirstOrDefault();
            var itemHeight = firstItem != null && firstItem.ActualHeight > 0 ? firstItem.ActualHeight : 56;
            var viewportHeight = list.ActualHeight;
            var extentHeight = itemHeight * list.Items.Count;
            if (scroller == null || extentHeight <= viewportHeight + 0.5)
            {
                SavePng(list, path);
                return new StitchedScrollCapture
                {
                    Width = (int)Math.Ceiling(list.ActualWidth),
                    Height = (int)Math.Ceiling(list.ActualHeight),
                    SliceCount = 1,
                    Path = path
                };
            }

            var scrollablePixels = extentHeight - viewportHeight;
            var offsets = BuildPixelOffsets(viewportHeight, scrollablePixels);
            var slices = new List<CroppedBitmap>();
            var previousEnd = 0d;
            foreach (var pixelOffset in offsets)
            {
                var itemOffset = Math.Max(0, pixelOffset / itemHeight);
                itemOffset = Math.Min(scroller.ScrollableHeight, itemOffset);
                scroller.ScrollToVerticalOffset(itemOffset);
                scroller.UpdateLayout();
                var slice = RenderVisual(list, list.ActualWidth, viewportHeight);
                var rangeStart = Math.Max(pixelOffset, previousEnd);
                var rangeEnd = Math.Min(extentHeight, pixelOffset + viewportHeight);
                var cropTop = Math.Max(0, (int)Math.Ceiling(rangeStart - pixelOffset));
                var cropHeight = Math.Max(1, (int)Math.Ceiling(rangeEnd - rangeStart));
                cropHeight = Math.Min(cropHeight, slice.PixelHeight - cropTop);
                slices.Add(new CroppedBitmap(slice, new Int32Rect(0, cropTop, slice.PixelWidth, cropHeight)));
                previousEnd = rangeEnd;
            }
            return Stitch(slices, path);
        }
        finally
        {
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(originalOffset);
                scroller.UpdateLayout();
            }
        }
    }

    private static List<double> BuildPixelOffsets(double viewportHeight, double scrollablePixels)
    {
        var offsets = new List<double>();
        var current = 0d;
        while (current < scrollablePixels - 0.5)
        {
            offsets.Add(current);
            var next = Math.Min(scrollablePixels, current + viewportHeight);
            if (next <= current + 0.5)
                break;
            current = next;
        }
        if (scrollablePixels > 0.5
            && (offsets.Count == 0 || Math.Abs(offsets[offsets.Count - 1] - scrollablePixels) > 0.5))
        {
            offsets.Add(scrollablePixels);
        }
        return offsets;
    }

    private static StitchedScrollCapture Stitch(List<CroppedBitmap> slices, string path)
    {
        var totalWidth = slices.Max(slice => slice.PixelWidth);
        var totalHeight = slices.Sum(slice => slice.PixelHeight);
        var stitched = new WriteableBitmap(totalWidth, totalHeight, 96, 96, PixelFormats.Pbgra32, null);
        var y = 0;
        foreach (var slice in slices)
        {
            var stride = slice.PixelWidth * 4;
            var pixels = new byte[stride * slice.PixelHeight];
            slice.CopyPixels(pixels, stride, 0);
            stitched.WritePixels(
                new Int32Rect(0, y, slice.PixelWidth, slice.PixelHeight),
                pixels,
                stride,
                0);
            y += slice.PixelHeight;
        }
        SaveBitmap(stitched, path);
        return new StitchedScrollCapture
        {
            Width = totalWidth,
            Height = totalHeight,
            SliceCount = slices.Count,
            Path = path
        };
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed)
                yield return typed;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static BitmapSource RenderVisual(Visual visual, double width, double height)
    {
        var pixelWidth = Math.Max(1, (int)Math.Ceiling(width));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(height));
        var bitmap = new RenderTargetBitmap(
            pixelWidth,
            pixelHeight,
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(visual);
        return bitmap;
    }

    private static void SaveBitmap(BitmapSource bitmap, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}

public sealed class StitchedScrollCapture
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int SliceCount { get; set; }
    public string Path { get; set; } = string.Empty;
}
