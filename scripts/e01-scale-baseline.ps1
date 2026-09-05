[CmdletBinding()]
param(
    [ValidateSet('full', 'stress')][string]$Profile = 'full',
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repositoryRoot ('.tmp\e01-scale-baseline\' + $Profile)
}
$outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)
$buildRoot = Join-Path $outputRootFull 'build'
$testProject = Join-Path $repositoryRoot 'tests\GameSaveCenter.Worker.Tests\GameSaveCenter.Worker.Tests.csproj'
$msbuild = @(
    '-m:1',
    '-nodeReuse:false',
    '-p:NuGetAudit=false',
    '-p:MSBuildEnableWorkloadResolver=false',
    ('-p:GscBuildOutputRoot=' + $buildRoot)
)
$scaleValue = if ($Profile -eq 'stress') { '2' } else { '1' }
$originalScale = $env:GSC_SOAK_DATA_SCALE
$originalArtifactRoot = $env:GSC_TEST_ARTIFACT_ROOT

New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null
try {
    $env:GSC_SOAK_DATA_SCALE = $scaleValue
    $env:GSC_TEST_ARTIFACT_ROOT = $outputRootFull

    Write-Host "`n==> 还原规模基线测试依赖（$Profile）" -ForegroundColor Cyan
    & dotnet restore $testProject @msbuild
    if ($LASTEXITCODE -ne 0) { throw "规模基线还原失败：$LASTEXITCODE" }

    Write-Host "`n==> 编译规模基线测试（$Configuration）" -ForegroundColor Cyan
    & dotnet build $testProject -c $Configuration --no-restore @msbuild
    if ($LASTEXITCODE -ne 0) { throw "规模基线构建失败：$LASTEXITCODE" }

    Write-Host "`n==> 执行规模基线（$Profile）" -ForegroundColor Cyan
    $testArguments = @(
        'test', $testProject, '-c', $Configuration, '--no-build', '--no-restore',
        '--filter', 'FullyQualifiedName~SoakDataScaleTests', '-m:1', '-nodeReuse:false',
        '-p:NuGetAudit=false', '-p:MSBuildEnableWorkloadResolver=false',
        ('-p:GscBuildOutputRoot=' + $buildRoot), '--logger', 'console;verbosity=normal'
    )
    $testLog = Join-Path $outputRootFull 'test-output.txt'
    $testOutput = @(& dotnet @testArguments 2>&1)
    $testExitCode = $LASTEXITCODE
    $testOutput | Out-File -LiteralPath $testLog -Encoding utf8
    if ($testExitCode -ne 0) { throw "规模基线测试失败：$testExitCode，详见 $testLog" }

    $metricsPath = Join-Path $outputRootFull 'worker-scale.json'
    if (-not (Test-Path -LiteralPath $metricsPath)) {
        throw "规模基线没有生成结构化报告：$metricsPath"
    }
    $metrics = Get-Content -LiteralPath $metricsPath -Raw | ConvertFrom-Json
    $report = @(
        '# E01 规模性能基线',
        '',
        "- profile: $($metrics.profile)",
        "- games: $($metrics.games)",
        "- backups: $($metrics.backups)",
        "- tasks: $($metrics.tasks)",
        "- media: $($metrics.media)",
        "- tools: $($metrics.tools)",
        "- seed duration: $($metrics.seedDurationMilliseconds) ms",
        "- simulation duration: $($metrics.simulationDurationMilliseconds) ms",
        "- bounded growth: $($metrics.boundedGrowth)",
        "- subscriber residue: $($metrics.subscriberResidue)",
        "- temp residue: $($metrics.tempResidue)",
        "- growth: $($metrics.growthSummary)",
        '',
        '该报告只描述隔离 SQLite/内存夹具，不代表真实 Playnite 宿主帧率或用户目录性能。'
    )
    $report | Out-File -LiteralPath (Join-Path $outputRootFull 'baseline.md') -Encoding utf8
    Write-Host "E01 规模基线完成：$($metrics.profile)，games=$($metrics.games)，media=$($metrics.media)，tasks=$($metrics.tasks)" -ForegroundColor Green
}
finally {
    if ($null -eq $originalScale) { Remove-Item Env:GSC_SOAK_DATA_SCALE -ErrorAction SilentlyContinue } else { $env:GSC_SOAK_DATA_SCALE = $originalScale }
    if ($null -eq $originalArtifactRoot) { Remove-Item Env:GSC_TEST_ARTIFACT_ROOT -ErrorAction SilentlyContinue } else { $env:GSC_TEST_ARTIFACT_ROOT = $originalArtifactRoot }
}
