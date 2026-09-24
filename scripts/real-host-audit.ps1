[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Output = '',
    [string]$UserDataDir = '',
    [string]$PlayniteExecutable = '',
    [string]$TestTempRoot = '',
    [switch]$SeedSyntheticLibrary,
    [ValidateRange(1, 512)][int]$SyntheticLibraryCount = 64,
    [switch]$SkipInstallTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if ($SeedSyntheticLibrary) {
    if ([string]::IsNullOrWhiteSpace($UserDataDir)) {
        throw 'SeedSyntheticLibrary requires an explicit isolated UserDataDir under repository .tmp.'
    }
    $UserDataDir = [System.IO.Path]::GetFullPath($UserDataDir)
    $temporaryRoot = [System.IO.Path]::GetFullPath((Join-Path $root '.tmp'))
    $temporaryPrefix = $temporaryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $UserDataDir.StartsWith($temporaryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Synthetic library seeding is restricted to an isolated profile below repository .tmp: $UserDataDir"
    }
    $cursor = $UserDataDir
    while ($cursor.StartsWith($temporaryPrefix, [System.StringComparison]::OrdinalIgnoreCase) -or
           [string]::Equals($cursor, $temporaryRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        if (Test-Path -LiteralPath $cursor) {
            $entry = Get-Item -LiteralPath $cursor -Force
            if (($entry.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Synthetic library profile cannot traverse a reparse point: $cursor"
            }
        }
        if ([string]::Equals($cursor, $temporaryRoot, [System.StringComparison]::OrdinalIgnoreCase)) { break }
        $cursor = Split-Path -Parent $cursor
    }
}
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
$previousSyntheticSeedDirectory = [Environment]::GetEnvironmentVariable('GSC_UI_AUDIT_SEED_DIRECTORY', 'Process')
$previousSyntheticSeedRunId = [Environment]::GetEnvironmentVariable('GSC_UI_AUDIT_SEED_RUN_ID', 'Process')
$previousSyntheticSeedCount = [Environment]::GetEnvironmentVariable('GSC_UI_AUDIT_SEED_COUNT', 'Process')
$isolatedWorkerDataDirectory = ''
$isolatedPipeName = ''
$isolatedEventPipeName = ''
$syntheticSeedDirectory = ''
$syntheticSeedManifestPath = ''
$syntheticSeedRunId = ''
$syntheticBuildOutputRoot = ''
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
    SyntheticLibrary = if ($SeedSyntheticLibrary) {
        [ordered]@{ Status = 'requested'; RequestedCount = $SyntheticLibraryCount; EvidenceSource = 'SyntheticPlayniteLibrary' }
    } else { [ordered]@{ Status = 'not-requested'; EvidenceSource = 'none' } }
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
if ($SkipInstallTests) {
    $installArguments.SkipTests = $true
    $runnerMetadata.InstallTests = 'skipped-by-explicit-audit-switch'
}
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
    # Isolated audits must keep package archives and their historical hashes intact.
    $installArguments.SkipPackageArchives = $true
    $installArguments.RunLogPath = Join-Path $Output 'dev-install.log'
    $installArguments.InstallReportPath = Join-Path $Output 'dev-install-report.txt'
    $runnerMetadata.UserDataDir = $UserDataDir
    $runnerMetadata.PlayniteExecutable = $PlayniteExecutable
    $runnerMetadata.WorkerDataDirectory = $isolatedWorkerDataDirectory
    $runnerMetadata.IpcIsolation = [ordered]@{
        PipeName = $isolatedPipeName
        EventPipeName = $isolatedEventPipeName
        Scope = 'isolated-audit-process'
    }
    if ($SeedSyntheticLibrary) {
        $syntheticSeedDirectory = Join-Path $UserDataDir 'audit-fixtures'
        $syntheticSeedManifestPath = Join-Path $syntheticSeedDirectory 'synthetic-library-manifest.json'
        $syntheticBuildOutputRoot = Join-Path $syntheticSeedDirectory 'build'
        $syntheticSeedRunId = [Guid]::NewGuid().ToString('D')
        $installArguments.BuildOutputRoot = $syntheticBuildOutputRoot
        $runnerMetadata.SyntheticLibrary = [ordered]@{
            Status = 'seeder-build-and-install-pending'
            RequestedCount = $SyntheticLibraryCount
            EvidenceSource = 'SyntheticPlayniteLibrary'
            RunId = $syntheticSeedRunId
            ManifestPath = $syntheticSeedManifestPath
            BuildOutputRoot = $syntheticBuildOutputRoot
        }
    }
}

function Find-PlayniteDesktopThemeDirectory {
    param([Parameter(Mandatory = $true)][string]$ThemeId)

    $themeRoots = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($env:APPDATA)) {
        $themeRoots.Add((Join-Path $env:APPDATA 'Playnite\Themes\Desktop'))
    }
    if (-not [string]::IsNullOrWhiteSpace($PlayniteExecutable)) {
        $playniteInstallThemeRoot = Join-Path (Split-Path -Parent $PlayniteExecutable) 'Themes\Desktop'
        if (-not $themeRoots.Contains($playniteInstallThemeRoot)) {
            $themeRoots.Add($playniteInstallThemeRoot)
        }
    }

    foreach ($themeRoot in $themeRoots) {
        if (-not (Test-Path -LiteralPath $themeRoot -PathType Container)) { continue }
        foreach ($themeDirectory in Get-ChildItem -LiteralPath $themeRoot -Directory -ErrorAction SilentlyContinue) {
            $manifestPath = Join-Path $themeDirectory.FullName 'theme.yaml'
            if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { continue }
            try {
                $idLine = Get-Content -LiteralPath $manifestPath -ErrorAction Stop |
                    Where-Object { $_ -match '^\s*Id\s*:\s*(.+?)\s*$' } |
                    Select-Object -First 1
                if ($idLine -and $idLine -match '^\s*Id\s*:\s*(.+?)\s*$') {
                    $candidateId = $Matches[1].Trim().Trim('"').Trim("'")
                    if ([string]::Equals($candidateId, $ThemeId, [System.StringComparison]::OrdinalIgnoreCase)) {
                        return $themeDirectory.FullName
                    }
                }
            }
            catch {
                # A broken user theme should not prevent trying other installed themes.
            }
        }
    }

    return $null
}

function Initialize-IsolatedPlayniteConfig {
    if ([string]::IsNullOrWhiteSpace($UserDataDir)) {
        return
    }

    $configPath = Join-Path $UserDataDir 'config.json'
    if (Test-Path -LiteralPath $configPath -PathType Leaf) {
        $runnerMetadata.IsolatedProfileConfig = 'pre-existing'
        return
    }

    $bootstrapProcess = $null
    $backupConfigPath = Join-Path $UserDataDir 'Backup\config.json'
    try {
        Write-Host '==> Initializing isolated Playnite profile configuration' -ForegroundColor DarkCyan
        $bootstrapProcess = Start-Process -FilePath $PlayniteExecutable `
            -WorkingDirectory (Split-Path -Parent $PlayniteExecutable) `
            -ArgumentList @('--startdesktop', '--hidesplashscreen', '--userdatadir', $UserDataDir) `
            -PassThru
        $deadline = (Get-Date).AddSeconds(45)
        while (-not (Test-Path -LiteralPath $configPath -PathType Leaf) -and
               -not (Test-Path -LiteralPath $backupConfigPath -PathType Leaf) -and
               (Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 2
            try { $bootstrapProcess.Refresh() } catch { }
            if ($bootstrapProcess.HasExited -and (Get-Date) -lt $deadline) {
                Start-Sleep -Seconds 1
            }
        }

        if (Test-Path -LiteralPath $configPath -PathType Leaf) {
            $runnerMetadata.IsolatedProfileConfig = 'created-by-bootstrap'
            return
        }

        if (Test-Path -LiteralPath $backupConfigPath -PathType Leaf) {
            Copy-Item -LiteralPath $backupConfigPath -Destination $configPath -Force
            $runnerMetadata.IsolatedProfileConfig = 'restored-from-isolated-backup'
            return
        }

        throw "Playnite did not create an isolated config or backup within 45 seconds: $UserDataDir"
    }
    finally {
        if ($null -ne $bootstrapProcess) {
            try { $bootstrapProcess.Refresh() } catch { }
            if (-not $bootstrapProcess.HasExited) {
                $windowDeadline = [DateTime]::UtcNow.AddSeconds(20)
                while (-not $bootstrapProcess.HasExited -and
                       $bootstrapProcess.MainWindowHandle -eq [IntPtr]::Zero -and
                       [DateTime]::UtcNow -lt $windowDeadline) {
                    Start-Sleep -Milliseconds 250
                    try { $bootstrapProcess.Refresh() } catch { }
                }

                if (-not $bootstrapProcess.HasExited) {
                    $closeRequested = $false
                    try { $closeRequested = $bootstrapProcess.CloseMainWindow() } catch { }
                    if (-not $closeRequested) {
                        throw "Playnite isolated profile bootstrap did not expose a closable main window: $UserDataDir"
                    }

                    if (-not $bootstrapProcess.WaitForExit(20000)) {
                        throw "Playnite isolated profile bootstrap did not exit gracefully; refusing to force-stop it and leave a safe-start marker: $UserDataDir"
                    }
                }
            }

            try { $bootstrapProcess.Refresh() } catch { }
            $safeStartFlag = Join-Path $UserDataDir 'safestart.flag'
            if (-not $bootstrapProcess.HasExited -or (Test-Path -LiteralPath $safeStartFlag -PathType Leaf)) {
                throw "Playnite isolated profile bootstrap did not close cleanly: $UserDataDir"
            }
            $runnerMetadata.IsolatedProfileBootstrapShutdown = 'closed-gracefully'
        }
    }
}

Initialize-IsolatedPlayniteConfig
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

        # A copied config can reference a desktop theme that is not present in the
        # isolated profile.  Resolve it by manifest ID from user or installed Playnite
        # theme roots, then copy only into this isolated profile.
        $configuredTheme = [string]$playniteConfig.Theme
        if (-not [string]::IsNullOrWhiteSpace($configuredTheme)) {
            $sourceTheme = Find-PlayniteDesktopThemeDirectory -ThemeId $configuredTheme
            $isolatedTheme = Join-Path (Join-Path $UserDataDir 'Themes\Desktop') $configuredTheme
            if (-not [string]::IsNullOrWhiteSpace($sourceTheme) -and
                (Test-Path -LiteralPath $sourceTheme -PathType Container)) {
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

        if ($SeedSyntheticLibrary) {
            $seederId = '9466c3cb-4c5d-4909-8334-21c608eeb309'
            $seederDirectory = Join-Path $isolatedExtensionsPath "GameSaveCenter_AuditSeeder_$seederId"
            $seederAssembly = Join-Path (Join-Path (Join-Path (Join-Path $syntheticBuildOutputRoot 'bin') 'GameSaveCenter.Playnite.HostAuditSeeder') $Configuration) 'net462\GameSaveCenter.Playnite.HostAuditSeeder.dll'
            $seederManifest = Join-Path (Split-Path -Parent $seederAssembly) 'extension.yaml'
            if (-not (Test-Path -LiteralPath $seederAssembly -PathType Leaf) -or
                -not (Test-Path -LiteralPath $seederManifest -PathType Leaf)) {
                throw "The synthetic Playnite seeder was not built: $seederAssembly"
            }
            if (Test-Path -LiteralPath $seederDirectory -PathType Container) {
                $existingManifest = Join-Path $seederDirectory 'extension.yaml'
                if (-not (Test-Path -LiteralPath $existingManifest -PathType Leaf) -or
                    -not (Select-String -LiteralPath $existingManifest -Pattern "^Id:\s*$seederId\s*$" -Quiet)) {
                    throw "Refusing to overwrite an unrecognized isolated extension directory: $seederDirectory"
                }
            }
            else {
                New-Item -ItemType Directory -Path $seederDirectory -Force | Out-Null
            }
            Copy-Item -LiteralPath $seederAssembly -Destination (Join-Path $seederDirectory 'GameSaveCenter.Playnite.HostAuditSeeder.dll') -Force
            Copy-Item -LiteralPath $seederManifest -Destination (Join-Path $seederDirectory 'extension.yaml') -Force
            $runnerMetadata.SyntheticLibrary.SeederAssemblyIdentity = (Get-Item -LiteralPath $seederAssembly).VersionInfo.ProductVersion
            $runnerMetadata.SyntheticLibrary.ExtensionDirectory = $seederDirectory
        }

        $runnerMetadata.DatabasePath = Join-Path $UserDataDir 'library'
        $runnerMetadata.WorkerExecutable = $pluginWorker
        $runnerMetadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
        Write-Host "==> Starting Playnite with isolated user data: $UserDataDir" -ForegroundColor Cyan
        if ($SeedSyntheticLibrary) {
            [Environment]::SetEnvironmentVariable('GSC_UI_AUDIT_SEED_DIRECTORY', $syntheticSeedDirectory, 'Process')
            [Environment]::SetEnvironmentVariable('GSC_UI_AUDIT_SEED_RUN_ID', $syntheticSeedRunId, 'Process')
            [Environment]::SetEnvironmentVariable('GSC_UI_AUDIT_SEED_COUNT', $SyntheticLibraryCount.ToString([System.Globalization.CultureInfo]::InvariantCulture), 'Process')
        }
        $startedPlayniteProcess = Start-Process -FilePath $PlayniteExecutable `
            -WorkingDirectory (Split-Path -Parent $PlayniteExecutable) `
            -ArgumentList @('--startdesktop', '--hidesplashscreen', '--userdatadir', $UserDataDir) `
            -PassThru
        if ($SeedSyntheticLibrary) {
            $seedDeadline = (Get-Date).AddSeconds(30)
            $seedManifest = $null
            while ((Get-Date) -lt $seedDeadline) {
                if (Test-Path -LiteralPath $syntheticSeedManifestPath -PathType Leaf) {
                    try {
                        $candidateManifest = Get-Content -LiteralPath $syntheticSeedManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
                        if ([string]::Equals([string]$candidateManifest.runId, $syntheticSeedRunId, [System.StringComparison]::OrdinalIgnoreCase)) {
                            $seedManifest = $candidateManifest
                            break
                        }
                    }
                    catch { }
                }
                try { $startedPlayniteProcess.Refresh() } catch { }
                if ($startedPlayniteProcess.HasExited) { break }
                Start-Sleep -Seconds 1
            }
            $runnerMetadata.SyntheticLibrary.RuntimeStatus = if ($null -eq $seedManifest) { 'not-observed' } else { [string]$seedManifest.status }
            $runnerMetadata.SyntheticLibrary.RuntimeManifestObserved = $null -ne $seedManifest
            if ($null -ne $seedManifest) {
                Copy-Item -LiteralPath $syntheticSeedManifestPath -Destination (Join-Path $Output 'synthetic-library-manifest.json') -Force
                $runnerMetadata.SyntheticLibrary.PresentCount = [int]$seedManifest.presentCount
                $runnerMetadata.SyntheticLibrary.AddedCount = [int]$seedManifest.addedCount
                $runnerMetadata.SyntheticLibrary.RuntimeAssemblyIdentity = [string]$seedManifest.assemblyIdentity
                $runnerMetadata.SyntheticLibrary.CopiedManifest = Join-Path $Output 'synthetic-library-manifest.json'
            }
            else {
                $runnerMetadata.SyntheticLibrary.UnverifiedReason = 'No manifest from this run was observed before the host exited or the 30-second startup window elapsed.'
            }
            $runnerMetadata | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $Output 'runner-metadata.json') -Encoding UTF8
        }
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
        @{ Name = 'GSC_UI_AUDIT_EVENT_PIPE_NAME'; Value = $previousAuditEventPipeName },
        @{ Name = 'GSC_UI_AUDIT_SEED_DIRECTORY'; Value = $previousSyntheticSeedDirectory },
        @{ Name = 'GSC_UI_AUDIT_SEED_RUN_ID'; Value = $previousSyntheticSeedRunId },
        @{ Name = 'GSC_UI_AUDIT_SEED_COUNT'; Value = $previousSyntheticSeedCount }
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

if (-not ('GscTopLevelWindowProbe' -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class GscTopLevelWindowProbe
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int count);

    public static object[] Enumerate(uint[] processIds)
    {
        var windows = new List<object>();
        if (processIds == null || processIds.Length == 0)
        {
            return windows.ToArray();
        }

        EnumWindows((hWnd, lParam) =>
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);
            if (Array.IndexOf(processIds, processId) < 0)
            {
                return true;
            }

            var title = new StringBuilder(512);
            var className = new StringBuilder(256);
            GetWindowText(hWnd, title, title.Capacity);
            GetClassName(hWnd, className, className.Capacity);
            windows.Add(new
            {
                Handle = hWnd.ToInt64(),
                HandleHex = "0x" + hWnd.ToInt64().ToString("X"),
                ProcessId = processId,
                IsVisible = IsWindowVisible(hWnd),
                Title = title.ToString(),
                ClassName = className.ToString()
            });
            return true;
        }, IntPtr.Zero);

        return windows.ToArray();
    }
}
"@
}

