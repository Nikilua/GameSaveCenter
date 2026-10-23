# Read-only review of explicitly registered artifacts. Never deletes files.
function Get-GscArtifactPreview {
    param([Parameter(Mandatory=$true)][string]$RepositoryRoot, [Parameter(Mandatory=$true)]$Manifest)
    $repo = [IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\','/')
    $allowed = @((Join-Path $repo 'artifacts'), (Join-Path $repo '.tmp'))
    $ownerState = 'unknown'
    if ($Manifest.owner -and $Manifest.owner.ProcessId -and $Manifest.owner.ProcessStartedUtc) {
        try {
            $owner = Get-Process -Id $Manifest.owner.ProcessId -ErrorAction Stop
            $rawStarted = $Manifest.owner.ProcessStartedUtc
            $expected = if ($rawStarted -is [DateTime]) { $rawStarted.ToUniversalTime() }
            elseif ($rawStarted -is [DateTimeOffset]) { $rawStarted.UtcDateTime }
            else { [DateTimeOffset]::Parse([string]$rawStarted, [Globalization.CultureInfo]::InvariantCulture).UtcDateTime }
            $ownerState = if ($owner.StartTime.ToUniversalTime().Ticks -eq $expected.Ticks) { 'active' } else { 'different-process' }
        }
        catch [Microsoft.PowerShell.Commands.ProcessCommandException] { $ownerState = 'exited' }
        catch { $ownerState = 'unknown' }
    }
    foreach ($artifact in $Manifest.artifacts) {
        $reason = ''; $path = ''; $bytes = $null
        try {
            if ([string]::IsNullOrWhiteSpace($artifact.Path)) { throw 'Empty artifact path' }
            $raw = [string]$artifact.Path
            if ($raw.StartsWith('[repo]')) { $raw = $repo + $raw.Substring(6) }
            if (-not [IO.Path]::IsPathRooted($raw)) { throw 'Relative artifact path' }
            $path = [IO.Path]::GetFullPath($raw)
            $scope = @($allowed | Where-Object { $path.StartsWith($_ + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) })
            if ($scope.Count -ne 1) { throw 'Outside allowed artifact roots or root itself' }
            # Reject links at every ancestor, including the repository root.
            $cursor = $path
            while ($cursor -and $cursor.Length -ge $repo.Length) {
                if (Test-Path -LiteralPath $cursor) {
                    $item = Get-Item -LiteralPath $cursor -Force -ErrorAction Stop
                    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Linked path boundary' }
                }
                if ($cursor -eq $repo) { break }
                $cursor = Split-Path -Parent $cursor
            }
            if (-not $artifact.CreatedByThisRun) { $reason = 'Ownership not claimed' }
            elseif ($ownerState -eq 'active') { $reason = 'Owner process still active' }
            elseif ($ownerState -eq 'unknown') { $reason = 'Owner identity unavailable' }
            elseif ($artifact.Active) { $reason = 'Artifact still marked active' }
            elseif (-not [string]::IsNullOrWhiteSpace($artifact.Retention)) { $reason = 'Retention: ' + $artifact.Retention }
            elseif (-not (Test-Path -LiteralPath $path)) { $reason = 'Already absent' }
            else { $reason = 'Review candidate; references and file occupancy still require verification' }
            if (Test-Path -LiteralPath $path) {
                $items = @(Get-Item -LiteralPath $path -Force -ErrorAction Stop)
                if ($items[0].PSIsContainer) { $items = @(Get-ChildItem -LiteralPath $path -Recurse -Force -ErrorAction Stop) }
                if (@($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Linked descendant' }
                $bytes = ($items | Where-Object { -not $_.PSIsContainer } | Measure-Object Length -Sum).Sum
            }
        }
        catch { $reason = 'Rejected: ' + $_.Exception.Message }
        [pscustomobject]@{ Path=$path; EstimatedBytes=$bytes; OwnerState=$ownerState; Reason=$reason; CanDelete=$false }
    }
}
