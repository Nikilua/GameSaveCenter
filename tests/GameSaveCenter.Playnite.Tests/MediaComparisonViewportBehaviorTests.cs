using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private static MediaItemDto Screenshot(string id, string path)
            => new MediaItemDto
            {
                MediaId = id,
                Kind = MediaKind.Screenshot,
                ArchivePath = path,
                OriginalPath = path
            };

        private static string CreateImage(string path, int width, int height, byte value)
        {
            var pixels = new byte[width * height * 4];
            for (var index = 0; index < width * height; index++)
            {
                pixels[index * 4] = value;
                pixels[index * 4 + 1] = (byte)(255 - value);
                pixels[index * 4 + 2] = 127;
                pixels[index * 4 + 3] = 255;
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(path)) encoder.Save(stream);
            return path;
        }

        private static DashboardViewModel CreateDashboard(MediaComparisonSelection selection)
        {
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
            SetAutoProperty(plugin, nameof(GameSaveCenterPlugin.Settings), new GameSaveCenterSettings());
            SetField(dashboard, "plugin", plugin);
            SetField(dashboard, "mediaComparisonSelection", selection);
            SetAutoProperty(dashboard, nameof(DashboardViewModel.Media), new BatchObservableCollection<MediaItemDto>());
            SetField(dashboard, "unassignedMedia", new BatchObservableCollection<MediaItemDto>());
            SetField(dashboard, "ignoredMedia", new BatchObservableCollection<MediaItemDto>());
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
