[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/checkout-lock.ps1')
$engine = (Get-Process -Id $PID).Path
$fixtureRoot = Join-Path $repo ('.tmp/checkout-lock-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $fixtureRoot | Out-Null
$helper = (Join-Path $repo 'scripts/checkout-lock.ps1').Replace("'", "''")
$fixture = $fixtureRoot.Replace("'", "''")
$probe = ". '$helper'; try { `$m=Enter-GscCheckoutLock '$fixture'; Exit-GscCheckoutLock `$m; exit 0 } catch { exit 23 }"
$encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($probe))
$lock = Enter-GscCheckoutLock $fixtureRoot
try {
    $nested = Enter-GscCheckoutLock $fixtureRoot
    Exit-GscCheckoutLock $nested
    & $engine -NoProfile -EncodedCommand $encoded
    if ($LASTEXITCODE -ne 23) { throw 'Concurrent lock was allowed' }
} finally { Exit-GscCheckoutLock $lock }
& $engine -NoProfile -EncodedCommand $encoded
if ($LASTEXITCODE -ne 0) { throw 'Released lock was not recoverable' }
# Child exits while owning its mutex; kernel recovery requires no stale-file deletion.
$crash = ". '$helper'; `$m=Enter-GscCheckoutLock '$fixture'; [Environment]::Exit(17)"
& $engine -NoProfile -EncodedCommand ([Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($crash)))
if ($LASTEXITCODE -ne 17) { throw 'Crash fixture did not execute' }
$recovered = Enter-GscCheckoutLock $fixtureRoot
Exit-GscCheckoutLock $recovered
# Different checkout remains independent while this one is locked.
$lock = Enter-GscCheckoutLock $fixtureRoot
try { $other = Enter-GscCheckoutLock (Join-Path $fixtureRoot 'other'); Exit-GscCheckoutLock $other }
finally { Exit-GscCheckoutLock $lock }
$evidence = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-07'
New-Item -ItemType Directory $evidence -Force | Out-Null
@{ Engine=$PSVersionTable.PSVersion.ToString(); Reentrant=$true; ConcurrentRefused=$true; ReleasedRetry=$true; ExitedOwnerRecovery=$true; OtherCheckoutIndependent=$true; Scope='Current Windows session' } | ConvertTo-Json | Set-Content (Join-Path $evidence ('lock-' + $PSVersionTable.PSEdition + '.json'))
$allowed = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar
if (-not ([IO.Path]::GetFullPath($fixtureRoot)).StartsWith($allowed)) { throw 'Unsafe cleanup' }
Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
Write-Host 'Checkout mutex: 5/5 passed'
