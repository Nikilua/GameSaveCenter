using GameSaveCenter.Worker.Tests.Infrastructure;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class DatabaseMigrationHarnessTests
{
    private const string LegacySchema = @"
CREATE TABLE legacy_marker(marker TEXT NOT NULL);
CREATE TABLE games(playnite_id TEXT PRIMARY KEY,name TEXT NOT NULL,platform INTEGER NOT NULL,platform_game_id TEXT,install_directory TEXT,descriptor_json TEXT NOT NULL,ludusavi_name TEXT,match_confidence REAL DEFAULT 0,last_backup_utc TEXT,last_media_sync_utc TEXT,health_state TEXT DEFAULT 'Unknown',cloud_state TEXT DEFAULT 'Disabled',updated_utc TEXT NOT NULL);
CREATE TABLE tasks(task_id TEXT PRIMARY KEY,task_type TEXT NOT NULL,game_id TEXT,game_name TEXT,state INTEGER NOT NULL,progress INTEGER NOT NULL,message TEXT,created_utc TEXT NOT NULL,started_utc TEXT,finished_utc TEXT,error_code TEXT,error_message TEXT);
CREATE TABLE backup_versions(backup_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,ludusavi_name TEXT NOT NULL,created_utc TEXT NOT NULL,total_bytes INTEGER NOT NULL,file_count INTEGER NOT NULL,is_locked INTEGER NOT NULL DEFAULT 0,comment TEXT,source_device TEXT,operating_system TEXT,is_pre_restore INTEGER NOT NULL DEFAULT 0,manifest_json TEXT,parent_backup_id TEXT);
CREATE TABLE media(media_id TEXT PRIMARY KEY,playnite_id TEXT,kind INTEGER NOT NULL,source INTEGER NOT NULL,archive_path TEXT NOT NULL,original_path TEXT NOT NULL,captured_utc TEXT NOT NULL,size_bytes INTEGER NOT NULL,sha256 TEXT NOT NULL UNIQUE,is_favorite INTEGER NOT NULL DEFAULT 0,comment TEXT,cloud_state TEXT NOT NULL DEFAULT 'Pending');
CREATE TABLE media_sources(source_id TEXT PRIMARY KEY,playnite_id TEXT,source_kind INTEGER NOT NULL,root_path TEXT NOT NULL,include_pattern TEXT,enabled INTEGER NOT NULL DEFAULT 1);
CREATE TABLE game_tools(tool_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,tool_type INTEGER NOT NULL,source_type INTEGER NOT NULL,display_name TEXT NOT NULL,enabled INTEGER NOT NULL DEFAULT 1,auto_start INTEGER NOT NULL DEFAULT 0,launch_timing INTEGER NOT NULL DEFAULT 1,launch_delay_seconds INTEGER NOT NULL DEFAULT 8,close_on_game_exit INTEGER NOT NULL DEFAULT 0,requires_admin INTEGER NOT NULL DEFAULT 0,active_version_id TEXT,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE game_tool_versions(version_id TEXT PRIMARY KEY,tool_id TEXT NOT NULL REFERENCES game_tools(tool_id) ON DELETE CASCADE,version_name TEXT,entry_path TEXT NOT NULL,working_directory TEXT,arguments TEXT,source_url TEXT,file_sha256 TEXT,download_utc TEXT,created_utc TEXT NOT NULL);
CREATE TABLE protection_prompt_states(playnite_id TEXT PRIMARY KEY,updated_utc TEXT NOT NULL);
";

    private const string LegacyData = @"
INSERT INTO legacy_marker(marker) VALUES ('keep');
INSERT INTO games(playnite_id,name,platform,descriptor_json,updated_utc) VALUES ('g1','Demo',0,'{}','2026-01-01T00:00:00Z');
INSERT INTO tasks(task_id,task_type,game_id,game_name,state,progress,message,created_utc) VALUES ('t1','Backup','g1','Demo',2,100,'ok','2026-01-01T00:00:00Z');
INSERT INTO backup_versions(backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,manifest_json,parent_backup_id)
VALUES ('b1','g1','Demo','2026-01-01T00:00:00Z',10,1,0,'legacy','old','Windows',0,'[]','');
INSERT INTO media(media_id,playnite_id,kind,source,archive_path,original_path,captured_utc,size_bytes,sha256,is_favorite,comment,cloud_state)
VALUES ('m1','g1',0,0,'C:\\archive\\x.png','C:\\source\\x.png','2026-01-01T00:00:00Z',1,'abc',0,'','Pending');
";

    [Fact]
    public async Task LegacyFixture_UpgradesInPlaceAndPreservesData()
    {
        using var harness = new DatabaseMigrationHarness();
        await harness.CreateLegacyFixtureAsync(LegacySchema, LegacyData, CancellationToken.None);

        var result = await harness.RunAsync(
            new[] { "games", "tasks", "backup_versions", "media", "media_sources", "game_tools", "game_tool_versions", "protection_prompt_states", "legacy_marker" },
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                ["games"] = new[] { "match_input_hash", "last_match_attempt_utc" },
                ["tasks"] = new[] { "session_id" },
                ["backup_versions"] = new[] { "archive_path", "restore_readiness_json" },
                ["media"] = new[] { "classification_state", "classification_reason" },
                ["media_sources"] = new[] { "shared_directory" },
                ["game_tools"] = new[] { "if_already_running", "risk_category", "allow_unknown_anticheat_autostart" },
                ["game_tool_versions"] = new[] { "resolved_target_path" },
                ["protection_prompt_states"] = new[] { "state", "last_save_recognized", "last_observed_utc", "last_prompt_utc" }
            },
            CancellationToken.None);

        Assert.True(result.FirstInitializationSucceeded, "首次迁移失败：" + result.Error);
        Assert.True(result.SecondInitializationSucceeded, "重复迁移失败：" + result.Error);
        Assert.Empty(result.MissingTables);
        Assert.Empty(result.MissingColumns);
        Assert.Equal(1, result.RowCounts["legacy_marker"]);
        Assert.Equal(1, result.RowCounts["games"]);
        Assert.Equal(1, result.RowCounts["tasks"]);
        Assert.Equal(1, result.RowCounts["backup_versions"]);
        Assert.Equal(1, result.RowCounts["media"]);
    }

    [Fact]
    public async Task FreshDatabase_InitializesWithoutLegacyFixture()
    {
        using var harness = new DatabaseMigrationHarness();

        var result = await harness.RunAsync(
            new[] { "games", "tasks", "backup_versions", "media", "cloud_retry_queue" },
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                ["games"] = new[] { "match_input_hash" },
                ["backup_versions"] = new[] { "archive_path", "restore_readiness_json" }
            },
            CancellationToken.None);

        Assert.True(result.FirstInitializationSucceeded, "首次迁移失败：" + result.Error);
        Assert.True(result.SecondInitializationSucceeded, "重复迁移失败：" + result.Error);
        Assert.Empty(result.MissingTables);
        Assert.Empty(result.MissingColumns);
    }

    [Fact]
    public async Task InvalidLegacySchema_ReportsFailureClearly()
    {
        using var harness = new DatabaseMigrationHarness();
        const string invalidSchema = @"
CREATE TABLE backup_versions(playnite_id TEXT PRIMARY KEY,ludusavi_name TEXT NOT NULL);
";
        await harness.CreateLegacyFixtureAsync(invalidSchema, string.Empty, CancellationToken.None);

        var result = await harness.RunAsync(
            new[] { "backup_versions" },
            new Dictionary<string, IReadOnlyCollection<string>>(),
            CancellationToken.None);

        Assert.False(result.FirstInitializationSucceeded);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.Contains("迁移失败", result.Summary);
    }
}
