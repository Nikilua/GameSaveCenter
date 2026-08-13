namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
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
