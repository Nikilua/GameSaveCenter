using System;
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
        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
            nameof(SourcePath), typeof(string), typeof(AsyncThumbnailImage),
            new PropertyMetadata(null, OnSourcePathChanged));

        public static readonly DependencyProperty PreviewWidthProperty = DependencyProperty.Register(
            nameof(PreviewWidth), typeof(int), typeof(AsyncThumbnailImage),
            new PropertyMetadata(480, OnPreviewWidthChanged));

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
            if (string.IsNullOrWhiteSpace(path) || !IsLoaded || !IsVisible)
            {
                Source = null;
                return;
            }

            Source = null;
            var width = Math.Max(48, Math.Min(PreviewWidth, 480));
            var cancellation = new CancellationTokenSource();
            pending = cancellation;
            _ = LoadAsync(path!, width, expected, cancellation.Token);
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
                Source = null;
        }

        private async Task LoadAsync(string path, int width, int expected, CancellationToken token)
        {
            try
            {
                var image = await AsyncThumbnailLoader.LoadAsync(path, width, token).ConfigureAwait(false);
                if (token.IsCancellationRequested || expected != Volatile.Read(ref generation)) return;
                await Dispatcher.InvokeAsync(() =>
                {
                    if (expected != Volatile.Read(ref generation) || token.IsCancellationRequested) return;
                    Source = image;
                }, DispatcherPriority.Background);
            }
            catch (OperationCanceledException)
            {
                // The path was replaced or the control was unloaded before decode finished.
            }
            catch
            {
                // Keep the placeholder; missing/corrupt media never tears down the list.
            }
        }
    }
}
