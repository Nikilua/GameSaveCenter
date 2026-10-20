using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Converters;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    [Collection("R09FocusWpf")]
    public sealed class MediaComparisonViewportBehaviorTests
    {
        [Fact]
        public void WipeComparisonReusesDecodedImagesShowsTransparencyGridAndMovesByKeyboard()
        {
            RunSta(() =>
            {
                var root = Path.Combine(Path.GetTempPath(), "gsc-s04-04-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var firstPath = CreateImage(Path.Combine(root, "first-transparent.png"), 960, 540, 75, transparent: true);
                var secondPath = CreateImage(Path.Combine(root, "second-transparent.png"), 960, 540, 155, transparent: true);
                try
                {
                    var selection = new MediaComparisonSelection();
                    selection.Add(new[]
                    {
                        Screenshot("media-first", firstPath),
                        Screenshot("media-second", secondPath)
                    }, _ => true);
                    var dashboard = CreateDashboard(selection);
                    var view = new MediaCenterView { DataContext = dashboard };
                    var viewType = typeof(MediaCenterView);
                    var tabControl = GetField<TabControl>(viewType, view, "MediaTabControl");
                    var wipeToggle = GetField<ToggleSwitch>(viewType, view, "MediaComparisonWipeToggle");
                    var wipeCard = GetField<Border>(viewType, view, "MediaComparisonWipeCard");
                    var wipeCanvas = GetField<Grid>(viewType, view, "MediaComparisonWipeCanvas");
                    var wipeReveal = GetField<Border>(viewType, view, "MediaComparisonWipeReveal");
                    var wipeHandle = GetField<Border>(viewType, view, "MediaComparisonWipeHandle");
                    var firstImage = GetField<AsyncThumbnailImage>(viewType, view, "MediaComparisonFirstImage");
                    var secondImage = GetField<AsyncThumbnailImage>(viewType, view, "MediaComparisonSecondImage");
                    var wipeFirstImage = GetField<System.Windows.Controls.Image>(viewType, view, "MediaComparisonWipeFirstImage");
                    var wipeSecondImage = GetField<System.Windows.Controls.Image>(viewType, view, "MediaComparisonWipeSecondImage");

                    using (var host = new WindowHost(new Window
                    {
                        Content = view,
                        Width = 1180,
                        Height = 900,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    }))
                    {
                        tabControl.SelectedIndex = 1;
                        dashboard.SetMediaComparisonWorkspaceOpen(true);
                        host.UpdateLayout();
                        PumpUntil(host.Window.Dispatcher,
                            () => firstImage.PreviewState == "Ready" && secondImage.PreviewState == "Ready",
                            TimeSpan.FromSeconds(6));
                        host.UpdateLayout();

                        wipeToggle.IsChecked = true;
                        wipeToggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, wipeToggle));
                        host.UpdateLayout();

                        Assert.Equal(Visibility.Visible, wipeCard.Visibility);
                        Assert.Same(firstImage.Source, wipeFirstImage.Source);
                        Assert.Same(secondImage.Source, wipeSecondImage.Source);
                        Assert.Equal(wipeCanvas.ActualWidth * 0.5d, wipeReveal.Width, 2);
                        Assert.Equal("Ready", firstImage.PreviewState);
                        Assert.Equal("Ready", secondImage.PreviewState);

                        var comparisonRender = new RenderTargetBitmap(
                            (int)Math.Ceiling(wipeCanvas.ActualWidth),
                            (int)Math.Ceiling(wipeCanvas.ActualHeight),
                            96,
                            96,
                            PixelFormats.Pbgra32);
                        comparisonRender.Render(wipeCanvas);
                        var comparisonPixels = new byte[comparisonRender.PixelWidth * comparisonRender.PixelHeight * 4];
                        comparisonRender.CopyPixels(comparisonPixels, comparisonRender.PixelWidth * 4, 0);
                        var comparisonCenterY = comparisonRender.PixelHeight / 2;
                        var aPixel = (comparisonCenterY * comparisonRender.PixelWidth + (int)(comparisonRender.PixelWidth * 0.42d)) * 4;
                        var bPixel = (comparisonCenterY * comparisonRender.PixelWidth + (int)(comparisonRender.PixelWidth * 0.58d)) * 4;
                        Assert.True(comparisonPixels[aPixel + 1] > comparisonPixels[aPixel + 2], "The left half should show A's green synthetic image.");
                        Assert.True(comparisonPixels[bPixel + 2] > comparisonPixels[bPixel + 1], "The right half should show B's purple synthetic image.");

                        var initialWidth = wipeReveal.Width;
                        RaisePreviewKeyDown(wipeHandle, Key.Right);
                        host.UpdateLayout();
                        Assert.Equal(initialWidth + wipeCanvas.ActualWidth * 0.02d, wipeReveal.Width, 2);

                        RaisePreviewKeyDown(wipeHandle, Key.Home);
                        host.UpdateLayout();
                        Assert.Equal(0d, wipeReveal.Width);
                        RaisePreviewKeyDown(wipeHandle, Key.End);
                        host.UpdateLayout();
                        Assert.Equal(wipeCanvas.ActualWidth, wipeReveal.Width, 2);

                        var transparentPixelRender = new RenderTargetBitmap(
                            (int)Math.Ceiling(wipeCanvas.ActualWidth),
                            (int)Math.Ceiling(wipeCanvas.ActualHeight),
                            96,
                            96,
                            PixelFormats.Pbgra32);
                        transparentPixelRender.Render(wipeCanvas);
                        var pixels = new byte[transparentPixelRender.PixelWidth * transparentPixelRender.PixelHeight * 4];
                        transparentPixelRender.CopyPixels(pixels, transparentPixelRender.PixelWidth * 4, 0);
                        var firstCheckerPixel = pixels[(4 * transparentPixelRender.PixelWidth + 4) * 4];
                        var secondCheckerPixel = pixels[(4 * transparentPixelRender.PixelWidth + 12) * 4];
                        Assert.NotEqual(firstCheckerPixel, secondCheckerPixel);

                        var visualCapturePath = Environment.GetEnvironmentVariable("GSC_S04_04_WIPE_CAPTURE");
                        if (!string.IsNullOrWhiteSpace(visualCapturePath))
                        {
                            RaisePreviewKeyDown(wipeHandle, Key.Home);
                            for (var index = 0; index < 25; index++)
                                RaisePreviewKeyDown(wipeHandle, Key.Right);
                            host.UpdateLayout();
                            SaveVisualCapture(wipeCard, visualCapturePath!);
                        }

                        wipeToggle.IsChecked = false;
                        wipeToggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, wipeToggle));
                        host.UpdateLayout();
                        Assert.Equal(Visibility.Collapsed, wipeCard.Visibility);
                        Assert.True(GetField<Border>(viewType, view, "MediaComparisonFirstCard").IsEnabled);
                        Assert.True(GetField<Border>(viewType, view, "MediaComparisonSecondCard").IsEnabled);
                    }
                }
                finally
                {
                    AsyncThumbnailLoader.ClearCache();
                    if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
                }
            });
        }

        [Fact]
        public void ComparisonToolbarFitsUsesOneToOneZoomsAndSynchronizesNormalizedPan()
        {
            RunSta(() =>
            {
                var root = Path.Combine(Path.GetTempPath(), "gsc-s04-03-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var firstPath = CreateImage(Path.Combine(root, "first.png"), 960, 540, 75);
                var secondPath = CreateImage(Path.Combine(root, "second.png"), 720, 1200, 155);
                try
                {
                    var selection = new MediaComparisonSelection();
                    selection.Add(new[]
                    {
                        Screenshot("media-first", firstPath),
                        Screenshot("media-second", secondPath)
                    }, _ => true);
                    var dashboard = CreateDashboard(selection);
                    var view = new MediaCenterView { DataContext = dashboard };
                    var viewType = typeof(MediaCenterView);
                    var tabControl = GetField<TabControl>(viewType, view, "MediaTabControl");
                    var fitButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonFitButton");
                    var oneToOneButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonOneToOneButton");
                    var zoomInButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonFirstZoomInButton");
                    var firstImage = GetField<AsyncThumbnailImage>(viewType, view, "MediaComparisonFirstImage");
                    var secondImage = GetField<AsyncThumbnailImage>(viewType, view, "MediaComparisonSecondImage");
                    var firstViewer = GetField<ScrollViewer>(viewType, view, "MediaComparisonFirstScroller");
                    var secondViewer = GetField<ScrollViewer>(viewType, view, "MediaComparisonSecondScroller");
                    var state = GetField<MediaComparisonViewportState>(viewType, view, "mediaComparisonViewportState");

                    using (var host = new WindowHost(new Window
                    {
                        Content = view,
                        Width = 1180,
                        Height = 900,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    }))
                    {
                        tabControl.SelectedIndex = 1;
                        dashboard.SetMediaComparisonWorkspaceOpen(true);
                        host.UpdateLayout();
                        PumpUntil(host.Window.Dispatcher,
                            () => firstImage.PreviewState == "Ready" && secondImage.PreviewState == "Ready",
                            TimeSpan.FromSeconds(6));
                        host.UpdateLayout();

                        Assert.Equal(2048, firstImage.PreviewWidth);
                        Assert.True(firstImage.PreserveSourceSize);
                        Assert.True(state.IsSynchronized);

                        Execute(fitButton);
                        host.UpdateLayout();
                        Assert.True(state.IsFit(MediaComparisonPane.First));
                        Assert.True(firstImage.Width <= firstViewer.ViewportWidth + 0.5d);
                        Assert.True(firstImage.Height <= firstViewer.ViewportHeight + 0.5d);

                        Execute(oneToOneButton);
                        host.UpdateLayout();
                        Assert.Equal(960d, firstImage.Width);
                        Assert.Equal(540d, firstImage.Height);
                        Assert.Equal(720d, secondImage.Width);
                        Assert.Equal(1200d, secondImage.Height);

                        Execute(zoomInButton);
                        Assert.Equal(state.GetZoom(MediaComparisonPane.First, 0.5d), state.GetZoom(MediaComparisonPane.Second, 0.4d));
                        host.UpdateLayout();
                        firstViewer.ScrollToHorizontalOffset(180d);
                        firstViewer.ScrollToVerticalOffset(210d);
                        host.UpdateLayout();
                        PumpFor(host.Window.Dispatcher, TimeSpan.FromMilliseconds(120));

                        Assert.Equal(state.GetCenterX(MediaComparisonPane.First), state.GetCenterX(MediaComparisonPane.Second), 3);
                        Assert.Equal(state.GetCenterY(MediaComparisonPane.First), state.GetCenterY(MediaComparisonPane.Second), 3);
                        Assert.InRange(state.GetCenterX(MediaComparisonPane.Second), 0d, 1d);
                        Assert.InRange(state.GetCenterY(MediaComparisonPane.Second), 0d, 1d);
                    }
                }
                finally
                {
                    AsyncThumbnailLoader.ClearCache();
                    if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
                }
            });
        }

        [Fact]
        public void CandidateButtonsKeepReferenceAndDetailsSkipFilteredAndBrokenImages()
        {
            RunSta(() =>
            {
                var root = Path.Combine(Path.GetTempPath(), "gsc-s04-05-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var reference = Screenshot("reference", CreateImage(Path.Combine(root, "reference.png"), 320, 180, 45));
                var current = Screenshot("current", CreateImage(Path.Combine(root, "current.png"), 320, 180, 85));
                var filtered = Screenshot("filtered-out", CreateImage(Path.Combine(root, "filtered.png"), 320, 180, 105));
                var next = Screenshot("next", CreateImage(Path.Combine(root, "next.png"), 320, 180, 125));
                var broken = Screenshot("broken", Path.Combine(root, "broken.png"));
                File.WriteAllBytes(broken.ArchivePath, Array.Empty<byte>());
                var last = Screenshot("last", CreateImage(Path.Combine(root, "last.png"), 320, 180, 175));
                var detailSelection = Screenshot("detail-selection", reference.ArchivePath);

                try
                {
                    var selection = new MediaComparisonSelection();
                    selection.Add(new[] { reference, current }, _ => true);
                    var viewModel = CreateDashboard(selection, new[]
                    {
                        reference,
                        current,
                        new MediaItemDto { MediaId = "video", Kind = MediaKind.VideoClip, ArchivePath = Path.Combine(root, "clip.mp4") },
                        filtered,
                        next,
                        broken,
                        last
                    }, item => !string.Equals(item.MediaId, "filtered-out", StringComparison.Ordinal));
                    SetField(viewModel, "selectedMedia", detailSelection);
                    var view = new MediaCenterView { DataContext = viewModel };
                    var viewType = typeof(MediaCenterView);
                    var tabControl = GetField<TabControl>(viewType, view, "MediaTabControl");
                    var previousButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonPreviousCandidateButton");
                    var nextButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonNextCandidateButton");
                    var rightImage = GetField<AsyncThumbnailImage>(viewType, view, "MediaComparisonSecondImage");
                    var candidatePosition = GetField<TextBlock>(viewType, view, "MediaComparisonCandidateNavigationText");

                    using (var host = new WindowHost(new Window
                    {
                        Content = view,
                        Width = 1180,
                        Height = 900,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    }))
                    {
                        tabControl.SelectedIndex = 1;
                        viewModel.SetMediaComparisonWorkspaceOpen(true);
                        host.UpdateLayout();

                        Assert.Same(viewModel.NextMediaComparisonCandidateCommand, nextButton.Command);
                        Assert.Same(viewModel.PreviousMediaComparisonCandidateCommand, previousButton.Command);
                        Assert.True(nextButton.Command!.CanExecute(null));
                        nextButton.Focus();
                        Keyboard.Focus(nextButton);
                        nextButton.Command.Execute(null);
                        host.UpdateLayout();
                        Assert.Equal("next", selection.Items[1].MediaId);
                        Assert.Equal("reference", selection.Items[0].MediaId);
                        Assert.Same(detailSelection, viewModel.SelectedMedia);
                        Assert.Same(nextButton, Keyboard.FocusedElement);
                        Assert.Contains("2 / 4", candidatePosition.Text);

                        nextButton.Command.Execute(null);
                        PumpUntil(host.Window.Dispatcher,
                            () => selection.Items[1].MediaId == "last" && rightImage.PreviewState == "Ready",
                            TimeSpan.FromSeconds(6));
                        host.UpdateLayout();
                        Assert.Equal("Ready", rightImage.PreviewState);
                        Assert.Equal("reference", selection.Items[0].MediaId);
                        Assert.Same(detailSelection, viewModel.SelectedMedia);
                        Assert.Contains("载入失败", viewModel.MediaComparisonStatus);
                        Assert.Contains("4 / 4", candidatePosition.Text);

                        previousButton.Focus();
                        Keyboard.Focus(previousButton);
                        previousButton.Command!.Execute(null);
                        host.UpdateLayout();
                        Assert.Equal("next", selection.Items[1].MediaId);
                        Assert.Equal("reference", selection.Items[0].MediaId);
                        Assert.Same(detailSelection, viewModel.SelectedMedia);
                        Assert.Same(previousButton, Keyboard.FocusedElement);

                        nextButton.Focus();
                        Keyboard.Focus(nextButton);
                        nextButton.Command.Execute(null);
                        host.UpdateLayout();
                        Assert.Equal("last", selection.Items[1].MediaId);
                        Assert.Contains("已跳过 1 张缺失或失效图片", viewModel.MediaComparisonStatus);
                        var capturePath = Environment.GetEnvironmentVariable("GSC_S04_05_CAPTURE_PATH");
                        if (!string.IsNullOrWhiteSpace(capturePath))
                            SaveVisualCapture(view, capturePath!);
                    }
                }
                finally
                {
                    if (Directory.Exists(root))
                        Directory.Delete(root, recursive: true);
                }
            });
        }

        [Fact]
        public void ComparisonOpinionButtonsTrackCandidatesUndoAndClearOnlySessionMarks()
        {
            RunSta(() =>
            {
                var root = Path.Combine(Path.GetTempPath(), "gsc-s04-06-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var reference = Screenshot("reference", CreateImage(Path.Combine(root, "reference.png"), 320, 180, 45));
                var current = Screenshot("current", CreateImage(Path.Combine(root, "current.png"), 320, 180, 85));
                var next = Screenshot("next", CreateImage(Path.Combine(root, "next.png"), 320, 180, 125));

                try
                {
                    var selection = new MediaComparisonSelection();
                    selection.Add(new[] { reference, current }, _ => true);
                    var viewModel = CreateDashboard(selection, new[] { reference, current, next });
                    var view = new MediaCenterView { DataContext = viewModel };
                    var viewType = typeof(MediaCenterView);
                    var tabControl = GetField<TabControl>(viewType, view, "MediaTabControl");
                    var nextCandidateButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonNextCandidateButton");
                    var previousCandidateButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonPreviousCandidateButton");
                    var keepButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonSecondKeepOpinionButton");
                    var reviewButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonSecondReviewOpinionButton");
                    var undoButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonSecondClearOpinionButton");
                    var clearComparisonButton = GetField<GameSaveCenter.Playnite.Controls.Button>(viewType, view, "MediaComparisonClearButton");
                    var secondOpinionText = GetField<TextBlock>(viewType, view, "MediaComparisonSecondOpinionText");
                    var opinionSummaryText = GetField<TextBlock>(viewType, view, "MediaComparisonOpinionSummaryText");

                    using (var host = new WindowHost(new Window
                    {
                        Content = view,
                        Width = 1180,
                        Height = 900,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    }))
                    {
                        tabControl.SelectedIndex = 1;
                        viewModel.SetMediaComparisonWorkspaceOpen(true);
                        host.UpdateLayout();

                        Assert.Same(viewModel.MarkMediaComparisonToKeepCommand, keepButton.Command);
                        Assert.Same(viewModel.MarkMediaComparisonToReviewCommand, reviewButton.Command);
                        Assert.Same(viewModel.ClearMediaComparisonOpinionCommand, undoButton.Command);
                        Assert.Contains("0 项", opinionSummaryText.Text);

                        ExecuteBoundCommand(keepButton);
                        host.UpdateLayout();
                        Assert.Equal(MediaComparisonOpinionKind.Keep,
                            GetField<MediaComparisonOpinionTracker>(typeof(DashboardViewModel), viewModel, "mediaComparisonOpinionTracker").Get("current"));
                        Assert.Equal("人工意见：待保留", secondOpinionText.Text);
                        Assert.Contains("待保留 1 项", opinionSummaryText.Text);

                        nextCandidateButton.Command!.Execute(null);
                        host.UpdateLayout();
                        Assert.Equal("next", selection.Items[1].MediaId);
                        Assert.Equal("未标记", secondOpinionText.Text);
                        previousCandidateButton.Command!.Execute(null);
                        host.UpdateLayout();
                        Assert.Equal("current", selection.Items[1].MediaId);
                        Assert.Equal("人工意见：待保留", secondOpinionText.Text);

                        ExecuteBoundCommand(reviewButton);
                        host.UpdateLayout();
                        Assert.Equal("人工意见：待检查", secondOpinionText.Text);
                        Assert.Contains("待保留 0 项 · 待检查 1 项", opinionSummaryText.Text);
                        ExecuteBoundCommand(undoButton);
                        host.UpdateLayout();
                        Assert.Equal("未标记", secondOpinionText.Text);
                        Assert.Contains("待保留 0 项 · 待检查 0 项", opinionSummaryText.Text);

                        ExecuteBoundCommand(reviewButton);
                        host.UpdateLayout();
                        Assert.Equal("人工意见：待检查", secondOpinionText.Text);
                        Assert.Contains("待保留 0 项 · 待检查 1 项", opinionSummaryText.Text);
                        var capturePath = Environment.GetEnvironmentVariable("GSC_S04_06_CAPTURE_PATH");
                        if (!string.IsNullOrWhiteSpace(capturePath))
                            SaveVisualCapture(view, capturePath!);
                        ExecuteBoundCommand(clearComparisonButton);
                        host.UpdateLayout();
                        Assert.Empty(selection.Items);
                        Assert.Contains("待保留 0 项 · 待检查 0 项", opinionSummaryText.Text);
                        Assert.True(File.Exists(reference.ArchivePath));
                        Assert.True(File.Exists(current.ArchivePath));

                    }
                }
                finally
                {
                    AsyncThumbnailLoader.ClearCache();
                    if (Directory.Exists(root))
                        Directory.Delete(root, recursive: true);
                }
            });
        }

        private static MediaItemDto Screenshot(string id, string path)
            => new MediaItemDto
            {
                MediaId = id,
                Kind = MediaKind.Screenshot,
                ArchivePath = path,
                OriginalPath = path
            };

        private static string CreateImage(string path, int width, int height, byte value, bool transparent = false)
        {
            var pixels = new byte[width * height * 4];
            for (var index = 0; index < width * height; index++)
            {
                pixels[index * 4] = value;
                pixels[index * 4 + 1] = (byte)(255 - value);
                pixels[index * 4 + 2] = 127;
                pixels[index * 4 + 3] = transparent && index % width < width / 5 ? (byte)0 : (byte)255;
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(path)) encoder.Save(stream);
            return path;
        }

        private static void SaveVisualCapture(FrameworkElement element, string path)
        {
            var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
            var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
            var render = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            render.Render(element);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(render));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using (var stream = File.Create(path)) encoder.Save(stream);
        }

        private static DashboardViewModel CreateDashboard(
            MediaComparisonSelection selection,
            IEnumerable<MediaItemDto>? media = null,
            Func<MediaItemDto, bool>? filter = null)
        {
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
            SetAutoProperty(plugin, nameof(GameSaveCenterPlugin.Settings), new GameSaveCenterSettings());
            SetField(dashboard, "plugin", plugin);
            SetField(dashboard, "mediaComparisonSelection", selection);
            SetField(dashboard, "mediaComparisonOpinionTracker", new MediaComparisonOpinionTracker());
            SetField(dashboard, "mediaComparisonCandidateNavigator", new MediaComparisonCandidateNavigator());
            var mediaCollection = new BatchObservableCollection<MediaItemDto>();
            foreach (var item in media ?? Array.Empty<MediaItemDto>()) mediaCollection.Add(item);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.Media), mediaCollection);
            var mediaView = new ListCollectionView(mediaCollection);
            if (filter != null) mediaView.Filter = value => value is MediaItemDto item && filter(item);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.MediaView), mediaView);
            SetField(dashboard, "unassignedMedia", new BatchObservableCollection<MediaItemDto>());
            SetField(dashboard, "ignoredMedia", new BatchObservableCollection<MediaItemDto>());
            var navigateMethod = typeof(DashboardViewModel).GetMethod("NavigateMediaComparisonCandidate", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(navigateMethod);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.PreviousMediaComparisonCandidateCommand), new RelayCommand(
                _ => navigateMethod!.Invoke(dashboard, new object[] { -1 }), _ => dashboard.CanNavigatePreviousMediaComparisonCandidate));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.NextMediaComparisonCandidateCommand), new RelayCommand(
                _ => navigateMethod!.Invoke(dashboard, new object[] { 1 }), _ => dashboard.CanNavigateNextMediaComparisonCandidate));
            var setOpinionMethod = typeof(DashboardViewModel).GetMethod("SetMediaComparisonOpinion", BindingFlags.Instance | BindingFlags.NonPublic);
            var canSetOpinionMethod = typeof(DashboardViewModel).GetMethod("CanSetMediaComparisonOpinion", BindingFlags.Instance | BindingFlags.NonPublic);
            var clearOpinionMethod = typeof(DashboardViewModel).GetMethod("ClearMediaComparisonOpinion", BindingFlags.Instance | BindingFlags.NonPublic);
            var canClearOpinionMethod = typeof(DashboardViewModel).GetMethod("CanClearMediaComparisonOpinion", BindingFlags.Instance | BindingFlags.NonPublic);
            var clearComparisonMethod = typeof(DashboardViewModel).GetMethod("ClearMediaComparison", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(setOpinionMethod);
            Assert.NotNull(canSetOpinionMethod);
            Assert.NotNull(clearOpinionMethod);
            Assert.NotNull(canClearOpinionMethod);
            Assert.NotNull(clearComparisonMethod);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.MarkMediaComparisonToKeepCommand), new RelayCommand(
                value => setOpinionMethod!.Invoke(dashboard, new object[] { value!, MediaComparisonOpinionKind.Keep }),
                value => (bool)canSetOpinionMethod!.Invoke(dashboard, new[] { value })!));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.MarkMediaComparisonToReviewCommand), new RelayCommand(
                value => setOpinionMethod!.Invoke(dashboard, new object[] { value!, MediaComparisonOpinionKind.Review }),
                value => (bool)canSetOpinionMethod!.Invoke(dashboard, new[] { value })!));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.ClearMediaComparisonOpinionCommand), new RelayCommand(
                value => clearOpinionMethod!.Invoke(dashboard, new[] { value }),
                value => (bool)canClearOpinionMethod!.Invoke(dashboard, new[] { value })!));
            SetAutoProperty(dashboard, nameof(DashboardViewModel.ClearMediaComparisonCommand), new RelayCommand(
                _ => clearComparisonMethod!.Invoke(dashboard, null), _ => dashboard.MediaComparisonItems.Count > 0));
            return dashboard;
        }

        private static T GetField<T>(Type type, object target, string name)
            => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

        private static void SetField(object target, string name, object value)
            => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

        private static void SetAutoProperty(object target, string propertyName, object value)
            => SetField(target, $"<{propertyName}>k__BackingField", value);

        private static void Execute(System.Windows.Controls.Button button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        private static void ExecuteBoundCommand(GameSaveCenter.Playnite.Controls.Button button)
        {
            Assert.NotNull(button.Command);
            Assert.True(button.Command!.CanExecute(button.CommandParameter));
            button.Command.Execute(button.CommandParameter);
        }

        private static KeyEventArgs RaisePreviewKeyDown(UIElement element, Key key)
        {
            var presentationSource = PresentationSource.FromVisual(element);
            Assert.NotNull(presentationSource);
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, presentationSource!, Environment.TickCount, key)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
                Source = element
            };
            element.RaiseEvent(args);
            return args;
        }

        private static void PumpUntil(Dispatcher dispatcher, Func<bool> condition, TimeSpan timeout)
        {
            var frame = new DispatcherFrame();
            var started = DateTime.UtcNow;
            var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            timer.Tick += (_, _) =>
            {
                if (condition() || DateTime.UtcNow - started >= timeout)
                {
                    timer.Stop();
                    frame.Continue = false;
                }
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
            Assert.True(condition(), "Timed out waiting for comparison images to load.");
        }

        private static void PumpFor(Dispatcher dispatcher, TimeSpan duration)
        {
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            var started = DateTime.UtcNow;
            timer.Tick += (_, _) =>
            {
                if (DateTime.UtcNow - started >= duration)
                {
                    timer.Stop();
                    frame.Continue = false;
                }
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception exception) { failure = exception; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
        }

        private sealed class WindowHost : IDisposable
        {
            public WindowHost(Window window)
            {
                Window = window;
                Window.Show();
                Window.UpdateLayout();
            }

            public Window Window { get; }

            public void UpdateLayout()
            {
                Window.UpdateLayout();
                Window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                Window.UpdateLayout();
            }

            public void Dispose()
            {
                if (Window.IsVisible) Window.Close();
                Window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            }
        }
    }
}
