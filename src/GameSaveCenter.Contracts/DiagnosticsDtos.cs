using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Contracts;

public sealed class CreateDiagnosticsPackageRequestDto
{
    public int AuditLimit { get; set; } = 300;
    public int TaskLimit { get; set; } = 200;
    public string PluginVersion { get; set; } = string.Empty;
    public string PluginBuildIdentity { get; set; } = string.Empty;
    public string PlayniteVersion { get; set; } = string.Empty;
    public string ThemeMode { get; set; } = string.Empty;
    public string CurrentWorkspace { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string EvidenceSource { get; set; } = string.Empty;
    public double WindowWidthDip { get; set; }
    public double WindowHeightDip { get; set; }
    public int LoadedItemCount { get; set; }
    public double DpiScale { get; set; } = 1;
    public int ScreenCount { get; set; } = 1;
}

public sealed class DiagnosticsPackageResultDto
{
    public string PackagePath { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public long PackageBytes { get; set; }
    public int IncludedFileCount { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string ResultDisplay => $"{Summary}\n位置：{PackagePath}\n大小：{FormatBytes(PackageBytes)}";

    private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
}

public sealed class DiagnosticsPackagePreviewItemDto
{
    public string EntryName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Redaction { get; set; } = string.Empty;
    public bool Optional { get; set; }
    public string OptionalDisplay => Optional ? "（存在时包含）" : string.Empty;
}

public sealed class DiagnosticsPackagePreviewDto
{
    public List<DiagnosticsPackagePreviewItemDto> IncludedItems { get; set; } = new List<DiagnosticsPackagePreviewItemDto>();
    public List<string> ExcludedItems { get; set; } = new List<string>();
    public int MaxPackageBytes { get; set; }
    public int MaxLogBytes { get; set; }
    public int AuditLimit { get; set; }
    public int TaskLimit { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string ConfirmationText
        => Summary
            + "\n\n将包含：\n"
            + string.Join("\n", IncludedItems.Select(item => $"· {item.EntryName}：{item.Description}{item.OptionalDisplay}；{item.Redaction}"))
            + "\n\n明确不包含：\n"
            + string.Join("\n", ExcludedItems.Select(item => "· " + item));
}
