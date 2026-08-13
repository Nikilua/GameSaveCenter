namespace GameSaveCenter.Contracts
{
    /// <summary>Pure IPC compatibility rules shared by client and server.</summary>
    public static class ProtocolCompatibility
    {
        public static bool IsCompatible(int clientProtocolVersion, int serverProtocolVersion, int serverMinimumSupportedProtocolVersion)
            => clientProtocolVersion == serverProtocolVersion
               && clientProtocolVersion >= serverMinimumSupportedProtocolVersion;
    }
}
