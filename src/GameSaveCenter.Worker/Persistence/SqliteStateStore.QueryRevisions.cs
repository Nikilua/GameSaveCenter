using System.Globalization;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Durable revisions used to keep offset-based views from silently skipping rows.</summary>
public sealed partial class SqliteStateStore
{
    public const string CloudTransferQueryRevision = "cloud_transfers";
    public const string ClassificationHistoryQueryRevision = "classification_history";

    private async Task EnsureQueryRevisionSchemaAsync(SqliteConnection connection, CancellationToken token)
    {
        var statements = new[]
        {
            "CREATE TABLE IF NOT EXISTS query_revisions(query_name TEXT PRIMARY KEY,revision INTEGER NOT NULL);",
            $"INSERT OR IGNORE INTO query_revisions(query_name,revision) VALUES('{CloudTransferQueryRevision}',1);",
            $"INSERT OR IGNORE INTO query_revisions(query_name,revision) VALUES('{ClassificationHistoryQueryRevision}',1);",
            RevisionTrigger("cloud_transfer_queue", "cloud_transfer_queue", CloudTransferQueryRevision),
            RevisionTrigger("cloud_retry_queue", "cloud_retry_queue", CloudTransferQueryRevision),
            RevisionTrigger("games", "games", CloudTransferQueryRevision, "name,cloud_state,playnite_id"),
            RevisionTrigger("media", "media", CloudTransferQueryRevision, "playnite_id,cloud_state,classification_state"),
            RevisionTrigger("media_classification_batches", "media_classification_batches", ClassificationHistoryQueryRevision),
            RevisionTrigger("media_classification_batch_items", "media_classification_batch_items", ClassificationHistoryQueryRevision)
        };

        foreach (var statement in statements)
        {
            var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }

    public async Task<string> GetQueryRevisionAsync(string queryName, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT revision FROM query_revisions WHERE query_name=$name LIMIT 1;";
        command.Parameters.AddWithValue("$name", queryName);
        var value = await command.ExecuteScalarAsync(token).ConfigureAwait(false);
        return Convert.ToInt64(value ?? 1L, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
    }

    private static string RevisionTrigger(string source, string table, string queryName, string? updateColumns = null)
    {
        var updateClause = updateColumns == null ? "UPDATE" : $"UPDATE OF {updateColumns}";
        return $@"
CREATE TRIGGER IF NOT EXISTS trg_query_revision_{source}_insert
AFTER INSERT ON {table}
BEGIN
    UPDATE query_revisions SET revision=revision+1 WHERE query_name='{queryName}';
END;
CREATE TRIGGER IF NOT EXISTS trg_query_revision_{source}_delete
AFTER DELETE ON {table}
BEGIN
    UPDATE query_revisions SET revision=revision+1 WHERE query_name='{queryName}';
END;
CREATE TRIGGER IF NOT EXISTS trg_query_revision_{source}_update
AFTER {updateClause} ON {table}
BEGIN
    UPDATE query_revisions SET revision=revision+1 WHERE query_name='{queryName}';
END;";
    }
}
