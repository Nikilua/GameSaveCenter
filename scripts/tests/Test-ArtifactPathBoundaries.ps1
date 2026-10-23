[CmdletBinding()]
param([string]$EvidenceName = '')

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/artifact-preview.ps1')

if ([string]::IsNullOrWhiteSpace($EvidenceName)) {
    $EvidenceName = 'results-' + $PSVersionTable.PSEdition + '.json'
}

$runId = [guid]::NewGuid().ToString('N')
$fixtureLeaf = 't01-04-' + $runId
$fixtureRoot = Join-Path $repo ('.tmp/' + $fixtureLeaf)
$sentinelPath = Join-Path $fixtureRoot 'sentinel.txt'
$junctionPath = Join-Path $fixtureRoot 'junction'
$symbolicLinkPath = Join-Path $fixtureRoot 'sentinel-link.txt'
$evidenceRoot = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-04'
$evidencePath = Join-Path $evidenceRoot $EvidenceName
$owner = $null
$sentinelHashBefore = $null
$symbolicLinkState = 'not-attempted'
$symbolicLinkFailure = $null
$cleanupFailure = $null
$testFailure = $null
$startedUtc = [DateTimeOffset]::UtcNow.ToString('o')
$sentinelHashAfter = $null
$caseResults = [Collections.Generic.List[object]]::new()

function Add-PreviewCase {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedReason,
        [bool]$CreatedByThisRun = $true
    )

    $manifest = @{
        owner = $owner
        artifacts = @(@{
            Path = $Path
            CreatedByThisRun = $CreatedByThisRun
            Active = $false
            Retention = ''
        })
    }
    $preview = @(Get-GscArtifactPreview -RepositoryRoot $repo -Manifest $manifest)[0]
    $passed = $preview.Reason -like $ExpectedReason -and -not $preview.CanDelete
    $recordedPath = $Path
    if (-not [string]::IsNullOrEmpty($recordedPath) -and $recordedPath.StartsWith($repo, [StringComparison]::OrdinalIgnoreCase)) {
        $recordedPath = '[repo]' + $recordedPath.Substring($repo.Length)
    }
    $script:caseResults.Add([ordered]@{
        name = $Name
        path = $recordedPath
        expectedReason = $ExpectedReason
        actualReason = $preview.Reason
        ownerState = $preview.OwnerState
        estimatedBytes = $preview.EstimatedBytes
        canDelete = $preview.CanDelete
        passed = $passed
    }) | Out-Null
    if (-not $passed) {
        throw "Preview case '$Name' failed: expected '$ExpectedReason', got '$($preview.Reason)', CanDelete=$($preview.CanDelete)."
    }
}

