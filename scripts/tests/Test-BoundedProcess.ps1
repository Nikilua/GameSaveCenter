[CmdletBinding()]
param([string]$EvidenceRoot = '')
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
    # Both PowerShell editions must stop an owned descendant, not only its parent.
    $pidFile = Join-Path $root 'child.pid'
    $tree = Join-Path $root 'tree.ps1'
    [IO.File]::WriteAllText($tree, 'param($PidFile); $p=Start-Process (Get-Process -Id $PID).Path -ArgumentList ''-NoProfile -Command "Start-Sleep 60"'' -WindowStyle Hidden -PassThru; [IO.File]::WriteAllText($PidFile,([string]$p.Id+","+[string]$p.StartTime.ToUniversalTime().Ticks)); Start-Sleep 60')
    $treeResult = Invoke-GscRecordedCommand $context 'tree-timeout' $engine @('-NoProfile','-File',$tree,$pidFile) -TimeoutSeconds 3
    if (-not (Test-Path $pidFile)) { throw 'Child fixture did not start' }
    $witness = (Get-Content -LiteralPath $pidFile -Raw).Trim().Split(',')
    $childId = [int]$witness[0]
    $childStartTicks = [long]$witness[1]
    $child = Get-Process -Id $childId -ErrorAction SilentlyContinue
    if ($child -and -not $child.HasExited -and $child.StartTime.ToUniversalTime().Ticks -eq $childStartTicks) { throw 'Owned child survived tree timeout' }
    if ($treeResult.ExitCode -ne 124) { throw 'Tree timeout reported success' }
    if ($unrelated.HasExited) { throw 'Tree cleanup terminated an unrelated process' }
    if (@($context.Steps | Where-Object id -EQ 'tree-timeout').Count -ne 1) { throw 'Owned tree fixture was not executed' }
    # Interrupt the managed wait after a real child has started. This models an
    # interrupted host wait without claiming a physical Ctrl+C keyboard event.
    if (-not ('GscBoundedWaitInterruptor' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Threading;
public static class GscBoundedWaitInterruptor {
    public static Thread Start(Thread target, string markerPath, int delayMilliseconds) {
        var watcher = new Thread(() => {
            var deadline = DateTime.UtcNow.AddSeconds(8);
            while (!File.Exists(markerPath) && DateTime.UtcNow < deadline) Thread.Sleep(25);
            if (File.Exists(markerPath)) {
                Thread.Sleep(delayMilliseconds);
                target.Interrupt();
            }
        });
        watcher.IsBackground = true;
        watcher.Start();
        return watcher;
    }
}
'@
    }
    $interruptWitnessPath = Join-Path $root 'interrupted-child.json'
    $interruptFixture = Join-Path $root 'interrupted-child.ps1'
    [IO.File]::WriteAllText($interruptFixture, @'
param($WitnessPath)
$owned = Get-Process -Id $PID
@{ ProcessId=$PID; StartUtcTicks=$owned.StartTime.ToUniversalTime().Ticks } | ConvertTo-Json | Set-Content -LiteralPath $WitnessPath
Start-Sleep -Seconds 60
'@)
    $interruptWatcher = [GscBoundedWaitInterruptor]::Start([Threading.Thread]::CurrentThread, $interruptWitnessPath, 300)
    $interruptionTimer = [Diagnostics.Stopwatch]::StartNew()
    $waitWasInterrupted = $false
    try {
        [void](Invoke-GscBoundedProcess $engine @('-NoProfile','-File',$interruptFixture,$interruptWitnessPath) 15)
    }
    catch {
        $exception = $_.Exception
        while ($null -ne $exception) {
            if ($exception -is [Threading.ThreadInterruptedException]) { $waitWasInterrupted = $true; break }
            $exception = $exception.InnerException
        }
        if (-not $waitWasInterrupted) { throw }
    }
    finally {
        $interruptionTimer.Stop()
        if (-not $interruptWatcher.Join(5000)) { throw 'Wait interruption fixture did not finish' }
    }
    if (-not $waitWasInterrupted -or $interruptionTimer.Elapsed.TotalSeconds -ge 10) { throw 'Managed wait was not interrupted before timeout' }
    if (-not (Test-Path -LiteralPath $interruptWitnessPath)) { throw 'Interrupted child did not write its identity witness' }
    $interruptWitness = Get-Content -LiteralPath $interruptWitnessPath -Raw | ConvertFrom-Json
    $interruptedChild = Get-Process -Id ([int]$interruptWitness.ProcessId) -ErrorAction SilentlyContinue
    if ($interruptedChild -and $interruptedChild.StartTime.ToUniversalTime().Ticks -eq [long]$interruptWitness.StartUtcTicks) { throw 'Owned child survived interrupted wait cleanup' }
    if ($unrelated.HasExited) { throw 'Wait interruption cleanup terminated an unrelated process' }
    $afterInterruption = Invoke-GscRecordedCommand $context 'retry-after-wait-interruption' $engine @('-NoProfile','-Command','exit 0') -TimeoutSeconds 10
    if ($afterInterruption.ExitCode -ne 0) { throw 'Retry after wait interruption inherited a stale cancellation state' }
    # cmd starts a background child inheriting redirected stdout, then exits.
    # The command process exit alone must not discard the child's later output.
    $pipeFixture = Join-Path $root 'pipe.cmd'
    [IO.File]::WriteAllText($pipeFixture, '@echo off' + "`r`n" + 'start "" /b powershell.exe -NoProfile -Command "Start-Sleep 4; Write-Output delayed"' + "`r`n" + 'exit /b 0')
    $pipe = Invoke-GscRecordedCommand $context 'pipe-held' 'cmd.exe' @('/d','/c',$pipeFixture) -TimeoutSeconds 1
    if ($pipe.ExitCode -ne 124 -or $context.Steps[$context.Steps.Count-1].process.OutputDrained) { throw 'Inherited pipe incorrectly reported success' }
    Start-Sleep -Seconds 4
    $evidence = if ($EvidenceRoot) { [IO.Path]::GetFullPath($EvidenceRoot) } else { Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-06' }
    New-Item -ItemType Directory -Path $evidence -Force | Out-Null
    $evidenceJson = @{ Engine=$PSVersionTable.PSVersion.ToString(); TrxBeforeTimeout=$true; TimeoutExit=124; OwnedDescendantStopped=$true; OwnedDescendantPid=$childId; OwnedDescendantStartedUtcTicks=$childStartTicks; WaitInterruptedBeforeTimeout=$waitWasInterrupted; InterruptedProcessStopped=$true; InterruptedProcessId=[int]$interruptWitness.ProcessId; InterruptedProcessStartedUtcTicks=[long]$interruptWitness.StartUtcTicks; CancellationSignal='managed-thread wait interruption; physical Ctrl+C not exercised'; RetryAfterInterruptionExit=$afterInterruption.ExitCode; UnrelatedProcessPreserved=$true; RetryExit=0; Steps=$context.Steps } | ConvertTo-Json -Depth 8
    $evidenceJson = ConvertTo-GscDiagnosticText $evidenceJson $context
    [IO.File]::WriteAllText((Join-Path $evidence ('bounded-' + $PSVersionTable.PSEdition + '.json')), $evidenceJson, [Text.UTF8Encoding]::new($false))
} finally { if (-not $unrelated.HasExited) { $unrelated.Kill() }; $unrelated.Dispose() }
if (-not ([IO.Path]::GetFullPath($root)).StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar)) { throw 'Unsafe cleanup' }
Remove-Item -LiteralPath $root -Recurse -Force
Write-Host 'TRX/hang, unrelated process and retry passed'
