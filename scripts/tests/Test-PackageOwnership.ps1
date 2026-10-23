[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/package-ownership.ps1')
$root=Join-Path $repo ('.tmp/package-owner-'+[guid]::NewGuid().ToString('N'))
$artifacts=Join-Path $root 'artifacts';$stage=Join-Path $artifacts 'stage'
New-Item -ItemType Directory $stage -Force|Out-Null
$zip=Join-Path $artifacts 'delivery.zip';$pext=Join-Path $artifacts 'delivery.pext';$unknown=Join-Path $artifacts 'unknown.zip'
foreach($path in @($zip,$pext,$unknown)){[IO.File]::WriteAllText($path,'fixture')}
$hash=(Get-FileHash $unknown).Hash
Write-GscPackageOwnership $root $stage @($zip,$pext) 'fixture-commit'
Write-GscPackageOwnership $root $stage @() 'fixture-retry'
$files=@(Get-ChildItem (Join-Path $artifacts 'package-ownership') -Filter *.json)
if($files.Count -ne 2){throw 'Retry overwrote prior manifest'}
$records=@($files|ForEach-Object {Get-Content $_.FullName -Raw|ConvertFrom-Json})
$record=@($records|Where-Object {$_.sourceCommit -eq 'fixture-commit'})[0]
if($record.artifacts.Count -ne 3 -or $record.owner.ProcessId -ne $PID){throw 'Incomplete package manifest'}
foreach($entry in $record.artifacts){if($entry.Path -eq $unknown){throw 'Unknown package adopted'};if($entry.SHA256 -and $entry.SHA256 -ne (Get-FileHash $entry.Path).Hash){throw 'Hash mismatch'}}
Write-GscPackageOwnership $root (Join-Path $artifacts 'missing-stage') @() 'failed-fixture' -FailureMessage 'controlled package failure'
$failure=@(Get-ChildItem (Join-Path $artifacts 'package-ownership') -Filter *.json|ForEach-Object {Get-Content $_.FullName -Raw|ConvertFrom-Json}|Where-Object {$_.sourceCommit -eq 'failed-fixture'})[0]
if($failure.outcome -ne 'failed' -or $failure.artifacts[0].CreatedByThisRun -or $failure.artifacts[0].Exists){throw 'Failed package wrongly owns missing output'}
$refused=$false;try{Write-GscPackageOwnership $root $unknown @((Join-Path $root 'outside')) 'invalid'}catch{$refused=$true}
if(-not $refused){throw 'Outside artifact allowed'}
if((Get-FileHash $unknown).Hash -ne $hash){throw 'Unknown changed'}
$e=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-01'
@{Engine=$PSVersionTable.PSVersion.ToString();DistinctRetry=$true;ExplicitArtifactsOnly=$true;HashVerified=$true;UnknownUnchanged=$true;OutsideRejected=$true}|ConvertTo-Json|Set-Content (Join-Path $e ('package-owner-'+$PSVersionTable.PSEdition+'.json'))
if(-not ([IO.Path]::GetFullPath($root)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar)){throw 'Unsafe cleanup'}
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'Package ownership passed'
