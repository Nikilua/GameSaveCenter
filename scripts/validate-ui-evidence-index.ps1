[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$AuditRoot,
    [int]$MinimumRows = 20
)

$ErrorActionPreference = 'Stop'
$auditRootFull = [System.IO.Path]::GetFullPath($AuditRoot)
$indexPath = Join-Path $auditRootFull 'EVIDENCE_INDEX.md'
if (-not (Test-Path -LiteralPath $indexPath -PathType Leaf)) {
    throw "Evidence index is missing: $indexPath"
}
if ($MinimumRows -lt 1) {
    throw "MinimumRows must be positive."
}

$rows = @(Select-String -LiteralPath $indexPath -Pattern '^\| E\d{2} \|' | ForEach-Object { $_.Line })
if ($rows.Count -lt $MinimumRows) {
    throw "Evidence index has $($rows.Count) rows; expected at least $MinimumRows."
}

$expectedIds = @{}
for ($i = 1; $i -le $MinimumRows; $i++) {
    $expectedIds[('E{0:00}' -f $i)] = $false
}

$referenceCount = 0
$identityCount = 0
$sampleCount = 0
$boundaryCount = 0
foreach ($row in $rows) {
    $cells = $row.Trim().Trim('|').Split('|')
    if ($cells.Count -lt 8) {
        throw "Evidence row has fewer than 8 columns: $row"
    }

    $id = $cells[0].Trim()
    if ($expectedIds.ContainsKey($id)) {
        $expectedIds[$id] = $true
    }

    $result = $cells[4].Trim()
    if (($result -match '(UI_MANIFEST\.md|LAYOUT_REPORT\.md|UI_FIDELITY_MATRIX\.md)') -and ($result -match 'source=src\\')) {
        $referenceCount++
    }

    if ($cells[5].Trim() -match '^`[0-9a-f]{40}`$') {
        $identityCount++
    }

    if ($cells[6].Trim() -and $cells[6].Trim() -notmatch '^unknown$') {
        $sampleCount++
    }

    if ($cells[7].Trim() -match '未验|没有本轮') {
        $boundaryCount++
    }
}

$missingIds = @($expectedIds.GetEnumerator() | Where-Object { -not $_.Value } | Select-Object -ExpandProperty Key)
if ($missingIds.Count -gt 0) {
    throw "Evidence index is missing expected IDs: $($missingIds -join ', ')"
}
if ($referenceCount -lt $MinimumRows) {
    throw "Only $referenceCount rows have a report and source reference."
}
if ($identityCount -lt $MinimumRows) {
    throw "Only $identityCount rows have a 40-character code identity."
}
if ($sampleCount -lt $MinimumRows) {
    throw "Only $sampleCount rows have a non-empty sample field."
}
if ($boundaryCount -lt $MinimumRows) {
    throw "Only $boundaryCount rows have an explicit unverified boundary."
}

foreach ($requiredFile in @('UI_MANIFEST.md', 'LAYOUT_REPORT.md', 'UI_FIDELITY_MATRIX.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $auditRootFull $requiredFile) -PathType Leaf)) {
        throw "Evidence index references missing report: $requiredFile"
    }
}

Write-Output "Evidence index validation OK: rows=$($rows.Count), references=$referenceCount/$($rows.Count), identities=$identityCount/$($rows.Count), samples=$sampleCount/$($rows.Count), boundaries=$boundaryCount/$($rows.Count)"
