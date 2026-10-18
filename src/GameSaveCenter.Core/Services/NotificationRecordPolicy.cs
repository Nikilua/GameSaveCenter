using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>Normalizes local notification records and defines bounded inbox retention.</summary>
public static class NotificationRecordPolicy
{
    public const int RetentionDays = 30;
    public const int MaximumRecords = 500;
    public const int MaximumPageSize = 500;
    public const int MaximumTitleLength = 96;
    public const int MaximumSummaryLength = 320;
    public const int MaximumKeyLength = 128;

    public static IReadOnlyList<string> NormalizeNotificationIds(IEnumerable<string?>? notificationIds)
    {
        if (notificationIds == null) return Array.Empty<string>();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var normalized = new List<string>();
        foreach (var value in notificationIds)
        {
            var id = (value ?? string.Empty).Trim();
            if (id.Length == 0 || id.Length > MaximumKeyLength || id.Any(char.IsWhiteSpace) || !seen.Add(id))
                continue;

            normalized.Add(id);
            if (normalized.Count == MaximumPageSize) break;
        }

        return normalized;
    }

    public static NotificationRecordDto? Normalize(NotificationRecordDto? source)
    {
        if (source == null) return null;

        var deduplicationKey = NormalizeKey(source.DeduplicationKey);
        if (deduplicationKey.Length == 0) return null;

        var createdUtc = source.CreatedUtc == default
            ? DateTime.UtcNow
            : source.CreatedUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(source.CreatedUtc, DateTimeKind.Utc)
                : source.CreatedUtc.ToUniversalTime();

        return new NotificationRecordDto
        {
            NotificationId = NormalizeIdentifier(source.NotificationId, required: true),
            DeduplicationKey = deduplicationKey,
            TaskId = NormalizeIdentifier(source.TaskId),
            GameId = NormalizeIdentifier(source.GameId),
            Severity = Enum.IsDefined(typeof(NotificationSeverity), source.Severity)
                ? source.Severity
                : NotificationSeverity.Information,
            Title = NormalizeText(source.Title, MaximumTitleLength),
            Summary = NormalizeText(source.Summary, MaximumSummaryLength),
            CreatedUtc = createdUtc,
            IsRead = source.IsRead
        };
    }

    public static bool IsExpired(NotificationRecordDto record, DateTime? utcNow = null)
    {
        if (record == null) return true;
        var now = utcNow?.ToUniversalTime() ?? DateTime.UtcNow;
        var created = record.CreatedUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(record.CreatedUtc, DateTimeKind.Utc)
            : record.CreatedUtc.ToUniversalTime();
        return created < now.AddDays(-RetentionDays);
    }

    private static string NormalizeKey(string? value)
    {
        var normalized = NormalizeWhitespace(value, MaximumKeyLength + 1);
        return normalized.Length == 0 || normalized.Length > MaximumKeyLength ? string.Empty : normalized;
    }

    private static string NormalizeIdentifier(string? value, bool required = false)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Any(char.IsWhiteSpace) || normalized.Length > MaximumKeyLength)
            return required ? Guid.NewGuid().ToString("N") : string.Empty;
        return normalized.Length == 0 && required ? Guid.NewGuid().ToString("N") : normalized;
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        return NormalizeWhitespace(value, maxLength);
    }

    private static string NormalizeWhitespace(string? value, int maxLength)
    {
        var text = value ?? string.Empty;
        if (text.Length == 0 || maxLength <= 0) return string.Empty;

        var result = new StringBuilder(Math.Min(text.Length, maxLength));
        var needsSpace = false;
        foreach (var character in text)
        {
            if (char.IsWhiteSpace(character))
            {
                needsSpace = result.Length > 0;
                continue;
            }

            if (needsSpace && result.Length < maxLength)
                result.Append(' ');
            needsSpace = false;
            if (result.Length >= maxLength) break;
            result.Append(character);
        }

        return result.ToString().TrimEnd();
    }
}
