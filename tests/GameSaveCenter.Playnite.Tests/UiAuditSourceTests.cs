using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiAuditSourceTests
{
    [Fact]
    public void AuditEntryPointsExistAndReuseTheRenderHarness()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, "scripts", "capture-ui-audit.ps1"));
        var cmd = File.ReadAllText(Path.Combine(root, "GameSaveCenter-UI-Audit.cmd"));

        Assert.Contains("GameSaveCenter.RenderHarness.exe", script);
        Assert.Contains(" audit ", script);
        Assert.Contains("GameSaveCenter-ui-audit.zip", script);
        Assert.Contains("artifacts", script);
        Assert.Contains("capture-ui-audit.ps1", cmd);
    }

    [Fact]
    public void AuditHarnessContainsStaticRuntimeAndFullScrollCapabilities()
    {
        var root = FindRepositoryRoot();
        var harnessRoot = Path.Combine(root, "tests", "GameSaveCenter.RenderHarness");
        var program = File.ReadAllText(Path.Combine(harnessRoot, "Program.cs"));
        var manifest = File.ReadAllText(Path.Combine(harnessRoot, "UiAudit", "UiStaticManifestBuilder.cs"));
        var screenshot = File.ReadAllText(Path.Combine(harnessRoot, "UiAudit", "UiScreenshotService.cs"));
        var visualTree = File.ReadAllText(Path.Combine(harnessRoot, "UiAudit", "UiVisualTreeInspector.cs"));
        var layout = File.ReadAllText(Path.Combine(harnessRoot, "UiAudit", "UiLayoutAnalyzer.cs"));
        var sanitizer = File.ReadAllText(Path.Combine(harnessRoot, "UiAudit", "UiAuditSanitizer.cs"));

        Assert.Contains("UiAuditRunner", program);
        Assert.Contains("TabItem", manifest);
        Assert.Contains("DataGridColumn", manifest);
        Assert.Contains("ScrollViewer", manifest);
        Assert.Contains("Expander", manifest);
        Assert.Contains("Visibility", manifest);
        Assert.Contains("CaptureScrollViewerFull", screenshot);
        Assert.Contains("ScrollToVerticalOffset", screenshot);
        Assert.Contains("VisualTreeHelper", visualTree);
        Assert.Contains("NESTED_VERTICAL_SCROLL", layout);
        Assert.Contains("TABLE_VIEWPORT_TOO_SHORT", layout);
        Assert.Contains("TOOLBAR_VERTICAL_EXPANSION", layout);
        Assert.Contains("PRIMARY_VIEWPORT_UNREACHABLE", layout);
        Assert.Contains("CONTROL_CLIPPED", layout);
        Assert.Contains("RunProductionShellMediaProbe", program);
        Assert.Contains("MediaInboxFooter", program);
        Assert.Contains("footer remains unreachable", program);
        Assert.Contains("RunProductionShellMaintenanceProbe", program);
        Assert.Contains("RunProductionShellTaskProbe", program);
        Assert.Contains("%USERPROFILE%", sanitizer);
    }

    [Fact]
    public void MediaInboxKeepsFiniteGridAndCompactInspectorContracts()
    {
        var root = FindRepositoryRoot();
        var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var codeBehind = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs"));
        var sourceValidator = File.ReadAllText(Path.Combine(root, "scripts", "validate-source.py"));

        Assert.Contains("x:Name=\"MediaInboxPageScrollViewer\"", xaml);
        Assert.Contains("Style=\"{DynamicResource GscPageScrollViewer}\"", xaml);
        Assert.Contains("x:Name=\"MediaInboxCompactDetailsButton\"", xaml);
        Assert.Contains("x:Name=\"MediaInboxSecondaryActions\"", xaml);
        Assert.Contains("x:Name=\"MediaInboxGrid\" Tag=\"FiniteViewport\"", xaml);
        Assert.Contains("mediaInboxInspectorOpen", codeBehind);
        Assert.Contains("mediaInboxHistoryOpen", codeBehind);
        Assert.Contains("MediaInboxSelectionDetails", xaml);
        Assert.Contains("MediaInboxNoSelectionHint", xaml);
        Assert.Contains("MediaInboxGrid.MinHeight = 0d", codeBehind);
        Assert.Contains("MediaInboxGrid.Height = double.NaN", codeBehind);
        Assert.Contains("height < 620", codeBehind);
        Assert.Contains("ScrollBarVisibility.Disabled", codeBehind);
        Assert.Contains("MediaInboxPageScrollViewer", sourceValidator);
    }

    [Fact]
    public void ThemeQaHostUsesThePageBackdropResource()
    {
        var root = FindRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "Program.cs"));

        Assert.Contains("private static Brush CreateHarnessBackground(FrameworkElement view)", program);
        Assert.Contains("view.TryFindResource(\"GscBackdropBrush\") as Brush", program);
        Assert.Contains("Background = CreateHarnessBackground(view)", program);
    }

    [Fact]
    public void AuditDoesNotTouchProductionViews()
    {
        var root = FindRepositoryRoot();
        var harnessRoot = Path.Combine(root, "tests", "GameSaveCenter.RenderHarness");
        Assert.True(Directory.Exists(Path.Combine(harnessRoot, "UiAudit")));

        // The audit tool lives entirely outside the plugin source tree. Production XAML
        // files must not be modified for audit-only behavior.
        var views = Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views");
        Assert.DoesNotContain("UiAudit", Directory.GetDirectories(views));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
