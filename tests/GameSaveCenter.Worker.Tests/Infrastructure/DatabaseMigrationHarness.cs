using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameSaveCenter.Worker.Tests.Infrastructure;

/// <summary>
/// Reusable test harness for upgrading legacy SQLite fixtures through the current
/// <see cref="SqliteStateStore"/> migration path without touching a real user database.
/// </summary>
public sealed class DatabaseMigrationHarness : IDisposable
{
    private readonly string root;
    private readonly WorkerOptions options;

    public DatabaseMigrationHarness()
    {
        root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.MigrationHarness", Guid.NewGuid().ToString("N"));
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
    }

    public string DatabasePath => options.DatabasePath;

    public async Task CreateLegacyFixtureAsync(string schemaSql, string dataSql, CancellationToken token)
    {
        await using var connection = new SqliteConnection($"Data Source={DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync(token).ConfigureAwait(false);
        var schema = connection.CreateCommand();
        schema.CommandText = schemaSql;
        await schema.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(dataSql))
        {
            var data = connection.CreateCommand();
            data.CommandText = dataSql;
            await data.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }

    public async Task<MigrationHarnessResult> RunAsync(
        IReadOnlyCollection<string> expectedTables,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> expectedColumns,
        CancellationToken token)
    {
        var result = new MigrationHarnessResult();
        try
        {
            var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
            await store.InitializeAsync(token).ConfigureAwait(false);
            result.FirstInitializationSucceeded = true;
        }
        catch (Exception ex)
        {
            result.FirstInitializationSucceeded = false;
            result.Error = ex.Message;
            result.Summary = $"首次迁移失败：{ex.GetType().Name}：{ex.Message}";
            return result;
        }

        await InspectAsync(result, expectedTables, expectedColumns, token).ConfigureAwait(false);
        try
        {
            var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
            await store.InitializeAsync(token).ConfigureAwait(false);
            result.SecondInitializationSucceeded = true;
        }
        catch (Exception ex)
        {
            result.SecondInitializationSucceeded = false;
            result.Error = ex.Message;
        }

        result.Summary = result.MissingTables.Count == 0 && result.MissingColumns.Count == 0
            ? $"升级完成：{result.RowCounts.Count} 张表已校验，重复初始化成功。"
            : $"升级完成但存在缺失：{result.MissingTables.Count} 张表、{result.MissingColumns.Count} 组列。";
        return result;
    }

    private async Task InspectAsync(
        MigrationHarnessResult result,
        IReadOnlyCollection<string> expectedTables,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> expectedColumns,
        CancellationToken token)
    {
        await using var connection = new SqliteConnection($"Data Source={DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync(token).ConfigureAwait(false);

        var tables = connection.CreateCommand();
        tables.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await tables.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
                existing.Add(reader.GetString(0));
        }
        foreach (var table in expectedTables)
        {
            if (!existing.Contains(table))
                result.MissingTables.Add(table);
        }

        foreach (var pair in expectedColumns)
        {
            if (!existing.Contains(pair.Key)) continue;
            var columns = connection.CreateCommand();
            columns.CommandText = $"PRAGMA table_info({pair.Key});";
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await columns.ExecuteReaderAsync(token).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(token).ConfigureAwait(false))
                    present.Add(reader.GetString(1));
            }
            var missing = pair.Value.Where(x => !present.Contains(x)).ToList();
            if (missing.Count > 0)
                result.MissingColumns[pair.Key] = missing;
        }

        foreach (var table in expectedTables)
        {
            var count = connection.CreateCommand();
            count.CommandText = $"SELECT COUNT(*) FROM {table};";
            var scalar = await count.ExecuteScalarAsync(token).ConfigureAwait(false);
            result.RowCounts[table] = Convert.ToInt64(scalar ?? 0);
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}

public sealed class MigrationHarnessResult
{
    public bool FirstInitializationSucceeded { get; set; }
    public bool SecondInitializationSucceeded { get; set; }
    public string? Error { get; set; }
    public List<string> MissingTables { get; } = new List<string>();
    public Dictionary<string, List<string>> MissingColumns { get; } = new Dictionary<string, List<string>>();
    public Dictionary<string, long> RowCounts { get; } = new Dictionary<string, long>();
    public string Summary { get; set; } = string.Empty;
}