try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    [IO.File]::WriteAllText($sentinelPath, 'preserve T01-04 fixture')
    $sentinelHashBefore = (Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash
    $owner = @{
        ProcessId = $PID
        ProcessStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
    }

    $tempRoot = Join-Path $repo '.tmp'
    $artifactRoot = Join-Path $repo 'artifacts'
    $drive = [IO.Path]::GetPathRoot($repo).Substring(0, 1)
    $otherDrive = if ($drive -ieq 'C') { 'D' } else { 'C' }
    $upperCaseRoot = $fixtureRoot.ToUpperInvariant()
    $lowerDriveRoot = $drive.ToLowerInvariant() + $fixtureRoot.Substring(1)

    Add-PreviewCase 'current owner protects an allowed file' $sentinelPath 'Owner process still active'
    Add-PreviewCase 'path comparison ignores case' $upperCaseRoot 'Owner process still active'
    Add-PreviewCase 'drive-letter comparison ignores case' $lowerDriveRoot 'Owner process still active'
    Add-PreviewCase 'empty path' '' 'Rejected:*'
    Add-PreviewCase 'relative path' 'artifacts/relative.txt' 'Rejected:*'
    Add-PreviewCase 'parent traversal' (Join-Path $repo '.tmp/../../outside.txt') 'Rejected:*'
    Add-PreviewCase 'similar root prefix' (Join-Path $repo '.tmp-other/outside.txt') 'Rejected:*'
    Add-PreviewCase 'allowed root itself' $tempRoot 'Rejected:*'
    Add-PreviewCase 'alternate drive' ("${otherDrive}:\artifacts\T01-04-outside.txt") 'Rejected:*'
    Add-PreviewCase 'drive-relative escape' ("${drive}:..\GameSaveCenter-T01-04-outside\file.txt") 'Rejected:*'
    Add-PreviewCase 'unclaimed file ownership' $sentinelPath 'Ownership not claimed' -CreatedByThisRun $false

    $junctionTarget = Join-Path $fixtureRoot 'junction-target'
    New-Item -ItemType Directory -Path $junctionTarget | Out-Null
    New-Item -ItemType Junction -Path $junctionPath -Target $junctionTarget | Out-Null
    Add-PreviewCase 'junction path' $junctionPath 'Rejected:*Linked path boundary*'
    Add-PreviewCase 'linked descendant of registered directory' $fixtureRoot 'Rejected:*Linked descendant*'

    try {
        if ($PSVersionTable.PSVersion.Major -ge 6) {
            New-Item -ItemType SymbolicLink -Path $symbolicLinkPath -Target $sentinelPath | Out-Null
        }
        else {
            $linkCommand = 'mklink "{0}" "{1}"' -f $symbolicLinkPath, $sentinelPath
            & $env:ComSpec /d /c $linkCommand | Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw "cmd mklink exited with code $LASTEXITCODE."
            }
        }
        $symbolicLinkState = 'tested'
    }
    catch {
        $symbolicLinkState = 'unavailable'
        $symbolicLinkFailure = $_.Exception.Message
    }
    if ($symbolicLinkState -eq 'tested') {
        Add-PreviewCase 'file symbolic link' $symbolicLinkPath 'Rejected:*Linked path boundary*'
    }

    $sentinelHashAfter = (Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash
    if ($sentinelHashAfter -ne $sentinelHashBefore) {
        throw 'Preview changed the isolated sentinel.'
    }
}
catch {
    $testFailure = $_.Exception.Message
}
finally {
    try {
        if (Test-Path -LiteralPath $symbolicLinkPath) {
            Remove-Item -LiteralPath $symbolicLinkPath -Force
        }
        if (Test-Path -LiteralPath $junctionPath) {
            Remove-Item -LiteralPath $junctionPath -Force
        }
        if (Test-Path -LiteralPath $fixtureRoot) {
            $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
            $allowedTemp = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')).TrimEnd('\') + [IO.Path]::DirectorySeparatorChar
            if (-not $resolvedFixture.StartsWith($allowedTemp, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Refusing fixture cleanup outside repository .tmp: $resolvedFixture"
            }
            Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
        }
    }
    catch {
        $cleanupFailure = $_.Exception.Message
    }

    $report = [ordered]@{
        schemaVersion = 1
        task = 'T01-04'
        runId = $runId
        powershellVersion = $PSVersionTable.PSVersion.ToString()
        powershellEdition = $PSVersionTable.PSEdition
        startedUtc = $startedUtc
        completedUtc = [DateTimeOffset]::UtcNow.ToString('o')
        testScriptSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
        productionPreviewScriptSha256 = (Get-FileHash -LiteralPath (Join-Path $repo 'scripts/artifact-preview.ps1') -Algorithm SHA256).Hash
        fixture = [ordered]@{
            path = "[repo]/.tmp/$fixtureLeaf"
            createdByThisRun = $true
            removed = -not (Test-Path -LiteralPath $fixtureRoot)
            sentinelSha256Before = $sentinelHashBefore
            sentinelSha256After = $sentinelHashAfter
        }
        symbolicLink = [ordered]@{
            state = $symbolicLinkState
            unavailableReason = $symbolicLinkFailure
        }
        cases = @($caseResults)
        totals = [ordered]@{
            planned = 14
            passed = @($caseResults | Where-Object passed).Count
            skipped = if ($symbolicLinkState -eq 'tested') { 0 } else { 1 }
            failed = if ($testFailure) { 1 } else { 0 }
            allCanDeleteFalse = (@($caseResults | Where-Object canDelete).Count -eq 0)
        }
        testFailure = $testFailure
        cleanupFailure = $cleanupFailure
        outcome = if ($testFailure -or $cleanupFailure) { 'failed' } elseif ($symbolicLinkState -eq 'tested') { 'passed' } else { 'passed-with-symlink-capability-unavailable' }
    }
    New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
    [IO.File]::WriteAllText($evidencePath, ($report | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
}

if ($testFailure) {
    Write-Error $testFailure
    exit 1
}
if ($cleanupFailure) {
    Write-Error $cleanupFailure
    exit 1
}
if ($symbolicLinkState -eq 'tested') {
    Write-Host "Artifact path boundaries: $($caseResults.Count)/$($caseResults.Count) passed; symlink tested; sentinel unchanged."
}
else {
    Write-Warning "Artifact path boundaries passed except file symlink creation, which is unavailable: $symbolicLinkFailure"
}
