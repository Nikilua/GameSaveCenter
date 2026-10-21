using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistCrossSessionPersistenceBehaviorTests
{
    [Fact]
    public void InstalledSettingsRoundTripKeepsWorklistAndRejectsDuplicateAfterReload()
    {
        var listId = Guid.NewGuid();
        var settings = new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\GameSaveCenter\Worker.exe",
            EnableCloudUpload = true,
            Worklists = new()
            {
                new WorklistDefinition
                {
                    Id = listId,
                    Name = "恢复后续",
                    Items = new()
                    {
                        new WorklistItemRecord
                        {
                            Kind = WorklistObjectKind.BackgroundTask,
                            StableObjectId = "task-stable-42",
                            Note = "检查 Worker 结果",
                            IsManuallyComplete = true
                        }
                    }
                }
            },
            ActiveWorklistId = listId.ToString("D")
        };

        var serialized = JsonConvert.SerializeObject(settings);
        var reloaded = JsonConvert.DeserializeObject<GameSaveCenterSettings>(serialized)!;
        var list = Assert.Single(reloaded.Worklists);
        var item = Assert.Single(list.Items);

        Assert.Equal(listId, list.Id);
        Assert.Equal("恢复后续", list.Name);
        Assert.Equal(WorklistObjectKind.BackgroundTask, item.Kind);
        Assert.Equal("task-stable-42", item.StableObjectId);
        Assert.Equal("检查 Worker 结果", item.Note);
        Assert.True(item.IsManuallyComplete);
        Assert.Equal(listId.ToString("D"), reloaded.ActiveWorklistId);
        Assert.Equal(@"C:\GameSaveCenter\Worker.exe", reloaded.WorkerExecutable);
        Assert.True(reloaded.EnableCloudUpload);

        Assert.Equal(WorklistItemAddResult.AlreadyPresent, list.TryAdd(new WorklistItemRecord
        {
            Kind = WorklistObjectKind.BackgroundTask,
            StableObjectId = " TASK-STABLE-42 "
        }));
        Assert.Single(list.Items);
        Assert.Equal("task-stable-42", item.StableObjectId);
    }

    [Fact]
    public void LegacySettingsWithoutNewWorklistFieldsLoadWithDefaultsAndStayReadable()
    {
        var listId = Guid.NewGuid();
        var json = "{\"EnableCloudUpload\":true,\"Worklists\":[{\"Id\":\""
            + listId.ToString("D")
            + "\",\"Name\":\" 旧版待办 \",\"Items\":[{\"Kind\":3,\"StableObjectId\":\"task-legacy-9\"}]}],\"ActiveWorklistId\":\""
            + listId.ToString("D")
            + "\"}";

        var upgraded = JsonConvert.DeserializeObject<GameSaveCenterSettings>(json)!;
        var list = Assert.Single(upgraded.Worklists);
        var item = Assert.Single(list.Items);

        Assert.Equal(listId, list.Id);
        Assert.Equal("旧版待办", list.Name);
        Assert.Equal("task-legacy-9", item.StableObjectId);
        Assert.Equal(string.Empty, item.Note);
        Assert.False(item.IsManuallyComplete);
        Assert.True(upgraded.EnableCloudUpload);

        var reloaded = JsonConvert.DeserializeObject<GameSaveCenterSettings>(
            JsonConvert.SerializeObject(upgraded))!;
        Assert.Equal("task-legacy-9", Assert.Single(Assert.Single(reloaded.Worklists).Items).StableObjectId);
    }

    [Fact]
    public void MalformedWorklistSettingsFallBackWithoutChangingOrLosingTheOriginalConfig()
    {
        var directory = Path.Combine(Path.GetTempPath(), "GameSaveCenterWorklistRecoveryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var settingsPath = Path.Combine(directory, "config.json");
            const string originalJson = "{\"WorkerExecutable\":\"C:\\\\GameSaveCenter\\\\Worker.exe\",\"EnableCloudUpload\":true,\"Worklists\":{\"invalid\":true}}";
            File.WriteAllText(settingsPath, originalJson);

            var result = PluginSettingsLoadRecovery.Load(
                () => JsonConvert.DeserializeObject<GameSaveCenterSettings>(File.ReadAllText(settingsPath)),
                settingsPath);

            Assert.True(result.WasRecovered);
            Assert.True(result.CanPersistRecoveredSettings);
            Assert.NotNull(result.Settings);
            Assert.Empty(result.Settings!.Worklists);
            Assert.True(File.Exists(result.BackupPath));
            Assert.Equal(originalJson, File.ReadAllText(settingsPath));
            Assert.Equal(originalJson, File.ReadAllText(result.BackupPath!));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
