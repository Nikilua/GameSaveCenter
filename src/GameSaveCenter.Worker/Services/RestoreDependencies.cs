using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

/// <summary>Read-only catalog surface required by the safe restore workflow.</summary>
public interface IRestoreCatalog
{
    Task<GameDescriptorDto?> GetGameAsync(string playniteId, CancellationToken token);
    Task<Dictionary<string, (string Name, double Confidence)>> GetMatchesAsync(CancellationToken token);
}

/// <summary>Live session surface required to prevent restore while a game is running.</summary>
public interface IRestoreSessionState
{
    IReadOnlyCollection<GameSessionEventDto> ActiveSessions { get; }
}

/// <summary>Remote staging surface used only by the remote restore entry point.</summary>
public interface IRemoteBackupStageProvider
{
    Task<RemoteBackupStage> RevalidateAsync(string stagingId, CancellationToken token);
}
