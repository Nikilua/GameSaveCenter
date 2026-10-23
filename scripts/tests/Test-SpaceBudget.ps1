[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/space-budget.ps1')
$req=@(@{Stage='output';Path=(Join-Path $repo 'artifacts');BudgetBytes=300;Basis='fixture'},@{Stage='temp';Path=$env:TEMP;BudgetBytes=200;Basis='fixture'},@{Stage='package';Path=(Join-Path $repo 'artifacts');BudgetBytes=100;Basis='fixture'})
$low=@(Get-GscSpaceBudget $req {param($p) @{Root='same-volume';AvailableBytes=599}})
if($low.Count -ne 1 -or $low[0].BudgetBytes -ne 600) {throw 'Same-volume budget not aggregated'}
$refused=$false;try {Assert-GscSpaceBudget $low} catch {$refused=$true}
if(-not $refused) {throw 'Low space was allowed'}
$retry=@(Get-GscSpaceBudget $req {param($p) @{Root='same-volume';AvailableBytes=600}})
Assert-GscSpaceBudget $retry
$unknown=$false;try {Get-GscSpaceBudget $req {param($p) @{Root='unknown'}}}catch{$unknown=$true}
if(-not $unknown){throw 'Unknown free space accepted'}
$e=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-03'
New-Item -ItemType Directory $e -Force|Out-Null
@{Engine=$PSVersionTable.PSVersion.ToString();SameVolumeAggregated=$true;InsufficientRefused=$true;RetrySucceeded=$true;UnknownRefused=$true;Transport='Injected volume reader; no real disk exhaustion'}|ConvertTo-Json|Set-Content (Join-Path $e ('budget-'+$PSVersionTable.PSEdition+'.json'))
Write-Host 'Space budget 4/4 passed'
