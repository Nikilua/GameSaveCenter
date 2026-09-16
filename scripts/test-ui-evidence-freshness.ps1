[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$checkScript = Join-Path $PSScriptRoot 'check-ui-evidence-freshness.ps1'

function Invoke-Check {
    param(
        [string[]]$Arguments
    )
    return (& powershell -NoProfile -ExecutionPolicy Bypass -File $checkScript @Arguments -AsJson | ConvertFrom-Json)
}

$docOnly = Invoke-Check @(
    '-HeadCommit', 'HEAD',
    '-ChangedPath', 'docs/design/reviews/ui-finesse-round3-20260915/README.md'
)
if (-not $docOnly.documentationOnlyChange -or @($docOnly.records | Where-Object { $_.needsRerun }).Count -ne 0 -or @($docOnly.records | Where-Object { $_.reinstallRequired }).Count -ne 0) {
    throw 'documentation-only change must not invalidate evidence or require reinstall'
}

$shared = Invoke-Check @(
    '-HeadCommit', 'HEAD',
    '-ChangedPath', 'src/GameSaveCenter.Playnite/Themes/Redesign.xaml'
)
$sharedIds = @($shared.records | Where-Object { $_.needsRerun -and $_.scopes -contains 'shared-controls' } | Select-Object -ExpandProperty id)
if (-not ($sharedIds -contains 'R00-01-02') -or -not ($sharedIds -contains 'R00-05')) {
    throw 'shared control change did not invalidate shared-control evidence'
}
if (@($shared.records | Where-Object { $_.needsRerun -and $_.id -eq 'R00-06' }).Count -ne 0) {
    throw 'shared control change invalidated unrelated media evidence'
}

$fixturePath = Join-Path (Join-Path $root '.tmp') 'r01-07-package-fixture.json'
$baselinePath = Join-Path $root 'docs/design/reviews/ui-finesse-round3-20260915/UI_EVIDENCE_BASELINE.json'
$baseline = Get-Content -Raw -LiteralPath $baselinePath | ConvertFrom-Json
$baseline.records[0].packageCommit = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
[System.IO.File]::WriteAllText($fixturePath, ($baseline | ConvertTo-Json -Depth 8), (New-Object System.Text.UTF8Encoding($false)))
try {
    $package = Invoke-Check @(
        '-BaselinePath', $fixturePath,
        '-HeadCommit', 'HEAD',
        '-ChangedPath', 'src/GameSaveCenter.Playnite/Themes/Redesign.xaml',
        '-PackageCommit', 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
    )
    $packageRecord = @($package.records)[0]
    if ($packageRecord.packageStatus -ne 'mismatch' -or -not $packageRecord.reinstallRequired) {
        throw 'package identity mismatch did not require reinstall'
    }
    if ($package.currentSourceCommit -eq $package.currentPackageCommit) {
        throw 'source and package identities collapsed into one value'
    }
}
finally {
    if (Test-Path -LiteralPath $fixturePath) {
        Remove-Item -LiteralPath $fixturePath -Force
    }
}

Write-Output 'UI evidence freshness tests passed: docs-only, shared-control, package-identity'
