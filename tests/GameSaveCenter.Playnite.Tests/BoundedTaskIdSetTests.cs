using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class BoundedTaskIdSetTests
    {
        [Fact]
        public void RetainsRecentIdsAndEvictsOldestIds()
        {
            var set = new BoundedTaskIdSet(2);

            Assert.True(set.TryAdd("task-1"));
            Assert.True(set.TryAdd("task-2"));
            Assert.False(set.TryAdd("TASK-1"));
            Assert.True(set.TryAdd("task-3"));

            Assert.False(set.Contains("task-1"));
            Assert.True(set.Contains("task-2"));
            Assert.True(set.Contains("task-3"));
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public async Task ConcurrentAddsRemainBoundedAndDeduplicated()
        {
            var set = new BoundedTaskIdSet(64);
            var adds = Enumerable.Range(0, 200)
                .SelectMany(index => Enumerable.Repeat("task-" + (index % 20), 4))
                .Select(id => Task.Run(() => set.TryAdd(id)))
                .ToArray();

            await Task.WhenAll(adds);

            Assert.Equal(20, set.Count);
            Assert.True(set.Contains("task-0"));
            Assert.True(set.Contains("task-19"));
        }
    }
}
