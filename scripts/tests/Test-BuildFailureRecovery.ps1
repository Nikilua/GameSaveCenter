[CmdletBinding()]
param([string]$EvidenceRoot = '')

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$evidence = if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-08'
}
else {
    [IO.Path]::GetFullPath($EvidenceRoot)
}
$repoPrefix = [IO.Path]::GetFullPath($repo).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if (-not $evidence.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "EvidenceRoot must be inside the repository: $evidence"
}
if ((Test-Path -LiteralPath $evidence -PathType Leaf) -or
    ((Test-Path -LiteralPath $evidence -PathType Container) -and @(Get-ChildItem -LiteralPath $evidence -Force).Count -gt 0)) {
    throw "Refusing to overwrite existing recovery evidence: $evidence"
}

$fixture = Join-Path $repo ('.tmp/t00-08-build-recovery-' + [guid]::NewGuid().ToString('N'))
$shimRoot = Join-Path $fixture 'shim'
$diagnosticsRoot = Join-Path $fixture 'diagnostics'
$outputRoot = Join-Path $fixture 'output'
$markerPath = Join-Path $fixture 'first-test-failed.marker'
$fakeDotNet = Join-Path $shimRoot 'dotnet.cmd'
New-Item -ItemType Directory -Path $shimRoot, $diagnosticsRoot, $outputRoot -Force | Out-Null

$shimLines = @(
    '@echo off',
    'setlocal',
    'if "%~1"=="--version" (',
    '  echo 8.0.423',
    '  exit /b 0',
    ')',
    'if "%~1"=="--list-sdks" (',
    '  echo 8.0.423 [T00-08 controlled dotnet shim]',
    '  exit /b 0',
    ')',
    'if "%~1"=="test" goto test',
    'echo controlled dotnet success: %*',
    'exit /b 0',
    ':test',
    'set "GSC_T00_08_ARGS=%*"',
    'echo(%GSC_T00_08_ARGS%|%SystemRoot%\System32\findstr.exe /C:"--list-tests" >nul',
    'if not errorlevel 1 goto list_tests',
    'if not exist "%GSC_T00_08_MARKER%" (',
    '  >"%GSC_T00_08_MARKER%" echo first controlled test failed',
    '  echo controlled test failure, exit 23',
    '  exit /b 23',
    ')',
    'echo controlled test success',
    'exit /b 0',
    ':list_tests',
    'echo The following Tests are available:',
    'echo   GameSaveCenter.Playnite.Tests.R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt',
    'exit /b 0'
)
[IO.File]::WriteAllText($fakeDotNet, ($shimLines -join "`r`n") + "`r`n", [Text.Encoding]::ASCII)

$previous = @{
    Path = [Environment]::GetEnvironmentVariable('PATH', 'Process')
    Temp = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
    Tmp = [Environment]::GetEnvironmentVariable('TMP', 'Process')
    DotNetRoot = [Environment]::GetEnvironmentVariable('DOTNET_ROOT', 'Process')
    DotNetRootX64 = [Environment]::GetEnvironmentVariable('DOTNET_ROOT_X64', 'Process')
    BuildCommit = [Environment]::GetEnvironmentVariable('GSC_BUILD_COMMIT', 'Process')
    SourceRoot = [Environment]::GetEnvironmentVariable('GSC_SOURCE_ROOT', 'Process')
    FailureMarker = [Environment]::GetEnvironmentVariable('GSC_T00_08_MARKER', 'Process')
}
$outerTemp = Join-Path $fixture 'outer-temp'
New-Item -ItemType Directory -Path $outerTemp -Force | Out-Null
$env:PATH = $shimRoot + ';' + $previous.Path
$env:TEMP = $outerTemp
$env:TMP = $outerTemp
$env:DOTNET_ROOT = 'T00-08-DOTNET_ROOT-SENTINEL'
$env:DOTNET_ROOT_X64 = 'T00-08-DOTNET_ROOT_X64-SENTINEL'
$env:GSC_BUILD_COMMIT = 'T00-08-BUILD-COMMIT-SENTINEL'
$env:GSC_SOURCE_ROOT = 'T00-08-SOURCE-ROOT-SENTINEL'
$env:GSC_T00_08_MARKER = $markerPath
$expectedBuildEnvironment = @{
    Path = [Environment]::GetEnvironmentVariable('PATH', 'Process')
    Temp = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
    Tmp = [Environment]::GetEnvironmentVariable('TMP', 'Process')
    DotNetRoot = [Environment]::GetEnvironmentVariable('DOTNET_ROOT', 'Process')
    DotNetRootX64 = [Environment]::GetEnvironmentVariable('DOTNET_ROOT_X64', 'Process')
    BuildCommit = [Environment]::GetEnvironmentVariable('GSC_BUILD_COMMIT', 'Process')
    SourceRoot = [Environment]::GetEnvironmentVariable('GSC_SOURCE_ROOT', 'Process')
    FailureMarker = [Environment]::GetEnvironmentVariable('GSC_T00_08_MARKER', 'Process')
}

