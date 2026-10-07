[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$SkipTests,
    [string]$OutputRoot = '',
    [string]$TestTempRoot = '',
    [string]$DiagnosticsRoot = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$previousBuildCommit = [Environment]::GetEnvironmentVariable('GSC_BUILD_COMMIT', 'Process')
$previousSourceRoot = [Environment]::GetEnvironmentVariable('GSC_SOURCE_ROOT', 'Process')
$previousTemp = [Environment]::GetEnvironmentVariable('TEMP', 'Process')
$previousTmp = [Environment]::GetEnvironmentVariable('TMP', 'Process')
. (Join-Path $PSScriptRoot 'build-diagnostics.ps1')
$diagnostics = New-GscDiagnosticContext -RepoRoot $root -DiagnosticsRoot $DiagnosticsRoot
$diagnostics.RunType = 'solution-build'
$diagnostics.TestsRequested = -not $SkipTests
$succeeded = $false

function Get-CurrentBuildCommit {
    try {
        $commit = (& git -C $root rev-parse --verify HEAD 2>$null | Select-Object -First 1).ToString().Trim()
        if ($commit -match '^[0-9a-fA-F]{7,40}$') { return $commit }
    }
    catch { }
    return ''
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$StepName,
        [Parameter(Mandatory = $true)][string]$StepId
    )

    Write-Host "`n==> $StepName" -ForegroundColor Cyan
    $result = Invoke-GscRecordedCommand -Context $diagnostics -StepId $StepId -FilePath dotnet -Arguments $Arguments -EchoOutput
    $exitCode = $result.ExitCode
    if ($exitCode -ne 0) {
        throw "$StepName 失败，dotnet 退出码：$exitCode"
    }
}

