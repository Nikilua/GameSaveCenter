$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repoRoot 'scripts/preview-build-diagnostics.ps1') -DefineOnly
$summaryPath = Join-Path $repoRoot 'docs/ai/evidence/close-sdk-20261007/local-sdk8-full/summary.json'

$summary = [pscustomobject]@{
    runId = 'fixture-run-identity'
    sourceCommit = '0123456789abcdef'
    workingTreeClean = $false
    dirtyPaths = @('src/Views/Sample.xaml')
    environment = [pscustomobject]@{
        osVersion = '10.0.19045'; osBitness = 64; processBitness = 64
        powershellVersion = '7.5.0'; powershellEdition = 'Core'; dotnetRuntimeVersion = '9.0.0'
    }
    sdk = '8.0.423'; outcome = 'failed'; runType = 'preview-fixture'; testsRequested = $true
    steps = @([pscustomobject]@{ id = 'test'; exitCode = 1 })
    assemblies = @([pscustomobject]@{
        file = 'GameSaveCenter.Playnite.Tests.dll'; sha256 = 'dll-sha'; mvid = 'dll-mvid'
        pdb = [pscustomobject]@{ sha256 = 'pdb-sha'; unavailableReason = $null }
    })
    testSummary = [pscustomobject]@{
        trxFileCount = 1
        totals = [pscustomobject]@{
            testCaseCount = 4; passed = 2; failed = 1; skipped = 1; error = 0; timeout = 0; aborted = 0
            counterMismatchCount = 0; parseErrorCount = 0
        }
        groups = @([pscustomobject]@{ id = 'playnite-wpf-isolated'; testCaseCount = 4; distinctTestClassCount = 2; wpfIsolatedClassRunCount = 2 })
        files = @([pscustomobject]@{ path = 'wpf/Fixture.trx' })
    }
    failureMessage = 'do-not-display user@example.invalid'
}

$text = (Get-GscDiagnosticPreviewLines -Summary $summary -SummaryPath $summaryPath) -join "`n"
foreach ($expected in @(
    '运行标识：fixture-run-identity', '诊断入口：[repo]',
    '源码提交：0123456789abcdef', 'Windows 10.0.19045', 'PowerShell 7.5.0', 'SDK：8.0.423',
    '非零步骤：test=1', 'DLL SHA-256=dll-sha', 'PDB SHA-256=pdb-sha',
    'TRX 测试用例：4 total / 2 passed / 1 failed / 1 skipped',
    '2 classes / 2 WPF isolated class runs', 'TRX missing from this folder/artifact'
)) {
    if (-not $text.Contains($expected)) { throw "Diagnostic preview is missing expected field: $expected" }
}
if ($text.Contains('user@example.invalid') -or $text.Contains($repoRoot)) {
    throw 'Diagnostic preview exposed a failure detail or absolute repository path.'
}

# Large diagnostic detail must not displace the acceptance fields from the first screen.
$crowded = $summary | ConvertTo-Json -Depth 12 | ConvertFrom-Json
$crowded.dirtyPaths = @(1..100 | ForEach-Object { "src/fixture-$_.cs" })
$crowded.assemblies = @(1..100 | ForEach-Object { $summary.assemblies[0] })
$crowded.testSummary.files = @(1..100 | ForEach-Object { [pscustomobject]@{ path = "wpf/Fixture-$_.trx" } })
$crowdedLines = @(Get-GscDiagnosticPreviewLines -Summary $crowded -SummaryPath $summaryPath)
$firstScreen = ($crowdedLines | Select-Object -First 12) -join "`n"
foreach ($expected in @('fixture-run-identity', '诊断入口：[repo]', '源码提交：0123456789abcdef',
    '工作树：有 100 条改动路径', 'SDK：8.0.423', '运行结果：failed', '非零步骤：test=1',
    '4 total / 2 passed / 1 failed / 1 skipped', '发布、安装与启动结果须另查标准入口终态')) {
    if (-not $firstScreen.Contains($expected)) { throw "First screen lost a result field: $expected" }
}
foreach ($detail in @('src/fixture-100.cs', 'TRX missing from this folder/artifact：wpf/Fixture-100.trx', 'DLL SHA-256=dll-sha')) {
    if (-not (($crowdedLines -join "`n").Contains($detail))) { throw "Reordering lost diagnostic detail: $detail" }
}
$crowded.outcome = 'running'
$crowded.steps = @([pscustomobject]@{ id = 'test'; exitCode = $null })
$runningText = (Get-GscDiagnosticPreviewLines -Summary $crowded -SummaryPath $summaryPath) -join "`n"
if (-not $runningText.Contains('运行结果：running') -or $runningText.Contains('全部成功')) {
    throw 'An unfinished diagnostic run was presented as complete.'
}

$legacySummary = [pscustomobject]@{
    sourceCommit = 'old-commit'; workingTreeClean = $null; dirtyPaths = @(); sdk = 'unknown'
    outcome = 'failed'; runType = 'legacy'; testsRequested = $false; steps = @(); assemblies = @(); testSummary = $null
}
$legacyText = (Get-GscDiagnosticPreviewLines -Summary $legacySummary -SummaryPath $summaryPath) -join "`n"
foreach ($expected in @('运行标识：目录名：local-sdk8-full', '旧格式摘要没有 OS/PowerShell/位数记录', '没有身份记录', '本次没有请求测试')) {
    if (-not $legacyText.Contains($expected)) { throw "Diagnostic preview does not explain missing legacy input: $expected" }
}

$historicalSummaryPath = Join-Path $repoRoot 'docs/ai/evidence/close-sdk-20261007/local-sdk8-full/summary.json'
$historicalSummary = Get-Content -LiteralPath $historicalSummaryPath -Raw | ConvertFrom-Json
$historicalText = (Get-GscDiagnosticPreviewLines -Summary $historicalSummary -SummaryPath $historicalSummaryPath) -join "`n"
foreach ($expected in @(
    '旧 summary.json；以下计数由本机现存原始 TRX 只读复算',
    'TRX 测试用例：1073 total / 1054 passed / 1 failed / 18 skipped；TRX 文件 27',
    'playnite-wpf-isolated：103 cases / 24 classes / 24 WPF isolated class runs'
)) {
    if (-not $historicalText.Contains($expected)) { throw "Diagnostic preview did not recount historical TRX: $expected" }
}

Write-Host 'Diagnostic preview tests passed: identity, outcomes, WPF counts, missing artifacts, legacy omissions and redaction.'
