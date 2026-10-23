# Read-only evidence inventory and archive verification. Never authorizes deletion.
function Get-GscEvidenceInventory {
    param([Parameter(Mandatory=$true)][string]$Root)
    $canonical = [IO.Path]::GetFullPath($Root).TrimEnd('\','/')
    $ancestor = $canonical
    while ($ancestor) {
        $ancestorItem = Get-Item -LiteralPath $ancestor -Force -ErrorAction Stop
        if ($ancestorItem.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Linked evidence ancestor is not supported' }
        $ancestor = Split-Path -Parent $ancestor
    }
    $rootItem = Get-Item -LiteralPath $canonical -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer -or ($rootItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Evidence root must be a non-linked directory' }
    $items = @(Get-ChildItem -LiteralPath $canonical -Recurse -Force -ErrorAction Stop)
    if (@($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Linked evidence is not supported' }
    $hashes = @{}
    foreach ($file in @($items | Where-Object { -not $_.PSIsContainer } | Sort-Object FullName)) {
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256 -ErrorAction Stop).Hash
        $relative = $file.FullName.Substring($canonical.Length).TrimStart('\','/').Replace('\','/')
        $tier = if ($file.Extension -in @('.dmp','.dump')) { 'large-diagnostic' }
            elseif ($file.Extension -in @('.png','.jpg','.jpeg')) { 'visual-evidence' }
            else { 'required-context' }
        $duplicate = $null
        # Only visual evidence can be described as duplicate; no deletion follows.
        if ($tier -eq 'visual-evidence') {
            if ($hashes.ContainsKey($hash)) { $duplicate = $hashes[$hash] }
            else { $hashes[$hash] = $relative }
        }
        [pscustomobject]@{ Path=$relative; Bytes=$file.Length; SHA256=$hash; Tier=$tier; DuplicateOf=$duplicate; CanDelete=$false }
    }
}

function Assert-GscEvidenceArchived {
    param([Parameter(Mandatory=$true)][string]$SourceRoot, [Parameter(Mandatory=$true)][string]$ArchiveRoot, [Parameter(Mandatory=$true)][object[]]$Inventory)
    $source=[IO.Path]::GetFullPath($SourceRoot).TrimEnd('\','/')
    $archive=[IO.Path]::GetFullPath($ArchiveRoot).TrimEnd('\','/')
    if($source.Equals($archive,[StringComparison]::OrdinalIgnoreCase) -or $archive.StartsWith($source+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Archive is not independent of its source'}
    $current=@(Get-GscEvidenceInventory $source)
    if($current.Count -ne $Inventory.Count){throw 'Source evidence set changed after archival'}
    $sourceCheck=Test-GscEvidenceArchive $Inventory $source
    if(-not $sourceCheck.Verified){throw 'Source evidence changed after archival'}
    $archiveCheck=Test-GscEvidenceArchive $Inventory $archive
    if(-not $archiveCheck.Verified){throw ('Archive incomplete: '+($archiveCheck.Issues -join '; '))}
    # This establishes preservation only. Ownership, active processes, references,
    # path scope and file occupancy remain mandatory before any actual cleanup.
    return $archiveCheck
}

function Test-GscEvidenceArchive {
    param([Parameter(Mandatory=$true)][object[]]$Inventory, [Parameter(Mandatory=$true)][string]$ArchiveRoot)
    $actual = @(Get-GscEvidenceInventory -Root $ArchiveRoot)
    $map = @{}
    foreach ($entry in $actual) { $map[$entry.Path] = $entry }
    $issues = [Collections.Generic.List[string]]::new()
    $matched = 0
    $seen = @{}
    foreach ($entry in $Inventory) {
        $relative = [string]$entry.Path
        if ([string]::IsNullOrWhiteSpace($relative) -or [IO.Path]::IsPathRooted($relative) -or $relative.Split([char[]]@('/','\')) -contains '..') { throw 'Invalid inventory path' }
        if ($seen.ContainsKey($relative)) { throw 'Duplicate inventory path' }
        $seen[$relative] = $true
        if (-not $map.ContainsKey($relative)) { $issues.Add('Missing: ' + $relative); continue }
        if ($map[$relative].SHA256 -ne $entry.SHA256 -or $map[$relative].Bytes -ne $entry.Bytes) { $issues.Add('Changed: ' + $relative) }
        else { $matched++ }
    }
    if ($Inventory.Count -eq 0) { $issues.Add('Empty inventory cannot prove an archive') }
    [pscustomobject]@{ Verified=($issues.Count -eq 0); Issues=$issues.ToArray(); VerifiedFileCount=$matched; CanDelete=$false }
}
