[CmdletBinding()]
param([string]$OutputRoot = (Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) '.tmp/build-diagnostics-tests'))
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repoRoot 'scripts/build-diagnostics.ps1')
$context = New-GscDiagnosticContext $repoRoot $OutputRoot
$engine = (Get-Process -Id $PID).Path
$child = Join-Path $context.RunRoot 'controlled-failure.ps1'
[IO.File]::WriteAllText($child, @'
Write-Output 'Failed ExampleTests.ControlledFailure'
Write-Output 'Assert.Equal() Failure: Expected: 1 Actual: 2'
[Console]::Error.WriteLine('Stack Trace: at ExampleTests.ControlledFailure():line 42')
Write-Output $env:GSC_DIAGNOSTIC_TEST_PATH
Write-Output 'https://alice:sample-secret@example.invalid/test?token=sample-token&ok=true'
exit 7
'@)
$previous = [Environment]::GetEnvironmentVariable('GSC_DIAGNOSTIC_TEST_PATH', 'Process')
try {
    $env:GSC_DIAGNOSTIC_TEST_PATH = "$repoRoot $env:USERPROFILE"
    $result = Invoke-GscRecordedCommand $context 'controlled-failure' $engine @('-NoProfile', '-File', $child)
    if ($result.ExitCode -ne 7) { throw "Native exit code changed: $($result.ExitCode)" }
    $passed = Invoke-GscRecordedCommand $context 'controlled-success' $engine @('-NoProfile', '-Command', 'Write-Output ''passed''; exit 0')
    if ($passed.ExitCode -ne 0) { throw 'Success exit code changed' }
    $missing = Invoke-GscRecordedCommand $context 'missing-command' 'gsc-nonexistent-command-for-diagnostic-test.exe' @()
    if ($missing.ExitCode -ne -1 -or -not $context.Steps[2].launchError) { throw 'Missing executable inherited a successful exit code' }
    $results = Join-Path $context.RunRoot 'tests'
    New-Item -ItemType Directory -Path $results -Force | Out-Null
    $trx = Join-Path $results 'controlled.trx'
    [IO.File]::WriteAllText($trx, '<TestRun name="private-user@private-host"><UnitTestResult computerName="private-host"><ErrorInfo><Message>Assert.Equal() Failure</Message><StackTrace>at ExampleTests.ControlledFailure():line 42</StackTrace></ErrorInfo><Path>' + [Security.SecurityElement]::Escape($env:GSC_DIAGNOSTIC_TEST_PATH) + '</Path></UnitTestResult></TestRun>')
    Complete-GscTestDiagnostics $context $results
    Complete-GscDiagnostics $context $false
    $summaryText = [IO.File]::ReadAllText((Join-Path $context.RunRoot 'summary.json'))
    $summary = $summaryText | ConvertFrom-Json
    if ($summary.outcome -ne 'failed' -or $summary.steps.Count -ne 3 -or $summary.steps[0].exitCode -ne 7) { throw 'Failure summary lost actual status' }
    $log = [IO.File]::ReadAllText((Join-Path $context.RunRoot 'controlled-failure-console.txt'))
    foreach ($expected in @('ExampleTests.ControlledFailure', 'Assert.Equal()', 'Expected: 1 Actual: 2', 'line 42', '[repo]', '[user-profile]', '[redacted]')) {
        if (-not $log.Contains($expected)) { throw "Missing diagnostic evidence: $expected" }
    }
    $trxText = [IO.File]::ReadAllText($trx)
    [xml]$xml = $trxText
    if ($xml.TestRun.UnitTestResult.computerName -ne 'redacted-host') { throw 'TRX host was not redacted' }
    foreach ($content in @($summaryText, $log, $trxText)) {
        foreach ($private in @($repoRoot, $repoRoot.Replace('\', '\\'), $env:USERPROFILE, 'sample-secret', 'sample-token', 'private-host', 'private-user')) {
            if ($private -and $content.Contains($private)) { throw "Diagnostic redaction failed for a private value" }
        }
    }
    Write-Host "Diagnostics tests passed: real exits 7/0, failure details, JSON and TRX redaction. Evidence: $($context.RunRoot)"
}
finally { [Environment]::SetEnvironmentVariable('GSC_DIAGNOSTIC_TEST_PATH', $previous, 'Process') }
