[CmdletBinding()]
param([ValidateRange(2,120)][int]$TimeoutSeconds = 30)

$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$tmpRoot = [IO.Path]::GetFullPath((Join-Path $repo '.tmp'))
$fixtureRoot = [IO.Path]::GetFullPath((Join-Path $tmpRoot ('T00-07-cross-session-' + [guid]::NewGuid().ToString('N'))))
$resultPath = Join-Path $fixtureRoot 'worker-result.json'
$taskName = 'GameSaveCenterCheckoutLockTest-' + [guid]::NewGuid().ToString('N')
$currentSessionId = [Diagnostics.Process]::GetCurrentProcess().SessionId
$record = [ordered]@{
    Engine = $PSVersionTable.PSVersion.ToString()
    TaskName = $taskName
    TaskCreated = $false
    CurrentSessionId = $currentSessionId
    WorkerSessionId = $null
    CrossSession = $false
    LockRefused = $false
    Outcome = 'failed'
    Error = $null
    TaskRemoved = $true
    FixtureRemoved = $false
}
$checkoutLock = $null
$taskRegistered = $false
$failure = $null

if (-not $fixtureRoot.StartsWith($tmpRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Unsafe test fixture path.'
}
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null

try {
    . (Join-Path $repo 'scripts/checkout-lock.ps1')
    $checkoutLock = Enter-GscCheckoutLock $repo

    $repoLiteral = "'" + $repo.Replace("'", "''") + "'"
    $resultLiteral = "'" + $resultPath.Replace("'", "''") + "'"
    $worker = @"
`$ErrorActionPreference = 'Stop'
`$repo = $repoLiteral
`$resultPath = $resultLiteral
. (Join-Path `$repo 'scripts/checkout-lock.ps1')
`$outcome = @{ Acquired = `$false; SessionId = [Diagnostics.Process]::GetCurrentProcess().SessionId; Error = `$null }
try {
    `$mutex = Enter-GscCheckoutLock `$repo
    Exit-GscCheckoutLock `$mutex
    `$outcome.Acquired = `$true
} catch {
    `$outcome.Error = `$_.Exception.Message
}
`$outcome | ConvertTo-Json | Set-Content -LiteralPath `$resultPath -Encoding UTF8
"@
    $encodedWorker = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($worker))
    $engine = (Get-Process -Id $PID).Path
    $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $action = New-ScheduledTaskAction -Execute $engine -Argument "-NoProfile -NonInteractive -EncodedCommand $encodedWorker"
    $principal = New-ScheduledTaskPrincipal -UserId $user -LogonType S4U -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet -ExecutionTimeLimit (New-TimeSpan -Minutes 2)
    $task = New-ScheduledTask -Action $action -Principal $principal -Settings $settings
    Register-ScheduledTask -TaskName $taskName -InputObject $task | Out-Null
    $taskRegistered = $true
    $record.TaskCreated = $true
    $record.TaskRemoved = $false

    Start-ScheduledTask -TaskName $taskName
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while (-not (Test-Path -LiteralPath $resultPath) -and [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 200
    }
    if (-not (Test-Path -LiteralPath $resultPath)) {
        throw "Cross-session worker did not report within $TimeoutSeconds seconds."
    }

    $workerResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    $record.WorkerSessionId = [int]$workerResult.SessionId
    $record.CrossSession = $record.WorkerSessionId -ne $currentSessionId
    $expectedRefusal = 'This checkout already has an active build/install run. Wait for it to finish, then retry.'
    $record.LockRefused = -not [bool]$workerResult.Acquired -and $workerResult.Error -ceq $expectedRefusal
    if (-not $record.CrossSession) { throw 'Scheduled worker ran in the same Windows session; cross-session behavior was not tested.' }
    if (-not $record.LockRefused) { throw 'Cross-session worker did not receive the expected active-checkout refusal.' }
    $record.Outcome = 'passed'
} catch {
    $failure = $_
    $record.Error = $_.Exception.Message
} finally {
    if ($taskRegistered) {
        try { Stop-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue } catch { }
        try {
            Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction Stop
            $record.TaskRemoved = $true
        } catch {
            if (-not $record.Error) { $record.Error = 'Failed to remove the temporary scheduled task: ' + $_.Exception.Message }
            $record.Outcome = 'failed'
        }
    }
    if ($checkoutLock) {
        try { Exit-GscCheckoutLock $checkoutLock } catch {
            if (-not $record.Error) { $record.Error = 'Failed to release the checkout mutex: ' + $_.Exception.Message }
            $record.Outcome = 'failed'
        }
    }
    try {
        if (Test-Path -LiteralPath $fixtureRoot) {
            Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
        }
        $record.FixtureRemoved = -not (Test-Path -LiteralPath $fixtureRoot)
    } catch {
        if (-not $record.Error) { $record.Error = 'Failed to remove the isolated fixture: ' + $_.Exception.Message }
        $record.Outcome = 'failed'
    }

    $evidence = Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-07/cross-session-current.json'
    New-Item -ItemType Directory -Path (Split-Path -Parent $evidence) -Force | Out-Null
    $record | ConvertTo-Json | Set-Content -LiteralPath $evidence -Encoding UTF8
}

if ($failure) { throw $failure }
if ($record.Outcome -ne 'passed' -or -not $record.TaskRemoved -or -not $record.FixtureRemoved) {
    throw 'Cross-session checkout-lock test or its cleanup did not pass.'
}
Write-Host "Checkout mutex cross-session: $($record.Outcome) (session $currentSessionId -> $($record.WorkerSessionId)); temporary task removed=$($record.TaskRemoved), fixture removed=$($record.FixtureRemoved)."
