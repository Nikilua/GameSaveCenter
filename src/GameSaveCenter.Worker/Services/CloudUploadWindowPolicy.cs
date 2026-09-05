namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Evaluates an optional local-time upload window. The end is exclusive; when the
/// end precedes the start the window intentionally crosses midnight.
/// </summary>
public static class CloudUploadWindowPolicy
{
    public static bool IsAllowed(DateTime utc, int startMinute, int endMinute)
    {
        Validate(startMinute, endMinute);
        var local = utc.ToUniversalTime().ToLocalTime();
        var minute = local.Hour * 60 + local.Minute;
        if (startMinute == 0 && endMinute == 1440) return true;
        if (startMinute == endMinute) return true;
        return startMinute < endMinute
            ? minute >= startMinute && minute < endMinute
            : minute >= startMinute || minute < endMinute;
    }

    public static DateTime GetNextAllowedStartUtc(DateTime utc, int startMinute, int endMinute)
    {
        Validate(startMinute, endMinute);
        if (IsAllowed(utc, startMinute, endMinute)) return utc.ToUniversalTime();

        var local = utc.ToUniversalTime().ToLocalTime();
        var todayStart = local.Date.AddMinutes(startMinute);
        DateTime next;
        if (startMinute < endMinute)
            next = local < todayStart ? todayStart : todayStart.AddDays(1);
        else
            next = local < todayStart ? todayStart : todayStart.AddDays(1);
        return next.ToLocalTime().ToUniversalTime();
    }

    private static void Validate(int startMinute, int endMinute)
    {
        if (startMinute is < 0 or > 1439) throw new ArgumentOutOfRangeException(nameof(startMinute));
        if (endMinute is < 1 or > 1440) throw new ArgumentOutOfRangeException(nameof(endMinute));
    }
}
