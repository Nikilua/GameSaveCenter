using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>
/// Durable local state. Large binaries remain on disk; SQLite stores identities,
/// summaries and audit history so files stay usable without this application.
/// </summary>
public sealed partial class SqliteStateStore
{
    private readonly WorkerOptions _options;
    private readonly ILogger<SqliteStateStore> _logger;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public SqliteStateStore(WorkerOptions options, ILogger<SqliteStateStore> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        Directory.CreateDirectory(_options.DataDirectory);
        Directory.CreateDirectory(_options.LogDirectory);
        Directory.CreateDirectory(_options.MediaArchiveDirectory);
        Directory.CreateDirectory(_options.LudusaviBackupDirectory);
        Directory.CreateDirectory(_options.GameToolsDirectory);
        Directory.CreateDirectory(_options.DownloadDirectory);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = Schema;
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        await EnsureBuiltInPolicyTemplatesAsync(connection, token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "media_sources", "shared_directory", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "media", "classification_state", "TEXT NOT NULL DEFAULT 'Assigned'", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "media", "classification_reason", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "games", "match_input_hash", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "games", "last_match_attempt_utc", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "game_tools", "if_already_running", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "game_tools", "risk_category", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "game_tools", "allow_unknown_anticheat_autostart", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "protection_prompt_states", "state", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "protection_prompt_states", "last_save_recognized", "INTEGER NOT NULL DEFAULT 0", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "protection_prompt_states", "last_observed_utc", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "protection_prompt_states", "last_prompt_utc", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "game_tool_versions", "resolved_target_path", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "backup_versions", "archive_path", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "backup_versions", "restore_readiness_json", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "backup_versions", "parent_backup_id", "TEXT", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "tasks", "session_id", "TEXT NOT NULL DEFAULT ''", token).ConfigureAwait(false);
        await EnsureColumnAsync(connection, "tasks", "worker_session_id", "TEXT NOT NULL DEFAULT ''", token).ConfigureAwait(false);
        var normalizeMedia = connection.CreateCommand();
        normalizeMedia.CommandText = @"
UPDATE media
SET classification_state=CASE
    WHEN COALESCE(playnite_id,'')='' THEN 'Inbox'
    ELSE 'Assigned'
END
WHERE COALESCE(classification_state,'')='' OR classification_state='Assigned';";
        await normalizeMedia.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        var mediaClassificationIndex = connection.CreateCommand();
        mediaClassificationIndex.CommandText = "CREATE INDEX IF NOT EXISTS ix_media_classification ON media(classification_state,captured_utc DESC);";
        await mediaClassificationIndex.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        await EnsureBackupVersionSchemaAsync(connection, token).ConfigureAwait(false);
    }

