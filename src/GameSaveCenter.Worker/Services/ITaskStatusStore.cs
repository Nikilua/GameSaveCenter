using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

/// <summary>Minimal durable task-state seam used by the coordinator.</summary>
public interface ITaskStatusStore
{
    Task AddOrUpdateTaskAsync(TaskStatusDto task, CancellationToken token);
}
