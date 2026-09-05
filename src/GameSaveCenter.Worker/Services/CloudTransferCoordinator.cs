using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Serializes rclone writes and lets a restore reserve the same gate. Rclone copies shared
/// backup roots, so a global gate is safer than a per-game lock: another game's backup could
/// otherwise upload files for the game currently being restored.
/// </summary>
public sealed class CloudTransferCoordinator
{
    private readonly SemaphoreSlim gate=new(1,1);
    private readonly ILogger<CloudTransferCoordinator> logger;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string,CloudTransferActivity> active=new(StringComparer.OrdinalIgnoreCase);

    public CloudTransferCoordinator(ILogger<CloudTransferCoordinator> logger)=>this.logger=logger;

    public async Task<T> RunUploadAsync<T>(string operation,Func<CancellationToken,Task<T>> action,CancellationToken token,string? transferKey=null)
    {
        await gate.WaitAsync(token).ConfigureAwait(false);
        if(!string.IsNullOrWhiteSpace(transferKey)) active[transferKey]=new CloudTransferActivity{TransferKey=transferKey,Operation=operation,StartedUtc=DateTime.UtcNow};
        try
        {
            logger.LogDebug("Cloud transfer gate acquired for {Operation}",operation);
            return await action(token).ConfigureAwait(false);
        }
        finally
        {
            if(!string.IsNullOrWhiteSpace(transferKey)) active.TryRemove(transferKey,out _);
            gate.Release();
        }
    }

    public IReadOnlyList<CloudTransferActivity> GetActiveTransfers()=>active.Values.ToList();

    /// <summary>Waits for active uploads and blocks new uploads until the returned lease is disposed.</summary>
    public async Task<IDisposable> PauseForRestoreAsync(CancellationToken token)
    {
        await gate.WaitAsync(token).ConfigureAwait(false);
        logger.LogInformation("Cloud transfer gate reserved for restore");
        return new Lease(gate,logger);
    }

    private sealed class Lease : IDisposable
    {
        private readonly SemaphoreSlim gate;
        private readonly ILogger logger;
        private int disposed;

        public Lease(SemaphoreSlim gate,ILogger logger){this.gate=gate;this.logger=logger;}

        public void Dispose()
        {
            if(Interlocked.Exchange(ref disposed,1)!=0)return;
            gate.Release();
            logger.LogInformation("Cloud transfer gate released after restore");
        }
    }
}

public sealed class CloudTransferActivity
{
    public string TransferKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime StartedUtc { get; set; }
}
