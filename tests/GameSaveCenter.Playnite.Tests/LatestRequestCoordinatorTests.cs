using System.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class LatestRequestCoordinatorTests
    {
        [Fact]
        public void StartingANewerScopeCancelsTheOldScopeAndOnlyTheNewestCanCommit()
        {
            using var coordinator = new LatestRequestCoordinator();
            var first = coordinator.Begin();
            var second = coordinator.Begin();

            Assert.True(first.Token.IsCancellationRequested);
            Assert.False(coordinator.IsCurrent(first));
            Assert.True(coordinator.IsCurrent(second));

            coordinator.End(second);
            Assert.False(coordinator.IsCurrent(second));
        }

        [Fact]
        public void CancelInvalidatesTheCurrentScopeWithoutAReplacement()
        {
            using var coordinator = new LatestRequestCoordinator();
            var scope = coordinator.Begin();

            coordinator.Cancel();

            Assert.True(scope.Token.IsCancellationRequested);
            Assert.False(coordinator.IsCurrent(scope));
        }

        [Fact]
        public void ReplacingTwentyReadsLeavesOnlyTheLatestCommitCandidate()
        {
            using var coordinator = new LatestRequestCoordinator();
            var scopes = new LatestRequestCoordinator.RequestScope[20];

            for (var index = 0; index < scopes.Length; index++)
                scopes[index] = coordinator.Begin();

            for (var index = 0; index < scopes.Length - 1; index++)
            {
                Assert.True(scopes[index].Token.IsCancellationRequested);
                Assert.False(coordinator.IsCurrent(scopes[index]));
            }

            Assert.True(coordinator.IsCurrent(scopes[scopes.Length - 1]));
        }

        [Fact]
        public void SlowSuccessFromOldContextCannotReplaceANewContextFailure()
        {
            using var coordinator = new LatestRequestCoordinator();
            var oldContext = coordinator.Begin();
            var newContext = coordinator.Begin();
            var visible = new ReadSurface("game-b|media|收藏", "B 标题", "B 数据", "b-item", "B 更新时间");

            if (coordinator.IsCurrent(newContext))
                visible.Failure = "B 读取失败";
            if (coordinator.IsCurrent(oldContext))
                visible = new ReadSurface("game-a|media|全部", "A 标题", "A 数据", "a-item", "A 更新时间");

            Assert.Equal("game-b|media|收藏", visible.ContextKey);
            Assert.Equal("B 标题", visible.Title);
            Assert.Equal("B 数据", visible.Data);
            Assert.Equal("b-item", visible.SelectedId);
            Assert.Equal("B 更新时间", visible.UpdatedAt);
            Assert.Equal("B 读取失败", visible.Failure);
        }

        private sealed class ReadSurface
        {
            public ReadSurface(string contextKey, string title, string data, string selectedId, string updatedAt, string? failure = null)
            {
                ContextKey = contextKey;
                Title = title;
                Data = data;
                SelectedId = selectedId;
                UpdatedAt = updatedAt;
                Failure = failure;
            }

            public string ContextKey { get; }
            public string Title { get; }
            public string Data { get; }
            public string SelectedId { get; }
            public string UpdatedAt { get; }
            public string? Failure { get; set; }
        }
    }
}
