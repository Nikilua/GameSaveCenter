namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>
/// The small Ludusavi surface used by the restore state machine. Keeping this boundary
/// narrow makes the PreRestore/rollback workflow testable without invoking a real game
/// or touching a user's save directory.
/// </summary>
public interface IRestoreClient
{
    Task<LudusaviCommandResult> BackupAsync(IEnumerable<string> games, bool force, bool preview, CancellationToken token);
    Task<LudusaviCommandResult> ListBackupsAsync(IEnumerable<string> games, CancellationToken token);
    Task<LudusaviCommandResult> RestoreAsync(string game, string backupId, bool preview, CancellationToken token);
    Task<LudusaviCommandResult> RestoreFromPathAsync(string backupPath, string game, string backupId, bool preview, CancellationToken token);
    Task<LudusaviCommandResult> EditBackupAsync(string game, string backupId, string? comment, bool? locked, CancellationToken token);
}
