[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$OutputRoot = '',
    [switch]$SkipBuild,
    [switch]$IncludeRender
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repositoryRoot '.tmp\e01-behavior'
}
$outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)
New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null

$workerProject = Join-Path $repositoryRoot 'tests\GameSaveCenter.Worker.Tests\GameSaveCenter.Worker.Tests.csproj'
$playniteProject = Join-Path $repositoryRoot 'tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj'
$buildScript = Join-Path $repositoryRoot 'scripts\build.ps1'
$groups = @(
    [pscustomobject]@{ Name = 'business'; Project = $workerProject; Tests = @(
        'MediaSyncServiceTests', 'GameCatalogPersistenceTests', 'TaskQueryPersistenceTests',
        'CloudTransferStateTests', 'HealthInspectionServiceTests', 'RestoreReadinessTests',
        'RetentionQuarantineRecoveryTests', 'SqliteMediaMetadataTests') },
    [pscustomobject]@{ Name = 'ipc'; Project = $workerProject; Tests = @(
        'IpcMessageBoundaryTests', 'IpcRequestLedgerTests', 'TaskEventBroadcasterTests',
        'ExternalProcessRunnerTests') },
    [pscustomobject]@{ Name = 'wpf-sta'; Project = $playniteProject; Tests = @(
        'GamePickerFilterBehaviorTests', 'GamePickerViewModelTests', 'GameSelectionResolverTests',
        'SettingsSelectionStateSourceTests', 'TaskFilterOptionsSyncTests', 'ResponsiveLayoutCoordinatorTests',
        'TaskCenterViewResponsiveTests', 'WorkerIpcClientBehaviorTests') },
    [pscustomobject]@{ Name = 'fault-soak'; Project = $workerProject; Tests = @(
        'FaultInjectionTests', 'SoakStabilityTests', 'SoakDataScaleTests') }
)

if (-not $SkipBuild) {
    & $buildScript -Configuration $Configuration -OutputRoot $outputRootFull
    if ($LASTEXITCODE -ne 0) { throw "隔离 Release 构建失败：$LASTEXITCODE" }
}

$summary = [System.Collections.Generic.List[object]]::new()
$originalSoakIterations = $env:GSC_SOAK_ITERATIONS
$originalDataScale = $env:GSC_SOAK_DATA_SCALE
try {
    $env:GSC_SOAK_ITERATIONS = '20'
    $env:GSC_SOAK_DATA_SCALE = '0'
    foreach ($group in $groups) {
        $groupDirectory = Join-Path $outputRootFull $group.Name
        New-Item -ItemType Directory -Path $groupDirectory -Force | Out-Null
        foreach ($testClass in $group.Tests) {
            $logPath = Join-Path $groupDirectory ($testClass + '.txt')
            $filter = 'FullyQualifiedName~' + $testClass
            $arguments = @(
                'test', $group.Project, '-c', $Configuration, '--no-build', '--no-restore',
                '--filter', $filter, '-m:1', '-nodeReuse:false', '-p:NuGetAudit=false',
                '-p:MSBuildEnableWorkloadResolver=false', ('-p:GscBuildOutputRoot=' + $outputRootFull))
            $started = Get-Date
            $output = @(& dotnet @arguments 2>&1)
            $exitCode = $LASTEXITCODE
            $output | Out-File -LiteralPath $logPath -Encoding utf8
            $joined = $output -join [Environment]::NewLine
            $passed = 0
            $skipped = 0
            $total = 0
            if ($joined -match '通过:\s*(\d+).*跳过:\s*(\d+).*总计:\s*(\d+)') {
                $passed = [int]$Matches[1]
                $skipped = [int]$Matches[2]
                $total = [int]$Matches[3]
            } elseif ($joined -match 'Passed:\s*(\d+).*Skipped:\s*(\d+).*Total:\s*(\d+)') {
                $passed = [int]$Matches[1]
                $skipped = [int]$Matches[2]
                $total = [int]$Matches[3]
            }
            $summary.Add([pscustomobject]@{
                Category = $group.Name
                Test = $testClass
                ExitCode = $exitCode
                Passed = $passed
                Skipped = $skipped
                Total = $total
                DurationSeconds = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)
                Log = $logPath.Substring($outputRootFull.Length).TrimStart('\', '/')
            })
            if ($exitCode -ne 0) { throw "行为测试失败：$($group.Name)/$testClass，详见 $logPath" }
        }
    }
} finally {
    if ($null -eq $originalSoakIterations) { Remove-Item Env:GSC_SOAK_ITERATIONS -ErrorAction SilentlyContinue } else { $env:GSC_SOAK_ITERATIONS = $originalSoakIterations }
    if ($null -eq $originalDataScale) { Remove-Item Env:GSC_SOAK_DATA_SCALE -ErrorAction SilentlyContinue } else { $env:GSC_SOAK_DATA_SCALE = $originalDataScale }
}

$manual = [pscustomobject]@{
    Category = 'real-playnite'
    Status = 'MANUAL QA REQUIRED'
    Detail = '需在隔离 Playnite、独立数据目录和明确进程边界下验证真实 Named Pipe、Worker 中断恢复、双选择器操作、主题/DPI、睡眠唤醒和退出重启。'
}
$summary | ConvertTo-Json -Depth 4 | Out-File -LiteralPath (Join-Path $outputRootFull 'behavior-summary.json') -Encoding utf8
$manual | ConvertTo-Json -Depth 4 | Out-File -LiteralPath (Join-Path $outputRootFull 'manual-qa.json') -Encoding utf8
$reportLines = [System.Collections.Generic.List[string]]::new()
$reportLines.Add('# E01 行为证据矩阵')
$reportLines.Add('')
$reportLines.Add('| 分类 | 测试 | 通过 | 跳过 | 总数 | 退出码 |')
$reportLines.Add('|---|---|---:|---:|---:|---:|')
foreach ($item in $summary) {
    $reportLines.Add("| $($item.Category) | $($item.Test) | $($item.Passed) | $($item.Skipped) | $($item.Total) | $($item.ExitCode) |")
}
$reportLines.Add('')
$reportLines.Add('| 分类 | 状态 | 说明 |')
$reportLines.Add('|---|---|---|')
$reportLines.Add("| $($manual.Category) | $($manual.Status) | $($manual.Detail) |")
$reportLines | Out-File -LiteralPath (Join-Path $outputRootFull 'behavior-report.md') -Encoding utf8

if ($IncludeRender) {
    $renderOutput = Join-Path $outputRootFull 'render'
    & (Join-Path $repositoryRoot 'scripts\render-qa.ps1') -Configuration $Configuration -Output $renderOutput
    if ($LASTEXITCODE -ne 0) { throw "RenderHarness 验收失败：$LASTEXITCODE" }
}

Write-Host "E01 行为矩阵完成：$outputRootFull" -ForegroundColor Green
Write-Host "业务/IPC/WPF-STA/故障-Soak 已分组；真实 Playnite 保留 MANUAL QA REQUIRED。" -ForegroundColor DarkYellow
