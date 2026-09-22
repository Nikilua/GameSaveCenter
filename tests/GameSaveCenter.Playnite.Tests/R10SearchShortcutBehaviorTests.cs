using System.Windows.Input;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10SearchShortcutBehaviorTests
{
    [Fact]
    public void CtrlFFocusesTheCurrentPageOnlyWhenNoOverlayOwnsInput()
    {
        Assert.True(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.F, ModifierKeys.Control, false, false, false));
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.F, ModifierKeys.Control, true, false, false));
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.F, ModifierKeys.Control, false, true, false));
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.F, ModifierKeys.Control, false, false, true));
    }

    [Fact]
    public void EditingShortcutsAreNotConsumedByTheSearchRoute()
    {
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.Z, ModifierKeys.Control, false, false, false));
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.C, ModifierKeys.Control, false, false, false));
        Assert.False(SearchShortcutPolicy.ShouldFocusWorkspaceSearch(Key.F, ModifierKeys.None, false, false, false));
    }
}
