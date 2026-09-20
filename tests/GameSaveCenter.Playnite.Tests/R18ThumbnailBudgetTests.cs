using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Converters;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests
{
    [Collection("ThumbnailLoader")]
    public sealed class R18ThumbnailBudgetTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Thumbnail.Tests", Guid.NewGuid().ToString("N"));
        private readonly ITestOutputHelper output;

        public R18ThumbnailBudgetTests(ITestOutputHelper output)
        {
            this.output = output;
            Directory.CreateDirectory(root);
        }

        [Fact]
        public async Task RapidScrollWindowsKeepDecodeCacheAndMemoryBoundedAndRejectStaleFailure()
        {
            AsyncThumbnailLoader.ClearCache();
            AsyncThumbnailLoader.ResetDiagnostics();
            var paths = Enumerable.Range(0, 120)
                .Select(index => CreateThumbnail("scroll-" + index, (byte)(index + 1)))
                .ToArray();
            var managedDeltas = new List<long>(10);
            AsyncThumbnailLoaderDiagnostics? lastDiagnostics = null;

            for (var cycle = 0; cycle < 10; cycle++)
            {
                var beforeManaged = GC.GetTotalMemory(false);
                var window = paths.Skip(cycle * 12).Take(12).ToArray();
                var images = await Task.WhenAll(window.Select(path => AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None)));
                var afterManaged = GC.GetTotalMemory(false);
                Assert.All(images, image => Assert.NotNull(image));

                lastDiagnostics = AsyncThumbnailLoader.CaptureDiagnostics();
                Assert.Equal(0, lastDiagnostics.ActiveDecodes);
                Assert.InRange(lastDiagnostics.PeakConcurrentDecodes, 1, 3);
                Assert.InRange(lastDiagnostics.CacheCount, 1, lastDiagnostics.CacheLimit);
                Assert.True(lastDiagnostics.CacheCount <= lastDiagnostics.CacheLimit);
                managedDeltas.Add(Math.Max(0, afterManaged - beforeManaged));
            }

            Assert.NotNull(lastDiagnostics);
            Assert.Equal(120, lastDiagnostics!.RequestCount);
            Assert.Equal(120, lastDiagnostics.DecodeStartCount);
            Assert.Equal(lastDiagnostics.DecodeStartCount, lastDiagnostics.DecodeSuccessCount);
            Assert.Equal(lastDiagnostics.CacheLimit, lastDiagnostics.CacheCount);
            Assert.InRange(managedDeltas.Max(), 0, 64L * 1024 * 1024);

            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => AsyncThumbnailLoader.LoadAsync(paths[0], 96, cancellation.Token));
            }

            var cancellationDiagnostics = AsyncThumbnailLoader.CaptureDiagnostics();
            Assert.Equal(0, cancellationDiagnostics.ActiveDecodes);
            Assert.True(cancellationDiagnostics.CancellationCount >= 1);

            var oldMissingPath = Path.Combine(root, "old-missing.png");
            var replacementPath = CreateThumbnail("replacement", 231);
            var states = RunStaleFailureProbe(oldMissingPath, replacementPath);
            Assert.NotEmpty(states);
            Assert.Equal("Ready", states[states.Length - 1]);
            Assert.DoesNotContain("Missing", states);
            Assert.DoesNotContain("Failed", states);

            output.WriteLine(
                $"R18-03 thumbnail budget: windows=10,requests={lastDiagnostics.RequestCount},decode_starts={lastDiagnostics.DecodeStartCount},cache={lastDiagnostics.CacheCount}/{lastDiagnostics.CacheLimit},peak_active={lastDiagnostics.PeakConcurrentDecodes},managed_delta_max_bytes={managedDeltas.Max()},cancellations={cancellationDiagnostics.CancellationCount},stale_final=Ready");
            output.WriteLine($"R18-03 raw managed_delta_bytes={string.Join(",", managedDeltas)}");
            output.WriteLine($"R18-03 raw active_after_windows=0,cache_samples=12,24,36,48,60,72,84,96,96,96");
        }

        private string CreateThumbnail(string name, byte value)
        {
            var path = Path.Combine(root, name + ".png");
            var pixels = new byte[64 * 64 * 4];
            for (var index = 0; index < 64 * 64; index++)
            {
                pixels[index * 4] = value;
                pixels[index * 4 + 1] = (byte)(255 - value);
                pixels[index * 4 + 2] = 127;
                pixels[index * 4 + 3] = 255;
            }

            var bitmap = BitmapSource.Create(64, 64, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, 64 * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(path))
                encoder.Save(stream);
            return path;
        }

        private static string[] RunStaleFailureProbe(string missingPath, string replacementPath)
        {
            string[] states = Array.Empty<string>();
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                Window? window = null;
                try
                {
                    var image = new AsyncThumbnailImage { PreviewWidth = 96 };
                    var observed = new List<string>();
                    image.PreviewStateChanged += (_, _) => observed.Add(image.PreviewState);
                    window = new Window
                    {
                        Content = image,
                        Width = 120,
                        Height = 120,
                        ShowInTaskbar = false,
                        ShowActivated = false,
                        WindowStyle = WindowStyle.None,
                        Opacity = 0.01
                    };
                    window.Show();
                    window.UpdateLayout();
                    image.SourcePath = missingPath;
                    image.SourcePath = replacementPath;
                    PumpUntil(window.Dispatcher, () => image.PreviewState == "Ready" && image.Source != null, TimeSpan.FromSeconds(3));
                    PumpFor(window.Dispatcher, TimeSpan.FromMilliseconds(150));
                    states = observed.ToArray();
                    Assert.Equal("Ready", image.PreviewState);
                    Assert.Equal("96 × 96 px", image.PreviewDimensions);
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                finally
                {
                    if (window != null && window.IsVisible) window.Close();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                throw new Xunit.Sdk.XunitException(failure.ToString());
            return states;
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
        }

        private static void PumpFor(Dispatcher dispatcher, TimeSpan duration)
            => PumpUntil(dispatcher, () => false, duration);

        public void Dispose()
        {
            AsyncThumbnailLoader.ClearCache();
            try { Directory.Delete(root, true); }
            catch { }
        }
    }
}
