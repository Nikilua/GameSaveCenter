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
