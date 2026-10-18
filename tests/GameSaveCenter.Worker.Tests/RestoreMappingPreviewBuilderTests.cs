using System.Text.Json;
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
        var serialized = JsonSerializer.Serialize(preview);
        Assert.DoesNotContain(secretPath, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("slot-2.sav", serialized, StringComparison.OrdinalIgnoreCase);
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
