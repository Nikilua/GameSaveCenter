using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>Result of reconciling tasks that were interrupted by a Worker restart.</summary>
    public sealed class TaskReconcileResultDto
    {
        public DateTime ReconcileUtc { get; set; } = DateTime.UtcNow;
        public int InterruptedTaskCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
