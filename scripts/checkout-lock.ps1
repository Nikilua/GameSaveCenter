# Kernel mutexes recover automatically when their owning process exits.
function Enter-GscCheckoutLock {
    param([Parameter(Mandatory=$true)][string]$RepositoryRoot)
    $canonical = [IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\','/').ToUpperInvariant()
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $key = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical))).Replace('-','') }
    finally { $sha.Dispose() }
    $mutex = [Threading.Mutex]::new($false, ('Local\GameSaveCenter.Build.' + $key))
    try {
        $acquired = $false
        try { $acquired = $mutex.WaitOne(0) }
        catch [Threading.AbandonedMutexException] { $acquired = $true }
        if (-not $acquired) { throw 'This checkout already has an active build/install run. Wait for it to finish, then retry.' }
        return $mutex
    }
    catch { $mutex.Dispose(); throw }
}
function Exit-GscCheckoutLock {
    param([Threading.Mutex]$Mutex)
    if ($null -ne $Mutex) { try { $Mutex.ReleaseMutex() } finally { $Mutex.Dispose() } }
}
