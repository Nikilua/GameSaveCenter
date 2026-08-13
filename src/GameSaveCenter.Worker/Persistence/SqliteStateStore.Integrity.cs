using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    /// <summary>
    /// Reads read-only SQLite facts for the global integrity check. The probe never
    /// modifies schema or data; file existence checks are left to the caller.
    /// </summary>
    public async Task<DatabaseIntegrityProbeDto> ProbeIntegrityAsync(IReadOnlyCollection<string> expectedTables, CancellationToken token)
    {
        var probe = new DatabaseIntegrityProbeDto { Opened = true };
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);

        var integrity = connection.CreateCommand();
        integrity.CommandText = "PRAGMA integrity_check;";
        await using (var reader = await integrity.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
                probe.IntegrityRows.Add(reader.GetString(0));
        }

        var foreignKeys = connection.CreateCommand();
        foreignKeys.CommandText = "PRAGMA foreign_key_check;";
        await using (var reader = await foreignKeys.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                var table = reader.GetString(0);
                var rowId = reader.GetInt64(1);
                var parent = reader.GetString(2);
                probe.ForeignKeyViolations.Add($"{table} (rowid {rowId}) -> {parent}");
            }
        }

        var tables = connection.CreateCommand();
        tables.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await tables.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
                existingTables.Add(reader.GetString(0));
        }
        foreach (var table in expectedTables)
        {
            if (!existingTables.Contains(table))
                probe.MissingTables.Add(table);
        }

        probe.BackupArchivePaths = await ReadStringColumnAsync(connection,
            "SELECT archive_path FROM backup_versions WHERE archive_path IS NOT NULL AND archive_path <> '';", token).ConfigureAwait(false);
        probe.GameToolEntryPaths = await ReadStringColumnAsync(connection,
            "SELECT entry_path FROM game_tool_versions WHERE entry_path IS NOT NULL AND entry_path <> '';", token).ConfigureAwait(false);
        probe.MediaArchivePaths = await ReadStringColumnAsync(connection,
            "SELECT archive_path FROM media WHERE archive_path IS NOT NULL AND archive_path <> '';", token).ConfigureAwait(false);
        return probe;
    }

    public async Task<List<(string PlayniteId, string BackupId)>> GetBackupManifestKeysAsync(CancellationToken token)
    {
        var keys = new List<(string PlayniteId, string BackupId)>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT playnite_id, backup_id FROM backup_versions ORDER BY created_utc DESC;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
            keys.Add((reader.GetString(0), reader.GetString(1)));
        return keys;
    }

    private static async Task<List<string>> ReadStringColumnAsync(SqliteConnection connection, string sql, CancellationToken token)
    {
        var output = new List<string>();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            if (!reader.IsDBNull(0))
                output.Add(reader.GetString(0));
        }
        return output;
    }
}
