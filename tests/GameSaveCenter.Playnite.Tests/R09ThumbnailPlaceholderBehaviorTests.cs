using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Converters;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09ThumbnailPlaceholderBehaviorTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.R09.Thumbnail.Tests", Guid.NewGuid().ToString("N"));

    public R09ThumbnailPlaceholderBehaviorTests()
    {
        Directory.CreateDirectory(root);
    }

    [Fact]
    public void FixedPreviewSlotUsesDistinctFallbackTextForEveryMediaState()
    {
        RunSta(() =>
        {
            var card = new Grid { Width = 164, Height = 154 };
            card.RowDefinitions.Add(new RowDefinition { Height = new GridLength(96) });
            card.RowDefinitions.Add(new RowDefinition { Height = new GridLength(58) });
            var preview = new MediaThumbnailPreview
            {
                Width = 96,
                Height = 96,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Kind = MediaKind.Screenshot
            };
            Grid.SetRow(preview, 0);
            card.Children.Add(preview);
            var action = new System.Windows.Controls.Button { Content = "操作", Width = 72, Height = 28, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetRow(action, 1);
            card.Children.Add(action);

            var window = CreateWindow(card, 164, 154);
            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Equal("NoImage", preview.PreviewState);
                Assert.Equal("暂无截图", preview.PlaceholderText);
                Assert.True(preview.IsPlaceholderVisible);
                Assert.Equal(96, preview.ActualWidth, 3);
                Assert.Equal(96, preview.ActualHeight, 3);
                Assert.True(action.TransformToAncestor(card).Transform(new Point(0, 0)).Y >= 96);

                preview.Kind = MediaKind.VideoClip;
                Assert.Equal("Video", preview.PreviewState);
                Assert.Equal("录像预览", preview.PlaceholderText);
                Assert.True(preview.IsPlaceholderVisible);

                var missing = Path.Combine(root, "missing.png");
                preview.Kind = MediaKind.Screenshot;
                preview.SourcePath = missing;
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Missing", TimeSpan.FromSeconds(3));
                Assert.Equal("媒体文件不存在", preview.PlaceholderText);
                Assert.True(preview.IsPlaceholderVisible);

                var corrupt = Path.Combine(root, "corrupt.png");
                File.WriteAllText(corrupt, "not an image");
                preview.SourcePath = corrupt;
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Failed", TimeSpan.FromSeconds(3));
                Assert.Equal("缩略图损坏或无法读取", preview.PlaceholderText);
                Assert.True(preview.IsPlaceholderVisible);

                var valid = Path.Combine(root, "valid.png");
                WritePng(valid, 64, 64, 61);
                preview.SourcePath = valid;
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Ready", TimeSpan.FromSeconds(3));
                Assert.Equal(string.Empty, preview.PlaceholderText);
                Assert.False(preview.IsPlaceholderVisible);
                Assert.Equal(96, preview.ActualWidth, 3);
                Assert.Equal(96, preview.ActualHeight, 3);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ExplicitRefreshAfterMediaMovesReplacesOldImageAndCanRecover()
    {
        RunSta(() =>
        {
            var originalPath = Path.Combine(root, "media.png");
            var movedPath = Path.Combine(root, "moved.png");
            WritePng(originalPath, 64, 64, 61);
            AsyncThumbnailLoader.ClearCache();

            var preview = new MediaThumbnailPreview
            {
                Width = 96,
                Height = 96,
                Kind = MediaKind.Screenshot
            };
            var image = Assert.IsType<AsyncThumbnailImage>(preview.Children[0]);
            var window = CreateWindow(preview, 128, 128);
            try
            {
                window.Show();
                window.UpdateLayout();
                preview.SourcePath = originalPath;
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Ready", TimeSpan.FromSeconds(3));
                Assert.Equal((byte)61, FirstPixel(image));

                File.Move(originalPath, movedPath);
                RefreshPreview(preview, originalPath);
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Missing", TimeSpan.FromSeconds(3));

                Assert.True(preview.IsPlaceholderVisible);
                Assert.Equal("媒体文件不存在", preview.PlaceholderText);
                Assert.Null(image.Source);
                Assert.Equal(96, preview.ActualWidth, 3);
                Assert.Equal(96, preview.ActualHeight, 3);
                Assert.True(File.Exists(movedPath));

                WritePng(originalPath, 64, 64, 231);
                RefreshPreview(preview, originalPath);
                PumpUntil(window.Dispatcher, () => preview.PreviewState == "Ready", TimeSpan.FromSeconds(3));

                Assert.Equal((byte)231, FirstPixel(image));
                Assert.False(preview.IsPlaceholderVisible);
                Assert.Equal(96, preview.ActualWidth, 3);
                Assert.Equal(96, preview.ActualHeight, 3);
            }
            finally
            {
                window.Close();
            }
        });
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); }
        catch { }
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void PumpUntil(Dispatcher dispatcher, Func<bool> condition, TimeSpan timeout)
    {
        var frame = new DispatcherFrame();
        var started = DateTime.UtcNow;
        var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        timer.Tick += (_, _) =>
        {
            if (condition() || DateTime.UtcNow - started >= timeout)
            {
                timer.Stop();
                frame.Continue = false;
            }
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void WritePng(string path, int width, int height, byte value)
    {
        var pixels = new byte[width * height * 4];
        for (var index = 0; index < width * height; index++)
        {
            pixels[index * 4] = value;
            pixels[index * 4 + 1] = (byte)(255 - value);
            pixels[index * 4 + 2] = 127;
            pixels[index * 4 + 3] = 255;
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void RefreshPreview(MediaThumbnailPreview preview, string path)
    {
        preview.SourcePath = string.Empty;
        preview.SourcePath = path;
    }

    private static byte FirstPixel(AsyncThumbnailImage image)
    {
        var source = Assert.IsAssignableFrom<BitmapSource>(image.Source);
        var stride = source.PixelWidth * 4;
        var pixels = new byte[stride * source.PixelHeight];
        source.CopyPixels(pixels, stride, 0);
        return pixels[0];
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
        thread.Join();
        Assert.Null(failure);
    }
}
