using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class PolicyTemplatePersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public PolicyTemplatePersistenceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media"),
            LudusaviExecutable = Path.Combine(root, "missing-ludusavi.exe")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task InitializationSeedsBuiltInsAndCustomTemplateSurvivesRestart()
    {
        var builtIns = await store.GetPolicyTemplatesAsync(CancellationToken.None);
        Assert.Equal(5, builtIns.Count(x => x.IsBuiltIn));

        var custom = new BackupPolicyTemplateDto
        {
            TemplateId = "custom-test",
            Name = "测试模板",
            Policy = new BackupPolicyDto { DuringPlayIntervalMinutes = 999, AllowAutomaticRestore = true }
        };
        await store.UpsertPolicyTemplateAsync(custom, CancellationToken.None);

        var restarted = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restarted.InitializeAsync(CancellationToken.None);
        var templates = await restarted.GetPolicyTemplatesAsync(CancellationToken.None);
        var saved = Assert.Single(templates, x => x.TemplateId == custom.TemplateId);

        Assert.Equal(custom.Name, saved.Name);
        Assert.Equal(999, saved.Policy.DuringPlayIntervalMinutes);
        Assert.False(saved.Policy.AllowAutomaticRestore);
    }

    [Fact]
    public async Task BuiltInsCannotBeRemovedByStorageOperationAndCustomCanBeRemoved()
    {
        await store.DeletePolicyTemplateAsync(BackupPolicyTemplateCatalog.DefaultId, CancellationToken.None);
        Assert.NotNull(await store.GetPolicyTemplateAsync(BackupPolicyTemplateCatalog.DefaultId, CancellationToken.None));

        await store.UpsertPolicyTemplateAsync(new BackupPolicyTemplateDto
        {
            TemplateId = "custom-delete",
            Name = "待删除"
        }, CancellationToken.None);
        await store.DeletePolicyTemplateAsync("custom-delete", CancellationToken.None);

        Assert.Null(await store.GetPolicyTemplateAsync("custom-delete", CancellationToken.None));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
