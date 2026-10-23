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
        $reason = ''; $path = ''; $bytes = $null; $occupancyState = 'not-checked'; $occupancyIssue = $null
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
                if ($ownerState -eq 'active') { $occupancyState = 'owner-active' }
                elseif ($ownerState -eq 'unknown') { $occupancyState = 'owner-unknown' }
                else {
                    $occupancyState = 'clear'
                    foreach ($file in @($items | Where-Object { -not $_.PSIsContainer })) {
                        $probe = $null
                        try {
                            $probe = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
                        }
                        catch {
                            $openException = $_.Exception
                            while ($openException.InnerException) { $openException = $openException.InnerException }
                            $win32Code = $openException.HResult -band 0xFFFF
                            $occupancyState = if ($openException -is [IO.IOException] -and ($win32Code -eq 32 -or $win32Code -eq 33)) { 'in-use' } else { 'unavailable' }
                            $occupancyIssue = [IO.Path]::GetFileName($file.FullName)
                            break
                        }
                        finally {
                            if ($probe) { $probe.Dispose() }
                        }
                    }
                }
                if ($occupancyState -in @('in-use', 'unavailable') -and $reason -notlike 'Rejected:*') {
                    $reason += "; file occupancy $occupancyState`: $occupancyIssue"
                }
            }
        }
        catch { $reason = 'Rejected: ' + $_.Exception.Message }
        [pscustomobject]@{ Path=$path; EstimatedBytes=$bytes; RunId=$Manifest.owner.RunId; OwnerState=$ownerState; OccupancyState=$occupancyState; OccupancyIssue=$occupancyIssue; Reason=$reason; CanDelete=$false }
    }
}
function Get-GscDuplicateArtifactPreview {
    param([string]$RepositoryRoot, [object[]]$Manifests)
    $files=@{}; $rejected=[Collections.Generic.List[object]]::new()
    $docs=Join-Path $RepositoryRoot 'docs'
    $references=@(Get-ChildItem -LiteralPath $docs -Recurse -File -ErrorAction Stop | Where-Object {$_.Extension -in '.md','.json'} | ForEach-Object {@{Path=$_.FullName;Text=[IO.File]::ReadAllText($_.FullName)}})
    foreach($manifest in $Manifests) {
        foreach($entry in $manifest.artifacts) {
            if(-not $entry.SHA256){continue}
            $one=@{owner=$manifest.owner;artifacts=@($entry)}
            $preview=@(Get-GscArtifactPreview $RepositoryRoot $one)[0]
            if($preview.Reason -like 'Rejected:*' -or -not (Test-Path -LiteralPath $preview.Path -PathType Leaf)) {
                $rejected.Add(@{Path=$preview.Path;Reason=$preview.Reason});continue
            }
            $actual=(Get-FileHash -LiteralPath $preview.Path -Algorithm SHA256).Hash
            if($actual -ne $entry.SHA256) {$rejected.Add(@{Path=$preview.Path;Reason='Recorded hash is stale; file ownership is not transferable'});continue}
            $key=$preview.Path.ToUpperInvariant()
            $matches=@($references|Where-Object {$_.Text.IndexOf($preview.Path,[StringComparison]::OrdinalIgnoreCase) -ge 0 -or $_.Text.IndexOf([IO.Path]::GetFileName($preview.Path),[StringComparison]::OrdinalIgnoreCase) -ge 0}|ForEach-Object {$_.Path})
            $files[$key]=@{Path=$preview.Path;SHA256=$actual;Bytes=(Get-Item -LiteralPath $preview.Path).Length;References=$matches;Reason=$preview.Reason;CanDelete=$false}
        }
    }
    foreach($group in @($files.Values|Group-Object SHA256|Where-Object {$_.Count -gt 1})) {
        [pscustomobject]@{SHA256=$group.Name;Copies=$group.Count;Artifacts=@($group.Group);Action='Review only; current delivery, references, owner and occupancy must remain protected'}
    }
    foreach($item in $rejected){[pscustomobject]@{SHA256=$null;Copies=0;Artifacts=@($item);Action='Excluded'}}
}
