using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class R18DispatcherVisibilityBenchmarkTests
    {
        private readonly ITestOutputHelper output;

        public R18DispatcherVisibilityBenchmarkTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GamePickerDispatcherSeparatesViewModelCompletionFromVisibleContainer()
        {
            DispatcherMetrics metrics = default;
            RunOnSta(() => metrics = MeasureOnControlledWindow());

            Assert.Equal(20, metrics.SampleCount);
            Assert.All(metrics.ViewModelMilliseconds, value => Assert.InRange(value, 0, 1000));
            Assert.All(metrics.VisibleFeedbackMilliseconds, value => Assert.InRange(value, 0, 1000));
            Assert.All(metrics.VisibleCounts, value => Assert.Equal(1, value));
            Assert.All(metrics.ContainerHeights, value => Assert.True(value > 0));
            Assert.All(metrics.ContainerWidths, value => Assert.True(value > 0));

            output.WriteLine(
                $"R18-02 dispatcher benchmark: samples={metrics.SampleCount},vm_p95_ms={Percentile(metrics.ViewModelMilliseconds, 0.95):0.###},visible_feedback_p95_ms={Percentile(metrics.VisibleFeedbackMilliseconds, 0.95):0.###},visible_feedback_max_ms={metrics.VisibleFeedbackMilliseconds.Max():0.###},source=ListBox.ItemContainerGenerator+IsVisible+ActualWidthHeight+UpdateLayout");
            output.WriteLine($"R18-02 raw vm_ms={string.Join(",", metrics.ViewModelMilliseconds.Select(value => value.ToString("0.###")))}");
            output.WriteLine($"R18-02 raw visible_feedback_ms={string.Join(",", metrics.VisibleFeedbackMilliseconds.Select(value => value.ToString("0.###")))}");
            output.WriteLine($"R18-02 raw visible_counts={string.Join(",", metrics.VisibleCounts)}");
            output.WriteLine($"R18-02 raw container_heights={string.Join(",", metrics.ContainerHeights.Select(value => value.ToString("0.##")))}");
            output.WriteLine($"R18-02 raw container_widths={string.Join(",", metrics.ContainerWidths.Select(value => value.ToString("0.##")))}");
        }

        private static DispatcherMetrics MeasureOnControlledWindow()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            const int itemCount = 2000;
            picker.SetItems(Enumerable.Range(0, itemCount).Select(Game).ToArray());

            var list = new ListBox
            {
                Width = 480,
                Height = 180,
                ItemsSource = picker.ItemsView,
                ItemTemplate = CreateItemTemplate()
            };
            var window = new Window
            {
                Content = list,
                Width = 520,
                Height = 220,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.ToolWindow,
                Opacity = 0.01
            };

            var viewModelMilliseconds = new List<double>(20);
            var visibleFeedbackMilliseconds = new List<double>(20);
            var visibleCounts = new List<int>(20);
            var containerHeights = new List<double>(20);
            var containerWidths = new List<double>(20);

            try
            {
                window.Show();
                window.UpdateLayout();
                for (var index = 0; index < 20; index++)
                {
                    var query = "Game " + (itemCount - 1 - index).ToString("D5");
                    var beforeRefresh = picker.PerformanceDiagnostics.RefreshCount;
                    var timer = Stopwatch.StartNew();
                    picker.SearchText = query;
                    PumpUntil(
                        window.Dispatcher,
                        () => picker.PerformanceDiagnostics.RefreshCount > beforeRefresh
                            && string.Equals(picker.PerformanceDiagnostics.LastSearchText, query, StringComparison.Ordinal),
                        TimeSpan.FromSeconds(2));
                    var viewModelMillisecondsForSample = timer.Elapsed.TotalMilliseconds;
                    Assert.Equal(beforeRefresh + 1, picker.PerformanceDiagnostics.RefreshCount);
                    Assert.Equal(query, picker.PerformanceDiagnostics.LastSearchText);

                    PumpUntil(window.Dispatcher, () => HasVisibleContainer(list, window), TimeSpan.FromSeconds(2));
                    timer.Stop();
                    var container = (FrameworkElement)list.ItemContainerGenerator.ContainerFromIndex(0)!;
                    viewModelMilliseconds.Add(viewModelMillisecondsForSample);
                    visibleFeedbackMilliseconds.Add(Math.Max(0, timer.Elapsed.TotalMilliseconds - viewModelMillisecondsForSample));
                    visibleCounts.Add(list.Items.Count);
                    containerHeights.Add(container.ActualHeight);
                    containerWidths.Add(container.ActualWidth);
                }
            }
            finally
            {
                if (window.IsVisible) window.Close();
            }

            return new DispatcherMetrics(
                viewModelMilliseconds,
                visibleFeedbackMilliseconds,
                visibleCounts,
                containerHeights,
                containerWidths);
        }

        private static bool HasVisibleContainer(ListBox list, Window window)
        {
            list.UpdateLayout();
            var container = list.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            return window.IsVisible
                && list.Visibility == Visibility.Visible
                && list.Items.Count == 1
                && container != null
                && container.IsVisible
                && container.ActualWidth > 0
                && container.ActualHeight > 0;
        }

        private static DataTemplate CreateItemTemplate()
        {
            var template = new DataTemplate(typeof(GamePickerItem));
            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding(nameof(GamePickerItem.Name)));
            template.VisualTree = text;
            return template;
        }

        private static GameStatusDto Game(int index)
            => new GameStatusDto
            {
                PlayniteId = "game-" + index.ToString("D5"),
                Name = "Game " + index.ToString("D5"),
                Platform = GamePlatformKind.Other,
                IsInstalled = true,
                LudusaviMatched = true,
                HealthState = "Ready"
            };

        private static double Percentile(IReadOnlyList<double> samples, double percentile)
        {
            var ordered = samples.OrderBy(value => value).ToArray();
            var index = (int)Math.Ceiling(ordered.Length * percentile) - 1;
            return ordered[Math.Max(0, Math.Min(index, ordered.Length - 1))];
        }

        private static void PumpUntil(Dispatcher dispatcher, Func<bool> condition, TimeSpan timeout)
        {
            var frame = new DispatcherFrame();
            var started = DateTime.UtcNow;
            var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(5)
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

        private static void RunOnSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                finally
                {
                    if (!Dispatcher.CurrentDispatcher.HasShutdownStarted)
                        Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                throw new Xunit.Sdk.XunitException(failure.ToString());
        }

        private readonly struct DispatcherMetrics
        {
            public DispatcherMetrics(
                IReadOnlyList<double> viewModelMilliseconds,
                IReadOnlyList<double> visibleFeedbackMilliseconds,
                IReadOnlyList<int> visibleCounts,
                IReadOnlyList<double> containerHeights,
                IReadOnlyList<double> containerWidths)
            {
                ViewModelMilliseconds = viewModelMilliseconds;
                VisibleFeedbackMilliseconds = visibleFeedbackMilliseconds;
                VisibleCounts = visibleCounts;
                ContainerHeights = containerHeights;
                ContainerWidths = containerWidths;
            }

            public IReadOnlyList<double> ViewModelMilliseconds { get; }
            public IReadOnlyList<double> VisibleFeedbackMilliseconds { get; }
            public IReadOnlyList<int> VisibleCounts { get; }
            public IReadOnlyList<double> ContainerHeights { get; }
            public IReadOnlyList<double> ContainerWidths { get; }
            public int SampleCount => ViewModelMilliseconds.Count;
        }
    }
}
