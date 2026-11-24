using System;
using System.Linq;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        internal string[] GetGameCommandShortcuts(string gameId)
            => plugin.Settings.GameCommandShortcuts.TryGetValue(gameId, out var ids) ? ids.ToArray() : Array.Empty<string>();

        internal bool TrySaveGameCommandShortcuts(string gameId, string[] ids)
        {
            if (!IsSelectedGame(gameId)) return false;
            var previous = plugin.Settings.GameCommandShortcuts;
            var updated = CommandDirectoryPreferencePolicy.NormalizeGameShortcuts(previous);
            if (ids.Length == 0) updated.Remove(gameId);
            else updated[gameId] = ids.ToList();
            plugin.Settings.GameCommandShortcuts = updated;
            try
            {
                PersistUiPreference();
                return true;
            }
            catch (Exception ex)
            {
                plugin.Settings.GameCommandShortcuts = previous;
                Logger.Error(ex, "Could not save the current game's command shortcuts.");
                return false;
            }
        }
    }
}
