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

    Write-Host "==> Rendering workspace views to $Output" -ForegroundColor Cyan
    & $exe $Output
    if ($LASTEXITCODE -ne 0) { throw "render harness failed: $LASTEXITCODE" }
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
