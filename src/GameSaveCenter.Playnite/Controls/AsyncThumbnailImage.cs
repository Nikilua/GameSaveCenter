using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Converters;
using GameSaveCenter.Playnite.Infrastructure;

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
        private static readonly DependencyPropertyKey IsLoadingFeedbackReadyPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsLoadingFeedbackReady), typeof(bool), typeof(AsyncThumbnailImage), new PropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingFeedbackReadyProperty = IsLoadingFeedbackReadyPropertyKey.DependencyProperty;
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
        private DispatcherTimer? loadingFeedbackTimer;
        private bool uiAnimationsEnabled;

        private static readonly TimeSpan LoadingFeedbackDelay = TimeSpan.FromMilliseconds(180);

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

        /// <summary>
        /// Controls only the ready-image entrance. Preview state, decode timing and source
        /// publication remain immediate and independent of this visual preference.
        /// </summary>
        public bool UiAnimationsEnabled
        {
            get => uiAnimationsEnabled;
            set
            {
                uiAnimationsEnabled = value;
                if (!GscMotion.IsEnabled(value))
                    GscMotion.Normalize(this);
            }
        }

        public bool IsLoadingFeedbackReady => (bool)GetValue(IsLoadingFeedbackReadyProperty);

        public event EventHandler? PreviewStateChanged;

        internal Action? BeforePublishForTest { get; set; }
        internal TimeSpan? LoadingFeedbackDelayForTest { get; set; }

        private static void OnPreviewStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = (AsyncThumbnailImage)d;
            if (!string.Equals(e.NewValue as string, "Loading", StringComparison.Ordinal))
            {
                image.StopLoadingFeedback();
                if (string.Equals(e.NewValue as string, "Ready", StringComparison.Ordinal))
                    image.AnimateReadyImage();
            }

            image.PreviewStateChanged?.Invoke(d, EventArgs.Empty);
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
            StartLoadingFeedback();
            var width = Math.Max(48, Math.Min(PreviewWidth, AsyncThumbnailLoader.MaximumDecodeDimension));
            var preserveSourceSize = PreserveSourceSize;
            var cancellation = new CancellationTokenSource();
            pending = cancellation;
            _ = LoadAsync(path!, width, preserveSourceSize, expected, cancellation.Token);
        }

        private void CancelPendingLoad(bool clearSource)
        {
            Interlocked.Increment(ref generation);
            StopLoadingFeedback();
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
                GscMotion.Normalize(this);
            }
        }

        private void StartLoadingFeedback()
        {
            StopLoadingFeedback();
            if (!IsLoaded || !IsVisible || !string.Equals(PreviewState, "Loading", StringComparison.Ordinal))
                return;

            loadingFeedbackTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
            {
                Interval = LoadingFeedbackDelayForTest ?? LoadingFeedbackDelay
            };
            loadingFeedbackTimer.Tick += OnLoadingFeedbackReady;
            loadingFeedbackTimer.Start();
        }

        private void OnLoadingFeedbackReady(object? sender, EventArgs e)
        {
            StopLoadingFeedback();
            if (IsLoaded && IsVisible && string.Equals(PreviewState, "Loading", StringComparison.Ordinal))
                SetValue(IsLoadingFeedbackReadyPropertyKey, true);
        }

        private void StopLoadingFeedback()
        {
            if (loadingFeedbackTimer != null)
            {
                loadingFeedbackTimer.Stop();
                loadingFeedbackTimer.Tick -= OnLoadingFeedbackReady;
                loadingFeedbackTimer = null;
            }

            SetValue(IsLoadingFeedbackReadyPropertyKey, false);
        }

        private void AnimateReadyImage()
        {
            if (Source == null)
                return;

            if (!GscMotion.IsEnabled(UiAnimationsEnabled))
            {
                GscMotion.Normalize(this);
                return;
            }

            GscMotion.AnimateEntrance(this, 0, 0, GscMotion.Transition, GscMotion.Transition);
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
