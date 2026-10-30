using System;
using System.Globalization;

namespace GameSaveCenter.RenderHarness;

internal static class FakeSampleClock
{
    private const string SampleUtcEnvironmentVariable = "GSC_RENDER_SAMPLE_UTC";

    static FakeSampleClock()
    {
        var configuredSample = Environment.GetEnvironmentVariable(SampleUtcEnvironmentVariable);
        IsPinned = !string.IsNullOrWhiteSpace(configuredSample);
        if (!IsPinned)
        {
            SampleUtc = DateTimeOffset.UtcNow;
            return;
        }

        if (!DateTimeOffset.TryParse(
                configuredSample,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedSample))
        {
            throw new InvalidOperationException(
                $"{SampleUtcEnvironmentVariable} must be a valid ISO-8601 timestamp; received '{configuredSample}'.");
        }

        SampleUtc = parsedSample.ToUniversalTime();
    }

    public static DateTimeOffset SampleUtc { get; }

    public static DateTime UtcNow => SampleUtc.UtcDateTime;

    public static bool IsPinned { get; }
}
