$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repoRoot 'scripts/package-dirty-state.ps1')

$fixture = @(
    '?? docs/design/reviews/example/evidence/tests/run.trx',
    '?? tests/GameSaveCenter.Playnite.Tests/TestResults/current.trx',
    '?? tests/GameSaveCenter.Playnite.Tests/TestResultsExtra/keep.cs',
    ' M docs/notes.md',
    ' M src/GameSaveCenter.Playnite/Views/Page.xaml',
    '?? src/GameSaveCenter.Playnite/Views/NewPage.xaml'
)
$expected = @(
    '?? tests/GameSaveCenter.Playnite.Tests/TestResultsExtra/keep.cs',
    ' M docs/notes.md',
    ' M src/GameSaveCenter.Playnite/Views/Page.xaml',
    '?? src/GameSaveCenter.Playnite/Views/NewPage.xaml'
)
$actual = @(Get-GscPackageRelevantDirtyFiles $fixture)
if (($actual -join "`n") -cne ($expected -join "`n")) {
    throw "Package dirty-state regression: expected [$($expected -join '; ')], got [$($actual -join '; ')]"
}

Write-Host 'Package dirty-state tests passed: generated evidence is ignored; source changes remain blocking.' -ForegroundColor Green
