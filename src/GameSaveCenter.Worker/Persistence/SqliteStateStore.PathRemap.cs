namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    public async Task<List<(string Category, string OldPath, string NewPath)>> PreviewRemapPathsAsync(
        string oldRoot,
        string newRoot,
        CancellationToken token)
    {
        var result = new List<(string Category, string OldPath, string NewPath)>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var columns = new (string Category, string Table, string Column)[]
        {
            ("备份归档", "backup_versions", "archive_path"),
            ("媒体归档", "media", "archive_path"),
            ("媒体原始路径", "media", "original_path"),
            ("媒体来源", "media_sources", "root_path"),
            ("游戏工具入口", "game_tool_versions", "entry_path"),
            ("游戏工具工作目录", "game_tool_versions", "working_directory"),
            ("游戏工具解析目标", "game_tool_versions", "resolved_target_path"),
            ("存档候选", "save_candidates", "path")
        };
        foreach (var column in columns)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT {column.Column} FROM {column.Table} " +
                                  $"WHERE substr({column.Column},1,length($old)) = $old COLLATE NOCASE " +
                                  $"AND (length({column.Column})=length($old) OR substr({column.Column},length($old)+1,1) IN ('/','\\')) " +
                                  "LIMIT 500;";
            command.Parameters.AddWithValue("$old", oldRoot);
            await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                var oldPath = reader.GetString(0);
                var newPath = newRoot + oldPath.Substring(oldRoot.Length);
                result.Add((column.Category, oldPath, newPath));
            }
        }
        return result;
    }

    /// <summary>
    /// Remaps absolute path prefixes stored in SQLite when a backup/media/game-tool root
    /// moves. Only prefix matches are updated; archives and files are never moved.
    /// </summary>
    public async Task<int> RemapStoredPathsAsync(string oldRoot, string newRoot, CancellationToken token)
    {
        var affected = 0;
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var columns = new[]
        {
            "backup_versions.archive_path",
            "media.archive_path",
            "media.original_path",
            "media_sources.root_path",
            "game_tool_versions.entry_path",
            "game_tool_versions.working_directory",
            "game_tool_versions.resolved_target_path",
            "save_candidates.path"
        };
        foreach (var column in columns)
        {
            var parts = column.Split('.');
            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE {parts[0]} SET {parts[1]} = $new || substr({parts[1]}, length($old)+1) " +
                                  $"WHERE substr({parts[1]},1,length($old)) = $old COLLATE NOCASE " +
                                  $"AND (length({parts[1]})=length($old) OR substr({parts[1]},length($old)+1,1) IN ('/','\\'));";
            command.Parameters.AddWithValue("$old", oldRoot);
            command.Parameters.AddWithValue("$new", newRoot);
            affected += await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        return affected;
    }
}
