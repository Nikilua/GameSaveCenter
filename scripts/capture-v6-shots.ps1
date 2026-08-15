[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Output = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$harness = Join-Path $root 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj'
if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root 'artifacts\ui-qa\v6-shots'
}

Push-Location $root
try {
    Write-Host "==> Building render harness ($Configuration)" -ForegroundColor Cyan
    & dotnet build $harness -c $Configuration -m:1 -nodeReuse:false -p:NuGetAudit=false
    if ($LASTEXITCODE -ne 0) { throw "render harness build failed: $LASTEXITCODE" }

    $exe = Join-Path $root "tests\GameSaveCenter.RenderHarness\bin\$Configuration\net472\GameSaveCenter.RenderHarness.exe"
    if (-not (Test-Path $exe)) { throw "render harness exe missing: $exe" }

    Write-Host "==> Rendering v6 screenshots to $Output" -ForegroundColor Cyan
    & $exe v6shots $Output
    if ($LASTEXITCODE -ne 0) { throw "v6 screenshot capture failed: $LASTEXITCODE" }
    Write-Host "Report: $(Join-Path $Output 'v6-shots-report.txt')" -ForegroundColor Green
}
finally {
    Pop-Location
}
