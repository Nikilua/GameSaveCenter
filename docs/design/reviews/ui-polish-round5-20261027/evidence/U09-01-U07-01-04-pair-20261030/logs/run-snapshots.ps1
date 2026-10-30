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
if (-not $evidencePath.StartsWith([IO.Path]::GetFullPath((Join-Path $workspacePath 'docs')), [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence root is outside repository docs: $evidencePath"
}

$sdk = Join-Path $workspacePath '.tmp\round5-sdk8-8.0.423'
$dotnet = Join-Path $sdk 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { throw "Required SDK8.0.423 executable missing: $dotnet" }
$env:DOTNET_ROOT = $sdk
$env:DOTNET_MULTILEVEL_LOOKUP = '0'
$env:PATH = $sdk + ';' + $env:PATH
$env:MSBuildEnableWorkloadResolver = 'false'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:GSC_RENDER_SAMPLE_UTC = '2026-10-28T09:00:00Z'

$probePath = Join-Path $evidencePath 'logs\U07PairProbe.cs'
$commits = @(
    [pscustomobject]@{ Label='U07-01-02-before'; Commit='ac0afe72'; Scenarios=@('dialog') },
    [pscustomobject]@{ Label='U07-01-02-after'; Commit='2aba614f'; Scenarios=@('dialog','tooltip','menu') },
    [pscustomobject]@{ Label='U07-03-04-after'; Commit='7c4d0775'; Scenarios=@('tooltip','menu') }
)

Push-Location $workspacePath
try {
    foreach ($item in $commits) {
        $commit = (& git rev-parse $item.Commit).Trim()
        if ($LASTEXITCODE -ne 0 -or $commit.Length -ne 40) { throw "Could not resolve historical commit $($item.Commit)." }
        $snapshotRoot = Join-Path $phasePath $item.Label
        $sourceRoot = Join-Path $snapshotRoot 'source'
        $archive = Join-Path $snapshotRoot ($item.Label + '.zip')
        $captureRoot = Join-Path $snapshotRoot 'captures'
        New-Item -ItemType Directory -Force -Path $snapshotRoot,$sourceRoot,$captureRoot | Out-Null
        $archiveLog = Join-Path $snapshotRoot 'archive.log'
        $workerLog = Join-Path $snapshotRoot 'worker-build.log'
        $harnessLog = Join-Path $snapshotRoot 'render-harness-build.log'
        $captureLog = Join-Path $snapshotRoot 'capture-run.log'
        $started = [DateTime]::UtcNow.ToString('o')
        & git archive --format=zip --output=$archive $commit *> $archiveLog
        $archiveExit = $LASTEXITCODE
        if ($archiveExit -ne 0) { throw "git archive failed for $($item.Label), exit $archiveExit." }
        $archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
        $archiveBytes = (Get-Item -LiteralPath $archive).Length
        Expand-Archive -LiteralPath $archive -DestinationPath $sourceRoot -Force
        Remove-Item -LiteralPath $archive -Force

        $fakeDataPath = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\FakeDashboardData.cs'
        $fakeData = Get-Content -LiteralPath $fakeDataPath -Raw
        $fakeData = $fakeData.Replace('DateTimeOffset.UtcNow', 'FakeSampleClock.SampleUtc')
        $fakeData = $fakeData.Replace('DateTime.UtcNow', 'FakeSampleClock.UtcNow')
        Set-Content -LiteralPath $fakeDataPath -Value $fakeData -Encoding utf8
        $clockPath = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\FakeSampleClock.cs'
        if (-not (Test-Path -LiteralPath $clockPath)) {
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
    public static DateTimeOffset SampleUtc => new DateTimeOffset(Sample, TimeSpan.Zero);
}
'@ | Set-Content -LiteralPath $clockPath -Encoding utf8
        }

        $programPath = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\Program.cs'
        $program = Get-Content -LiteralPath $programPath -Raw
        $marker = '(?m)^(    public static int Main\(string\[\] args\)\r?\n    \{)'
        if ([regex]::Matches($program, $marker).Count -ne 1) { throw "RenderHarness Main marker is missing or ambiguous in $($item.Label)." }
        $replacement = '$1' + "`r`n        if (args.Length > 0 && args[0].Equals(`"u07pairprobe`", StringComparison.OrdinalIgnoreCase))`r`n            return U07PairProbe.Run(args);"
        $program = [regex]::Replace($program, $marker, $replacement, 1)
        Set-Content -LiteralPath $programPath -Value $program -Encoding utf8
        Copy-Item -LiteralPath $probePath -Destination (Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\U07PairProbe.cs')

        $env:GSC_SOURCE_ROOT = $sourceRoot
        $env:GSC_BUILD_COMMIT = $commit
        Push-Location $sourceRoot
        try {
            & $dotnet 'build' 'src\GameSaveCenter.Worker\GameSaveCenter.Worker.csproj' '-c' 'Release' '-m:1' '-nodeReuse:false' '-p:NuGetAudit=false' '-p:MSBuildEnableWorkloadResolver=false' ("-p:GscSourceRoot=" + $sourceRoot) ("-p:GscBuildCommit=" + $commit) *> $workerLog
            $workerExit = $LASTEXITCODE
            if ($workerExit -ne 0) { throw "Worker build failed for $($item.Label), exit $workerExit. See $workerLog." }
            & $dotnet 'build' 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj' '-c' 'Release' '-m:1' '-nodeReuse:false' '-p:NuGetAudit=false' '-p:MSBuildEnableWorkloadResolver=false' ("-p:GscSourceRoot=" + $sourceRoot) ("-p:GscBuildCommit=" + $commit) *> $harnessLog
            $harnessExit = $LASTEXITCODE
            if ($harnessExit -ne 0) { throw "RenderHarness build failed for $($item.Label), exit $harnessExit. See $harnessLog." }

            $exe = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\bin\Release\net472\GameSaveCenter.RenderHarness.exe'
            if (-not (Test-Path -LiteralPath $exe)) { throw "RenderHarness executable missing for $($item.Label)." }
            $captureResults = @()
            foreach ($scenario in $item.Scenarios) {
                $sizeSpecs = if ($scenario -eq 'dialog') { @('1040x700','1366x768') } else { @('1040x700') }
                foreach ($sizeSpec in $sizeSpecs) {
                    $size = $sizeSpec -split 'x'
                    foreach ($theme in @('light','dark')) {
                        $beforePngs = @(Get-ChildItem -LiteralPath $captureRoot -Filter '*.png' -File | Select-Object -ExpandProperty Name)
                        & $exe 'u07pairprobe' $captureRoot $scenario $theme ([string]$size[0]) ([string]$size[1]) *> $captureLog
                        $captureExit = $LASTEXITCODE
                        if ($captureExit -ne 0) { throw "Targeted $scenario capture failed for $($item.Label)/$theme/$($size[0])x$($size[1]), exit $captureExit. See $captureLog." }
                        $afterPngs = @(Get-ChildItem -LiteralPath $captureRoot -Filter '*.png' -File | Select-Object -ExpandProperty Name)
                        $captureResults += [pscustomobject]@{
                            scenario=$scenario; theme=$theme; widthDip=$size[0]; heightDip=$size[1]; exitCode=$captureExit
                            newImages=@($afterPngs | Where-Object { $beforePngs -notcontains $_ })
                        }
                    }
                }
            }
        }
        finally { Pop-Location }

        $workerExe = Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'src\GameSaveCenter.Worker\bin\Release') -Recurse -Filter 'GameSaveCenter.Worker.exe' -File | Select-Object -First 1 -ExpandProperty FullName
        $harnessExe = Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\bin\Release\net472\GameSaveCenter.RenderHarness.exe'
        $metadata = [pscustomobject]@{
            label=$item.Label; sourceCommit=$commit; sdk='8.0.423'; configuration='Release'; sampleUtc=$env:GSC_RENDER_SAMPLE_UTC
            archiveBytes=$archiveBytes; archiveSha256=$archiveHash; startedUtc=$started; finishedUtc=[DateTime]::UtcNow.ToString('o')
            fakeDashboardDataSha256=(Get-FileHash -LiteralPath $fakeDataPath -Algorithm SHA256).Hash
            fakeSampleClockSha256=(Get-FileHash -LiteralPath $clockPath -Algorithm SHA256).Hash
            probeSha256=(Get-FileHash -LiteralPath (Join-Path $sourceRoot 'tests\GameSaveCenter.RenderHarness\U07PairProbe.cs') -Algorithm SHA256).Hash
            workerBuildExit=$workerExit; renderHarnessBuildExit=$harnessExit
            workerExeSha256=(Get-FileHash -LiteralPath $workerExe -Algorithm SHA256).Hash
            renderHarnessExeSha256=(Get-FileHash -LiteralPath $harnessExe -Algorithm SHA256).Hash
            captures=$captureResults; pngCount=@(Get-ChildItem -LiteralPath $captureRoot -Filter '*.png' -File).Count
        }
        $metadata | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $snapshotRoot 'snapshot.json') -Encoding utf8
        Write-Output ($metadata | ConvertTo-Json -Depth 5)
    }
}
finally { Pop-Location }
