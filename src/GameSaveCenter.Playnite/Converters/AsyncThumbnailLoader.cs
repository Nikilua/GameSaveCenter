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
    internal sealed class AsyncThumbnailLoaderDiagnostics
    {
        public AsyncThumbnailLoaderDiagnostics(
            int cacheCount,
            int cacheLimit,
            int activeDecodes,
            int peakConcurrentDecodes,
            long requestCount,
            long cacheHitCount,
            long decodeStartCount,
            long decodeSuccessCount,
            long cancellationCount,
            long failureCount)
        {
            CacheCount = cacheCount;
            CacheLimit = cacheLimit;
            ActiveDecodes = activeDecodes;
            PeakConcurrentDecodes = peakConcurrentDecodes;
            RequestCount = requestCount;
            CacheHitCount = cacheHitCount;
            DecodeStartCount = decodeStartCount;
            DecodeSuccessCount = decodeSuccessCount;
            CancellationCount = cancellationCount;
            FailureCount = failureCount;
        }

        public int CacheCount { get; }
        public int CacheLimit { get; }
        public int ActiveDecodes { get; }
        public int PeakConcurrentDecodes { get; }
        public long RequestCount { get; }
        public long CacheHitCount { get; }
        public long DecodeStartCount { get; }
        public long DecodeSuccessCount { get; }
        public long CancellationCount { get; }
        public long FailureCount { get; }
    }

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
        private static long requestCount;
        private static long cacheHitCount;
        private static long decodeStartCount;
        private static long decodeSuccessCount;
        private static long cancellationCount;
        private static long failureCount;
        private static int activeDecodes;
        private static int peakConcurrentDecodes;

        public static async Task<BitmapSource?> LoadAsync(string path, int width, CancellationToken token)
        {
            Interlocked.Increment(ref requestCount);
            try
            {
                // Task.Run guarantees the file metadata probe never runs on the caller's
                // thread (normally the WPF Dispatcher), even when the semaphore is
                // immediately available.
                var request = await Task.Run(() => PrepareRequest(path, width), token).ConfigureAwait(false);
                if (request == null)
                {
                    Interlocked.Increment(ref failureCount);
                    return null;
                }

                if (request.Cached != null)
                {
                    Interlocked.Increment(ref cacheHitCount);
                    return request.Cached;
                }

                await Gate.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    var cached = await Task.Run(() => TryGetCached(request.Key), token).ConfigureAwait(false);
                    if (cached != null)
                    {
                        Interlocked.Increment(ref cacheHitCount);
                        return cached;
                    }

                    Interlocked.Increment(ref decodeStartCount);
                    var active = Interlocked.Increment(ref activeDecodes);
                    UpdatePeak(active);
                    try
                    {
                        var image = await Task.Run(() => Decode(request.Path, request.Width, request.Key, token), token).ConfigureAwait(false);
                        Interlocked.Increment(ref decodeSuccessCount);
                        return image;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex) when (IsExpectedLoadFailure(ex))
                    {
                        Interlocked.Increment(ref failureCount);
                        return null;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeDecodes);
                    }
                }
                finally
                {
                    Gate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref cancellationCount);
                throw;
            }
            catch (Exception ex) when (IsExpectedLoadFailure(ex))
            {
                Interlocked.Increment(ref failureCount);
                return null;
            }
        }

        internal static AsyncThumbnailLoaderDiagnostics CaptureDiagnostics()
        {
            int cacheCount;
            lock (CacheLock)
            {
                cacheCount = Cache.Count;
            }

            return new AsyncThumbnailLoaderDiagnostics(
                cacheCount,
                CacheLimit,
                Volatile.Read(ref activeDecodes),
                Volatile.Read(ref peakConcurrentDecodes),
                Interlocked.Read(ref requestCount),
                Interlocked.Read(ref cacheHitCount),
                Interlocked.Read(ref decodeStartCount),
                Interlocked.Read(ref decodeSuccessCount),
                Interlocked.Read(ref cancellationCount),
                Interlocked.Read(ref failureCount));
        }

        internal static void ResetDiagnostics()
        {
            Interlocked.Exchange(ref requestCount, 0);
            Interlocked.Exchange(ref cacheHitCount, 0);
            Interlocked.Exchange(ref decodeStartCount, 0);
            Interlocked.Exchange(ref decodeSuccessCount, 0);
            Interlocked.Exchange(ref cancellationCount, 0);
            Interlocked.Exchange(ref failureCount, 0);
            Interlocked.Exchange(ref activeDecodes, 0);
            Interlocked.Exchange(ref peakConcurrentDecodes, 0);
        }

        private static LoadRequest? PrepareRequest(string path, int width)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsImage(path)) return null;
                var info = new FileInfo(path);
                var boundedWidth = Math.Max(48, Math.Min(width, 480));
                var key = string.Concat(path, "|", boundedWidth.ToString(CultureInfo.InvariantCulture), "|",
                    info.Length.ToString(CultureInfo.InvariantCulture), "|",
                    info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture));
                return new LoadRequest(path, boundedWidth, key, TryGetCached(key));
            }
            catch (Exception ex) when (IsExpectedLoadFailure(ex))
            {
                return null;
            }
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

        private static void UpdatePeak(int active)
        {
            while (true)
            {
                var observed = Volatile.Read(ref peakConcurrentDecodes);
                if (observed >= active || Interlocked.CompareExchange(ref peakConcurrentDecodes, active, observed) == observed)
                    return;
            }
        }

        private static bool IsExpectedLoadFailure(Exception exception)
            => exception is IOException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException
                || exception is NotSupportedException
                || exception is InvalidOperationException;

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
