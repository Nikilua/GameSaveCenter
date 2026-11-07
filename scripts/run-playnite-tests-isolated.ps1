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
. (Join-Path $PSScriptRoot 'windows-desktop-process.ps1')
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
        [Parameter(Mandatory = $true)][string]$StepId,
        [switch]$HiddenDesktop
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
    if ($HiddenDesktop) {
        $windowStation = [GscHiddenDesktopProcess]::CurrentWindowStation()
        if (-not [string]::Equals($windowStation, 'WinSta0', [StringComparison]::OrdinalIgnoreCase)) {
            # Hosted CI commonly runs in a noninteractive service station. WPF
            # windows there cannot appear on the developer's input desktop; keep
            # the complete test run and diagnostics without trying to access WinSta0.
            Write-Host "    noninteractive station '$windowStation'; running the complete WPF class without a user-visible desktop" -ForegroundColor DarkCyan
            $result = Invoke-GscRecordedCommand $DiagnosticsContext $StepId dotnet $arguments -TimeoutSeconds 300
        }
        else {
            $inputDesktopBefore = [GscHiddenDesktopProcess]::InputDesktop()
            $desktop = 'GscWpfTest_' + [Guid]::NewGuid().ToString('N')
            $testRunner = Join-Path $PSScriptRoot 'run-playnite-wpf-test-child.ps1'
            $testDirectory = Join-Path $DiagnosticsContext.RunRoot ('tests/' + $StepId)
            New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
            $consolePath = Join-Path $DiagnosticsContext.RunRoot ($StepId + '-console.txt')
            $resultPath = Join-Path $testDirectory ($StepId + '-desktop-result.json')
            $specificationPath = Join-Path $testDirectory ($StepId + '-desktop-specification.json')
            $specification = [ordered]@{
                FilePath = 'dotnet'
                Arguments = @($arguments)
                WorkingDirectory = [System.IO.Path]::GetFullPath($ProjectRoot)
                RepositoryRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
                ConsolePath = $consolePath
                ResultPath = $resultPath
                ExpectedDesktop = $desktop
                TimeoutSeconds = 300
            }
            [IO.File]::WriteAllText($specificationPath, (ConvertTo-Json -InputObject $specification -Depth 5), [Text.UTF8Encoding]::new($false))

            $powershell = Join-Path $env:SystemRoot 'System32/WindowsPowerShell/v1.0/powershell.exe'
            $hiddenArguments = @('-NoLogo', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $testRunner, '-SpecificationPath', $specificationPath)
            $desktopResult = Invoke-GscHiddenDesktopProcess -Application $powershell -Arguments $hiddenArguments -WorkingDirectory ([System.IO.Path]::GetFullPath($ProjectRoot)) -TimeoutSeconds 360 -DesktopName $desktop
            if ($desktopResult.TimedOut) {
                $ownedProcess = $null
                try {
                    $ownedProcess = [Diagnostics.Process]::GetProcessById([int]$desktopResult.ProcessId)
                    Stop-GscOwnedProcessTree $ownedProcess
                }
                finally {
                    if ($ownedProcess) { $ownedProcess.Dispose() }
                }
                throw "WPF hidden desktop runner exceeded 360 seconds for $Label."
            }
            if (-not $desktopResult.Exited) {
                throw "WPF hidden desktop runner did not exit for $Label."
            }
            if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf)) {
                throw "WPF hidden desktop runner did not produce its result record for $Label."
            }

            $childResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
            $inputDesktopAfter = [GscHiddenDesktopProcess]::InputDesktop()
            if ($desktopResult.InputDesktopBefore -ne $inputDesktopBefore -or
                $desktopResult.InputDesktopAfter -ne $inputDesktopBefore -or
                $inputDesktopAfter -ne $inputDesktopBefore -or
                $childResult.windowStation -ne 'WinSta0' -or
                $childResult.desktop -ne $desktop -or
                $childResult.inputDesktopBefore -ne $inputDesktopBefore -or
                $childResult.inputDesktopAfter -ne $inputDesktopBefore -or
                [int]$desktopResult.ExitCode -ne [int]$childResult.exitCode) {
                throw "WPF hidden desktop identity or input desktop changed during $Label."
            }

            $processEvidence = [ordered]@{
                ExitCode = [int]$childResult.exitCode
                TimedOut = [bool]$childResult.timedOut
                OutputDrained = [bool]$childResult.outputDrained
                ProcessId = $childResult.testProcessId
                ProcessStartedUtc = $childResult.testProcessStartedUtc
                ProcessExited = [bool]$childResult.testProcessExited
                RunnerProcessId = [int]$desktopResult.ProcessId
                RunnerProcessStartedUtc = [string]$desktopResult.ProcessStartedUtc
                RunnerProcessExited = [bool]$desktopResult.Exited
                WindowStation = [string]$childResult.windowStation
                Desktop = [string]$childResult.desktop
                InputDesktopBefore = [string]$inputDesktopBefore
                InputDesktopAfter = [string]$inputDesktopAfter
            }
            $DiagnosticsContext.Steps.Add([ordered]@{
                id = $StepId
                command = 'dotnet'
                arguments = $arguments
                startedUtc = [string]$childResult.startedUtc
                completedUtc = [string]$childResult.completedUtc
                exitCode = [int]$childResult.exitCode
                console = [IO.Path]::GetFileName($consolePath)
                launchError = $childResult.launchError
                process = $processEvidence
            })
            if ([int]$childResult.exitCode -ne 0 -or $childResult.launchError) { $DiagnosticsContext.FailedStageId = $StepId }
            Write-GscDiagnosticSummary $DiagnosticsContext
            $outputText = if (Test-Path -LiteralPath $consolePath -PathType Leaf) { Get-Content -LiteralPath $consolePath -Raw } else { '' }
            $result = [pscustomobject]@{ ExitCode = [int]$childResult.exitCode; Output = @($outputText -split '\r?\n') }
        }
    }
    else {
        $result = Invoke-GscRecordedCommand $DiagnosticsContext $StepId dotnet $arguments -TimeoutSeconds 300
    }
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
        Invoke-PlayniteTestProcess -Filter ("FullyQualifiedName~$class") -Label "class $class" -StepId ('wpf.' + $class.Substring($class.LastIndexOf('.') + 1)) -HiddenDesktop
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
