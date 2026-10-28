using System;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Round5OverviewSaveContextTests
{
    [Fact]
    public void OverviewStatusLabelIsNeutralAndSaveHistoryShowsSelectedIdentity()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var overview = new OverviewView();
                Assert.Equal("当前状态", Assert.IsType<TextBlock>(overview.FindName("OverviewTodayHeroEyebrow")).Text);
                var save = new SaveCenterView { DataContext = new SaveFixture { SelectedGame = new GameFixture { Name = "合成游戏", IdentityDisplay = "合成游戏 · 合成平台" } } };
                var target = Assert.IsType<TextBlock>(save.FindName("SaveHistoryTargetGame"));
                // The tab content is not attached until a host selects it; provide the same
                // context explicitly for this detached binding fixture.
                target.DataContext = save.DataContext;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                target.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                BindingOperations.GetBindingExpression(target, AutomationProperties.HelpTextProperty)?.UpdateTarget();
                Assert.Equal("合成游戏", target.Text);
                Assert.Equal("合成游戏 · 合成平台", AutomationProperties.GetHelpText(target));
                save.DataContext = new SaveFixture();
                target.DataContext = save.DataContext;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                target.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                Assert.Equal("未选择游戏", target.Text);
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    public sealed class SaveFixture { public GameFixture? SelectedGame { get; set; } }
    public sealed class GameFixture { public string Name { get; set; } = ""; public string IdentityDisplay { get; set; } = ""; }
}
