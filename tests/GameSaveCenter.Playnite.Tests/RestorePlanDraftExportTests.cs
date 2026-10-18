using System;
using System.IO;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RestorePlanDraftExportTests
{
    [Fact]
    public void ExportContainsOnlyReadableSummaryAndRedactsPathAndInternalIdentity()
    {
        var draft = CreateDraft();
        draft.GameName = @"C:\Users\private-user\Saved Games\Custom Name";
        draft.RevalidationReason = @"映射已变化：D:\private\profiles\save";

        var text = RestorePlanDraftExportFormatter.Create(draft);

        Assert.Contains("GameSaveCenter 恢复方案摘要", text, StringComparison.Ordinal);
        Assert.Contains("[包含本地路径，已隐藏]", text, StringComparison.Ordinal);
        Assert.Contains("目标文件：4 个 · 8192 bytes", text, StringComparison.Ordinal);
        Assert.Contains("映射摘要：3 个文件 · 4096 bytes · 大小未知 1 个", text, StringComparison.Ordinal);
        Assert.Contains("需重验：", text, StringComparison.Ordinal);
        Assert.Contains("保护要求：", text, StringComparison.Ordinal);
        Assert.Contains("不可导入或执行", text, StringComparison.Ordinal);
        Assert.Contains("导入此文本不能触发恢复", text, StringComparison.Ordinal);

        Assert.DoesNotContain("private-user", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Saved Games", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("profiles", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("draft-internal", text, StringComparison.Ordinal);
        Assert.DoesNotContain("playnite-internal", text, StringComparison.Ordinal);
        Assert.DoesNotContain("backup-internal", text, StringComparison.Ordinal);
        Assert.DoesNotContain("mapping-preview-internal", text, StringComparison.Ordinal);
        Assert.DoesNotContain(new string('A', 64), text, StringComparison.Ordinal);
        Assert.DoesNotContain(new string('B', 64), text, StringComparison.Ordinal);
        Assert.DoesNotContain("fingerprint-key-internal", text, StringComparison.Ordinal);
    }

    [Fact]
    public void CurrentDraftExportDoesNotClaimFreshValidation()
    {
        var draft = CreateDraft();
        draft.RevalidationState = RestorePlanDraftDefinition.CurrentRevalidationState;
        draft.RevalidationReason = string.Empty;

        var text = RestorePlanDraftExportFormatter.Create(draft);

        Assert.Contains("当前草稿未标记失效", text, StringComparison.Ordinal);
        Assert.Contains("不代表刚刚完成验证", text, StringComparison.Ordinal);
        Assert.Contains("实际恢复仍需重新检查", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportRedactsCredentialParametersFromUserSuppliedLabels()
    {
        var draft = CreateDraft();
        draft.GameName = "Demo https://example.test/game?token=share-secret";

        var text = RestorePlanDraftExportFormatter.Create(draft);

        Assert.Contains("[参数已隐藏]", text, StringComparison.Ordinal);
        Assert.DoesNotContain("share-secret", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ExportPreviewIsExplicitReadOnlyAndHasNoTextImportCommand()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.RestorePlanDrafts.cs"));

        Assert.Contains("PreviewRestorePlanDraftExportCommand", view, StringComparison.Ordinal);
        Assert.Contains("ExportRestorePlanDraftCommand", view, StringComparison.Ordinal);
        Assert.Contains("IsReadOnly=\"True\"", view, StringComparison.Ordinal);
        Assert.Contains("RestorePlanDraftExportFormatter.Create(draft)", viewModel, StringComparison.Ordinal);
        Assert.Contains("File.WriteAllText(dialog.FileName, currentText", viewModel, StringComparison.Ordinal);
        Assert.Contains("_ => RunLocal(ExportRestorePlanDraft)", viewModel, StringComparison.Ordinal);
        Assert.DoesNotContain("ImportRestorePlanDraft", viewModel, StringComparison.Ordinal);
        Assert.DoesNotContain("RestorePlanDraftImportCommand", view, StringComparison.Ordinal);
    }

    private static RestorePlanDraftDefinition CreateDraft()
        => new RestorePlanDraftDefinition
        {
            DraftId = "draft-internal",
            CreatedUtc = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc),
            PlayniteId = "playnite-internal",
            GameName = "Demo Game",
            TargetBackupId = "backup-internal",
            TargetCreatedUtc = new DateTime(2026, 10, 18, 8, 0, 0, DateTimeKind.Utc),
            BaselineBackupId = "baseline-internal",
            TargetFileCount = 4,
            TargetTotalBytes = 8192,
            TargetFileIdentityFingerprintVersion = 1,
            TargetFileIdentityFingerprint = new string('A', 64),
            TargetFileIdentityFingerprintKeyId = "file-fingerprint-key-internal",
            MappingFileCount = 3,
            MappingTotalBytes = 4096,
            MappingUnknownSizeCount = 1,
            MappingPreviewId = "mapping-preview-internal",
            MappingPreviewedUtc = new DateTime(2026, 10, 19, 8, 1, 0, DateTimeKind.Utc),
            MappingFingerprintVersion = 1,
            MappingFingerprint = new string('B', 64),
            MappingFingerprintKeyId = "fingerprint-key-internal",
            ReadinessStatus = "Ready",
            ReadinessCheckedUtc = new DateTime(2026, 10, 19, 7, 55, 0, DateTimeKind.Utc),
            ReadinessBackupId = "backup-internal",
            ReadinessFingerprintVersion = 1,
            ReadinessFingerprint = new string('C', 64),
            ReadinessFingerprintKeyId = "readiness-fingerprint-key-internal",
            RevalidationState = RestorePlanDraftDefinition.NeedsRevalidationState,
            RevalidationReason = "映射或文件身份已变化，需要重新验证草稿。"
        };
}