function Get-PlayniteProcessSnapshot {
    return @(
        Get-Process -Name 'Playnite.DesktopApp' -ErrorAction SilentlyContinue |
            ForEach-Object {
                $process = $_
                try {
                    $process.Refresh()
                    $mainWindowHandle = [Int64]$process.MainWindowHandle
                    [ordered]@{
                        ProcessId = $process.Id
                        ProcessName = $process.ProcessName
                        HasExited = [bool]$process.HasExited
                        MainWindowHandle = $mainWindowHandle
                        MainWindowHandleHex = ('0x{0:X}' -f $mainWindowHandle)
                        MainWindowTitle = [string]$process.MainWindowTitle
                    }
                }
                catch {
                    [ordered]@{
                        ProcessId = $process.Id
                        ProcessName = $process.ProcessName
                        HasExited = $true
                        MainWindowHandle = 0
                        MainWindowHandleHex = '0x0'
                        MainWindowTitle = ''
                        ProbeError = $_.Exception.Message
                    }
                }
            }
    )
}

function Write-RealHostWindowExposure {
    param(
        [bool]$SidebarAutomationFound,
        [object]$UiAutomationProbe
    )

    $processSnapshots = @(Get-PlayniteProcessSnapshot)
    $topLevelWindows = @()
    if ($processSnapshots.Count -gt 0) {
        $processIds = @($processSnapshots | ForEach-Object { [uint32]$_.ProcessId })
        $topLevelWindows = @([GscTopLevelWindowProbe]::Enumerate($processIds))
    }

    $classification = 'playnite-process-not-observed'
    if ($processSnapshots.Count -gt 0 -and $topLevelWindows.Count -eq 0) {
        $classification = 'playnite-process-without-top-level-window'
    }
    elseif ($topLevelWindows.Count -gt 0 -and -not $SidebarAutomationFound) {
        $classification = 'top-level-window-observed-ui-automation-not-confirmed'
    }
    elseif ($SidebarAutomationFound) {
        $classification = 'sidebar-automation-found-window-exposure-captured'
    }

    $evidence = [ordered]@{
        Scenario = 'real-host-window-exposure'
        EvidenceSource = 'RealPlaynite'
        Classification = $classification
        ObservedUtc = (Get-Date).ToUniversalTime().ToString('o')
        OutputRoot = $Output
        UserDataMode = if ([string]::IsNullOrWhiteSpace($UserDataDir)) { 'current-user-data' } else { 'isolated-user-data' }
        PlayniteExecutable = $PlayniteExecutable
        StartedProcessId = if ($null -eq $startedPlayniteProcess) { $null } else { $startedPlayniteProcess.Id }
        UiAutomationProbeSeconds = if ($null -eq $UiAutomationProbe) { 60 } else { $UiAutomationProbe.ProbeSeconds }
        SidebarAutomationFound = $SidebarAutomationFound
        UiAutomation = $UiAutomationProbe
        ProcessSnapshots = $processSnapshots
        TopLevelWindows = $topLevelWindows
        MainWindowHandleNonZero = @($processSnapshots | Where-Object { $_.MainWindowHandle -ne 0 }).Count
        TopLevelWindowCount = $topLevelWindows.Count
        VisualEvidenceCaptured = $false
        CountsAsVisualPass = $false
    }
    $path = Join-Path $Output 'host-window-exposure.json'
    $evidence | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
    Write-Warning "Real-host window exposure evidence: $classification. Evidence: $path"
    return $evidence
}

