namespace GameSaveCenter.Contracts
{
    /// <summary>Read-only preview of the backup repository scan.</summary>
    public sealed class RepositoryRebuildPreviewDto
    {
        public int FoundArchives { get; set; }
        public int ConfirmableArchives { get; set; }
        public int PartialMetadataArchives { get; set; }
        public int CorruptArchives { get; set; }
        public int UnassignedArchives { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>Request to rebuild the backup index after an explicit preview.</summary>
    public sealed class RepositoryRebuildRequestDto
    {
        public bool Confirmed { get; set; }
    }

    /// <summary>Result of rebuilding the indexed backup repository from Ludusavi.</summary>
    public sealed class RepositoryRebuildResultDto
    {
        public int RebuiltGameCount { get; set; }
        public int IndexedVersionCount { get; set; }
        public int FailedGameCount { get; set; }
        public int RecoveredGameCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
