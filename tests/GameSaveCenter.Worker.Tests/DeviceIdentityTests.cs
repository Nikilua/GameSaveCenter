using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class DeviceIdentityTests
{
    [Fact]
    public void WorkerUsesStableOpaqueIdentityFromPersistedSettings()
    {
        var id = Guid.NewGuid().ToString("N");
        var options = new WorkerOptions();

        options.Apply(new WorkerSettingsDto { DeviceId = id });

        Assert.Equal(id, options.DeviceId);
        Assert.Equal(id, options.DeviceStorageKey);
        Assert.Equal(id, options.ToDto().DeviceId);
        Assert.False(string.Equals(Environment.MachineName, options.DeviceStorageKey, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("machine-name")]
    [InlineData("../../escape")]
    public void InvalidIncomingIdentityCannotReplaceCurrentInstallationId(string invalid)
    {
        var options = new WorkerOptions();
        var original = options.DeviceId;

        options.Apply(new WorkerSettingsDto { DeviceId = invalid });

        Assert.Equal(original, options.DeviceId);
        Assert.True(WorkerOptions.IsValidDeviceId(options.DeviceStorageKey));
    }

    [Fact]
    public void SafeModeEnabledSurvivesWorkerSettingsRoundTrip()
    {
        var options = new WorkerOptions();

        options.Apply(new WorkerSettingsDto { SafeModeEnabled = true });

        Assert.True(options.SafeModeEnabled);
        Assert.True(options.ToDto().SafeModeEnabled);

        options.Apply(new WorkerSettingsDto { SafeModeEnabled = false });
        Assert.False(options.SafeModeEnabled);
    }

    [Fact]
    public void HealthInspectionSettingsAreClampedAndRoundTrip()
    {
        var options = new WorkerOptions();

        options.Apply(new WorkerSettingsDto
        {
            HealthInspectionEnabled = false,
            HealthInspectionIntervalMinutes = 1,
            HealthInspectionStaleAfterDays = 99999
        });

        Assert.False(options.HealthInspectionEnabled);
        Assert.Equal(15, options.HealthInspectionIntervalMinutes);
        Assert.Equal(3650, options.HealthInspectionStaleAfterDays);
        Assert.False(options.ToDto().HealthInspectionEnabled);
        Assert.Equal(15, options.ToDto().HealthInspectionIntervalMinutes);
        Assert.Equal(3650, options.ToDto().HealthInspectionStaleAfterDays);
    }

    [Fact]
    public void CloudQueueWindowSettingsAreClampedAndRoundTrip()
    {
        var options = new WorkerOptions();
        options.Apply(new WorkerSettingsDto
        {
            CloudUploadQueuePaused = true,
            CloudUploadAllowedStartMinute = -5,
            CloudUploadAllowedEndMinute = 2000
        });

        Assert.True(options.CloudUploadQueuePaused);
        Assert.Equal(0, options.CloudUploadAllowedStartMinute);
        Assert.Equal(1440, options.CloudUploadAllowedEndMinute);
        Assert.True(options.ToDto().CloudUploadQueuePaused);
        Assert.Equal(0, options.ToDto().CloudUploadAllowedStartMinute);
        Assert.Equal(1440, options.ToDto().CloudUploadAllowedEndMinute);
    }
}
