[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/artifact-preview.ps1')

$runId = [guid]::NewGuid().ToString('N')
$fixtureRoot = Join-Path $repo ('.tmp/t01-05-' + $runId)
$artifactRoot = Join-Path $fixtureRoot 'active-run'
$readyPath = Join-Path $fixtureRoot 'owner-ready.json'
$stopPath = Join-Path $fixtureRoot 'owner-stop'
$sentinelPath = Join-Path $artifactRoot 'sentinel.txt'
$evidenceRoot = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-05'
$evidencePath = Join-Path $evidenceRoot ('results-' + $PSVersionTable.PSEdition + '.json')
$auditFixtureRoot = Join-Path $repo ('artifacts/t01-05-' + $runId)
$auditOutputRoot = Join-Path $auditFixtureRoot 'existing-output'
$auditSentinelPath = Join-Path $auditOutputRoot 'keep.txt'
$auditStdoutPath = Join-Path $fixtureRoot 'audit-stdout.txt'
$auditStderrPath = Join-Path $fixtureRoot 'audit-stderr.txt'
$child = $null
$childIdentity = $null
$auditChild = $null
$auditChildStartTicks = $null
$checkoutMutex = $null
$lock = $null
$testFailure = $null
$result = [ordered]@{
    schemaVersion = 1
    engine = $PSVersionTable.PSVersion.ToString()
    edition = $PSVersionTable.PSEdition
    runId = $runId
    activeOwner = $null
    exitedOwnerLockedFile = $null
    exitedOwnerReleasedFile = $null
    staleStartTime = $null
    unavailableOwner = $null
    parallelAuditRefused = $false
    auditOutputPreserved = $false
    sentinelUnchanged = $false
    deletionAuthorized = $false
    outcome = 'failed'
}

function Assert-Preview {
    param(
        [Parameter(Mandatory = $true)]$Preview,
        [Parameter(Mandatory = $true)][string]$OwnerState,
        [Parameter(Mandatory = $true)][string]$OccupancyState,
        [Parameter(Mandatory = $true)][string]$ReasonPattern
    )
    if ($Preview.OwnerState -ne $OwnerState) { throw "Expected owner state '$OwnerState', got '$($Preview.OwnerState)'" }
    if ($Preview.OccupancyState -ne $OccupancyState) { throw "Expected occupancy '$OccupancyState', got '$($Preview.OccupancyState)'" }
    if ($Preview.Reason -notlike $ReasonPattern) { throw "Expected reason '$ReasonPattern', got '$($Preview.Reason)'" }
    if (-not $Preview.RunId -or $Preview.RunId -ne $runId) { throw 'Preview did not retain the run ID' }
    if ($Preview.CanDelete) { throw 'Read-only preview authorized deletion' }
}

try {
    New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
    [IO.File]::WriteAllText($sentinelPath, 'preserve T01-05 fixture')
    $sentinelHash = (Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash

    $engine = (Get-Process -Id $PID).Path
    $runIdLiteral = $runId.Replace("'", "''")
    $readyLiteral = $readyPath.Replace("'", "''")
    $stopLiteral = $stopPath.Replace("'", "''")
    $childSource = @"
`$ownerRunId = '$runIdLiteral'
`$readyFile = '$readyLiteral'
`$stopFile = '$stopLiteral'
`$self = Get-Process -Id `$PID
`$identity = [ordered]@{ RunId = `$ownerRunId; ProcessId = `$PID; ProcessStartedUtc = `$self.StartTime.ToUniversalTime().ToString('o') }
[IO.File]::WriteAllText(`$readyFile, (`$identity | ConvertTo-Json -Compress))
while (-not [IO.File]::Exists(`$stopFile)) { Start-Sleep -Milliseconds 100 }
"@
    $encodedChild = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($childSource))
    $child = Start-Process -FilePath $engine -ArgumentList @('-NoProfile', '-EncodedCommand', $encodedChild) -WindowStyle Hidden -PassThru

    $readyDeadline = [DateTime]::UtcNow.AddSeconds(15)
    while (-not (Test-Path -LiteralPath $readyPath -PathType Leaf)) {
        $child.Refresh()
        if ($child.HasExited) { throw "Owner child exited before publishing identity (code $($child.ExitCode))" }
        if ([DateTime]::UtcNow -ge $readyDeadline) { throw 'Timed out waiting for the owner child identity' }
        Start-Sleep -Milliseconds 100
    }
    $childIdentity = Get-Content -LiteralPath $readyPath -Raw | ConvertFrom-Json
    if ([string]$childIdentity.RunId -ne $runId -or [int]$childIdentity.ProcessId -ne $child.Id) {
        throw 'Child owner identity did not match the registered run'
    }

    $manifest = @{
        owner = @{
            RunId = [string]$childIdentity.RunId
            ProcessId = [int]$childIdentity.ProcessId
            ProcessStartedUtc = $childIdentity.ProcessStartedUtc
        }
        artifacts = @(@{
            Path = $artifactRoot
            Stage = 'test-active-run'
            CreatedByThisRun = $true
            Active = $true
            Retention = ''
        })
    }

    $active = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $manifest)[0]
    Assert-Preview -Preview $active -OwnerState 'active' -OccupancyState 'owner-active' -ReasonPattern '*Owner process still active*'
    $result.activeOwner = [ordered]@{
        ownerState = $active.OwnerState
        occupancyState = $active.OccupancyState
        occupancyIssue = $active.OccupancyIssue
        reason = $active.Reason
        processIdMatches = ($active.OwnerState -eq 'active')
        processStartTimeRecorded = [bool]$manifest.owner.ProcessStartedUtc
        runIdMatches = ($active.RunId -eq $runId)
        canDelete = $active.CanDelete
    }

    $lock = [IO.File]::Open($sentinelPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
    [IO.File]::WriteAllText($stopPath, 'stop')
    if (-not $child.WaitForExit(10000)) { throw 'Owner child did not exit after its stop signal' }
    if ($child.ExitCode -ne 0) { throw "Owner child exited with code $($child.ExitCode)" }
    $manifest.artifacts[0].Active = $false

    $exitedLocked = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $manifest)[0]
    Assert-Preview -Preview $exitedLocked -OwnerState 'exited' -OccupancyState 'in-use' -ReasonPattern '*Review candidate*file occupancy in-use: sentinel.txt*'
    if (-not (Test-Path -LiteralPath $sentinelPath -PathType Leaf)) { throw 'Locked sentinel disappeared' }
    $result.exitedOwnerLockedFile = [ordered]@{
        ownerState = $exitedLocked.OwnerState
        occupancyState = $exitedLocked.OccupancyState
        occupancyIssue = $exitedLocked.OccupancyIssue
        reason = $exitedLocked.Reason
        filePreserved = (Test-Path -LiteralPath $sentinelPath -PathType Leaf)
        canDelete = $exitedLocked.CanDelete
    }

    $lock.Dispose()
    $lock = $null
    $released = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $manifest)[0]
    Assert-Preview -Preview $released -OwnerState 'exited' -OccupancyState 'clear' -ReasonPattern 'Review candidate;*'
    $result.exitedOwnerReleasedFile = [ordered]@{
        ownerState = $released.OwnerState
        occupancyState = $released.OccupancyState
        reason = $released.Reason
        filePreserved = (Test-Path -LiteralPath $sentinelPath -PathType Leaf)
        canDelete = $released.CanDelete
    }

    $staleManifest = @{
        owner = @{
            RunId = 'stale-run-id'
            ProcessId = $PID
            ProcessStartedUtc = [DateTime]::UtcNow.AddYears(-10).ToString('o')
        }
        artifacts = @(@{
            Path = $artifactRoot
            CreatedByThisRun = $true
            Active = $false
            Retention = ''
        })
    }
    $stale = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $staleManifest)[0]
    if ($stale.OwnerState -ne 'different-process' -or $stale.CanDelete) { throw 'PID reuse was not rejected by the recorded start time' }
    $result.staleStartTime = [ordered]@{
        ownerState = $stale.OwnerState
        runId = $stale.RunId
        canDelete = $stale.CanDelete
    }

    $unknownManifest = @{
        artifacts = @(@{
            Path = $artifactRoot
            CreatedByThisRun = $true
            Active = $false
            Retention = ''
        })
    }
    $unknown = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $unknownManifest)[0]
    if ($unknown.OwnerState -ne 'unknown' -or $unknown.OccupancyState -ne 'owner-unknown' -or $unknown.CanDelete) {
        throw 'Unavailable owner identity was not kept out of the cleanup candidate path'
    }
    $result.unavailableOwner = [ordered]@{
        ownerState = $unknown.OwnerState
        occupancyState = $unknown.OccupancyState
        runId = $unknown.RunId
        canDelete = $unknown.CanDelete
    }

    . (Join-Path $repo 'scripts/checkout-lock.ps1')
    $checkoutMutex = Enter-GscCheckoutLock $repo
    try {
        New-Item -ItemType Directory -Path $auditOutputRoot -Force | Out-Null
        [IO.File]::WriteAllText($auditSentinelPath, 'keep active audit output')
        $auditHash = (Get-FileHash -LiteralPath $auditSentinelPath -Algorithm SHA256).Hash
        $captureScript = Join-Path $repo 'scripts/capture-ui-audit.ps1'
        $auditArguments = @('-NoProfile', '-File', ('"' + $captureScript + '"'), '-Output', ('"' + $auditOutputRoot + '"'))
        $auditChild = Start-Process -FilePath $engine -ArgumentList $auditArguments -WindowStyle Hidden -PassThru -RedirectStandardOutput $auditStdoutPath -RedirectStandardError $auditStderrPath
        $auditChildStartTicks = (Get-Process -Id $auditChild.Id).StartTime.ToUniversalTime().Ticks
        if (-not $auditChild.WaitForExit(10000)) { throw 'Concurrent audit did not refuse the active checkout promptly' }
        if ($auditChild.ExitCode -eq 0) { throw 'Concurrent audit succeeded while another checkout owner held the lock' }
        if (-not (Test-Path -LiteralPath $auditSentinelPath -PathType Leaf)) { throw 'Concurrent audit removed the existing output sentinel' }
        if ((Get-FileHash -LiteralPath $auditSentinelPath -Algorithm SHA256).Hash -ne $auditHash) { throw 'Concurrent audit changed the existing output sentinel' }
        $auditText = (Get-Content -LiteralPath $auditStdoutPath -Raw -ErrorAction SilentlyContinue) + (Get-Content -LiteralPath $auditStderrPath -Raw -ErrorAction SilentlyContinue)
        if ($auditText -notlike '*active build/install run*') { throw 'Concurrent audit did not report the active checkout owner' }
        $result.parallelAuditRefused = $true
        $result.auditOutputPreserved = $true
    }
    finally {
        if ($checkoutMutex) { Exit-GscCheckoutLock $checkoutMutex; $checkoutMutex = $null }
    }

    $finalHash = (Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash
    if ($finalHash -ne $sentinelHash) { throw 'Preview changed the locked sentinel' }
    $result.sentinelUnchanged = $true
    $result.outcome = 'passed'
    New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
    [IO.File]::WriteAllText($evidencePath, ($result | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
}
catch {
    $testFailure = $_.Exception.Message
}
finally {
    if ($auditChild -and -not $auditChild.HasExited) {
        try {
            if (-not $auditChild.WaitForExit(5000)) {
                $stillRunning = Get-Process -Id $auditChild.Id -ErrorAction SilentlyContinue
                if ($stillRunning -and $auditChildStartTicks -and $stillRunning.StartTime.ToUniversalTime().Ticks -eq $auditChildStartTicks) {
                    Stop-Process -Id $auditChild.Id -Force
                    [void]$auditChild.WaitForExit(5000)
                }
            }
        }
        catch { }
    }
    if ($checkoutMutex) { Exit-GscCheckoutLock $checkoutMutex }
    if ($child -and -not $child.HasExited) {
        try {
            if (-not (Test-Path -LiteralPath $stopPath)) { [IO.File]::WriteAllText($stopPath, 'stop') }
            if (-not $child.WaitForExit(5000)) {
                $stillRunning = Get-Process -Id $child.Id -ErrorAction SilentlyContinue
                if ($stillRunning -and $childIdentity -and $stillRunning.StartTime.ToUniversalTime().Ticks -eq ([DateTimeOffset]::Parse([string]$childIdentity.ProcessStartedUtc).UtcDateTime.Ticks)) {
                    Stop-Process -Id $child.Id -Force
                    [void]$child.WaitForExit(5000)
                }
            }
        }
        catch { }
    }
    if ($lock) { $lock.Dispose() }
    if (Test-Path -LiteralPath $fixtureRoot) {
        $fullFixture = [IO.Path]::GetFullPath($fixtureRoot)
        $allowedRoot = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar
        if (-not $fullFixture.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Refusing to clean fixture outside repository .tmp'
        }
        if ((Get-Item -LiteralPath $fullFixture -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Refusing to clean a linked fixture root'
        }
        Remove-Item -LiteralPath $fullFixture -Recurse -Force
    }
    if (Test-Path -LiteralPath $auditFixtureRoot) {
        $fullAuditFixture = [IO.Path]::GetFullPath($auditFixtureRoot)
        $allowedArtifactsRoot = [IO.Path]::GetFullPath((Join-Path $repo 'artifacts')) + [IO.Path]::DirectorySeparatorChar
        if (-not $fullAuditFixture.StartsWith($allowedArtifactsRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Refusing to clean audit fixture outside repository artifacts'
        }
        if ((Get-Item -LiteralPath $fullAuditFixture -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Refusing to clean a linked audit fixture root'
        }
        Remove-Item -LiteralPath $fullAuditFixture -Recurse -Force
    }
}

if ($testFailure) { throw $testFailure }
Write-Host "Active artifact protection: passed ($($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion))"
