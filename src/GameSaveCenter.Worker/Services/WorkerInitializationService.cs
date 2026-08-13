using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Initializes durable storage before regular background operations begin.</summary>
public sealed class WorkerInitializationService : IHostedService
{
    private readonly SqliteStateStore _store;
    private readonly SavePathDetectionService _detection;
    private readonly WorkerOptions _options;
    private readonly ILogger<WorkerInitializationService> _logger;

    public WorkerInitializationService(SqliteStateStore store, SavePathDetectionService detection, WorkerOptions options, ILogger<WorkerInitializationService> logger)
    { _store=store; _detection=detection; _options=options; _logger=logger; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _store.InitializeAsync(cancellationToken).ConfigureAwait(false);
            await _store.MarkInterruptedTasksAsync(_options.WorkerSessionId, cancellationToken).ConfigureAwait(false);
            await _detection.CleanupExpiredSnapshotsAsync(cancellationToken).ConfigureAwait(false);
            _options.RecordStartupSuccess();
            var version = typeof(WorkerInitializationService).Assembly.GetName().Version?.ToString() ?? "unknown";
            _logger.LogInformation("GameSaveCenter Worker {Version} storage initialized and stale tasks reconciled", version);
        }
        catch
        {
            _options.RecordStartupFailure();
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)=>Task.CompletedTask;
}
