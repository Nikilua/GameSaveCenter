using System;

namespace GameSaveCenter.Contracts;

public sealed class CreateDiagnosticsPackageRequestDto
{
    public int AuditLimit { get; set; } = 300;
    public int TaskLimit { get; set; } = 200;
    public string PluginVersion { get; set; } = string.Empty;
    public string PlayniteVersion { get; set; } = string.Empty;
    public string ThemeMode { get; set; } = string.Empty;
    public string CurrentWorkspace { get; set; } = string.Empty;
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
}
