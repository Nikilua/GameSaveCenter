using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Fixed-slot media preview that keeps the thumbnail surface and its semantic fallback
    /// inside the same visual rectangle. Video entries intentionally remain a labelled preview
    /// placeholder here; the detail pane owns the existing MediaElement player.
    /// </summary>
    public sealed class MediaThumbnailPreview : Grid
    {
        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
            nameof(SourcePath), typeof(string), typeof(MediaThumbnailPreview),
            new PropertyMetadata(null, OnSourcePathChanged));
        public static readonly DependencyProperty PreviewWidthProperty = DependencyProperty.Register(
            nameof(PreviewWidth), typeof(int), typeof(MediaThumbnailPreview),
            new PropertyMetadata(96, OnPreviewWidthChanged));
        public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
            nameof(Kind), typeof(MediaKind), typeof(MediaThumbnailPreview),
            new PropertyMetadata(MediaKind.Unknown, OnKindChanged));
        public static readonly DependencyProperty PreviewStateProperty = DependencyProperty.Register(
            nameof(PreviewState), typeof(string), typeof(MediaThumbnailPreview),
            new PropertyMetadata("NoImage"));
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText), typeof(string), typeof(MediaThumbnailPreview),
            new PropertyMetadata("暂无预览"));

        private readonly AsyncThumbnailImage image;
        private readonly Border placeholder;
        private readonly TextBlock placeholderText;

        public MediaThumbnailPreview()
        {
            ClipToBounds = true;
            image = new AsyncThumbnailImage
            {
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            image.PreviewStateChanged += OnImagePreviewStateChanged;
            image.SetBinding(AsyncThumbnailImage.SourcePathProperty, new System.Windows.Data.Binding(nameof(SourcePath)) { Source = this });
            image.SetBinding(AsyncThumbnailImage.PreviewWidthProperty, new System.Windows.Data.Binding(nameof(PreviewWidth)) { Source = this });

            placeholderText = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12)
            };
            placeholderText.SetResourceReference(TextBlock.ForegroundProperty, "GscSecondaryTextBrush");
            placeholder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = placeholderText
            };
            placeholder.SetResourceReference(Border.BackgroundProperty, "GscControlFillBrush");

            Children.Add(image);
            Children.Add(placeholder);
            UpdateVisualState();
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

        public MediaKind Kind
        {
            get => (MediaKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public string PreviewState
        {
            get => (string)GetValue(PreviewStateProperty);
            private set => SetValue(PreviewStateProperty, value);
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            private set => SetValue(PlaceholderTextProperty, value);
        }

        internal bool IsPlaceholderVisible => placeholder.Visibility == Visibility.Visible;

        private static void OnSourcePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MediaThumbnailPreview)d).UpdateVisualState();

        private static void OnPreviewWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MediaThumbnailPreview)d).UpdateVisualState();

        private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MediaThumbnailPreview)d).UpdateVisualState();

        private void OnImagePreviewStateChanged(object? sender, System.EventArgs e)
            => UpdateVisualState();

        private void UpdateVisualState()
        {
            if (placeholder == null || placeholderText == null)
                return;

            if (Kind == MediaKind.VideoClip)
            {
                PreviewState = "Video";
                PlaceholderText = "录像预览";
                image.Visibility = Visibility.Collapsed;
                placeholder.Visibility = Visibility.Visible;
                return;
            }

            if (Kind != MediaKind.Screenshot)
            {
                PreviewState = "Unknown";
                PlaceholderText = "未知媒体类型";
                image.Visibility = Visibility.Collapsed;
                placeholder.Visibility = Visibility.Visible;
                return;
            }

            PreviewState = image.PreviewState;
            PlaceholderText = image.PreviewState switch
            {
                "Loading" => "正在载入缩略图…",
                "Missing" => "媒体文件不存在",
                "Failed" => "缩略图损坏或无法读取",
                "Ready" => string.Empty,
                _ => "暂无截图"
            };
            image.Visibility = Visibility.Visible;
            placeholder.Visibility = image.PreviewState == "Ready" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