    public async Task<List<GameToolDto>> GetGameToolsAsync(string playniteId, CancellationToken token)
    {
        var tools = new List<GameToolDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT tool_id,playnite_id,tool_type,source_type,display_name,enabled,auto_start,
launch_timing,launch_delay_seconds,close_on_game_exit,requires_admin,if_already_running,risk_category,active_version_id,created_utc,updated_utc,allow_unknown_anticheat_autostart
FROM game_tools WHERE playnite_id=$game ORDER BY tool_type,display_name COLLATE NOCASE;";
        command.Parameters.AddWithValue("$game", playniteId);
        await using (var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                tools.Add(new GameToolDto
                {
                    ToolId=reader.GetString(0),PlayniteId=reader.GetString(1),ToolType=(GameToolType)reader.GetInt32(2),
                    SourceType=(GameToolSourceType)reader.GetInt32(3),DisplayName=reader.GetString(4),
                    Enabled=reader.GetInt32(5)==1,AutoStart=reader.GetInt32(6)==1,
                    LaunchTiming=(GameToolLaunchTiming)reader.GetInt32(7),LaunchDelaySeconds=reader.GetInt32(8),
                    CloseOnGameExit=reader.GetInt32(9)==1,RequiresAdmin=reader.GetInt32(10)==1,
                    IfAlreadyRunning=(GameToolIfAlreadyRunning)reader.GetInt32(11),RiskCategory=(GameToolRiskCategory)reader.GetInt32(12),
                    ActiveVersionId=reader.IsDBNull(13)?string.Empty:reader.GetString(13),
                    CreatedUtc=DateTime.Parse(reader.GetString(14)).ToUniversalTime(),
                    UpdatedUtc=DateTime.Parse(reader.GetString(15)).ToUniversalTime(),
                    AllowUnknownToolWithAntiCheat=reader.GetInt32(16)==1
                });
            }
        }
        foreach (var tool in tools)
        {
            var versions = connection.CreateCommand();
            versions.CommandText = @"SELECT version_id,version_name,entry_path,working_directory,arguments,source_url,
file_sha256,resolved_target_path,download_utc,created_utc FROM game_tool_versions WHERE tool_id=$tool ORDER BY created_utc DESC;";
            versions.Parameters.AddWithValue("$tool", tool.ToolId);
            await using var reader = await versions.ExecuteReaderAsync(token).ConfigureAwait(false);
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                var path = reader.GetString(2);
                tool.Versions.Add(new GameToolVersionDto
                {
                    VersionId=reader.GetString(0),ToolId=tool.ToolId,VersionName=reader.IsDBNull(1)?"":reader.GetString(1),
                    EntryPath=path,WorkingDirectory=reader.IsDBNull(3)?Path.GetDirectoryName(path)??"":reader.GetString(3),
                    Arguments=reader.IsDBNull(4)?"":reader.GetString(4),SourceUrl=reader.IsDBNull(5)?"":reader.GetString(5),
                    FileSha256=reader.IsDBNull(6)?"":reader.GetString(6),
                    ResolvedTargetPath=reader.IsDBNull(7)?"":reader.GetString(7),
                    DownloadUtc=reader.IsDBNull(8)?null:DateTime.Parse(reader.GetString(8)).ToUniversalTime(),
                    CreatedUtc=DateTime.Parse(reader.GetString(9)).ToUniversalTime(),IsAvailable=File.Exists(path)
                });
            }
        }
        return tools;
    }

    public async Task<GameToolDto?> GetGameToolAsync(string toolId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var find = connection.CreateCommand();
        find.CommandText = "SELECT playnite_id FROM game_tools WHERE tool_id=$id;";
        find.Parameters.AddWithValue("$id", toolId);
        var gameId = await find.ExecuteScalarAsync(token).ConfigureAwait(false) as string;
        if (gameId == null) return null;
        return (await GetGameToolsAsync(gameId, token).ConfigureAwait(false)).FirstOrDefault(x => x.ToolId == toolId);
    }

    public Task UpsertGameToolAsync(GameToolDto tool, GameToolVersionDto version, CancellationToken token) => ExecuteAsync(@"
INSERT INTO game_tools(tool_id,playnite_id,tool_type,source_type,display_name,enabled,auto_start,launch_timing,
launch_delay_seconds,close_on_game_exit,requires_admin,if_already_running,risk_category,allow_unknown_anticheat_autostart,active_version_id,created_utc,updated_utc)
VALUES($id,$game,$type,$source,$name,$enabled,$auto,$timing,$delay,$close,$admin,$running,$risk,$allow,$version,$created,$updated)
ON CONFLICT(tool_id) DO UPDATE SET display_name=excluded.display_name,active_version_id=excluded.active_version_id,updated_utc=excluded.updated_utc;
INSERT INTO game_tool_versions(version_id,tool_id,version_name,entry_path,working_directory,arguments,source_url,file_sha256,resolved_target_path,download_utc,created_utc)
VALUES($version,$id,$versionName,$path,$working,$arguments,$url,$hash,$resolved,$download,$created)
ON CONFLICT(version_id) DO UPDATE SET entry_path=excluded.entry_path,file_sha256=excluded.file_sha256,resolved_target_path=excluded.resolved_target_path;",
        new Dictionary<string,object?>
        {
            ["$id"]=tool.ToolId,["$game"]=tool.PlayniteId,["$type"]=(int)tool.ToolType,["$source"]=(int)tool.SourceType,
            ["$name"]=tool.DisplayName,["$enabled"]=tool.Enabled?1:0,["$auto"]=tool.AutoStart?1:0,["$timing"]=(int)tool.LaunchTiming,
            ["$delay"]=tool.LaunchDelaySeconds,["$close"]=tool.CloseOnGameExit?1:0,["$admin"]=tool.RequiresAdmin?1:0,
            ["$running"]=(int)tool.IfAlreadyRunning,["$risk"]=(int)tool.RiskCategory,
            ["$allow"]=tool.AllowUnknownToolWithAntiCheat?1:0,
            ["$version"]=version.VersionId,["$versionName"]=version.VersionName,["$path"]=version.EntryPath,
            ["$working"]=version.WorkingDirectory,["$arguments"]=version.Arguments,["$url"]=version.SourceUrl,
            ["$hash"]=version.FileSha256,["$resolved"]=version.ResolvedTargetPath,["$download"]=version.DownloadUtc?.ToString("O"),["$created"]=tool.CreatedUtc.ToString("O"),
            ["$updated"]=tool.UpdatedUtc.ToString("O")
        }, token);

    public Task UpdateGameToolAsync(UpdateGameToolRequestDto update, CancellationToken token) => ExecuteAsync(@"
UPDATE game_tools SET enabled=$enabled,auto_start=$auto,launch_timing=$timing,launch_delay_seconds=$delay,
close_on_game_exit=$close,requires_admin=$admin,if_already_running=$running,risk_category=$risk,
allow_unknown_anticheat_autostart=$allow,
display_name=CASE WHEN COALESCE($name,'')='' THEN display_name ELSE $name END,
active_version_id=CASE WHEN COALESCE($version,'')='' THEN active_version_id ELSE $version END,updated_utc=$utc
WHERE tool_id=$id;
UPDATE game_tool_versions SET working_directory=$working,arguments=$args
WHERE version_id=CASE WHEN COALESCE($version,'')='' THEN (SELECT active_version_id FROM game_tools WHERE tool_id=$id) ELSE $version END;",
        new Dictionary<string,object?>
        {
            ["$id"]=update.ToolId,["$enabled"]=update.Enabled?1:0,["$auto"]=update.AutoStart?1:0,
            ["$timing"]=(int)update.LaunchTiming,["$delay"]=Math.Clamp(update.LaunchDelaySeconds,0,300),
            ["$close"]=update.CloseOnGameExit?1:0,["$admin"]=update.RequiresAdmin?1:0,
            ["$running"]=(int)update.IfAlreadyRunning,["$risk"]=(int)update.RiskCategory,
            ["$allow"]=update.AllowUnknownToolWithAntiCheat?1:0,
            ["$name"]=update.DisplayName,["$working"]=update.WorkingDirectory,["$args"]=update.Arguments,
            ["$version"]=update.ActiveVersionId,["$utc"]=DateTime.UtcNow.ToString("O")
        }, token);

    public Task RelocateGameToolAsync(string toolId,string entryPath,string workingDirectory,string resolvedTargetPath,string fileSha256,CancellationToken token) => ExecuteAsync(@"
UPDATE game_tool_versions SET entry_path=$path,working_directory=$working,resolved_target_path=$resolved,file_sha256=$hash
WHERE version_id=(SELECT active_version_id FROM game_tools WHERE tool_id=$id);
UPDATE game_tools SET updated_utc=$utc WHERE tool_id=$id;",
        new Dictionary<string,object?>
        {
            ["$id"]=toolId,["$path"]=entryPath,["$working"]=workingDirectory,
            ["$resolved"]=resolvedTargetPath,["$hash"]=fileSha256,["$utc"]=DateTime.UtcNow.ToString("O")
        }, token);

    public Task DeleteGameToolAsync(string toolId, CancellationToken token) => ExecuteAsync(
        "DELETE FROM game_tools WHERE tool_id=$id;",
        new Dictionary<string,object?> { ["$id"]=toolId }, token);

    public Task ReplaceTrainerCatalogAsync(IEnumerable<TrainerCatalogItemDto> items, CancellationToken token)
        => ExecuteCatalogReplaceAsync(items, token);

    private async Task ExecuteCatalogReplaceAsync(IEnumerable<TrainerCatalogItemDto> items, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token).ConfigureAwait(false);
            foreach (var item in items)
            {
                var command = connection.CreateCommand();
                command.Transaction = (SqliteTransaction)transaction;
                command.CommandText = @"INSERT INTO trainer_catalog(catalog_id,title,normalized_title,page_url,game_version,option_count,last_updated_utc,last_synced_utc)
VALUES($id,$title,$normalized,$url,$version,$options,$updated,$synced)
ON CONFLICT(catalog_id) DO UPDATE SET title=excluded.title,normalized_title=excluded.normalized_title,page_url=excluded.page_url,last_synced_utc=excluded.last_synced_utc;";
                command.Parameters.AddWithValue("$id",item.CatalogId);command.Parameters.AddWithValue("$title",item.Title);
                command.Parameters.AddWithValue("$normalized",item.NormalizedTitle);command.Parameters.AddWithValue("$url",item.PageUrl);
                command.Parameters.AddWithValue("$version",item.GameVersion);command.Parameters.AddWithValue("$options",item.OptionCount);
                command.Parameters.AddWithValue("$updated",(object?)item.LastUpdatedUtc?.ToString("O")??DBNull.Value);
                command.Parameters.AddWithValue("$synced",item.LastSyncedUtc.ToString("O"));
                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
            await transaction.CommitAsync(token).ConfigureAwait(false);
        }
        finally { _writeGate.Release(); }
    }

    public async Task<List<TrainerCatalogItemDto>> SearchTrainerCatalogAsync(string query, int limit, CancellationToken token)
    {
        var result = new List<TrainerCatalogItemDto>();
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText=@"SELECT catalog_id,title,normalized_title,page_url,game_version,option_count,last_updated_utc,last_synced_utc
FROM trainer_catalog WHERE normalized_title LIKE $query OR title LIKE $query
ORDER BY CASE WHEN normalized_title=$exact THEN 0 WHEN normalized_title LIKE $prefix THEN 1 ELSE 2 END,title LIMIT $limit;";
        var normalized = NormalizeSearch(query);
        command.Parameters.AddWithValue("$query","%"+normalized+"%");command.Parameters.AddWithValue("$exact",normalized);
        command.Parameters.AddWithValue("$prefix",normalized+"%");command.Parameters.AddWithValue("$limit",Math.Clamp(limit,1,200));
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false))result.Add(new TrainerCatalogItemDto
        {
            CatalogId=reader.GetString(0),Title=reader.GetString(1),NormalizedTitle=reader.GetString(2),PageUrl=reader.GetString(3),
            GameVersion=reader.IsDBNull(4)?"":reader.GetString(4),OptionCount=reader.IsDBNull(5)?0:reader.GetInt32(5),
            LastUpdatedUtc=reader.IsDBNull(6)?null:DateTime.Parse(reader.GetString(6)).ToUniversalTime(),
            LastSyncedUtc=DateTime.Parse(reader.GetString(7)).ToUniversalTime()
        });
        return result;
    }

    public Task ReplaceTrainerReleasesAsync(string catalogId, IEnumerable<TrainerReleaseDto> releases, CancellationToken token)
        => ExecuteReleaseReplaceAsync(catalogId,releases,token);

    private async Task ExecuteReleaseReplaceAsync(string catalogId,IEnumerable<TrainerReleaseDto> releases,CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction=await connection.BeginTransactionAsync(token).ConfigureAwait(false);
            var delete=connection.CreateCommand();delete.Transaction=(SqliteTransaction)transaction;
            delete.CommandText="DELETE FROM trainer_releases WHERE catalog_id=$id;";delete.Parameters.AddWithValue("$id",catalogId);
            await delete.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            foreach(var release in releases)
            {
                var command=connection.CreateCommand();command.Transaction=(SqliteTransaction)transaction;
                command.CommandText=@"INSERT INTO trainer_releases(release_id,catalog_id,display_name,download_url,size_bytes,published_utc,last_synced_utc)
VALUES($id,$catalog,$name,$url,$size,$published,$synced);";
                command.Parameters.AddWithValue("$id",release.ReleaseId);command.Parameters.AddWithValue("$catalog",catalogId);
                command.Parameters.AddWithValue("$name",release.DisplayName);command.Parameters.AddWithValue("$url",release.DownloadUrl);
                command.Parameters.AddWithValue("$size",release.SizeBytes);command.Parameters.AddWithValue("$published",(object?)release.PublishedUtc?.ToString("O")??DBNull.Value);
                command.Parameters.AddWithValue("$synced",DateTime.UtcNow.ToString("O"));
                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
            await transaction.CommitAsync(token).ConfigureAwait(false);
        }
        finally{_writeGate.Release();}
    }

    public async Task<List<TrainerReleaseDto>> GetTrainerReleasesAsync(string catalogId,CancellationToken token)
    {
        var result=new List<TrainerReleaseDto>();await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();command.CommandText=@"SELECT release_id,catalog_id,display_name,download_url,size_bytes,published_utc
FROM trainer_releases WHERE catalog_id=$id ORDER BY COALESCE(published_utc,'') DESC;";command.Parameters.AddWithValue("$id",catalogId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false))result.Add(new TrainerReleaseDto
        {ReleaseId=reader.GetString(0),CatalogId=reader.GetString(1),DisplayName=reader.GetString(2),DownloadUrl=reader.GetString(3),
         SizeBytes=reader.GetInt64(4),PublishedUtc=reader.IsDBNull(5)?null:DateTime.Parse(reader.GetString(5)).ToUniversalTime()});
        return result;
    }

    public async Task<TrainerCatalogItemDto?> GetTrainerCatalogItemAsync(string catalogId,CancellationToken token)
    {
        await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();command.CommandText=@"SELECT catalog_id,title,normalized_title,page_url,game_version,option_count,last_updated_utc,last_synced_utc
FROM trainer_catalog WHERE catalog_id=$id;";command.Parameters.AddWithValue("$id",catalogId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);if(!await reader.ReadAsync(token).ConfigureAwait(false))return null;
        return new TrainerCatalogItemDto{CatalogId=reader.GetString(0),Title=reader.GetString(1),NormalizedTitle=reader.GetString(2),PageUrl=reader.GetString(3),
            GameVersion=reader.IsDBNull(4)?"":reader.GetString(4),OptionCount=reader.IsDBNull(5)?0:reader.GetInt32(5),
            LastUpdatedUtc=reader.IsDBNull(6)?null:DateTime.Parse(reader.GetString(6)).ToUniversalTime(),LastSyncedUtc=DateTime.Parse(reader.GetString(7)).ToUniversalTime()};
    }

    public async Task<TrainerReleaseDto?> GetTrainerReleaseAsync(string releaseId,CancellationToken token)
    {
        await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();command.CommandText=@"SELECT release_id,catalog_id,display_name,download_url,size_bytes,published_utc
FROM trainer_releases WHERE release_id=$id;";command.Parameters.AddWithValue("$id",releaseId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);if(!await reader.ReadAsync(token).ConfigureAwait(false))return null;
        return new TrainerReleaseDto{ReleaseId=reader.GetString(0),CatalogId=reader.GetString(1),DisplayName=reader.GetString(2),
            DownloadUrl=reader.GetString(3),SizeBytes=reader.GetInt64(4),PublishedUtc=reader.IsDBNull(5)?null:DateTime.Parse(reader.GetString(5)).ToUniversalTime()};
    }

    private static string NormalizeSearch(string value)
        => new string((value??string.Empty).ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static async Task EnsureColumnAsync(SqliteConnection connection, string table, string column, string definition, CancellationToken token)
    {
        var inspect = connection.CreateCommand();
        inspect.CommandText = $"PRAGMA table_info({table});";
        var found = false;
        await using (var reader = await inspect.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
        }
        if (found) return;
        var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";
        await alter.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    private static async Task EnsureBackupVersionSchemaAsync(SqliteConnection connection, CancellationToken token)
    {
        var inspect = connection.CreateCommand();
        inspect.CommandText = "PRAGMA table_info(backup_versions);";
        var backupIdPrimary = false;
        var playniteIdPrimary = false;
        var hasArchivePath = false;
        var hasReadinessJson = false;
        await using (var reader = await inspect.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                var name = reader.GetString(1);
                var primaryOrder = reader.GetInt32(5);
                if (string.Equals(name, "backup_id", StringComparison.OrdinalIgnoreCase)) backupIdPrimary = primaryOrder > 0;
                if (string.Equals(name, "playnite_id", StringComparison.OrdinalIgnoreCase)) playniteIdPrimary = primaryOrder > 0;
                if (string.Equals(name, "archive_path", StringComparison.OrdinalIgnoreCase)) hasArchivePath = true;
                if (string.Equals(name, "restore_readiness_json", StringComparison.OrdinalIgnoreCase)) hasReadinessJson = true;
            }
        }
        if (backupIdPrimary && playniteIdPrimary) return;

        var archivePathSelect = hasArchivePath ? "archive_path" : "NULL";
        var readinessSelect = hasReadinessJson ? "restore_readiness_json" : "NULL";

        await using var transaction = await connection.BeginTransactionAsync(token).ConfigureAwait(false);
        var migrate = connection.CreateCommand();
        migrate.Transaction = (SqliteTransaction)transaction;
        migrate.CommandText = $@"
DROP TABLE IF EXISTS backup_versions_v2;
CREATE TABLE backup_versions_v2(
    backup_id TEXT NOT NULL,playnite_id TEXT NOT NULL,ludusavi_name TEXT NOT NULL,created_utc TEXT NOT NULL,
    total_bytes INTEGER NOT NULL,file_count INTEGER NOT NULL,is_locked INTEGER NOT NULL DEFAULT 0,comment TEXT,
    source_device TEXT,operating_system TEXT,is_pre_restore INTEGER NOT NULL DEFAULT 0,manifest_json TEXT,archive_path TEXT,restore_readiness_json TEXT,parent_backup_id TEXT,
    PRIMARY KEY(playnite_id,backup_id));
INSERT OR REPLACE INTO backup_versions_v2(backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,manifest_json,archive_path,restore_readiness_json,parent_backup_id)
SELECT backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,manifest_json,{archivePathSelect},{readinessSelect},parent_backup_id FROM backup_versions;
DROP TABLE backup_versions;
ALTER TABLE backup_versions_v2 RENAME TO backup_versions;
CREATE INDEX IF NOT EXISTS ix_backup_versions_game_time ON backup_versions(playnite_id,created_utc DESC);";
        await migrate.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        await transaction.CommitAsync(token).ConfigureAwait(false);
    }

    public async Task UpsertGamesAsync(IEnumerable<GameDescriptorDto> games, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token).ConfigureAwait(false);
            foreach (var game in games)
            {
                var command = connection.CreateCommand();
                command.Transaction = (SqliteTransaction)transaction;
                command.CommandText = @"
INSERT INTO games(playnite_id, name, platform, platform_game_id, install_directory, descriptor_json, match_input_hash, last_match_attempt_utc, updated_utc)
VALUES($id,$name,$platform,$platformId,$install,$json,$matchHash,$matchAttemptUtc,$utc)
ON CONFLICT(playnite_id) DO UPDATE SET
 name=excluded.name, platform=excluded.platform, platform_game_id=excluded.platform_game_id,
 install_directory=excluded.install_directory, descriptor_json=excluded.descriptor_json,
 ludusavi_name=CASE WHEN COALESCE(games.match_input_hash,'')<>excluded.match_input_hash THEN '' ELSE games.ludusavi_name END,
 match_confidence=CASE WHEN COALESCE(games.match_input_hash,'')<>excluded.match_input_hash THEN 0 ELSE games.match_confidence END,
 match_input_hash=excluded.match_input_hash,
 last_match_attempt_utc=CASE WHEN COALESCE(games.match_input_hash,'')<>excluded.match_input_hash THEN NULL ELSE games.last_match_attempt_utc END,
 updated_utc=excluded.updated_utc;";
                command.Parameters.AddWithValue("$id", game.PlayniteId);
                command.Parameters.AddWithValue("$name", game.Name);
                command.Parameters.AddWithValue("$platform", (int)game.Platform);
                command.Parameters.AddWithValue("$platformId", game.PlatformGameId ?? string.Empty);
                command.Parameters.AddWithValue("$install", game.InstallDirectory ?? string.Empty);
                command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(game, _json));
                command.Parameters.AddWithValue("$matchHash", GameMatchInput.CreateHash(game));
                // A descriptor can be persisted before its background Ludusavi match runs.
                // Keep the attempt timestamp NULL until SetGameMatchAsync completes so a
                // Worker restart can safely re-queue an item that was stranded mid-refresh.
                command.Parameters.AddWithValue("$matchAttemptUtc", DBNull.Value);
                command.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
            await transaction.CommitAsync(token).ConfigureAwait(false);
        }
        finally { _writeGate.Release(); }
    }

    public async Task<List<GameDescriptorDto>> GetGamesAsync(CancellationToken token)
    {
        var result = new List<GameDescriptorDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT descriptor_json FROM games ORDER BY name COLLATE NOCASE;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            var item = JsonSerializer.Deserialize<GameDescriptorDto>(reader.GetString(0), _json);
            if (item != null) result.Add(item);
        }
        return result;
    }

    public async Task<GameDescriptorDto?> GetGameAsync(string playniteId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT descriptor_json FROM games WHERE playnite_id=$id;";
        command.Parameters.AddWithValue("$id", playniteId);
        var value = await command.ExecuteScalarAsync(token).ConfigureAwait(false) as string;
        return value == null ? null : JsonSerializer.Deserialize<GameDescriptorDto>(value, _json);
    }

    public async Task<Dictionary<string, GameMatchCacheEntry>> GetGameMatchCacheAsync(CancellationToken token)
    {
        var result = new Dictionary<string, GameMatchCacheEntry>(StringComparer.OrdinalIgnoreCase);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT playnite_id,descriptor_json,ludusavi_name,match_confidence,match_input_hash,last_match_attempt_utc
FROM games;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            var descriptor = JsonSerializer.Deserialize<GameDescriptorDto>(reader.GetString(1), _json);
            if (descriptor == null) continue;
            result[reader.GetString(0)] = new GameMatchCacheEntry
            {
                Descriptor = descriptor,
                LudusaviName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Confidence = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                MatchInputHash = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                LastMatchAttemptUtc = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)).ToUniversalTime()
            };
        }
        return result;
    }

    public async Task SetGameMatchAsync(string playniteId, string ludusaviName, double confidence, string matchInputHash, CancellationToken token)
    {
        await ExecuteAsync(@"UPDATE games
SET ludusavi_name=$name,match_confidence=$confidence,match_input_hash=$hash,last_match_attempt_utc=$utc,updated_utc=$utc
WHERE playnite_id=$id;",
            new Dictionary<string, object?> { ["$id"] = playniteId, ["$name"] = ludusaviName, ["$confidence"] = confidence, ["$hash"] = matchInputHash, ["$utc"] = DateTime.UtcNow.ToString("O") }, token).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, (string Name, double Confidence)>> GetGameMatchesAsync(CancellationToken token)
    {
        var result = new Dictionary<string, (string, double)>(StringComparer.OrdinalIgnoreCase);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT playnite_id, ludusavi_name, match_confidence FROM games;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
            result[reader.GetString(0)] = (reader.IsDBNull(1) ? string.Empty : reader.GetString(1), reader.IsDBNull(2) ? 0 : reader.GetDouble(2));
        return result;
    }

    public async Task<BackupPolicyDto> GetPolicyAsync(string playniteId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT policy_json FROM game_policies WHERE playnite_id=$id;";
        command.Parameters.AddWithValue("$id", playniteId);
        var value = await command.ExecuteScalarAsync(token).ConfigureAwait(false) as string;
        return string.IsNullOrWhiteSpace(value) ? new BackupPolicyDto() : JsonSerializer.Deserialize<BackupPolicyDto>(value, _json) ?? new BackupPolicyDto();
    }

    public async Task<List<DashboardGameRecord>> GetDashboardGameRecordsAsync(CancellationToken token)
    {
        var result = new List<DashboardGameRecord>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT g.descriptor_json,g.ludusavi_name,g.match_confidence,g.cloud_state,
       COALESCE(b.version_count,0),b.last_backup_utc,b.latest_readiness_json,
       COALESCE(bt.recent_backup_failures,0),bt.last_backup_attempt_utc,bt.latest_backup_state,
       COALESCE(m.media_count,0),m.last_media_utc,p.policy_json,
       COALESCE(f.warning_count,0),COALESCE(f.error_count,0),f.latest_title
FROM games g
LEFT JOIN (
    SELECT bv.playnite_id,COUNT(*) AS version_count,MAX(bv.created_utc) AS last_backup_utc,
           (SELECT b2.restore_readiness_json FROM backup_versions b2
            WHERE b2.playnite_id=bv.playnite_id ORDER BY b2.created_utc DESC LIMIT 1) AS latest_readiness_json
    FROM backup_versions bv GROUP BY bv.playnite_id
) b ON b.playnite_id=g.playnite_id
LEFT JOIN (
    SELECT t.game_id,MAX(t.created_utc) AS last_backup_attempt_utc,
           (SELECT t2.state FROM tasks t2 WHERE t2.game_id=t.game_id AND t2.task_type='Backup'
            ORDER BY t2.created_utc DESC LIMIT 1) AS latest_backup_state,
           SUM(CASE WHEN t.state=3 AND t.created_utc >= $backupFailureCutoff THEN 1 ELSE 0 END) AS recent_backup_failures
    FROM tasks t WHERE t.task_type='Backup' AND COALESCE(t.game_id,'')<>'' GROUP BY t.game_id
) bt ON bt.game_id=g.playnite_id
LEFT JOIN (
    SELECT f0.playnite_id,
           SUM(CASE WHEN f0.severity >= 1 THEN 1 ELSE 0 END) AS warning_count,
           SUM(CASE WHEN f0.severity >= 2 THEN 1 ELSE 0 END) AS error_count,
            (SELECT f2.title FROM findings f2 WHERE f2.playnite_id=f0.playnite_id AND f2.resolved=0 AND f2.severity >= 1
            ORDER BY f2.created_utc DESC LIMIT 1) AS latest_title
    FROM findings f0 WHERE f0.resolved=0 AND COALESCE(f0.playnite_id,'')<>'' GROUP BY f0.playnite_id
) f ON f.playnite_id=g.playnite_id
LEFT JOIN (
    SELECT playnite_id,COUNT(*) AS media_count,MAX(captured_utc) AS last_media_utc
    FROM media WHERE classification_state='Assigned' GROUP BY playnite_id
) m ON m.playnite_id=g.playnite_id
LEFT JOIN game_policies p ON p.playnite_id=g.playnite_id
ORDER BY g.name COLLATE NOCASE;";
        command.Parameters.AddWithValue("$backupFailureCutoff", DateTime.UtcNow.AddDays(-30).ToString("O"));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            var descriptor = JsonSerializer.Deserialize<GameDescriptorDto>(reader.GetString(0), _json);
            if (descriptor == null) continue;
            result.Add(new DashboardGameRecord
            {
                Descriptor = descriptor,
                LudusaviName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                MatchConfidence = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                CloudState = reader.IsDBNull(3) ? "Disabled" : reader.GetString(3),
                BackupVersionCount = reader.GetInt32(4),
                LastBackupUtc = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)).ToUniversalTime(),
                LatestRestoreReadiness = reader.IsDBNull(6) ? null : TryDeserializeRestoreReadiness(reader.GetString(6)),
                RecentBackupFailureCount = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                LastBackupAttemptUtc = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)).ToUniversalTime(),
                LastBackupTaskState = reader.IsDBNull(9) ? null : (TaskState?)reader.GetInt32(9),
                MediaCount = reader.GetInt32(10),
                LastMediaUtc = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)).ToUniversalTime(),
                Policy = reader.IsDBNull(12)
                    ? new BackupPolicyDto()
                    : JsonSerializer.Deserialize<BackupPolicyDto>(reader.GetString(12), _json) ?? new BackupPolicyDto(),
                OpenFindingWarningCount = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                OpenFindingErrorCount = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                LatestFindingTitle = reader.IsDBNull(15) ? string.Empty : reader.GetString(15)
            });
        }
        return result;
    }

    public Task SetPolicyAsync(string playniteId, BackupPolicyDto policy, CancellationToken token) => ExecuteAsync(@"
INSERT INTO game_policies(playnite_id,policy_json,updated_utc) VALUES($id,$json,$utc)
ON CONFLICT(playnite_id) DO UPDATE SET policy_json=excluded.policy_json,updated_utc=excluded.updated_utc;",
        new Dictionary<string, object?> { ["$id"] = playniteId, ["$json"] = JsonSerializer.Serialize(policy, _json), ["$utc"] = DateTime.UtcNow.ToString("O") }, token);

    public Task AddSessionAsync(GameSessionEventDto session, CancellationToken token) => ExecuteAsync(@"
INSERT INTO sessions(session_id,playnite_id,source,process_id,process_name,launch_profile,started_utc,stopped_utc,elapsed_seconds)
VALUES($session,$game,$source,$pid,$process,$profile,$started,$stopped,$elapsed)
ON CONFLICT(session_id) DO UPDATE SET stopped_utc=excluded.stopped_utc,elapsed_seconds=excluded.elapsed_seconds;",
        new Dictionary<string, object?>
        {
            ["$session"] = session.SessionId, ["$game"] = session.PlayniteId, ["$source"] = (int)session.Source,
            ["$pid"] = session.ProcessId, ["$process"] = session.ProcessName, ["$profile"] = session.LaunchProfile,
            ["$started"] = session.StartedUtc.ToString("O"), ["$stopped"] = session.StoppedUtc?.ToString("O"), ["$elapsed"] = session.ElapsedSeconds
        }, token);

    public async Task<List<GameSessionEventDto>> GetOpenSessionsAsync(CancellationToken token)
    {
        var result = new List<GameSessionEventDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT session_id,playnite_id,source,process_id,process_name,launch_profile,started_utc,elapsed_seconds FROM sessions WHERE stopped_utc IS NULL;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Add(new GameSessionEventDto
            {
                SessionId = reader.GetString(0), PlayniteId = reader.GetString(1), Source = (SessionSourceKind)reader.GetInt32(2),
                ProcessId = reader.IsDBNull(3) ? null : reader.GetInt32(3), ProcessName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                LaunchProfile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), StartedUtc = DateTime.Parse(reader.GetString(6)).ToUniversalTime(),
                ElapsedSeconds = reader.GetInt64(7)
            });
        }
        return result;
    }

    public async Task<GameSessionEventDto?> GetSessionAsync(string sessionId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT session_id,playnite_id,source,process_id,process_name,launch_profile,started_utc,stopped_utc,elapsed_seconds
FROM sessions WHERE session_id=$session LIMIT 1;";
        command.Parameters.AddWithValue("$session", sessionId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return null;
        return new GameSessionEventDto
        {
            SessionId = reader.GetString(0),
            PlayniteId = reader.GetString(1),
            Source = (SessionSourceKind)reader.GetInt32(2),
            ProcessId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            ProcessName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            LaunchProfile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            StartedUtc = DateTime.Parse(reader.GetString(6)).ToUniversalTime(),
            StoppedUtc = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)).ToUniversalTime(),
            ElapsedSeconds = reader.IsDBNull(8) ? 0 : reader.GetInt64(8)
        };
    }

    public async Task<bool> HasOverlappingGameSessionAsync(string playniteId, DateTime startUtc, DateTime stopUtc, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT COUNT(*) FROM sessions
WHERE playnite_id<>$game
  AND started_utc<=$stop
  AND (stopped_utc IS NULL OR stopped_utc>=$start);";
        command.Parameters.AddWithValue("$game", playniteId);
        command.Parameters.AddWithValue("$start", startUtc.ToUniversalTime().ToString("O"));
        command.Parameters.AddWithValue("$stop", stopUtc.ToUniversalTime().ToString("O"));
        return Convert.ToInt32(await command.ExecuteScalarAsync(token).ConfigureAwait(false)) > 0;
    }

    public async Task<int> MarkInterruptedTasksAsync(string currentWorkerSessionId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"UPDATE tasks
SET state=$failed, progress=CASE WHEN progress>99 THEN 99 ELSE progress END, message=$message,
    finished_utc=$finished,
    error_code=CASE WHEN task_type IN ('Restore') THEN 'MANUAL_INTERVENTION_REQUIRED'
                    WHEN task_type IN ('Backup','MediaSync','MediaInbox','CloudUpload') THEN 'WORKER_RESTARTED_RETRYABLE'
                    ELSE 'WORKER_RESTARTED' END,
    error_message=CASE WHEN task_type IN ('Restore') THEN 'Worker 意外退出，恢复任务已中断，需要人工介入并检查 PreRestore 快照。'
                       ELSE 'Worker 意外退出，任务中断；请确认目标文件状态后重新执行。' END
WHERE state IN ($queued,$running) AND (worker_session_id='' OR worker_session_id<>$current);";
        command.Parameters.AddWithValue("$failed", (int)TaskState.Failed);
        command.Parameters.AddWithValue("$queued", (int)TaskState.Queued);
        command.Parameters.AddWithValue("$running", (int)TaskState.Running);
        command.Parameters.AddWithValue("$message", "Worker 重启前任务未完成");
        command.Parameters.AddWithValue("$finished", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$current", currentWorkerSessionId ?? string.Empty);
        return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    public Task AddOrUpdateTaskAsync(TaskStatusDto task, CancellationToken token) => ExecuteAsync(@"
INSERT INTO tasks(task_id,session_id,worker_session_id,task_type,game_id,game_name,state,progress,message,created_utc,started_utc,finished_utc,error_code,error_message)
VALUES($id,$session,$worker,$type,$game,$name,$state,$progress,$message,$created,$started,$finished,$errorCode,$errorMessage)
ON CONFLICT(task_id) DO UPDATE SET state=excluded.state,progress=excluded.progress,message=excluded.message,worker_session_id=excluded.worker_session_id,
 started_utc=excluded.started_utc,finished_utc=excluded.finished_utc,error_code=excluded.error_code,error_message=excluded.error_message;",
        new Dictionary<string, object?>
        {
            ["$id"] = task.TaskId, ["$session"] = task.SessionId, ["$worker"] = task.WorkerSessionId, ["$type"] = task.TaskType, ["$game"] = task.GameId, ["$name"] = task.GameName,
            ["$state"] = (int)task.State, ["$progress"] = task.ProgressPercent, ["$message"] = task.Message,
            ["$created"] = task.CreatedUtc.ToString("O"), ["$started"] = task.StartedUtc?.ToString("O"), ["$finished"] = task.FinishedUtc?.ToString("O"),
            ["$errorCode"] = task.ErrorCode, ["$errorMessage"] = task.ErrorMessage
        }, token);

    public async Task<List<TaskStatusDto>> GetRecentTasksAsync(int limit, CancellationToken token)
    {
        var result = new List<TaskStatusDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT task_id,session_id,worker_session_id,task_type,game_id,game_name,state,progress,message,created_utc,started_utc,finished_utc,error_code,error_message FROM tasks ORDER BY created_utc DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 500));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Add(new TaskStatusDto
            {
                TaskId=reader.GetString(0), SessionId=reader.IsDBNull(1)?string.Empty:reader.GetString(1), WorkerSessionId=reader.IsDBNull(2)?string.Empty:reader.GetString(2),
                TaskType=reader.GetString(3), GameId=reader.IsDBNull(4)?string.Empty:reader.GetString(4),
                GameName=reader.IsDBNull(5)?string.Empty:reader.GetString(5), State=(TaskState)reader.GetInt32(6), ProgressPercent=reader.GetInt32(7),
                Message=reader.IsDBNull(8)?string.Empty:reader.GetString(8), CreatedUtc=DateTime.Parse(reader.GetString(9)).ToUniversalTime(),
                StartedUtc=reader.IsDBNull(10)?null:DateTime.Parse(reader.GetString(10)).ToUniversalTime(),
                FinishedUtc=reader.IsDBNull(11)?null:DateTime.Parse(reader.GetString(11)).ToUniversalTime(),
                ErrorCode=reader.IsDBNull(12)?string.Empty:reader.GetString(12), ErrorMessage=reader.IsDBNull(13)?string.Empty:reader.GetString(13)
            });
        }
        return result;
    }

    public Task AddFindingAsync(string playniteId, ValidationFindingDto finding, CancellationToken token) => ExecuteAsync(@"
INSERT INTO findings(finding_id,playnite_id,severity,code,title,detail,suggested_action,created_utc,resolved)
VALUES($id,$game,$severity,$code,$title,$detail,$action,$utc,0);",
        new Dictionary<string, object?> { ["$id"] = Guid.NewGuid().ToString("N"), ["$game"] = playniteId, ["$severity"] = (int)finding.Severity,
            ["$code"] = finding.Code, ["$title"] = finding.Title, ["$detail"] = finding.Detail, ["$action"] = finding.SuggestedAction, ["$utc"] = DateTime.UtcNow.ToString("O") }, token);

    public async Task<List<ValidationFindingDto>> GetOpenFindingsAsync(int limit, CancellationToken token)
    {
        var result = new List<ValidationFindingDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT playnite_id,severity,code,title,detail,suggested_action FROM findings WHERE resolved=0 ORDER BY created_utc DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 500));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new ValidationFindingDto
        {
            PlayniteId=reader.IsDBNull(0)?string.Empty:reader.GetString(0), Severity=(FindingSeverity)reader.GetInt32(1), Code=reader.GetString(2), Title=reader.GetString(3), Detail=reader.IsDBNull(4)?string.Empty:reader.GetString(4), SuggestedAction=reader.IsDBNull(5)?string.Empty:reader.GetString(5)
        });
        return result;
    }

    public Task AddBackupVersionAsync(BackupVersionDto version, string manifestJson, CancellationToken token) => ExecuteAsync(@"
INSERT INTO backup_versions(backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,manifest_json,archive_path,restore_readiness_json,parent_backup_id)
VALUES($id,$game,$ludusavi,$created,$bytes,$count,$locked,$comment,$device,$os,$pre,$manifest,$archive,$readiness,$parent)
ON CONFLICT(playnite_id,backup_id) DO UPDATE SET ludusavi_name=excluded.ludusavi_name,created_utc=excluded.created_utc,total_bytes=CASE WHEN excluded.total_bytes=0 AND backup_versions.total_bytes>0 THEN backup_versions.total_bytes ELSE excluded.total_bytes END,file_count=CASE WHEN excluded.file_count=0 AND backup_versions.file_count>0 THEN backup_versions.file_count ELSE excluded.file_count END,is_locked=excluded.is_locked,comment=excluded.comment,source_device=excluded.source_device,operating_system=excluded.operating_system,is_pre_restore=excluded.is_pre_restore,manifest_json=CASE WHEN excluded.manifest_json='{}' THEN backup_versions.manifest_json ELSE excluded.manifest_json END,archive_path=CASE WHEN COALESCE(excluded.archive_path,'')='' THEN backup_versions.archive_path ELSE excluded.archive_path END,restore_readiness_json=CASE WHEN excluded.restore_readiness_json='{}' THEN backup_versions.restore_readiness_json ELSE excluded.restore_readiness_json END,parent_backup_id=CASE WHEN COALESCE(excluded.parent_backup_id,'')='' THEN backup_versions.parent_backup_id ELSE excluded.parent_backup_id END;",
        new Dictionary<string, object?> { ["$id"]=version.BackupId,["$game"]=version.PlayniteId,["$ludusavi"]=version.LudusaviName,["$created"]=version.CreatedUtc.ToString("O"),
            ["$bytes"]=version.TotalBytes,["$count"]=version.FileCount,["$locked"]=version.IsLocked?1:0,["$comment"]=version.Comment,["$device"]=version.SourceDevice,
            ["$os"]=version.OperatingSystem,["$pre"]=version.IsPreRestore?1:0,["$manifest"]=manifestJson,["$archive"]=version.ArchivePath,["$parent"]=version.ParentBackupId,
            ["$readiness"]=version.RestoreReadiness == null ? "{}" : JsonSerializer.Serialize(version.RestoreReadiness, _json) }, token);

    public Task SaveRestoreReadinessAsync(string playniteId, string backupId, RestoreReadinessDto readiness, CancellationToken token)
        => ExecuteAsync("UPDATE backup_versions SET restore_readiness_json=$json WHERE playnite_id=$game AND backup_id=$backup;",
            new Dictionary<string, object?> { ["$game"] = playniteId, ["$backup"] = backupId, ["$json"] = JsonSerializer.Serialize(readiness, _json) }, token);

    public async Task<List<BackupVersionDto>> GetBackupVersionsAsync(string playniteId, CancellationToken token)
    {
        var result = new List<BackupVersionDto>();
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT backup_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,archive_path,restore_readiness_json,parent_backup_id FROM backup_versions WHERE playnite_id=$id ORDER BY created_utc DESC;";
        command.Parameters.AddWithValue("$id",playniteId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new BackupVersionDto
        {
            BackupId=reader.GetString(0),PlayniteId=playniteId,LudusaviName=reader.GetString(1),CreatedUtc=DateTime.Parse(reader.GetString(2)).ToUniversalTime(),
            TotalBytes=reader.GetInt64(3),FileCount=reader.GetInt32(4),IsLocked=reader.GetInt32(5)==1,Comment=reader.IsDBNull(6)?string.Empty:reader.GetString(6),SourceDevice=reader.IsDBNull(7)?string.Empty:reader.GetString(7),
            OperatingSystem=reader.IsDBNull(8)?string.Empty:reader.GetString(8),IsPreRestore=reader.GetInt32(9)==1,
            ArchivePath=reader.IsDBNull(10)?string.Empty:reader.GetString(10),
            RestoreReadiness=reader.IsDBNull(11)||string.IsNullOrWhiteSpace(reader.GetString(11))||reader.GetString(11)=="{}"
                ? null : TryDeserializeRestoreReadiness(reader.GetString(11))
            ,ParentBackupId=reader.IsDBNull(12)?string.Empty:reader.GetString(12)
        });
        return result;
    }

    /// <summary>Returns indexed backup rows needed for read-only storage analysis.</summary>
    public async Task<List<BackupVersionDto>> GetStorageAnalysisRowsAsync(CancellationToken token)
    {
        var result = new List<BackupVersionDto>();
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT playnite_id,ludusavi_name,created_utc,total_bytes FROM backup_versions ORDER BY created_utc ASC;";
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new BackupVersionDto
        {
            PlayniteId=reader.GetString(0),LudusaviName=reader.GetString(1),CreatedUtc=DateTime.Parse(reader.GetString(2)).ToUniversalTime(),TotalBytes=reader.GetInt64(3)
        });
        return result;
    }

    /// <summary>Returns one newest indexed backup per game for a content-free device sidecar.</summary>
    public async Task<List<DeviceBackupSummaryDto>> GetLatestBackupSummariesAsync(CancellationToken token)
    {
        var result = new List<DeviceBackupSummaryDto>();
        await using var connection = Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT b.playnite_id,g.name,b.backup_id,b.created_utc,b.total_bytes,b.file_count,b.parent_backup_id,b.manifest_json
FROM backup_versions b
JOIN games g ON g.playnite_id=b.playnite_id
JOIN (SELECT playnite_id,MAX(created_utc) AS newest FROM backup_versions GROUP BY playnite_id) latest
  ON latest.playnite_id=b.playnite_id AND latest.newest=b.created_utc
ORDER BY g.name COLLATE NOCASE;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new DeviceBackupSummaryDto
        {
            PlayniteId=reader.GetString(0),GameName=reader.GetString(1),BackupId=reader.GetString(2),
            CreatedUtc=DateTime.Parse(reader.GetString(3)).ToUniversalTime(),TotalBytes=reader.GetInt64(4),FileCount=reader.GetInt32(5),ParentBackupId=reader.IsDBNull(6)?string.Empty:reader.GetString(6),
            ContentFingerprint=ComputeContentFingerprint(reader.IsDBNull(7)?string.Empty:reader.GetString(7))
        });
        return result;
    }

    private static string ComputeContentFingerprint(string manifestJson)
    {
        if (string.IsNullOrWhiteSpace(manifestJson) || manifestJson == "{}") return string.Empty;
        try
        {
            var entries = JsonSerializer.Deserialize<List<FileManifestEntry>>(manifestJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return BackupContentFingerprint.Compute(entries ?? new List<FileManifestEntry>());
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    public async Task<List<ProcessMappingDto>> GetProcessMappingsAsync(CancellationToken token)
    {
        var result=new List<ProcessMappingDto>();await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();command.CommandText="SELECT executable_name,playnite_id,game_name,enabled,created_utc FROM process_mappings ORDER BY game_name COLLATE NOCASE,executable_name COLLATE NOCASE;";
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false))result.Add(new ProcessMappingDto{ExecutableName=reader.GetString(0),PlayniteId=reader.GetString(1),GameName=reader.GetString(2),Enabled=reader.GetInt32(3)==1,CreatedUtc=DateTime.Parse(reader.GetString(4)).ToUniversalTime()});return result;
    }
    public Task UpsertProcessMappingAsync(ProcessMappingDto value,CancellationToken token)=>ExecuteAsync("INSERT INTO process_mappings(executable_name,playnite_id,game_name,enabled,created_utc) VALUES($exe,$game,$name,$enabled,$utc) ON CONFLICT(executable_name) DO UPDATE SET playnite_id=excluded.playnite_id,game_name=excluded.game_name,enabled=excluded.enabled;",new Dictionary<string,object?>{{"$exe",value.ExecutableName},{"$game",value.PlayniteId},{"$name",value.GameName},{"$enabled",value.Enabled?1:0},{"$utc",value.CreatedUtc.ToString("O")}},token);
    public Task DeleteProcessMappingAsync(string executableName,CancellationToken token)=>ExecuteAsync("DELETE FROM process_mappings WHERE executable_name=$exe;",new Dictionary<string,object?>{{"$exe",executableName}},token);

    public async Task RemoveMissingBackupVersionsAsync(string playniteId, IReadOnlyCollection<string> activeBackupIds, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            if (activeBackupIds.Count == 0)
            {
                command.CommandText = "DELETE FROM backup_versions WHERE playnite_id=$game;";
                command.Parameters.AddWithValue("$game", playniteId);
            }
            else
            {
                var parameterNames = activeBackupIds.Select((_, index) => $"$id{index}").ToArray();
                command.CommandText = $"DELETE FROM backup_versions WHERE playnite_id=$game AND backup_id NOT IN ({string.Join(",", parameterNames)});";
                command.Parameters.AddWithValue("$game", playniteId);
                var index = 0;
                foreach (var id in activeBackupIds) command.Parameters.AddWithValue($"$id{index++}", id);
            }
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        finally { _writeGate.Release(); }
    }

    public async Task<string> GetBackupManifestAsync(string playniteId,string backupId,CancellationToken token)
    {
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT manifest_json FROM backup_versions WHERE playnite_id=$game AND backup_id=$backup;";
        command.Parameters.AddWithValue("$game",playniteId);command.Parameters.AddWithValue("$backup",backupId);
        return await command.ExecuteScalarAsync(token).ConfigureAwait(false) as string ?? "[]";
    }

    private RestoreReadinessDto? TryDeserializeRestoreReadiness(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}") return null;
        try { return JsonSerializer.Deserialize<RestoreReadinessDto>(json, _json); }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not deserialize restore readiness for a backup version");
            return null;
        }
    }

    public Task AddSaveCandidateAsync(string playniteId, string path, double score, string reasonsJson, CancellationToken token) => ExecuteAsync(@"
UPDATE save_candidates
SET score=$score,reasons_json=$reasons,status='Pending',created_utc=$utc
WHERE candidate_id=(
    SELECT candidate_id FROM save_candidates
    WHERE playnite_id=$game AND path=$path AND status<>'Accepted'
    ORDER BY created_utc DESC LIMIT 1
);
INSERT INTO save_candidates(candidate_id,playnite_id,path,score,reasons_json,status,created_utc)
SELECT $id,$game,$path,$score,$reasons,'Pending',$utc
WHERE changes()=0
  AND NOT EXISTS(
      SELECT 1 FROM save_candidates
      WHERE playnite_id=$game AND path=$path AND status='Accepted'
  );",
        new Dictionary<string, object?> { ["$id"]=Guid.NewGuid().ToString("N"),["$game"]=playniteId,["$path"]=path,["$score"]=score,["$reasons"]=reasonsJson,["$utc"]=DateTime.UtcNow.ToString("O")},token);

    public Task UpdateGameCloudStateAsync(string playniteId,string state,CancellationToken token)=>ExecuteAsync("UPDATE games SET cloud_state=$state,updated_utc=$utc WHERE playnite_id=$game;",new Dictionary<string,object?>{{"$game",playniteId},{"$state",state},{"$utc",DateTime.UtcNow.ToString("O")}},token);

    public async Task<List<SavePathCandidateDto>> GetSaveCandidatesAsync(string playniteId, CancellationToken token)
    {
        var result=new List<SavePathCandidateDto>();
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText=@"SELECT s.path,s.score,s.reasons_json,s.status
FROM save_candidates s
WHERE s.playnite_id=$game
  AND s.candidate_id=(
      SELECT x.candidate_id FROM save_candidates x
      WHERE x.playnite_id=s.playnite_id AND x.path=s.path
      ORDER BY CASE x.status WHEN 'Accepted' THEN 0 WHEN 'Pending' THEN 1 ELSE 2 END,
               x.created_utc DESC
      LIMIT 1
  )
ORDER BY CASE s.status WHEN 'Pending' THEN 0 WHEN 'Accepted' THEN 1 ELSE 2 END,
         s.score DESC
LIMIT 100;";
        command.Parameters.AddWithValue("$game",playniteId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new SavePathCandidateDto
        {
            PlayniteId=playniteId, Path=reader.GetString(0), Score=reader.GetDouble(1),
            Reasons=JsonSerializer.Deserialize<List<string>>(reader.IsDBNull(2)?"[]":reader.GetString(2),_json)??new List<string>(),
            Status=reader.IsDBNull(3)?"Pending":reader.GetString(3)
        });
        return result;
    }

    public Task SetSaveCandidateStatusAsync(string playniteId,string path,string status,CancellationToken token) => ExecuteAsync(
        "UPDATE save_candidates SET status=$status WHERE playnite_id=$game AND path=$path;",
        new Dictionary<string, object?> { ["$game"]=playniteId,["$path"]=path,["$status"]=status },token);

    public async Task<ProtectionPromptRecord> GetProtectionPromptRecordAsync(string playniteId, CancellationToken token)
    {
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT state,last_save_recognized,last_observed_utc,last_prompt_utc,updated_utc FROM protection_prompt_states WHERE playnite_id=$game;";
        command.Parameters.AddWithValue("$game",playniteId);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if(!await reader.ReadAsync(token).ConfigureAwait(false))return new ProtectionPromptRecord();
        return new ProtectionPromptRecord
        {
            State=(ProtectionPromptState)reader.GetInt32(0),
            LastSaveRecognized=reader.GetInt32(1)==1,
            LastObservedUtc=reader.IsDBNull(2)?null:DateTime.Parse(reader.GetString(2)).ToUniversalTime(),
            LastPromptUtc=reader.IsDBNull(3)?null:DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
            UpdatedUtc=DateTime.Parse(reader.GetString(4)).ToUniversalTime()
        };
    }

    public Task SetProtectionPromptStateAsync(string playniteId, ProtectionPromptState state, CancellationToken token)
        => ExecuteAsync(@"UPDATE protection_prompt_states SET state=$state,updated_utc=$utc WHERE playnite_id=$game;
INSERT INTO protection_prompt_states(playnite_id,state,last_save_recognized,last_observed_utc,last_prompt_utc,updated_utc)
SELECT $game,$state,0,NULL,NULL,$utc
WHERE changes()=0;",
            new Dictionary<string,object?> { ["$game"]=playniteId,["$state"]=(int)state,["$utc"]=DateTime.UtcNow.ToString("O") },token);

    public Task RecordProtectionPromptObservationAsync(string playniteId, bool saveRecognized, bool promptIssued, CancellationToken token)
        => ExecuteAsync(@"INSERT INTO protection_prompt_states(playnite_id,state,last_save_recognized,last_observed_utc,last_prompt_utc,updated_utc)
VALUES($game,0,$recognized,$observed,$prompt,$utc)
ON CONFLICT(playnite_id) DO UPDATE SET
last_save_recognized=$recognized,last_observed_utc=$observed,
last_prompt_utc=CASE WHEN $prompt IS NULL THEN last_prompt_utc ELSE $prompt END,updated_utc=$utc;",
            new Dictionary<string,object?>
            {
                ["$game"]=playniteId,["$recognized"]=saveRecognized?1:0,["$observed"]=DateTime.UtcNow.ToString("O"),
                ["$prompt"]=promptIssued?DateTime.UtcNow.ToString("O"):null,["$utc"]=DateTime.UtcNow.ToString("O")
            },token);

    public async Task<(int Games,int Matched,int Media,int Unassigned)> GetCountsAsync(CancellationToken token)
    {
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        async Task<int> Scalar(string sql){var c=connection.CreateCommand();c.CommandText=sql;return Convert.ToInt32(await c.ExecuteScalarAsync(token).ConfigureAwait(false));}
        return (await Scalar("SELECT COUNT(*) FROM games;"),await Scalar("SELECT COUNT(*) FROM games WHERE COALESCE(ludusavi_name,'')<>'';"),await Scalar("SELECT COUNT(*) FROM media WHERE classification_state='Assigned';"),await Scalar("SELECT COUNT(*) FROM media WHERE classification_state='Inbox';"));
    }

    public async Task<Dictionary<string,int>> GetHealthStateCountsAsync(CancellationToken token)
    {
        await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT health_state,COUNT(*) FROM games GROUP BY health_state;";
        var counts=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false))
            counts[reader.GetString(0)]=reader.GetInt32(1);
        return counts;
    }

    /// <summary>Runs a temporary-table round trip to verify the configured database is writable.</summary>
    public async Task ProbeReadWriteAsync(CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "CREATE TEMP TABLE gsc_environment_probe(value TEXT NOT NULL); INSERT INTO gsc_environment_probe(value) VALUES('ok'); SELECT value FROM gsc_environment_probe; DROP TABLE gsc_environment_probe;";
            var value = Convert.ToString(await command.ExecuteScalarAsync(token).ConfigureAwait(false));
            if (!string.Equals(value, "ok", StringComparison.Ordinal)) throw new InvalidOperationException("SQLite temporary write probe returned an unexpected value.");
            await transaction.CommitAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite read/write probe failed");
            throw;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public Task AppendAuditAsync(string category, string message, string detailJson, CancellationToken token) => ExecuteAsync(
        "INSERT INTO audit_log(audit_id,category,message,detail_json,created_utc) VALUES($id,$category,$message,$detail,$utc);",
        new Dictionary<string, object?> { ["$id"]=Guid.NewGuid().ToString("N"),["$category"]=category,["$message"]=message,["$detail"]=detailJson,["$utc"]=DateTime.UtcNow.ToString("O")},token);

    public Task SaveDeviceConflictDecisionAsync(DeviceConflictDecisionDto decision,CancellationToken token)=>ExecuteAsync(@"
INSERT INTO device_conflict_decisions(playnite_id,remote_device,local_backup_id,remote_backup_id,decision,comment,decided_utc)
VALUES($game,$device,$local,$remote,$decision,$comment,$utc)
ON CONFLICT(playnite_id,remote_device) DO UPDATE SET
local_backup_id=excluded.local_backup_id,remote_backup_id=excluded.remote_backup_id,
decision=excluded.decision,comment=excluded.comment,decided_utc=excluded.decided_utc;",
        new Dictionary<string,object?>
        {
            ["$game"]=decision.PlayniteId,["$device"]=decision.RemoteDevice,
            ["$local"]=decision.LocalBackupId,["$remote"]=decision.RemoteBackupId,
            ["$decision"]=decision.Decision,["$comment"]=decision.Comment,
            ["$utc"]=decision.DecidedUtc.ToString("O")
        },token);

    public async Task<DeviceConflictDecisionDto?> GetDeviceConflictDecisionAsync(string playniteId,string remoteDevice,CancellationToken token)
    {
        await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();command.CommandText=@"SELECT local_backup_id,remote_backup_id,decision,comment,decided_utc
FROM device_conflict_decisions WHERE playnite_id=$game AND remote_device=$device LIMIT 1;";
        command.Parameters.AddWithValue("$game",playniteId);command.Parameters.AddWithValue("$device",remoteDevice);
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if(!await reader.ReadAsync(token).ConfigureAwait(false))return null;
        return new DeviceConflictDecisionDto
        {
            PlayniteId=playniteId,RemoteDevice=remoteDevice,
            LocalBackupId=reader.GetString(0),RemoteBackupId=reader.GetString(1),
            Decision=reader.GetString(2),Comment=reader.IsDBNull(3)?string.Empty:reader.GetString(3),
            DecidedUtc=DateTime.Parse(reader.GetString(4)).ToUniversalTime()
        };
    }


    public async Task<List<AuditLogEntryDto>> GetAuditAsync(int limit, CancellationToken token)
    {
        var result=new List<AuditLogEntryDto>();
        await using var connection=Open();await connection.OpenAsync(token).ConfigureAwait(false);
        var command=connection.CreateCommand();
        command.CommandText="SELECT category,message,detail_json,created_utc FROM audit_log ORDER BY created_utc DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit",Math.Clamp(limit,1,1000));
        await using var reader=await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while(await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new AuditLogEntryDto
        {
            Category=reader.GetString(0),Message=reader.GetString(1),DetailJson=reader.IsDBNull(2)?"{}":reader.GetString(2),CreatedUtc=DateTime.Parse(reader.GetString(3)).ToUniversalTime()
        });
        return result;
    }

    private async Task ExecuteAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection=Open(); await connection.OpenAsync(token).ConfigureAwait(false);
            var command=connection.CreateCommand();command.CommandText=sql;
            foreach(var item in parameters) command.Parameters.AddWithValue(item.Key,item.Value??DBNull.Value);
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        catch(Exception ex){_logger.LogError(ex,"SQLite operation failed");throw;}
        finally{_writeGate.Release();}
    }

    private SqliteConnection Open() => new($"Data Source={_options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");

    private const string Schema = @"
