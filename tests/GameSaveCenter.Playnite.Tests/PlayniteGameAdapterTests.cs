using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class PlayniteGameAdapterTests
    {
        [Fact]
        public void ExistingInstallDirectoryCountsAsInstalled()
        {
            var path = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            try
            {
                Assert.True(PlayniteGameAdapter.IsInstallDirectoryPresent(path));
            }
            finally
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
        }

        [Fact]
        public void MissingOrEmptyInstallDirectoryDoesNotCountAsInstalled()
        {
            Assert.False(PlayniteGameAdapter.IsInstallDirectoryPresent(null));
            Assert.False(PlayniteGameAdapter.IsInstallDirectoryPresent(string.Empty));
            Assert.False(PlayniteGameAdapter.IsInstallDirectoryPresent(Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"))));
        }
    }
}
