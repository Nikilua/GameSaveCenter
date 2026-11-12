[CmdletBinding()]
param([string]$EvidencePath = '')

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$checker = Join-Path $repo 'scripts/check-evidence-paths.ps1'
$runId = [guid]::NewGuid().ToString('N')
$fixtureRoot = Join-Path $repo ('.tmp/assessment-evidence-path-preflight-' + $runId)
$fixtureRepo = Join-Path $fixtureRoot 'repo'
$startedUtc = [DateTimeOffset]::UtcNow.ToString('o')
$driveRoot = [IO.Path]::GetPathRoot($repo)
$driveInfo = [IO.DriveInfo]::new($driveRoot)
$freeBytesBefore = $driveInfo.AvailableFreeSpace
$baselineGitProcessIds = @(Get-Process -Name git -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$fixtureBytes = 0L
$fixtureFiles = 0
$fixtureRemoved = $false
$reparseEntries = @()
$gitProcessesObserved = @()
$indexLockAbsent = $false
$exclusiveFileReadsPassed = $false
$cleanupFailure = $null
$testFailure = $null
$cases = [Collections.Generic.List[object]]::new()

function Invoke-FixtureGit {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)
    $output = @(& git -C $script:fixtureRepo @Arguments 2>&1)
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        throw "Fixture git command failed ($code): git $($Arguments -join ' '): $($output -join ' ')"
    }
    return ,$output
}

function Add-CaseResult {
    param([string]$Name, [string]$Expected, [string]$Actual, [bool]$Passed, [int]$ExitCode)
    $script:cases.Add([ordered]@{ name = $Name; expected = $Expected; actual = $Actual; passed = $Passed; exitCode = $ExitCode }) | Out-Null
    if (-not $Passed) { throw "Case '$Name' failed: expected $Expected, got $Actual." }
}

function Invoke-PreflightExpect {
    param([string]$Name, [bool]$ShouldPass, [string]$ExpectedMessage = '')
    try {
        $output = @(& $script:checker -RepositoryRoot $script:fixtureRepo -CheckoutRoot 'C:\a\GameSaveCenter\GameSaveCenter' -MaximumPathLength 240 2>&1)
        $actual = $output -join ' '
        if ($ShouldPass) {
            Add-CaseResult $Name 'pass' 'pass' $true 0
        }
        else {
            Add-CaseResult $Name $ExpectedMessage 'unexpected pass' $false 0
        }
    }
    catch {
        $actual = $_.Exception.Message
        if ($ShouldPass) {
            Add-CaseResult $Name 'pass' $actual $false 1
        }
        elseif ($actual -like "*$ExpectedMessage*") {
            Add-CaseResult $Name $ExpectedMessage $actual $true 1
        }
        else {
            Add-CaseResult $Name $ExpectedMessage $actual $false 1
        }
    }
}

