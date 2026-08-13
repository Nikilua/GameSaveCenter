using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services
{
    /// <summary>
    /// Maps persisted audit entries to the curated Overview business timeline. It never
    /// exposes raw log payloads or stack traces.
    /// </summary>
    public static class ActivityTimelineMapper
    {
        private static readonly string[] FailureWords =
        {
            "失败", "异常", "错误", "停止", "failed", "error", "exception", "rejected", "中断"
        };

        private static readonly string[] WarningWords =
        {
            "警告", "跳过", "不可用", "未找到", "未配置", "warning", "skipped", "unavailable"
        };

        public static ActivityEntryDto Map(AuditLogEntryDto entry, IReadOnlyDictionary<string, string>? gameNames)
        {
            var gameId = TryGetJsonString(entry.DetailJson, "playniteId", "gameId", "PlayniteId", "GameId");
            var gameName = "全局";
            if (!string.IsNullOrWhiteSpace(gameId) && gameNames != null && gameNames.TryGetValue(gameId!, out var resolved))
                gameName = resolved ?? "全局";
            else
            {
                var embedded = TryGetJsonString(entry.DetailJson, "gameName", "GameName");
                if (!string.IsNullOrWhiteSpace(embedded)) gameName = embedded!;
            }

            var kind = MapKind(entry.Category);
            return new ActivityEntryDto
            {
                Kind = kind,
                Result = MapResult(entry.Message, kind),
                GameName = gameName,
                Summary = Normalize(entry.Message),
                CreatedUtc = entry.CreatedUtc
            };
        }

        private static string MapKind(string category)
        {
            return category switch
            {
                "Backup" or "BackupHistory" => "Backup",
                "Restore" => "Restore",
                "CloudRetry" or "RemoteBackup" => "Cloud",
                "Media" or "MediaMetadata" => "Media",
                "GameTool" => "GameTool",
                "Protection" or "LudusaviMatch" or "Detection" or "SavePathDetection" => "Health",
                "DeviceConflict" => "Conflict",
                "Integrity" or "IntegrityCheck" => "Integrity",
                "RepositoryRebuild" => "RepositoryRepair",
                _ => "Maintenance"
            };
        }

        private static string MapResult(string message, string kind)
        {
            if (ContainsAny(message, FailureWords)) return "Failed";
            if (ContainsAny(message, WarningWords)) return "Warning";
            return kind is "Backup" or "Restore" or "Cloud" or "Media" or "GameTool" or "RepositoryRepair"
                ? "Succeeded"
                : "Info";
        }

        private static bool ContainsAny(string value, IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static string Normalize(string value)
        {
            var text = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length <= 180 ? text : text.Substring(0, 180) + "…";
        }

        private static string? TryGetJsonString(string json, params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}") return null;
            foreach (var name in propertyNames)
            {
                var key = "\"" + name + "\":\"";
                var start = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (start < 0) continue;
                start += key.Length;
                var end = json.IndexOf('"', start);
                if (end > start)
                {
                    return json.Substring(start, end - start);
                }
            }
            return null;
        }
    }
}
