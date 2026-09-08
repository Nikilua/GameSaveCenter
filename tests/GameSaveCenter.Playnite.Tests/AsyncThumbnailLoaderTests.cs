using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Converters;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class AsyncThumbnailLoaderTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Thumbnail.Tests", Guid.NewGuid().ToString("N"));

        public AsyncThumbnailLoaderTests()
        {
            Directory.CreateDirectory(root);
        }

        [Fact]
        public async Task LoadAsync_ReturnsFrozenBoundedImage()
        {
            var path = Path.Combine(root, "thumb.png");
            WritePng(path, 60);

            var image = await AsyncThumbnailLoader.LoadAsync(path, 96, System.Threading.CancellationToken.None);

            Assert.NotNull(image);
            Assert.True(image!.IsFrozen);
            Assert.True(image.PixelWidth <= 96);
        }

        [Fact]
        public async Task LoadAsync_MissingFile_ReturnsNull()
        {
            Assert.Null(await AsyncThumbnailLoader.LoadAsync(Path.Combine(root, "missing.png"), 96, System.Threading.CancellationToken.None));
        }

        [Fact]
        public async Task LoadAsync_ServesCachedInstanceForUnchangedFile()
        {
            var path = Path.Combine(root, "cached.png");
            WritePng(path, 90);

            var first = await AsyncThumbnailLoader.LoadAsync(path, 96, System.Threading.CancellationToken.None);
            var second = await AsyncThumbnailLoader.LoadAsync(path, 96, System.Threading.CancellationToken.None);

            Assert.Same(first, second);
        }

        [Fact]
        public async Task LoadAsync_InvalidImageReturnsNullAndDoesNotKeepTheFileOpen()
        {
            var path = Path.Combine(root, "invalid.png");
            File.WriteAllText(path, "not an image");

            Assert.Null(await AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None));

            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Fact]
        public async Task LoadAsync_ConcurrentScrollWindowStaysWithinDecodeAndCacheBounds()
        {
            AsyncThumbnailLoader.ClearCache();
            AsyncThumbnailLoader.ResetDiagnostics();
            var paths = Enumerable.Range(0, 120)
                .Select(index =>
                {
                    var path = Path.Combine(root, "window-" + index + ".png");
                    WritePng(path, 64, 64, (byte)index);
                    return path;
                })
                .ToArray();

            var images = await Task.WhenAll(paths.Select(path => AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None)));
            Assert.All(images, image => Assert.NotNull(image));

            var diagnostics = AsyncThumbnailLoader.CaptureDiagnostics();
            Assert.Equal(120, diagnostics.RequestCount);
            Assert.InRange(diagnostics.PeakConcurrentDecodes, 1, 3);
            Assert.InRange(diagnostics.CacheCount, 1, diagnostics.CacheLimit);
            Assert.Equal(diagnostics.DecodeStartCount, diagnostics.DecodeSuccessCount);

            foreach (var path in paths)
                File.Delete(path);
            Assert.All(paths, path => Assert.False(File.Exists(path)));
        }

        [Fact]
        public async Task LoadAsync_CancelledRequestDoesNotReturnAnImage()
        {
            var path = Path.Combine(root, "cancelled.png");
            WritePng(path, 64, 64, 91);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => AsyncThumbnailLoader.LoadAsync(path, 96, cancellation.Token));
        }

        public void Dispose()
        {
            try { Directory.Delete(root, true); }
            catch { }
        }

        private static void WritePng(string path, byte value)
            => WritePng(path, 1, 1, value);

        private static void WritePng(string path, int width, int height, byte value)
        {
            var pixels = Enumerable.Repeat(value, width * height * 4).ToArray();
            for (var index = 0; index < width * height; index++)
            {
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
    }
}
