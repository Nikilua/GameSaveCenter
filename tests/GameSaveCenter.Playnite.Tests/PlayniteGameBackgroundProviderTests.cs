using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class PlayniteGameBackgroundProviderTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Background.Tests", Guid.NewGuid().ToString("N"));

        public PlayniteGameBackgroundProviderTests()
        {
            Directory.CreateDirectory(root);
        }

        [Fact]
        public void ResolveLocalPathAcceptsPlayniteDatabaseFileAndRejectsRemoteReference()
        {
            var path = Path.Combine(root, "background.jpg");
            File.WriteAllText(path, "image");

            Assert.Equal(path, PlayniteGameBackgroundProvider.ResolveLocalPath("background.jpg", _ => path));
            Assert.Null(PlayniteGameBackgroundProvider.ResolveLocalPath("background.jpg", _ => "https://example.com/background.jpg"));
        }

        [Fact]
        public void ResolveRemoteUriAcceptsPlayniteBackgroundUrlWithoutTreatingLocalFilesAsRemote()
        {
            var uri = PlayniteGameBackgroundProvider.ResolveRemoteUri(
                "https://example.com/background.jpg",
                _ => "https://example.com/background.jpg");

            Assert.Equal("https://example.com/background.jpg", uri?.AbsoluteUri);
            Assert.Null(PlayniteGameBackgroundProvider.ResolveRemoteUri("background.jpg", _ => root));
        }

        [Fact]
        public void DecodeLocalImageReturnsFrozenBoundedBitmap()
        {
            var path = Path.Combine(root, "background.png");
            File.WriteAllBytes(path, Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAACqaXHeAAAAEElEQVR42mNkYPj/n4GBgYGJAQoA" +
                "AAABAgEAAPcQ5fIAAAAASUVORK5CYII="));

            var image = PlayniteGameBackgroundProvider.DecodeLocalImage(path);

            Assert.NotNull(image);
            Assert.True(image!.IsFrozen);
            Assert.InRange(image.PixelWidth, 1, 1920);
        }

        [Fact]
        public void DecodeImageBytesReturnsFrozenBoundedBitmap()
        {
            var bytes = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAACqaXHeAAAAEElEQVR42mNkYPj/n4GBgYGJAQoA" +
                "AAABAgEAAPcQ5fIAAAAASUVORK5CYII=");

            var image = PlayniteGameBackgroundProvider.DecodeImage(bytes);

            Assert.NotNull(image);
            Assert.True(image!.IsFrozen);
            Assert.InRange(image.PixelWidth, 1, 1920);
        }

        [Fact]
        public void AmbientBrushIsFrozenAndUsesTheDecodedBackground()
        {
            var bytes = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAACqaXHeAAAAEElEQVR42mNkYPj/n4GBgYGJAQoA" +
                "AAABAgEAAPcQ5fIAAAAASUVORK5CYII=");
            var image = PlayniteGameBackgroundProvider.DecodeImage(bytes);

            Assert.NotNull(image);
            var brush = PlayniteGameBackgroundProvider.CreateAmbientBrush(image!);

            Assert.True(brush.IsFrozen);
            Assert.Equal(4, brush.GradientStops.Count);
            Assert.Contains(brush.GradientStops, stop => stop.Color.A > 0);
        }

        public void Dispose()
        {
            try { Directory.Delete(root, true); }
            catch { }
        }
    }
}