$buildScript = Join-Path $repo 'scripts/build.ps1'
$capturedRuns = [Collections.Generic.List[object]]::new()
$completed = $false

function Assert-T00_08EnvironmentRestored {
    param([hashtable]$Expected)
    foreach ($name in @('PATH', 'TEMP', 'TMP', 'DOTNET_ROOT', 'DOTNET_ROOT_X64', 'GSC_BUILD_COMMIT', 'GSC_SOURCE_ROOT', 'GSC_T00_08_MARKER')) {
        $key = switch ($name) {
            'PATH' { 'Path' }
            'TEMP' { 'Temp' }
            'TMP' { 'Tmp' }
            'DOTNET_ROOT' { 'DotNetRoot' }
            'DOTNET_ROOT_X64' { 'DotNetRootX64' }
            'GSC_BUILD_COMMIT' { 'BuildCommit' }
            'GSC_SOURCE_ROOT' { 'SourceRoot' }
            'GSC_T00_08_MARKER' { 'FailureMarker' }
        }
        $actual = [Environment]::GetEnvironmentVariable($name, 'Process')
        if ($actual -cne $Expected[$key]) { throw "$name changed across the controlled build." }
    }
}

function Invoke-T00_08Build {
    param([string]$Name, [switch]$SkipTests)
    $before = @(Get-ChildItem -LiteralPath $diagnosticsRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object Name)
    $caught = ''
    if ($SkipTests) {
        try { . $buildScript -Configuration Release -OutputRoot $outputRoot -DiagnosticsRoot $diagnosticsRoot -SkipTests }
        catch { $caught = $_.Exception.Message }
    }
    else {
        try { . $buildScript -Configuration Release -OutputRoot $outputRoot -DiagnosticsRoot $diagnosticsRoot }
        catch { $caught = $_.Exception.Message }
    }

    Assert-T00_08EnvironmentRestored $expectedBuildEnvironment
    $newRuns = @(Get-ChildItem -LiteralPath $diagnosticsRoot -Directory | Where-Object { $before -notcontains $_.Name })
    if ($newRuns.Count -ne 1) { throw "Expected exactly one diagnostics run for '$Name'; found $($newRuns.Count)" }
    $runRoot = $newRuns[0].FullName
    $summaryPath = Join-Path $runRoot 'summary.json'
    if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) { throw "Missing diagnostics summary for '$Name'" }
    $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    $capturedRuns.Add([pscustomobject]@{ Name = $Name; RunRoot = $runRoot; Summary = $summary; Caught = $caught })
    return $capturedRuns[$capturedRuns.Count - 1]
}

