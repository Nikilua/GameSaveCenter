using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class LargeLibraryPerformanceTests
    {
        [Fact]
        public void UnchangedGamePickerWith2000Games_EmitsNoSecondCollectionNotification()
        {
            using var picker = new GamePickerViewModel();
            var notifications = 0;
            picker.Items.CollectionChanged += (_, _) => notifications++;
            var games = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();

            picker.SetItems(games);
            Assert.Equal(1, notifications);

            picker.SetItems(Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray());
            Assert.Equal(1, notifications);
            Assert.Equal(2000, picker.Items.Count);
        }

        [Fact]
        public void ChangedGamePickerWith2000Games_EmitsOneResetWithoutPerItemAdds()
        {
            using var picker = new GamePickerViewModel();
            var resets = 0;
            var adds = 0;
            picker.Items.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
                if (args.Action == NotifyCollectionChangedAction.Add) adds++;
            };
            var games = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();
            picker.SetItems(games);

            var changed = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();
            changed[1500].HealthState = "Attention";
            picker.SetItems(changed);

            Assert.Equal(2, resets);
            Assert.Equal(0, adds);
            Assert.Equal(2000, picker.Items.Count);
        }

        [Fact]
        public void UnchangedTasksWith2000Rows_EmitsNoSecondCollectionChanged()
        {
            var tasks = Enumerable.Range(0, 2000).Select(i => TaskStatus("task-" + i, i % 100)).ToArray();
            var collection = new BatchObservableCollection<TaskStatusDto>();
            var notifications = 0;
            collection.CollectionChanged += (_, _) => notifications++;

            Assert.True(collection.ReplaceAll(tasks, SnapshotComparers.Task));
            Assert.Equal(1, notifications);

            Assert.False(collection.ReplaceAll(tasks, SnapshotComparers.Task));
            Assert.Equal(1, notifications);
            Assert.Equal(2000, collection.Count);
        }

        [Fact]
        public async Task GamePicker2000_Benchmark_WritesMeasuredTimings()
        {
            using var picker = new GamePickerViewModel();
            var games = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();

            var timer = Stopwatch.StartNew();
            picker.SetItems(games);
            timer.Stop();
            var firstSetMs = timer.ElapsedMilliseconds;

            timer.Restart();
            picker.SetItems(Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray());
            timer.Stop();
            var unchangedSetMs = timer.ElapsedMilliseconds;

            var changed = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();
            changed[1500].HealthState = "Attention";
            timer.Restart();
            picker.SetItems(changed);
            timer.Stop();
            var changedSetMs = timer.ElapsedMilliseconds;

            timer.Restart();
            picker.SearchText = "Game 1999";
            Assert.True(await WaitForFilteredCountAsync(picker, 1, TimeSpan.FromSeconds(5)));
            timer.Stop();
            var searchRefreshMs = timer.ElapsedMilliseconds;

            timer.Restart();
            picker.SearchText = "";
            Assert.True(await WaitForFilteredCountAsync(picker, 2000, TimeSpan.FromSeconds(5)));
            timer.Stop();
            var searchClearMs = timer.ElapsedMilliseconds;

            // Q25-01 requires a repeatable interaction sample rather than a single
            // average. Warm the filter path first, then use a new matching game name
            // for every formal sample so each input changes the visible result set.
            for (var warmup = 0; warmup < 5; warmup++)
            {
                var query = warmup % 2 == 0 ? "" : "Game 1999";
                picker.SearchText = query;
                Assert.True(await WaitForFilteredCountAsync(picker, query.Length == 0 ? 2000 : 1, TimeSpan.FromSeconds(5)));
            }

            var searchSamples = new List<long>(30);
            var searchQueries = new List<string>(30);
            var previousVisibleIds = picker.ItemsView.Cast<GamePickerItem>().Select(item => item.PlayniteId).ToArray();
            var changedResultSets = 0;
            for (var sample = 0; sample < 30; sample++)
            {
                var query = "Game " + (1999 - sample);
                searchQueries.Add(query);
                timer.Restart();
                picker.SearchText = query;
                Assert.True(await WaitForFilteredIdsAsync(picker, new[] { query }, TimeSpan.FromSeconds(5)),
                    "FilteredCount did not reach the expected result for " + query + ".");
                timer.Stop();
                searchSamples.Add(timer.ElapsedMilliseconds);

                var visibleIds = picker.ItemsView.Cast<GamePickerItem>().Select(item => item.PlayniteId).ToArray();
                if (!visibleIds.SequenceEqual(previousVisibleIds))
                    changedResultSets++;
                previousVisibleIds = visibleIds;
            }

            Assert.Equal(30, changedResultSets);

            var searchP50Ms = Percentile(searchSamples, 0.50);
            var searchP95Ms = Percentile(searchSamples, 0.95);
            var searchMaxMs = searchSamples.Max();

            var tasks = Enumerable.Range(0, 2000).Select(i => TaskStatus("task-" + i, i % 100)).ToArray();
            var collection = new BatchObservableCollection<TaskStatusDto>();
            timer.Restart();
            collection.ReplaceAll(tasks, SnapshotComparers.Task);
            timer.Stop();
            var taskFirstReplaceMs = timer.ElapsedMilliseconds;
            timer.Restart();
            collection.ReplaceAll(tasks, SnapshotComparers.Task);
            timer.Stop();
            var taskUnchangedReplaceMs = timer.ElapsedMilliseconds;

            // These are deliberately broad regression guards rather than machine-specific
            // micro-benchmarks. A 2000-item update should never turn into a multi-second UI
            // freeze; the detailed timings above remain the diagnostic baseline artifact.
            Assert.InRange(firstSetMs, 0, 5000);
            Assert.InRange(unchangedSetMs, 0, 1000);
            Assert.InRange(changedSetMs, 0, 5000);
            Assert.InRange(taskFirstReplaceMs, 0, 5000);
            Assert.InRange(taskUnchangedReplaceMs, 0, 1000);
            Assert.InRange(searchP50Ms, 0, 5000);
            // Q25-01's hot-input budget is p95 <= 100 ms. Keep this as a
            // regression guard instead of leaving the requirement only in the
            // checked-in benchmark artifact.
            Assert.InRange(searchP95Ms, 0, 100);
            Assert.InRange(searchMaxMs, 0, 5000);

            var artifactRoot = Environment.GetEnvironmentVariable("GSC_TEST_ARTIFACT_ROOT");
            var benchmarkDirectory = string.IsNullOrWhiteSpace(artifactRoot)
                ? Path.Combine(Path.GetTempPath(), "GameSaveCenter", "ui-qa", "benchmarks")
                : Path.Combine(artifactRoot, "ui-qa", "benchmarks");
            Directory.CreateDirectory(benchmarkDirectory);
            File.WriteAllText(Path.Combine(benchmarkDirectory, "large-library.txt"),
                $"first_set_ms={firstSetMs}\n" +
                $"unchanged_set_ms={unchangedSetMs}\n" +
                $"changed_set_ms={changedSetMs}\n" +
                $"search_refresh_ms={searchRefreshMs}\n" +
                $"search_clear_ms={searchClearMs}\n" +
                "search_warmup_samples=5\n" +
                "search_measured_samples=30\n" +
                "search_measurement=SearchText_to_FilteredCount\n" +
                $"search_p50_ms={searchP50Ms}\n" +
                $"search_p95_ms={searchP95Ms}\n" +
                $"search_max_ms={searchMaxMs}\n" +
                $"search_queries={string.Join("|", searchQueries)}\n" +
                $"search_samples_ms={string.Join(",", searchSamples)}\n" +
                $"search_changed_result_sets={changedResultSets}\n" +
                $"task_first_replace_ms={taskFirstReplaceMs}\n" +
                $"task_unchanged_replace_ms={taskUnchangedReplaceMs}\n");
        }

        [Fact]
        public async Task GamePickerBenchmarkWaitTimesOutOnAnImpossibleExpectedCount()
        {
            using var picker = new GamePickerViewModel();
            picker.SetItems(new[] { Game("Game 1") });

            var timer = Stopwatch.StartNew();
            var reached = await WaitForFilteredCountAsync(picker, 0, TimeSpan.FromMilliseconds(75));

            Assert.False(reached);
            Assert.InRange(timer.ElapsedMilliseconds, 0, 1000);
        }

        private static long Percentile(IReadOnlyList<long> samples, double percentile)
        {
            var ordered = samples.OrderBy(value => value).ToArray();
            var index = (int)Math.Ceiling(ordered.Length * percentile) - 1;
            index = Math.Max(0, Math.Min(index, ordered.Length - 1));
            return ordered[index];
        }

        private static async Task<bool> WaitForFilteredCountAsync(GamePickerViewModel picker, int expected, TimeSpan timeout)
        {
            var timeoutTimer = Stopwatch.StartNew();
            while (picker.FilteredCount != expected && timeoutTimer.Elapsed < timeout)
                await Task.Delay(10);
            return picker.FilteredCount == expected;
        }

        private static async Task<bool> WaitForFilteredIdsAsync(GamePickerViewModel picker, IReadOnlyList<string> expectedIds, TimeSpan timeout)
        {
            var timeoutTimer = Stopwatch.StartNew();
            while (timeoutTimer.Elapsed < timeout)
            {
                var visibleIds = picker.ItemsView.Cast<GamePickerItem>().Select(item => item.PlayniteId).ToArray();
                if (visibleIds.SequenceEqual(expectedIds))
                    return true;

                await Task.Delay(10);
            }

            return picker.ItemsView.Cast<GamePickerItem>().Select(item => item.PlayniteId).SequenceEqual(expectedIds);
        }

        private static GameStatusDto Game(string name)
            => new GameStatusDto
            {
                PlayniteId = name,
                Name = name,
                Platform = GamePlatformKind.Other,
                IsInstalled = true,
                LudusaviMatched = true,
                HealthState = "Ready"
            };

        private static TaskStatusDto TaskStatus(string id, int progress)
            => new TaskStatusDto
            {
                TaskId = id,
                TaskType = "Backup",
                State = TaskState.Running,
                ProgressPercent = progress
            };
    }
}
