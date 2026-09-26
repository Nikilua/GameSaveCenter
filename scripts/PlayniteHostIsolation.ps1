function Resolve-GscRepositoryScopedPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][ValidateSet('.tmp', 'artifacts')][string]$ScopeRootName
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $scopeRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryPath $ScopeRootName))
    if (-not (Test-Path -LiteralPath $scopeRoot -PathType Container)) {
        throw "Repository scope root does not exist: $scopeRoot"
    }

    $scopeItem = Get-Item -LiteralPath $scopeRoot -Force
    if (($scopeItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Repository scope root cannot be a reparse point: $scopeRoot"
    }

    $candidatePath = [System.IO.Path]::GetFullPath($Path)
    $scopePrefix = $scopeRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidatePath.StartsWith($scopePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path must be a descendant of repository ${ScopeRootName}: $candidatePath"
    }

    $relativePath = $candidatePath.Substring($scopePrefix.Length)
    $cursor = $scopeRoot
    foreach ($segment in ($relativePath -split '[\\/]+')) {
        if ([string]::IsNullOrWhiteSpace($segment)) { continue }
        $cursor = Join-Path $cursor $segment
        if (-not (Test-Path -LiteralPath $cursor)) { continue }
        $item = Get-Item -LiteralPath $cursor -Force
        if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Repository-scoped path cannot traverse a reparse point: $cursor"
        }
    }

    return $candidatePath
}

function Get-GscPlayniteConflictingProcesses {
    [CmdletBinding()]
    param()

    $snapshots = [System.Collections.Generic.List[object]]::new()
    foreach ($processName in @('Playnite.DesktopApp', 'Playnite.FullscreenApp', 'GameSaveCenter.Worker')) {
        foreach ($process in @(Get-Process -Name $processName -ErrorAction SilentlyContinue)) {
            $processPath = ''
            try { $processPath = [string]$process.Path } catch { }
            $snapshots.Add([pscustomobject]@{
                ProcessId = [int]$process.Id
                ProcessName = [string]$process.ProcessName
                Path = $processPath
            })
        }
    }

    return @($snapshots.ToArray())
}

function Assert-GscNoActivePlayniteProcesses {
    [CmdletBinding()]
    param([AllowEmptyCollection()][object[]]$Processes = @())

    if ($Processes.Count -gt 0) {
        $details = @($Processes | ForEach-Object { "$($_.ProcessName) [$($_.ProcessId)]" }) -join ', '
        throw "Isolated Playnite audit refused because Playnite or GameSaveCenter Worker is already running: $details. Close the user instance manually, then retry."
    }
}

function Assert-GscProcessCommandLineInspectionAvailable {
    [CmdletBinding()]
    param()

    try {
        $currentProcess = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $PID" -ErrorAction Stop
        if ($null -eq $currentProcess -or [string]::IsNullOrWhiteSpace([string]$currentProcess.CommandLine)) {
            throw 'The current process command line was not returned.'
        }
    }
    catch {
        throw "Isolated Playnite audit requires process-command-line inspection before launch; refusing to start a host that cannot be verified. $($_.Exception.Message)"
    }
}

function Assert-GscNoReparsePointsUnderPath {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$Path)

    $rootPath = [System.IO.Path]::GetFullPath($Path)
    if (-not (Test-Path -LiteralPath $rootPath -PathType Container)) {
        throw "Isolated Playnite profile directory does not exist: $rootPath"
    }

    $pendingDirectories = [System.Collections.Generic.Stack[string]]::new()
    $pendingDirectories.Push($rootPath)
    while ($pendingDirectories.Count -gt 0) {
        $currentDirectory = $pendingDirectories.Pop()
        foreach ($entryPath in [System.IO.Directory]::GetFileSystemEntries($currentDirectory)) {
            $attributes = [System.IO.File]::GetAttributes($entryPath)
            if (($attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Isolated Playnite profile cannot contain reparse points: $entryPath"
            }
            if (($attributes -band [System.IO.FileAttributes]::Directory) -ne 0) {
                $pendingDirectories.Push($entryPath)
            }
        }
    }
}

