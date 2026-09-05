namespace GameSaveCenter.Contracts;

/// <summary>Shared request classification used by both IPC endpoints.</summary>
public static class IpcRequestSemantics
{
    /// <summary>Whether losing the response could leave durable or file state changed.</summary>
    public static bool RequiresReplayProtection(string type)
        => type switch
        {
            MessageTypes.BackupGame or MessageTypes.BackupAll or MessageTypes.RestoreExecute or
            MessageTypes.RestoreRemoteBackup or MessageTypes.UndoRestore or MessageTypes.SyncMedia or
            MessageTypes.UpdateMediaMetadata or MessageTypes.UpdateMediaMetadataBatch or
            MessageTypes.ReassignMedia or MessageTypes.ReassignMediaBatch or MessageTypes.IgnoreMedia or
            MessageTypes.IgnoreMediaBatch or MessageTypes.RestoreIgnoredMediaBatch or
            MessageTypes.AddMediaSource or MessageTypes.UpdateMediaSource or MessageTypes.DeleteMediaSource or
            MessageTypes.AcceptSavePath or MessageTypes.RejectSavePath or MessageTypes.UpdateGamePolicy or
            MessageTypes.ProtectionPromptDecision or MessageTypes.ApplyRecommendedProtection or
            MessageTypes.SavePolicyTemplate or MessageTypes.DeletePolicyTemplate or MessageTypes.ApplyPolicyTemplate or
            MessageTypes.RetryCloudUpload or MessageTypes.SyncDeviceStates or MessageTypes.SaveDeviceConflictDecision or
            MessageTypes.StageRemoteBackup or MessageTypes.SaveProcessMapping or MessageTypes.DeleteProcessMapping or
            MessageTypes.UpdateSettings or MessageTypes.CreateMetadataBackup or MessageTypes.ExecuteMetadataRestore or
            MessageTypes.RollbackMetadataRestore or MessageTypes.RebuildRepository or MessageTypes.PathRemap or
            MessageTypes.ApplyRetentionSimulation or MessageTypes.MirrorLocalSync or MessageTypes.CancelTask or
            MessageTypes.ImportGameTool or MessageTypes.UpdateGameTool or MessageTypes.RelocateGameTool or
            MessageTypes.DeleteGameTool or MessageTypes.LaunchGameTool or MessageTypes.DownloadTrainer => true,
            _ => false
        };
}
