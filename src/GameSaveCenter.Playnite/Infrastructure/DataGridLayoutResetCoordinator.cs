using System;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Refreshes live grids after the confirmed all-view layout reset.</summary>
    internal static class DataGridLayoutResetCoordinator
    {
        internal static event EventHandler? AllLayoutsReset;

        internal static void NotifyAllLayoutsReset()
            => AllLayoutsReset?.Invoke(null, EventArgs.Empty);
    }
}
