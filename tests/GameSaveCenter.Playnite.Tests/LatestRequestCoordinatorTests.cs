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
    }
}
