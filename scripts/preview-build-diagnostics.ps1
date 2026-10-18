[CmdletBinding()]
param(
    [string]$Path = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/build-diagnostics'),
    [switch]$DefineOnly
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'build-diagnostics.ps1')

function Resolve-GscDiagnosticSummaryPath {
    param([Parameter(Mandatory = $true)][string]$InputPath)

    $fullPath = [IO.Path]::GetFullPath($InputPath)
    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        if ([IO.Path]::GetFileName($fullPath) -ne 'summary.json') {
            throw "Expected summary.json, got: $fullPath"
        }
        return $fullPath
    }
    if (-not (Test-Path -LiteralPath $fullPath -PathType Container)) {
        throw "Diagnostics path does not exist: $fullPath"
    }

    $summaries = @(Get-ChildItem -LiteralPath $fullPath -Filter 'summary.json' -Recurse -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending)
    if ($summaries.Count -eq 0) {
        throw "No summary.json found under $fullPath. Point to an existing diagnostics run; this preview does not build, collect data, or use the network."
    }
    return $summaries[0].FullName
}

function Get-GscPreviewProperty {
    param([object]$Object, [string]$Name, [object]$DefaultValue = $null)

    if ($null -ne $Object -and $null -ne $Object.PSObject.Properties[$Name]) {
        return $Object.PSObject.Properties[$Name].Value
    }
    return $DefaultValue
}

