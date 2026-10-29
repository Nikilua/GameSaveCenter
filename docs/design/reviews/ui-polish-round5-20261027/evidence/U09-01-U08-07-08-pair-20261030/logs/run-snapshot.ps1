[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Label,
    [Parameter(Mandatory=$true)][string]$Commit,
    [switch]$KeepSource,
    [switch]$NotificationNoResults,
    [switch]$ExpectClearFilterButton
)
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$phase = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$sdk = Join-Path $workspace '.tmp\round5-sdk8-8.0.423'
$snapshot = Join-Path $phase ('snapshots\' + $Label)
$source = Join-Path $snapshot 'source'
$archive = Join-Path $snapshot ($Label + '.zip')
$render = Join-Path $phase ('renders\' + $Label)
New-Item -ItemType Directory -Force -Path $source,$render | Out-Null
$archiveLog = Join-Path $snapshot 'archive.log'
$workerLog = Join-Path $snapshot 'worker-build.log'
$harnessLog = Join-Path $snapshot 'render-harness-build.log'
$renderLog = Join-Path $snapshot 'render-qa.log'
$metaPath = Join-Path $snapshot 'snapshot.json'
$dotnet = Join-Path $sdk 'dotnet.exe'
$env:DOTNET_ROOT = $sdk
$env:DOTNET_MULTILEVEL_LOOKUP = '0'
$env:PATH = $sdk + ';' + $env:PATH
$env:MSBuildEnableWorkloadResolver = 'false'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:GSC_RENDER_SAMPLE_UTC = '2026-10-28T09:00:00Z'
$env:GSC_RENDER_TOOL_BUSY = '1'
$env:GSC_RENDER_NOTIFICATION_NO_RESULTS = if ($NotificationNoResults) { '1' } else { '0' }
$env:GSC_RENDER_EXPECT_NOTIFICATION_CLEAR = if ($ExpectClearFilterButton) { '1' } else { '0' }
$archiveStarted = (Get-Date).ToUniversalTime().ToString('o')
Push-Location $workspace
try {
    & git archive --format=zip --output=$archive $Commit *> $archiveLog
    $archiveExit = $LASTEXITCODE
    if ($archiveExit -ne 0) { throw "git archive exit $archiveExit" }
    $archiveInfo = Get-Item -LiteralPath $archive
    $archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    Expand-Archive -LiteralPath $archive -DestinationPath $source -Force
    Remove-Item -LiteralPath $archive -Force
    $archiveBytes = $archiveInfo.Length
    $archiveFinished = (Get-Date).ToUniversalTime().ToString('o')
    $fakeDataPath = Join-Path $source 'tests\GameSaveCenter.RenderHarness\FakeDashboardData.cs'
    $fakeClockPath = Join-Path $source 'tests\GameSaveCenter.RenderHarness\FakeSampleClock.cs'
    $fakeDataText = Get-Content -LiteralPath $fakeDataPath -Raw
    if ($fakeDataText.Contains('DateTime.UtcNow')) {
        $fakeDataText = $fakeDataText.Replace('DateTime.UtcNow', 'FakeSampleClock.UtcNow')
        Set-Content -LiteralPath $fakeDataPath -Value $fakeDataText -Encoding utf8
    }
    if (-not (Test-Path -LiteralPath $fakeClockPath)) {
        @'
using System;
using System.Globalization;

namespace GameSaveCenter.RenderHarness;

internal static class FakeSampleClock
{
    private static readonly DateTime Sample = DateTime.Parse(
        Environment.GetEnvironmentVariable("GSC_RENDER_SAMPLE_UTC") ?? "2026-10-28T09:00:00Z",
        CultureInfo.InvariantCulture,
        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    public static DateTime UtcNow => Sample;
}
'@ | Set-Content -LiteralPath $fakeClockPath -Encoding utf8
    }
    $fakeDataText = $fakeDataText.Replace('public string NotificationInboxSearchText { get; set; } = string.Empty;', 'public string NotificationInboxSearchText { get; set; } = string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase) ? "不存在的通知样本" : string.Empty;')
    $fakeDataText = $fakeDataText.Replace('public int NotificationUnreadCount => NotificationInboxItems.Count(item => !item.IsRead);', 'public int NotificationUnreadCount => string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase) ? 2 : NotificationInboxItems.Count(item => !item.IsRead);')
    $fakeDataText = $fakeDataText.Replace('public string NotificationInboxCountSummary => $"最近 30 天 · 未读 {NotificationUnreadCount} · 当前显示 {NotificationInboxItems.Count}/{NotificationInboxItems.Count} 个";', 'public string NotificationInboxCountSummary => string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase) ? "最近 30 天 · 未读 2 · 当前显示 0/3 个" : $"最近 30 天 · 未读 {NotificationUnreadCount} · 当前显示 {NotificationInboxItems.Count}/{NotificationInboxItems.Count} 个";')
    $fakeDataText = $fakeDataText.Replace('public bool NotificationInboxHasNoRecords => NotificationInboxItems.Count == 0;', 'public bool NotificationInboxHasNoRecords => NotificationInboxItems.Count == 0 && !string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase);')
    $fakeDataText = $fakeDataText.Replace('public bool NotificationInboxHasNoFilterResults => false;', 'public bool NotificationInboxHasNoFilterResults => string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase);')
    $selectedMarker = 'SelectedNotification = NotificationInboxItems.FirstOrDefault();'
    $selectedReplacement = $selectedMarker + $nl + '        if (string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_NOTIFICATION_NO_RESULTS"), "1", StringComparison.OrdinalIgnoreCase)) { NotificationInboxItems.Clear(); SelectedNotification = null; }'
    if (-not $fakeDataText.Contains($selectedMarker)) { throw 'Notification fixture initialization marker missing.' }
    $fakeDataText = $fakeDataText.Replace($selectedMarker,$selectedReplacement)
    if ($fakeDataText -notmatch 'IsLaunchingGameTool') {
        $busyProperty = '    public bool IsLaunchingGameTool => string.Equals(Environment.GetEnvironmentVariable("GSC_RENDER_TOOL_BUSY"), "1", StringComparison.OrdinalIgnoreCase);'
        $fakeDataText = [regex]::Replace(
            $fakeDataText,
            '(?m)^(public sealed class FakeDashboardData[^\r\n]*\r?\n\{)',
            ('$1' + "`r`n" + $busyProperty),
            1)
        Set-Content -LiteralPath $fakeDataPath -Value $fakeDataText -Encoding utf8
    }
    $programPath = Join-Path $source 'tests\GameSaveCenter.RenderHarness\Program.cs'
    $programText = Get-Content -LiteralPath $programPath -Raw
    $themeProbeNeedle = '                        ApplyThemePalette(view, themeMode);'
    $themeProbeReplacement = @'
                        if (name == "Maintenance" && view.FindName("MaintenanceDiagnosticsSubTabs") is TabControl diagnosticsTabs)
                            diagnosticsTabs.SelectedIndex = 1;
                        ApplyThemePalette(view, themeMode);
'@
    $themeLoopMarker = 'var label = $"{themeName}/{name}/{windowW}x{windowH}";'
    $themeLoopIndex = $programText.IndexOf($themeLoopMarker, [StringComparison]::Ordinal)
    if ($themeLoopIndex -lt 0) { throw 'Could not find the RenderHarness theme QA loop.' }
    $themeProbeIndex = $programText.IndexOf($themeProbeNeedle, $themeLoopIndex, [StringComparison]::Ordinal)
    if ($themeProbeIndex -lt 0) { throw 'Could not find the RenderHarness theme view setup for the focused maintenance state.' }
    $programText = $programText.Substring(0, $themeProbeIndex) + $themeProbeReplacement.TrimEnd() + $programText.Substring($themeProbeIndex + $themeProbeNeedle.Length)
    $trainerCaptureNeedle = '                        SavePng(host, Path.Combine(themeDir, $"{name}-{windowW}x{windowH}.png"));'
    $trainerCaptureReplacement = @'
                        if (name == "Trainer" && view.FindName("TrainerToolsSettingsScrollViewer") is ScrollViewer toolSettingsScroll)
                            toolSettingsScroll.ScrollToEnd();
                        host.UpdateLayout();
                        SavePng(host, Path.Combine(themeDir, $"{name}-{windowW}x{windowH}.png"));
'@
    if (-not $programText.Contains($trainerCaptureNeedle)) { throw 'Could not find the RenderHarness theme screenshot capture point.' }
    $programText = $programText.Replace($trainerCaptureNeedle, $trainerCaptureReplacement.TrimEnd())
    $maintenanceCaptureNeedle = '                        SavePng(host, Path.Combine(themeDir, $"{name}-{windowW}x{windowH}.png"));'
    $maintenanceCaptureReplacement = @'
                        SavePng(host, Path.Combine(themeDir, $"{name}-{windowW}x{windowH}.png"));
                        if (name == "Maintenance" && view.FindName("MaintenanceDiagnosticsOverviewScrollSurface") is ScrollViewer overviewScroll)
                        {
                            overviewScroll.ScrollToEnd();
                            host.UpdateLayout();
                            SavePng(host, Path.Combine(themeDir, $"{name}-overview-bottom-{windowW}x{windowH}.png"));
                            overviewScroll.ScrollToHome();
                            host.UpdateLayout();
                        }
'@
    if (-not $programText.Contains($maintenanceCaptureNeedle)) { throw 'Could not find the RenderHarness theme screenshot capture point for the maintenance scroll surface.' }
    $programText = $programText.Replace($maintenanceCaptureNeedle, $maintenanceCaptureReplacement.TrimEnd())
    Set-Content -LiteralPath $programPath -Value $programText -Encoding utf8
    Push-Location $source
    try {
        & $dotnet build 'src\GameSaveCenter.Worker\GameSaveCenter.Worker.csproj' -c Release -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false *> $workerLog
        $workerExit = $LASTEXITCODE
        if ($workerExit -ne 0) { throw "Worker build exit $workerExit" }
        & $dotnet build 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj' -c Release -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false *> $harnessLog
        $harnessExit = $LASTEXITCODE
        if ($harnessExit -ne 0) { throw "RenderHarness build exit $harnessExit" }
        $renderScript = '.\scripts\render-qa.ps1'
        $renderScriptText = Get-Content -LiteralPath $renderScript -Raw
        $renderExit = 0
        try {
            if ($renderScriptText -match '\[string\]\$SampleUtc') {
                & $renderScript -Configuration Release -Output $render -SampleUtc '2026-10-28T09:00:00Z' *> $renderLog
            }
            else {
                & $renderScript -Configuration Release -Output $render *> $renderLog
            }
        }
        catch {
            $renderExit = 1
            ($_ | Out-String) | Add-Content -LiteralPath $renderLog
        }
        if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { $renderExit = $LASTEXITCODE }
    }
    finally { Pop-Location }
    $reportPath = Join-Path $render 'render-qa-report.txt'
    if (-not (Test-Path -LiteralPath $reportPath)) { throw 'Render QA report missing.' }
    $report = Get-Content -LiteralPath $reportPath -Raw
    $pngs = @(Get-ChildItem -LiteralPath $render -Filter '*.png' -File)
    $workerPath = Get-ChildItem -LiteralPath (Join-Path $source 'src\GameSaveCenter.Worker\bin\Release') -Recurse -Filter 'GameSaveCenter.Worker.exe' -File | Select-Object -First 1 -ExpandProperty FullName
    $pluginPath = Get-ChildItem -LiteralPath (Join-Path $source 'src\GameSaveCenter.Playnite\bin\Release') -Recurse -Filter 'GameSaveCenter.Playnite.dll' -File | Select-Object -First 1 -ExpandProperty FullName
    $harnessPath = Get-ChildItem -LiteralPath (Join-Path $source 'tests\GameSaveCenter.RenderHarness\bin\Release') -Recurse -Filter 'GameSaveCenter.RenderHarness.exe' -File | Select-Object -First 1 -ExpandProperty FullName
    $assemblies = @{}
    foreach ($item in @(@{Name='worker';Path=$workerPath},@{Name='plugin';Path=$pluginPath},@{Name='renderHarness';Path=$harnessPath})) {
        if (Test-Path -LiteralPath $item.Path) { $assemblies[$item.Name] = (Get-FileHash -LiteralPath $item.Path -Algorithm SHA256).Hash }
        else { $assemblies[$item.Name] = $null }
    }
    $meta = [pscustomobject]@{
        label=$Label; commit=$Commit; sdk='8.0.423'; configuration='Release'; sampleUtc='2026-10-28T09:00:00Z'
        archiveBytes=$archiveBytes; archiveSha256=$archiveHash; archiveStartedUtc=$archiveStarted; archiveExpandedUtc=$archiveFinished
        fakeDashboardDataSha256=(Get-FileHash -LiteralPath $fakeDataPath -Algorithm SHA256).Hash
        fakeSampleClockSha256=(Get-FileHash -LiteralPath $fakeClockPath -Algorithm SHA256).Hash
        renderHarnessProbeSha256=(Get-FileHash -LiteralPath $programPath -Algorithm SHA256).Hash
        workerBuildExit=$workerExit; renderHarnessBuildExit=$harnessExit; renderQaExit=$renderExit; pngCount=$pngs.Count
        problemCount=@($report -split "`r?`n" | Where-Object { $_ -match '^\s*PROBLEM\b' }).Count; assemblySha256=$assemblies
        reportPath=$reportPath; reportSha256=(Get-FileHash -LiteralPath $reportPath -Algorithm SHA256).Hash
        retainedSource=[bool]$KeepSource
    }
    $meta | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $metaPath -Encoding utf8
    if (-not $KeepSource) { Remove-Item -LiteralPath $source -Recurse -Force }
    Write-Output ($meta | ConvertTo-Json -Depth 6)
}
catch {
    $failure = [pscustomobject]@{label=$Label;commit=$Commit;failedAt=(Get-Date).ToUniversalTime().ToString('o');error=$_.Exception.Message}
    $failure | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $snapshot 'failure.json') -Encoding utf8
    Write-Error $_
    exit 1
}
finally { Pop-Location }
