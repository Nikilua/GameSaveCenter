[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$CheckoutRoot = '',
    [ValidateRange(100, 259)][int]$MaximumPathLength = 240,
    [ValidateRange(1, 2147483647)][long]$DuplicateLargeEvidenceBytes = 1048576
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
if ([string]::IsNullOrWhiteSpace($CheckoutRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_WORKSPACE)) {
        $CheckoutRoot = $env:GITHUB_WORKSPACE
    }
    else {
        $CheckoutRoot = 'D:\a\GameSaveCenter\GameSaveCenter'
    }
}
$CheckoutRoot = [IO.Path]::GetFullPath($CheckoutRoot).TrimEnd('\', '/')

$stagedOutput = @(& git -C $RepositoryRoot -c core.quotepath=false diff --cached --name-only --diff-filter=ACMT --no-renames -- 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "Cannot read staged paths: $($stagedOutput -join ' ')"
}
$evidencePaths = @($stagedOutput | ForEach-Object { [string]$_ } | Where-Object { $_ -match '(?i)(^|/)evidence/' } | Sort-Object -Unique)
if (-not $evidencePaths.Count) {
    Write-Output 'No staged evidence paths to check.'
    return
}

$indexOutput = @(& git -C $RepositoryRoot -c core.quotepath=false ls-files --stage 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "Cannot read index paths: $($indexOutput -join ' ')"
}
$indexEntries = [Collections.Generic.List[object]]::new()
$entriesByPath = @{}
foreach ($line in $indexOutput) {
    $match = [regex]::Match([string]$line, '^(?<mode>\d{6}) (?<blob>[0-9a-f]{40,64}) (?<stage>\d)\t(?<path>.*)$')
    if (-not $match.Success) {
        throw "Cannot parse Git index entry: $line"
    }
    $entry = [pscustomobject]@{
        Mode = $match.Groups['mode'].Value
        Blob = $match.Groups['blob'].Value
        Stage = [int]$match.Groups['stage'].Value
        Path = $match.Groups['path'].Value
    }
    $indexEntries.Add($entry)
    if ($entry.Stage -eq 0) {
        $entriesByPath[$entry.Path] = $entry
    }
}

$checked = [Collections.Generic.List[object]]::new()
foreach ($path in $evidencePaths) {
    if ([string]::IsNullOrWhiteSpace($path) -or $path.StartsWith('/') -or $path -match '(^|/)\.\.?(/|$)|:') {
        throw "Invalid staged evidence path: $path"
    }
    if (-not $entriesByPath.ContainsKey($path)) {
        throw "Staged evidence path has no stage-0 index entry: $path"
    }

    $projectedPath = $CheckoutRoot + '\' + $path.Replace('/', '\')
    if ($projectedPath.Length -gt $MaximumPathLength) {
        throw "Projected checkout path length $($projectedPath.Length) exceeds the $MaximumPathLength character limit: $path"
    }

    $entry = $entriesByPath[$path]
    if ($entry.Mode -notin @('100644', '100755')) {
        throw "Staged evidence must be a regular file: $path (mode $($entry.Mode))"
    }
    $sizeOutput = @(& git -C $RepositoryRoot cat-file -s $entry.Blob 2>&1)
    if ($LASTEXITCODE -ne 0 -or $sizeOutput.Count -ne 1) {
        throw "Cannot read staged evidence size for $path : $($sizeOutput -join ' ')"
    }
    $size = [long]$sizeOutput[0]
    if ($size -ge $DuplicateLargeEvidenceBytes) {
        $duplicates = @($indexEntries | Where-Object {
            $_.Stage -eq 0 -and $_.Mode -in @('100644', '100755') -and
            $_.Path -ne $path -and $_.Path -match '(?i)(^|/)evidence/' -and $_.Blob -eq $entry.Blob
        } | Select-Object -ExpandProperty Path -Unique)
        if ($duplicates.Count) {
            throw "Large staged evidence ($size bytes) duplicates an indexed evidence blob at: $($duplicates -join ', ') ; new path: $path"
        }
    }

    $checked.Add([pscustomobject]@{ Path = $path; ProjectedLength = $projectedPath.Length; BlobBytes = $size })
}

Write-Output ([pscustomobject]@{
    Result = 'Passed'
    CheckoutRoot = $CheckoutRoot
    MaximumPathLength = $MaximumPathLength
    DuplicateLargeEvidenceBytes = $DuplicateLargeEvidenceBytes
    CheckedEvidencePaths = $checked.Count
    Paths = $checked.ToArray()
})