function Assert-GscPlayniteProfileDatabaseIsolation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$UserDataDir,
        [Parameter(Mandatory = $true)][psobject]$Configuration
    )

    $databasePath = [string]$Configuration.DatabasePath
    if ([string]::IsNullOrWhiteSpace($databasePath)) {
        throw 'Isolated Playnite config must declare a DatabasePath before host startup.'
    }

    $profilePath = [System.IO.Path]::GetFullPath($UserDataDir)
    $resolvedDatabasePath = if ([System.IO.Path]::IsPathRooted($databasePath)) {
        [System.IO.Path]::GetFullPath($databasePath)
    }
    else {
        [System.IO.Path]::GetFullPath((Join-Path $profilePath $databasePath))
    }
    $profilePrefix = $profilePath.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedDatabasePath.StartsWith($profilePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Isolated Playnite database path must remain under its marked user-data profile: $resolvedDatabasePath"
    }

    $cursor = $resolvedDatabasePath
    while ($cursor.StartsWith($profilePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Isolated Playnite database path cannot traverse a reparse point: $cursor"
            }
        }
        $parent = Split-Path -Parent $cursor
        if ([string]::Equals($parent, $cursor, [System.StringComparison]::OrdinalIgnoreCase)) { break }
        $cursor = $parent
    }

    return $resolvedDatabasePath
}

function Assert-GscPlayniteExecutable {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$PlayniteExecutable)

    $resolvedPath = [System.IO.Path]::GetFullPath($PlayniteExecutable)
    if (-not (Test-Path -LiteralPath $resolvedPath -PathType Leaf)) {
        throw "Playnite executable does not exist: $resolvedPath"
    }
    if (-not [string]::Equals([System.IO.Path]::GetFileName($resolvedPath), 'Playnite.DesktopApp.exe', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Isolated desktop audit requires Playnite.DesktopApp.exe: $resolvedPath"
    }

    $executableItem = Get-Item -LiteralPath $resolvedPath -Force
    if (($executableItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Playnite executable cannot be a reparse point: $resolvedPath"
    }

    return $resolvedPath
}

function Get-GscHostAuditOutputPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$Output = ''
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
    if ([string]::IsNullOrWhiteSpace($Output)) {
        $runToken = ([Guid]::NewGuid().ToString('N')).Substring(0, 8)
        $runStamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff')
        $Output = Join-Path (Join-Path $repositoryPath 'artifacts') "ui-host-audit-$runStamp-$runToken"
    }

    $resolvedOutput = Resolve-GscRepositoryScopedPath -RepositoryRoot $repositoryPath -Path $Output -ScopeRootName 'artifacts'
    if (Test-Path -LiteralPath $resolvedOutput) {
        throw "Refusing to overwrite existing Playnite host audit output: $resolvedOutput"
    }

    return $resolvedOutput
}

