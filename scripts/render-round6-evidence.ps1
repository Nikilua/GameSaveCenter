[CmdletBinding()]
param(
    [ValidateSet('Baseline', 'StateFixtures')]
    [string]$Scenario = 'Baseline',
    [string]$Output = '',
    [string]$SampleUtc = '2026-11-13T00:00:00Z'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj'
$scenarioName = switch ($Scenario) {
    'Baseline' { 'round6baseline' }
    'StateFixtures' { 'statefixtures' }
}
if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root ".tmp\round6-$($Scenario.ToLowerInvariant())"
}

Push-Location $root
$previousSampleUtc = [Environment]::GetEnvironmentVariable('GSC_RENDER_SAMPLE_UTC', 'Process')
try {
    . (Join-Path $PSScriptRoot 'sdk-baseline.ps1')
    Initialize-GscSdkBaseline -ProjectRoot $root | Out-Null

    Write-Host "==> Building render harness (Release)" -ForegroundColor Cyan
    & dotnet build $project -c Release -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false
    if ($LASTEXITCODE -ne 0) { throw "render harness build failed: $LASTEXITCODE" }

    $exe = Join-Path $root 'tests\GameSaveCenter.RenderHarness\bin\Release\net472\GameSaveCenter.RenderHarness.exe'
    if (-not (Test-Path -LiteralPath $exe)) { throw "render harness executable missing: $exe" }
    $outputPath = [System.IO.Path]::GetFullPath($Output)
    [System.IO.Directory]::CreateDirectory($outputPath) | Out-Null
    $sampleTime = [DateTimeOffset]::Parse(
        $SampleUtc,
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::AssumeUniversal).ToUniversalTime()
    $env:GSC_RENDER_SAMPLE_UTC = $sampleTime.ToString('O', [Globalization.CultureInfo]::InvariantCulture)
    Write-Host "==> Running $Scenario in an isolated WPF render session" -ForegroundColor Cyan
    Write-Host "Sample data UTC: $env:GSC_RENDER_SAMPLE_UTC" -ForegroundColor Cyan

    . (Join-Path $PSScriptRoot 'windows-desktop-process.ps1')
    . (Join-Path $PSScriptRoot 'bounded-process.ps1')
    $desktopResult = $null
    $inputDesktopBefore = $null
    $inputDesktopAfter = $null
    $renderExitCode = 0
    $windowStation = [GscHiddenDesktopProcess]::CurrentWindowStation()
    if ([string]::Equals($windowStation, 'WinSta0', [StringComparison]::OrdinalIgnoreCase)) {
        $inputDesktopBefore = [GscHiddenDesktopProcess]::InputDesktop()
        $desktopName = 'GscRound6_' + [Guid]::NewGuid().ToString('N')
        $desktopResult = Invoke-GscHiddenDesktopProcess `
            -Application $exe `
            -Arguments @($scenarioName, $outputPath) `
            -WorkingDirectory $root `
            -TimeoutSeconds 1800 `
            -DesktopName $desktopName
        if ($desktopResult.TimedOut) {
            $ownedProcess = $null
            try {
                $ownedProcess = [System.Diagnostics.Process]::GetProcessById([int]$desktopResult.ProcessId)
                Stop-GscOwnedProcessTree $ownedProcess
            }
            finally {
                if ($ownedProcess) { $ownedProcess.Dispose() }
            }
            throw "$Scenario render exceeded 1800 seconds on WinSta0\$desktopName."
        }
        if (-not $desktopResult.Exited) { throw "$Scenario render did not exit on WinSta0\$desktopName." }
        $inputDesktopAfter = [GscHiddenDesktopProcess]::InputDesktop()
        if ($desktopResult.WindowStation -ne 'WinSta0' -or
            $desktopResult.Desktop -ne $desktopName -or
            $desktopResult.InputDesktopBefore -ne $inputDesktopBefore -or
            $desktopResult.InputDesktopAfter -ne $inputDesktopBefore -or
            $inputDesktopAfter -ne $inputDesktopBefore) {
            throw "Render isolation failed. Input desktop before='$inputDesktopBefore', runner before='$($desktopResult.InputDesktopBefore)', runner after='$($desktopResult.InputDesktopAfter)', parent after='$inputDesktopAfter', runner desktop='$($desktopResult.WindowStation)\$($desktopResult.Desktop)'."
        }
        $renderExitCode = [int]$desktopResult.ExitCode
        Write-Host "Render desktop: WinSta0\$desktopName; input desktop remained $inputDesktopBefore." -ForegroundColor DarkCyan
    }
    else {
        & $exe $scenarioName $outputPath
        $renderExitCode = $LASTEXITCODE
    }
    $metadata = [ordered]@{
        scenario = $Scenario
        harnessMode = $scenarioName
        sourceSha = (& git rev-parse HEAD).Trim()
        sampleUtc = $env:GSC_RENDER_SAMPLE_UTC
        windowStation = if ($desktopResult) { $desktopResult.WindowStation } else { $windowStation }
        desktop = if ($desktopResult) { $desktopResult.Desktop } else { $null }
        inputDesktopBefore = $inputDesktopBefore
        inputDesktopAfter = $inputDesktopAfter
        exitCode = $renderExitCode
        output = $outputPath
    }
    $metadata | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $outputPath 'runner-metadata.json') -Encoding utf8
    if ($renderExitCode -ne 0) { throw "$Scenario render failed: $renderExitCode" }
    Write-Host "Evidence output: $outputPath" -ForegroundColor Green
}
finally {
    if ($null -eq $previousSampleUtc) {
        Remove-Item Env:\GSC_RENDER_SAMPLE_UTC -ErrorAction SilentlyContinue
    }
    else {
        $env:GSC_RENDER_SAMPLE_UTC = $previousSampleUtc
    }
    Pop-Location
}
