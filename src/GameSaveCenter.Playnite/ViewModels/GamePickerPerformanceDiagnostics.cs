using System;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Bounded diagnostics for the local game-picker filter path; not a UI state contract.</summary>
    internal sealed class GamePickerPerformanceDiagnostics
    {
        public int RefreshCount { get; internal set; }
        public int LastFilterEvaluationCount { get; internal set; }
        public int LastFilteredCount { get; internal set; }
        public double LastRefreshMilliseconds { get; internal set; }
        public string LastSearchText { get; internal set; } = string.Empty;
        public long TotalFilterEvaluationCount { get; internal set; }
        public TimeSpan SearchDebounceDelay { get; internal set; }
    }
}
