using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17DiagnosticsPackagePreviewSourceTests
{
    [Fact]
    public void PreviewRunsBeforeCreateAndConfirmationIsRequired()
    {
        var root = TestRepositoryContext.Root;
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var messageTypes = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        var previewIndex = viewModel.IndexOf("MessageTypes.PreviewDiagnosticsPackage", StringComparison.Ordinal);
        var createIndex = viewModel.IndexOf("MessageTypes.CreateDiagnosticsPackage", previewIndex, StringComparison.Ordinal);
        Assert.True(previewIndex >= 0 && createIndex > previewIndex);
        Assert.Contains("plugin.ConfirmAsync(\"确认生成诊断包\"", viewModel, StringComparison.Ordinal);
        Assert.Contains("已取消生成诊断包", viewModel, StringComparison.Ordinal);
        Assert.Contains("PreviewDiagnosticsPackage=>_diagnostics.Preview", dispatcher, StringComparison.Ordinal);
        Assert.Contains("PreviewDiagnosticsPackage = \"diagnostics.package.preview\"", messageTypes, StringComparison.Ordinal);
        Assert.Contains("先预览类别和脱敏范围", view, StringComparison.Ordinal);
    }
}
