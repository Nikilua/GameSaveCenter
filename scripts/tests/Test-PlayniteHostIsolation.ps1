$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$scopeRoot = Join-Path $repoRoot '.tmp'
if (-not (Test-Path -LiteralPath $scopeRoot -PathType Container)) {
    New-Item -ItemType Directory -Path $scopeRoot -Force | Out-Null
}

. (Join-Path (Split-Path -Parent $PSScriptRoot) 'PlayniteHostIsolation.ps1')

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Assert-ThrowsContaining {
    param([scriptblock]$Action, [string]$ExpectedText)
    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notlike "*$ExpectedText*") {
            throw "Expected error containing '$ExpectedText', got '$($_.Exception.Message)'"
        }
        return
    }
    throw "Expected an exception containing '$ExpectedText'."
}

$testRoot = Join-Path $scopeRoot ("env001-isolation-test-" + [Guid]::NewGuid().ToString('N'))
$outputPath = ''
New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $profilePath = Join-Path $testRoot 'profile with spaces'
    $profile = Initialize-GscIsolatedPlayniteProfile -RepositoryRoot $repoRoot -UserDataDir $profilePath
    Assert-True $profile.Created 'A new empty profile should receive an audit marker.'
    Assert-True (Test-Path -LiteralPath $profile.MarkerPath -PathType Leaf) 'The audit marker should be written under the profile root.'

    $reopened = Initialize-GscIsolatedPlayniteProfile -RepositoryRoot $repoRoot -UserDataDir $profilePath
    Assert-True (-not $reopened.Created) 'A marked profile should be recognized rather than reinitialized.'
    Assert-True ($reopened.ProfileId -eq $profile.ProfileId) 'A marked profile should retain its stable profile ID.'
    $profileMarker = Get-Content -LiteralPath $profile.MarkerPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-True ($profileMarker.CreatedBy -eq 'scripts/real-host-audit.ps1') 'The existing real-host profile marker should keep its creator identity.'

    $installProfilePath = Join-Path $testRoot 'install-profile'
    $installProfile = Initialize-GscIsolatedPlayniteProfile `
        -RepositoryRoot $repoRoot `
        -UserDataDir $installProfilePath `
        -CreatedBy 'scripts/dev-install-run.ps1'
    $installProfileMarker = Get-Content -LiteralPath $installProfile.MarkerPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-True ($installProfileMarker.CreatedBy -eq 'scripts/dev-install-run.ps1') 'The one-click isolated profile marker should identify its creator.'

    $isolatedWorkerDirectory = Join-Path $installProfilePath 'GameSaveCenter'
    New-Item -ItemType Directory -Path $isolatedWorkerDirectory -Force | Out-Null
    $environmentNames = @(
        'GameSaveCenter__DataDirectory', 'GameSaveCenter__PipeName', 'GameSaveCenter__EventPipeName',
        'GameSaveCenter__EnableCloudUpload', 'GameSaveCenter__RcloneDestination', 'RCLONE_CONFIG',
        'GSC_REAL_HOST_AUDIT', 'GSC_UI_AUDIT_PIPE_NAME')
    $originalEnvironment = @{}
    foreach ($name in $environmentNames) {
        $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, [EnvironmentVariableTarget]::Process)
    }
    $playniteStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $playniteStartInfo.FileName = $env:ComSpec
    $playniteStartInfo.Arguments = '/d /c if /i not "%GameSaveCenter__EnableCloudUpload%"=="false" (exit /b 40) else if defined GameSaveCenter__RcloneDestination (exit /b 41) else if defined RCLONE_CONFIG (exit /b 42) else if defined GSC_REAL_HOST_AUDIT (exit /b 43) else if defined GSC_UI_AUDIT_PIPE_NAME (exit /b 44) else exit /b 0'
    $playniteStartInfo.UseShellExecute = $false
    $childEnvironment = $playniteStartInfo.EnvironmentVariables
    foreach ($name in $environmentNames) {
        $childEnvironment[$name] = "sentinel-$name"
    }
    Configure-GscIsolatedPlayniteProcessEnvironment `
        -ProcessStartInfo $playniteStartInfo `
        -WorkerDataDirectory $isolatedWorkerDirectory `
        -PipeName 'GameSaveCenter.Worker.Isolated.fixture' `
        -EventPipeName 'GameSaveCenter.Worker.Isolated.fixture.Events'
    Assert-True ($childEnvironment['GameSaveCenter__DataDirectory'] -eq $isolatedWorkerDirectory) 'The isolated Worker should receive only its private data directory.'
    Assert-True ($childEnvironment['GameSaveCenter__PipeName'] -eq 'GameSaveCenter.Worker.Isolated.fixture') 'The isolated Worker should receive its private IPC pipe.'
    Assert-True ($childEnvironment['GameSaveCenter__EventPipeName'] -eq 'GameSaveCenter.Worker.Isolated.fixture.Events') 'The isolated Worker should receive its private event pipe.'
    Assert-True ($childEnvironment['GameSaveCenter__EnableCloudUpload'] -eq 'false') 'Cloud upload must be explicitly disabled for an isolated Playnite launch.'
    foreach ($name in @('GameSaveCenter__RcloneDestination', 'RCLONE_CONFIG', 'GSC_REAL_HOST_AUDIT', 'GSC_UI_AUDIT_PIPE_NAME')) {
        Assert-True (-not $childEnvironment.ContainsKey($name)) "Isolated Playnite child must not inherit $name."
    }
    $playniteProcess = [System.Diagnostics.Process]::Start($playniteStartInfo)
    if ($null -eq $playniteProcess) {
        throw 'Could not start the isolated environment probe process.'
    }
    try {
        if (-not $playniteProcess.WaitForExit(5000)) {
            $playniteProcess.Kill()
            throw 'The isolated environment probe process did not exit within five seconds.'
        }
        Assert-True ($playniteProcess.ExitCode -eq 0) 'Sensitive upload or audit environment variables reached the isolated child process.'
    }
    finally {
        $playniteProcess.Dispose()
    }
    foreach ($name in $environmentNames) {
        if ([Environment]::GetEnvironmentVariable($name, [EnvironmentVariableTarget]::Process) -ne $originalEnvironment[$name]) {
            throw "Isolated launch changed the caller's process environment variable: $name"
        }
    }

    $copiedMarkerProfilePath = Join-Path $testRoot 'copied-marker-profile'
    New-Item -ItemType Directory -Path $copiedMarkerProfilePath | Out-Null
    $copiedMarkerPath = Join-Path $copiedMarkerProfilePath (Split-Path -Leaf $profile.MarkerPath)
    Copy-Item -LiteralPath $profile.MarkerPath -Destination $copiedMarkerPath
    $copiedProfileData = Join-Path $copiedMarkerProfilePath 'preserve-me.txt'
    [System.IO.File]::WriteAllText($copiedProfileData, 'copied markers must not adopt another directory')
    Assert-ThrowsContaining {
        Initialize-GscIsolatedPlayniteProfile -RepositoryRoot $repoRoot -UserDataDir $copiedMarkerProfilePath
    } 'another repository/profile'
    Assert-True (Test-Path -LiteralPath $copiedProfileData -PathType Leaf) 'A profile containing a copied marker must remain untouched.'

    $startupArguments = New-GscPlayniteUserDataArguments -UserDataDir $profilePath
    Assert-True ($startupArguments.Contains('--userdatadir')) 'The launch arguments must explicitly select Playnite user data.'
    Assert-True ($startupArguments.Contains('"' + $profilePath + '"')) 'A user-data path with spaces must remain one quoted argument.'
    $safeDatabase = Assert-GscPlayniteProfileDatabaseIsolation -UserDataDir $profilePath -Configuration ([pscustomobject]@{ DatabasePath = (Join-Path $profilePath 'library') })
    Assert-True ($safeDatabase -eq (Join-Path $profilePath 'library')) 'A database under the marked profile should be accepted.'
    Assert-ThrowsContaining {
        Assert-GscPlayniteProfileDatabaseIsolation -UserDataDir $profilePath -Configuration ([pscustomobject]@{ DatabasePath = (Join-Path $env:TEMP 'outside-library') })
    } 'must remain under its marked user-data profile'

    Assert-ThrowsContaining {
        Resolve-GscRepositoryScopedPath -RepositoryRoot $repoRoot -Path (Join-Path $repoRoot 'artifacts\outside-profile') -ScopeRootName '.tmp'
    } 'descendant of repository .tmp'
    Assert-ThrowsContaining {
        Resolve-GscRepositoryScopedPath -RepositoryRoot $repoRoot -Path $scopeRoot -ScopeRootName '.tmp'
    } 'descendant of repository .tmp'

    $unmarkedPath = Join-Path $testRoot 'unmarked-profile'
    New-Item -ItemType Directory -Path $unmarkedPath | Out-Null
    $unmarkedFile = Join-Path $unmarkedPath 'preserve-me.txt'
    [System.IO.File]::WriteAllText($unmarkedFile, 'must remain untouched')
    Assert-ThrowsContaining {
        Initialize-GscIsolatedPlayniteProfile -RepositoryRoot $repoRoot -UserDataDir $unmarkedPath
    } 'non-empty unmarked'
    Assert-True (Test-Path -LiteralPath $unmarkedFile -PathType Leaf) 'An unrecognized existing profile must remain untouched.'

    Assert-GscNoActivePlayniteProcesses -Processes @()
    Assert-ThrowsContaining {
        Assert-GscNoActivePlayniteProcesses -Processes @([pscustomobject]@{ ProcessName = 'Playnite.DesktopApp'; ProcessId = 4242 })
    } 'already running'
    Assert-ThrowsContaining {
        Assert-GscNoActivePlayniteProcesses -Processes @([pscustomobject]@{ ProcessName = 'GameSaveCenter.Worker'; ProcessId = 4243 })
    } 'already running'

    $fakeExecutable = Join-Path $testRoot 'Playnite.DesktopApp.exe'
    [System.IO.File]::WriteAllBytes($fakeExecutable, [byte[]]@())
    Assert-True ((Assert-GscPlayniteExecutable -PlayniteExecutable $fakeExecutable) -eq $fakeExecutable) 'An explicit desktop executable path should be canonicalized.'
    $wrongExecutable = Join-Path $testRoot 'Other.exe'
    [System.IO.File]::WriteAllBytes($wrongExecutable, [byte[]]@())
    Assert-ThrowsContaining { Assert-GscPlayniteExecutable -PlayniteExecutable $wrongExecutable } 'Playnite.DesktopApp.exe'

    $script:mockPlayniteProcessRecord = [pscustomobject]@{
        ProcessId = $PID
        Name = 'Playnite.DesktopApp.exe'
        ExecutablePath = $fakeExecutable
        CommandLine = '"' + $fakeExecutable + '" --startdesktop --hidesplashscreen --userdatadir "' + $profilePath + '"'
    }
    $script:terminateAfterFirstPlayniteSnapshot = $null
    function Get-CimInstance {
        [CmdletBinding()]
        param([string]$ClassName, [string]$Filter)
        if ($null -ne $script:terminateAfterFirstPlayniteSnapshot -and -not $script:forwardingSnapshotReturned) {
            $script:forwardingSnapshotReturned = $true
            $snapshot = $script:mockPlayniteProcessRecord
            $script:terminateAfterFirstPlayniteSnapshot.Kill()
            return $snapshot
        }
        return $script:mockPlayniteProcessRecord
    }
    $currentProcess = [System.Diagnostics.Process]::GetCurrentProcess()
    $verifiedStart = Get-GscPlayniteProcessStartEvidence `
        -StartedProcess $currentProcess `
        -ExpectedExecutable $fakeExecutable `
        -ExpectedUserDataDir $profilePath
    Assert-True ($verifiedStart.Status -eq 'verified-isolated-process') 'A process record with the exact isolated path should be accepted.'
    Assert-True ($verifiedStart.ObservedUserDataDir -eq $profilePath) 'The observed user-data path should be recorded explicitly.'

    $siblingProfilePath = $profilePath + '-sibling'
    $script:mockPlayniteProcessRecord.CommandLine = '"' + $fakeExecutable + '" --userdatadir "' + $siblingProfilePath + '"'
    Assert-ThrowsContaining {
        Get-GscPlayniteProcessStartEvidence -StartedProcess $currentProcess -ExpectedExecutable $fakeExecutable -ExpectedUserDataDir $profilePath
    } 'selected a different user-data path'

    $script:mockPlayniteProcessRecord.CommandLine = '"' + $fakeExecutable + '" --userdatadir "' + $profilePath + '" --userdatadir "' + $siblingProfilePath + '"'
    Assert-ThrowsContaining {
        Get-GscPlayniteProcessStartEvidence -StartedProcess $currentProcess -ExpectedExecutable $fakeExecutable -ExpectedUserDataDir $profilePath
    } 'exactly one quoted --userdatadir'

    $forwardedStartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $forwardedStartInfo.FileName = $env:ComSpec
    $forwardedStartInfo.Arguments = '/c ping 127.0.0.1 -n 6 >NUL'
    $forwardedStartInfo.UseShellExecute = $false
    $forwardedProcess = [System.Diagnostics.Process]::Start($forwardedStartInfo)
    try {
        $script:mockPlayniteProcessRecord = [pscustomobject]@{
            ProcessId = $forwardedProcess.Id
            Name = 'Playnite.DesktopApp.exe'
            ExecutablePath = $fakeExecutable
            CommandLine = '"' + $fakeExecutable + '" --startdesktop --hidesplashscreen --userdatadir "' + $profilePath + '"'
        }
        $script:terminateAfterFirstPlayniteSnapshot = $forwardedProcess
        $script:forwardingSnapshotReturned = $false
        Assert-ThrowsContaining {
            Get-GscPlayniteProcessStartEvidence -StartedProcess $forwardedProcess -ExpectedExecutable $fakeExecutable -ExpectedUserDataDir $profilePath
        } 'exited before the isolation process stability confirmation'
    }
    finally {
        $script:terminateAfterFirstPlayniteSnapshot = $null
        if (-not $forwardedProcess.HasExited) { $forwardedProcess.Kill() }
        $forwardedProcess.Dispose()
    }

    $exitStartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $exitStartInfo.FileName = $env:ComSpec
    $exitStartInfo.Arguments = '/c exit 0'
    $exitStartInfo.UseShellExecute = $false
    $exitedProcess = [System.Diagnostics.Process]::Start($exitStartInfo)
    try {
        $exitedProcess.WaitForExit()
        $script:mockPlayniteProcessRecord = $null
        Assert-ThrowsContaining {
            Get-GscPlayniteProcessStartEvidence -StartedProcess $exitedProcess -ExpectedExecutable $fakeExecutable -ExpectedUserDataDir $profilePath
        } 'exited before its isolation command line could be verified'
    }
    finally {
        $exitedProcess.Dispose()
        $currentProcess.Dispose()
    }

    $outputPath = Join-Path (Join-Path $repoRoot 'artifacts') ("env001-output-test-" + [Guid]::NewGuid().ToString('N'))
    Assert-True ((Get-GscHostAuditOutputPath -RepositoryRoot $repoRoot -Output $outputPath) -eq $outputPath) 'A new artifacts child should be accepted.'
    New-Item -ItemType Directory -Path $outputPath | Out-Null
    $preservedOutput = Join-Path $outputPath 'preserve-me.txt'
    [System.IO.File]::WriteAllText($preservedOutput, 'do not delete')
    Assert-ThrowsContaining {
        Get-GscHostAuditOutputPath -RepositoryRoot $repoRoot -Output $outputPath
    } 'Refusing to overwrite'
    Assert-True (Test-Path -LiteralPath $preservedOutput -PathType Leaf) 'An existing audit output must never be recursively deleted.'
    Assert-ThrowsContaining {
        Get-GscHostAuditOutputPath -RepositoryRoot $repoRoot -Output (Join-Path $repoRoot 'docs')
    } 'descendant of repository artifacts'

    $junctionPath = Join-Path $profilePath 'reparse-junction'
    $junctionCreated = $false
    try {
        New-Item -ItemType Junction -Path $junctionPath -Target (Join-Path $repoRoot 'artifacts') | Out-Null
        $junctionCreated = $true
    }
    catch {
        Write-Host "Junction check skipped by OS policy: $($_.Exception.Message)"
    }
    if ($junctionCreated) {
        Assert-ThrowsContaining {
            Resolve-GscRepositoryScopedPath -RepositoryRoot $repoRoot -Path (Join-Path $junctionPath 'redirected') -ScopeRootName '.tmp'
        } 'reparse point'
        Assert-ThrowsContaining {
            Assert-GscNoReparsePointsUnderPath -Path $testRoot
        } 'reparse points'
        Assert-ThrowsContaining {
            Initialize-GscIsolatedPlayniteProfile -RepositoryRoot $repoRoot -UserDataDir $profilePath
        } 'reparse points'
        [System.IO.Directory]::Delete($junctionPath, $false)
    }

    Write-Host 'Playnite host isolation helper tests passed.' -ForegroundColor Green
}
finally {
    $resolvedTempRoot = [System.IO.Path]::GetFullPath($scopeRoot)
    $resolvedTestRoot = [System.IO.Path]::GetFullPath($testRoot)
    $tempPrefix = $resolvedTempRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if ($resolvedTestRoot.StartsWith($tempPrefix, [System.StringComparison]::OrdinalIgnoreCase) -and
        -not [string]::Equals($resolvedTestRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar), $resolvedTempRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar), [System.StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $resolvedTestRoot -PathType Container)) {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force
    }

    if (-not [string]::IsNullOrWhiteSpace($outputPath)) {
        $resolvedArtifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts'))
        $resolvedOutputPath = [System.IO.Path]::GetFullPath($outputPath)
        $artifactsPrefix = $resolvedArtifactsRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
        if ($resolvedOutputPath.StartsWith($artifactsPrefix, [System.StringComparison]::OrdinalIgnoreCase) -and
            (Split-Path -Parent $resolvedOutputPath) -eq $resolvedArtifactsRoot -and
            (Test-Path -LiteralPath $resolvedOutputPath -PathType Container)) {
            Remove-Item -LiteralPath $resolvedOutputPath -Recurse -Force
        }
    }
}
