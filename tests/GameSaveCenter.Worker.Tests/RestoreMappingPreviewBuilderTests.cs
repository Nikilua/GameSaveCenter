using System.Text.Json;
using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RestoreMappingPreviewBuilderTests
{
    [Fact]
    public void PreviewSummaryContainsCountsButNeverRawPathsOrWarnings()
    {
        const string secretPath = "C:/Users/test-user/Documents/private-slot.sav";
        using var document = JsonDocument.Parse("""
        {"games":{"DEMO GAME":{"files":{"C:/Users/test-user/Documents/private-slot.sav":{"bytes":80},"slot-2.sav":{"bytes":120}}}}}
        """);
        var result = LudusaviCommandResult.SuccessResult(document.RootElement.Clone(), secretPath, 0, secretPath);

        var preview = RestoreMappingPreviewBuilder.Build(
            result, "playnite-game-id", "backup-v2", "Demo Game", DateTime.UtcNow);

        Assert.True(preview.Success);
        Assert.Equal(2, preview.FileCount);
        Assert.Equal(200, preview.TotalBytes);
        Assert.Equal(0, preview.UnknownSizeCount);
        var serialized = JsonSerializer.Serialize(preview);
        Assert.DoesNotContain(secretPath, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("slot-2.sav", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PathFreeFingerprintIsStableForSameMappingAndDistinguishesSameVersionMappings()
    {
        const string keyId = "7921d802aedb490ab9935cd8f2241968";
        var key = Convert.ToBase64String(Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"));
        var first = BuildPreview("""
            {"games":{"Demo Game":{"files":{"C:/Users/Alice/private-slot.sav":{"bytes":80},"slot-2.sav":{"bytes":120}}}}}
            """, key, keyId);
        var reordered = BuildPreview("""
            {"games":{"Demo Game":{"files":{"slot-2.sav":{"bytes":120},"C:/Users/Alice/private-slot.sav":{"bytes":80}}}}}
            """, key, keyId);
        var changedPath = BuildPreview("""
            {"games":{"Demo Game":{"files":{"D:/Different/private-slot.sav":{"bytes":80},"slot-2.sav":{"bytes":120}}}}}
            """, key, keyId);

        Assert.Equal(first.MappingFingerprint, reordered.MappingFingerprint);
        Assert.NotEqual(first.MappingFingerprint, changedPath.MappingFingerprint);
        Assert.Equal(1, first.MappingFingerprintVersion);
        Assert.Equal(keyId, first.MappingFingerprintKeyId);
        Assert.Equal(2, first.FileCount);
        Assert.Equal(200, first.TotalBytes);
        var serialized = JsonSerializer.Serialize(first);
        Assert.DoesNotContain(key, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("private-slot.sav", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/Alice", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingFileSizesRemainUnknownAndAreExcludedFromKnownSubtotal()
    {
        var preview = BuildPreview("""
            {"games":{"Demo Game":{"files":{"known.sav":{"bytes":80},"unknown.sav":{},"negative.sav":{"bytes":-2}}}}}
            """, Convert.ToBase64String(Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef")), "7921d802aedb490ab9935cd8f2241968");

        Assert.Equal(3, preview.FileCount);
        Assert.Equal(80, preview.TotalBytes);
        Assert.Equal(2, preview.UnknownSizeCount);
        Assert.Contains("2 个文件大小未知", preview.Summary, StringComparison.Ordinal);
        Assert.DoesNotContain("unknown.sav", JsonSerializer.Serialize(preview), StringComparison.OrdinalIgnoreCase);
    }

    private static RestoreMappingPreviewDto BuildPreview(string json, string key, string keyId)
    {
        using var document = JsonDocument.Parse(json);
        return RestoreMappingPreviewBuilder.Build(
            LudusaviCommandResult.SuccessResult(document.RootElement.Clone(), "", 0, ""),
            "playnite-game-id",
            "backup-v2",
            "Demo Game",
            DateTime.UtcNow,
            key,
            keyId);
    }

    [Fact]
    public void InvalidPreviewProducesGenericFailureWithoutLeakingErrorText()
    {
        var result = LudusaviCommandResult.Failure("", "Failure at C:/Users/test-user/private-slot.sav");

        var preview = RestoreMappingPreviewBuilder.Build(
            result, "playnite-game-id", "backup-v2", "Demo Game", DateTime.UtcNow);

        Assert.False(preview.Success);
        Assert.Equal("RESTORE_PREVIEW_FAILED", preview.ErrorCode);
        Assert.DoesNotContain("test-user", JsonSerializer.Serialize(preview), StringComparison.OrdinalIgnoreCase);
    }
}
