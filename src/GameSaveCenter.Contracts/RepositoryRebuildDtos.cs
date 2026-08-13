namespace GameSaveCenter.Contracts
{
    /// <summary>Result of rebuilding the indexed backup repository from Ludusavi.</summary>
    public sealed class RepositoryRebuildResultDto
    {
        public int RebuiltGameCount { get; set; }
        public int IndexedVersionCount { get; set; }
        public int FailedGameCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
