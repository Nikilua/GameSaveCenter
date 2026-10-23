[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/package-ownership.ps1')
$t=$null;$e=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $repo 'scripts/package.ps1'),[ref]$t,[ref]$e)
if($e.Count){throw $e}
$tries=@($ast.FindAll({param($n) $n -is [Management.Automation.Language.TryStatementAst] -and $null -ne $n.Finally -and $n.Finally.Extent.Text -match 'previousBuildCommit' -and $n.Body.Extent.Text -match 'Write-GscPackageOwnership'},$true))
if($tries.Count -ne 1){throw 'Main package try is ambiguous'}
$fixture=Join-Path $repo ('.tmp/package-failure-'+[guid]::NewGuid().ToString('N'))
$root=$fixture;$stage=Join-Path $root 'artifacts/stage'
New-Item -ItemType Directory $stage -Force|Out-Null
$sentinel=Join-Path $stage 'sentinel'
[IO.File]::WriteAllText($sentinel,'prior-stage')
$hash=(Get-FileHash $sentinel).Hash
$previousBuildCommit=$env:GSC_BUILD_COMMIT
$env:GSC_BUILD_COMMIT='fixture-source'
$code=$tries[0].Extent.Text.Replace($tries[0].Body.Extent.Text,"{ throw 'controlled-package-failure' }")
$caught=''
try{Invoke-Expression $code}catch{$caught=$_.Exception.Message}
if($caught -notmatch 'controlled-package-failure'){throw 'Original failure was masked'}
$records=@(Get-ChildItem (Join-Path $root 'artifacts/package-ownership') -Filter *.json|ForEach-Object {Get-Content $_.FullName -Raw|ConvertFrom-Json})
if($records.Count -ne 1 -or $records[0].outcome -ne 'failed' -or $records[0].artifacts[0].CreatedByThisRun){throw 'Failure record absent or adopted old stage'}
if((Get-FileHash $sentinel).Hash -ne $hash){throw 'Failure evidence changed prior stage'}
if($env:GSC_BUILD_COMMIT -ne $previousBuildCommit){throw 'Build environment not restored'}
$out=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-01'
@{Engine=$PSVersionTable.PSVersion.ToString();ProductionCatchExecuted=$true;OriginalErrorPreserved=$true;FailureRecordWritten=$true;PreviousStageUnchanged=$true;EnvironmentRestored=$true;Fixture='Only main try body replaced with controlled throw; production catch/finally unchanged'}|ConvertTo-Json|Set-Content (Join-Path $out ('package-failure-'+$PSVersionTable.PSEdition+'.json'))
if(-not ([IO.Path]::GetFullPath($fixture)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar)){throw 'Unsafe cleanup'}
Remove-Item -LiteralPath $fixture -Recurse -Force
Write-Host 'Package failure control flow passed'