function Initialize-GscIsolatedPlayniteProfile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$UserDataDir
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $profilePath = Resolve-GscRepositoryScopedPath -RepositoryRoot $repositoryPath -Path $UserDataDir -ScopeRootName '.tmp'
    if (Test-Path -LiteralPath $profilePath) {
        if (-not (Test-Path -LiteralPath $profilePath -PathType Container)) {
            throw "Isolated Playnite user-data path is not a directory: $profilePath"
        }
    }
    else {
        New-Item -ItemType Directory -Path $profilePath -Force | Out-Null
    }

    $markerPath = Join-Path $profilePath '.gsc-playnite-audit-profile.json'
    if (Test-Path -LiteralPath $markerPath) {
        $markerItem = Get-Item -LiteralPath $markerPath -Force
        if (($markerItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Isolated Playnite profile marker cannot be a reparse point: $markerPath"
        }
        $marker = Get-Content -LiteralPath $markerPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $markerProfileId = [Guid]::Empty
        if ([int]$marker.SchemaVersion -ne 1 -or
            -not [string]::Equals([string]$marker.RepositoryRoot, $repositoryPath, [System.StringComparison]::OrdinalIgnoreCase) -or
            -not [Guid]::TryParse([string]$marker.ProfileId, [ref]$markerProfileId)) {
            throw "Isolated Playnite profile marker is invalid or belongs to another repository: $markerPath"
        }
        Assert-GscNoReparsePointsUnderPath -Path $profilePath
        return [pscustomobject]@{
            Path = $profilePath
            MarkerPath = $markerPath
            ProfileId = $markerProfileId.ToString('D')
            Created = $false
        }
    }

    $existingContents = @(Get-ChildItem -LiteralPath $profilePath -Force)
    if ($existingContents.Count -gt 0) {
        throw "Refusing to adopt a non-empty unmarked Playnite profile: $profilePath"
    }

    $profileId = [Guid]::NewGuid().ToString('D')
    $marker = [ordered]@{
        SchemaVersion = 1
        ProfileId = $profileId
        RepositoryRoot = $repositoryPath
        CreatedBy = 'scripts/real-host-audit.ps1'
        CreatedUtc = [DateTime]::UtcNow.ToString('O')
    }
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($markerPath, ($marker | ConvertTo-Json -Depth 4), $encoding)
    Assert-GscNoReparsePointsUnderPath -Path $profilePath

    return [pscustomobject]@{
        Path = $profilePath
        MarkerPath = $markerPath
        ProfileId = $profileId
        Created = $true
    }
}

function New-GscPlayniteUserDataArguments {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$UserDataDir)

    if ($UserDataDir.Contains('"')) {
        throw 'Playnite user-data path contains a quote and cannot be represented safely as a command-line argument.'
    }
    return '--startdesktop --hidesplashscreen --userdatadir "{0}"' -f $UserDataDir
}

function Get-GscPlayniteProcessStartEvidence {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][System.Diagnostics.Process]$StartedProcess,
        [Parameter(Mandatory = $true)][string]$ExpectedExecutable,
        [Parameter(Mandatory = $true)][string]$ExpectedUserDataDir
    )

    $deadline = [DateTime]::UtcNow.AddSeconds(3)
    do {
        $StartedProcess.Refresh()
        $processRecord = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $($StartedProcess.Id)" -ErrorAction Stop
        if ($null -ne $processRecord) {
            $expectedPath = [System.IO.Path]::GetFullPath($ExpectedExecutable)
            $actualPath = [string]$processRecord.ExecutablePath
            if ([string]::IsNullOrWhiteSpace($actualPath) -or
                -not [string]::Equals([System.IO.Path]::GetFullPath($actualPath), $expectedPath, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Started Playnite process executable does not match the requested isolated executable. PID=$($StartedProcess.Id); actual='$actualPath'; expected='$expectedPath'"
            }

            $commandLine = [string]$processRecord.CommandLine
            $expectedDataDir = [System.IO.Path]::GetFullPath($ExpectedUserDataDir)
            if ($commandLine -notmatch '(?i)(?:^|\s)--userdatadir(?:\s|$)' -or
                -not $commandLine.Contains($expectedDataDir)) {
                throw "Started Playnite process command line does not prove the requested --userdatadir. PID=$($StartedProcess.Id); expected='$expectedDataDir'; command='$commandLine'"
            }

            return [ordered]@{
                Status = 'verified-isolated-process'
                ProcessId = [int]$processRecord.ProcessId
                ProcessName = [string]$processRecord.Name
                ExecutablePath = $actualPath
                CommandLine = $commandLine
                UserDataDir = $expectedDataDir
                ObservedUtc = [DateTime]::UtcNow.ToString('O')
            }
        }

        if ($StartedProcess.HasExited) {
            return [ordered]@{
                Status = 'process-exited-before-isolation-snapshot'
                ProcessId = [int]$StartedProcess.Id
                ExecutablePath = [System.IO.Path]::GetFullPath($ExpectedExecutable)
                UserDataDir = [System.IO.Path]::GetFullPath($ExpectedUserDataDir)
                ObservedUtc = [DateTime]::UtcNow.ToString('O')
            }
        }

        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Unable to observe the isolated Playnite process command line within three seconds. PID=$($StartedProcess.Id)"
}
