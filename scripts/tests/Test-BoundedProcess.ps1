[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/build-diagnostics.ps1')
$root = Join-Path $repo ('.tmp/bounded-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
$context = New-GscDiagnosticContext $repo $root
$engine = (Get-Process -Id $PID).Path
$trx = Join-Path $root 'completed.trx'
$fixture = Join-Path $root 'hang.ps1'
[IO.File]::WriteAllText($fixture, 'param($Trx); [IO.File]::WriteAllText($Trx,''<TestRun><ResultSummary outcome="Completed"/></TestRun>''); Write-Output ''cases completed''; Start-Sleep 60')
$unrelated = Start-Process $engine -ArgumentList '-NoProfile -Command "Start-Sleep 60"' -WindowStyle Hidden -PassThru
try {
    $result = Invoke-GscRecordedCommand $context 'trx-then-hang' $engine @('-NoProfile','-File',$fixture,$trx) -TimeoutSeconds 2
    if ($result.ExitCode -ne 124 -or -not (Test-Path $trx)) { throw 'TRX existence masked timeout or fixture did not complete' }
    if ($unrelated.HasExited) { throw 'Unrelated process was terminated' }
    $success = Invoke-GscRecordedCommand $context 'retry' $engine @('-NoProfile','-Command','Write-Output ''retry completed''; exit 0') -TimeoutSeconds 10
    if ($success.ExitCode -ne 0) { throw 'Retry inherited timeout' }
    $unicode = Invoke-GscRecordedCommand $context 'unicode' $engine @('-NoProfile','-Command','[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false); Write-Output ([char]0x4e2d+[string][char]0x6587); exit 0') -TimeoutSeconds 10
    if (($unicode.Output -join '') -notmatch ([string][char]0x4e2d+[string][char]0x6587)) { throw 'Unicode output corrupted' }
    if ($PSVersionTable.PSEdition -eq 'Core') {
        $pidFile = Join-Path $root 'child.pid'
        $tree = Join-Path $root 'tree.ps1'
        [IO.File]::WriteAllText($tree, 'param($PidFile); $p=Start-Process (Get-Process -Id $PID).Path -ArgumentList ''-NoProfile -Command "Start-Sleep 60"'' -WindowStyle Hidden -PassThru; [IO.File]::WriteAllText($PidFile,[string]$p.Id); Start-Sleep 60')
        $treeResult = Invoke-GscRecordedCommand $context 'tree-timeout' $engine @('-NoProfile','-File',$tree,$pidFile) -TimeoutSeconds 3
        if (-not (Test-Path $pidFile)) { throw 'Child fixture did not start' }
        $childId = [int](Get-Content $pidFile)
        $child = Get-Process -Id $childId -ErrorAction SilentlyContinue
        if ($child -and -not $child.HasExited) { throw 'Owned child survived tree timeout' }
        if ($treeResult.ExitCode -ne 124) { throw 'Tree timeout reported success' }
    }
    # cmd starts a background child inheriting redirected stdout, then exits.
    # The command process exit alone must not discard the child's later output.
    $pipeFixture = Join-Path $root 'pipe.cmd'
    [IO.File]::WriteAllText($pipeFixture, '@echo off' + "`r`n" + 'start "" /b powershell.exe -NoProfile -Command "Start-Sleep 4; Write-Output delayed"' + "`r`n" + 'exit /b 0')
    $pipe = Invoke-GscRecordedCommand $context 'pipe-held' 'cmd.exe' @('/d','/c',$pipeFixture) -TimeoutSeconds 1
    if ($pipe.ExitCode -ne 124 -or $context.Steps[$context.Steps.Count-1].process.OutputDrained) { throw 'Inherited pipe incorrectly reported success' }
    Start-Sleep -Seconds 4
    $evidence = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-06'
    @{ Engine=$PSVersionTable.PSVersion.ToString(); TrxBeforeTimeout=$true; TimeoutExit=124; UnrelatedProcessPreserved=$true; RetryExit=0; Steps=$context.Steps } | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $evidence ('bounded-' + $PSVersionTable.PSEdition + '.json'))
} finally { if (-not $unrelated.HasExited) { $unrelated.Kill() }; $unrelated.Dispose() }
if (-not ([IO.Path]::GetFullPath($root)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar)) { throw 'Unsafe cleanup' }
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'TRX/hang, unrelated process and retry passed'
