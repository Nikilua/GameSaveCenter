[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$PlayniteExtensionsPath = '',
    [string]$PlayniteExecutable = '',
    [switch]$NoStart,
    [switch]$SkipClean
)

$ErrorActionPreference = 'Stop'

function Test-IsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdministrator)) {
    Write-Host '一键安装需要管理员权限，正在请求 UAC...' -ForegroundColor Yellow
    $hostPath = Join-Path $PSHOME ($(if ($PSVersionTable.PSEdition -eq 'Core') { 'pwsh.exe' } else { 'powershell.exe' }))
    $forwardedArguments = @(
        '-NoLogo',
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', $PSCommandPath,
        '-Configuration', $Configuration
    )
    if ($PlayniteExtensionsPath) { $forwardedArguments += @('-PlayniteExtensionsPath', $PlayniteExtensionsPath) }
    if ($PlayniteExecutable) { $forwardedArguments += @('-PlayniteExecutable', $PlayniteExecutable) }
    if ($NoStart) { $forwardedArguments += '-NoStart' }
    if ($SkipClean) { $forwardedArguments += '-SkipClean' }
    try {
        $elevatedRun = Start-Process -FilePath $hostPath -Verb RunAs -Wait -PassThru -ArgumentList $forwardedArguments -ErrorAction Stop
        exit $elevatedRun.ExitCode
    }
    catch {
        throw "无法以管理员身份启动一键安装：$($_.Exception.Message)。请右键选择以管理员身份运行，或在 UAC 对话框中允许本次操作。"
    }
}

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
$artifactsPath = Join-Path $root 'artifacts'
New-Item $artifactsPath -ItemType Directory -Force | Out-Null
$runLogPath = Join-Path $artifactsPath 'one-click-install.log'
$transcriptStarted = $false
try {
    Start-Transcript -Path $runLogPath -Force | Out-Null
    $transcriptStarted = $true
}
catch {
    Write-Warning "无法启动安装日志记录：$($_.Exception.Message)"
}

$extensionId = 'GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec'
$reportPath = Join-Path $root 'artifacts\last-dev-install.txt'

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
        Where-Object { $_ -and (Test-Path $_) } |
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

function Invoke-ElevatedProcessStop {
    param(
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][string]$ExpectedName
    )

    # The normal entry point is already elevated. Keep this fallback narrow: elevate
    # only the exact PID that the current session could not terminate, and re-check
    # its name after UAC approval to avoid killing a process that reused the PID.
    $escapedName = $ExpectedName.Replace("'", "''")
    $stopCommand = @"
`$ErrorActionPreference = 'Stop'
`$target = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
if (`$null -eq `$target) { exit 0 }
if (-not [string]::Equals(`$target.ProcessName, '$escapedName', [System.StringComparison]::OrdinalIgnoreCase)) { exit 3 }
try {
    Stop-Process -Id $ProcessId -Force -ErrorAction Stop
    exit 0
}
catch {
    [Console]::Error.WriteLine(`$_.Exception.Message)
    exit 1
}
"@

    $encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($stopCommand))
    $powershellPath = (Get-Command powershell.exe -CommandType Application -ErrorAction Stop).Source
    try {
        $elevated = Start-Process -FilePath $powershellPath -Verb RunAs -Wait -PassThru -ArgumentList @(
            '-NoLogo',
            '-NoProfile',
            '-ExecutionPolicy', 'Bypass',
            '-EncodedCommand', $encodedCommand
        ) -ErrorAction Stop
    }
    catch {
        Write-Warning "管理员权限请求未完成：$($_.Exception.Message)"
        return $false
    }

    if ($elevated.ExitCode -eq 0) { return $true }
    if ($elevated.ExitCode -eq 3) {
        throw "停止进程失败：PID $ProcessId 在管理员确认期间已变成其他进程，已中止以避免误杀。"
    }

    return $false
}

