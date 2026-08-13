using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Manually reconciles tasks left in Queued/Running by an interrupted Worker. The same
/// operation runs automatically at startup; this entry point gives users a visible,
/// idempotent re-run from Maintenance.
/// </summary>
public sealed class TaskReconcileService
{
    private readonly SqliteStateStore _store;
    private readonly ILogger<TaskReconcileService> _logger;

    public TaskReconcileService(SqliteStateStore store, ILogger<TaskReconcileService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<TaskReconcileResultDto> ReconcileAsync(CancellationToken token)
    {
        var count = await _store.MarkInterruptedTasksAsync(token).ConfigureAwait(false);
        var summary = count == 0
            ? "任务协调完成：没有需要处理的中断任务。"
            : $"任务协调完成：已将 {count} 个中断任务标记为 WORKER_RESTARTED，可在任务中心查看详情后重试。";
        await _store.AppendAuditAsync("TaskReconcile", summary,
            JsonSerializer.Serialize(new { count, reconcileUtc = DateTime.UtcNow }),
            token).ConfigureAwait(false);
        _logger.LogInformation("Task reconcile completed with {Count} interrupted tasks", count);
        return new TaskReconcileResultDto
        {
            ReconcileUtc = DateTime.UtcNow,
            InterruptedTaskCount = count,
            Summary = summary
        };
    }
}
