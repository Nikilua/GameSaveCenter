using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>
    /// Lightweight Worker handshake returned by the stable named pipe.
    /// The plugin uses this to reject a healthy-but-stale Worker left behind by
    /// an older installed extension version.
    /// </summary>
    public sealed class WorkerPingDto
    {
        public DateTime Utc { get; set; }
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>Explicit protocol handshake returned by the Worker named pipe.</summary>
    public sealed class WorkerHandshakeDto
    {
        public int ProtocolVersion { get; set; } = ProtocolConstants.ProtocolVersion;
        public int MinimumSupportedProtocolVersion { get; set; } = ProtocolConstants.ProtocolVersion;
        public string WorkerVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public List<string> Capabilities { get; set; } = new List<string>();
        public DateTime Utc { get; set; }
    }

    /// <summary>Worker capabilities exposed through the handshake for progressive upgrades.</summary>
    public static class WorkerCapabilities
    {
        public static readonly string[] Current =
        {
            "RestoreReadiness",
            "MetadataBackup",
            "RepositoryRebuild",
            "PathRemap",
            "TaskReconcile",
            "GameOperationLock",
            "AtomicIo",
            "StorageAnalysis",
            "RetentionSimulation",
            "LocalMirror"
        };
    }
}
