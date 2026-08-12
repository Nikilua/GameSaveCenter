using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// UI-only provider for the currently selected game's Playnite icon. It never touches
    /// Worker/Contracts, never performs network IO, and caches only recently selected games
    /// so opening a 1000-game dashboard decodes at most one icon.
    /// </summary>
    public sealed class PlayniteGameIconProvider
    {
        private const int CacheLimit = 48;
        private readonly IPlayniteAPI api;
        private readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> recency = new LinkedList<string>();

        public PlayniteGameIconProvider(IPlayniteAPI api)
        {
            this.api = api;
        }

        public ImageSource? Load(Guid gameId)
        {
            var key = gameId.ToString("D");
            lock (cache)
            {
                if (cache.TryGetValue(key, out var cached))
                {
                    recency.Remove(key);
                    recency.AddFirst(key);
                    return cached;
                }
            }

            var image = Decode(gameId);
            if (image == null) return null;
            lock (cache)
            {
                cache[key] = image;
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

        private ImageSource? Decode(Guid gameId)
        {
            try
            {
                var game = api.Database.Games.Get(gameId);
                var reference = game?.Icon;
                if (string.IsNullOrWhiteSpace(reference)) return null;
                var path = ResolveLocalPath(reference, api.Database.GetFullFilePath);
                if (path == null) return null;

                return DecodeLocalImage(path);
            }
            catch
            {
                return null;
            }
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
                image.DecodePixelWidth = 48;
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
