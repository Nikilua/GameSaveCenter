[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$OutputRoot = '',
    [string]$TestTempRoot = '',
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$DiagnosticsRoot = '',
    [hashtable]$DiagnosticsContext
)

$ErrorActionPreference = 'Stop'
$previousTemp = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
$previousTmp = [Environment]::GetEnvironmentVariable('TMP', 'Process')
. (Join-Path $PSScriptRoot 'build-diagnostics.ps1')
. (Join-Path $PSScriptRoot 'sdk-baseline.ps1')
$ownsDiagnostics = $null -eq $DiagnosticsContext
if ($ownsDiagnostics) {
    $DiagnosticsContext = New-GscDiagnosticContext $ProjectRoot $DiagnosticsRoot
    $DiagnosticsContext.RunType = 'playnite-tests'
    $DiagnosticsContext.TestsRequested = $true
}
$playniteSucceeded = $false

if ($OutputRoot) {
    $isolatedTestTempRoot = if ([string]::IsNullOrWhiteSpace($TestTempRoot)) {
        Join-Path ([System.IO.Path]::GetFullPath($OutputRoot)) 'test-temp'
    }
    else {
        [System.IO.Path]::GetFullPath($TestTempRoot)
    }
    New-Item -ItemType Directory -Path $isolatedTestTempRoot -Force | Out-Null
    $env:TEMP = $isolatedTestTempRoot
    $env:TMP = $isolatedTestTempRoot
}

