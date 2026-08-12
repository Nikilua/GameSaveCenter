using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Converters
{
    /// <summary>
    /// Bounded, cached, background thumbnail decoder. File IO and BitmapImage decode never
    /// run on the UI thread; at most three decodes run concurrently and every image is
    /// frozen before it can be handed back to the UI.
    /// </summary>
    public static class AsyncThumbnailLoader
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private const int MaxConcurrency = 3;
        private const int CacheLimit = 96;
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, BitmapSource> Cache = new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
        private static readonly LinkedList<string> Recency = new LinkedList<string>();

        public static async Task<BitmapSource?> LoadAsync(string path, int width, CancellationToken token)
        {
            // Task.Run guarantees the file metadata probe never runs on the caller's thread
            // (normally the WPF Dispatcher), even when the semaphore is immediately available.
            var request = await Task.Run(() => PrepareRequest(path, width), token).ConfigureAwait(false);
            if (request == null) return null;
            if (request.Cached != null) return request.Cached;

            await Gate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var cached = await Task.Run(() => TryGetCached(request.Key), token).ConfigureAwait(false);
                if (cached != null) return cached;
                return await Task.Run(() => Decode(request.Path, request.Width, request.Key, token), token).ConfigureAwait(false);
            }
            finally
            {
                Gate.Release();
            }
        }

        private static LoadRequest? PrepareRequest(string path, int width)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsImage(path)) return null;
            var info = new FileInfo(path);
            var key = string.Concat(path, "|", width.ToString(CultureInfo.InvariantCulture), "|",
                info.Length.ToString(CultureInfo.InvariantCulture), "|",
                info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            return new LoadRequest(path, Math.Max(48, Math.Min(width, 480)), key, TryGetCached(key));
        }

        private static BitmapSource? TryGetCached(string key)
        {
            lock (CacheLock)
            {
                if (!Cache.TryGetValue(key, out var cached)) return null;
                Recency.Remove(key);
                Recency.AddFirst(key);
                return cached;
            }
        }

        private static BitmapSource Decode(string path, int width, string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var timer = Stopwatch.StartNew();
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = width;
            image.StreamSource = stream;
            image.EndInit();
            timer.Stop();
            Logger.Debug($"[PERF] Thumbnail decode={timer.ElapsedMilliseconds}ms width={width} path={path}");
            image.Freeze();
            AddToCache(key, image);
            return image;
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

        private sealed class LoadRequest
        {
            public LoadRequest(string path, int width, string key, BitmapSource? cached)
            {
                Path = path;
                Width = width;
                Key = key;
                Cached = cached;
            }

            public string Path { get; }
            public int Width { get; }
            public string Key { get; }
            public BitmapSource? Cached { get; }
        }
    }
}
