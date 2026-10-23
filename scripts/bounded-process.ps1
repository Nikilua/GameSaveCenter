# Bounded native process execution. The Process object owns the launched PID;
# no process-name based termination is used.
function Invoke-GscBoundedProcess {
    param([string]$FilePath, [string[]]$Arguments, [int]$TimeoutSeconds)
    $quoted = foreach ($argument in $Arguments) {
        '"' + [regex]::Replace([regex]::Replace($argument, '(\\*)"', '$1$1\"'), '(\\+)$', '$1$1') + '"'
    }
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $FilePath
    $info.Arguments = $quoted -join ' '
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.StandardOutputEncoding = [Text.UTF8Encoding]::new($false)
    $info.StandardErrorEncoding = [Text.UTF8Encoding]::new($false)
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $info
    try {
        if (-not $process.Start()) { throw 'Native process did not start' }
        $processId = $process.Id
        $startTime = $process.StartTime.ToUniversalTime().ToString('o')
        $timer = [Diagnostics.Stopwatch]::StartNew()
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $exited = $process.WaitForExit($TimeoutSeconds * 1000)
        if (-not $exited) {
            # Kill only the process object created above. Retain evidence and
            # report failure even if the process races to exit after the deadline.
            try {
                if ($PSVersionTable.PSEdition -eq 'Core') { $process.Kill($true) }
                else { $process.Kill() }
            }
            catch { if (-not $process.HasExited) { throw } }
            [void]$process.WaitForExit(5000)
        }
        $remaining = [Math]::Max(0, ($TimeoutSeconds * 1000) - [int]$timer.ElapsedMilliseconds)
        $drained = [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout, $stderr), $remaining)
        $text = ''
        if ($stdout.IsCompleted) { $text += $stdout.GetAwaiter().GetResult() }
        if ($stderr.IsCompleted) { $text += $stderr.GetAwaiter().GetResult() }
        [pscustomobject]@{ ExitCode = $(if ($exited -and $drained) { $process.ExitCode } else { 124 }); Output = $text; TimedOut = (-not $exited -or -not $drained); OutputDrained = $drained; ProcessId = $processId; ProcessStartedUtc = $startTime; ProcessExited = $process.HasExited }
    }
    finally { $process.Dispose() }
}
