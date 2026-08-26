using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class PlayniteGameStartedSubscriptionTests
    {
        [Fact]
        public void FirstDashboardLoadDeliversGameStartedOnce()
        {
            var handlers = new List<Action<Guid>>();
            var callbackCount = 0;
            var subscription = Create(handlers, _ => callbackCount++);

            subscription.Start();
            handlers.Single()(Guid.NewGuid());

            Assert.True(subscription.IsSubscribed);
            Assert.Equal(1, callbackCount);
        }

        [Fact]
        public void UnloadedDashboardStopsCallbacks()
        {
            var handlers = new List<Action<Guid>>();
            var callbackCount = 0;
            var subscription = Create(handlers, _ => callbackCount++);

            subscription.Start();
            subscription.Stop();

            Assert.False(subscription.IsSubscribed);
            Assert.Empty(handlers);
            Assert.Equal(0, callbackCount);
        }

        [Fact]
        public void ReloadedDashboardCanReceiveGameStartedAgain()
        {
            var handlers = new List<Action<Guid>>();
            var callbackCount = 0;
            var subscription = Create(handlers, _ => callbackCount++);

            subscription.Start();
            subscription.Stop();
            subscription.Start();
            handlers.Single()(Guid.NewGuid());

            Assert.True(subscription.IsSubscribed);
            Assert.Equal(1, callbackCount);
        }

        [Fact]
        public void RepeatedLoadedAndUnloadedCyclesKeepOneHandlerPerVisibleDashboard()
        {
            var handlers = new List<Action<Guid>>();
            var callbackCount = 0;
            var subscription = Create(handlers, _ => callbackCount++);

            for (var i = 0; i < 3; i++)
            {
                subscription.Start();
                subscription.Start();
                Assert.Single(handlers);
                subscription.Stop();
                subscription.Stop();
                Assert.Empty(handlers);
            }

            subscription.Start();
            handlers.Single()(Guid.NewGuid());

            Assert.Equal(1, callbackCount);
        }

        [Fact]
        public void PendingSelectionRemainsOwnedByDashboardHandlerUntilGamesArrive()
        {
            var root = FindRepositoryRoot();
            var viewModel = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

            Assert.Contains("pendingAutoSelectPlayniteId = playniteId.ToString(\"D\");", viewModel);
            Assert.Contains("TryApplyPendingAutoSelection();", viewModel);
            Assert.Contains("if (game == null) return;", viewModel);
            Assert.Contains("pendingAutoSelectPlayniteId = null;", viewModel);
        }

        private static PlayniteGameStartedSubscription Create(
            ICollection<Action<Guid>> handlers,
            Action<Guid> callback)
            => new PlayniteGameStartedSubscription(
                handler => handlers.Add(handler),
                handler => handlers.Remove(handler),
                callback);

        private static string FindRepositoryRoot()
        {
            var directory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
