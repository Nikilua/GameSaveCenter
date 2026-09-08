using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Converters;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class AsyncThumbnailImageTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Thumbnail.Tests", Guid.NewGuid().ToString("N"));

        public AsyncThumbnailImageTests()
        {
            Directory.CreateDirectory(root);
        }

        [Fact]
        public void SourcePathSetBeforeLoadDoesNotStartUntilTheImageIsVisible()
        {
            var path = Path.Combine(root, "visible.png");
            WritePng(path, 64, 64, 31);
            AsyncThumbnailLoader.ClearCache();
            AsyncThumbnailLoader.ResetDiagnostics();

            Exception? exception = null;
            long requestsBeforeShow = -1;
            long requestsAfterShow = -1;
            var thread = new Thread(() =>
            {
                Window? window = null;
                try
                {
                    var image = new AsyncThumbnailImage { SourcePath = path, PreviewWidth = 96 };
                    requestsBeforeShow = AsyncThumbnailLoader.CaptureDiagnostics().RequestCount;
                    window = CreateWindow(image);
                    window.Show();
                    window.UpdateLayout();
                    PumpUntil(window.Dispatcher, () => image.Source != null, TimeSpan.FromSeconds(3));
                    requestsAfterShow = AsyncThumbnailLoader.CaptureDiagnostics().RequestCount;
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
            Assert.Equal(0, requestsBeforeShow);
            Assert.True(requestsAfterShow >= 1);
        }

        [Fact]
        public void ReplacingPathNeverLeavesTheOldImageVisible()
        {
            var oldPath = Path.Combine(root, "old.png");
            var newPath = Path.Combine(root, "new.png");
            WritePng(oldPath, 800, 800, 11);
            WritePng(newPath, 64, 64, 231);
            AsyncThumbnailLoader.ClearCache();

            Exception? exception = null;
            byte firstPixel = 0;
            var thread = new Thread(() =>
            {
                Window? window = null;
                try
                {
                    var image = new AsyncThumbnailImage { PreviewWidth = 96 };
                    window = CreateWindow(image);
                    window.Show();
                    window.UpdateLayout();
                    image.SourcePath = oldPath;
                    image.SourcePath = newPath;
                    PumpUntil(window.Dispatcher, () => image.Source != null, TimeSpan.FromSeconds(3));
                    var source = Assert.IsAssignableFrom<BitmapSource>(image.Source);
                    var stride = source.PixelWidth * 4;
                    var pixels = new byte[stride * source.PixelHeight];
                    source.CopyPixels(pixels, stride, 0);
                    firstPixel = pixels[0];
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
            Assert.Equal((byte)231, firstPixel);
        }

        public void Dispose()
        {
            try { Directory.Delete(root, true); }
            catch { }
        }

        private static Window CreateWindow(UIElement content)
            => new Window
            {
                Content = content,
                Width = 120,
                Height = 120,
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

            var bitmap = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, width * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }
    }
}
