[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')][string]$SdkVersion = '10.0.401',
    [string]$WorktreePath = '',
    [string]$OutputRoot = '',
    [string]$DiagnosticsRoot = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$runnerTemp = [Environment]::GetEnvironmentVariable('RUNNER_TEMP', 'Process')
if ([string]::IsNullOrWhiteSpace($runnerTemp)) {
    $runnerTemp = Join-Path $repoRoot '.tmp'
}

function Get-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    return [System.IO.Path]::GetFullPath($Path)
}

function Test-PathBelow {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Parent
    )

    $fullPath = Get-FullPath $Path
    $fullParent = (Get-FullPath $Parent).TrimEnd('\', '/')
    $prefix = $fullParent + [System.IO.Path]::DirectorySeparatorChar
    return $fullPath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

$temporaryRoot = Get-FullPath $runnerTemp
$repositoryTempRoot = Get-FullPath (Join-Path $repoRoot '.tmp')
$allowedTemporaryRoots = @($temporaryRoot, $repositoryTempRoot) | Select-Object -Unique

if ([string]::IsNullOrWhiteSpace($WorktreePath)) {
    $WorktreePath = Join-Path $temporaryRoot "gsc-sdk-$SdkVersion-worktree"
}
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $temporaryRoot "gsc-sdk-$SdkVersion-output"
}
if ([string]::IsNullOrWhiteSpace($DiagnosticsRoot)) {
    $DiagnosticsRoot = Join-Path $temporaryRoot "gsc-sdk-$SdkVersion-diagnostics"
}

$WorktreePath = Get-FullPath $WorktreePath
$OutputRoot = Get-FullPath $OutputRoot
$DiagnosticsRoot = Get-FullPath $DiagnosticsRoot
foreach ($candidate in @($WorktreePath, $OutputRoot, $DiagnosticsRoot)) {
    $allowed = $false
    foreach ($temporaryRootCandidate in $allowedTemporaryRoots) {
        if (Test-PathBelow $candidate $temporaryRootCandidate) {
            $allowed = $true
            break
        }
    }
    if (-not $allowed) {
        throw "SDK verification output must stay in RUNNER_TEMP or repository .tmp: $candidate"
    }
}

if (Test-Path -LiteralPath $WorktreePath) {
    throw "SDK verification worktree path already exists: $WorktreePath"
}
foreach ($outputPath in @($OutputRoot, $DiagnosticsRoot)) {
    if (Test-Path -LiteralPath $outputPath) {
        throw "SDK verification output path already exists: $outputPath"
    }
}

$headOutput = @(& git -C $repoRoot rev-parse --verify HEAD 2>&1)
$headExitCode = $LASTEXITCODE
$head = if ($headOutput.Count -gt 0) { $headOutput[0].ToString().Trim() } else { '' }
if ($headExitCode -ne 0 -or $head -notmatch '^[0-9a-fA-F]{40}$') {
    throw 'Could not resolve the current repository commit for the SDK verification worktree.'
}

$worktreeAdded = $false
try {
    Write-Host "Create isolated SDK verification worktree at $WorktreePath"
    & git -C $repoRoot worktree add --detach $WorktreePath $head
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not create the isolated SDK verification worktree.'
    }
    $worktreeAdded = $true

    $globalJsonPath = Join-Path $WorktreePath 'global.json'
    $globalConfiguration = Get-Content -Raw -LiteralPath $globalJsonPath | ConvertFrom-Json
    if (-not $globalConfiguration.sdk) {
        throw 'The isolated repository does not contain an SDK baseline.'
    }
    $globalConfiguration.sdk.version = $SdkVersion
    $globalConfiguration.sdk.rollForward = 'disable'
    $globalConfiguration.sdk.allowPrerelease = $false
    $globalJsonText = ConvertTo-Json -InputObject $globalConfiguration -Depth 8
    [System.IO.File]::WriteAllText(
        $globalJsonPath,
        $globalJsonText + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false)
    )

    Push-Location $WorktreePath
    try {
        . (Join-Path $WorktreePath 'scripts/sdk-baseline.ps1')
        $sdkInitialization = Initialize-GscSdkBaseline -ProjectRoot $WorktreePath
        $selectedSdk = $sdkInitialization.SelectedVersion

        Write-Host "Run Release build and tests with .NET SDK $selectedSdk"
        & (Join-Path $WorktreePath 'scripts/build.ps1') `
            -Configuration Release `
            -OutputRoot $OutputRoot `
            -DiagnosticsRoot $DiagnosticsRoot
    }
    finally {
        Pop-Location
    }

    Write-Host "SDK $SdkVersion Release build and tests passed. Diagnostics: $DiagnosticsRoot" -ForegroundColor Green
}
finally {
    if ($worktreeAdded) {
        $removeOutput = @(& git -C $repoRoot worktree remove --force $WorktreePath 2>&1)
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Could not remove SDK verification worktree '$WorktreePath': $($removeOutput -join ' ')"
        }
    }
}
