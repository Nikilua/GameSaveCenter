using System;

namespace GameSaveCenter.Contracts
{
    public enum NotificationSeverity
    {
        Information = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Durable, local notification metadata. Summary text is a short user-facing snapshot;
    /// task state remains authoritative in the task store.
    /// </summary>
    public sealed class NotificationRecordDto
    {
        public string NotificationId { get; set; } = Guid.NewGuid().ToString("N");
        public string DeduplicationKey { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public NotificationSeverity Severity { get; set; } = NotificationSeverity.Information;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }
}