PRAGMA journal_mode=WAL;
CREATE TABLE IF NOT EXISTS games(playnite_id TEXT PRIMARY KEY,name TEXT NOT NULL,platform INTEGER NOT NULL,platform_game_id TEXT,install_directory TEXT,descriptor_json TEXT NOT NULL,ludusavi_name TEXT,match_confidence REAL DEFAULT 0,match_input_hash TEXT,last_match_attempt_utc TEXT,last_backup_utc TEXT,last_media_sync_utc TEXT,health_state TEXT DEFAULT 'Unknown',cloud_state TEXT DEFAULT 'Disabled',updated_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS game_policies(playnite_id TEXT PRIMARY KEY,policy_json TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS backup_policy_templates(template_id TEXT PRIMARY KEY,name TEXT NOT NULL,is_built_in INTEGER NOT NULL,policy_json TEXT NOT NULL,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS sessions(session_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,source INTEGER NOT NULL,process_id INTEGER,process_name TEXT,launch_profile TEXT,started_utc TEXT NOT NULL,stopped_utc TEXT,elapsed_seconds INTEGER DEFAULT 0);
CREATE TABLE IF NOT EXISTS tasks(task_id TEXT PRIMARY KEY,session_id TEXT NOT NULL DEFAULT '',worker_session_id TEXT NOT NULL DEFAULT '',task_type TEXT NOT NULL,game_id TEXT,game_name TEXT,state INTEGER NOT NULL,progress INTEGER NOT NULL,message TEXT,created_utc TEXT NOT NULL,started_utc TEXT,finished_utc TEXT,error_code TEXT,error_message TEXT);
CREATE TABLE IF NOT EXISTS findings(finding_id TEXT PRIMARY KEY,playnite_id TEXT,severity INTEGER NOT NULL,code TEXT NOT NULL,title TEXT NOT NULL,detail TEXT,suggested_action TEXT,created_utc TEXT NOT NULL,resolved INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS backup_versions(backup_id TEXT NOT NULL,playnite_id TEXT NOT NULL,ludusavi_name TEXT NOT NULL,created_utc TEXT NOT NULL,total_bytes INTEGER NOT NULL,file_count INTEGER NOT NULL,is_locked INTEGER NOT NULL DEFAULT 0,comment TEXT,source_device TEXT,operating_system TEXT,is_pre_restore INTEGER NOT NULL DEFAULT 0,manifest_json TEXT,archive_path TEXT,restore_readiness_json TEXT,parent_backup_id TEXT,PRIMARY KEY(playnite_id,backup_id));
CREATE TABLE IF NOT EXISTS media(media_id TEXT PRIMARY KEY,playnite_id TEXT,kind INTEGER NOT NULL,source INTEGER NOT NULL,archive_path TEXT NOT NULL,original_path TEXT NOT NULL,captured_utc TEXT NOT NULL,size_bytes INTEGER NOT NULL,sha256 TEXT NOT NULL UNIQUE,is_favorite INTEGER NOT NULL DEFAULT 0,comment TEXT,cloud_state TEXT NOT NULL DEFAULT 'Pending',classification_state TEXT NOT NULL DEFAULT 'Assigned',classification_reason TEXT);
CREATE TABLE IF NOT EXISTS media_sources(source_id TEXT PRIMARY KEY,playnite_id TEXT,source_kind INTEGER NOT NULL,root_path TEXT NOT NULL,include_pattern TEXT,enabled INTEGER NOT NULL DEFAULT 1,shared_directory INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS save_candidates(candidate_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,path TEXT NOT NULL,score REAL NOT NULL,reasons_json TEXT,status TEXT NOT NULL,created_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS audit_log(audit_id TEXT PRIMARY KEY,category TEXT NOT NULL,message TEXT NOT NULL,detail_json TEXT,created_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS game_tools(tool_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,tool_type INTEGER NOT NULL,source_type INTEGER NOT NULL,display_name TEXT NOT NULL,enabled INTEGER NOT NULL DEFAULT 1,auto_start INTEGER NOT NULL DEFAULT 0,launch_timing INTEGER NOT NULL DEFAULT 1,launch_delay_seconds INTEGER NOT NULL DEFAULT 8,close_on_game_exit INTEGER NOT NULL DEFAULT 0,requires_admin INTEGER NOT NULL DEFAULT 0,if_already_running INTEGER NOT NULL DEFAULT 0,risk_category INTEGER NOT NULL DEFAULT 0,allow_unknown_anticheat_autostart INTEGER NOT NULL DEFAULT 0,active_version_id TEXT,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS protection_prompt_states(playnite_id TEXT PRIMARY KEY,state INTEGER NOT NULL DEFAULT 0,last_save_recognized INTEGER NOT NULL DEFAULT 0,last_observed_utc TEXT,last_prompt_utc TEXT,updated_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS game_tool_versions(version_id TEXT PRIMARY KEY,tool_id TEXT NOT NULL REFERENCES game_tools(tool_id) ON DELETE CASCADE,version_name TEXT,entry_path TEXT NOT NULL,working_directory TEXT,arguments TEXT,source_url TEXT,file_sha256 TEXT,download_utc TEXT,created_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS trainer_catalog(catalog_id TEXT PRIMARY KEY,title TEXT NOT NULL,normalized_title TEXT NOT NULL,page_url TEXT NOT NULL,game_version TEXT,option_count INTEGER NOT NULL DEFAULT 0,last_updated_utc TEXT,last_synced_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS trainer_releases(release_id TEXT PRIMARY KEY,catalog_id TEXT NOT NULL REFERENCES trainer_catalog(catalog_id) ON DELETE CASCADE,display_name TEXT NOT NULL,download_url TEXT NOT NULL,size_bytes INTEGER NOT NULL DEFAULT 0,published_utc TEXT,last_synced_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS process_mappings(executable_name TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,game_name TEXT NOT NULL,enabled INTEGER NOT NULL DEFAULT 1,created_utc TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS device_conflict_decisions(playnite_id TEXT NOT NULL,remote_device TEXT NOT NULL,local_backup_id TEXT,remote_backup_id TEXT,decision TEXT NOT NULL,comment TEXT,decided_utc TEXT NOT NULL,PRIMARY KEY(playnite_id,remote_device));
CREATE TABLE IF NOT EXISTS cloud_retry_queue(playnite_id TEXT PRIMARY KEY,attempt_count INTEGER NOT NULL,next_attempt_utc TEXT NOT NULL,last_error TEXT,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
 CREATE INDEX IF NOT EXISTS ix_tasks_created ON tasks(created_utc DESC);
 CREATE INDEX IF NOT EXISTS ix_tasks_game_type_created ON tasks(game_id,task_type,created_utc DESC);
CREATE INDEX IF NOT EXISTS ix_backup_versions_game_time ON backup_versions(playnite_id,created_utc DESC);
CREATE INDEX IF NOT EXISTS ix_media_game ON media(playnite_id,captured_utc DESC);
CREATE INDEX IF NOT EXISTS ix_sessions_open ON sessions(stopped_utc);
CREATE INDEX IF NOT EXISTS ix_save_candidates_game_path ON save_candidates(playnite_id,path,created_utc DESC);
CREATE INDEX IF NOT EXISTS ix_game_tools_game ON game_tools(playnite_id,tool_type);
CREATE INDEX IF NOT EXISTS ix_game_tool_versions_tool ON game_tool_versions(tool_id,created_utc DESC);
CREATE INDEX IF NOT EXISTS ix_trainer_catalog_title ON trainer_catalog(normalized_title);
CREATE INDEX IF NOT EXISTS ix_cloud_retry_due ON cloud_retry_queue(next_attempt_utc);
";
}

public sealed class GameMatchCacheEntry
{
    public GameDescriptorDto Descriptor { get; set; } = new();
    public string LudusaviName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string MatchInputHash { get; set; } = string.Empty;
    public DateTime? LastMatchAttemptUtc { get; set; }
}

public sealed class DashboardGameRecord
{
    public GameDescriptorDto Descriptor { get; set; } = new();
    public string LudusaviName { get; set; } = string.Empty;
    public double MatchConfidence { get; set; }
    public string CloudState { get; set; } = "Disabled";
    public int BackupVersionCount { get; set; }
    public DateTime? LastBackupUtc { get; set; }
    public RestoreReadinessDto? LatestRestoreReadiness { get; set; }
    public int RecentBackupFailureCount { get; set; }
    public DateTime? LastBackupAttemptUtc { get; set; }
    public TaskState? LastBackupTaskState { get; set; }
    public int MediaCount { get; set; }
    public DateTime? LastMediaUtc { get; set; }
    public BackupPolicyDto Policy { get; set; } = new();
    public int OpenFindingWarningCount { get; set; }
    public int OpenFindingErrorCount { get; set; }
    public string LatestFindingTitle { get; set; } = string.Empty;
}

public sealed class ProtectionPromptRecord
{
    public ProtectionPromptState State { get; set; } = ProtectionPromptState.NeverShown;
    public bool LastSaveRecognized { get; set; }
    public DateTime? LastObservedUtc { get; set; }
    public DateTime? LastPromptUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}
