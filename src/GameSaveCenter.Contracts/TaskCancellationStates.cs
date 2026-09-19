namespace GameSaveCenter.Contracts;

/// <summary>Durable cancellation phases shared by Worker, IPC and the task center.</summary>
public static class TaskCancellationStates
{
    public const string None = "";
    public const string Requested = "Requested";
    public const string Finalizing = "Finalizing";
    public const string Cancelled = "Cancelled";
    public const string NotInterruptible = "NotInterruptible";
}
