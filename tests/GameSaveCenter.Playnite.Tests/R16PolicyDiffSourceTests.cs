using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16PolicyDiffSourceTests
{
    [Fact]
    public void SaveCenterSeparatesPolicyBaselineDraftTemplateAndLocalCancel()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("当前已保存/继承基线", view);
        Assert.Contains("显式草稿值", view);
        Assert.Contains("模板将覆盖值", view);
        Assert.Contains("SelectedGamePolicyDiffEntries", view);
        Assert.Contains("PolicyTemplateDiffEntries", view);
        Assert.Contains("CancelPolicyDraftCommand", view);
        Assert.Contains("BackupPolicyDiff.Compare", viewModel);
        Assert.Contains("BackupPolicyDiff.CopyTo(selectedGamePolicyBaseline, SelectedGame.Policy)", viewModel);
        Assert.Contains("!HasSelectedGamePolicyChanges && SelectedPolicyTemplate", viewModel);

        var cancelStart = viewModel.IndexOf("private void CancelPolicyDraft()", StringComparison.Ordinal);
        Assert.True(cancelStart >= 0);
        var cancelEnd = viewModel.IndexOf("private void UpdateSelectedGamePolicyBaseline", cancelStart, StringComparison.Ordinal);
        Assert.True(cancelEnd > cancelStart);
        Assert.DoesNotContain("RequestAsync", viewModel.Substring(cancelStart, cancelEnd - cancelStart), StringComparison.Ordinal);
    }
}
