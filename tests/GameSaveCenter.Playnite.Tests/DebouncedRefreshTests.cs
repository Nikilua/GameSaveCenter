using System;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class DebouncedRefreshTests
    {
        [Fact]
        public void EmptyQueryRefreshesImmediately()
        {
            var count = 0;
            var debouncer = new DebouncedRefresh(() => Interlocked.Increment(ref count), TimeSpan.FromMilliseconds(60));

            debouncer.Schedule("");
            debouncer.Schedule(null);

            Assert.Equal(2, Volatile.Read(ref count));
        }

        [Fact]
        public async Task RapidNonEmptyQueriesRefreshOnce()
        {
            var count = 0;
            var debouncer = new DebouncedRefresh(() => Interlocked.Increment(ref count), TimeSpan.FromMilliseconds(60));

            debouncer.Schedule("a");
            debouncer.Schedule("ab");
            debouncer.Schedule("abc");
            await Task.Delay(250);

            Assert.Equal(1, Volatile.Read(ref count));
        }

        [Fact]
        public async Task CancelPreventsPendingRefresh()
        {
            var count = 0;
            var debouncer = new DebouncedRefresh(() => Interlocked.Increment(ref count), TimeSpan.FromMilliseconds(60));

            debouncer.Schedule("abc");
            debouncer.Cancel();
            await Task.Delay(250);

            Assert.Equal(0, Volatile.Read(ref count));
        }

        [Fact]
        public async Task ClearingQueryRefreshesImmediatelyAndCancelsPendingRefresh()
        {
            var count = 0;
            var debouncer = new DebouncedRefresh(() => Interlocked.Increment(ref count), TimeSpan.FromMilliseconds(60));

            debouncer.Schedule("abc");
            debouncer.Schedule("");
            await Task.Delay(250);

            Assert.Equal(1, Volatile.Read(ref count));
        }
    }
}