function Get-GscDiagnosticPreviewLines {
    param(
        [Parameter(Mandatory = $true)][object]$Summary,
        [Parameter(Mandatory = $true)][string]$SummaryPath
    )

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('GameSaveCenter 构建诊断预览（只读；不访问网络或用户存档）')
    $lines.Add("摘要文件：$([IO.Path]::GetFileName($SummaryPath))")
    $lines.Add("源码提交：$(Get-GscPreviewProperty $Summary 'sourceCommit' '未记录')")
    $clean = Get-GscPreviewProperty $Summary 'workingTreeClean'
    if ($null -eq $clean) {
        $lines.Add('工作树：旧格式摘要未记录')
    }
    elseif ($clean) {
        $lines.Add('工作树：干净')
    }
    else {
        $dirtyPaths = @(Get-GscPreviewProperty $Summary 'dirtyPaths' @())
        $lines.Add("工作树：有 $($dirtyPaths.Count) 条改动路径")
        foreach ($dirtyPath in $dirtyPaths) { $lines.Add("  改动：$dirtyPath") }
    }

    $environment = Get-GscPreviewProperty $Summary 'environment'
    if ($null -eq $environment) {
        $lines.Add('运行环境：旧格式摘要没有 OS/PowerShell/位数记录；不能从当前机器状态补推历史环境。')
    }
    else {
        $osVersion = Get-GscPreviewProperty $environment 'osVersion' '未记录'
        $osBitness = Get-GscPreviewProperty $environment 'osBitness' '未记录'
        $processBitness = Get-GscPreviewProperty $environment 'processBitness' '未记录'
        $powershell = Get-GscPreviewProperty $environment 'powershellVersion' '未记录'
        $edition = Get-GscPreviewProperty $environment 'powershellEdition' '未记录'
        $runtime = Get-GscPreviewProperty $environment 'dotnetRuntimeVersion' '未记录'
        $lines.Add("运行环境：Windows $osVersion / OS ${osBitness}-bit / 进程 ${processBitness}-bit / PowerShell $powershell ($edition) / .NET $runtime")
    }
    $lines.Add("SDK：$(Get-GscPreviewProperty $Summary 'sdk' '未记录')")
    $lines.Add("运行结果：$(Get-GscPreviewProperty $Summary 'outcome' '未记录')；类型：$(Get-GscPreviewProperty $Summary 'runType' '未记录')")

    $steps = @(Get-GscPreviewProperty $Summary 'steps' @())
    $failedSteps = @($steps | Where-Object { $null -ne $_.exitCode -and $_.exitCode -ne 0 })
    if ($failedSteps.Count -eq 0) {
        $lines.Add("诊断步骤：$($steps.Count) 项；未记录非零退出码")
    }
    else {
        $failedIds = @($failedSteps | ForEach-Object { "$($_.id)=$($_.exitCode)" }) -join ', '
        $lines.Add("诊断步骤：$($steps.Count) 项；非零步骤：$failedIds")
    }

    $assemblies = @(Get-GscPreviewProperty $Summary 'assemblies' @())
    if ($assemblies.Count -eq 0) {
        $lines.Add('程序集/PDB：没有身份记录；构建可能未到达程序集扫描阶段。')
    }
    else {
        $lines.Add("程序集/PDB：$($assemblies.Count) 个程序集记录")
        foreach ($assembly in $assemblies) {
            $lines.Add("  $($assembly.file)：DLL SHA-256=$($assembly.sha256)；MVID=$(Get-GscPreviewProperty $assembly 'mvid' '未记录')")
            $pdb = Get-GscPreviewProperty $assembly 'pdb'
            if ($null -eq $pdb) {
                $lines.Add('    PDB：未记录或同目录 PDB 不存在')
            }
            elseif (Get-GscPreviewProperty $pdb 'sha256') {
                $lines.Add("    PDB SHA-256=$(Get-GscPreviewProperty $pdb 'sha256')")
            }
            elseif (Get-GscPreviewProperty $pdb 'unavailableReason') {
                $lines.Add("    PDB：读取失败 ($(Get-GscPreviewProperty $pdb 'unavailableReason'))")
            }
            else {
                $lines.Add('    PDB：没有可用身份')
            }
        }
    }

    $testSummary = Get-GscPreviewProperty $Summary 'testSummary'
    $recomputedFromRawTrx = $false
    $testsRequested = Get-GscPreviewProperty $Summary 'testsRequested'
    if ($null -eq $testSummary -and $testsRequested -ne $false) {
        $diagnosticsRoot = Split-Path -Parent $SummaryPath
        $rawTestsRoot = Join-Path $diagnosticsRoot 'tests'
        if (Test-Path -LiteralPath $rawTestsRoot -PathType Container) {
            $recount = Get-GscTestSummary @{ RunRoot = $diagnosticsRoot }
            if ($recount.trxFileCount -gt 0) {
                $testSummary = $recount
                $recomputedFromRawTrx = $true
            }
        }
    }
    if ($null -eq $testSummary) {
        if ($testsRequested -eq $true) {
            $lines.Add('测试/TRX：摘要未记录；检查原始步骤和 diagnostics artifact，不能据此推断测试通过。')
        }
        else {
            $lines.Add('测试/TRX：本次没有请求测试，未生成测试汇总。')
        }
    }
    else {
        if ($recomputedFromRawTrx) {
            $lines.Add('TRX 汇总未保存在旧 summary.json；以下计数由本机现存原始 TRX 只读复算。')
        }
        $totals = $testSummary.totals
        $lines.Add("TRX 测试用例：$($totals.testCaseCount) total / $($totals.passed) passed / $($totals.failed) failed / $($totals.skipped) skipped；TRX 文件 $($testSummary.trxFileCount)")
        $lines.Add("TRX 计数核对：$($totals.counterMismatchCount) 个 Counter 不匹配；$($totals.parseErrorCount) 个解析失败")
        foreach ($group in @($testSummary.groups)) {
            $lines.Add("  $($group.id)：$($group.testCaseCount) cases / $($group.distinctTestClassCount) classes / $($group.wpfIsolatedClassRunCount) WPF isolated class runs")
        }
        $testsRoot = Join-Path (Split-Path -Parent $SummaryPath) 'tests'
        foreach ($file in @($testSummary.files)) {
            $relativePath = [string]$file.path
            $rawPath = Join-Path $testsRoot ($relativePath.Replace('/', [IO.Path]::DirectorySeparatorChar))
            $presence = if (Test-Path -LiteralPath $rawPath -PathType Leaf) { 'available' } else { 'missing from this folder/artifact' }
            $lines.Add("  TRX $presence：$relativePath")
        }
        if ($testSummary.trxFileCount -eq 0) {
            $lines.Add('  没有找到原始 TRX；查看 testsRequested 与失败步骤以判断测试是否尚未启动。')
        }
    }

    $lines.Add('安装包身份不由该诊断摘要采集；若需核对安装目录，请另提供包哈希和实际加载路径。')
    return $lines.ToArray()
}

if (-not $DefineOnly) {
    try {
        $summaryPath = Resolve-GscDiagnosticSummaryPath $Path
        $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
        Get-GscDiagnosticPreviewLines -Summary $summary -SummaryPath $summaryPath | ForEach-Object { Write-Output $_ }
    }
    catch {
        Write-Error $_.Exception.Message
        exit 1
    }
}
