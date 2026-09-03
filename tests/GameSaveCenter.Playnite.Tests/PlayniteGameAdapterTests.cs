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

        [Fact]
        public void ExistingPlayActionOrWorkingDirectoryCountsAsInstalled()
        {
            var root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
            var executable = Path.Combine(root, "DeadSpace.exe");
            Directory.CreateDirectory(root);
            File.WriteAllText(executable, string.Empty);
            try
            {
                Assert.True(PlayniteGameAdapter.IsLocalPathPresent(executable, null));
                Assert.True(PlayniteGameAdapter.IsLocalPathPresent(null, root));
                Assert.False(PlayniteGameAdapter.IsLocalPathPresent("steam://rungameid/17470", null));
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
