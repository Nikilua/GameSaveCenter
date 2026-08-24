using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// UI-only provider for the selected game's Playnite background. Local Playnite cache files
    /// are preferred; a remote BackgroundImage is downloaded only for the one game the user
    /// selected, with cancellation, a short timeout and a strict size limit.
    /// </summary>
    public sealed class PlayniteGameBackgroundProvider
    {
        private const int CacheLimit = 6;
        private const int DecodePixelWidth = 1920;
        private const int MaxRemoteImageBytes = 12 * 1024 * 1024;
        private static readonly HttpClient RemoteClient = CreateRemoteClient();
        private readonly IPlayniteAPI api;
        private readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> recency = new LinkedList<string>();

        public PlayniteGameBackgroundProvider(IPlayniteAPI api)
        {
            this.api = api;
        }

        /// <summary>
        /// Resolves the Playnite reference on the caller's UI thread and performs the larger
        /// bitmap decode/download off-thread, so changing the selected game does not block the shell.
        /// </summary>
        public async Task<ImageSource?> LoadAsync(Guid gameId, CancellationToken token)
        {
            string? reference;
            try
            {
                var game = api.Database.Games.Get(gameId);
                reference = game?.BackgroundImage;
            }
            catch
            {
                return null;
            }

            string? path;
            Uri? remoteUri;
            try
            {
                path = ResolveLocalPath(reference, api.Database.GetFullFilePath);
                remoteUri = path == null ? ResolveRemoteUri(reference, api.Database.GetFullFilePath) : null;
            }
            catch
            {
                return null;
            }
            if (path == null && remoteUri == null) return null;

            var key = gameId.ToString("D") + "|" + (path ?? remoteUri!.AbsoluteUri);
            lock (cache)
            {
                if (cache.TryGetValue(key, out var cached))
                {
                    recency.Remove(key);
                    recency.AddFirst(key);
                    return cached;
                }
            }

            if (path != null)
                return await Task.Run(() => DecodeAndCache(key, path), token).ConfigureAwait(false);

            var bytes = await DownloadRemoteImageAsync(remoteUri!, token).ConfigureAwait(false);
            if (bytes == null || token.IsCancellationRequested) return null;
            return await Task.Run(() => DecodeAndCache(key, bytes), token).ConfigureAwait(false);
        }

        private ImageSource? DecodeAndCache(string key, string path)
        {
            var image = DecodeLocalImage(path);
            return AddToCache(key, image);
        }

        private ImageSource? DecodeAndCache(string key, byte[] bytes)
        {
            var image = DecodeImage(bytes);
            return AddToCache(key, image);
        }

        private ImageSource? AddToCache(string key, ImageSource? image)
        {
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

        private static HttpClient CreateRemoteClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GameSaveCenter/0.6");
            return client;
        }

        private static async Task<byte[]?> DownloadRemoteImageAsync(Uri uri, CancellationToken token)
        {
            try
            {
                using (var response = await RemoteClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode
                        || response.Content.Headers.ContentLength > MaxRemoteImageBytes)
                        return null;

                    using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var buffer = new MemoryStream())
                    {
                        var chunk = new byte[81920];
                        var total = 0;
                        while (true)
                        {
                            var read = await source.ReadAsync(chunk, 0, chunk.Length, token).ConfigureAwait(false);
                            if (read == 0) break;
                            total += read;
                            if (total > MaxRemoteImageBytes) return null;
                            buffer.Write(chunk, 0, read);
                        }
                        return buffer.ToArray();
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
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

        public static BitmapImage? DecodeImage(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0) return null;
                using (var stream = new MemoryStream(bytes, writable: false))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = DecodePixelWidth;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
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

        public static Uri? ResolveRemoteUri(string? reference, Func<string, string> getFullFilePath)
        {
            if (string.IsNullOrWhiteSpace(reference)) return null;
            var resolved = getFullFilePath(reference!);
            return TryCreateRemoteUri(reference) ?? TryCreateRemoteUri(resolved);
        }

        private static Uri? TryCreateRemoteUri(string? value)
        {
            var text = value?.Trim();
            if (string.IsNullOrWhiteSpace(text)
                || !Uri.TryCreate(text, UriKind.Absolute, out var uri)
                || uri == null
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return null;
            return uri;
        }
    }
}
