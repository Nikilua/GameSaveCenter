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
    internal sealed class AsyncThumbnailSourceDimensions
    {
        public AsyncThumbnailSourceDimensions(int pixelWidth, int pixelHeight)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }

        public int PixelWidth { get; }
        public int PixelHeight { get; }
        public string DisplayText => $"{PixelWidth} × {PixelHeight} px";
    }

    internal sealed class AsyncThumbnailLoaderDiagnostics
    {
        public AsyncThumbnailLoaderDiagnostics(
            int cacheCount,
            int cacheLimit,
            long cachedBytes,
            long cacheByteLimit,
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
            CachedBytes = cachedBytes;
            CacheByteLimit = cacheByteLimit;
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
        public long CachedBytes { get; }
        public long CacheByteLimit { get; }
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
        internal const int MaximumDecodeDimension = 2048;
        private const long CacheByteLimit = 64L * 1024 * 1024;
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, BitmapSource> Cache = new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, long> CacheEntryBytes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private static readonly LinkedList<string> Recency = new LinkedList<string>();
        private static long cachedBytes;
        private static long requestCount;
        private static long cacheHitCount;
        private static long decodeStartCount;
        private static long decodeSuccessCount;
        private static long cancellationCount;
        private static long failureCount;
        private static int activeDecodes;
        private static int peakConcurrentDecodes;

        public static Task<BitmapSource?> LoadAsync(string path, int width, CancellationToken token)
            => LoadAsync(path, width, token, preserveSourceSize: false);

        internal static async Task<BitmapSource?> LoadAsync(string path, int width, CancellationToken token, bool preserveSourceSize)
        {
            Interlocked.Increment(ref requestCount);
            try
            {
                // Task.Run guarantees the file metadata probe never runs on the caller's
                // thread (normally the WPF Dispatcher), even when the semaphore is
                // immediately available.
                var request = await Task.Run(() => PrepareRequest(path, width, preserveSourceSize), token).ConfigureAwait(false);
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
                        var image = await Task.Run(() => Decode(request.Path, request.DecodePixelWidth, request.DecodePixelHeight, request.Key, token), token).ConfigureAwait(false);
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

        /// <summary>
        /// Reads only the selected image's frame metadata on a worker thread. The full
        /// source dimensions remain available even when the displayed bitmap is capped
        /// to a thumbnail width.
        /// </summary>
        internal static Task<AsyncThumbnailSourceDimensions?> ReadSourceDimensionsAsync(string path, CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsImage(path))
                        return null;

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    var decoder = BitmapDecoder.Create(
                        stream,
                        BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.None);
                    if (decoder.Frames.Count == 0)
                        return null;

                    var frame = decoder.Frames[0];
                    return frame.PixelWidth > 0 && frame.PixelHeight > 0
                        ? new AsyncThumbnailSourceDimensions(frame.PixelWidth, frame.PixelHeight)
                        : null;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception) when (!(exception is OutOfMemoryException) && !(exception is StackOverflowException))
                {
                    return null;
                }
            }, token);
        }

        internal static AsyncThumbnailLoaderDiagnostics CaptureDiagnostics()
        {
            int cacheCount;
            long currentCachedBytes;
            lock (CacheLock)
            {
                cacheCount = Cache.Count;
                currentCachedBytes = cachedBytes;
            }

            return new AsyncThumbnailLoaderDiagnostics(
                cacheCount,
                CacheLimit,
                currentCachedBytes,
                CacheByteLimit,
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

        private static LoadRequest? PrepareRequest(string path, int requestedDimension, bool preserveSourceSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsImage(path)) return null;
                var info = new FileInfo(path);
                var maximumDimension = Math.Max(48, Math.Min(requestedDimension, MaximumDecodeDimension));
                int sourceWidth;
                int sourceHeight;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    var decoder = BitmapDecoder.Create(
                        stream,
                        BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.None);
                    if (decoder.Frames.Count == 0) return null;
                    sourceWidth = decoder.Frames[0].PixelWidth;
                    sourceHeight = decoder.Frames[0].PixelHeight;
                }

                if (sourceWidth <= 0 || sourceHeight <= 0) return null;
                var sourceLongEdge = Math.Max(sourceWidth, sourceHeight);
                var decodeLongEdge = preserveSourceSize ? Math.Min(maximumDimension, sourceLongEdge) : maximumDimension;
                var decodeWidth = sourceWidth >= sourceHeight ? decodeLongEdge : 0;
                var decodeHeight = sourceHeight > sourceWidth ? decodeLongEdge : 0;
                var key = string.Concat(path, "|", decodeWidth.ToString(CultureInfo.InvariantCulture), "x", decodeHeight.ToString(CultureInfo.InvariantCulture), "|",
                    info.Length.ToString(CultureInfo.InvariantCulture), "|",
                    info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture));
                return new LoadRequest(path, decodeWidth, decodeHeight, key, TryGetCached(key));
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

        private static BitmapSource Decode(string path, int decodeWidth, int decodeHeight, string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var timer = Stopwatch.StartNew();
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            if (decodeWidth > 0)
                image.DecodePixelWidth = decodeWidth;
            else
                image.DecodePixelHeight = decodeHeight;
            image.StreamSource = stream;
            image.EndInit();
            timer.Stop();
            Logger.Debug($"[PERF] Thumbnail decode={timer.ElapsedMilliseconds}ms bound={decodeWidth}x{decodeHeight} path={path}");
            image.Freeze();
            AddToCache(key, image);
            return image;
        }

        public static void ClearCache()
        {
            lock (CacheLock)
            {
                Cache.Clear();
                CacheEntryBytes.Clear();
                Recency.Clear();
                cachedBytes = 0;
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
                || exception is FormatException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException
                || exception is NotSupportedException
                || exception is InvalidOperationException;

        private static void AddToCache(string key, BitmapSource image)
        {
            lock (CacheLock)
            {
                if (Cache.ContainsKey(key)) return;
                var imageBytes = Math.Max(1L, (long)image.PixelWidth * image.PixelHeight * 4L);
                if (imageBytes > CacheByteLimit) return;
                Recency.AddFirst(key);
                Cache[key] = image;
                CacheEntryBytes[key] = imageBytes;
                cachedBytes += imageBytes;
                while ((Cache.Count > CacheLimit || cachedBytes > CacheByteLimit) && Recency.Last != null)
                {
                    var expired = Recency.Last;
                    Recency.RemoveLast();
                    Cache.Remove(expired.Value);
                    if (CacheEntryBytes.TryGetValue(expired.Value, out var expiredBytes))
                    {
                        cachedBytes = Math.Max(0, cachedBytes - expiredBytes);
                        CacheEntryBytes.Remove(expired.Value);
                    }
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
            public LoadRequest(string path, int decodePixelWidth, int decodePixelHeight, string key, BitmapSource? cached)
            {
                Path = path;
                DecodePixelWidth = decodePixelWidth;
                DecodePixelHeight = decodePixelHeight;
                Key = key;
                Cached = cached;
            }

            public string Path { get; }
            public int DecodePixelWidth { get; }
            public int DecodePixelHeight { get; }
            public string Key { get; }
            public BitmapSource? Cached { get; }
        }
    }
}
