using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RestorePlanDraftRevalidationTests
{
    [Fact]
    public void ObjectFileIdentityMappingAndReadinessChangesRequireRevalidation()
    {
        var settings = new GameSaveCenterSettings();
        using var key = RestorePlanMappingFingerprintKeyStore.GetOrCreate(settings, () => { });
        var game = new GameStatusDto { PlayniteId = "game-1", Name = "Demo Game" };
        var checkedUtc = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc);
        var backup = new BackupVersionDto
        {
            PlayniteId = game.PlayniteId,
            LudusaviName = "Demo Game",
            BackupId = "backup-2",
            ParentBackupId = "backup-1",
            CreatedUtc = checkedUtc.AddDays(-1),
            FileCount = 2,
            TotalBytes = 384,
            ContentFingerprint = new string('A', 64),
            RestoreReadiness = new RestoreReadinessDto
            {
                BackupVersionId = "backup-2",
                Status = RestoreReadinessStatus.Ready,
                CheckedUtc = checkedUtc
            }
        };
        var mapping = new RestoreMappingPreviewDto
        {
            Success = true,
            PlayniteId = game.PlayniteId,
            BackupId = backup.BackupId,
            PreviewedUtc = checkedUtc,
            FileCount = 2,
            TotalBytes = 384,
            UnknownSizeCount = 0,
            MappingFingerprintVersion = 1,
            MappingFingerprint = new string('B', 64),
            MappingFingerprintKeyId = key.KeyId
        };
        var draft = RestorePlanDraftDefinition.Create(
            game,
            backup,
            mapping,
            key.ComputeFileIdentityFingerprint(backup.ContentFingerprint),
            key.KeyId,
            key.ComputeReadinessFingerprint(backup.RestoreReadiness));

        Assert.False(draft.RequiresRevalidation);
        Assert.Equal(string.Empty, RestorePlanDraftRevalidation.CompareSnapshot(
            draft,
            game,
            backup,
            key.ComputeFileIdentityFingerprint(backup.ContentFingerprint),
            key.KeyId,
            key.ComputeReadinessFingerprint(backup.RestoreReadiness)));

        var changedObject = Clone(backup);
        changedObject.CreatedUtc = changedObject.CreatedUtc.AddMinutes(1);
        Assert.Contains("信息已变化", RestorePlanDraftRevalidation.CompareSnapshot(
            draft,
            game,
            changedObject,
            key.ComputeFileIdentityFingerprint(changedObject.ContentFingerprint),
            key.KeyId,
            key.ComputeReadinessFingerprint(changedObject.RestoreReadiness)), StringComparison.Ordinal);

        var changedFileIdentity = Clone(backup);
        changedFileIdentity.ContentFingerprint = new string('C', 64);
        Assert.Contains("文件身份已变化", RestorePlanDraftRevalidation.CompareSnapshot(
            draft,
            game,
            changedFileIdentity,
            key.ComputeFileIdentityFingerprint(changedFileIdentity.ContentFingerprint),
            key.KeyId,
            key.ComputeReadinessFingerprint(changedFileIdentity.RestoreReadiness)), StringComparison.Ordinal);

        var changedReadiness = Clone(backup);
        changedReadiness.RestoreReadiness!.HashValidation = "Mismatch";
        Assert.Contains("检查结果已变化", RestorePlanDraftRevalidation.CompareSnapshot(
            draft,
            game,
            changedReadiness,
            key.ComputeFileIdentityFingerprint(changedReadiness.ContentFingerprint),
            key.KeyId,
            key.ComputeReadinessFingerprint(changedReadiness.RestoreReadiness)), StringComparison.Ordinal);

        var changedMapping = new RestoreMappingPreviewDto
        {
            Success = true,
            PlayniteId = game.PlayniteId,
            BackupId = backup.BackupId,
            FileCount = mapping.FileCount,
            TotalBytes = mapping.TotalBytes,
            UnknownSizeCount = mapping.UnknownSizeCount,
            MappingFingerprintVersion = mapping.MappingFingerprintVersion,
            MappingFingerprint = new string('D', 64),
            MappingFingerprintKeyId = key.KeyId
        };
        var mappingReason = RestorePlanDraftRevalidation.CompareMapping(draft, changedMapping);
        Assert.Contains("映射或文件身份已变化", mappingReason, StringComparison.Ordinal);
        var notified = false;
        draft.PropertyChanged += (_, args) => notified |= args.PropertyName == nameof(draft.RevalidationDisplay);
        Assert.True(draft.MarkNeedsRevalidation(mappingReason));
        Assert.True(draft.RequiresRevalidation);
        Assert.Contains("需重验", draft.RevalidationDisplay, StringComparison.Ordinal);
        Assert.True(notified);

        var serialized = JsonConvert.SerializeObject(draft);
        Assert.DoesNotContain(backup.ContentFingerprint, serialized, StringComparison.Ordinal);
        Assert.Contains(draft.TargetFileIdentityFingerprint, serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyDraftWithoutIdentityBaselineIsNormalizedToNeedsRevalidation()
    {
        var draft = RestorePlanDraftDefinition.NormalizeMany(new[]
        {
            new RestorePlanDraftDefinition
            {
                DraftId = Guid.NewGuid().ToString("N"),
                PlayniteId = "game-1",
                TargetBackupId = "backup-2",
                MappingFingerprintVersion = 1,
                MappingFingerprint = new string('A', 64),
                MappingFingerprintKeyId = Guid.NewGuid().ToString("N")
            }
        })[0];

        Assert.True(draft.RequiresRevalidation);
        Assert.Contains("旧草稿", draft.RevalidationDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void DetailsRefreshAndMappingPreviewRecheckSavedDraftsWithoutBypassingRestoreGuard()
    {
        var root = TestRepositoryContext.Root;
        var dashboard = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var paging = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.BackupHistory.cs"));
        var drafts = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.RestorePlanDrafts.cs"));

        Assert.Contains("MarkRestorePlanDraftsForSnapshot(", dashboard, StringComparison.Ordinal);
        Assert.Contains("MarkRestorePlanDraftsForSnapshot(", paging, StringComparison.Ordinal);
        Assert.Contains("MarkRestorePlanDraftsForMappingPreview(preview)", drafts, StringComparison.Ordinal);
        Assert.Contains("RestoreConfirmationGuard.IsCurrent", dashboard, StringComparison.Ordinal);
        Assert.Contains("MessageTypes.ValidateRestoreReadiness", dashboard, StringComparison.Ordinal);
    }

    private static BackupVersionDto Clone(BackupVersionDto source)
        => new()
        {
            PlayniteId = source.PlayniteId,
            LudusaviName = source.LudusaviName,
            BackupId = source.BackupId,
            ParentBackupId = source.ParentBackupId,
            CreatedUtc = source.CreatedUtc,
            FileCount = source.FileCount,
            TotalBytes = source.TotalBytes,
            ContentFingerprint = source.ContentFingerprint,
            RestoreReadiness = source.RestoreReadiness == null ? null : new RestoreReadinessDto
            {
                BackupVersionId = source.RestoreReadiness.BackupVersionId,
                Status = source.RestoreReadiness.Status,
                CheckedUtc = source.RestoreReadiness.CheckedUtc
            }
        };
}
