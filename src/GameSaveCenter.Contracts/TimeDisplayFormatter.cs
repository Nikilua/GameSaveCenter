using System;
using System.Globalization;

namespace GameSaveCenter.Contracts;

/// <summary>
/// Formats persisted UTC timestamps for user-facing views without using the
/// formatted text as a sort key. The reference time is injectable so boundary
/// behavior can be verified without waiting on the wall clock.
/// </summary>
public static class TimeDisplayFormatter
{
    public static string Relative(DateTime utc, DateTime nowUtc)
    {
        if (utc == DateTime.MinValue)
            return "时间未知";

        var pointUtc = NormalizeUtc(utc);
        var referenceUtc = NormalizeUtc(nowUtc);
        var delta = referenceUtc - pointUtc;

        if (delta >= TimeSpan.Zero)
        {
            if (delta < TimeSpan.FromMinutes(1))
                return "刚刚";
            if (delta < TimeSpan.FromHours(1))
                return $"{Math.Max(1, (int)delta.TotalMinutes)} 分钟前";
        }
        else
        {
            var until = pointUtc - referenceUtc;
            if (until < TimeSpan.FromMinutes(1))
                return "即将";
            if (until < TimeSpan.FromHours(1))
                return $"{Math.Max(1, (int)until.TotalMinutes)} 分钟后";
        }

        var localPoint = pointUtc.ToLocalTime();
        var localReference = referenceUtc.ToLocalTime();
        var dayDelta = (localReference.Date - localPoint.Date).Days;

        if (dayDelta == 0)
            return $"今天 {localPoint:HH:mm}";
        if (dayDelta == 1)
            return $"昨天 {localPoint:HH:mm}";
        if (dayDelta > 1 && dayDelta <= 7)
            return $"{dayDelta} 天前 {localPoint:HH:mm}";
        if (dayDelta == -1)
            return $"明天 {localPoint:HH:mm}";
        if (dayDelta < -1 && dayDelta >= -7)
            return $"{Math.Abs(dayDelta)} 天后 {localPoint:HH:mm}";

        return localPoint.Year == localReference.Year
            ? localPoint.ToString("MM-dd HH:mm", CultureInfo.InvariantCulture)
            : localPoint.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    public static string Full(DateTime utc)
    {
        if (utc == DateTime.MinValue)
            return "时间未知";

        var normalized = NormalizeUtc(utc);
        var local = normalized.ToLocalTime();
        var offset = local.ToString("zzz", CultureInfo.InvariantCulture);
        return $"{local.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} (UTC{offset}) · {RawUtc(normalized)}";
    }

    public static string RawUtc(DateTime utc)
        => utc == DateTime.MinValue
            ? "未记录 UTC 时间"
            : NormalizeUtc(utc).ToString("O", CultureInfo.InvariantCulture);

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
}
