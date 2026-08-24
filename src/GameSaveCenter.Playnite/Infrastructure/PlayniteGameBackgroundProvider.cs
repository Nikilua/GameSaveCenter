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
    /// The decoded background plus the low-alpha material derived from the same pixels. Keeping
    /// these together prevents the shell from showing a fixed green wash over every game image.
    /// </summary>
    public sealed class GameBackgroundVisual
    {
        public GameBackgroundVisual(ImageSource image, Brush ambientBrush)
        {
            Image = image;
            AmbientBrush = ambientBrush;
        }

        public ImageSource Image { get; }

        public Brush AmbientBrush { get; }
    }

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
        private readonly Dictionary<string, GameBackgroundVisual> cache = new Dictionary<string, GameBackgroundVisual>(StringComparer.OrdinalIgnoreCase);
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
            var visual = await LoadVisualAsync(gameId, token).ConfigureAwait(false);
            return visual?.Image;
        }

        /// <summary>
        /// Loads the selected game's real background and derives a bounded material brush from the
        /// same image. The material is deliberately low-alpha: it follows the artwork without
        /// replacing the artwork or reducing text contrast across the plugin.
        /// </summary>
        public async Task<GameBackgroundVisual?> LoadVisualAsync(Guid gameId, CancellationToken token)
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

        private GameBackgroundVisual? DecodeAndCache(string key, string path)
        {
            var image = DecodeLocalImage(path);
            return AddToCache(key, image);
        }

        private GameBackgroundVisual? DecodeAndCache(string key, byte[] bytes)
        {
            var image = DecodeImage(bytes);
            return AddToCache(key, image);
        }

        private GameBackgroundVisual? AddToCache(string key, BitmapImage? image)
        {
            if (image == null) return null;
            var visual = new GameBackgroundVisual(image, CreateAmbientBrush(image));
            lock (cache)
            {
                cache[key] = visual;
                recency.Remove(key);
                recency.AddFirst(key);
                while (cache.Count > CacheLimit && recency.Last != null)
                {
                    var expired = recency.Last;
                    recency.RemoveLast();
                    cache.Remove(expired.Value);
                }
            }
            return visual;
        }

        /// <summary>
        /// Samples a few broad areas instead of trying to reproduce the full bitmap as a brush.
        /// This keeps the material cheap and lets the actual image remain the source of truth.
        /// </summary>
        internal static LinearGradientBrush CreateAmbientBrush(BitmapSource image)
        {
            try
            {
                var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
                converted.Freeze();
                var width = converted.PixelWidth;
                var height = converted.PixelHeight;
                if (width < 1 || height < 1) return TransparentGradient();

                var stride = width * 4;
                var pixels = new byte[stride * height];
                converted.CopyPixels(pixels, stride, 0);
                var topLeft = Sample(pixels, stride, width, height, 0.12, 0.18);
                var topRight = Sample(pixels, stride, width, height, 0.88, 0.18);
                var center = Sample(pixels, stride, width, height, 0.50, 0.50);
                var bottomLeft = Sample(pixels, stride, width, height, 0.12, 0.82);
                var bottomRight = Sample(pixels, stride, width, height, 0.88, 0.82);
                var brush = new LinearGradientBrush
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = new System.Windows.Point(1, 1)
                };
                brush.GradientStops.Add(new GradientStop(MaterialColor(topLeft, 0.24), 0));
                brush.GradientStops.Add(new GradientStop(MaterialColor(Blend(center, topRight, 0.35), 0.18), 0.30));
                brush.GradientStops.Add(new GradientStop(MaterialColor(Blend(center, bottomLeft, 0.42), 0.16), 0.60));
                brush.GradientStops.Add(new GradientStop(MaterialColor(bottomRight, 0.22), 1));
                brush.Freeze();
                return brush;
            }
            catch
            {
                return TransparentGradient();
            }
        }

        private static Color Sample(byte[] pixels, int stride, int width, int height, double x, double y)
        {
            var px = Math.Max(0, Math.Min(width - 1, (int)Math.Round((width - 1) * x)));
            var py = Math.Max(0, Math.Min(height - 1, (int)Math.Round((height - 1) * y)));
            var offset = (py * stride) + (px * 4);
            return Color.FromRgb(pixels[offset + 2], pixels[offset + 1], pixels[offset]);
        }

        private static Color Blend(Color first, Color second, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromRgb(
                (byte)Math.Round(first.R + ((second.R - first.R) * amount)),
                (byte)Math.Round(first.G + ((second.G - first.G) * amount)),
                (byte)Math.Round(first.B + ((second.B - first.B) * amount)));
        }

        private static Color MaterialColor(Color source, double alpha)
        {
            // Pull very bright artwork toward a neutral reading surface while preserving hue.
            var softened = Blend(source, Color.FromRgb(30, 34, 42), 0.28);
            return Color.FromArgb((byte)Math.Round(255 * alpha), softened.R, softened.G, softened.B);
        }

        private static LinearGradientBrush TransparentGradient()
        {
            var brush = new LinearGradientBrush(Colors.Transparent, Colors.Transparent, 45);
            brush.Freeze();
            return brush;
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
