using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using System.Windows.Media.Imaging;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class PlayniteGameIconProviderTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Icon.Tests", Guid.NewGuid().ToString("N"));

        public PlayniteGameIconProviderTests()
        {
            Directory.CreateDirectory(root);
        }

        [Fact]
        public void ResolveLocalPath_ReturnsExistingLocalFile()
        {
            var path = Path.Combine(root, "icon.png");
            File.WriteAllText(path, "png");

            Assert.Equal(path, PlayniteGameIconProvider.ResolveLocalPath("icon.png", _ => path));
        }

        [Fact]
        public void ResolveLocalPath_NullOrMissingFallsBack()
        {
            Assert.Null(PlayniteGameIconProvider.ResolveLocalPath(null, _ => root));
            Assert.Null(PlayniteGameIconProvider.ResolveLocalPath("missing.png", _ => Path.Combine(root, "missing.png")));
        }

        [Fact]
        public void ResolveLocalPath_RemoteUrlFallsBackWithoutNetwork()
        {
            Assert.Null(PlayniteGameIconProvider.ResolveLocalPath("icon.png", _ => "https://example.com/icon.png"));
            Assert.Null(PlayniteGameIconProvider.ResolveLocalPath("icon.png", _ => "http://example.com/icon.png"));
        }

        [Fact]
        public void DecodeLocalImageUsesSmallFrozenBitmap()
        {
            var path = Path.Combine(root, "icon.png");
            File.WriteAllBytes(path, Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAACqaXHeAAAAEElEQVR42mNkYPj/n4GBgYGJAQoA" +
                "AAABAgEAAPcQ5fIAAAAASUVORK5CYII="));

            var image = PlayniteGameIconProvider.DecodeLocalImage(path);

            Assert.NotNull(image);
            Assert.True(image!.IsFrozen);
            Assert.InRange(image.PixelWidth, 1, 48);
        }

        public void Dispose()
        {
            try { Directory.Delete(root, true); }
            catch { }
        }
    }
}
