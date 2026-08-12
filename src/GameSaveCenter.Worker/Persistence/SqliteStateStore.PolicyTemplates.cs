using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    public async Task<List<BackupPolicyTemplateDto>> GetPolicyTemplatesAsync(CancellationToken token)
    {
        var result = new List<BackupPolicyTemplateDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT template_id,name,is_built_in,policy_json
FROM backup_policy_templates
ORDER BY is_built_in DESC,name COLLATE NOCASE,template_id;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            var policy = JsonSerializer.Deserialize<BackupPolicyDto>(reader.GetString(3), _json)
                         ?? new BackupPolicyDto();
            result.Add(new BackupPolicyTemplateDto
            {
                TemplateId = reader.GetString(0),
                Name = reader.GetString(1),
                IsBuiltIn = reader.GetInt32(2) == 1,
                Policy = BackupPolicyTemplateCatalog.ClonePolicy(policy)
            });
        }
        return result;
    }

    public async Task<BackupPolicyTemplateDto?> GetPolicyTemplateAsync(string templateId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(templateId)) return null;
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT template_id,name,is_built_in,policy_json FROM backup_policy_templates WHERE template_id=$id;";
        command.Parameters.AddWithValue("$id", templateId.Trim());
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return null;
        return new BackupPolicyTemplateDto
        {
            TemplateId = reader.GetString(0),
            Name = reader.GetString(1),
            IsBuiltIn = reader.GetInt32(2) == 1,
            Policy = BackupPolicyTemplateCatalog.ClonePolicy(JsonSerializer.Deserialize<BackupPolicyDto>(reader.GetString(3), _json))
        };
    }

    public Task UpsertPolicyTemplateAsync(BackupPolicyTemplateDto template, CancellationToken token)
    {
        var now = DateTime.UtcNow.ToString("O");
        return ExecuteAsync(@"
INSERT INTO backup_policy_templates(template_id,name,is_built_in,policy_json,created_utc,updated_utc)
VALUES($id,$name,$builtIn,$json,$utc,$utc)
ON CONFLICT(template_id) DO UPDATE SET name=excluded.name,is_built_in=excluded.is_built_in,
policy_json=excluded.policy_json,updated_utc=excluded.updated_utc;",
            new Dictionary<string, object?>
            {
                ["$id"] = template.TemplateId,
                ["$name"] = template.Name,
                ["$builtIn"] = template.IsBuiltIn ? 1 : 0,
                ["$json"] = JsonSerializer.Serialize(BackupPolicyTemplateCatalog.ClonePolicy(template.Policy), _json),
                ["$utc"] = now
            }, token);
    }

    public Task DeletePolicyTemplateAsync(string templateId, CancellationToken token)
        => ExecuteAsync("DELETE FROM backup_policy_templates WHERE template_id=$id AND is_built_in=0;",
            new Dictionary<string, object?> { ["$id"] = templateId }, token);

    private static async Task EnsureBuiltInPolicyTemplatesAsync(SqliteConnection connection, CancellationToken token)
    {
        var now = DateTime.UtcNow.ToString("O");
        foreach (var template in BackupPolicyTemplateCatalog.CreateBuiltIns())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT OR IGNORE INTO backup_policy_templates(template_id,name,is_built_in,policy_json,created_utc,updated_utc)
VALUES($id,$name,1,$json,$utc,$utc);";
            command.Parameters.AddWithValue("$id", template.TemplateId);
            command.Parameters.AddWithValue("$name", template.Name);
            command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(template.Policy));
            command.Parameters.AddWithValue("$utc", now);
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }
}
