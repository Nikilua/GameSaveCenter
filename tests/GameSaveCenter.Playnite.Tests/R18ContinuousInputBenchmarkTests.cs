using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class R18ContinuousInputBenchmarkTests
    {
        private readonly ITestOutputHelper output;

        public R18ContinuousInputBenchmarkTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GamePicker2000And10000_ReplayTypingPasteDeleteAndImeCommit()
        {
            var small = await MeasureDatasetAsync(2000);
            var large = await MeasureDatasetAsync(10000);

            Assert.InRange(small.SearchP95Milliseconds, 0, 1000);
            Assert.InRange(large.SearchP95Milliseconds, 0, 1000);
            Assert.Equal(30, small.ContinuousSampleCount);
            Assert.Equal(30, large.ContinuousSampleCount);
            Assert.Equal(1, small.PasteVisibleCount);
            Assert.Equal(1, large.PasteVisibleCount);
            Assert.Equal(small.ItemCount, small.DeleteVisibleCount);
            Assert.Equal(large.ItemCount, large.DeleteVisibleCount);
            Assert.Equal(1, small.ImeVisibleCount);
            Assert.Equal(1, large.ImeVisibleCount);
            Assert.Equal(TimeSpan.FromMilliseconds(20), small.DebounceDelay);
            Assert.Equal(TimeSpan.FromMilliseconds(20), large.DebounceDelay);
            Assert.Equal(1, small.DebouncedRefreshCount);
            Assert.Equal(1, large.DebouncedRefreshCount);

            WriteMetrics(small);
            WriteMetrics(large);
        }

        private async Task<DatasetMetrics> MeasureDatasetAsync(int itemCount)
        {
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SetItems(Enumerable.Range(0, itemCount).Select(Game).ToArray());

            var samples = new List<double>(30);
            var allocationSamples = new List<long>(30);
            var filterEvaluationSamples = new List<int>(30);
            for (var index = 0; index < 30; index++)
            {
                var queryNumber = itemCount - 1 - index;
                var result = MeasureSynchronousInput(picker, "Game " + queryNumber.ToString("D5"));
                Assert.Equal(1, result.VisibleCount);
                samples.Add(result.ElapsedMilliseconds);
                allocationSamples.Add(result.AllocatedBytes);
                filterEvaluationSamples.Add(result.FilterEvaluationCount);
            }

            var paste = MeasureSynchronousInput(picker, "Game " + (itemCount - 1).ToString("D5"));
            var delete = MeasureSynchronousInput(picker, string.Empty);
            var ime = MeasureSynchronousInput(picker, "中文游戏 00000");
            Assert.Equal(1, paste.VisibleCount);
            Assert.Equal(itemCount, delete.VisibleCount);
            Assert.Equal(1, ime.VisibleCount);

            picker.SearchText = "Game 00001";
            picker.SearchText = "Game 00002";
            picker.SearchText = "Game 00003";
            picker.CancelPendingRefresh();
            var beforeDebounceRefreshes = picker.PerformanceDiagnostics.RefreshCount;
            var debounceTimer = Stopwatch.StartNew();
            picker.SearchText = "Game " + (itemCount - 2).ToString("D5");
            while (picker.PerformanceDiagnostics.RefreshCount == beforeDebounceRefreshes && debounceTimer.Elapsed < TimeSpan.FromSeconds(2))
                await Task.Delay(1);
            debounceTimer.Stop();
            Assert.Equal(1, picker.PerformanceDiagnostics.RefreshCount - beforeDebounceRefreshes);
            Assert.Equal(1, picker.FilteredCount);

            return new DatasetMetrics(
                itemCount,
                Percentile(samples, 0.95),
                samples.Max(),
                allocationSamples.Max(),
                samples.Count,
                paste.VisibleCount,
                delete.VisibleCount,
                ime.VisibleCount,
                paste.ElapsedMilliseconds,
                delete.ElapsedMilliseconds,
                ime.ElapsedMilliseconds,
                filterEvaluationSamples,
                allocationSamples,
                samples,
                picker.PerformanceDiagnostics.SearchDebounceDelay,
                1,
                debounceTimer.Elapsed.TotalMilliseconds);
        }

        private void WriteMetrics(DatasetMetrics metrics)
        {
            output.WriteLine(
                $"R18-01 input benchmark: {metrics.ItemCount}={{p95_ms={metrics.SearchP95Milliseconds:0.###},max_ms={metrics.SearchMaxMilliseconds:0.###},filter_evaluations_total={metrics.FilterEvaluationCount},managed_delta_max_bytes={metrics.MaxAllocationBytes},paste_ms={metrics.PasteMilliseconds:0.###},delete_ms={metrics.DeleteMilliseconds:0.###},ime_ms={metrics.ImeMilliseconds:0.###},debounce_ms={metrics.DebounceElapsedMilliseconds:0.###}}}");
            output.WriteLine($"R18-01 raw {metrics.ItemCount} search_ms={string.Join(",", metrics.SearchMilliseconds.Select(value => value.ToString("0.###")))}");
            output.WriteLine($"R18-01 raw {metrics.ItemCount} filter_evaluations={string.Join(",", metrics.FilterEvaluationSamples)}");
            output.WriteLine($"R18-01 raw {metrics.ItemCount} managed_delta_bytes={string.Join(",", metrics.ManagedDeltaSamples)}");
        }

        private static InputMeasurement MeasureSynchronousInput(GamePickerViewModel picker, string query)
        {
            picker.SearchText = query;
            picker.CancelPendingRefresh();
            var allocatedBefore = GC.GetTotalMemory(false);
            var timer = Stopwatch.StartNew();
            picker.RefreshNow();
            timer.Stop();
            var allocatedAfter = GC.GetTotalMemory(false);
            var allocatedBytes = Math.Max(0, allocatedAfter - allocatedBefore);
            return new InputMeasurement(
                timer.Elapsed.TotalMilliseconds,
                allocatedBytes,
                picker.FilteredCount,
                picker.PerformanceDiagnostics.LastFilterEvaluationCount);
        }

        private static GameStatusDto Game(int index)
        {
            var isChinese = index == 0;
            return new GameStatusDto
            {
                PlayniteId = "game-" + index.ToString("D5"),
                Name = isChinese ? "中文游戏 00000" : "Game " + index.ToString("D5"),
                Platform = GamePlatformKind.Other,
                IsInstalled = true,
                LudusaviMatched = true,
                HealthState = "Ready"
            };
        }

        private static double Percentile(IReadOnlyList<double> samples, double percentile)
        {
            var ordered = samples.OrderBy(value => value).ToArray();
            var index = (int)Math.Ceiling(ordered.Length * percentile) - 1;
            return ordered[Math.Max(0, Math.Min(index, ordered.Length - 1))];
        }

        private readonly struct InputMeasurement
        {
            public InputMeasurement(double elapsedMilliseconds, long allocatedBytes, int visibleCount, int filterEvaluationCount)
            {
                ElapsedMilliseconds = elapsedMilliseconds;
                AllocatedBytes = allocatedBytes;
                VisibleCount = visibleCount;
                FilterEvaluationCount = filterEvaluationCount;
            }

            public double ElapsedMilliseconds { get; }
            public long AllocatedBytes { get; }
            public int VisibleCount { get; }
            public int FilterEvaluationCount { get; }
        }

        private readonly struct DatasetMetrics
        {
            public DatasetMetrics(
                int itemCount,
                double searchP95Milliseconds,
                double searchMaxMilliseconds,
                long maxAllocationBytes,
                int continuousSampleCount,
                int pasteVisibleCount,
                int deleteVisibleCount,
                int imeVisibleCount,
                double pasteMilliseconds,
                double deleteMilliseconds,
                double imeMilliseconds,
                IReadOnlyList<int> filterEvaluationSamples,
                IReadOnlyList<long> managedDeltaSamples,
                IReadOnlyList<double> searchMilliseconds,
                TimeSpan debounceDelay,
                int debouncedRefreshCount,
                double debounceElapsedMilliseconds)
            {
                ItemCount = itemCount;
                SearchP95Milliseconds = searchP95Milliseconds;
                SearchMaxMilliseconds = searchMaxMilliseconds;
                MaxAllocationBytes = maxAllocationBytes;
                ContinuousSampleCount = continuousSampleCount;
                PasteVisibleCount = pasteVisibleCount;
                DeleteVisibleCount = deleteVisibleCount;
                ImeVisibleCount = imeVisibleCount;
                PasteMilliseconds = pasteMilliseconds;
                DeleteMilliseconds = deleteMilliseconds;
                ImeMilliseconds = imeMilliseconds;
                FilterEvaluationSamples = filterEvaluationSamples;
                ManagedDeltaSamples = managedDeltaSamples;
                SearchMilliseconds = searchMilliseconds;
                DebounceDelay = debounceDelay;
                DebouncedRefreshCount = debouncedRefreshCount;
                DebounceElapsedMilliseconds = debounceElapsedMilliseconds;
            }

            public int ItemCount { get; }
            public double SearchP95Milliseconds { get; }
            public double SearchMaxMilliseconds { get; }
            public long MaxAllocationBytes { get; }
            public int ContinuousSampleCount { get; }
            public int PasteVisibleCount { get; }
            public int DeleteVisibleCount { get; }
            public int ImeVisibleCount { get; }
            public double PasteMilliseconds { get; }
            public double DeleteMilliseconds { get; }
            public double ImeMilliseconds { get; }
            public IReadOnlyList<int> FilterEvaluationSamples { get; }
            public IReadOnlyList<long> ManagedDeltaSamples { get; }
            public IReadOnlyList<double> SearchMilliseconds { get; }
            public long FilterEvaluationCount => FilterEvaluationSamples.Sum(value => (long)value);
            public TimeSpan DebounceDelay { get; }
            public int DebouncedRefreshCount { get; }
            public double DebounceElapsedMilliseconds { get; }
        }
    }
}
