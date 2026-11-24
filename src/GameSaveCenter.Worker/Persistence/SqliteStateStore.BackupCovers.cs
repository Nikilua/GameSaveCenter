using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private static async Task EnsureBackupCoverSchemaAsync(SqliteConnection connection, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS backup_covers (
    playnite_id TEXT NOT NULL,
    backup_id TEXT NOT NULL,
    media_id TEXT NOT NULL,
    PRIMARY KEY (playnite_id, backup_id),
    FOREIGN KEY (playnite_id, backup_id) REFERENCES backup_versions(playnite_id, backup_id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_backup_covers_media ON backup_covers(media_id, playnite_id, backup_id);";
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    public async Task<BackupCoverDto> GetBackupCoverAsync(string playniteId, string backupId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT c.media_id, m.playnite_id, m.kind, m.classification_state, m.archive_path
FROM backup_covers c LEFT JOIN media m ON m.media_id=c.media_id
WHERE c.playnite_id=$game AND c.backup_id=$backup;";
        command.Parameters.AddWithValue("$game", playniteId);
        command.Parameters.AddWithValue("$backup", backupId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return new BackupCoverDto();

        var cover = new BackupCoverDto { MediaId = reader.GetString(0), State = BackupCoverState.Offline };
        if (reader.IsDBNull(2)) return cover;
        if (reader.IsDBNull(1) || !string.Equals(reader.GetString(1), playniteId, StringComparison.Ordinal)
            || reader.GetInt32(2) != (int)MediaKind.Screenshot || reader.GetString(3) != "Assigned")
        {
            cover.State = BackupCoverState.Reclassified;
            return cover;
        }
        var path = reader.GetString(4);
        if (File.Exists(path))
        {
            cover.State = BackupCoverState.Available;
            cover.ArchivePath = path;
        }
        return cover;
    }

    public Task<BackupVersionPageDto> GetBackupCoversForMediaAsync(BackupCoverMediaQueryDto query, CancellationToken token)
        => string.IsNullOrWhiteSpace(query.MediaId)
            ? Task.FromResult(new BackupVersionPageDto())
            : GetBackupVersionPageAsync(query.PlayniteId, 48, query.BeforeCreatedUtcKey, query.BeforeBackupId,
                token, coverMediaId: query.MediaId, exactBackupId: query.BackupId);

    public async Task<BackupCoverDto> SetBackupCoverAsync(BackupCoverUpdateDto update, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(update.PlayniteId) || string.IsNullOrWhiteSpace(update.BackupId))
            throw new ArgumentException("请选择当前游戏的存档版本。");
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.Parameters.AddWithValue("$game", update.PlayniteId);
        command.Parameters.AddWithValue("$backup", update.BackupId);
        if (string.IsNullOrWhiteSpace(update.MediaId))
        {
            command.CommandText = "DELETE FROM backup_covers WHERE playnite_id=$game AND backup_id=$backup;";
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            return new BackupCoverDto();
        }

        command.CommandText = @"
INSERT INTO backup_covers(playnite_id, backup_id, media_id)
SELECT b.playnite_id, b.backup_id, m.media_id
FROM backup_versions b JOIN media m ON m.playnite_id=b.playnite_id
WHERE b.playnite_id=$game AND b.backup_id=$backup AND m.media_id=$media
  AND m.kind=$screenshot AND m.classification_state='Assigned'
ON CONFLICT(playnite_id, backup_id) DO UPDATE SET media_id=excluded.media_id;";
        command.Parameters.AddWithValue("$media", update.MediaId);
        command.Parameters.AddWithValue("$screenshot", (int)MediaKind.Screenshot);
        if (await command.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 0)
            throw new InvalidOperationException("存档版本或当前游戏的截图已不可用，请重新选择。");

        return await GetBackupCoverAsync(update.PlayniteId, update.BackupId, token).ConfigureAwait(false);
    }
}