try {
    $resolvedShim = (Get-Command dotnet -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
    if (-not [string]::Equals([IO.Path]::GetFullPath($resolvedShim), [IO.Path]::GetFullPath($fakeDotNet), [StringComparison]::OrdinalIgnoreCase)) {
        throw "The controlled dotnet shim did not win PATH resolution: $resolvedShim"
    }

    # Seed an explicitly skipped build, then fail a test phase. The final run
    # omits -SkipTests and must execute every configured test group again.
    $skipped = Invoke-T00_08Build -Name 'skip-tests-baseline' -SkipTests
    if ($skipped.Caught -or $skipped.Summary.outcome -ne 'passed' -or $skipped.Summary.testsRequested -ne $false) {
        throw 'The explicit SkipTests baseline did not complete with testsRequested=false.'
    }

    $failed = Invoke-T00_08Build -Name 'test-failure'
    $failedCore = @($failed.Summary.steps | Where-Object id -EQ 'core') | Select-Object -First 1
    if (-not $failed.Caught -or $failed.Summary.outcome -ne 'failed' -or $failed.Summary.testsRequested -ne $true -or
        $failed.Summary.failedStageId -ne 'core' -or
        -not $failedCore -or $failedCore.exitCode -ne 23 -or -not (Test-Path -LiteralPath $markerPath -PathType Leaf)) {
        throw 'The controlled test failure or explicit failed stage was not preserved in the build diagnostics.'
    }

    $retry = Invoke-T00_08Build -Name 'successful-retry'
    $requiredSteps = @('core', 'worker', 'playnite-discovery', 'wpf.R05TooltipTimingBehaviorTests')
    $retryStepIds = @($retry.Summary.steps | ForEach-Object id)
    $missing = @($requiredSteps | Where-Object { $_ -notin $retryStepIds })
    $failedRetrySteps = @($retry.Summary.steps | Where-Object { $_.exitCode -ne 0 })
    if ($retry.Caught -or $retry.Summary.outcome -ne 'passed' -or $retry.Summary.testsRequested -ne $true -or
        $missing.Count -gt 0 -or $failedRetrySteps.Count -gt 0 -or $retry.Summary.testSummary.totals.testCaseCount -ne 0) {
        throw 'Retry did not execute all test groups cleanly, or synthetic results were misreported as test cases.'
    }

    $runsEvidenceRoot = Join-Path $evidence 'build-runs'
    New-Item -ItemType Directory -Path $runsEvidenceRoot -Force | Out-Null
    foreach ($run in $capturedRuns) {
        $target = Join-Path $runsEvidenceRoot $run.Name
        New-Item -ItemType Directory -Path $target -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $run.RunRoot 'summary.json') -Destination (Join-Path $target 'summary.json') -Force
        foreach ($step in $run.Summary.steps) {
            $sourceLog = [IO.Path]::GetFullPath((Join-Path $run.RunRoot $step.console))
            $runPrefix = [IO.Path]::GetFullPath($run.RunRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
            if (-not $sourceLog.StartsWith($runPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe diagnostics log path: $sourceLog" }
            Copy-Item -LiteralPath $sourceLog -Destination (Join-Path $target $step.console) -Force
        }
    }

    $summaryEvidence = [ordered]@{
        HostPowerShell = $PSVersionTable.PSVersion.ToString()
        SourceCommit = (& git -C $repo rev-parse HEAD).Trim()
        Runtime = 'Controlled dotnet.cmd shim; command orchestration only, no product build or test case execution.'
        ExplicitSkipBaseline = @{ Outcome = $skipped.Summary.outcome; TestsRequested = $skipped.Summary.testsRequested; RunId = $skipped.Summary.owner.RunId }
        InjectedTestFailure = @{ Outcome = $failed.Summary.outcome; TestsRequested = $failed.Summary.testsRequested; FailedStageId = $failed.Summary.failedStageId; CoreExitCode = $failedCore.exitCode; RunId = $failed.Summary.owner.RunId; Evidence = 'build-runs/test-failure' }
        SuccessfulRetry = @{ Outcome = $retry.Summary.outcome; TestsRequested = $retry.Summary.testsRequested; Steps = @($retry.Summary.steps | ForEach-Object { @{ Id = $_.id; ExitCode = $_.exitCode } }); TestCaseCount = $retry.Summary.testSummary.totals.testCaseCount; RunId = $retry.Summary.owner.RunId; Evidence = 'build-runs/successful-retry' }
        PreviousFailureRetained = (Test-Path -LiteralPath (Join-Path $runsEvidenceRoot 'test-failure/summary.json'))
        PreviousSkipNotInherited = ($skipped.Summary.testsRequested -eq $false -and $retry.Summary.testsRequested -eq $true -and $requiredSteps.Count -eq 4)
        EnvironmentRestoredAfterEachRun = $true
        EnvironmentKeysVerified = @('PATH', 'TEMP', 'TMP', 'DOTNET_ROOT', 'DOTNET_ROOT_X64', 'GSC_BUILD_COMMIT', 'GSC_SOURCE_ROOT', 'GSC_T00_08_MARKER')
        SyntheticTestCasesExecuted = 0
        TimestampUtc = [DateTime]::UtcNow.ToString('o')
    }
    $summaryEvidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $evidence 'build-recovery.json') -Encoding UTF8
    $completed = $true
    Write-Host "Build recovery orchestration passed: first test exit 23, clean retry covered $($requiredSteps.Count) test groups. Evidence: $evidence"
}
finally {
    foreach ($pair in $previous.GetEnumerator()) {
        $name = switch ($pair.Key) {
            'Path' { 'PATH' }
            'Temp' { 'TEMP' }
            'Tmp' { 'TMP' }
            'DotNetRoot' { 'DOTNET_ROOT' }
            'DotNetRootX64' { 'DOTNET_ROOT_X64' }
            'BuildCommit' { 'GSC_BUILD_COMMIT' }
            'SourceRoot' { 'GSC_SOURCE_ROOT' }
            'FailureMarker' { 'GSC_T00_08_MARKER' }
        }
        [Environment]::SetEnvironmentVariable($name, $pair.Value, 'Process')
    }
    if ($completed) {
        $resolvedFixture = [IO.Path]::GetFullPath($fixture)
        $allowedPrefix = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if (-not $resolvedFixture.StartsWith($allowedPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe T00-08 fixture cleanup: $resolvedFixture" }
        $fixtureItem = Get-Item -LiteralPath $resolvedFixture -Force
        if ($fixtureItem.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing to clean T00-08 fixture link: $resolvedFixture" }
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
    elseif (Test-Path -LiteralPath $fixture) {
        Write-Warning "T00-08 fixture retained for diagnosis: $fixture"
    }
}
