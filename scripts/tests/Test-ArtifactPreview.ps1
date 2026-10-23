[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/artifact-preview.ps1')
$root = Join-Path $repo ('.tmp/preview-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
$sentinel = Join-Path $root 'sentinel'
[IO.File]::WriteAllText($sentinel,'preserve')
$hash = (Get-FileHash $sentinel).Hash
$owner = @{ProcessId=$PID;ProcessStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')}
$cases = @(
    @{Path=$root;Expected='Owner process still active'},
    @{Path='';Expected='Rejected:*'},
    @{Path='.tmp/relative';Expected='Rejected:*'},
    @{Path=(Join-Path $repo '.tmp/../../outside');Expected='Rejected:*'},
    @{Path=(Join-Path $repo '.tmp-other/file');Expected='Rejected:*'},
    @{Path=(Join-Path $repo '.tmp');Expected='Rejected:*'}
)
$rows = @()
foreach($case in $cases) {
    $m = @{owner=$owner;artifacts=@(@{Path=$case.Path;CreatedByThisRun=$true;Active=$false;Retention=''})}
    $r = @(Get-GscArtifactPreview $repo $m)[0]
    if($r.Reason -notlike $case.Expected -or $r.CanDelete) {throw "Preview failed: $($case.Path): $($r.Reason)"}
    $rows += $r
}
# New PowerShell JSON readers materialize ISO timestamps as DateTime.
$roundTrip = @{owner=$owner;artifacts=@(@{Path=$root;CreatedByThisRun=$true;Active=$false;Retention=''})} | ConvertTo-Json -Depth 5 | ConvertFrom-Json
$typed = @(Get-GscArtifactPreview $repo $roundTrip)[0]
if ($typed.OwnerState -ne 'active') { throw 'JSON owner timestamp lost precision or timezone' }
$rows += $typed
# Same PID with a different start time must not be treated as its old owner.
$m = @{owner=@{ProcessId=$PID;ProcessStartedUtc='2000-01-01T00:00:00Z'};artifacts=@(@{Path=$root;CreatedByThisRun=$true;Active=$false;Retention='current evidence'})}
$r = @(Get-GscArtifactPreview $repo $m)[0]
if($r.OwnerState -ne 'different-process' -or $r.Reason -notlike 'Retention:*') {throw 'PID reuse handling failed'}
$rows += $r
$outside = Join-Path $root 'outside'
New-Item -ItemType Directory $outside | Out-Null
$link = Join-Path $root 'junction'
New-Item -ItemType Junction -Path $link -Target $outside | Out-Null
try {
    $m.artifacts[0].Path=$link
    $r = @(Get-GscArtifactPreview $repo $m)[0]
    if($r.Reason -notlike 'Rejected:*Linked*') {throw 'Junction not rejected'}
    $rows += $r
} finally { Remove-Item -LiteralPath $link -Force }
if((Get-FileHash $sentinel).Hash -ne $hash) {throw 'Preview changed data'}
$evidence=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-02'
New-Item -ItemType Directory $evidence -Force | Out-Null
$rows | ConvertTo-Json | Set-Content (Join-Path $evidence ('preview-' + $PSVersionTable.PSEdition + '.json'))
$allowed=[IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar
if(-not ([IO.Path]::GetFullPath($root)).StartsWith($allowed)) {throw 'Unsafe cleanup'}
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'Artifact preview: 9/9 passed; sentinel unchanged'
