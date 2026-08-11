using System;
using System.IO;
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

        public void Dispose()
        {
            try { Directory.Delete(root, true); }
            catch { }
        }

        private static void WritePng(string path, byte value)
        {
            var pixels = new[] { value, (byte)(255 - value), (byte)127, (byte)255 };
            var bitmap = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }
    }
}
