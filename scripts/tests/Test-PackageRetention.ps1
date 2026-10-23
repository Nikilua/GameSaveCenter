[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $repo 'scripts/package.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw $errors}
$blocks=@($ast.FindAll({param($n) $n -is [Management.Automation.Language.IfStatementAst] -and $n.Clauses[0].Item1.Extent.Text -eq '-not $SkipPackageArchives' -and $n.Extent.Text -match 'Compress-Archive'},$true))
if($blocks.Count -ne 1){throw 'Archive production block ambiguous'}
$artifacts=Join-Path $repo ('.tmp/package-retention-'+[guid]::NewGuid().ToString('N'))
$stage=Join-Path $artifacts 'stage'
New-Item -ItemType Directory $stage -Force|Out-Null
[IO.File]::WriteAllText((Join-Path $stage 'extension.yaml'),'Version: 1.2.3')
$old=Join-Path $artifacts 'GameSaveCenter-0.0.1-playnite.zip'
$unknown=Join-Path $artifacts 'GameSaveCenter-user.pext'
[IO.File]::WriteAllText($old,'rollback');[IO.File]::WriteAllText($unknown,'unknown')
$before=@((Get-FileHash $old).Hash,(Get-FileHash $unknown).Hash)
$packageVersion='1.2.3';$SkipPackageArchives=$false;$SelfContainedWorker=$false
function Assert-PackageContents {param($PackagePath,$ExpectedVersion,$ExpectedSelfContained) if(-not(Test-Path $PackagePath)){throw 'No package'} }
Invoke-Expression $blocks[0].Extent.Text
if((Get-FileHash $old).Hash -ne $before[0] -or (Get-FileHash $unknown).Hash -ne $before[1]){throw 'Other package changed'}
if((Get-FileHash $zip).Hash -ne (Get-FileHash $pext).Hash){throw 'Current archives mismatch'}
$e=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-07'
New-Item -ItemType Directory $e -Force|Out-Null
@{Engine=$PSVersionTable.PSVersion.ToString();RollbackPreserved=$true;UnknownPackagePreserved=$true;CurrentZipPextMatch=$true;Scope='Real archive production block and Compress-Archive; package content validator fixture'}|ConvertTo-Json|Set-Content (Join-Path $e ('retention-'+$PSVersionTable.PSEdition+'.json'))
if(-not ([IO.Path]::GetFullPath($artifacts)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar)){throw 'Unsafe cleanup'}
Remove-Item -LiteralPath $artifacts -Recurse -Force
Write-Host 'Package retention passed'
