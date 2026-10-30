[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Workspace,
    [Parameter(Mandatory=$true)][string]$PhaseRoot,
    [Parameter(Mandatory=$true)][string]$EvidenceRoot
)

$ErrorActionPreference = 'Stop'
$workspacePath = (Resolve-Path -LiteralPath $Workspace).Path
$phasePath = [IO.Path]::GetFullPath($PhaseRoot)
$evidencePath = [IO.Path]::GetFullPath($EvidenceRoot)
$expectedTempRoot = [IO.Path]::GetFullPath((Join-Path $workspacePath '.tmp'))
if (-not $phasePath.StartsWith($expectedTempRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Phase root is outside repository .tmp: $phasePath"
}

$sdk = Join-Path $workspacePath '.tmp\round5-sdk8-8.0.423'
$dotnet = Join-Path $sdk 'dotnet.exe'
$probePath = Join-Path $evidencePath 'logs\U07PairProbe.cs'
$env:DOTNET_ROOT = $sdk
$env:DOTNET_MULTILEVEL_LOOKUP = '0'
$env:PATH = $sdk + ';' + $env:PATH
$env:MSBuildEnableWorkloadResolver = 'false'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:GSC_RENDER_SAMPLE_UTC = '2026-10-28T09:00:00Z'

$items = @(
    [pscustomobject]@{ Label='U07-01-02-before'; Scenarios=@('dialog') },
    [pscustomobject]@{ Label='U07-01-02-after'; Scenarios=@('dialog','tooltip','menu') },
    [pscustomobject]@{ Label='U07-03-04-after'; Scenarios=@('tooltip','menu') }
)

foreach ($item in $items) {
    $snapshotRoot = Join-Path $phasePath $item.Label
    $sourceRoot = Join-Path $snapshotRoot 'source'
    $snapshotMetaPath = Join-Path $snapshotRoot 'snapshot.json'
    if (-not (Test-Path -LiteralPath $sourceRoot) -or -not (Test-Path -LiteralPath $snapshotMetaPath)) {
        throw "Completed historical snapshot is missing: $($item.Label)"
    }
    $snapshotMeta = Get-Content -LiteralPath $snapshotMetaPath -Raw | ConvertFrom-Json
    $commit = $snapshotMeta.sourceCommit
    $probeDestination = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\U07PairProbe.cs'
    Copy-Item -LiteralPath $probePath -Destination $probeDestination -Force
    $programPath = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\Program.cs'
    if ((Get-Content -LiteralPath $programPath -Raw) -notmatch 'U07PairProbe\.Run\(args\)') {
        throw "Historical RenderHarness dispatch is missing for $($item.Label)."
    }
    $env:GSC_SOURCE_ROOT = $sourceRoot
    $env:GSC_BUILD_COMMIT = $commit
    $captureRoot = Join-Path $snapshotRoot 'captures-final-v2'
    if (Test-Path -LiteralPath $captureRoot) { throw "Final capture directory already exists: $captureRoot" }
    New-Item -ItemType Directory -Force -Path $captureRoot | Out-Null
    $harnessLog = Join-Path $snapshotRoot 'render-harness-final-build.log'

    Push-Location $sourceRoot
    try {
        & $dotnet 'build' 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj' '-c' 'Release' '-m:1' '-nodeReuse:false' '-p:NuGetAudit=false' '-p:MSBuildEnableWorkloadResolver=false' ("-p:GscSourceRoot=" + $sourceRoot) ("-p:GscBuildCommit=" + $commit) *> $harnessLog
        $harnessExit = $LASTEXITCODE
        if ($harnessExit -ne 0) { throw "Final RenderHarness build failed for $($item.Label), exit $harnessExit. See $harnessLog." }

        $exe = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\bin\Release\net472\GameSaveCenter.RenderHarness.exe'
        $captureResults = @()
        foreach ($scenario in $item.Scenarios) {
            $sizeSpecs = if ($scenario -eq 'dialog') { @('1040x700','1366x768') } else { @('1040x700') }
            foreach ($sizeSpec in $sizeSpecs) {
                $size = $sizeSpec -split 'x'
                foreach ($theme in @('light','dark')) {
                    $captureLog = Join-Path $snapshotRoot ("capture-final-$scenario-$theme-$sizeSpec.log")
                    & $exe 'u07pairprobe' $captureRoot $scenario $theme $size[0] $size[1] *> $captureLog
                    $captureExit = $LASTEXITCODE
                    if ($captureExit -ne 0) { throw "Final $scenario capture failed for $($item.Label)/$theme/$sizeSpec, exit $captureExit. See $captureLog." }
                    $captureResults += [pscustomobject]@{
                        scenario=$scenario; theme=$theme; widthDip=[int]$size[0]; heightDip=[int]$size[1]; exitCode=$captureExit
                    }
                }
            }
        }
    }
    finally { Pop-Location }

    $harnessExe = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\bin\Release\net472\GameSaveCenter.RenderHarness.exe'
    $productionFiles = @(
        'src\GameSaveCenter.Playnite\Views\DashboardView.xaml',
        'src\GameSaveCenter.Playnite\Infrastructure\GscToolTipBehavior.cs',
        'src\GameSaveCenter.Playnite\Infrastructure\DataGridColumnVisibilityController.cs',
        'src\GameSaveCenter.Playnite\Themes\DesignTokens.xaml',
        'src\GameSaveCenter.Playnite\Themes\WpfUiProduction.xaml',
        'src\GameSaveCenter.Playnite\Themes\Redesign.xaml',
        'src\GameSaveCenter.Playnite\Themes\AcrylicReferenceControls.xaml'
    )
    $sourceHashes = @{}
    foreach ($relative in $productionFiles) {
        $file = Join-Path $sourceRoot $relative
        if (Test-Path -LiteralPath $file) { $sourceHashes[$relative.Replace('\','/')] = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash }
    }
    $images = @(Get-ChildItem -LiteralPath $captureRoot -Filter '*.png' -File | Sort-Object Name | ForEach-Object {
        [pscustomobject]@{ name=$_.Name; bytes=$_.Length; sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    $reports = @(Get-ChildItem -LiteralPath $captureRoot -Filter '*-measurements.txt' -File | Sort-Object Name | ForEach-Object {
        [pscustomobject]@{ name=$_.Name; sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    $manifest = [pscustomobject]@{
        label=$item.Label; sourceCommit=$commit; sdk='8.0.423'; configuration='Release'; sampleUtc=$env:GSC_RENDER_SAMPLE_UTC
        archiveBytes=$snapshotMeta.archiveBytes; archiveSha256=$snapshotMeta.archiveSha256
        workerBuildExit=$snapshotMeta.workerBuildExit; renderHarnessFinalBuildExit=$harnessExit
        workerExeSha256=$snapshotMeta.workerExeSha256; renderHarnessFinalExeSha256=(Get-FileHash -LiteralPath $harnessExe -Algorithm SHA256).Hash
        fakeDashboardDataSha256=$snapshotMeta.fakeDashboardDataSha256; fakeSampleClockSha256=$snapshotMeta.fakeSampleClockSha256
        probeSha256=(Get-FileHash -LiteralPath $probeDestination -Algorithm SHA256).Hash
        productionSourceSha256=$sourceHashes; captures=$captureResults; pngCount=$images.Count; images=$images; measurementReports=$reports
        neutralBaseTextBlockStyleFallback='Application.Resources BaseTextBlockStyle = new Style(typeof(TextBlock)); no Playnite host loaded'
        renderHarnessBuildLogSha256=(Get-FileHash -LiteralPath $harnessLog -Algorithm SHA256).Hash
    }
    $manifestPath = Join-Path $snapshotRoot 'captures-final-v2.json'
    $manifest | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath $manifestPath -Encoding utf8
    Write-Output ($manifest | ConvertTo-Json -Depth 5)
}