function Get-PlayniteUiAutomationCandidates {
    $processSnapshots = @(Get-PlayniteProcessSnapshot)
    $processIds = @($processSnapshots | ForEach-Object { [uint32]$_.ProcessId })
    $topLevelWindows = @()
    if ($processSnapshots.Count -gt 0) {
        $topLevelWindows = @([GscTopLevelWindowProbe]::Enumerate($processIds))
    }

    $knownHandles = @{}
    $candidates = New-Object 'System.Collections.Generic.List[object]'
    foreach ($window in $topLevelWindows) {
        if ($null -eq $window -or [Int64]$window.Handle -eq 0) {
            continue
        }
        $handle = [Int64]$window.Handle
        if ($knownHandles.ContainsKey($handle)) {
            continue
        }
        $knownHandles[$handle] = $true
        $candidates.Add([ordered]@{
                Handle = $handle
                HandleHex = [string]$window.HandleHex
                ProcessId = [uint32]$window.ProcessId
                IsVisible = [bool]$window.IsVisible
                Title = [string]$window.Title
                ClassName = [string]$window.ClassName
                Source = 'win32-top-level-enumeration'
            })
    }

    # Keep the historical MainWindowHandle path as a candidate even when the host
    # does not expose it through EnumWindows during the same polling interval.
    foreach ($process in @($processSnapshots | Where-Object { $_.MainWindowHandle -ne 0 })) {
        $handle = [Int64]$process.MainWindowHandle
        if ($knownHandles.ContainsKey($handle)) {
            continue
        }
        $knownHandles[$handle] = $true
        $candidates.Add([ordered]@{
                Handle = $handle
                HandleHex = [string]$process.MainWindowHandleHex
                ProcessId = [uint32]$process.ProcessId
                IsVisible = $true
                Title = [string]$process.MainWindowTitle
                ClassName = ''
                Source = 'process-main-window-handle'
            })
    }

    return [ordered]@{
        ProcessSnapshots = $processSnapshots
        TopLevelWindows = $topLevelWindows
        Candidates = @($candidates.ToArray())
    }
}