Push-Location $root
try {
    # Keep ordinary builds and package builds on the same explicit identity input.
    # A non-Git build remains visibly unknown; package.ps1 rejects it before output.
    $env:GSC_BUILD_COMMIT = Get-CurrentBuildCommit
    $env:GSC_SOURCE_ROOT = $root
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        throw '未找到 dotnet。请安装 .NET 8 或更高版本的稳定版 SDK，并确认 dotnet 在 PATH 中。'
    }

    $sdkResult = Invoke-GscRecordedCommand $diagnostics 'sdk-list' dotnet @('--list-sdks')
    $sdkLines = $sdkResult.Output
    if ($sdkResult.ExitCode -ne 0) {
        throw "读取 .NET SDK 列表失败，退出码：$($sdkResult.ExitCode)"
    }
    $selectedSdk = Invoke-GscRecordedCommand $diagnostics 'sdk-version' dotnet @('--version')
    if ($selectedSdk.ExitCode -ne 0) { throw "选择 SDK 失败，退出码：$($selectedSdk.ExitCode)" }
    $diagnostics.Sdk = ($selectedSdk.Output -join '').Trim()
    Write-GscDiagnosticSummary $diagnostics

    $sdkVersions = @($sdkLines | ForEach-Object {
        if ($_ -match '^([0-9]+)\.([0-9]+)\.([0-9]+)') {
            [version]("{0}.{1}.{2}" -f $Matches[1], $Matches[2], $Matches[3])
        }
    })

    if (-not ($sdkVersions | Where-Object { $_.Major -ge 8 })) {
        $installed = if ($sdkLines) { $sdkLines -join [Environment]::NewLine } else { '未检测到任何 SDK' }
        throw "需要 .NET 8 或更高版本的稳定版 SDK。当前检测结果：`n$installed"
    }

    Write-Host '当前可用 SDK：' -ForegroundColor DarkCyan
    $sdkLines | ForEach-Object { Write-Host "  $_" }

    # This solution targets .NET Framework/WPF and does not consume SDK workloads.
    # Some machines only have a bare .NET SDK installation whose incomplete workload
    # resolver fails before MSBuild can evaluate the solution.
    $msbuildArguments = @('-m:1', '-nodeReuse:false', '-p:NuGetAudit=false', '-p:MSBuildEnableWorkloadResolver=false')
    if ($OutputRoot) {
        $isolatedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
        New-Item -ItemType Directory -Path $isolatedOutputRoot -Force | Out-Null
        # Keep each project's output separate. A single shared BaseOutputPath would make
        # net8 projects overwrite one another; MSBuildProjectName keeps references intact
        # while isolating this run from old testhost/Worker file locks in repository bin/.
        $msbuildArguments += @(
            ('-p:GscBuildOutputRoot=' + $isolatedOutputRoot)
        )
        Write-Host "隔离构建输出：$isolatedOutputRoot" -ForegroundColor DarkCyan

        # IntegrityCheckService deliberately reports low free space. Keep the
        # test fixture root on the same isolated volume as the build so a full
        # system TEMP drive cannot turn healthy fixture checks into warnings.
        $isolatedTestTempRoot = if ([string]::IsNullOrWhiteSpace($TestTempRoot)) {
            Join-Path $isolatedOutputRoot 'test-temp'
        }
        else {
            [System.IO.Path]::GetFullPath($TestTempRoot)
        }
        New-Item -ItemType Directory -Path $isolatedTestTempRoot -Force | Out-Null
        $env:TEMP = $isolatedTestTempRoot
        $env:TMP = $isolatedTestTempRoot
        Write-Host "测试临时目录：$isolatedTestTempRoot" -ForegroundColor DarkCyan
    }

    Write-Host "`n==> 检查 XAML 结构" -ForegroundColor Cyan
    & (Join-Path $PSScriptRoot 'check-xaml.ps1') -ProjectRoot $root

    Invoke-DotNet -StepId 'sdk-info' -StepName '显示当前 SDK 信息' -Arguments @('--info')
    Invoke-DotNet -StepId 'restore' -StepName '还原 NuGet 依赖' -Arguments (@('restore', '.\GameSaveCenter.sln') + $msbuildArguments)
    Invoke-DotNet -StepId 'build' -StepName "编译解决方案（$Configuration）" -Arguments (@('build', '.\GameSaveCenter.sln', '-c', $Configuration, '--no-restore') + $msbuildArguments)
    Set-GscDiagnosticAssemblies $diagnostics $OutputRoot $Configuration

    if (-not $SkipTests) {
        Invoke-DotNet -StepId 'core' -StepName '运行核心单元测试' -Arguments (@(
            'test',
            '.\tests\GameSaveCenter.Core.Tests\GameSaveCenter.Core.Tests.csproj',
            '-c', $Configuration,
            '--no-build', '--no-restore',
            '--logger', 'console;verbosity=normal', '--logger', 'trx;LogFileName=core.trx',
            '--results-directory', (Join-Path $diagnostics.RunRoot 'tests/core')
        ) + $msbuildArguments)
        Invoke-DotNet -StepId 'worker' -StepName '运行 Worker 集成测试' -Arguments (@(
            'test',
            '.\tests\GameSaveCenter.Worker.Tests\GameSaveCenter.Worker.Tests.csproj',
            '-c', $Configuration,
            '--no-build', '--no-restore',
            '--logger', 'console;verbosity=normal', '--logger', 'trx;LogFileName=worker.trx',
            '--results-directory', (Join-Path $diagnostics.RunRoot 'tests/worker')
        ) + $msbuildArguments)
        Write-Host "`n==> 运行 Playnite 测试（WPF 类隔离）" -ForegroundColor Cyan
        & (Join-Path $PSScriptRoot 'run-playnite-tests-isolated.ps1') `
            -Configuration $Configuration `
            -OutputRoot $OutputRoot `
            -TestTempRoot $TestTempRoot `
            -ProjectRoot $root `
            -DiagnosticsContext $diagnostics
    }

    $succeeded = $true
    if ($SkipTests) { Write-Host "`n构建成功；本次跳过测试。" -ForegroundColor Green }
    else { Write-Host "`n构建与测试全部成功。下一步可运行 scripts/package.ps1" -ForegroundColor Green }
}
catch {
    $diagnostics.FailureMessage = ConvertTo-GscDiagnosticText $_.Exception.ToString() $diagnostics
    throw
}
finally {
    try {
        Complete-GscTestDiagnostics $diagnostics (Join-Path $diagnostics.RunRoot 'tests')
        Complete-GscDiagnostics $diagnostics $succeeded
        Write-Host "诊断目录：$($diagnostics.RunRoot)" -ForegroundColor DarkCyan
    }
    finally {
        [Environment]::SetEnvironmentVariable('TEMP', $previousTemp, 'Process')
        [Environment]::SetEnvironmentVariable('TMP', $previousTmp, 'Process')
        [Environment]::SetEnvironmentVariable('GSC_BUILD_COMMIT', $previousBuildCommit, 'Process')
        [Environment]::SetEnvironmentVariable('GSC_SOURCE_ROOT', $previousSourceRoot, 'Process')
        Pop-Location
    }
}
