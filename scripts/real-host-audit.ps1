[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Output = '',
    [string]$UserDataDir = '',
    [string]$PlayniteExecutable = ''
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
$isolatedWorkerDataDirectory = ''
$auditStartedUtc = [DateTime]::UtcNow.ToString('O')
try {
    $commit = (& git -C $root rev-parse HEAD 2>$null | Select-Object -First 1).Trim()
    if ($LASTEXITCODE -eq 0 -and $commit) {
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
$installArguments = @{ Configuration = $Configuration }
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
    [Environment]::SetEnvironmentVariable('GameSaveCenter__DataDirectory', $isolatedWorkerDataDirectory, 'Process')
    $installArguments.PlayniteExtensionsPath = $isolatedExtensionsPath
    $installArguments.PlayniteExecutable = $PlayniteExecutable
    $installArguments.NoStart = $true
    $runnerMetadata.UserDataDir = $UserDataDir
    $runnerMetadata.PlayniteExecutable = $PlayniteExecutable
    $runnerMetadata.WorkerDataDirectory = $isolatedWorkerDataDirectory
}
$runnerMetadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
Write-Host "==> Starting Playnite with GSC_REAL_HOST_AUDIT=$Output" -ForegroundColor Cyan

Push-Location $root
try {
    & (Join-Path $root 'scripts\dev-install-run.ps1') @installArguments
    if ($LASTEXITCODE -ne 0) { throw "dev-install-run failed: $LASTEXITCODE" }

    if (-not [string]::IsNullOrWhiteSpace($UserDataDir)) {
        $playniteConfigPath = Join-Path $UserDataDir 'config.json'
        if (-not (Test-Path -LiteralPath $playniteConfigPath -PathType Leaf)) {
            throw "Isolated Playnite config was not found: $playniteConfigPath"
        }
        $playniteConfig = Get-Content -LiteralPath $playniteConfigPath -Raw | ConvertFrom-Json
        $playniteConfig.DatabasePath = Join-Path $UserDataDir 'library'
        $playniteConfig.AutoBackupEnabled = $false
        $playniteConfig | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $playniteConfigPath -Encoding UTF8

        $pluginId = '66e9f2d7-67bb-43ef-b62a-b8e60734fcec'
        $pluginWorker = Join-Path $isolatedExtensionsPath "GameSaveCenter_$pluginId\Worker\GameSaveCenter.Worker.exe"
        $pluginSettingsPath = Join-Path $UserDataDir "ExtensionsData\$pluginId\config.json"
        if (-not (Test-Path -LiteralPath $pluginWorker -PathType Leaf)) {
            throw "Isolated GameSaveCenter Worker was not found: $pluginWorker"
        }
        if (Test-Path -LiteralPath $pluginSettingsPath -PathType Leaf) {
            $pluginSettings = Get-Content -LiteralPath $pluginSettingsPath -Raw | ConvertFrom-Json
            $pluginSettings.WorkerExecutable = $pluginWorker
            $pluginSettings | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $pluginSettingsPath -Encoding UTF8
        }

        $runnerMetadata.DatabasePath = Join-Path $UserDataDir 'library'
        $runnerMetadata.WorkerExecutable = $pluginWorker
        $runnerMetadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
        Write-Host "==> Starting Playnite with isolated user data: $UserDataDir" -ForegroundColor Cyan
        Start-Process -FilePath $PlayniteExecutable `
            -WorkingDirectory (Split-Path -Parent $PlayniteExecutable) `
            -ArgumentList @('--startdesktop', '--hidesplashscreen', '--userdatadir', $UserDataDir)
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
}

function Invoke-GameSaveCenterSidebar {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms
    $deadline = (Get-Date).AddSeconds(60)
    while ((Get-Date) -lt $deadline) {
        $process = Get-Process -Name 'Playnite.DesktopApp' -ErrorAction SilentlyContinue |
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
Write-Host "[WAITING] 请在 Playnite 左侧点击 GameSaveCenter。将在检测到真实 DashboardView.Loaded 后继续；超时：90 秒。" -ForegroundColor Yellow
$deadline = (Get-Date).AddSeconds(90)
while (-not (Test-Path -LiteralPath $summary)) {
    if ((Get-Date) -gt $deadline) {
        Write-Warning "Timed out waiting for $summary. Check Playnite extension logs."
        break
    }
    Start-Sleep -Seconds 2
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
