using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiFinesseFoundationTests
{
    [Fact]
    public void CSharpMotionMatchesTheMotionTokenContract()
    {
        Exception? exception = null;
        var fast = TimeSpan.Zero;
        var press = TimeSpan.Zero;
        var normal = TimeSpan.Zero;
        var slow = TimeSpan.Zero;

        var thread = new Thread(() =>
        {
            try
            {
                fast = GscMotion.Fast;
                press = GscMotion.Press;
                normal = GscMotion.Normal;
                slow = GscMotion.Slow;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        var tokens = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "GameSaveCenter.Playnite", "Themes", "MotionTokens.xaml"));
        Assert.Contains("<Duration x:Key=\"GscMotionFast\">0:0:0.12</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionPress\">0:0:0.10</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionNormal\">0:0:0.22</Duration>", tokens);
        Assert.Contains("<Duration x:Key=\"GscMotionSlow\">0:0:0.30</Duration>", tokens);
        Assert.Equal(TimeSpan.FromMilliseconds(120), fast);
        Assert.Equal(TimeSpan.FromMilliseconds(100), press);
        Assert.Equal(TimeSpan.FromMilliseconds(220), normal);
        Assert.Equal(TimeSpan.FromMilliseconds(300), slow);
    }

    [Fact]
    public void MotionHostResourceOverridesTheCanonicalTokenWithoutChangingFallbacks()
    {
        Exception? exception = null;
        var hostDuration = TimeSpan.Zero;
        var canonicalDuration = TimeSpan.Zero;

        var thread = new Thread(() =>
        {
            try
            {
                var host = new FrameworkElement();
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(260));
                hostDuration = GscMotion.GetDuration(host, GscMotion.MotionDurationKind.Normal);
                canonicalDuration = GscMotion.GetDuration(null, GscMotion.MotionDurationKind.Normal);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(TimeSpan.FromMilliseconds(260), hostDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(220), canonicalDuration);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
