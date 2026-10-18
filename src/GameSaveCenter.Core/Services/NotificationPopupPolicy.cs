using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>Applies temporary mute and local quiet hours to non-critical notification popups.</summary>
public static class NotificationPopupPolicy
{
    public const int MinutesPerDay = 24 * 60;

    public static bool ShouldShowPopup(
        NotificationSeverity severity,
        DateTime? mutedUntilUtc,
        bool quietHoursEnabled,
        int quietHoursStartMinute,
        int quietHoursEndMinute,
        DateTime utcNow,
        DateTime localNow)
    {
        // S02-05 only quiets routine information and success. Warning and error behavior is
        // intentionally stable until the follow-up important-event policy is implemented.
        if (severity != NotificationSeverity.Information && severity != NotificationSeverity.Success)
            return true;

        var normalizedNowUtc = NormalizeUtc(utcNow);
        if (mutedUntilUtc.HasValue && NormalizeUtc(mutedUntilUtc.Value) > normalizedNowUtc)
            return false;

        return !IsWithinQuietHours(
            quietHoursEnabled,
            quietHoursStartMinute,
            quietHoursEndMinute,
            localNow);
    }

    public static bool IsWithinQuietHours(
        bool enabled,
        int startMinute,
        int endMinute,
        DateTime localNow)
    {
        if (!enabled
            || startMinute < 0 || startMinute >= MinutesPerDay
            || endMinute < 0 || endMinute >= MinutesPerDay
            || startMinute == endMinute)
        {
            return false;
        }

        var currentMinute = localNow.Hour * 60 + localNow.Minute;
        return startMinute < endMinute
            ? currentMinute >= startMinute && currentMinute < endMinute
            : currentMinute >= startMinute || currentMinute < endMinute;
    }

    public static DateTime NormalizeUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc) return value;
        if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
        // These settings are explicitly stored as UTC. Treat legacy/unspecified values as UTC
        // instead of applying the machine's current timezone a second time after a timezone change.
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public static DateTime? NormalizeUtc(DateTime? value)
        => value.HasValue ? NormalizeUtc(value.Value) : (DateTime?)null;

    public static string FormatMinuteOfDay(int minute)
    {
        if (minute < 0 || minute >= MinutesPerDay) return "--:--";
        return $"{minute / 60:00}:{minute % 60:00}";
    }
}
