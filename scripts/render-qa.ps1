[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Output = '',
    [string]$SampleUtc = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$harness = Join-Path $root 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj'
$previousSampleUtc = [Environment]::GetEnvironmentVariable('GSC_RENDER_SAMPLE_UTC', 'Process')
if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root 'artifacts\ui-qa\render'
}

Push-Location $root
try {
    . (Join-Path $PSScriptRoot 'sdk-baseline.ps1')
    Initialize-GscSdkBaseline -ProjectRoot $root | Out-Null

    Write-Host "==> Building render harness ($Configuration)" -ForegroundColor Cyan
    & dotnet build $harness -c $Configuration -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false
    if ($LASTEXITCODE -ne 0) { throw "render harness build failed: $LASTEXITCODE" }

    $exe = Join-Path $root "tests\GameSaveCenter.RenderHarness\bin\$Configuration\net472\GameSaveCenter.RenderHarness.exe"
    if (-not (Test-Path $exe)) { throw "render harness exe missing: $exe" }

    if ([string]::IsNullOrWhiteSpace($SampleUtc)) {
        $sampleTime = [DateTimeOffset]::UtcNow
    }
    else {
        try {
            $sampleTime = [DateTimeOffset]::Parse(
                $SampleUtc,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::AssumeUniversal)
        }
        catch {
            throw "SampleUtc must be a valid ISO-8601 timestamp: $SampleUtc"
        }
    }
    $env:GSC_RENDER_SAMPLE_UTC = $sampleTime.ToUniversalTime().ToString('O', [Globalization.CultureInfo]::InvariantCulture)
    Write-Host "Sample data UTC: $env:GSC_RENDER_SAMPLE_UTC" -ForegroundColor Cyan

    $outputPath = [System.IO.Path]::GetFullPath($Output)
    Write-Host "==> Rendering workspace views to $outputPath" -ForegroundColor Cyan

    . (Join-Path $PSScriptRoot 'windows-desktop-process.ps1')
    . (Join-Path $PSScriptRoot 'bounded-process.ps1')
    $renderExitCode = 0
    $windowStation = [GscHiddenDesktopProcess]::CurrentWindowStation()
    if ([string]::Equals($windowStation, 'WinSta0', [StringComparison]::OrdinalIgnoreCase)) {
        $inputDesktopBefore = [GscHiddenDesktopProcess]::InputDesktop()
        $renderDesktop = 'GscRenderQA_' + [Guid]::NewGuid().ToString('N')
        $desktopResult = Invoke-GscHiddenDesktopProcess `
            -Application $exe `
            -Arguments @($outputPath) `
            -WorkingDirectory $root `
            -TimeoutSeconds 1800 `
            -DesktopName $renderDesktop

        if ($desktopResult.TimedOut) {
            $ownedProcess = $null
            try {
                $ownedProcess = [System.Diagnostics.Process]::GetProcessById([int]$desktopResult.ProcessId)
                Stop-GscOwnedProcessTree $ownedProcess
            }
            finally {
                if ($ownedProcess) { $ownedProcess.Dispose() }
            }
            throw "Render harness exceeded 1800 seconds on WinSta0\$renderDesktop."
        }
        if (-not $desktopResult.Exited) {
            throw "Render harness did not exit on WinSta0\$renderDesktop."
        }

        $inputDesktopAfter = [GscHiddenDesktopProcess]::InputDesktop()
        if ($desktopResult.WindowStation -ne 'WinSta0' -or
            $desktopResult.Desktop -ne $renderDesktop -or
            $desktopResult.InputDesktopBefore -ne $inputDesktopBefore -or
            $desktopResult.InputDesktopAfter -ne $inputDesktopBefore -or
            $inputDesktopAfter -ne $inputDesktopBefore) {
            throw "Render harness desktop isolation failed. Input desktop before='$inputDesktopBefore', runner before='$($desktopResult.InputDesktopBefore)', runner after='$($desktopResult.InputDesktopAfter)', parent after='$inputDesktopAfter', runner desktop='$($desktopResult.WindowStation)\$($desktopResult.Desktop)'."
        }

        $renderExitCode = [int]$desktopResult.ExitCode
        Write-Host "Render harness desktop: WinSta0\$renderDesktop; input desktop remained $inputDesktopBefore." -ForegroundColor DarkCyan
    }
    else {
        # Service and CI sessions do not have an interactive user desktop. Keep
        # their established execution path instead of creating an unavailable WinSta0 desktop.
        & $exe $outputPath
        $renderExitCode = $LASTEXITCODE
    }

    if ($renderExitCode -ne 0) { throw "render harness failed: $renderExitCode" }
    Write-Host "PNG report: $(Join-Path $Output 'render-qa-report.txt')" -ForegroundColor Green
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
