using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Converters;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Image that decodes thumbnails on background workers instead of the UI thread. It
    /// shows a placeholder (empty source) until the bounded loader returns a frozen bitmap;
    /// stale loads for replaced paths are ignored.
    /// </summary>
    public sealed class AsyncThumbnailImage : System.Windows.Controls.Image
    {
        public static readonly DependencyProperty PreviewStateProperty = DependencyProperty.Register(
            nameof(PreviewState), typeof(string), typeof(AsyncThumbnailImage),
            new PropertyMetadata("Idle", OnPreviewStateChanged));
        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
            nameof(SourcePath), typeof(string), typeof(AsyncThumbnailImage),
            new PropertyMetadata(null, OnSourcePathChanged));

        public static readonly DependencyProperty PreviewWidthProperty = DependencyProperty.Register(
            nameof(PreviewWidth), typeof(int), typeof(AsyncThumbnailImage),
            new PropertyMetadata(480, OnPreviewWidthChanged));

        public static readonly DependencyProperty PreserveSourceSizeProperty = DependencyProperty.Register(
            nameof(PreserveSourceSize), typeof(bool), typeof(AsyncThumbnailImage),
            new PropertyMetadata(false, OnPreviewWidthChanged));

        public static readonly DependencyProperty PreviewDimensionsProperty = DependencyProperty.Register(
            nameof(PreviewDimensions), typeof(string), typeof(AsyncThumbnailImage),
            new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty SourceDimensionsProperty = DependencyProperty.Register(
            nameof(SourceDimensions), typeof(string), typeof(AsyncThumbnailImage),
            new PropertyMetadata(string.Empty));

        private int generation;
        private CancellationTokenSource? pending;

        public AsyncThumbnailImage()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        public string? SourcePath
        {
            get => (string?)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        public int PreviewWidth
        {
            get => (int)GetValue(PreviewWidthProperty);
            set => SetValue(PreviewWidthProperty, value);
        }

        /// <summary>
        /// Avoids upscaling small source images. Larger sources remain capped by PreviewWidth.
        /// </summary>
        public bool PreserveSourceSize
        {
            get => (bool)GetValue(PreserveSourceSizeProperty);
            set => SetValue(PreserveSourceSizeProperty, value);
        }

        public string PreviewDimensions
        {
            get => (string)GetValue(PreviewDimensionsProperty);
            private set => SetValue(PreviewDimensionsProperty, value);
        }

        public string SourceDimensions
        {
            get => (string)GetValue(SourceDimensionsProperty);
            private set => SetValue(SourceDimensionsProperty, value);
        }

        public string PreviewState
        {
            get => (string)GetValue(PreviewStateProperty);
            private set => SetValue(PreviewStateProperty, value);
        }

        public event EventHandler? PreviewStateChanged;

        internal Action? BeforePublishForTest { get; set; }

        private static void OnPreviewStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((AsyncThumbnailImage)d).PreviewStateChanged?.Invoke(d, EventArgs.Empty);

        private static void OnSourcePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((AsyncThumbnailImage)d).StartLoad();

        private static void OnPreviewWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((AsyncThumbnailImage)d).StartLoad();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Source == null && IsVisible && !string.IsNullOrWhiteSpace(SourcePath))
                StartLoad();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (Source == null && IsLoaded && !string.IsNullOrWhiteSpace(SourcePath))
                    StartLoad();
                return;
            }

            CancelPendingLoad(clearSource: true);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CancelPendingLoad(clearSource: true);
        }

        private void StartLoad()
        {
            CancelPendingLoad(clearSource: false);
            var expected = Interlocked.Increment(ref generation);

            var path = SourcePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                Source = null;
                PreviewDimensions = string.Empty;
                SourceDimensions = string.Empty;
                PreviewState = "NoImage";
                return;
            }

            if (!IsLoaded || !IsVisible)
            {
                Source = null;
                PreviewDimensions = string.Empty;
                SourceDimensions = string.Empty;
                PreviewState = "Unavailable";
                return;
            }

            Source = null;
            PreviewDimensions = string.Empty;
            SourceDimensions = string.Empty;
            PreviewState = "Loading";
            var width = Math.Max(48, Math.Min(PreviewWidth, AsyncThumbnailLoader.MaximumDecodeDimension));
            var preserveSourceSize = PreserveSourceSize;
            var cancellation = new CancellationTokenSource();
            pending = cancellation;
            _ = LoadAsync(path!, width, preserveSourceSize, expected, cancellation.Token);
        }

        private void CancelPendingLoad(bool clearSource)
        {
            Interlocked.Increment(ref generation);
            var previous = Interlocked.Exchange(ref pending, null);
            if (previous != null)
            {
                previous.Cancel();
                previous.Dispose();
            }

            if (clearSource)
            {
                Source = null;
                PreviewDimensions = string.Empty;
                SourceDimensions = string.Empty;
            }
        }

        private async Task LoadAsync(string path, int width, bool preserveSourceSize, int expected, CancellationToken token)
        {
            try
            {
                var imageTask = AsyncThumbnailLoader.LoadAsync(path, width, token, preserveSourceSize);
                var dimensionsTask = AsyncThumbnailLoader.ReadSourceDimensionsAsync(path, token);
                await Task.WhenAll(imageTask, dimensionsTask).ConfigureAwait(false);
                var image = await imageTask.ConfigureAwait(false);
                var sourceDimensions = await dimensionsTask.ConfigureAwait(false);
                if (token.IsCancellationRequested || expected != Volatile.Read(ref generation)) return;
                BeforePublishForTest?.Invoke();
                await Dispatcher.InvokeAsync(() =>
                {
                     if (expected != Volatile.Read(ref generation) || token.IsCancellationRequested) return;
                     Source = image;
                     SourceDimensions = image == null ? string.Empty : sourceDimensions?.DisplayText ?? string.Empty;
                     PreviewDimensions = image == null
                         ? string.Empty
                         : $"{image.PixelWidth} × {image.PixelHeight} px";
                     PreviewState = image == null ? ClassifyUnavailable(path) : "Ready";
                }, DispatcherPriority.Background);
            }
            catch (OperationCanceledException)
            {
                // The path was replaced or the control was unloaded before decode finished.
            }
            catch
            {
                // Keep the placeholder; missing/corrupt media never tears down the list.
                if (expected == Volatile.Read(ref generation))
                    await Dispatcher.InvokeAsync(() =>
                    {
                        PreviewDimensions = string.Empty;
                        SourceDimensions = string.Empty;
                        PreviewState = "Failed";
                    }, DispatcherPriority.Background);
            }
        }

        private static string ClassifyUnavailable(string path)
            => File.Exists(path) ? "Failed" : "Missing";
    }
}