function Invoke-PlayniteTestProcess {
    param(
        [Parameter(Mandatory = $true)][string]$Filter,
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$StepId
    )

    $testProject = Join-Path $ProjectRoot 'tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj'
    $arguments = @(
        'test', $testProject,
        '-c', $Configuration,
        '--no-build',
        '--no-restore',
        '--filter', $Filter,
        # Capture normal diagnostic detail so a failed isolated WPF class includes
        # its assertion message and stack when the buffered output is replayed below.
        # Every process also keeps a console log and TRX, including successful classes.
        '--logger', 'console;verbosity=normal',
        '--logger', ('trx;LogFileName=' + $StepId + '.trx'),
        '--results-directory', (Join-Path $DiagnosticsContext.RunRoot ('tests/' + $StepId)),
        '--blame-hang-timeout', '2m',
        '-m:1',
        '-nodeReuse:false',
        '-p:NuGetAudit=false',
        '-p:MSBuildEnableWorkloadResolver=false'
    )
    if ($OutputRoot) {
        $arguments += ('-p:GscBuildOutputRoot=' + [System.IO.Path]::GetFullPath($OutputRoot))
    }
    # Each group already owns a separate process. WPF TextServices COM cleanup can
    # fail when the adapter unloads an extra test AppDomain. Keep the process boundary
    # and run in its host domain; all selected cases and assertions still execute.
    $arguments += @('--', 'RunConfiguration.DisableAppDomain=true')

    Write-Host "    $Label" -ForegroundColor DarkCyan
    $isolatedTempRootForClass = ''
    $existingTempEntries = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    if (-not [string]::IsNullOrWhiteSpace($OutputRoot)) {
        $processTempRoot = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
        if (-not [string]::IsNullOrWhiteSpace($processTempRoot) -and
            (Test-Path -LiteralPath $processTempRoot -PathType Container)) {
            $isolatedTempRootForClass = [System.IO.Path]::GetFullPath($processTempRoot)
            foreach ($entry in Get-ChildItem -LiteralPath $isolatedTempRootForClass -Force) {
                [void]$existingTempEntries.Add([System.IO.Path]::GetFullPath($entry.FullName))
            }
        }
    }
    $result = Invoke-GscRecordedCommand $DiagnosticsContext $StepId dotnet $arguments -TimeoutSeconds 300
    $output = $result.Output
    $exitCode = $result.ExitCode
    Complete-GscTestDiagnostics $DiagnosticsContext (Join-Path $DiagnosticsContext.RunRoot ('tests/' + $StepId))
    if ($exitCode -ne 0) {
        $output | ForEach-Object { Write-Host $_ }
        throw "$Label failed; dotnet exit code: $exitCode"
    }

    # Adapters and fixtures may create temporary directories below TEMP. Remove only
    # entries created by this successful process; retain a failing class's evidence.
    if (-not [string]::IsNullOrWhiteSpace($isolatedTempRootForClass)) {
        $tempRootPrefix = $isolatedTempRootForClass.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
        $removedTempEntries = 0
        foreach ($entry in Get-ChildItem -LiteralPath $isolatedTempRootForClass -Force) {
            $entryPath = [System.IO.Path]::GetFullPath($entry.FullName)
            if (-not $entryPath.StartsWith($tempRootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Refusing to clear a test temp path outside the isolated root: $entryPath"
            }
            if (-not $existingTempEntries.Contains($entryPath)) {
                Remove-Item -LiteralPath $entryPath -Recurse -Force
                $removedTempEntries++
            }
        }
        if ($removedTempEntries -gt 0) {
            Write-Host "    cleaned $removedTempEntries isolated test temp entr$(if ($removedTempEntries -eq 1) { 'y' } else { 'ies' })" -ForegroundColor DarkCyan
        }
    }
}

Push-Location $ProjectRoot
try {
    if ($ownsDiagnostics) {
        $sdkInitialization = Initialize-GscSdkBaseline -ProjectRoot $ProjectRoot
        $requiredSdk = $sdkInitialization.RequiredVersion
        $sdk = Invoke-GscRecordedCommand $DiagnosticsContext 'sdk-version' dotnet @('--version')
        $selectedVersion = if ($sdk.ExitCode -eq 0) { ($sdk.Output -join '').Trim() } else { '' }
        Assert-GscSdkSelection $requiredSdk $selectedVersion $sdk.ExitCode @()
        $DiagnosticsContext.Sdk = $selectedVersion
        Set-GscDiagnosticAssemblies $DiagnosticsContext $OutputRoot $Configuration
    }
    $testProject = Join-Path $ProjectRoot 'tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj'
    $discoveryArguments = @(
        'test', $testProject,
        '-c', $Configuration,
        '--no-build',
        '--no-restore',
        '--list-tests',
        '-m:1',
        '-nodeReuse:false',
        '-p:NuGetAudit=false',
        '-p:MSBuildEnableWorkloadResolver=false'
    )
    if ($OutputRoot) {
        $discoveryArguments += ('-p:GscBuildOutputRoot=' + [System.IO.Path]::GetFullPath($OutputRoot))
    }
    # Discovery loads the same .NET Framework adapter as execution. Its extra
    # AppDomain can exceed MAX_PATH through shadow-copy paths in managed worktrees.
    # Use the same host-domain setting for both phases; keep the empty-list guard.
    $discoveryArguments += @('--', 'RunConfiguration.DisableAppDomain=true')

    $discoveryResult = Invoke-GscRecordedCommand $DiagnosticsContext 'playnite-discovery' dotnet $discoveryArguments
    $discovery = $discoveryResult.Output
    if ($discoveryResult.ExitCode -ne 0) {
        $discovery | ForEach-Object { Write-Host $_ }
        throw "Playnite test discovery failed; exit code: $($discoveryResult.ExitCode)"
    }

    $classes = @()
    foreach ($line in $discovery) {
        if (([string]$line) -match ('^\s+(GameSaveCenter\.Playnite\.Tests\.[^.]+)\.')) {
            $classes += $Matches[1]
        }
    }
    $classes = @($classes | Sort-Object -Unique)
    if ($classes.Count -eq 0) {
        throw 'Playnite test discovery returned no tests.'
    }

    $testFiles = @(Get-ChildItem (Join-Path $ProjectRoot 'tests\GameSaveCenter.Playnite.Tests') -Recurse -Filter '*.cs')
    $wpfMarkers = '(?m)^\s*using\s+System\.Windows|(?m)^\s*(?:[A-Za-z_][A-Za-z0-9_?]*\s*=\s*)?new\s+Window\s*\{|(?m)^\s*new\s+Application\s*\(|\bRunSta\s*\('
    $wpfClasses = @()
    $sourceClasses = @()
    foreach ($class in $classes) {
        $shortName = $class.Substring($class.LastIndexOf('.') + 1)
        $file = $testFiles | Where-Object BaseName -eq $shortName | Select-Object -First 1
        $isWpf = $null -eq $file -or (Get-Content -LiteralPath $file.FullName -Raw) -match $wpfMarkers
        if ($isWpf) { $wpfClasses += $class } else { $sourceClasses += $class }
    }

    Write-Host "Playnite test isolation: source classes $($sourceClasses.Count), WPF classes $($wpfClasses.Count)" -ForegroundColor DarkCyan
    if ($sourceClasses.Count -gt 0) {
        $sourceFilter = ($sourceClasses | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
        Invoke-PlayniteTestProcess -Filter $sourceFilter -Label 'source test group' -StepId 'playnite-source'
    }

    for ($index = 0; $index -lt $wpfClasses.Count; $index++) {
        $class = $wpfClasses[$index]
        Write-Host "  WPF isolated process [$($index + 1)/$($wpfClasses.Count)] $class" -ForegroundColor DarkCyan
        Invoke-PlayniteTestProcess -Filter ("FullyQualifiedName~$class") -Label "class $class" -StepId ('wpf.' + $class.Substring($class.LastIndexOf('.') + 1))
    }

    Write-Host 'All Playnite tests passed with WPF classes isolated by process.' -ForegroundColor Green
    $playniteSucceeded = $true
}
catch {
    $DiagnosticsContext.FailureMessage = ConvertTo-GscDiagnosticText $_.Exception.ToString() $DiagnosticsContext
    throw
}
finally {
    try {
        Complete-GscTestDiagnostics $DiagnosticsContext (Join-Path $DiagnosticsContext.RunRoot 'tests')
        if ($ownsDiagnostics) {
            Complete-GscDiagnostics $DiagnosticsContext $playniteSucceeded
            Write-Host "Diagnostics: $($DiagnosticsContext.RunRoot)" -ForegroundColor DarkCyan
        }
    }
    finally {
        [Environment]::SetEnvironmentVariable('TEMP', $previousTemp, 'Process')
        [Environment]::SetEnvironmentVariable('TMP', $previousTmp, 'Process')
        Pop-Location
    }
}
