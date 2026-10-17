[CmdletBinding()]
param([string]$OutputRoot = (Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) '.tmp/build-diagnostics-tests'))
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repoRoot 'scripts/build-diagnostics.ps1')
$context = New-GscDiagnosticContext $repoRoot $OutputRoot
$context.RunType = 'build-diagnostics-tests'
$context.TestsRequested = $true
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
    # Reproduce duplicate PATH entries using disposable native executables.
    # Before selecting the first match, collection tried to launch both paths.
    $previousPath = [Environment]::GetEnvironmentVariable('PATH', 'Process')
    try {
        $firstDirectory = Join-Path $context.RunRoot 'native-first'
        $secondDirectory = Join-Path $context.RunRoot 'native-second'
        New-Item -ItemType Directory -Path $firstDirectory,$secondDirectory -Force | Out-Null
        $fixtureName = 'gsc-diagnostic-native-fixture.exe'
        Copy-Item -LiteralPath (Join-Path $env:SystemRoot 'System32/cmd.exe') -Destination (Join-Path $firstDirectory $fixtureName)
        Copy-Item -LiteralPath (Join-Path $env:SystemRoot 'System32/whoami.exe') -Destination (Join-Path $secondDirectory $fixtureName)
        [Environment]::SetEnvironmentVariable('PATH', "$firstDirectory;$secondDirectory;$previousPath", 'Process')
        if (@(Get-Command $fixtureName -CommandType Application).Count -lt 2) { throw 'Duplicate native command fixture is incomplete' }
        $duplicate = Invoke-GscRecordedCommand $context 'duplicate-native-command' $fixtureName @('/d','/c','exit 7')
        if ($duplicate.ExitCode -ne 7 -or $context.Steps[3].launchError) { throw 'Duplicate PATH command did not use the first executable' }
    }
    finally { [Environment]::SetEnvironmentVariable('PATH', $previousPath, 'Process') }
    $results = Join-Path $context.RunRoot 'tests'
    New-Item -ItemType Directory -Path $results -Force | Out-Null
    $trx = Join-Path $results 'controlled.trx'
    [IO.File]::WriteAllText($trx, '<TestRun name="private-user@private-host" runUser="private-host\private-user"><Deployment runDeploymentRoot="private-user_private-host"/><UnitTestResult computerName="private-host"><ErrorInfo><Message>Assert.Equal() Failure</Message><StackTrace>at ExampleTests.ControlledFailure():line 42</StackTrace></ErrorInfo><Path>' + [Security.SecurityElement]::Escape($env:GSC_DIAGNOSTIC_TEST_PATH) + '</Path></UnitTestResult></TestRun>')
    Complete-GscTestDiagnostics $context $results
    Complete-GscDiagnostics $context $false
    $summaryText = [IO.File]::ReadAllText((Join-Path $context.RunRoot 'summary.json'))
    $summary = $summaryText | ConvertFrom-Json
    if ($summary.outcome -ne 'failed' -or $summary.steps.Count -ne 4 -or $summary.steps[0].exitCode -ne 7) { throw 'Failure summary lost actual status' }
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

    $assemblyRoot = Join-Path $context.RunRoot 'assembly-fixture'
    $testAssemblyDirectory = Join-Path $assemblyRoot 'bin/GameSaveCenter.Playnite.Tests/Release'
    New-Item -ItemType Directory -Path $testAssemblyDirectory -Force | Out-Null
    $testAssemblyPath = Join-Path $testAssemblyDirectory 'GameSaveCenter.Playnite.Tests.dll'
    $testPdbPath = [IO.Path]::ChangeExtension($testAssemblyPath, '.pdb')
    [IO.File]::WriteAllText($testAssemblyPath, 'synthetic test assembly identity fixture')
    [IO.File]::WriteAllText($testPdbPath, 'synthetic portable PDB identity fixture')
    $expectedPdbHash = (Get-FileHash -LiteralPath $testPdbPath -Algorithm SHA256).Hash
    Set-GscDiagnosticAssemblies $context $assemblyRoot 'Release'
    $pdbSummaryText = [IO.File]::ReadAllText((Join-Path $context.RunRoot 'summary.json'))
    $pdbSummary = $pdbSummaryText | ConvertFrom-Json
    $assemblyRecord = @($pdbSummary.assemblies | Where-Object { $_.file -eq 'GameSaveCenter.Playnite.Tests.dll' } | Select-Object -First 1)[0]
    if (-not $assemblyRecord -or $assemblyRecord.pdb.sha256 -ne $expectedPdbHash -or $assemblyRecord.pdb.bytes -ne (Get-Item -LiteralPath $testPdbPath).Length) {
        throw 'Serialized assembly diagnostics did not record the sibling PDB identity.'
    }
    if (-not $assemblyRecord.pdb.path.Contains('[repo]') -or $pdbSummaryText.Contains($repoRoot)) {
        throw 'Sibling PDB diagnostics did not redact the repository path.'
    }

    $pdbLock = [IO.File]::Open($testPdbPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
        Set-GscDiagnosticAssemblies $context $assemblyRoot 'Release'
        $lockedSummary = Get-Content -LiteralPath (Join-Path $context.RunRoot 'summary.json') -Raw | ConvertFrom-Json
        $lockedAssembly = @($lockedSummary.assemblies | Where-Object { $_.file -eq 'GameSaveCenter.Playnite.Tests.dll' } | Select-Object -First 1)[0]
        if (-not $lockedAssembly -or $lockedAssembly.pdb.sha256 -or -not $lockedAssembly.pdb.unavailableReason) {
            throw 'An unreadable PDB should preserve the DLL record and report a non-fatal identity error.'
        }
    }
    finally { $pdbLock.Dispose() }

    Remove-Item -LiteralPath $testPdbPath -Force
    Set-GscDiagnosticAssemblies $context $assemblyRoot 'Release'
    $assemblyWithoutPdb = @($context.Assemblies | Where-Object { $_.file -eq 'GameSaveCenter.Playnite.Tests.dll' } | Select-Object -First 1)[0]
    if (-not $assemblyWithoutPdb -or $null -ne $assemblyWithoutPdb.pdb) {
        throw 'Assembly diagnostic summary should report a missing sibling PDB as null.'
    }
    Write-Host "Diagnostics tests passed: real exits 7/0, duplicate PATH precedence, failure details, JSON/TRX redaction and present/locked/missing PDB identity. Evidence: $($context.RunRoot)"
}
finally { [Environment]::SetEnvironmentVariable('GSC_DIAGNOSTIC_TEST_PATH', $previous, 'Process') }
# Controlled native failures are expected inputs. Report the selftest's result,
# not their last exit code inherited by GitHub Actions' pwsh wrapper.
exit 0
