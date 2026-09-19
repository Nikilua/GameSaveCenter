using System;

namespace GameSaveCenter.Playnite.ViewModels
{
    internal static class RestoreConfirmationGuard
    {
        public static bool IsCurrent(
            string confirmedGameId,
            string confirmedBackupId,
            string? currentGameId,
            string? currentBackupId)
            => !string.IsNullOrWhiteSpace(confirmedGameId)
               && !string.IsNullOrWhiteSpace(confirmedBackupId)
               && string.Equals(confirmedGameId, currentGameId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(confirmedBackupId, currentBackupId, StringComparison.OrdinalIgnoreCase);
    }
}