function Stop-ProcessReliably {
    param([string[]]$Names)

    $elevationAttempted = @{}
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $remaining = @($Names | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
        if ($remaining.Count -eq 0) { return }

        foreach ($process in $remaining) {
            $processId = [int]$process.Id
            $processName = [string]$process.ProcessName
            if ($elevationAttempted.ContainsKey($processId)) { continue }
            Write-Host "停止进程：$processName [$processId]" -ForegroundColor DarkYellow
            try {
                # A process can exit between discovery and termination. The next
                # discovery pass treats that normal race as a successful stop.
                $currentProcess = Get-Process -Id $processId -ErrorAction SilentlyContinue
                if ($null -eq $currentProcess) { continue }
                if (-not [string]::Equals($currentProcess.ProcessName, $processName, [System.StringComparison]::OrdinalIgnoreCase)) {
                    throw "停止进程失败：PID $processId 已被 $($currentProcess.ProcessName) 复用，已中止以避免误杀。"
                }
                Stop-Process -Id $processId -Force -ErrorAction Stop
            }
            catch {
                if (-not $elevationAttempted.ContainsKey($processId)) {
                    $elevationAttempted[$processId] = $true
                    Write-Host "当前会话无权停止 $processName [$processId]，请求管理员权限..." -ForegroundColor Yellow
                    if (-not (Invoke-ElevatedProcessStop -ProcessId $processId -ExpectedName $processName)) {
                        Write-Warning "管理员权限也未能停止 $processName [$processId]。"
                    }
                }
            }
        }

        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)

    $remaining = @($Names | ForEach-Object { Get-Process -Name $_ -ErrorAction SilentlyContinue })
    if ($remaining.Count -eq 0) { return }
    $remainingNames = @($remaining | ForEach-Object { $_.ProcessName } | Select-Object -Unique)
    throw "无法停止以下进程：$($remainingNames -join ', ')。已尝试当前会话和管理员权限，但进程仍在运行；为避免覆盖正在使用的扩展，安装已停止。"
}

function Install-ExtensionAtomically {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$ExtensionsRoot,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion
    )

    New-Item $ExtensionsRoot -ItemType Directory -Force | Out-Null
    $target = Join-Path $ExtensionsRoot $extensionId
    $temporary = "$target.__new"

    Remove-Item $temporary -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item $Source $temporary -Recurse -Force

    $temporaryManifest = Join-Path $temporary 'extension.yaml'
    $temporaryVersion = Read-ManifestVersion $temporaryManifest
    if ($temporaryVersion -ne $ExpectedVersion) {
        throw "暂存扩展版本错误：期望 $ExpectedVersion，实际 $temporaryVersion。"
    }

    if (Test-Path $target) {
        Write-Host "移除旧扩展：$target" -ForegroundColor DarkYellow
        Remove-Item $target -Recurse -Force -ErrorAction Stop
        if (Test-Path $target) {
            throw "旧扩展目录未能删除：$target"
        }
    }

    Move-Item $temporary $target

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

    return [pscustomobject]@{
        Target = $target
        ManifestVersion = $installedVersion
        FileVersion = $fileVersion
    }
}

Push-Location $root
try {
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

    $playniteExecutables = @(Get-PlayniteExecutableCandidates -PreferredPath $PlayniteExecutable -RunningPaths $runningPlaynitePaths)
    $extensionRoots = @(Get-ExtensionRoots -ExplicitPath $PlayniteExtensionsPath -PlayniteExecutables $playniteExecutables)

    Stop-ProcessReliably -Names @('Playnite.DesktopApp', 'Playnite.FullscreenApp', 'GameSaveCenter.Worker')

    # A source archive can contain obj/project.assets.json created on another machine.
    # `dotnet clean` resolves package assets before deleting them, so clean must never
    # run before restore has rewritten those machine-specific package paths.
    Write-Host "`n==> 预先恢复 NuGet 依赖" -ForegroundColor Cyan
    & dotnet restore '.\GameSaveCenter.sln' '--force-evaluate'
    if ($LASTEXITCODE -ne 0) {
        throw "NuGet 依赖恢复失败，dotnet 退出码：$LASTEXITCODE。请检查网络、NuGet 源和磁盘空间后重试。"
    }

    if (-not $SkipClean) {
        Write-Host "`n==> 清理旧构建和旧打包产物" -ForegroundColor Cyan
        & dotnet clean '.\GameSaveCenter.sln' -c $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "清理解决方案失败，dotnet 退出码：$LASTEXITCODE"
        }
        Remove-Item (Join-Path $root 'artifacts\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec') -Recurse -Force -ErrorAction SilentlyContinue
    }

    & (Join-Path $PSScriptRoot 'build.ps1') -Configuration $Configuration
    & (Join-Path $PSScriptRoot 'package.ps1') -Configuration $Configuration -SkipBuild

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

    $results = @()
    foreach ($extensionRoot in $extensionRoots) {
        $results += Install-ExtensionAtomically -Source $stage -ExtensionsRoot $extensionRoot -ExpectedVersion $expectedVersion
    }

    New-Item (Split-Path -Parent $reportPath) -ItemType Directory -Force | Out-Null
    $report = [System.Collections.Generic.List[string]]::new()
    $report.Add("时间：$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $report.Add("源码版本：$expectedVersion")
    foreach ($result in $results) {
        $report.Add("安装目录：$($result.Target)")
        $report.Add("清单版本：$($result.ManifestVersion)")
        $report.Add("DLL 版本：$($result.FileVersion)")
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
