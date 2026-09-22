using System.Windows.Input;

namespace GameSaveCenter.Playnite.Views
{
    internal static class SearchShortcutPolicy
    {
        public static bool ShouldFocusWorkspaceSearch(
            Key key,
            ModifierKeys modifiers,
            bool dialogVisible,
            bool pickerVisible,
            bool compactGameBrowserOpen)
            => key == Key.F
               && (modifiers & ModifierKeys.Control) == ModifierKeys.Control
               && !dialogVisible
               && !pickerVisible
               && !compactGameBrowserOpen;
    }
}
