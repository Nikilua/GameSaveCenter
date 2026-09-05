using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Initializes durable storage before regular background operations begin.</summary>
public sealed class WorkerInitializationService : IHostedService
{
    private readonly SqliteStateStore _store;
    private readonly SavePathDetectionService _detection;
    private readonly BackupOrchestrator _backups;
    private readonly RetentionSimulationService _retentionSimulation;
    private readonly CloudTransferStateService _cloudState;
    private readonly WorkerOptions _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<WorkerInitializationService> _logger;

    public WorkerInitializationService(SqliteStateStore store, SavePathDetectionService detection, BackupOrchestrator backups, RetentionSimulationService retentionSimulation, CloudTransferStateService cloudState, WorkerOptions options, IHostApplicationLifetime lifetime, ILogger<WorkerInitializationService> logger)
    { _store=store; _detection=detection; _backups=backups; _retentionSimulation=retentionSimulation; _cloudState=cloudState; _options=options; _lifetime=lifetime; _logger=logger; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch=Stopwatch.StartNew();
        try
        {
            var storageTimer=Stopwatch.StartNew();
            await _store.InitializeAsync(cancellationToken).ConfigureAwait(false);
            storageTimer.Stop();
            _logger.LogDebug("Worker storage initialization completed in {ElapsedMs}ms", storageTimer.ElapsedMilliseconds);

            var quarantineTimer=Stopwatch.StartNew();
            var quarantineRecovery=await _retentionSimulation.RecoverPendingQuarantineAsync(cancellationToken).ConfigureAwait(false);
            quarantineTimer.Stop();
            _logger.LogInformation("Retention quarantine recovery completed in {ElapsedMs}ms: restored {RestoredCount}, deleted {DeletedCount}, recovery required {RecoveryRequiredCount}.", quarantineTimer.ElapsedMilliseconds, quarantineRecovery.RestoredCount, quarantineRecovery.DeletedCount, quarantineRecovery.RecoveryRequiredCount);

            var taskTimer=Stopwatch.StartNew();
            await _store.MarkInterruptedTasksAsync(_options.WorkerSessionId, cancellationToken).ConfigureAwait(false);
            taskTimer.Stop();
            _logger.LogDebug("Worker stale-task reconciliation completed in {ElapsedMs}ms", taskTimer.ElapsedMilliseconds);
            await _cloudState.RecoverInterruptedAsync(cancellationToken).ConfigureAwait(false);

            var snapshotTimer=Stopwatch.StartNew();
            await _detection.CleanupExpiredSnapshotsAsync(cancellationToken).ConfigureAwait(false);
            snapshotTimer.Stop();
            _logger.LogDebug("Worker snapshot cleanup completed in {ElapsedMs}ms", snapshotTimer.ElapsedMilliseconds);
            await _backups.ResumePendingAsync(_lifetime.ApplicationStopping).ConfigureAwait(false);
            _options.RecordStartupSuccess();
            var version = typeof(WorkerInitializationService).Assembly.GetName().Version?.ToString() ?? "unknown";
            stopwatch.Stop();
            _logger.LogInformation("GameSaveCenter Worker {Version} storage initialized and stale tasks reconciled in {ElapsedMs}ms", version, stopwatch.ElapsedMilliseconds);
        }
        catch
        {
            _options.RecordStartupFailure();
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)=>Task.CompletedTask;
}
