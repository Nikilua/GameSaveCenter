using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Views
{
    public partial class AcrylicProductionShellView
    {
        private string? CurrentShortcutGameId
        {
            get
            {
                var gameId = commandDirectoryContextSnapshot?.GameId;
                return !string.IsNullOrWhiteSpace(gameId)
                    && string.Equals(viewModel?.SelectedGame?.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)
                    ? gameId : null;
            }
        }

        private void RefreshGameShortcuts(string? preferredId = null)
        {
            if (GameShortcutsPanel == null || GameShortcutChoices == null) return;
            var gameId = CurrentShortcutGameId;
            GameShortcutsPanel.Visibility = gameId == null ? Visibility.Collapsed : Visibility.Visible;
            var ids = gameId == null || viewModel == null
                ? Array.Empty<string>() : viewModel.GetGameCommandShortcuts(gameId);
            var items = ids.Select(id => commandDirectoryItems.FirstOrDefault(item => item.Id == id))
                .OfType<CommandDirectoryItem>().ToArray();
            var previous = preferredId ?? (GameShortcutChoices.SelectedItem as CommandDirectoryItem)?.Id;
            GameShortcutChoices.ItemsSource = items;
            GameShortcutChoices.SelectedItem = items.FirstOrDefault(item => item.Id == previous) ?? items.FirstOrDefault();
            RefreshGameShortcutActions();
        }

        private void RefreshGameShortcutActions()
        {
            if (GameShortcutAddButton == null || GameShortcutChoices == null) return;
            var canEdit = CurrentShortcutGameId != null;
            var result = CommandDirectoryResultsListBox?.SelectedItem as CommandDirectoryItem;
            var ids = GameShortcutChoices.Items.OfType<CommandDirectoryItem>().Select(item => item.Id).ToArray();
            GameShortcutAddButton.IsEnabled = canEdit && result != null && CommandDirectoryPreferencePolicy.IsGameShortcut(result.Id)
                && !ids.Contains(result.Id, StringComparer.Ordinal);
            var shortcut = GameShortcutChoices.SelectedItem as CommandDirectoryItem;
            GameShortcutOpenButton.IsEnabled = canEdit && viewModel?.IsBusy == false && shortcut?.CanExecute(null) == true;
            GameShortcutRemoveButton.IsEnabled = canEdit && shortcut != null;
            GameShortcutUpButton.IsEnabled = canEdit && GameShortcutChoices.SelectedIndex > 0;
            GameShortcutDownButton.IsEnabled = canEdit && GameShortcutChoices.SelectedIndex >= 0 && GameShortcutChoices.SelectedIndex + 1 < ids.Length;
        }

        private void OnGameShortcutSelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshGameShortcutActions();

        private void OnGameShortcutAddClick(object sender, RoutedEventArgs e)
        {
            var gameId = CurrentShortcutGameId;
            if (gameId == null || viewModel == null || !(CommandDirectoryResultsListBox.SelectedItem is CommandDirectoryItem item)
                || !CommandDirectoryPreferencePolicy.IsGameShortcut(item.Id)) return;
            var shortcuts = viewModel.GetGameCommandShortcuts(gameId).ToList();
            if (shortcuts.Contains(item.Id, StringComparer.Ordinal)) return;
            shortcuts.Add(item.Id);
            SaveGameShortcuts(gameId, shortcuts.ToArray(), item.Id);
            GameShortcutChoices.Focus();
        }

        private void OnGameShortcutEditClick(object sender, RoutedEventArgs e)
        {
            var gameId = CurrentShortcutGameId;
            if (gameId == null || viewModel == null || !(GameShortcutChoices.SelectedItem is CommandDirectoryItem item)) return;
            var shortcuts = viewModel.GetGameCommandShortcuts(gameId).ToList();
            if (ReferenceEquals(sender, GameShortcutRemoveButton))
                shortcuts.Remove(item.Id);
            else if (!CommandDirectoryPreferencePolicy.TryMoveShortcut(shortcuts, item.Id,
                ReferenceEquals(sender, GameShortcutUpButton) ? -1 : 1)) return;
            SaveGameShortcuts(gameId, shortcuts.ToArray(), item.Id);
            GameShortcutChoices.Focus();
        }

        private void SaveGameShortcuts(string gameId, string[] shortcuts, string preferredId)
        {
            if (viewModel == null || !viewModel.TrySaveGameCommandShortcuts(gameId, shortcuts))
            {
                SetCommandDirectoryNotice("常用操作未能保存。");
                return;
            }
            SetCommandDirectoryNotice(string.Empty);
            RefreshGameShortcuts(preferredId);
        }

        private void OnGameShortcutOpenClick(object sender, RoutedEventArgs e)
        {
            if (CurrentShortcutGameId == null || viewModel?.IsBusy != false
                || !(GameShortcutChoices.SelectedItem is CommandDirectoryItem item) || !item.TryExecute()) return;
            CloseCommandDirectoryForItem(item);
            e.Handled = true;
        }
    }
}
