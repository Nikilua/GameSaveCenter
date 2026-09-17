using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class PasteNormalizationSourceTests
{
    [Fact]
    public void ProductionEditorsDeclareTheAppropriatePasteKindsAndUserFacingHint()
    {
        var root = TestRepositoryContext.Root;
        var settings = File.ReadAllText(Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var media = File.ReadAllText(Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Equal(6, Count(settings, "infra:PasteNormalization.Kind=\"Path\""));
        Assert.Contains("infra:PasteNormalization.Kind=\"RemoteTarget\"", settings);
        Assert.Contains("外层空白/引号", settings);
        Assert.Contains("多个非空行的粘贴会被阻止", settings);
        Assert.Contains("AutomationProperties.Name=\"路径粘贴标准化说明\"", settings);
        Assert.Contains("infra:PasteNormalization.Kind=\"Path\"", media);
        Assert.Contains("infra:PasteNormalization.Kind=\"ExcludePattern\"", media);
        Assert.Contains("AutomationProperties.Name=\"媒体来源粘贴标准化说明\"", media);
    }

    private static int Count(string text, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }
}
