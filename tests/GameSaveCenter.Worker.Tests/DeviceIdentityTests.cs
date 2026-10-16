using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using Microsoft.Extensions.Configuration;
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

    [Fact]
    public void CloudQueuePauseAndWindowSurviveWorkerRestartAndResume()
    {
        var root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var options = WorkerOptions.Load(CreateConfiguration(root));
            options.Apply(new WorkerSettingsDto
            {
                CloudUploadQueuePaused = true,
                CloudUploadAllowedStartMinute = 1320,
                CloudUploadAllowedEndMinute = 120
            }, persist: true);

            var restarted = WorkerOptions.Load(CreateConfiguration(root));
            Assert.True(restarted.CloudUploadQueuePaused);
            Assert.Equal(1320, restarted.CloudUploadAllowedStartMinute);
            Assert.Equal(120, restarted.CloudUploadAllowedEndMinute);

            restarted.Apply(new WorkerSettingsDto
            {
                CloudUploadQueuePaused = false,
                CloudUploadAllowedStartMinute = 60,
                CloudUploadAllowedEndMinute = 300
            }, persist: true);

            var resumed = WorkerOptions.Load(CreateConfiguration(root));
            Assert.False(resumed.CloudUploadQueuePaused);
            Assert.Equal(60, resumed.CloudUploadAllowedStartMinute);
            Assert.Equal(300, resumed.CloudUploadAllowedEndMinute);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static IConfiguration CreateConfiguration(string root)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GameSaveCenter:DataDirectory"] = root,
                ["GameSaveCenter:LudusaviBackupDirectory"] = Path.Combine(root, "Saves"),
                ["GameSaveCenter:MediaArchiveDirectory"] = Path.Combine(root, "Media")
            })
            .Build();
}
