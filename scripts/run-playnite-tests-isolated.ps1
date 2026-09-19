[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$OutputRoot = '',
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$previousTemp = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
$previousTmp = [Environment]::GetEnvironmentVariable('TMP', 'Process')

if ($OutputRoot) {
    $isolatedTestTempRoot = Join-Path ([System.IO.Path]::GetFullPath($OutputRoot)) 'test-temp'
    New-Item -ItemType Directory -Path $isolatedTestTempRoot -Force | Out-Null
    $env:TEMP = $isolatedTestTempRoot
    $env:TMP = $isolatedTestTempRoot
}

function Invoke-PlayniteTestProcess {
    param(
        [Parameter(Mandatory = $true)][string]$Filter,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $testProject = Join-Path $ProjectRoot 'tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj'
    $arguments = @(
        'test', $testProject,
        '-c', $Configuration,
        '--no-build',
        '--no-restore',
        '--filter', $Filter,
        '--logger', 'console;verbosity=quiet',
        '-m:1',
        '-nodeReuse:false',
        '-p:NuGetAudit=false',
        '-p:MSBuildEnableWorkloadResolver=false'
    )
    if ($OutputRoot) {
        $arguments += ('-p:GscBuildOutputRoot=' + [System.IO.Path]::GetFullPath($OutputRoot))
    }

    Write-Host "    $Label" -ForegroundColor DarkCyan
    $output = @(& dotnet @arguments 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $output | ForEach-Object { Write-Host $_ }
        throw "$Label failed; dotnet exit code: $exitCode"
    }
}

Push-Location $ProjectRoot
try {
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

    $discovery = @(& dotnet @discoveryArguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        $discovery | ForEach-Object { Write-Host $_ }
        throw 'Playnite test discovery failed.'
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
        Invoke-PlayniteTestProcess -Filter $sourceFilter -Label 'source test group'
    }

    for ($index = 0; $index -lt $wpfClasses.Count; $index++) {
        $class = $wpfClasses[$index]
        Write-Host "  WPF isolated process [$($index + 1)/$($wpfClasses.Count)] $class" -ForegroundColor DarkCyan
        Invoke-PlayniteTestProcess -Filter ("FullyQualifiedName~$class") -Label "class $class"
    }

    Write-Host 'All Playnite tests passed with WPF classes isolated by process.' -ForegroundColor Green
}
finally {
    if ($null -eq $previousTemp) {
        Remove-Item Env:TEMP -ErrorAction SilentlyContinue
    }
    else {
        $env:TEMP = $previousTemp
    }
    if ($null -eq $previousTmp) {
        Remove-Item Env:TMP -ErrorAction SilentlyContinue
    }
    else {
        $env:TMP = $previousTmp
    }
    Pop-Location
}
