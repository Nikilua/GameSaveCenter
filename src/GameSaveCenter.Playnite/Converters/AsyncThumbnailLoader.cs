using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameSaveCenter.Playnite.Converters
{
    /// <summary>
    /// Bounded, cached, background thumbnail decoder. File IO and BitmapImage decode never
    /// run on the UI thread; at most three decodes run concurrently and every image is
    /// frozen before it can be handed back to the UI.
    /// </summary>
    public static class AsyncThumbnailLoader
    {
        private const int MaxConcurrency = 3;
        private const int CacheLimit = 96;
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, BitmapSource> Cache = new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
        private static readonly LinkedList<string> Recency = new LinkedList<string>();

        public static async Task<BitmapSource?> LoadAsync(string path, int width, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsImage(path)) return null;
            var info = new FileInfo(path);
            var key = string.Concat(path, "|", width.ToString(CultureInfo.InvariantCulture), "|",
                info.Length.ToString(CultureInfo.InvariantCulture), "|",
                info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture));

            lock (CacheLock)
            {
                if (Cache.TryGetValue(key, out var cached))
                {
                    Recency.Remove(key);
                    Recency.AddFirst(key);
                    return cached;
                }
            }

            await Gate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                lock (CacheLock)
                {
                    if (Cache.TryGetValue(key, out var cached)) return cached;
                }
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = Math.Max(48, Math.Min(width, 480));
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                AddToCache(key, image);
                return image;
            }
            finally
            {
                Gate.Release();
            }
        }

        public static void ClearCache()
        {
            lock (CacheLock)
            {
                Cache.Clear();
                Recency.Clear();
            }
        }

        private static void AddToCache(string key, BitmapSource image)
        {
            lock (CacheLock)
            {
                if (Cache.ContainsKey(key)) return;
                var node = Recency.AddFirst(key);
                Cache[key] = image;
                while (Cache.Count > CacheLimit && Recency.Last != null)
                {
                    var expired = Recency.Last;
                    Recency.RemoveLast();
                    Cache.Remove(expired.Value);
                }
            }
        }

        private static bool IsImage(string path)
        {
            var extension = Path.GetExtension(path);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".bmp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
        }
    }
}
