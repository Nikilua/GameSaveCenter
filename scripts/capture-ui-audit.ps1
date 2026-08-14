[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [string]$Output = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$harness = Join-Path $root 'tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj'
if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root 'artifacts\ui-audit'
}
$Output = [System.IO.Path]::GetFullPath($Output)
$zip = Join-Path $root 'artifacts\GameSaveCenter-ui-audit.zip'
$buildRoot = Join-Path $root ("artifacts\ui-audit-build\{0}\audit-{1}" -f $Configuration, [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null

# Never clean a path outside the repository's artifacts directory.
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $root 'artifacts'))
if (-not $Output.StartsWith($artifactsRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output must be inside artifacts: $Output"
}
if (Test-Path -LiteralPath $Output) {
    Remove-Item -LiteralPath $Output -Recurse -Force
}
New-Item -ItemType Directory -Path $Output -Force | Out-Null

Push-Location $root
try {
    Write-Host "==> Building UI audit harness ($Configuration)" -ForegroundColor Cyan
    & dotnet build $harness -c $Configuration -m:1 -nodeReuse:false -p:NuGetAudit=false -p:GscBuildOutputRoot=$buildRoot
    if ($LASTEXITCODE -ne 0) { throw "UI audit harness build failed: $LASTEXITCODE" }

    $exe = Join-Path $buildRoot "bin\GameSaveCenter.RenderHarness\$Configuration\net472\GameSaveCenter.RenderHarness.exe"
    if (-not (Test-Path -LiteralPath $exe)) { throw "UI audit harness exe missing: $exe" }

    try {
        $commit = & git -C $root rev-parse --short HEAD 2>$null
        if ($LASTEXITCODE -eq 0 -and $commit) {
            $env:GSC_UI_AUDIT_COMMIT = ($commit | Select-Object -First 1).Trim()
        }
    }
    catch {
        $env:GSC_UI_AUDIT_COMMIT = ''
    }

    Write-Host "==> Running UI audit -> $Output" -ForegroundColor Cyan
    & $exe audit $Output
    if ($LASTEXITCODE -ne 0) { throw "UI audit failed: $LASTEXITCODE" }

    if (-not (Test-Path -LiteralPath $zip)) {
        Write-Host "==> Compressing audit output" -ForegroundColor Cyan
        Compress-Archive -Path (Join-Path $Output '*') -DestinationPath $zip -Force
    }

    Write-Host "Audit output: $Output" -ForegroundColor Green
    Write-Host "ZIP: $zip" -ForegroundColor Green
    Write-Host "Summary: $(Join-Path $Output 'AUDIT_SUMMARY.md')" -ForegroundColor Green
}
finally {
    Pop-Location
}
