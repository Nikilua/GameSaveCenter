[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/build-diagnostics.ps1')
$root = Join-Path $repo ('.tmp/ownership-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
$unknown = Join-Path $root 'unknown-user-file'
[IO.File]::WriteAllText($unknown, 'untouched')
$before = (Get-FileHash $unknown).Hash
$a = New-GscDiagnosticContext $repo $root
$b = New-GscDiagnosticContext $repo $root
if ($a.Owner.RunId -eq $b.Owner.RunId) { throw 'Runs share identity' }
if ($a.Owner.ProcessId -ne $PID -or -not $a.Owner.ProcessStartedUtc) { throw 'Owner identity missing' }
if ($a.Artifacts.Count -ne 1 -or -not $a.Artifacts[0].Active -or -not $a.Artifacts[0].CreatedByThisRun) { throw 'Diagnostic root ownership incorrect' }
if (@($a.Artifacts | Where-Object { $_.Path -eq $unknown }).Count) { throw 'Unknown file was adopted' }
Complete-GscDiagnostics $a $true
Complete-GscDiagnostics $b $false
$finished = Get-Content (Join-Path $a.RunRoot 'summary.json') -Raw | ConvertFrom-Json
$failed = Get-Content (Join-Path $b.RunRoot 'summary.json') -Raw | ConvertFrom-Json
if ($finished.artifacts[0].Active -or $finished.outcome -ne 'passed' -or $failed.outcome -ne 'failed') { throw 'Completion states incorrect' }
if ((Get-FileHash $unknown).Hash -ne $before) { throw 'Unknown file changed' }
$evidence = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-01'
New-Item -ItemType Directory $evidence -Force | Out-Null
@{DistinctRuns=$true;OwnerPidAndStart=$true;ActiveToCompleted=$true;FailureRetained=$true;UnknownFileUnchanged=$true;Engine=$PSVersionTable.PSVersion.ToString()} | ConvertTo-Json | Set-Content (Join-Path $evidence ('ownership-' + $PSVersionTable.PSEdition + '.json'))
$allowed = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar
if (-not ([IO.Path]::GetFullPath($root)).StartsWith($allowed)) { throw 'Unsafe cleanup' }
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'Ownership lifecycle: passed'