function Invoke-GameSaveCenterSidebar {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms
    $deadline = (Get-Date).AddSeconds(60)
    $attempts = 0
    $lastCandidateWindows = @()
    $lastMatchedWindows = @()
    while ((Get-Date) -lt $deadline) {
        $attempts++
        $candidateSnapshot = Get-PlayniteUiAutomationCandidates
        $windowResults = New-Object 'System.Collections.Generic.List[object]'
        $matchedWindows = New-Object 'System.Collections.Generic.List[object]'
        foreach ($candidate in @($candidateSnapshot.Candidates)) {
            $windowResult = [ordered]@{
                Handle = [Int64]$candidate.Handle
                HandleHex = [string]$candidate.HandleHex
                ProcessId = [uint32]$candidate.ProcessId
                IsVisible = [bool]$candidate.IsVisible
                Title = [string]$candidate.Title
                ClassName = [string]$candidate.ClassName
                Source = [string]$candidate.Source
                AutomationRootFound = $false
                SidebarElementFound = $false
            }
            $window = $null
            try {
                $window = [System.Windows.Automation.AutomationElement]::FromHandle(
                    [IntPtr]$candidate.Handle)
                if ($null -ne $window) {
                    $windowResult.AutomationRootFound = $true
                    $windowResult.AutomationRootName = [string]$window.Current.Name
                    $windowResult.AutomationRootControlType = [string]$window.Current.ControlType.ProgrammaticName
                }
            }
            catch {
                $windowResult.AutomationError = $_.Exception.Message
            }
            if ($null -eq $window) {
                $windowResults.Add($windowResult)
                continue
            }

            $condition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty,
                'GameSaveCenter')
            $item = $null
            try {
                $item = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
            }
            catch {
                $windowResult.AutomationError = $_.Exception.Message
            }
            if ($item) {
                $windowResult.SidebarElementFound = $true
                $windowResult.SidebarElementControlType = [string]$item.Current.ControlType.ProgrammaticName
                $windowResult.SidebarElementName = [string]$item.Current.Name
                $matchedWindows.Add($windowResult)
                $invoke = $null
                try { $invoke = $item.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern) } catch { }
                if ($invoke) {
                    $invoke.Invoke()
                    Write-Host 'Clicked GameSaveCenter sidebar item' -ForegroundColor Green
                    $windowResult.Action = 'invoke'
                    $windowResults.Add($windowResult)
                    return [pscustomobject]@{
                        Found = $true
                        Action = 'invoke'
                        Attempts = $attempts
                        ProbeSeconds = 60
                        CandidateWindows = @($windowResults.ToArray())
                        MatchedWindows = @($matchedWindows.ToArray())
                        ProcessSnapshots = @($candidateSnapshot.ProcessSnapshots)
                        TopLevelWindows = @($candidateSnapshot.TopLevelWindows)
                    }
                }
                $select = $null
                try { $select = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern) } catch { }
                if ($select) {
                    $select.Select()
                    Write-Host 'Selected GameSaveCenter sidebar item' -ForegroundColor Green
                    $windowResult.Action = 'select'
                    $windowResults.Add($windowResult)
                    return [pscustomobject]@{
                        Found = $true
                        Action = 'select'
                        Attempts = $attempts
                        ProbeSeconds = 60
                        CandidateWindows = @($windowResults.ToArray())
                        MatchedWindows = @($matchedWindows.ToArray())
                        ProcessSnapshots = @($candidateSnapshot.ProcessSnapshots)
                        TopLevelWindows = @($candidateSnapshot.TopLevelWindows)
                    }
                }
                try {
                    $item.SetFocus()
                    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
                    Write-Host 'Focused GameSaveCenter sidebar item' -ForegroundColor Green
                    $windowResult.Action = 'focus-enter'
                    $windowResults.Add($windowResult)
                    return [pscustomobject]@{
                        Found = $true
                        Action = 'focus-enter'
                        Attempts = $attempts
                        ProbeSeconds = 60
                        CandidateWindows = @($windowResults.ToArray())
                        MatchedWindows = @($matchedWindows.ToArray())
                        ProcessSnapshots = @($candidateSnapshot.ProcessSnapshots)
                        TopLevelWindows = @($candidateSnapshot.TopLevelWindows)
                    }
                }
                catch {
                    $windowResult.ActionError = $_.Exception.Message
                }
            }
            $windowResults.Add($windowResult)
        }

        $lastCandidateWindows = @($windowResults.ToArray())
        $lastMatchedWindows = @($matchedWindows.ToArray())
        Start-Sleep -Seconds 2
    }
    Write-Warning 'Could not locate GameSaveCenter sidebar item via UI Automation.'
    return [pscustomobject]@{
        Found = $false
        Action = 'not-found'
        Attempts = $attempts
        ProbeSeconds = 60
        CandidateWindows = $lastCandidateWindows
        MatchedWindows = $lastMatchedWindows
    }
}

$sidebarProbe = Invoke-GameSaveCenterSidebar
$sidebarAutomationFound = [bool]$sidebarProbe.Found
Write-RealHostWindowExposure -SidebarAutomationFound $sidebarAutomationFound -UiAutomationProbe $sidebarProbe | Out-Null

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
