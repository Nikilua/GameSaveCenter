using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class ProgressRecapOrderBehaviorTests
{
    [Fact]
    public void SelectionOrderSurvivesPageAppendAndMovesByStableGameVersionIdentity()
    {
        var order = new ProgressRecapSelectionOrder();
        var first = Card("game-a", "backup-a", "起点");
        var second = Card("game-a", "backup-b", "王城外");
        var third = Card("game-a", "backup-c", "王城前");

        Assert.True(order.Add(first));
        Assert.True(order.Add(second));
        Assert.True(order.Add(third)); // A later gallery page appends without changing A, B.
        Assert.False(order.Add(Card("GAME-A", "BACKUP-C", "重复版本")));
        Assert.True(order.Move(order.Items[2], -1));
        Assert.True(order.Move(order.Items[1], -1));

        Assert.Equal(new[] { "backup-c", "backup-a", "backup-b" }, order.Items.Select(item => item.Backup.BackupId));
        Assert.Equal("第 1 项，共 3 项", order.Items[0].PositionLabel);
        Assert.Equal("第 1 项，共 3 项：王城前", order.Items[0].AutomationName);

        Assert.True(order.Add(Card("game-b", "backup-c", "另一款游戏同 ID")));
        Assert.Equal(4, order.Count);
    }

    [Fact]
    public void SelectionOrderEnforcesNineItemsAndRemovalUpdatesTheRemainingPositions()
    {
        var order = new ProgressRecapSelectionOrder();
        for (var index = 0; index < 9; index++)
            Assert.True(order.Add(Card("game", $"backup-{index}", $"进度 {index}")));
        Assert.False(order.Add(Card("game", "backup-10", "超出上限")));

        var removed = order.Items[4];
        Assert.True(order.Remove(removed.Identity));

        Assert.Equal(8, order.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, order.Items.Select(item => int.Parse(item.PositionLabel.Split(' ')[1])));
        Assert.False(order.Move(order.Items[0], -1));
        Assert.False(order.Move(order.Items[7], 1));

        order.Clear();
        Assert.Empty(order.Items);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CompactGalleryKeepsCheckpointFocusAndAccessiblePositionAfterKeyboardMove(GameSaveCenterThemeMode theme)
    {
        RunSta(() =>
        {
            var state = new GalleryState();
            var view = CreateView(state, theme, motionEnabled: false, highContrast: false);
            var window = new Window
            {
                Width = 760,
                Height = 640,
                Background = new SolidColorBrush(theme == GameSaveCenterThemeMode.Dark
                    ? Color.FromRgb(23, 24, 31)
                    : Color.FromRgb(243, 244, 248)),
                Content = view,
                ShowInTaskbar = false
            };
            try
            {
                window.Show();
                PumpLayout(window);
                CaptureProgressRecapWindowIfRequested(window, theme);

                var orderList = FindVisualChildren<ItemsControl>(view).Single(item =>
                    AutomationProperties.GetName(item) == "已选进度顺序");
                Assert.Equal(3, orderList.Items.Count);
                var movedItem = state.ProgressRecapOrderItems[2];
                var presenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(movedItem));
                var position = FindVisualChildren<TextBlock>(presenter).Single(item => item.Text == "第 3 项，共 3 项");
                Assert.Equal("第 3 项，共 3 项：王城前", AutomationProperties.GetName(position));

                var moveUp = FindVisualChildren<ButtonBase>(presenter).Single(button =>
                    AutomationProperties.GetName(button).EndsWith("，上移", StringComparison.Ordinal));
                Assert.True(moveUp.IsEnabled);
                Assert.True(moveUp.Focus());
                Assert.True(moveUp.IsKeyboardFocused);
                InvokeClick(moveUp);
                PumpLayout(window);

                Assert.Equal(new[] { "起点", "王城前", "王城外" }, state.ProgressRecapOrderItems.Select(item => item.Title));
                Assert.Equal("第 2 项，共 3 项", movedItem.PositionLabel);
                var movedPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(movedItem));
                Assert.True(IsAncestorOrSelf(movedPresenter, Keyboard.FocusedElement as DependencyObject));
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void ReorderingAnimatesOnlyTheMovedAndAdjacentItemsWhenMotionIsEnabled()
    {
        RunSta(() =>
        {
            var state = new GalleryState();
            var view = CreateView(state, GameSaveCenterThemeMode.Light, motionEnabled: true, highContrast: false);
            var window = new Window { Width = 1280, Height = 720, Content = view, ShowInTaskbar = false };
            try
            {
                window.Show();
                PumpLayout(window);
                var orderList = FindVisualChildren<ItemsControl>(view).Single(item =>
                    AutomationProperties.GetName(item) == "已选进度顺序");
                var moved = state.ProgressRecapOrderItems[2];
                var adjacent = state.ProgressRecapOrderItems[1];
                var untouched = state.ProgressRecapOrderItems[0];
                var presenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                var moveUp = FindVisualChildren<ButtonBase>(presenter).Single(button =>
                    AutomationProperties.GetName(button).EndsWith("，上移", StringComparison.Ordinal));
                Assert.True(moveUp.Focus());
                InvokeClick(moveUp);
                PumpLayout(window);

                var movedPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                var adjacentPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(adjacent));
                Assert.True(IsTranslateAnimated(movedPresenter));
                Assert.True(IsTranslateAnimated(adjacentPresenter));
                Assert.False(IsTranslateAnimated(Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(untouched))));
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void RapidMovesRestartMotionAndKeepFocusOnTheMovedProgress()
    {
        RunSta(() =>
        {
            var state = new GalleryState();
            var view = CreateView(state, GameSaveCenterThemeMode.Light, motionEnabled: true, highContrast: false);
            var window = new Window { Width = 1280, Height = 720, Content = view, ShowInTaskbar = false };
            try
            {
                window.Show();
                PumpLayout(window);
                var orderList = FindVisualChildren<ItemsControl>(view).Single(item =>
                    AutomationProperties.GetName(item) == "已选进度顺序");
                var moved = state.ProgressRecapOrderItems[2];

                var firstPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                var firstMoveUp = FindVisualChildren<ButtonBase>(firstPresenter).Single(button =>
                    AutomationProperties.GetName(button).EndsWith("，上移", StringComparison.Ordinal));
                Assert.True(firstMoveUp.Focus());
                InvokeClick(firstMoveUp);
                PumpLayout(window);

                Assert.True(IsTranslateAnimated(Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved))));
                var secondPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                var secondMoveUp = FindVisualChildren<ButtonBase>(secondPresenter).Single(button =>
                    AutomationProperties.GetName(button).EndsWith("，上移", StringComparison.Ordinal));
                InvokeClick(secondMoveUp);
                PumpLayout(window);

                Assert.Equal(new[] { "王城前", "起点", "王城外" }, state.ProgressRecapOrderItems.Select(item => item.Title));
                Assert.True(IsTranslateAnimated(Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved))));
                Assert.Equal("第 1 项，共 3 项", moved.PositionLabel);
                var finalPresenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                Assert.True(IsAncestorOrSelf(finalPresenter, Keyboard.FocusedElement as DependencyObject));
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void ReorderingSettlesImmediatelyWhenMotionIsDisabled()
    {
        RunSta(() =>
        {
            var state = new GalleryState();
            var view = CreateView(state, GameSaveCenterThemeMode.Light, motionEnabled: false, highContrast: false);
            var window = new Window { Width = 1280, Height = 720, Content = view, ShowInTaskbar = false };
            try
            {
                window.Show();
                PumpLayout(window);
                var orderList = FindVisualChildren<ItemsControl>(view).Single(item =>
                    AutomationProperties.GetName(item) == "已选进度顺序");
                var moved = state.ProgressRecapOrderItems[2];
                var presenter = Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved));
                var moveUp = FindVisualChildren<ButtonBase>(presenter).Single(button =>
                    AutomationProperties.GetName(button).EndsWith("，上移", StringComparison.Ordinal));
                Assert.True(moveUp.Focus());
                InvokeClick(moveUp);
                PumpLayout(window);

                Assert.Equal(new[] { "起点", "王城前", "王城外" }, state.ProgressRecapOrderItems.Select(item => item.Title));
                Assert.False(IsTranslateAnimated(Assert.IsAssignableFrom<FrameworkElement>(orderList.ItemContainerGenerator.ContainerFromItem(moved))));
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void MovedOrderIsRenderedFirstAndSavedPngMatchesThePreviewPixels()
    {
        RunSta(() =>
        {
            var order = new ProgressRecapSelectionOrder();
            order.Add(Card("game", "a", "起点"));
            order.Add(Card("game", "b", "王城外"));
            order.Add(Card("game", "c", "王城前"));
            var moved = order.Items[2];
            order.Move(moved, -1);
            order.Move(moved, -1);

            var photos = new Dictionary<string, BitmapSource>
            {
                ["a"] = Photo(15, 40, 220),
                ["b"] = Photo(40, 210, 35),
                ["c"] = Photo(230, 45, 20)
            };
            var preview = ProgressRecapRenderer.Render(order.Items.Select(item =>
                new ProgressRecapCard(photos[item.Backup.BackupId], "2026-11-25", item.Title)).ToArray());
            var firstImageCenter = ReadPixel(preview, 164, 118);
            Assert.Equal(new byte[] { 20, 45, 230, 255 }, firstImageCenter);

            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gsc-recap-order-tests", Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                var destination = System.IO.Path.Combine(root, "ordered.png");
                ProgressRecapRenderer.SavePng(preview, destination);
                using var stream = System.IO.File.OpenRead(destination);
                var saved = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
                Assert.Equal(ReadPixels(preview), ReadPixels(saved));
                Assert.Equal(firstImageCenter, ReadPixel(saved, 164, 118));
            }
            finally { System.IO.Directory.Delete(root, true); }
        });
    }

    private static BackupGalleryCardViewModel Card(string gameId, string backupId, string title)
        => new BackupGalleryCardViewModel(new BackupCoverGalleryEntryDto
        {
            Backup = new BackupVersionDto { PlayniteId = gameId, BackupId = backupId, Comment = title },
            Cover = new BackupCoverDto()
        }, () => true);

    private static BitmapSource Photo(byte r, byte g, byte b)
    {
        var photo = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new[] { b, g, r, (byte)255 }, 4);
        photo.Freeze();
        return photo;
    }

    private static byte[] ReadPixel(BitmapSource source, int x, int y)
    {
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
        return pixel;
    }

    private static byte[] ReadPixels(BitmapSource source)
    {
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var stride = bitmap.PixelWidth * 4;
        var pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);
        return pixels;
    }

    private static bool IsTranslateAnimated(FrameworkElement element)
        => FindTranslateTransform(element.RenderTransform) is TranslateTransform translate
            && DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated;

    private static TranslateTransform? FindTranslateTransform(Transform transform)
    {
        if (transform is TranslateTransform translate) return translate;
        if (transform is TransformGroup group)
            return group.Children.OfType<Transform>().Select(FindTranslateTransform).FirstOrDefault(item => item != null);
        return null;
    }

    private static SaveCenterView CreateView(
        GalleryState state,
        GameSaveCenterThemeMode theme,
        bool motionEnabled,
        bool highContrast = true)
    {
        var view = new SaveCenterView { DataContext = state };
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            view,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: theme,
            highContrastOverride: highContrast);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: true, motionEnabled: motionEnabled);
        AdaptiveThemePaletteFactory.SetHighContrastActive(view, highContrast);
        AdaptiveThemePaletteFactory.SetMotionEnabledActive(view, motionEnabled && !highContrast);
        view.SystemMotionEnabledProviderForBehaviorTest = () => motionEnabled;
        return view;
    }

    private sealed class GalleryState : INotifyPropertyChanged, IProgressRecapOrderMoveSource
    {
        private readonly ProgressRecapSelectionOrder order = new ProgressRecapSelectionOrder();

        public GalleryState()
        {
            foreach (var (id, title) in new[] { ("a", "起点"), ("b", "王城外"), ("c", "王城前") })
            {
                var card = Card("game", id, title);
                card.PropertyChanged += (_, change) =>
                {
                    if (change.PropertyName != nameof(BackupGalleryCardViewModel.IsSelected)) return;
                    if (card.IsSelected) order.Add(card);
                    else order.Remove(ProgressRecapIdentity.From(card.Backup));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasProgressRecapItems)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressRecapSelectionLabel)));
                };
                BackupGalleryItems.Add(card);
                card.IsSelected = true;
            }

            MoveProgressRecapItemUpCommand = new RelayCommand(item => Move((ProgressRecapOrderItemViewModel)item!, -1),
                item => item is ProgressRecapOrderItemViewModel entry && order.CanMove(entry, -1));
            MoveProgressRecapItemDownCommand = new RelayCommand(item => Move((ProgressRecapOrderItemViewModel)item!, 1),
                item => item is ProgressRecapOrderItemViewModel entry && order.CanMove(entry, 1));
            RemoveProgressRecapItemCommand = new RelayCommand(item => order.Remove(((ProgressRecapOrderItemViewModel)item!).Identity));
            OpenBackupGalleryVersionCommand = new RelayCommand(_ => { });
            OpenHistoryFromBackupGalleryCommand = new RelayCommand(_ => { });
            RefreshBackupGalleryCommand = new RelayCommand(_ => { });
            LoadMoreBackupGalleryCommand = new RelayCommand(_ => { });
            PreviewProgressRecapCommand = new RelayCommand(_ => { });
            SaveProgressRecapCommand = new RelayCommand(_ => { });
        }

        public int SaveTabIndex { get; set; } = 4;
        public ObservableCollection<BackupGalleryCardViewModel> BackupGalleryItems { get; } = new ObservableCollection<BackupGalleryCardViewModel>();
        public ObservableCollection<ProgressRecapOrderItemViewModel> ProgressRecapOrderItems => order.Items;
        public bool HasProgressRecapItems => order.Count > 0;
        public string BackupGalleryStatus => "已载入 3/3 个进度";
        public string ProgressRecapSelectionLabel => $"已选 {order.Count}/9 个进度";
        public bool ProgressRecapKeepMissing { get; set; } = true;
        public BitmapSource? ProgressRecapPreview => null;
        public string ProgressRecapStatus => string.Empty;
        public ICommand OpenBackupGalleryVersionCommand { get; }
        public ICommand OpenHistoryFromBackupGalleryCommand { get; }
        public ICommand RefreshBackupGalleryCommand { get; }
        public ICommand LoadMoreBackupGalleryCommand { get; }
        public ICommand PreviewProgressRecapCommand { get; }
        public ICommand SaveProgressRecapCommand { get; }
        public ICommand MoveProgressRecapItemUpCommand { get; }
        public ICommand MoveProgressRecapItemDownCommand { get; }
        public ICommand RemoveProgressRecapItemCommand { get; }
        public event EventHandler<ProgressRecapOrderMoveStartingEventArgs>? ProgressRecapOrderMoveStarting;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Move(ProgressRecapOrderItemViewModel item, int direction)
        {
            var index = order.Items.IndexOf(item);
            if (index < 0 || !order.CanMove(item, direction)) return;
            ProgressRecapOrderMoveStarting?.Invoke(this,
                new ProgressRecapOrderMoveStartingEventArgs(item, order.Items[index + direction], direction));
            order.Move(item, direction);
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T typed) yield return typed;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private static bool IsAncestorOrSelf(DependencyObject? ancestor, DependencyObject? descendant)
    {
        while (ancestor != null && descendant != null)
        {
            if (ReferenceEquals(ancestor, descendant)) return true;
            descendant = descendant is Visual || descendant is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(descendant)
                : LogicalTreeHelper.GetParent(descendant);
        }
        return false;
    }

    private static void InvokeClick(ButtonBase button)
    {
        var onClick = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(onClick);
        onClick!.Invoke(button, null);
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void CaptureProgressRecapWindowIfRequested(Window window, GameSaveCenterThemeMode theme)
    {
        var captureRoot = Environment.GetEnvironmentVariable("GSC_PROGRESS_RECAP_CAPTURE_DIR");
        if (string.IsNullOrWhiteSpace(captureRoot)) return;
        Directory.CreateDirectory(captureRoot);
        var width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(captureRoot, $"progress-recap-{theme}-{width}x{height}.png"));
        encoder.Save(stream);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
