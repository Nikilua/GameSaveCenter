using System;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

/// <summary>
/// xUnit 2-compatible conditional fact for tests which compare production resources
/// with the optional AcrylicFork checkout.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class AcrylicForkDesignFactAttribute : FactAttribute
{
    public AcrylicForkDesignFactAttribute(string fileName)
    {
        if (AcrylicForkDesignSource.Exists(fileName))
            return;

        if (string.Equals(Environment.GetEnvironmentVariable("GSC_REQUIRE_ACRYLICFORK_BASELINE"), "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Environment.GetEnvironmentVariable("GSC_REQUIRE_ACRYLICFORK_BASELINE"), "true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(AcrylicForkDesignSource.MissingMessage(fileName));
        }

        Skip = AcrylicForkDesignSource.MissingMessage(fileName);
    }
}
