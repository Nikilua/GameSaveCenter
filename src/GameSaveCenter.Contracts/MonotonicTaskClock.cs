using System.Diagnostics;

namespace GameSaveCenter.Contracts;

/// <summary>Small shared seam for task duration measurement independent of wall-clock changes.</summary>
public static class MonotonicTaskClock
{
    public static long Timestamp => Stopwatch.GetTimestamp();
    public static long Frequency => Stopwatch.Frequency;

    public static double SecondsSince(long startedTimestamp, long frequency)
        => SecondsBetween(startedTimestamp, Timestamp, frequency);

    public static double SecondsBetween(long startedTimestamp, long endedTimestamp, long frequency)
    {
        if (startedTimestamp <= 0 || endedTimestamp < startedTimestamp || frequency <= 0)
            return 0;
        return (endedTimestamp - startedTimestamp) / (double)frequency;
    }
}
