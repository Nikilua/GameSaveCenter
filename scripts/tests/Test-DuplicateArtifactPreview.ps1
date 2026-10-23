[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/artifact-preview.ps1')
$root=Join-Path $repo ('.tmp/duplicate-'+[guid]::NewGuid().ToString('N'))
$dir=Join-Path $root 'artifacts';$docs=Join-Path $root 'docs'
New-Item -ItemType Directory $dir,$docs -Force|Out-Null
$a=Join-Path $dir 'first.zip';$b=Join-Path $dir 'second.pext';$stale=Join-Path $dir 'stale.zip'
foreach($p in @($a,$b,$stale)){[IO.File]::WriteAllText($p,'same')}
$hash=(Get-FileHash $a).Hash
[IO.File]::WriteAllText((Join-Path $docs 'reference.md'),'Keep second.pext for rollback')
$entries=@(foreach($p in @($a,$b,$stale)){@{Path=$p;SHA256=$hash;CreatedByThisRun=$true;Active=$false;Retention='Current package'}})
[IO.File]::WriteAllText($stale,'changed')
$m=@{owner=@{ProcessId=$PID;ProcessStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')};artifacts=$entries}
$rows=@(Get-GscDuplicateArtifactPreview $root @($m,$m))
$group=@($rows|Where-Object {$_.Copies -eq 2})
if($group.Count -ne 1){throw 'Duplicate path double counted or stale file adopted'}
if(@($group[0].Artifacts|Where-Object {$_.References.Count -gt 0}).Count -ne 1){throw 'Reference missing'}
if(@($rows|Where-Object {$_.Action -eq 'Excluded'}).Count -ne 2){throw 'Stale hash not excluded'}
if((Get-FileHash $a).Hash -ne $hash -or (Get-FileHash $b).Hash -ne $hash){throw 'Preview modified files'}
$imageA=Join-Path $dir 'audit-first.png';$imageB=Join-Path $dir 'audit-second.png';$unknown=Join-Path $dir 'user-image.png'
foreach($p in @($imageA,$imageB,$unknown)){[IO.File]::WriteAllText($p,'identical isolated visual fixture')}
$imageHash=(Get-FileHash -LiteralPath $imageA).Hash
[IO.File]::WriteAllText((Join-Path $docs 'visual.md'),'Unique evidence reference audit-second.png')
$images=@{owner=$m.owner;artifacts=@(foreach($p in @($imageA,$imageB)){@{Path=$p;SHA256=$imageHash;CreatedByThisRun=$true;Active=$false;Retention='Current audit evidence'}})}
$imageRows=@(Get-GscDuplicateArtifactPreview $root @($images))
if($imageRows.Count -ne 1 -or $imageRows[0].Copies -ne 2){throw 'Screenshot group adopted unknown file or lost a known copy'}
if(@($imageRows[0].Artifacts|Where-Object {$_.References.Count -gt 0}).Count -ne 1){throw 'Referenced screenshot was not protected'}
if(@($imageRows[0].Artifacts|Where-Object {$_.CanDelete}).Count){throw 'Screenshot preview authorized deletion'}
if((Get-FileHash -LiteralPath $unknown).Hash -ne $imageHash){throw 'Unknown image was changed'}
$e=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-07'
@{Engine=$PSVersionTable.PSVersion.ToString();ActualHashGroups=$true;StaleExcluded=$true;PathsDeduplicated=$true;ReferencesRetained=$true;FilesUnchanged=$true;ScreenshotHashGroup=$true;ScreenshotReferenceRetained=$true;UnknownScreenshotExcluded=$true}|ConvertTo-Json|Set-Content (Join-Path $e ('duplicates-'+$PSVersionTable.PSEdition+'.json'))
if(-not ([IO.Path]::GetFullPath($root)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar)){throw 'Unsafe cleanup'}
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'Duplicate preview passed'
