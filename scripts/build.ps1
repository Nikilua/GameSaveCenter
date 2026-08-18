[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$SkipTests,
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$StepName
    )

    Write-Host "`n==> $StepName" -ForegroundColor Cyan
    & dotnet @Arguments
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "$StepName 失败，dotnet 退出码：$exitCode"
    }
}

Push-Location $root
try {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        throw '未找到 dotnet。请安装 .NET 8 或更高版本的稳定版 SDK，并确认 dotnet 在 PATH 中。'
    }

    $sdkLines = & dotnet --list-sdks
    if ($LASTEXITCODE -ne 0) {
        throw "读取 .NET SDK 列表失败，退出码：$LASTEXITCODE"
    }

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
        $isolatedTestTempRoot = Join-Path $isolatedOutputRoot 'test-temp'
        New-Item -ItemType Directory -Path $isolatedTestTempRoot -Force | Out-Null
        $env:TEMP = $isolatedTestTempRoot
        $env:TMP = $isolatedTestTempRoot
        Write-Host "测试临时目录：$isolatedTestTempRoot" -ForegroundColor DarkCyan
    }

    Write-Host "`n==> 检查 XAML 结构" -ForegroundColor Cyan
    & (Join-Path $PSScriptRoot 'check-xaml.ps1') -ProjectRoot $root

    Invoke-DotNet -StepName '显示当前 SDK 信息' -Arguments @('--info')
    Invoke-DotNet -StepName '还原 NuGet 依赖' -Arguments (@('restore', '.\GameSaveCenter.sln') + $msbuildArguments)
    Invoke-DotNet -StepName "编译解决方案（$Configuration）" -Arguments (@('build', '.\GameSaveCenter.sln', '-c', $Configuration, '--no-restore') + $msbuildArguments)

    if (-not $SkipTests) {
        Invoke-DotNet -StepName '运行核心单元测试' -Arguments (@(
            'test',
            '.\tests\GameSaveCenter.Core.Tests\GameSaveCenter.Core.Tests.csproj',
            '-c', $Configuration,
            '--no-build'
        ) + $msbuildArguments)
        Invoke-DotNet -StepName '运行 Worker 集成测试' -Arguments (@(
            'test',
            '.\tests\GameSaveCenter.Worker.Tests\GameSaveCenter.Worker.Tests.csproj',
            '-c', $Configuration,
            '--no-build'
        ) + $msbuildArguments)
        Invoke-DotNet -StepName '运行 Playnite 设置迁移测试' -Arguments (@(
            'test',
            '.\tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj',
            '-c', $Configuration,
            '--no-build'
        ) + $msbuildArguments)
    }

    Write-Host "`n构建与测试全部成功。下一步可运行 scripts/package.ps1" -ForegroundColor Green
}
finally {
    Pop-Location
}
