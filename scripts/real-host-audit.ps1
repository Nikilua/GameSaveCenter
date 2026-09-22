[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Output = '',
    [string]$UserDataDir = '',
    [string]$PlayniteExecutable = '',
    [string]$TestTempRoot = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root 'artifacts\ui-host-audit'
}
$Output = [System.IO.Path]::GetFullPath($Output)
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $root 'artifacts'))
if (-not $Output.StartsWith($artifactsRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output must be inside artifacts: $Output"
}

if (Test-Path -LiteralPath $Output) {
    Remove-Item -LiteralPath $Output -Recurse -Force
}
New-Item -ItemType Directory -Path $Output -Force | Out-Null

$env:GSC_REAL_HOST_AUDIT = $Output
$previousWorkerDataDirectory = [Environment]::GetEnvironmentVariable('GameSaveCenter__DataDirectory', 'Process')
$previousWorkerPipeName = [Environment]::GetEnvironmentVariable('GameSaveCenter__PipeName', 'Process')
$previousWorkerEventPipeName = [Environment]::GetEnvironmentVariable('GameSaveCenter__EventPipeName', 'Process')
$previousAuditPipeName = [Environment]::GetEnvironmentVariable('GSC_UI_AUDIT_PIPE_NAME', 'Process')
$previousAuditEventPipeName = [Environment]::GetEnvironmentVariable('GSC_UI_AUDIT_EVENT_PIPE_NAME', 'Process')
$isolatedWorkerDataDirectory = ''
$isolatedPipeName = ''
$isolatedEventPipeName = ''
$auditStartedUtc = [DateTime]::UtcNow.ToString('O')
try {
    $commit = (& git -C $root rev-parse HEAD 2>$null | Select-Object -First 1).Trim()
    # The native exit code is not reliable after a PowerShell pipeline has consumed
    # git's output. The non-empty SHA is the evidence we need to pass to the plugin.
    if ($commit) {
        $env:GSC_UI_AUDIT_COMMIT = $commit
        Write-Host "Audit commit: $commit" -ForegroundColor Cyan
    }
}
catch {
    $env:GSC_UI_AUDIT_COMMIT = ''
}
$runnerMetadata = [ordered]@{
    Scenario = 'real-host-audit'
    EvidenceSource = 'RealPlaynite'
    Commit = if ([string]::IsNullOrWhiteSpace($env:GSC_UI_AUDIT_COMMIT)) { 'unknown' } else { $env:GSC_UI_AUDIT_COMMIT }
    Configuration = $Configuration
    StartedUtc = $auditStartedUtc
    OutputRoot = $Output
    WindowDip = 'captured per metadata-*.json; not inferred by runner'
    DpiScale = 'captured by WPF VisualTreeHelper.GetDpi'
    Theme = 'captured per metadata-*.json'
    DataVolume = 'captured from production snapshot/diagnostic metadata when available'
    Timing = 'capture manifest includes per-surface capture status; host interaction time is not fabricated'
    UserDataMode = if ([string]::IsNullOrWhiteSpace($UserDataDir)) { 'current-user-data' } else { 'isolated-user-data' }
}
Add-Type -AssemblyName System.Windows.Forms
$displayTopology = @([System.Windows.Forms.Screen]::AllScreens | ForEach-Object {
    [ordered]@{
        DeviceName = $_.DeviceName
        Primary = $_.Primary
        Bounds = [ordered]@{
            Left = $_.Bounds.Left
            Top = $_.Bounds.Top
            Width = $_.Bounds.Width
            Height = $_.Bounds.Height
        }
        WorkArea = [ordered]@{
            Left = $_.WorkingArea.Left
            Top = $_.WorkingArea.Top
            Width = $_.WorkingArea.Width
            Height = $_.WorkingArea.Height
        }
    }
})
$runnerMetadata.DisplayCount = $displayTopology.Count
$runnerMetadata.DisplayTopology = $displayTopology
$runnerMetadata.Q24_03PhysicalCrossScreen = [ordered]@{
    Status = if ($displayTopology.Count -ge 2) { 'ready-for-host-replay' } else { 'blocked-single-display' }
    Requirement = 'requires two physical displays and an open Popup while the host window crosses displays'
}
if ($displayTopology.Count -lt 2) {
    Write-Warning "Q24-03 physical cross-screen replay is blocked: only $($displayTopology.Count) display detected."
}
$installArguments = @{ Configuration = $Configuration }
if (-not [string]::IsNullOrWhiteSpace($TestTempRoot)) {
    $installArguments.TestTempRoot = [System.IO.Path]::GetFullPath($TestTempRoot)
    $runnerMetadata.TestTempRoot = $installArguments.TestTempRoot
}
if (-not [string]::IsNullOrWhiteSpace($UserDataDir)) {
    $UserDataDir = [System.IO.Path]::GetFullPath($UserDataDir)
    if (-not (Test-Path -LiteralPath $UserDataDir -PathType Container)) {
        throw "UserDataDir must already exist as an isolated Playnite data directory: $UserDataDir"
    }
    if ([string]::IsNullOrWhiteSpace($PlayniteExecutable) -or -not (Test-Path -LiteralPath $PlayniteExecutable -PathType Leaf)) {
        throw "Isolated audit requires an explicit Playnite executable: $PlayniteExecutable"
    }
    $PlayniteExecutable = [System.IO.Path]::GetFullPath($PlayniteExecutable)
    $isolatedExtensionsPath = Join-Path $UserDataDir 'Extensions'
    New-Item -ItemType Directory -Path $isolatedExtensionsPath -Force | Out-Null
    $isolatedWorkerDataDirectory = Join-Path $UserDataDir 'GameSaveCenter'
    New-Item -ItemType Directory -Path $isolatedWorkerDataDirectory -Force | Out-Null
    # The production pipe names stay fixed, but an isolated audit must not connect to
    # another user-data profile's Worker. The plugin and the Worker inherit these process
    # variables, so the audit gets a private IPC pair without touching user processes.
    $auditPipeToken = [Guid]::NewGuid().ToString('N')
    $isolatedPipeName = "GameSaveCenter.Worker.Audit.$auditPipeToken"
    $isolatedEventPipeName = "$isolatedPipeName.Events"
    [Environment]::SetEnvironmentVariable('GameSaveCenter__DataDirectory', $isolatedWorkerDataDirectory, 'Process')
    [Environment]::SetEnvironmentVariable('GameSaveCenter__PipeName', $isolatedPipeName, 'Process')
    [Environment]::SetEnvironmentVariable('GameSaveCenter__EventPipeName', $isolatedEventPipeName, 'Process')
    [Environment]::SetEnvironmentVariable('GSC_UI_AUDIT_PIPE_NAME', $isolatedPipeName, 'Process')
    [Environment]::SetEnvironmentVariable('GSC_UI_AUDIT_EVENT_PIPE_NAME', $isolatedEventPipeName, 'Process')
    $installArguments.PlayniteExtensionsPath = $isolatedExtensionsPath
    $installArguments.PlayniteExecutable = $PlayniteExecutable
    $installArguments.NoStart = $true
    $runnerMetadata.UserDataDir = $UserDataDir
    $runnerMetadata.PlayniteExecutable = $PlayniteExecutable
    $runnerMetadata.WorkerDataDirectory = $isolatedWorkerDataDirectory
    $runnerMetadata.IpcIsolation = [ordered]@{
        PipeName = $isolatedPipeName
        EventPipeName = $isolatedEventPipeName
        Scope = 'isolated-audit-process'
    }
}
$runnerMetadata | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
Write-Host "==> Starting Playnite with GSC_REAL_HOST_AUDIT=$Output" -ForegroundColor Cyan
$startedPlayniteProcess = $null

Push-Location $root
try {
    & (Join-Path $root 'scripts\dev-install-run.ps1') @installArguments
    if ($LASTEXITCODE -ne 0) { throw "dev-install-run failed: $LASTEXITCODE" }

    if (-not [string]::IsNullOrWhiteSpace($UserDataDir)) {
        $playniteConfigPath = Join-Path $UserDataDir 'config.json'
        if (-not (Test-Path -LiteralPath $playniteConfigPath -PathType Leaf)) {
            throw "Isolated Playnite config was not found: $playniteConfigPath"
        }
        $playniteConfig = Get-Content -LiteralPath $playniteConfigPath -Encoding UTF8 -Raw | ConvertFrom-Json
        $playniteConfig.DatabasePath = Join-Path $UserDataDir 'library'
        $playniteConfig.AutoBackupEnabled = $false
        $playniteConfig | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $playniteConfigPath -Encoding UTF8

        # A copied config can reference a user desktop theme that is not present in the
        # isolated profile.  Playnite then starts with a black/empty client window and the
        # host audit loses the very surface it is meant to inspect.  Copy only the configured
        # theme into the isolated profile; never alter the user's theme or global config.
        $configuredTheme = [string]$playniteConfig.Theme
        if (-not [string]::IsNullOrWhiteSpace($configuredTheme)) {
            $sourceTheme = Join-Path (Join-Path $env:APPDATA 'Playnite\Themes\Desktop') $configuredTheme
            $isolatedTheme = Join-Path (Join-Path $UserDataDir 'Themes\Desktop') $configuredTheme
            if (Test-Path -LiteralPath $sourceTheme -PathType Container) {
                New-Item -ItemType Directory -Path (Split-Path -Parent $isolatedTheme) -Force | Out-Null
                Copy-Item -LiteralPath $sourceTheme -Destination $isolatedTheme -Recurse -Force
                $runnerMetadata.ConfiguredDesktopTheme = $configuredTheme
                $runnerMetadata.ConfiguredDesktopThemeCopied = $true
            }
            else {
                $runnerMetadata.ConfiguredDesktopTheme = $configuredTheme
                $runnerMetadata.ConfiguredDesktopThemeCopied = $false
                Write-Warning "Configured Playnite desktop theme was not found in the current user profile: $configuredTheme"
            }
        }

        $pluginId = '66e9f2d7-67bb-43ef-b62a-b8e60734fcec'
        $pluginWorker = Join-Path $isolatedExtensionsPath "GameSaveCenter_$pluginId\Worker\GameSaveCenter.Worker.exe"
        $pluginSettingsPath = Join-Path $UserDataDir "ExtensionsData\$pluginId\config.json"
        if (-not (Test-Path -LiteralPath $pluginWorker -PathType Leaf)) {
            throw "Isolated GameSaveCenter Worker was not found: $pluginWorker"
        }
        if (Test-Path -LiteralPath $pluginSettingsPath -PathType Leaf) {
            $pluginSettings = Get-Content -LiteralPath $pluginSettingsPath -Encoding UTF8 -Raw | ConvertFrom-Json
            $pluginSettings.WorkerExecutable = $pluginWorker
            $pluginSettings | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $pluginSettingsPath -Encoding UTF8
        }

        $runnerMetadata.DatabasePath = Join-Path $UserDataDir 'library'
        $runnerMetadata.WorkerExecutable = $pluginWorker
        $runnerMetadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
        Write-Host "==> Starting Playnite with isolated user data: $UserDataDir" -ForegroundColor Cyan
        $startedPlayniteProcess = Start-Process -FilePath $PlayniteExecutable `
            -WorkingDirectory (Split-Path -Parent $PlayniteExecutable) `
            -ArgumentList @('--startdesktop', '--hidesplashscreen', '--userdatadir', $UserDataDir) `
            -PassThru
    }
}
finally {
    Pop-Location
    if ($null -eq $previousWorkerDataDirectory) {
        Remove-Item Env:GameSaveCenter__DataDirectory -ErrorAction SilentlyContinue
    }
    else {
        $env:GameSaveCenter__DataDirectory = $previousWorkerDataDirectory
    }
    foreach ($entry in @(
        @{ Name = 'GameSaveCenter__PipeName'; Value = $previousWorkerPipeName },
        @{ Name = 'GameSaveCenter__EventPipeName'; Value = $previousWorkerEventPipeName },
        @{ Name = 'GSC_UI_AUDIT_PIPE_NAME'; Value = $previousAuditPipeName },
        @{ Name = 'GSC_UI_AUDIT_EVENT_PIPE_NAME'; Value = $previousAuditEventPipeName }
    )) {
        if ($null -eq $entry.Value) {
            Remove-Item ("Env:" + $entry.Name) -ErrorAction SilentlyContinue
        }
        else {
            Set-Item ("Env:" + $entry.Name) $entry.Value
        }
    }
}

function Get-RealHostStartupBlocker {
    param(
        [string]$HostUserDataDir,
        [System.Diagnostics.Process]$StartedProcess
    )

    if ($null -eq $StartedProcess) {
        return $null
    }

    try {
        $StartedProcess.Refresh()
        if (-not $StartedProcess.HasExited) {
            return $null
        }
        $processState = 'exited-before-main-window'
    }
    catch {
        $processState = 'process-state-unavailable'
    }

    if ([string]::IsNullOrWhiteSpace($HostUserDataDir)) {
        $HostUserDataDir = Join-Path $env:APPDATA 'Playnite'
    }

    $playniteLogPath = Join-Path $HostUserDataDir 'playnite.log'
    $cefLogPath = Join-Path $HostUserDataDir 'cef.log'
    $playniteTail = @()
    $cefTail = @()
    if (Test-Path -LiteralPath $playniteLogPath -PathType Leaf) {
        $playniteTail = @(Get-Content -LiteralPath $playniteLogPath -Tail 200 -ErrorAction SilentlyContinue)
    }
    if (Test-Path -LiteralPath $cefLogPath -PathType Leaf) {
        $cefTail = @(Get-Content -LiteralPath $cefLogPath -Tail 200 -ErrorAction SilentlyContinue)
    }

    $cefMatches = @($cefTail | Where-Object { $_ -match 'platform_channel|FATAL|Access denied|拒绝访问|0x5' })
    $playniteMatches = @($playniteTail | Where-Object { $_ -match 'Application started|MainWindow|WindowFactory' })
    if ($cefMatches.Count -eq 0 -and $playniteMatches.Count -eq 0) {
        return $null
    }

    $classification = 'host-exited-before-main-window'
    if ($cefMatches | Where-Object { $_ -match 'platform_channel|Access denied|拒绝访问|0x5' }) {
        $classification = 'cef-startup-access-denied-before-main-window'
    }

    return [ordered]@{
        Scenario = 'real-host-startup'
        EvidenceSource = 'RealPlaynite'
        Classification = $classification
        ProcessState = $processState
        ProcessId = $StartedProcess.Id
        HostUserDataDir = $HostUserDataDir
        PlayniteLog = $playniteLogPath
        CefLog = $cefLogPath
        PlayniteLogMatches = @($playniteMatches | Select-Object -Last 20)
        CefLogMatches = @($cefMatches | Select-Object -Last 20)
        ObservedUtc = (Get-Date).ToUniversalTime().ToString('o')
        VisualEvidenceCaptured = $false
        CountsAsVisualPass = $false
    }
}

function Write-RealHostStartupBlocker {
    param([object]$Blocker)

    $path = Join-Path $Output 'host-startup-blocker.json'
    $Blocker | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding UTF8
    Write-Warning "Detected real-host startup blocker: $($Blocker.Classification). Evidence: $path"
}

function Invoke-GameSaveCenterSidebar {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms
    $deadline = (Get-Date).AddSeconds(60)
    while ((Get-Date) -lt $deadline) {
        $process = Get-Process -Name 'Playnite.DesktopApp' -ErrorAction SilentlyContinue |
            ForEach-Object {
                try { $_.Refresh() } catch { }
                $_
            } |
            Where-Object { $_.MainWindowHandle -ne 0 } |
            Select-Object -First 1
        if ($process) {
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
            $condition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty,
                'GameSaveCenter')
            $item = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
            if ($item) {
                $invoke = $null
                try { $invoke = $item.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern) } catch { }
                if ($invoke) {
                    $invoke.Invoke()
                    Write-Host 'Clicked GameSaveCenter sidebar item' -ForegroundColor Green
                    return
                }
                $select = $null
                try { $select = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern) } catch { }
                if ($select) {
                    $select.Select()
                    Write-Host 'Selected GameSaveCenter sidebar item' -ForegroundColor Green
                    return
                }
                $item.SetFocus()
                [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
                Write-Host 'Focused GameSaveCenter sidebar item' -ForegroundColor Green
                return
            }
        }
        Start-Sleep -Seconds 2
    }
    Write-Warning 'Could not locate GameSaveCenter sidebar item via UI Automation.'
}

Invoke-GameSaveCenterSidebar

$summary = Join-Path $Output 'summary.json'
$startupBlocker = Get-RealHostStartupBlocker -HostUserDataDir $UserDataDir -StartedProcess $startedPlayniteProcess
if ($startupBlocker) {
    Write-RealHostStartupBlocker -Blocker $startupBlocker
}
if ($null -eq $startupBlocker) {
    Write-Host "[WAITING] 请在 Playnite 左侧点击 GameSaveCenter。将在检测到真实 DashboardView.Loaded 后继续；超时：90 秒。" -ForegroundColor Yellow
    $deadline = (Get-Date).AddSeconds(90)
    while (-not (Test-Path -LiteralPath $summary)) {
        if ((Get-Date) -gt $deadline) {
            Write-Warning "Timed out waiting for $summary. Check Playnite extension logs."
            break
        }
        Start-Sleep -Seconds 2
        $startupBlocker = Get-RealHostStartupBlocker -HostUserDataDir $UserDataDir -StartedProcess $startedPlayniteProcess
        if ($startupBlocker) {
            Write-RealHostStartupBlocker -Blocker $startupBlocker
        }
    }
}

$zip = Join-Path $artifactsRoot 'GameSaveCenter-ui-host-audit.zip'
if (-not (Test-Path -LiteralPath $summary)) {
    Write-Host "[PARTIAL] 未捕获真实 Embedded Dashboard。已生成 Controlled Host evidence，但它不是实际插件视觉真值。" -ForegroundColor Yellow
    Write-Host "Output: $Output"
    exit 2
}

$summaryJson = Get-Content -LiteralPath $summary -Raw | ConvertFrom-Json
if ($summaryJson.EmbeddedDashboardCaptured) {
    Write-Host "[OK] Embedded Playnite Dashboard captured." -ForegroundColor Green
    Write-Host "Real host audit output: $Output" -ForegroundColor Green
    Write-Host "ZIP: $zip"
    exit 0
}

Write-Host "[PARTIAL] 未捕获真实 Embedded Dashboard（EmbeddedDashboardCaptured=false）。Controlled Host evidence 已生成，但不是真实插件视觉真值。" -ForegroundColor Yellow
Write-Host "Output: $Output"
Write-Host "ZIP: $zip"
exit 2
