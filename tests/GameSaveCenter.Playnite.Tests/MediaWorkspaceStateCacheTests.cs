using System;
using GameSaveCenter.Contracts;
using Xunit;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MediaWorkspaceStateCacheTests
    {
        [Fact]
        public void SameContextFailureKeepsTheLastSuccessfulTimestampAndBecomesStale()
        {
            var cache = new MediaWorkspaceStateCache();

            cache.Begin("game-a|全部|");
            cache.Complete("game-a|全部|", hasItems: true);
            var lastSuccess = cache.LastSuccessUtc;

            cache.Begin("game-a|全部|");
            cache.Fail("game-a|全部|", "读取失败");

            Assert.Equal(WorkspaceDataState.Stale, cache.State);
            Assert.Equal(lastSuccess, cache.LastSuccessUtc);
            Assert.Equal("读取失败", cache.ErrorMessage);
        }

        [Fact]
        public void NewContextFailureDoesNotReuseThePreviousContextCache()
        {
            var cache = new MediaWorkspaceStateCache();

            cache.Begin("game-a|全部|");
            cache.Complete("game-a|全部|", hasItems: true);
            cache.Begin("game-b|全部|");

            // A late response from game A must not complete the game B context.
            cache.Complete("game-a|全部|", hasItems: true);
            Assert.Equal(WorkspaceDataState.Loading, cache.State);
            Assert.Null(cache.LastSuccessUtc);

            cache.Fail("game-b|全部|", "B 读取失败");

            Assert.Equal(WorkspaceDataState.Error, cache.State);
            Assert.Null(cache.LastSuccessUtc);
            Assert.Equal("B 读取失败", cache.ErrorMessage);
        }

        [Fact]
        public void InboxModesDoNotShareSuccessTimestamps()
        {
            var cache = new MediaWorkspaceStateCache();

            cache.Begin("待归类");
            cache.Complete("待归类", hasItems: true);
            Assert.True(cache.HasCurrentContextSuccess);

            cache.Begin("已忽略");
            cache.Fail("已忽略", "已忽略列表不可用");

            Assert.Equal(WorkspaceDataState.Error, cache.State);
            Assert.False(cache.HasCurrentContextSuccess);
            Assert.Null(cache.LastSuccessUtc);
        }

        [Fact]
        public void CancellingAContextWithoutCacheDoesNotInventAReadyState()
        {
            var cache = new MediaWorkspaceStateCache();

            cache.Begin("game-a|收藏|clip");
            cache.Cancel("game-a|收藏|clip", hasItems: true);

            Assert.Equal(WorkspaceDataState.Empty, cache.State);
            Assert.Null(cache.LastSuccessUtc);
        }

        [Fact]
        public void InboxCacheCaptionUsesRelativeTimeAndFullTimeTooltip()
        {
            var lastSuccessUtc = new DateTime(2026, 9, 22, 8, 0, 0, DateTimeKind.Utc);
            var nowUtc = new DateTime(2026, 9, 22, 8, 2, 0, DateTimeKind.Utc);

            var relative = DashboardViewModel.FormatMediaInboxCountCaption(
                isWorkerOffline: true,
                state: WorkspaceDataState.Ready,
                lastSuccessUtc: lastSuccessUtc,
                hasCurrentContextSuccess: true,
                useFullTime: false,
                nowUtc: nowUtc);
            var full = DashboardViewModel.FormatMediaInboxCountCaption(
                isWorkerOffline: true,
                state: WorkspaceDataState.Ready,
                lastSuccessUtc: lastSuccessUtc,
                hasCurrentContextSuccess: true,
                useFullTime: true,
                nowUtc: nowUtc);

            Assert.Equal("离线 · 缓存于 2 分钟前", relative);
            Assert.Contains(TimeDisplayFormatter.Full(lastSuccessUtc), full, StringComparison.Ordinal);
            Assert.DoesNotContain("ToLocalTime", relative, StringComparison.Ordinal);
        }

        [Fact]
        public void InboxCacheCaptionWithoutSuccessDoesNotInventTimestamp()
        {
            var caption = DashboardViewModel.FormatMediaInboxCountCaption(
                isWorkerOffline: true,
                state: WorkspaceDataState.Error,
                lastSuccessUtc: null,
                hasCurrentContextSuccess: false,
                useFullTime: false,
                nowUtc: new DateTime(2026, 9, 22, 8, 2, 0, DateTimeKind.Utc));

            Assert.Equal("离线 · 无法读取", caption);
            Assert.DoesNotContain("缓存于", caption, StringComparison.Ordinal);
            Assert.DoesNotContain("上次成功", caption, StringComparison.Ordinal);
        }
    }
}
