using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    internal static class NotificationInboxFilter
    {
        public const string AllSeverities = "全部级别";
        public const string AllTime = "全部时间";
        public const string Last24Hours = "最近24小时";
        public const string Last7Days = "最近7天";
        public const string Last30Days = "最近30天";
        public const string AllGames = "全部游戏";
        public const string CurrentGame = "当前游戏";

        public static bool Matches(
            NotificationRecordDto record,
            string? severity,
            string? timeRange,
            DateTime nowUtc,
            bool filterByGame = false,
            string? gameId = null)
        {
            if (record == null) return false;
            if (filterByGame
                && (string.IsNullOrWhiteSpace(gameId)
                    || string.IsNullOrWhiteSpace(record.GameId)
                    || !string.Equals(record.GameId, gameId, StringComparison.OrdinalIgnoreCase)))
                return false;

            var selectedSeverity = severity ?? AllSeverities;
            if (selectedSeverity != AllSeverities
                && !string.Equals(selectedSeverity, SeverityName(record.Severity), StringComparison.Ordinal))
                return false;

            var createdUtc = record.CreatedUtc.Kind == DateTimeKind.Local
                ? record.CreatedUtc.ToUniversalTime()
                : DateTime.SpecifyKind(record.CreatedUtc, DateTimeKind.Utc);
            var threshold = timeRange switch
            {
                Last24Hours => nowUtc.AddHours(-24),
                Last7Days => nowUtc.AddDays(-7),
                Last30Days => nowUtc.AddDays(-30),
                _ => DateTime.MinValue
            };
            return createdUtc >= threshold;
        }

        public static string SeverityName(NotificationSeverity severity)
            => severity switch
            {
                NotificationSeverity.Error => "错误",
                NotificationSeverity.Warning => "警告",
                NotificationSeverity.Success => "成功",
                _ => "信息"
            };
    }
}
