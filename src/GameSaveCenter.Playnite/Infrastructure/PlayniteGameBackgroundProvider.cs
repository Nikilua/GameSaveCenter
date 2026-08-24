using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// UI-only provider for the selected game's Playnite background. It resolves only local
    /// Playnite cache/database files and never performs network IO on the dashboard path.
    /// </summary>
    public sealed class PlayniteGameBackgroundProvider
    {
        private const int CacheLimit = 6;
        private const int DecodePixelWidth = 1920;
        private readonly IPlayniteAPI api;
        private readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> recency = new LinkedList<string>();

        public PlayniteGameBackgroundProvider(IPlayniteAPI api)
        {
            this.api = api;
        }

        /// <summary>
        /// Resolves the Playnite reference on the caller's UI thread and performs the larger
        /// bitmap decode off-thread, so changing the selected game does not block the shell.
        /// </summary>
        public Task<ImageSource?> LoadAsync(Guid gameId, CancellationToken token)
        {
            string? path;
            try
            {
                var game = api.Database.Games.Get(gameId);
                var reference = game?.BackgroundImage;
                path = ResolveLocalPath(reference, api.Database.GetFullFilePath);
            }
            catch
            {
                return Task.FromResult<ImageSource?>(null);
            }

            if (path == null) return Task.FromResult<ImageSource?>(null);
            var key = gameId.ToString("D") + "|" + path;
            lock (cache)
            {
                if (cache.TryGetValue(key, out var cached))
                {
                    recency.Remove(key);
                    recency.AddFirst(key);
                    return Task.FromResult<ImageSource?>(cached);
                }
            }

            return Task.Run(() => DecodeAndCache(key, path), token);
        }

        private ImageSource? DecodeAndCache(string key, string path)
        {
            var image = DecodeLocalImage(path);
            if (image == null) return null;
            lock (cache)
            {
                cache[key] = image;
                recency.Remove(key);
                recency.AddFirst(key);
                while (cache.Count > CacheLimit && recency.Last != null)
                {
                    var expired = recency.Last;
                    recency.RemoveLast();
                    cache.Remove(expired.Value);
                }
            }
            return image;
        }

        public static BitmapImage? DecodeLocalImage(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)
                    || path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return null;
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = DecodePixelWidth;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public static string? ResolveLocalPath(string? reference, Func<string, string> getFullFilePath)
        {
            if (string.IsNullOrWhiteSpace(reference)) return null;
            var path = getFullFilePath(reference!);
            if (string.IsNullOrWhiteSpace(path)
                || path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || !File.Exists(path))
            {
                return null;
            }
            return path;
        }
    }
}
