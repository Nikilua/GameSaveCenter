using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Ipc;

internal static class IpcRequestPolicy
{
    /// <summary>Write requests use the durable request ledger; reads may simply stop waiting.</summary>
    public static bool RequiresReplayProtection(string type)
        => IpcRequestSemantics.RequiresReplayProtection(type);
}