try {
    New-Item -ItemType Directory -Path $fixtureRepo -ErrorAction Stop | Out-Null
    $initOutput = @(& git -C $fixtureRepo init --quiet 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "Cannot initialize isolated Git fixture: $($initOutput -join ' ')" }
    Invoke-FixtureGit @('config', 'user.name', 'Evidence Path Preflight Test') | Out-Null
    Invoke-FixtureGit @('config', 'user.email', 'evidence-path-test@example.invalid') | Out-Null

    $evidenceDirectory = Join-Path $fixtureRepo 'docs/design/reviews/path-preflight/evidence'
    New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null
    $largeOriginalPath = Join-Path $evidenceDirectory 'original-large.bin'
    $largeBytes = New-Object byte[] 1048577
    for ($index = 0; $index -lt $largeBytes.Length; $index++) { $largeBytes[$index] = [byte]($index % 251) }
    [IO.File]::WriteAllBytes($largeOriginalPath, $largeBytes)
    Invoke-FixtureGit @('add', '--', 'docs/design/reviews/path-preflight/evidence/original-large.bin') | Out-Null
    Invoke-FixtureGit @('commit', '--quiet', '-m', 'fixture baseline') | Out-Null
    $largeBlob = ([string](Invoke-FixtureGit @('rev-parse', 'HEAD:docs/design/reviews/path-preflight/evidence/original-large.bin'))[0]).Trim()

    $shortPath = Join-Path $evidenceDirectory 'new-short.json'
    [IO.File]::WriteAllText($shortPath, '{"fixture":true}', [Text.UTF8Encoding]::new($false))
    Invoke-FixtureGit @('add', '--', 'docs/design/reviews/path-preflight/evidence/new-short.json') | Out-Null
    Invoke-PreflightExpect 'short staged evidence is accepted' $true

    $longDirectory = 'd' * 100
    $longLeaf = ('p' * 120) + '.json'
    $longPath = 'docs/design/reviews/path-preflight/evidence/' + $longDirectory + '/' + $longLeaf
    $shortBlob = ([string](Invoke-FixtureGit @('hash-object', '-w', $shortPath))[0]).Trim()
    Invoke-FixtureGit @('update-index', '--add', '--cacheinfo', ('100644,' + $shortBlob + ',' + $longPath)) | Out-Null
    Invoke-PreflightExpect 'overlong staged evidence is rejected' $false 'exceeds the 240 character limit'
    Invoke-FixtureGit @('reset', '--quiet', 'HEAD', '--', $longPath) | Out-Null

    $duplicatePath = 'docs/design/reviews/path-preflight/evidence/duplicate-large.bin'
    Invoke-FixtureGit @('update-index', '--add', '--cacheinfo', ('100644,' + $largeBlob + ',' + $duplicatePath)) | Out-Null
    Invoke-PreflightExpect 'duplicate large evidence blob is rejected' $false 'duplicates an indexed evidence blob'
    Invoke-FixtureGit @('reset', '--quiet', 'HEAD', '--', $duplicatePath) | Out-Null

    $uniqueBytes = [byte[]]$largeBytes.Clone()
    $uniqueBytes[$uniqueBytes.Length - 1] = [byte]($uniqueBytes[$uniqueBytes.Length - 1] -bxor 1)
    $uniquePath = Join-Path $evidenceDirectory 'new-unique-large.bin'
    [IO.File]::WriteAllBytes($uniquePath, $uniqueBytes)
    Invoke-FixtureGit @('add', '--', 'docs/design/reviews/path-preflight/evidence/new-unique-large.bin') | Out-Null
    Invoke-PreflightExpect 'unique large evidence is accepted' $true
}
catch {
    $testFailure = $_.Exception.Message
}
finally {
    try {
        $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
        $allowedRoot = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')).TrimEnd('\') + [IO.Path]::DirectorySeparatorChar
        if (-not $resolvedFixture.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing fixture cleanup outside repository .tmp: $resolvedFixture"
        }
        if (Test-Path -LiteralPath $resolvedFixture) {
            $fixtureInventory = @(Get-ChildItem -LiteralPath $resolvedFixture -Force -Recurse -ErrorAction Stop)
            $reparseEntries = @($fixtureInventory | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint } | Select-Object -ExpandProperty FullName)
            if ($reparseEntries.Count) { throw "Fixture contains reparse entries: $($reparseEntries -join ', ')" }
            $files = @($fixtureInventory | Where-Object { -not $_.PSIsContainer })
            $fixtureFiles = $files.Count
            $fixtureBytes = [long](($files | Measure-Object -Property Length -Sum).Sum)
            foreach ($file in $files) {
                $stream = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
                $stream.Dispose()
            }
            $indexLockAbsent = -not (Test-Path -LiteralPath (Join-Path $fixtureRepo '.git/index.lock'))
            if (-not $indexLockAbsent) {
                throw 'Fixture Git index lock is still present.'
            }
            $gitProcessesObserved = @(Get-Process -Name git -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
            # Fixture Git commands are synchronous; exclusive file opens and the absent index lock
            # establish that no process currently holds the fixture data or index for writing.
            $exclusiveFileReadsPassed = $true

            Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
            $fixtureRemoved = -not (Test-Path -LiteralPath $resolvedFixture)
            if (-not $fixtureRemoved) { throw 'Fixture directory remains after cleanup.' }
        }
    }
    catch {
        $cleanupFailure = $_.Exception.Message
    }

    $cleanup = [ordered]@{
        fixtureRoot = "[repo]/.tmp/assessment-evidence-path-preflight-$runId"
        removed = $fixtureRemoved
        filesRemoved = $fixtureFiles
        bytesRemoved = $fixtureBytes
        reparseEntries = @($reparseEntries)
        baselineGitProcessIds = @($baselineGitProcessIds)
        gitProcessesObserved = @($gitProcessesObserved)
        indexLockAbsent = $indexLockAbsent
        exclusiveFileReadsPassed = $exclusiveFileReadsPassed
        processCheck = 'Fixture Git commands returned synchronously; no fixture index.lock remained; every fixture file opened with FileShare.None before removal.'
        driveRoot = $driveRoot
        freeBytesBefore = $freeBytesBefore
        freeBytesAfter = [IO.DriveInfo]::new($driveRoot).AvailableFreeSpace
        failure = $cleanupFailure
    }
    $report = [ordered]@{
        schemaVersion = 1
        task = 'A11 evidence path preflight'
        runId = $runId
        powershellVersion = $PSVersionTable.PSVersion.ToString()
        startedUtc = $startedUtc
        completedUtc = [DateTimeOffset]::UtcNow.ToString('o')
        checkerSha256 = (Get-FileHash -LiteralPath $checker -Algorithm SHA256).Hash
        testScriptSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
        cases = @($cases)
        totals = [ordered]@{ planned = 4; passed = @($cases | Where-Object passed).Count; failed = if ($testFailure) { 1 } else { 0 } }
        cleanup = $cleanup
        failure = $testFailure
    }
    if ($EvidencePath) {
        $evidenceFull = [IO.Path]::GetFullPath((Join-Path $repo $EvidencePath))
        $allowedEvidenceRoot = [IO.Path]::GetFullPath((Join-Path $repo 'docs/design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112')).TrimEnd('\') + [IO.Path]::DirectorySeparatorChar
        if (-not $evidenceFull.StartsWith($allowedEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "Evidence output is outside the assessment evidence folder: $evidenceFull" }
        if (Test-Path -LiteralPath $evidenceFull) { throw "Evidence output already exists: $evidenceFull" }
        New-Item -ItemType Directory -Path (Split-Path -Parent $evidenceFull) -Force | Out-Null
        [IO.File]::WriteAllText($evidenceFull, ($report | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
    }
}

if ($cleanupFailure) { throw "Evidence preflight fixture cleanup failed: $cleanupFailure" }
if ($testFailure) { throw "Evidence path preflight test failed: $testFailure" }
Write-Output 'Evidence path preflight tests 4/4 passed.'
