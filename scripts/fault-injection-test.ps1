[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$outputRoot = Join-Path $root ("artifacts\dev-build\fault-injection\" + [Guid]::NewGuid().ToString('N'))
$workerTests = Join-Path $root 'tests\GameSaveCenter.Worker.Tests\GameSaveCenter.Worker.Tests.csproj'
$msbuild = @('-m:1', '-nodeReuse:false', '-p:NuGetAudit=false', ('-p:GscBuildOutputRoot=' + $outputRoot))

Write-Host "`n==> 故障注入测试" -ForegroundColor Cyan
Write-Host "隔离输出：$outputRoot" -ForegroundColor DarkCyan

Push-Location $root
try {
    & dotnet restore $workerTests @msbuild
    if ($LASTEXITCODE -ne 0) { throw "还原失败，dotnet 退出码：$LASTEXITCODE" }

    & dotnet build $workerTests -c $Configuration --no-restore @msbuild
    if ($LASTEXITCODE -ne 0) { throw "构建失败，dotnet 退出码：$LASTEXITCODE" }

    & dotnet test $workerTests -c $Configuration --no-build --filter 'FullyQualifiedName~FaultInjectionTests' @msbuild
    if ($LASTEXITCODE -ne 0) { throw "故障注入测试失败，dotnet 退出码：$LASTEXITCODE" }
}
finally {
    Pop-Location
}
