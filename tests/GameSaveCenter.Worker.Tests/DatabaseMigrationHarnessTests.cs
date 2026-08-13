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
CREATE TABLE game_policies(playnite_id TEXT PRIMARY KEY,policy_json TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE backup_policy_templates(template_id TEXT PRIMARY KEY,name TEXT NOT NULL,is_built_in INTEGER NOT NULL,policy_json TEXT NOT NULL,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE sessions(session_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,source INTEGER NOT NULL,process_id INTEGER,process_name TEXT,launch_profile TEXT,started_utc TEXT NOT NULL,stopped_utc TEXT,elapsed_seconds INTEGER DEFAULT 0);
CREATE TABLE device_conflict_decisions(playnite_id TEXT NOT NULL,remote_device TEXT NOT NULL,local_backup_id TEXT,remote_backup_id TEXT,decision TEXT NOT NULL,comment TEXT,decided_utc TEXT NOT NULL,PRIMARY KEY(playnite_id,remote_device));
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
INSERT INTO game_policies(playnite_id,policy_json,updated_utc) VALUES ('g1','{""Enabled"":true,""BackupOnGameStop"":true}','2026-01-01T00:00:00Z');
INSERT INTO backup_policy_templates(template_id,name,is_built_in,policy_json,created_utc,updated_utc) VALUES ('important','重要游戏',1,'{""KeepRecentAllHours"":24}','2026-01-01T00:00:00Z','2026-01-01T00:00:00Z');
INSERT INTO sessions(session_id,playnite_id,source,process_id,process_name,launch_profile,started_utc) VALUES ('s1','g1',0,123,'game.exe','Default','2026-01-01T00:00:00Z');
INSERT INTO device_conflict_decisions(playnite_id,remote_device,local_backup_id,remote_backup_id,decision,comment,decided_utc) VALUES ('g1','DEVICE-A','b1','b2','KeepBoth','keep both','2026-01-01T00:00:00Z');
INSERT INTO media(media_id,playnite_id,kind,source,archive_path,original_path,captured_utc,size_bytes,sha256,is_favorite,comment,cloud_state)
VALUES ('m1','g1',0,0,'C:\\archive\\x.png','C:\\source\\x.png','2026-01-01T00:00:00Z',1,'abc',0,'','Pending');
";

    private const string OlderSchema = @"
CREATE TABLE legacy_marker(marker TEXT NOT NULL);
CREATE TABLE tasks(task_id TEXT PRIMARY KEY,task_type TEXT NOT NULL,game_id TEXT,game_name TEXT,state INTEGER NOT NULL,progress INTEGER NOT NULL,message TEXT,created_utc TEXT NOT NULL,started_utc TEXT,finished_utc TEXT,error_code TEXT,error_message TEXT);
CREATE TABLE backup_versions(backup_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,ludusavi_name TEXT NOT NULL,created_utc TEXT NOT NULL,total_bytes INTEGER NOT NULL,file_count INTEGER NOT NULL,is_locked INTEGER NOT NULL DEFAULT 0,comment TEXT,source_device TEXT,operating_system TEXT,is_pre_restore INTEGER NOT NULL DEFAULT 0,manifest_json TEXT,parent_backup_id TEXT);
CREATE TABLE media_sources(source_id TEXT PRIMARY KEY,playnite_id TEXT,source_kind INTEGER NOT NULL,root_path TEXT NOT NULL,include_pattern TEXT,enabled INTEGER NOT NULL DEFAULT 1);
CREATE TABLE game_tools(tool_id TEXT PRIMARY KEY,playnite_id TEXT NOT NULL,tool_type INTEGER NOT NULL,source_type INTEGER NOT NULL,display_name TEXT NOT NULL,enabled INTEGER NOT NULL DEFAULT 1,auto_start INTEGER NOT NULL DEFAULT 0,launch_timing INTEGER NOT NULL DEFAULT 1,launch_delay_seconds INTEGER NOT NULL DEFAULT 8,close_on_game_exit INTEGER NOT NULL DEFAULT 0,requires_admin INTEGER NOT NULL DEFAULT 0,active_version_id TEXT,created_utc TEXT NOT NULL,updated_utc TEXT NOT NULL);
CREATE TABLE game_tool_versions(version_id TEXT PRIMARY KEY,tool_id TEXT NOT NULL REFERENCES game_tools(tool_id) ON DELETE CASCADE,version_name TEXT,entry_path TEXT NOT NULL,working_directory TEXT,arguments TEXT,source_url TEXT,file_sha256 TEXT,download_utc TEXT,created_utc TEXT NOT NULL);
";

    private const string OlderData = @"
INSERT INTO legacy_marker(marker) VALUES ('older-keep');
INSERT INTO tasks(task_id,task_type,game_id,game_name,state,progress,message,created_utc) VALUES ('t9','MediaSync','g9','Old Game',2,100,'ok','2025-01-01T00:00:00Z');
INSERT INTO backup_versions(backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,comment,source_device,operating_system,is_pre_restore,manifest_json,parent_backup_id)
VALUES ('b9','g9','Old Game','2025-01-01T00:00:00Z',99,3,1,'locked','old','Windows',0,'[]','');
INSERT INTO media_sources(source_id,playnite_id,source_kind,root_path,include_pattern,enabled) VALUES ('src9','g9',1,'C:\\Screens','*.png',1);
INSERT INTO game_tools(tool_id,playnite_id,tool_type,source_type,display_name,enabled,auto_start,launch_timing,launch_delay_seconds,close_on_game_exit,requires_admin,active_version_id,created_utc,updated_utc)
VALUES ('tool9','g9',2,0,'Utility',1,1,1,8,0,0,'v9','2025-01-01T00:00:00Z','2025-01-01T00:00:00Z');
INSERT INTO game_tool_versions(version_id,tool_id,version_name,entry_path,working_directory,arguments,source_url,file_sha256,download_utc,created_utc)
VALUES ('v9','tool9','1.0','C:\Tools\u.exe','C:\Tools','','','','2025-01-01T00:00:00Z','2025-01-01T00:00:00Z');
";

    [Fact]
    public async Task LegacyFixture_UpgradesInPlaceAndPreservesData()
    {
        using var harness = new DatabaseMigrationHarness();
        await harness.CreateLegacyFixtureAsync(LegacySchema, LegacyData, CancellationToken.None);

        var result = await harness.RunAsync(
            new[] { "games", "tasks", "backup_versions", "game_policies", "backup_policy_templates", "sessions", "device_conflict_decisions", "media", "media_sources", "game_tools", "game_tool_versions", "protection_prompt_states", "legacy_marker" },
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
        Assert.Equal(1, result.RowCounts["game_policies"]);
        Assert.Equal(5, result.RowCounts["backup_policy_templates"]);
        Assert.Equal(1, result.RowCounts["sessions"]);
        Assert.Equal(1, result.RowCounts["device_conflict_decisions"]);
        Assert.Equal(1, result.RowCounts["media"]);
        Assert.Equal("KeepBoth", harness.ReadScalar("SELECT decision FROM device_conflict_decisions WHERE playnite_id='g1';"));
        Assert.Contains("BackupOnGameStop", harness.ReadScalar("SELECT policy_json FROM game_policies WHERE playnite_id='g1';"));
        Assert.Equal("重要游戏", harness.ReadScalar("SELECT name FROM backup_policy_templates WHERE template_id='important';"));
        Assert.Equal("Default", harness.ReadScalar("SELECT launch_profile FROM sessions WHERE session_id='s1';"));
    }

    [Fact]
    public async Task OlderFixture_WithoutNewerColumns_StillUpgradesAndPreservesData()
    {
        using var harness = new DatabaseMigrationHarness();
        await harness.CreateLegacyFixtureAsync(OlderSchema, OlderData, CancellationToken.None);

        var result = await harness.RunAsync(
            new[] { "games", "tasks", "backup_versions", "media_sources", "game_tools", "game_tool_versions", "legacy_marker" },
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                ["tasks"] = new[] { "session_id" },
                ["backup_versions"] = new[] { "archive_path", "restore_readiness_json" },
                ["media_sources"] = new[] { "shared_directory" },
                ["game_tools"] = new[] { "if_already_running", "risk_category", "allow_unknown_anticheat_autostart" },
                ["game_tool_versions"] = new[] { "resolved_target_path" }
            },
            CancellationToken.None);

        Assert.True(result.FirstInitializationSucceeded, "首次迁移失败：" + result.Error);
        Assert.True(result.SecondInitializationSucceeded, "重复迁移失败：" + result.Error);
        Assert.Empty(result.MissingTables);
        Assert.Empty(result.MissingColumns);
        Assert.Equal(1, result.RowCounts["tasks"]);
        Assert.Equal(1, result.RowCounts["backup_versions"]);
        Assert.Equal(1, result.RowCounts["media_sources"]);
        Assert.Equal(1, result.RowCounts["game_tools"]);
        Assert.Equal(1, result.RowCounts["game_tool_versions"]);
        Assert.Equal("older-keep", harness.ReadScalar("SELECT marker FROM legacy_marker;"));
        Assert.Equal("MediaSync", harness.ReadScalar("SELECT task_type FROM tasks WHERE task_id='t9';"));
        Assert.Equal("1", harness.ReadScalar("SELECT is_locked FROM backup_versions WHERE backup_id='b9';"));
        Assert.Equal("*.png", harness.ReadScalar("SELECT include_pattern FROM media_sources WHERE source_id='src9';"));
        Assert.Equal("Utility", harness.ReadScalar("SELECT display_name FROM game_tools WHERE tool_id='tool9';"));
        Assert.Equal("C:\\Tools\\u.exe", harness.ReadScalar("SELECT entry_path FROM game_tool_versions WHERE version_id='v9';"));
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
