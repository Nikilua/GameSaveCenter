using System;

namespace GameSaveCenter.Contracts;

/// <summary>Formats user-visible byte quantities with the product-wide 1024-based unit policy.</summary>
public static class ByteSizeFormatter
{
    public static string Format(long bytes)
    {
        var sign = bytes < 0 ? "-" : string.Empty;
        var value = Math.Abs((double)bytes);
        return sign + FormatMagnitude(value);
    }

    public static string FormatDelta(long bytes)
    {
        if (bytes == 0) return "0 B";
        return FormatSignedDelta(bytes);
    }

    public static string FormatSignedDelta(long bytes)
    {
        var sign = bytes < 0 ? "-" : "+";
        var value = Math.Abs((double)bytes);
        return sign + FormatMagnitude(value);
    }

    private static string FormatMagnitude(double value)
    {
        if (value < 1024) return $"{value:0} B";
        if (value < 1024d * 1024) return $"{value / 1024d:0.##} KiB";
        if (value < 1024d * 1024 * 1024) return $"{value / 1024d / 1024d:0.##} MiB";
        return $"{value / 1024d / 1024d / 1024d:0.##} GiB";
    }
}
