using System;

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
        public DateTime Utc { get; set; }
    }
}
