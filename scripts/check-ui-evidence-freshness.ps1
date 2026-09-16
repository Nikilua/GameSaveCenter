[CmdletBinding()]
param(
    [string]$BaselinePath = 'docs/design/reviews/ui-finesse-round3-20260915/UI_EVIDENCE_BASELINE.json',
    [string]$HeadCommit = 'HEAD',
    [string[]]$ChangedPath = @(),
    [string]$PackageCommit = '',
    [string]$OutputPath = '',
    [switch]$AsJson
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    return [System.IO.Path]::GetFullPath((Join-Path $root $Path))
}

function Normalize-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    return $Path.Replace('\', '/').TrimStart('./')
}

function Invoke-GitText {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)
    $output = @(& git -C $root @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "git command failed: $($Arguments -join ' '): $($output -join ' ')"
    }
    return (($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine).Trim()
}

function Test-DocumentationPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    $normalized = Normalize-RepoPath $Path
    return $normalized -like 'docs/*' -or
        $normalized -like '.codex/*' -or
        $normalized -in @('AGENTS.md', 'README.md')
}

function Get-ChangedPaths {
    param([Parameter(Mandatory = $true)][string]$SourceCommit)
    $raw = Invoke-GitText @('diff', '--name-only', $SourceCommit, $HeadCommit)
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @()
    }
    return @($raw -split '\r?\n' | Where-Object { $_ } | ForEach-Object { Normalize-RepoPath $_ } | Sort-Object -Unique)
}

$baselineFull = Resolve-RepoPath $BaselinePath
if (-not (Test-Path -LiteralPath $baselineFull -PathType Leaf)) {
    throw "Evidence baseline is missing: $baselineFull"
}
$baseline = Get-Content -Raw -LiteralPath $baselineFull | ConvertFrom-Json
if ($baseline.schemaVersion -ne 1 -or -not $baseline.records) {
    throw "Unsupported or empty evidence baseline: $baselineFull"
}

$currentSourceCommit = Invoke-GitText @('rev-parse', $HeadCommit)
if ($currentSourceCommit -notmatch '^[0-9a-f]{40}$') {
    throw "Head commit is not a full Git SHA: $currentSourceCommit"
}
$currentPackageCommit = if ([string]::IsNullOrWhiteSpace($PackageCommit)) { 'not-provided' } else { $PackageCommit.Trim() }
$overridePaths = @($ChangedPath | Where-Object { $_ } | ForEach-Object { Normalize-RepoPath $_ } | Sort-Object -Unique)
$allComparedPaths = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::OrdinalIgnoreCase)
$results = @()

foreach ($record in @($baseline.records)) {
    if ([string]::IsNullOrWhiteSpace($record.sourceCommit)) {
        throw "Evidence record has no sourceCommit: $($record.id)"
    }
    $recordChanges = if ($overridePaths.Count -gt 0) { $overridePaths } else { @(Get-ChangedPaths $record.sourceCommit) }
    foreach ($path in $recordChanges) {
        [void]$allComparedPaths.Add($path)
    }

    $matched = @()
    foreach ($path in $recordChanges) {
        foreach ($pattern in @($record.sourcePaths)) {
            if ($path -like (Normalize-RepoPath $pattern)) {
                $matched += $path
                break
            }
        }
    }
    $matched = @($matched | Sort-Object -Unique)
    $nonDocumentation = @($recordChanges | Where-Object { -not (Test-DocumentationPath $_) })
    $documentationOnly = $nonDocumentation.Count -eq 0
    $packageBaseline = if ($null -eq $record.packageCommit -or [string]::IsNullOrWhiteSpace([string]$record.packageCommit)) { 'not-applicable' } else { [string]$record.packageCommit }
    $packageStatus = if ($packageBaseline -eq 'not-applicable') {
        'not-applicable'
    }
    elseif ($currentPackageCommit -eq 'not-provided') {
        'not-provided'
    }
    elseif ($currentPackageCommit -eq $packageBaseline) {
        'matched'
    }
    else {
        'mismatch'
    }
    $needsRerun = $matched.Count -gt 0
    $reinstallRequired = $needsRerun -and ([string]$record.runtimeKind -eq 'package-host' -or $packageStatus -eq 'mismatch')
    $results += [ordered]@{
        id = [string]$record.id
        evidence = [string]$record.evidence
        scopes = @($record.scopes)
        runtimeKind = [string]$record.runtimeKind
        evidenceSourceCommit = [string]$record.sourceCommit
        changedPathsSinceEvidence = @($recordChanges)
        matchedSourcePaths = @($matched)
        documentationOnlyChange = $documentationOnly
        needsRerun = $needsRerun
        reinstallRequired = $reinstallRequired
        packageBaselineCommit = $packageBaseline
        packageStatus = $packageStatus
    }
}

$allPaths = @($allComparedPaths | Sort-Object)
$allNonDocumentation = @($allPaths | Where-Object { -not (Test-DocumentationPath $_) })
$result = [ordered]@{
    schemaVersion = 1
    generatedUtc = [DateTime]::UtcNow.ToString('o')
    currentSourceCommit = $currentSourceCommit
    currentPackageCommit = $currentPackageCommit
    packageIdentityStatus = if ($currentPackageCommit -eq 'not-provided') { 'not-provided' } else { 'separate-input' }
    changedPaths = $allPaths
    documentationOnlyChange = $allNonDocumentation.Count -eq 0
    records = @($results)
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputFull = Resolve-RepoPath $OutputPath
    $parent = Split-Path -Parent $outputFull
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    $json = $result | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText($outputFull, $json, (New-Object System.Text.UTF8Encoding($false)))
}

if ($AsJson) {
    Write-Output ($result | ConvertTo-Json -Depth 8)
}
else {
    Write-Output "UI evidence freshness: source=$currentSourceCommit package=$currentPackageCommit documentationOnly=$($result.documentationOnlyChange)"
    foreach ($item in $results) {
        $status = if ($item.needsRerun) { 'STALE' } else { 'FRESH' }
        $scopes = (@($item.scopes) -join ',')
        $changed = (@($item.matchedSourcePaths) -join ',')
        Write-Output "$($item.id)|$status|scopes=$scopes|matched=$changed|package=$($item.packageStatus)|reinstall=$($item.reinstallRequired)"
    }
}
