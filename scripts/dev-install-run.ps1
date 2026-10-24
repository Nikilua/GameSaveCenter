[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$PlayniteExtensionsPath = '',
    [string]$PlayniteExecutable = '',
    [string]$TestTempRoot = '',
    [string]$BuildOutputRoot = '',
    [string]$RunLogPath = '',
    [string]$InstallReportPath = '',
    [switch]$SkipTests,
    [switch]$NoStart,
    [switch]$SkipPlayniteShutdownForIsolatedTarget,
    [switch]$SkipClean,
    [switch]$SkipPackageArchives
)

$ErrorActionPreference = 'Stop'
$installerRevision = 'DEV-INSTALL-009'

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
try {
    [Console]::InputEncoding = $utf8NoBom
    [Console]::OutputEncoding = $utf8NoBom
    $OutputEncoding = $utf8NoBom
}
catch {
    # 某些宿主不允许修改控制台编码；不影响构建和安装。
}

$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'checkout-lock.ps1')
$checkoutLock = Enter-GscCheckoutLock $root
try {
. (Join-Path $PSScriptRoot 'space-budget.ps1')
$spaceBudget = Invoke-GscSpacePreflight $root $BuildOutputRoot $TestTempRoot -IncludePackage
. (Join-Path $PSScriptRoot 'sdk-baseline.ps1')
$hostIsolationHelpers = Join-Path $PSScriptRoot 'PlayniteHostIsolation.ps1'
. $hostIsolationHelpers
if ($SkipPlayniteShutdownForIsolatedTarget) {
    if (-not $NoStart) {
        throw 'SkipPlayniteShutdownForIsolatedTarget requires -NoStart.'
    }
    if ([string]::IsNullOrWhiteSpace($PlayniteExtensionsPath)) {
        throw 'SkipPlayniteShutdownForIsolatedTarget requires an explicit isolated PlayniteExtensionsPath under repository .tmp.'
    }

    $PlayniteExtensionsPath = Resolve-GscRepositoryScopedPath `
        -RepositoryRoot $root `
        -Path $PlayniteExtensionsPath `
        -ScopeRootName '.tmp'
    $isolationConflicts = @(Get-GscPlayniteConflictingProcesses)
    Assert-GscNoActivePlayniteProcesses -Processes $isolationConflicts
}

$artifactsPath = Join-Path $root 'artifacts'
New-Item $artifactsPath -ItemType Directory -Force | Out-Null
# Keep the isolated root short enough for the .NET Framework test adapter.
# The configuration is already passed to build.ps1, so it does not need to be
# repeated in the filesystem path.
$buildToken = ([Guid]::NewGuid().ToString('N')).Substring(0, 8)
$installRunId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
$buildOutputRoot = if ([string]::IsNullOrWhiteSpace($BuildOutputRoot)) {
    Join-Path $artifactsPath ("gsc-b\{0}" -f $buildToken)
}
else {
    $resolvedBuildOutputRoot = [System.IO.Path]::GetFullPath($BuildOutputRoot)
    $temporaryRoot = [System.IO.Path]::GetFullPath((Join-Path $root '.tmp'))
    $temporaryPrefix = $temporaryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedBuildOutputRoot.StartsWith($temporaryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "BuildOutputRoot must be inside repository .tmp: $resolvedBuildOutputRoot"
    }
    $resolvedBuildOutputRoot
}
$runLogPath = Join-Path $artifactsPath 'one-click-install.log'
$reportPath = Join-Path $root 'artifacts\last-dev-install.txt'
$artifactsPrefix = [System.IO.Path]::GetFullPath($artifactsPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if (-not [string]::IsNullOrWhiteSpace($RunLogPath)) {
    $resolvedRunLogPath = [System.IO.Path]::GetFullPath($RunLogPath)
    if (-not $resolvedRunLogPath.StartsWith($artifactsPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "RunLogPath must be inside repository artifacts: $resolvedRunLogPath"
    }
    $runLogPath = $resolvedRunLogPath
}
if (-not [string]::IsNullOrWhiteSpace($InstallReportPath)) {
    $resolvedInstallReportPath = [System.IO.Path]::GetFullPath($InstallReportPath)
    if (-not $resolvedInstallReportPath.StartsWith($artifactsPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "InstallReportPath must be inside repository artifacts: $resolvedInstallReportPath"
    }
    $reportPath = $resolvedInstallReportPath
}
$transcriptStarted = $false
try {
    Start-Transcript -Path $runLogPath -Force | Out-Null
    $transcriptStarted = $true
}
catch {
    Write-Warning "无法启动安装日志记录：$($_.Exception.Message)"
}

Write-Host "一键安装器：$installerRevision" -ForegroundColor DarkCyan
Write-Host "安装脚本：$PSCommandPath" -ForegroundColor DarkCyan
Write-Host "安装运行 ID：$installRunId" -ForegroundColor DarkCyan

$extensionId = 'GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec'

function Read-ManifestVersion {
    param([Parameter(Mandatory = $true)][string]$ManifestPath)

    if (-not (Test-Path $ManifestPath)) {
        throw "找不到扩展清单：$ManifestPath"
    }

    $line = Get-Content $ManifestPath | Where-Object { $_ -match '^Version\s*:\s*(.+?)\s*$' } | Select-Object -First 1
    if (-not $line -or $line -notmatch '^Version\s*:\s*(.+?)\s*$') {
        throw "扩展清单缺少 Version：$ManifestPath"
    }

    return $Matches[1].Trim()
}

function Get-PlayniteExecutableCandidates {
    param([string]$PreferredPath, [string[]]$RunningPaths)

    $candidates = [System.Collections.Generic.List[string]]::new()
    if ($PreferredPath) { $candidates.Add($PreferredPath) }
    foreach ($path in $RunningPaths) {
        if ($path) { $candidates.Add($path) }
    }

    $candidates.Add((Join-Path $env:LOCALAPPDATA 'Playnite\Playnite.DesktopApp.exe'))
    $candidates.Add((Join-Path $env:LOCALAPPDATA 'Programs\Playnite\Playnite.DesktopApp.exe'))
    $candidates.Add((Join-Path $env:ProgramFiles 'Playnite\Playnite.DesktopApp.exe'))
    if (${env:ProgramFiles(x86)}) {
        $candidates.Add((Join-Path ${env:ProgramFiles(x86)} 'Playnite\Playnite.DesktopApp.exe'))
    }

    # Portable/custom Playnite installations may not use the standard folders or
    # register an uninstall entry. App Paths and PATH are cheap additional sources
    # that work without scanning whole drives.
    foreach ($appPathRegistry in @(
        'HKCU:\Software\Microsoft\Windows\CurrentVersion\App Paths\Playnite.DesktopApp.exe',
        'HKLM:\Software\Microsoft\Windows\CurrentVersion\App Paths\Playnite.DesktopApp.exe',
        'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\Playnite.DesktopApp.exe'
    )) {
        try {
            $appPathEntry = Get-ItemProperty -LiteralPath $appPathRegistry -ErrorAction SilentlyContinue
            if ($appPathEntry -and $appPathEntry.'(default)') {
                $candidates.Add([string]$appPathEntry.'(default)')
            }
        }
        catch {
            # Optional discovery source; registry access must not block installation.
        }
    }

    try {
        $pathCommand = Get-Command 'Playnite.DesktopApp.exe' -CommandType Application -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($pathCommand -and $pathCommand.Source) {
            $candidates.Add([string]$pathCommand.Source)
        }
    }
    catch {
        # PATH discovery is optional.
    }

    foreach ($registryPath in @(
        'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
    )) {
        try {
            Get-ItemProperty $registryPath -ErrorAction SilentlyContinue |
                Where-Object { $_.DisplayName -like 'Playnite*' } |
                ForEach-Object {
                    if ($_.InstallLocation) {
                        $candidates.Add((Join-Path $_.InstallLocation 'Playnite.DesktopApp.exe'))
                    }
                    if ($_.DisplayIcon) {
                        $iconPath = ($_.DisplayIcon -split ',')[0].Trim('"')
                        if ($iconPath -like '*.exe') { $candidates.Add($iconPath) }
                    }
                }
        }
        catch {
            # 注册表发现失败不阻断安装。
        }
    }

    return $candidates |
        Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } |
        ForEach-Object {
            try { (Resolve-Path -LiteralPath $_ -ErrorAction Stop).Path } catch { }
        } |
        Select-Object -Unique
}

function Get-ExtensionRoots {
    param(
        [string]$ExplicitPath,
        [string[]]$PlayniteExecutables
    )

    if ($ExplicitPath) {
        return @([System.IO.Path]::GetFullPath($ExplicitPath))
    }

    $candidates = [System.Collections.Generic.List[string]]::new()
    $candidates.Add((Join-Path $env:APPDATA 'Playnite\Extensions'))
    $candidates.Add((Join-Path $env:LOCALAPPDATA 'Playnite\Extensions'))

    foreach ($exe in $PlayniteExecutables) {
        $directory = Split-Path -Parent $exe
        if ($directory) { $candidates.Add((Join-Path $directory 'Extensions')) }
    }

    $existingInstallRoots = @($candidates |
        Where-Object { $_ -and (Test-Path (Join-Path $_ $extensionId)) } |
        Select-Object -Unique)

    if ($existingInstallRoots.Count -gt 0) {
        return $existingInstallRoots
    }

    return @((Join-Path $env:APPDATA 'Playnite\Extensions'))
}

function Stop-PlayniteAndOwnedWorkerReliably {
    param(
        [Parameter(Mandatory = $true)][string[]]$ExtensionRoots,
        [AllowEmptyCollection()][string[]]$TrustedPlayniteExecutables
    )

    $playniteNames = @('Playnite.DesktopApp', 'Playnite.FullscreenApp')
    $requestedShutdown = @{}
    $playniteDeadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        $runningPlaynite = @($playniteNames | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
        foreach ($process in $runningPlaynite) {
            if ($requestedShutdown.ContainsKey($process.Id)) { continue }
            $requestedShutdown[$process.Id] = $true
            try {
                if ($process.HasExited) { continue }
                if ($process.CloseMainWindow()) {
                    Write-Host "请求 Playnite 正常退出：$($process.ProcessName) [$($process.Id)]" -ForegroundColor DarkYellow
                }
                else {
                    Write-Warning "无法请求 Playnite 正常退出：$($process.ProcessName) [$($process.Id)]。请先手动退出 Playnite。"
                }
            }
            catch {
                Write-Warning "请求 Playnite 退出失败：$($process.ProcessName) [$($process.Id)]：$($_.Exception.Message)"
            }
        }

        if ($runningPlaynite.Count -eq 0) { break }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $playniteDeadline)

    $runningPlaynite = @($playniteNames | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
    if ($runningPlaynite.Count -gt 0) {
        $trustedPaths = @($TrustedPlayniteExecutables |
                Where-Object { $_ } |
                ForEach-Object { [System.IO.Path]::GetFullPath($_) } |
                Select-Object -Unique)
        $currentSessionId = (Get-Process -Id $PID).SessionId

        foreach ($process in $runningPlaynite) {
            try {
                $process.Refresh()
                if ($process.HasExited) { continue }
            }
            catch {
                if (-not (Get-Process -Id $process.Id -ErrorAction SilentlyContinue)) { continue }
            }

            $processPath = $null
            try { $processPath = $process.Path } catch { }
            if ([string]::IsNullOrWhiteSpace($processPath)) {
                throw "Playnite [$($process.Id)] 正常退出超时，且无法确认可执行文件路径；为避免误杀未知进程，安装已停止。"
            }

            $fullProcessPath = [System.IO.Path]::GetFullPath($processPath)
            if ($trustedPaths.Count -eq 0) {
                # The initial discovery can race with process startup or fail to
                # read a path transiently. A path read from this exact process is
                # sufficient to establish the identity needed by the guards below.
                $trustedPaths = @($fullProcessPath)
            }
            if (-not ($trustedPaths | Where-Object { [string]::Equals($_, $fullProcessPath, [StringComparison]::OrdinalIgnoreCase) })) {
                throw "Playnite [$($process.Id)] 正常退出超时，但路径不属于本次发现的 Playnite：$fullProcessPath。为避免误杀，安装已停止。"
            }
            if ($process.SessionId -ne $currentSessionId) {
                throw "Playnite [$($process.Id)] 正常退出超时，但不属于当前用户会话；安装已停止。"
            }
            if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
                throw "Playnite [$($process.Id)] 仍有主窗口且正常退出超时。请先保存操作并手动退出 Playnite 后重试。"
            }

            try {
                Write-Host "结束无窗口的 Playnite 残留进程：$($process.ProcessName) [$($process.Id)]" -ForegroundColor DarkYellow
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
            }
            catch {
                # 进程可能在 Refresh 与 Stop-Process 之间自行退出；这不是安装失败。
                if (Get-Process -Id $process.Id -ErrorAction SilentlyContinue) {
                    throw "无法结束无窗口的 Playnite 残留进程 [$($process.Id)]：$($_.Exception.Message)"
                }
            }
        }

        Start-Sleep -Milliseconds 500
        $remainingPlaynite = @($playniteNames | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
        if ($remainingPlaynite.Count -gt 0) {
            throw "Playnite 残留进程仍在运行：$($remainingPlaynite.Id -join ', ')。安装已停止。"
        }
    }

    Write-Host 'Playnite 已退出，等待插件回收 Worker...' -ForegroundColor DarkYellow
    $workerNames = @('GameSaveCenter.Worker')
    $workerDeadline = [DateTime]::UtcNow.AddSeconds(10)
    do {
        $workers = @($workerNames | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
        if ($workers.Count -eq 0) { return }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $workerDeadline)

    $workerPaths = @($ExtensionRoots | ForEach-Object {
            Join-Path (Join-Path $_ $extensionId) 'Worker\GameSaveCenter.Worker.exe'
        } | ForEach-Object { [System.IO.Path]::GetFullPath($_) })
    $ownedWorkers = [System.Collections.Generic.List[object]]::new()
    foreach ($worker in $workers) {
        $workerPath = $null
        try { $workerPath = $worker.MainModule.FileName } catch { }
        if ([string]::IsNullOrWhiteSpace($workerPath)) {
            throw "Playnite 已退出，但无法确认 Worker [$($worker.Id)] 的文件路径；为避免误杀未知进程，安装已停止。请手动结束该 Worker 后重试。"
        }
        $fullWorkerPath = [System.IO.Path]::GetFullPath($workerPath)
        if (-not ($workerPaths | Where-Object {
                    [string]::Equals($_, $fullWorkerPath, [StringComparison]::OrdinalIgnoreCase)
                })) {
            # A different preview/plugin extension can legitimately own the same
            # executable name. It is unsafe to terminate it, but it also does not
            # lock the current extension directory, so leave it running and let the
            # install continue. Unknown paths still fail closed above.
            Write-Warning "保留其他扩展目录的 Worker [$($worker.Id)]：$fullWorkerPath。安装器不会结束它。"
            continue
        }

        $ownedWorkers.Add($worker)
    }

    foreach ($worker in $ownedWorkers) {

        try {
            Write-Host "停止扩展残留 Worker：$($worker.Id)" -ForegroundColor DarkYellow
            Stop-Process -Id $worker.Id -Force -ErrorAction Stop
        }
        catch {
            throw "Playnite 已退出，但无法停止当前扩展的 Worker [$($worker.Id)]：$($_.Exception.Message)。安装器不会自动请求管理员权限，请使用与 Playnite 相同的用户权限结束它后重试。"
        }
    }

    Start-Sleep -Milliseconds 500
    $remainingOwnedWorkers = [System.Collections.Generic.List[object]]::new()
    foreach ($remainingWorker in @($workerNames | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })) {
        $remainingPath = $null
        try { $remainingPath = $remainingWorker.MainModule.FileName } catch { }
        if ([string]::IsNullOrWhiteSpace($remainingPath)) {
            throw "扩展 Worker [$($remainingWorker.Id)] 仍在运行且无法确认文件路径；安装已停止以避免覆盖正在使用的文件。"
        }

        $fullRemainingPath = [System.IO.Path]::GetFullPath($remainingPath)
        if ($workerPaths | Where-Object {
                    [string]::Equals($_, $fullRemainingPath, [StringComparison]::OrdinalIgnoreCase)
                }) {
            $remainingOwnedWorkers.Add($remainingWorker)
        }
        else {
            Write-Warning "其他扩展目录的 Worker 仍在运行 [$($remainingWorker.Id)]：$fullRemainingPath。安装已继续，不会结束它。"
        }
    }
    if ($remainingOwnedWorkers.Count -gt 0) {
        $remainingOwnedIds = ($remainingOwnedWorkers | ForEach-Object { $_.Id }) -join ', '
        throw "当前扩展 Worker 仍在运行：$remainingOwnedIds。安装已停止以避免覆盖正在使用的文件。"
    }
}

function Get-GscInstalledAssemblyIdentities {
    param([Parameter(Mandatory = $true)][string]$Target)

    foreach ($relative in @('GameSaveCenter.Playnite.dll', 'GameSaveCenter.Core.dll', 'GameSaveCenter.Contracts.dll',
        'Worker/GameSaveCenter.Worker.dll', 'Worker/GameSaveCenter.Core.dll', 'Worker/GameSaveCenter.Contracts.dll')) {
        $path = [IO.Path]::GetFullPath((Join-Path $Target $relative))
        $exists = Test-Path -LiteralPath $path -PathType Leaf
        $identity = 'missing'
        $sha256 = 'unavailable'
        if ($exists) {
            $identity = (Get-Item -LiteralPath $path).VersionInfo.ProductVersion
            if ([string]::IsNullOrWhiteSpace($identity)) { $identity = 'unknown' }
            $sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256 -ErrorAction Stop).Hash
        }
        [pscustomobject]@{ File = $relative; Path = $path; Exists = $exists; BuildIdentity = $identity; SHA256 = $sha256 }
    }
}

function Install-ExtensionAtomically {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$ExtensionsRoot,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [string]$DiagnosticRoot = '',
        [string]$RunId = ''
    )

    New-Item $ExtensionsRoot -ItemType Directory -Force | Out-Null
    $target = Join-Path $ExtensionsRoot $extensionId
    # Never reuse a fixed staging directory. Older elevated/aborted installers may
    # have left `<extension>.__new` with an ACL that the current user cannot open;
    # a unique staging path lets a normal user continue without taking ownership or
    # requesting UAC. The old directory is harmless and is not touched.
    $temporary = "$target.__new-$([Guid]::NewGuid().ToString('N'))"

    $attemptId = [Guid]::NewGuid().ToString('N')
    $ownerStart = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
    $effectiveRunId = if ([string]::IsNullOrWhiteSpace($RunId)) {
        [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + $attemptId.Substring(0, 8)
    } else { $RunId }
    $backup = "$target.__old-$([Guid]::NewGuid().ToString('N'))"
    $stagingCreatedByThisRun = $false
    $backupCreatedByThisRun = $false
    $attempt = @{
        SchemaVersion=2; RunId=$effectiveRunId; AttemptId=$attemptId; Stage='install-transaction'
        Outcome='failed'; StartedUtc=[DateTime]::UtcNow.ToString('o')
        Owner=@{ProcessId=$PID;ProcessStartedUtc=$ownerStart}; OwnerPid=$PID; OwnerStart=$ownerStart
        Temporary=$temporary; Target=$target; ExpectedVersion=$ExpectedVersion
    }
    try {
    Copy-Item $Source $temporary -Recurse -Force
    $stagingCreatedByThisRun = $true

    $temporaryManifest = Join-Path $temporary 'extension.yaml'
    $temporaryVersion = Read-ManifestVersion $temporaryManifest
    if ($temporaryVersion -ne $ExpectedVersion) {
        throw "暂存扩展版本错误：期望 $ExpectedVersion，实际 $temporaryVersion。"
    }

    $stagedDll = Join-Path $temporary 'GameSaveCenter.Playnite.dll'
    if (-not (Test-Path -LiteralPath $stagedDll)) { throw "暂存扩展缺少插件 DLL：$stagedDll" }
    $stagedFileVersion = (Get-Item -LiteralPath $stagedDll).VersionInfo.FileVersion
    if ($stagedFileVersion -and -not $stagedFileVersion.StartsWith("$ExpectedVersion.")) {
        throw "暂存 DLL 版本错误：$stagedFileVersion"
    }
    # Refuse occupied files before changing the old installation. Recursive
    # deletion can otherwise remove unrelated files before reaching a locked DLL.
    $oldMoved = $false
    try {
        if (Test-Path -LiteralPath $target) {
            foreach ($file in Get-ChildItem -LiteralPath $target -File -Recurse -Force) {
                $probe = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
                $probe.Dispose()
            }
            Move-Item -LiteralPath $target -Destination $backup -ErrorAction Stop
            $oldMoved = $true
            $backupCreatedByThisRun = $true
        }
        Move-Item -LiteralPath $temporary -Destination $target -ErrorAction Stop
    }
    catch {
        if ($oldMoved -and -not (Test-Path -LiteralPath $target)) {
            Move-Item -LiteralPath $backup -Destination $target -ErrorAction Stop
        }
        throw
    }

    $installedManifest = Join-Path $target 'extension.yaml'
    $installedVersion = Read-ManifestVersion $installedManifest
    $installedDll = Join-Path $target 'GameSaveCenter.Playnite.dll'
    if (-not (Test-Path $installedDll)) {
        throw "安装后缺少插件 DLL：$installedDll"
    }

    $fileVersion = (Get-Item $installedDll).VersionInfo.FileVersion
    if ($installedVersion -ne $ExpectedVersion) {
        throw "安装验证失败：清单版本应为 $ExpectedVersion，实际为 $installedVersion。"
    }
    if ($fileVersion -and -not $fileVersion.StartsWith("$ExpectedVersion.")) {
        throw "安装验证失败：DLL 文件版本应为 $ExpectedVersion.x，实际为 $fileVersion。"
    }
    $installedIdentities = @(Get-GscInstalledAssemblyIdentities -Target $target)

    if ($oldMoved) {
        try { Remove-Item -LiteralPath $backup -Recurse -Force -ErrorAction Stop }
        catch { Write-Warning "旧安装备份保留于 $backup ：$($_.Exception.Message)" }
    }
    $attempt.Outcome = 'passed'
    return [pscustomobject]@{
        Target = $target
        ManifestVersion = $installedVersion
        FileVersion = $fileVersion
        AssemblyIdentities = $installedIdentities
    }
    }
    catch { $attempt.FailureMessage = $_.Exception.Message; throw }
    finally {
        if ($DiagnosticRoot) {
            try {
                New-Item -ItemType Directory -Path $DiagnosticRoot -Force | Out-Null
                $attempt.CompletedUtc = [DateTime]::UtcNow.ToString('o')
                $temporaryExists = Test-Path -LiteralPath $temporary
                $backupExists = Test-Path -LiteralPath $backup
                $stagingCreatedByThisRun = $stagingCreatedByThisRun -or $temporaryExists
                $attempt.TemporaryExists = $temporaryExists
                $attempt.Artifacts = @(
                    @{
                        Path=$temporary; Stage='install-staging'; CreatedByThisRun=$stagingCreatedByThisRun
                        Exists=$temporaryExists; Active=$false
                        Retention=$(if ($temporaryExists) { 'Failed install staging; retain for diagnosis and manual ownership review' } else { 'Staging moved or removed; retain this identity record' })
                    }
                )
                if ($backupCreatedByThisRun -or $backupExists) {
                    $attempt.Artifacts += @{
                        Path=$backup; Stage='install-backup'; CreatedByThisRun=$backupCreatedByThisRun
                        Exists=$backupExists; Active=$false
                        Retention=$(if ($backupExists) { 'Prior installation backup; preserve for rollback and manual review' } else { 'Backup restored or removed; retain this identity record' })
                    }
                }
                $attempt.Retention = 'Install attempt evidence; external extension paths must never be passed to repository cleanup'
                $attempt | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $DiagnosticRoot ([guid]::NewGuid().ToString('N')+'.json')) -Encoding UTF8
            }
            catch { Write-Warning "Install attempt evidence could not be written: $($_.Exception.Message)" }
        }
    }
}

Push-Location $root
try {
    # Resolve or install the exact SDK before touching the running host. A first
    # run may need a network download, but an SDK preflight failure must not close
    # Playnite or its Worker.
    Initialize-GscSdkBaseline -ProjectRoot $root | Out-Null

    $sourceManifest = Join-Path $root 'src\GameSaveCenter.Playnite\extension.yaml'
    $expectedVersion = Read-ManifestVersion $sourceManifest
    Write-Host "GameSaveCenter 一键开发安装" -ForegroundColor Cyan
    Write-Host "源码版本：$expectedVersion" -ForegroundColor Cyan

    $runningPlaynitePaths = @()
    foreach ($processName in @('Playnite.DesktopApp', 'Playnite.FullscreenApp')) {
        foreach ($process in @(Get-Process -Name $processName -ErrorAction SilentlyContinue)) {
            try {
                if ($process.Path) { $runningPlaynitePaths += $process.Path }
            }
            catch {
                # 无权限读取路径时忽略，后续继续自动发现。
            }
        }
    }

    if ($PlayniteExecutable -and -not (Test-Path -LiteralPath $PlayniteExecutable -PathType Leaf)) {
        throw "指定的 Playnite 可执行文件不存在：$PlayniteExecutable"
    }

    $playniteExecutables = @(Get-PlayniteExecutableCandidates -PreferredPath $PlayniteExecutable -RunningPaths $runningPlaynitePaths)
    if ($playniteExecutables.Count -eq 0) {
        Write-Warning '未自动发现 Playnite.DesktopApp.exe。将继续构建和安装；安装完成后无法自动启动 Playnite。若 Playnite 使用便携版或自定义目录，请使用 -PlayniteExecutable 指定其路径。'
    }
    else {
        Write-Host "发现 Playnite：$($playniteExecutables -join ', ')" -ForegroundColor DarkCyan
    }
    $extensionRoots = @(Get-ExtensionRoots -ExplicitPath $PlayniteExtensionsPath -PlayniteExecutables $playniteExecutables)

    $stopArguments = @{ ExtensionRoots = $extensionRoots }
    if ($playniteExecutables.Count -gt 0) {
        $stopArguments.TrustedPlayniteExecutables = $playniteExecutables
    }

    if (-not $SkipClean) {
        Write-Host "`n==> 使用新的隔离构建目录：无需清理正在使用的标准 bin/obj" -ForegroundColor DarkCyan
        Remove-Item (Join-Path $root 'artifacts\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec') -Recurse -Force -ErrorAction SilentlyContinue
    }

    $buildArguments = @{ Configuration = $Configuration; OutputRoot = $buildOutputRoot }
    if (-not [string]::IsNullOrWhiteSpace($TestTempRoot)) {
        $buildArguments.TestTempRoot = $TestTempRoot
    }
    if ($SkipTests) {
        $buildArguments.SkipTests = $true
    }
    & (Join-Path $PSScriptRoot 'build.ps1') @buildArguments
    $packageArguments = @{
        Configuration = $Configuration
        SkipBuild = $true
        BuildOutputRoot = $buildOutputRoot
    }
    if ($SkipPackageArchives) { $packageArguments.SkipPackageArchives = $true }
    & (Join-Path $PSScriptRoot 'package.ps1') @packageArguments

    $stage = Join-Path $root 'artifacts\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec'
    # package.ps1 recreates this directory. Keep a second, explicit check here
    # so an interrupted/legacy package cannot be installed accidentally.
    if (-not (Test-Path $stage)) {
        throw "打包目录不存在：$stage。请检查 package.ps1 的输出。"
    }
    $stageVersion = Read-ManifestVersion (Join-Path $stage 'extension.yaml')
    if ($stageVersion -ne $expectedVersion) {
        throw "打包目录版本不一致：源码 $expectedVersion，打包目录 $stageVersion。"
    }

    # Build, tests, package identity and the stage manifest must all be valid
    # before stopping the host. Failed restores/tests must leave Playnite running.
    if ($SkipPlayniteShutdownForIsolatedTarget) {
        Write-Host '隔离扩展目标：跳过停止 Playnite/Worker；若发现既有进程则已在入口处拒绝。' -ForegroundColor DarkCyan
    }
    else {
        Stop-PlayniteAndOwnedWorkerReliably @stopArguments
    }

    $results = @()
    foreach ($extensionRoot in $extensionRoots) {
        $results += Install-ExtensionAtomically -Source $stage -ExtensionsRoot $extensionRoot -ExpectedVersion $expectedVersion -DiagnosticRoot (Join-Path $root 'artifacts/install-attempts') -RunId $installRunId
    }

    New-Item (Split-Path -Parent $reportPath) -ItemType Directory -Force | Out-Null
    $report = [System.Collections.Generic.List[string]]::new()
    $report.Add("时间：$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $report.Add("安装运行 ID：$installRunId")
    $report.Add("源码版本：$expectedVersion")
    $report.Add("隔离构建目录：$buildOutputRoot")
    foreach ($result in $results) {
        $report.Add("安装目录：$($result.Target)")
        $report.Add("清单版本：$($result.ManifestVersion)")
        $report.Add("DLL 版本：$($result.FileVersion)")
        $report.Add('程序集身份（磁盘读取；不代表运行中已加载）：')
        foreach ($identity in $result.AssemblyIdentities) {
            $report.Add("  $($identity.File)：$($identity.BuildIdentity)")
            $report.Add("  文件路径：$($identity.Path)")
            $report.Add("  SHA-256：$($identity.SHA256)")
        }
    }
    $report | Set-Content $reportPath -Encoding UTF8

    Write-Host "`n安装验证成功" -ForegroundColor Green
    foreach ($result in $results) {
        Write-Host "  目录：$($result.Target)" -ForegroundColor Green
        Write-Host "  extension.yaml：$($result.ManifestVersion)" -ForegroundColor Green
        Write-Host "  DLL：$($result.FileVersion)" -ForegroundColor Green
    }
    Write-Host "  报告：$reportPath" -ForegroundColor DarkCyan

    if (-not $NoStart) {
        $playniteExe = $playniteExecutables | Select-Object -First 1
        if ($playniteExe) {
            Write-Host "`n启动 Playnite：$playniteExe" -ForegroundColor Cyan
            Start-Process -FilePath $playniteExe -WorkingDirectory (Split-Path -Parent $playniteExe)
        }
        else {
            Write-Host "`n未自动找到 Playnite.DesktopApp.exe，请手工启动 Playnite。" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "`n一键构建安装失败：$($_.Exception.Message)" -ForegroundColor Red
    Write-Host "完整日志：$runLogPath" -ForegroundColor Yellow
    throw
}
finally {
    Pop-Location
    if ($transcriptStarted) {
        try { Stop-Transcript | Out-Null } catch { }
    }
}

} finally { Exit-GscCheckoutLock $checkoutLock }
