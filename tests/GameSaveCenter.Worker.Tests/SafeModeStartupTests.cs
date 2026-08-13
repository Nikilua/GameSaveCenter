using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SafeModeStartupTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;

    public SafeModeStartupTests()
    {
        options = new WorkerOptions { DataDirectory = Path.Combine(root, "Data") };
        Directory.CreateDirectory(options.DataDirectory);
    }

    [Fact]
    public void ThreeStartupFailuresRequestSafeModeAndPersistFlag()
    {
        options.RecordStartupFailure();
        options.RecordStartupFailure();
        Assert.False(options.SafeModeRequested);

        options.RecordStartupFailure();

        Assert.True(options.SafeModeRequested);
        Assert.Equal("3", File.ReadAllText(options.StartupFailureCountPath));
        var persisted = JsonSerializer.Deserialize<WorkerSettingsDto>(
            File.ReadAllText(options.RuntimeSettingsPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.True(persisted.SafeModeRequested);
    }

    [Fact]
    public void SuccessfulStartupClearsFailureCountButKeepsPendingRequest()
    {
        options.RecordStartupFailure();
        options.RecordStartupFailure();
        options.RecordStartupFailure();

        options.RecordStartupSuccess();

        Assert.False(File.Exists(options.StartupFailureCountPath));
        Assert.True(options.SafeModeRequested);
    }

    [Fact]
    public void AppliedSettingsCanClearSafeModeRequest()
    {
        options.SafeModeRequested = true;
        var settings = new WorkerSettingsDto { SafeModeRequested = false };

        options.Apply(settings, false);

        Assert.False(options.SafeModeRequested);
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); } catch { }
    }
}
