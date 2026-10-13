using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Controls;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q24FractionalDpiTypographyBehaviorTests
{
    private static readonly double[] TargetScales = { 1d, 1.25d, 1.5d, 1.75d, 2d };
    private const double SampleWidth = 320d;
    private const double SampleHeight = 68d;

    private readonly ITestOutputHelper output;

    public Q24FractionalDpiTypographyBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void ProductionTypographyIconAndOneDipBorderRenderAtFiveExplicitTargetDpis()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var observations = new List<string>();

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var icon = new ThemeAwareIcon
                {
                    Style = Assert.IsType<Style>(resources["GscActionIcon"]),
                    IconData = Assert.IsAssignableFrom<Geometry>(resources["GscIconActionVerify"]),
                    Width = 16,
                    Height = 16,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var label = new TextBlock
                {
                    Style = Assert.IsType<Style>(resources["GscBodyStyle"]),
                    Text = "重新校验当前存档路径 | Verify backup",
                    TextWrapping = TextWrapping.NoWrap,
                    TextTrimming = TextTrimming.None,
                    VerticalAlignment = VerticalAlignment.Center,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true
                };
                var content = new Grid();
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(icon, 0);
                Grid.SetColumn(label, 1);
                content.Children.Add(icon);
                content.Children.Add(label);

                var root = new Border
                {
                    Width = SampleWidth,
                    Height = SampleHeight,
                    Padding = new Thickness(12),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(123, 143, 170)),
                    CornerRadius = new CornerRadius(8),
                    Background = new SolidColorBrush(Color.FromRgb(20, 28, 42)),
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                    Resources = resources,
                    Child = content
                };
                root.Measure(new Size(SampleWidth, SampleHeight));
                root.Arrange(new Rect(0, 0, SampleWidth, SampleHeight));
                root.UpdateLayout();

                var sourceDpi = VisualTreeHelper.GetDpi(root);
                Assert.True(sourceDpi.DpiScaleX > 0 && sourceDpi.DpiScaleY > 0, "The WPF visual must report its inherited source DPI.");
                Assert.True(IsTextFitted(label), DescribeText(label));
                Assert.False(HasNonUnitScaleBetween(label, root), "Production text must not inherit a permanent bitmap-like scale transform.");

                var textBounds = label.TransformToAncestor(root).TransformBounds(new Rect(new Point(0, 0), label.RenderSize));
                var iconBounds = icon.TransformToAncestor(root).TransformBounds(new Rect(new Point(0, 0), icon.RenderSize));
                Assert.True(textBounds.Left >= 0 && textBounds.Right <= root.ActualWidth, $"Text escaped the sample viewport: {textBounds} / {root.ActualWidth:0.##} DIP.");
                Assert.True(iconBounds.Left >= 0 && iconBounds.Right <= root.ActualWidth, $"Icon escaped the sample viewport: {iconBounds} / {root.ActualWidth:0.##} DIP.");

                var evidenceDirectory = GetEvidenceDirectory();
                if (evidenceDirectory != null)
                    Directory.CreateDirectory(evidenceDirectory);

                foreach (var scale in TargetScales)
                {
                    var targetDpi = 96d * scale;
                    var bitmap = RenderAtTargetDpi(root, scale);
                    var expectedWidth = (int)Math.Ceiling(SampleWidth * scale);
                    var expectedHeight = (int)Math.Ceiling(SampleHeight * scale);
                    Assert.Equal(expectedWidth, bitmap.PixelWidth);
                    Assert.Equal(expectedHeight, bitmap.PixelHeight);
                    Assert.Equal(targetDpi, bitmap.DpiX, 3);
                    Assert.Equal(targetDpi, bitmap.DpiY, 3);

                    var pixels = CopyPixels(bitmap);
                    var textPixelCount = CountBrightPixels(pixels, bitmap.PixelWidth, bitmap.PixelHeight, textBounds, scale);
                    var iconPixelCount = CountBrightPixels(pixels, bitmap.PixelWidth, bitmap.PixelHeight, iconBounds, scale);
                    var borderPixelCount = CountBrightPixels(
                        pixels,
                        bitmap.PixelWidth,
                        bitmap.PixelHeight,
                        new Rect(20, 0, SampleWidth - 40, 1),
                        scale,
                        45);
                    Assert.True(textPixelCount >= 20, $"Text was not visibly rasterized at {scale:0.##}x (pixels={textPixelCount}).");
                    Assert.True(iconPixelCount >= 4, $"The production icon geometry was not visibly rasterized at {scale:0.##}x (pixels={iconPixelCount}).");
                    Assert.True(borderPixelCount >= (int)(SampleWidth * scale * 0.5), $"The 1 DIP top border disappeared at {scale:0.##}x (pixels={borderPixelCount}).");

                    if (evidenceDirectory != null)
                        SavePng(bitmap, Path.Combine(evidenceDirectory, "q24-01-target-" + (scale * 100).ToString("0", CultureInfo.InvariantCulture) + "pct.png"));

                    observations.Add(
                        $"target={scale * 100:0}% ({targetDpi:0} dpi); sourceVisualDpi={sourceDpi.DpiScaleX:0.##}; pixels={bitmap.PixelWidth}x{bitmap.PixelHeight}; text={textPixelCount}; icon={iconPixelCount}; border={borderPixelCount}; textBounds={textBounds}; iconBounds={iconBounds}");
                }

                var clippedNegative = new TextBlock
                {
                    Text = label.Text,
                    Width = 8,
                    Height = 24,
                    TextWrapping = TextWrapping.NoWrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ClipToBounds = true
                };
                clippedNegative.Measure(new Size(8, 24));
                clippedNegative.Arrange(new Rect(0, 0, 8, 24));
                Assert.False(IsTextFitted(clippedNegative), "The clipping negative control must be rejected by the same text-fit check.");

                var scaledNegative = new TextBlock { Text = label.Text, RenderTransform = new ScaleTransform(1.25, 1.25) };
                Assert.True(HasNonUnitScaleBetween(scaledNegative, scaledNegative), "The permanent-scale negative control must be detectable.");
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        foreach (var observation in observations)
            output.WriteLine(observation);
        output.WriteLine("Boundary: explicit offscreen RenderTargetBitmap DPI; not physical monitor DPI, Playnite presentation, or a screen capture.");
    }

    private static ResourceDictionary LoadProductionResources()
    {
        return Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/GscIconPack.xaml""/>
  </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>"));
    }

    private static RenderTargetBitmap RenderAtTargetDpi(Visual visual, double scale)
    {
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(SampleWidth * scale),
            (int)Math.Ceiling(SampleHeight * scale),
            96d * scale,
            96d * scale,
            PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private static byte[] CopyPixels(BitmapSource bitmap)
    {
        var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        return pixels;
    }

    private static int CountBrightPixels(
        IReadOnlyList<byte> pixels,
        int pixelWidth,
        int pixelHeight,
        Rect dipBounds,
        double scale,
        byte threshold = 155)
    {
        var left = Math.Max(0, (int)Math.Floor(dipBounds.Left * scale));
        var top = Math.Max(0, (int)Math.Floor(dipBounds.Top * scale));
        var right = Math.Min(pixelWidth, (int)Math.Ceiling(dipBounds.Right * scale));
        var bottom = Math.Min(pixelHeight, (int)Math.Ceiling(dipBounds.Bottom * scale));
        var count = 0;
        for (var y = top; y < bottom; y++)
        {
            for (var x = left; x < right; x++)
            {
                var offset = (y * pixelWidth + x) * 4;
                if (pixels[offset + 1] >= threshold
                    && pixels[offset + 2] >= threshold
                    && pixels[offset + 3] >= threshold)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static bool IsTextFitted(TextBlock text)
    {
        return text.TextWrapping == TextWrapping.NoWrap
            && text.TextTrimming == TextTrimming.None
            && text.ActualWidth > 0
            && text.ActualWidth + 0.5 >= text.DesiredSize.Width;
    }

    private static bool HasNonUnitScaleBetween(Visual visual, Visual ancestor)
    {
        DependencyObject? current = visual;
        while (current != null)
        {
            if (current is FrameworkElement element
                && (HasNonUnitScale(element.RenderTransform) || HasNonUnitScale(element.LayoutTransform)))
            {
                return true;
            }

            if (ReferenceEquals(current, ancestor))
                return false;
            current = VisualTreeHelper.GetParent(current);
        }

        return true;
    }

    private static bool HasNonUnitScale(Transform transform)
    {
        var matrix = transform.Value;
        return Math.Abs(matrix.M11 - 1d) > 0.001
            || Math.Abs(matrix.M22 - 1d) > 0.001
            || Math.Abs(matrix.M12) > 0.001
            || Math.Abs(matrix.M21) > 0.001;
    }

    private static string DescribeText(TextBlock text)
        => $"text={text.Text}; width={text.ActualWidth:0.##}/{text.DesiredSize.Width:0.##} DIP; wrapping={text.TextWrapping}; trimming={text.TextTrimming}";

    private static void SavePng(BitmapSource bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static string? GetEvidenceDirectory()
    {
        var configured = Environment.GetEnvironmentVariable("GSC_Q24_DPI_EVIDENCE_DIR");
        if (string.IsNullOrWhiteSpace(configured))
            return null;

        var root = Path.GetFullPath(Path.Combine(TestRepositoryContext.Root, "docs", "design", "reviews", "ui-finesse-round3-20260915", "evidence", "Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013", "renders"));
        var target = Path.GetFullPath(configured);
        if (!string.Equals(target.TrimEnd(Path.DirectorySeparatorChar), root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Q24-01 render evidence can only be written to the checked-in renders directory.");
        return target;
    }
}
