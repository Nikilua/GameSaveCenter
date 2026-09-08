[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [bool]$SelfContainedWorker = $true,
    [string]$Runtime = 'win-x64',
    [switch]$SkipBuild,
    [string]$BuildOutputRoot = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$previousBuildCommit = [Environment]::GetEnvironmentVariable('GSC_BUILD_COMMIT', 'Process')

function Read-AssemblyInformationalVersion {
    param([Parameter(Mandatory = $true)][string]$AssemblyPath)

    if (-not (Test-Path -LiteralPath $AssemblyPath -PathType Leaf)) {
        throw "找不到待校验程序集：$AssemblyPath"
    }

    # Read ECMA-335 metadata without loading the assembly into the packaging
    # process. This handles both net462 and net8 Worker binaries and avoids
    # resolving their runtime dependencies merely to inspect one attribute.
    # PowerShell 7 does not always probe the .NET SDK task assemblies when an
    # assembly is requested by name, so fall back to the SDK's net472 copy.
    if (-not ('System.Reflection.Metadata.MetadataReaderProvider' -as [type])) {
        $metadataLoaded = $false
        try {
            Add-Type -AssemblyName System.Reflection.Metadata -ErrorAction Stop
            $metadataLoaded = $true
        }
        catch {
            $metadataCandidates = @()
            $dotnetRoot = if ($env:DOTNET_ROOT) { $env:DOTNET_ROOT } else { Join-Path $env:ProgramFiles 'dotnet' }
            if (Test-Path -LiteralPath (Join-Path $dotnetRoot 'sdk')) {
                $metadataCandidates += Get-ChildItem -LiteralPath (Join-Path $dotnetRoot 'sdk') -Directory -ErrorAction SilentlyContinue |
                    Sort-Object Name -Descending |
                    ForEach-Object {
                        [pscustomobject]@{
                            Immutable = Join-Path $_.FullName 'TestHostNetFramework\System.Collections.Immutable.dll'
                            Metadata = Join-Path $_.FullName 'TestHostNetFramework\System.Reflection.Metadata.dll'
                        }
                    }
            }
            foreach ($candidate in $metadataCandidates | Where-Object { Test-Path -LiteralPath $_.Metadata }) {
                try {
                    # Load the matching immutable dependency first. Otherwise
                    # PowerShell may bind Metadata against a different SDK
                    # copy and fail while initializing its generic tables.
                    if (Test-Path -LiteralPath $candidate.Immutable) {
                        Add-Type -Path $candidate.Immutable -ErrorAction Stop
                    }
                    Add-Type -Path $candidate.Metadata -ErrorAction Stop
                    $metadataLoaded = $true
                    break
                }
                catch {
                    # Try the next installed SDK copy; no assembly is loaded
                    # from the target plugin or Worker output.
                }
            }
        }
        if (-not $metadataLoaded -or -not ('System.Reflection.Metadata.MetadataReaderProvider' -as [type])) {
            throw "当前 PowerShell 无法加载 System.Reflection.Metadata，无法安全校验程序集构建身份：$AssemblyPath"
        }
    }

    $stream = [System.IO.File]::OpenRead($AssemblyPath)
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    $provider = $null
    try {
        # Use PEReaderExtensions.GetMetadataReader instead of materializing an
        # ImmutableArray from GetMetadata().GetContent(). The latter can bind
        # to an incompatible System.Collections.Immutable copy in PowerShell
        # 7 while inspecting net462 assemblies.
        $reader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        $assembly = $reader.GetAssemblyDefinition()
        foreach ($attributeHandle in $assembly.GetCustomAttributes()) {
            $attribute = $reader.GetCustomAttribute($attributeHandle)
            $constructor = $attribute.Constructor
            $typeName = ''
            if ($constructor.Kind -eq [System.Reflection.Metadata.HandleKind]::MemberReference) {
                $member = $reader.GetMemberReference($constructor)
                $parent = $member.Parent
                if ($parent.Kind -eq [System.Reflection.Metadata.HandleKind]::TypeReference) {
                    $type = $reader.GetTypeReference($parent)
                    $typeName = $reader.GetString($type.Namespace) + '.' + $reader.GetString($type.Name)
                }
                elseif ($parent.Kind -eq [System.Reflection.Metadata.HandleKind]::TypeDefinition) {
                    $type = $reader.GetTypeDefinition($parent)
                    $typeName = $reader.GetString($type.Namespace) + '.' + $reader.GetString($type.Name)
                }
            }
            if ($typeName -eq 'System.Reflection.AssemblyInformationalVersionAttribute') {
                $blob = $reader.GetBlobReader($attribute.Value)
                if ($blob.ReadUInt16() -ne 1) { throw '程序集属性格式无效' }
                $value = $blob.ReadSerializedString()
                if ([string]::IsNullOrWhiteSpace($value)) { throw '程序集构建身份为空' }
                return $value.Trim()
            }
        }
        throw '程序集缺少 AssemblyInformationalVersionAttribute'
    }
    catch {
        throw "读取程序集构建身份失败：$AssemblyPath；$($_.Exception.Message)"
    }
    finally {
        if ($null -ne $provider) { $provider.Dispose() }
        $pe.Dispose()
        $stream.Dispose()
    }
}

function Assert-AssemblyIdentitySet {
    param(
        [Parameter(Mandatory = $true)][hashtable]$Assemblies,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][string]$ExpectedCommit
    )

    $expectedIdentity = "$ExpectedVersion+$ExpectedCommit"
    $identities = @{}
    foreach ($name in $Assemblies.Keys) {
        $identity = Read-AssemblyInformationalVersion $Assemblies[$name]
        $identities[$name] = $identity
        Write-Host "  $name 构建身份：$identity" -ForegroundColor DarkCyan
        if ($identity -ne $expectedIdentity) {
            throw "构建身份不一致：$name 实际为 $identity，期望为 $expectedIdentity。请用当前源码重新构建，不能用 SkipBuild 混用旧 DLL。"
        }
    }

    $distinct = @($identities.Values | Select-Object -Unique)
    if ($distinct.Count -ne 1) {
        throw "插件与 Worker 构建身份不一致：$($identities.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" } -join '; ')"
    }
    return $expectedIdentity
}

try {
$buildCommit = ''
try {
    # Never inherit a caller-provided identity when Git cannot prove the source.
    $env:GSC_BUILD_COMMIT = ''
    $buildCommit = (& git -C $root rev-parse --verify HEAD 2>$null | Select-Object -First 1).ToString().Trim()
}
catch {
    $buildCommit = ''
}
if ($buildCommit -match '^[0-9a-fA-F]{7,40}$') {
    # Directory.Build.props embeds this in AssemblyInformationalVersion for both
    # the plugin and the published Worker. It is diagnostic metadata only; the
    # public extension version remains controlled by extension.yaml/VersionPrefix.
    $env:GSC_BUILD_COMMIT = $buildCommit
}
else {
    throw '无法从当前 Git HEAD 获取有效提交号，已停止打包；不能生成 unknown 构建包。'
}
$dirtyFiles = @(& git -C $root status --porcelain --untracked-files=all 2>$null)
if ($LASTEXITCODE -ne 0) { throw '无法读取 Git 工作树状态，已停止打包。' }
if ($dirtyFiles.Count -gt 0) {
    throw "工作树存在未提交改动，已停止打包以避免用 HEAD 冒充实际源码：$($dirtyFiles -join '; ')"
}
$stage = Join-Path $artifacts 'GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec'
$workerStage = Join-Path $stage 'Worker'
$sourceManifest = Join-Path $root 'src\GameSaveCenter.Playnite\extension.yaml'
$sourceVersionLine = Get-Content $sourceManifest | Where-Object { $_ -match '^Version\s*:\s*(.+?)\s*$' } | Select-Object -First 1
if (-not $sourceVersionLine -or $sourceVersionLine -notmatch '^Version\s*:\s*(.+?)\s*$') {
    throw "无法从 $sourceManifest 读取源码扩展版本。"
}
$sourceVersion = $Matches[1].Trim()


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

function Assert-PackageContents {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][bool]$ExpectedSelfContained
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        # Compress-Archive writes backslashes on Windows. Normalize names so
        # this verification has the same result on Windows and PowerShell 7.
        $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        $requiredEntries = @(
            'extension.yaml',
            'GameSaveCenter.Playnite.dll',
            'GameSaveCenter.Contracts.dll',
            'GameSaveCenter.Core.dll',
            'Worker/GameSaveCenter.Worker.dll',
            'Worker/GameSaveCenter.Worker.runtimeconfig.json'
        )
        if ($ExpectedSelfContained) {
            $requiredEntries += @(
                'Worker/hostfxr.dll',
                'Worker/hostpolicy.dll',
                'Worker/coreclr.dll',
                'Worker/System.Private.CoreLib.dll'
            )
        }
        $missing = @($requiredEntries | Where-Object { $_ -notin $entries })
        if ($missing.Count -gt 0) {
            throw "安装包缺少必需文件：$($missing -join ', ')"
        }

        $manifestEntry = $archive.Entries | Where-Object { $_.FullName.Replace('\', '/') -eq 'extension.yaml' } | Select-Object -First 1
        $reader = [System.IO.StreamReader]::new($manifestEntry.Open())
        try {
            $manifestContent = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        if ($manifestContent -notmatch '(?m)^Version\s*:\s*(.+?)\s*$' -or $Matches[1].Trim() -ne $ExpectedVersion) {
            throw "安装包 extension.yaml 版本与预期不一致：预期 $ExpectedVersion。"
        }

        $runtimeConfigEntry = $archive.Entries |
            Where-Object { $_.FullName.Replace('\', '/') -eq 'Worker/GameSaveCenter.Worker.runtimeconfig.json' } |
            Select-Object -First 1
        $runtimeReader = [System.IO.StreamReader]::new($runtimeConfigEntry.Open())
        try {
            $runtimeConfigContent = $runtimeReader.ReadToEnd()
        }
        finally {
            $runtimeReader.Dispose()
        }

        if ($ExpectedSelfContained -and $runtimeConfigContent -notmatch '"includedFrameworks"') {
            throw 'Worker 安装包不是 self-contained 发布：runtimeconfig.json 缺少 includedFrameworks。'
        }
        if (-not $ExpectedSelfContained -and $runtimeConfigContent -match '"includedFrameworks"') {
            throw 'Worker 安装包标记为 framework-dependent，但 runtimeconfig.json 包含 includedFrameworks。'
        }
    }
    finally {
        $archive.Dispose()
    }
}

# 默认先完整构建；一键开发安装已单独完成构建时可显式跳过，避免重复编译。
if (-not $SkipBuild) {
    $buildArguments = @{ Configuration = $Configuration }
    if ($BuildOutputRoot) { $buildArguments.OutputRoot = $BuildOutputRoot }
    & (Join-Path $PSScriptRoot 'build.ps1') @buildArguments
}

# A normal solution restore does not necessarily contain the runtime-specific
# assets needed by a self-contained Worker publish. Restore this target here
# so package.ps1 remains reproducible after either build path.
$workerProject = Join-Path $root 'src\GameSaveCenter.Worker\GameSaveCenter.Worker.csproj'
$workerBuildProperties = @()
if ($BuildOutputRoot) {
    $workerBuildProperties = @('-p:GscBuildOutputRoot=' + [System.IO.Path]::GetFullPath($BuildOutputRoot))
}
$restoreArguments = @(
    'restore', $workerProject, '-r', $Runtime,
    "-p:RuntimeIdentifier=$Runtime",
    "-p:RuntimeIdentifiers=$Runtime"
)
if ($workerBuildProperties.Count -gt 0) {
    $restoreArguments += $workerBuildProperties
}
$restoreArguments += @(
    '-p:RestoreUseStaticGraphEvaluation=true',
    '-p:NuGetAudit=false',
    '-m:1',
    '-nodeReuse:false'
)
Invoke-DotNet -StepName "还原 Worker 发布运行时（$Runtime）" -Arguments $restoreArguments

Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
New-Item $workerStage -ItemType Directory -Force | Out-Null

$publishArgs = @(
    'publish',
    $workerProject,
    '-c', $Configuration,
    '-r', $Runtime,
    '-o', $workerStage,
    '--no-restore',
    '--self-contained', $(if ($SelfContainedWorker) { 'true' } else { 'false' }),
    "-p:RuntimeIdentifier=$Runtime",
    "-p:RuntimeIdentifiers=$Runtime"
)
if ($workerBuildProperties.Count -gt 0) {
    $publishArgs += $workerBuildProperties
}
$publishArgs += @(
    '-m:1',
    '-nodeReuse:false'
)
Invoke-DotNet -StepName "发布 Worker（$Runtime）" -Arguments $publishArgs

$pluginOutput = if ($BuildOutputRoot) {
    Join-Path (Join-Path (Join-Path (Join-Path ([System.IO.Path]::GetFullPath($BuildOutputRoot)) 'bin') 'GameSaveCenter.Playnite') $Configuration) 'net462'
}
else {
    Join-Path $root "src\GameSaveCenter.Playnite\bin\$Configuration\net462"
}
$pluginDllPath = Join-Path $pluginOutput 'GameSaveCenter.Playnite.dll'
if (-not (Test-Path $pluginDllPath)) {
    throw "找不到已编译插件：$pluginDllPath"
}
$pluginFileVersion = (Get-Item $pluginDllPath).VersionInfo.FileVersion
if ($pluginFileVersion -and -not $pluginFileVersion.StartsWith("$sourceVersion.")) {
    throw "已编译 DLL 版本不一致：源码为 $sourceVersion，DLL 为 $pluginFileVersion。请删除 bin/obj 后重新构建。"
}
$workerDllPath = Join-Path $workerStage 'GameSaveCenter.Worker.dll'
$identityPaths = @{
    Plugin = $pluginDllPath
    Worker = $workerDllPath
    PluginContracts = (Join-Path $pluginOutput 'GameSaveCenter.Contracts.dll')
    PluginCore = (Join-Path $pluginOutput 'GameSaveCenter.Core.dll')
    WorkerContracts = (Join-Path $workerStage 'GameSaveCenter.Contracts.dll')
    WorkerCore = (Join-Path $workerStage 'GameSaveCenter.Core.dll')
}
foreach ($name in @('PluginContracts','PluginCore','WorkerContracts','WorkerCore')) {
    if (-not (Test-Path -LiteralPath $identityPaths[$name])) {
        throw "缺少用于构建身份同源校验的程序集：$name -> $($identityPaths[$name])"
    }
}
Write-Host "`n==> 校验插件、Worker 与共享程序集构建身份" -ForegroundColor Cyan
$packageIdentity = Assert-AssemblyIdentitySet -Assemblies $identityPaths -ExpectedVersion $sourceVersion -ExpectedCommit $buildCommit
$required = @(
    'GameSaveCenter.Playnite.dll',
    'GameSaveCenter.Contracts.dll',
    'GameSaveCenter.Core.dll',
    'Newtonsoft.Json.dll',
    'extension.yaml',
    'icon.png'
)

# These assemblies were previously emitted beside the .NET Framework plugin
# when the old WPF-UI dependency was present. They are not plugin-level
# dependencies in the native-WPF build, and some are already supplied by the
# target framework. Keep them optional so packaging follows the actual build
# output instead of failing on stale dependency assumptions. Worker runtime
# dependencies remain under the published Worker directory.
$optionalCompatibilityDependencies = @(
    'System.Memory.dll',
    'System.Buffers.dll',
    'System.Runtime.CompilerServices.Unsafe.dll',
    'System.ValueTuple.dll'
)

foreach ($file in $required) {
    # extension.yaml is source-of-truth.  The copy emitted in bin/ can be
    # stale when a previous build was interrupted or when packaging is run
    # with SkipBuild; never let that stale file overwrite the current version.
    if ($file -eq 'extension.yaml') {
        $source = $sourceManifest
    }
    else {
        $source = Join-Path $pluginOutput $file
        if (-not (Test-Path $source)) {
            $source = Join-Path $root "src\GameSaveCenter.Playnite\$file"
        }
    }
    if (-not (Test-Path $source)) {
        throw "打包缺少文件：$file。请检查前面的编译输出，不能跳过构建错误继续打包。"
    }
    Copy-Item $source $stage -Force
}

foreach ($file in $optionalCompatibilityDependencies) {
    $source = Join-Path $pluginOutput $file
    if (Test-Path $source) {
        Copy-Item $source $stage -Force
    }
}


$manifestPath = Join-Path $stage 'extension.yaml'
$versionLine = Get-Content $manifestPath | Where-Object { $_ -match '^Version\s*:\s*(.+?)\s*$' } | Select-Object -First 1
if (-not $versionLine -or $versionLine -notmatch '^Version\s*:\s*(.+?)\s*$') {
    throw "无法从 $manifestPath 读取扩展版本。"
}
$packageVersion = $Matches[1].Trim()
if ($packageVersion -ne $sourceVersion) {
    throw "打包版本不一致：源码 extension.yaml 为 $sourceVersion，打包目录为 $packageVersion。请先清理并重新构建。"
}
$zip = Join-Path $artifacts "GameSaveCenter-$packageVersion-playnite.zip"
$pext = Join-Path $artifacts "GameSaveCenter-$packageVersion.pext"
Get-ChildItem $artifacts -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like 'GameSaveCenter-*-playnite.zip' -or $_.Name -like 'GameSaveCenter-*.pext' } |
    Remove-Item -Force -ErrorAction SilentlyContinue
Remove-Item $zip,$pext -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
Copy-Item $zip $pext
Assert-PackageContents -PackagePath $pext -ExpectedVersion $packageVersion -ExpectedSelfContained $SelfContainedWorker

Write-Host "`n打包成功：$zip" -ForegroundColor Green
Write-Host "Playnite 安装包：$pext" -ForegroundColor Green
Write-Host '若当前 Playnite 拒绝直接安装 .pext，请使用 scripts/install-dev.ps1。' -ForegroundColor Yellow
}
finally {
    if ($null -eq $previousBuildCommit) {
        Remove-Item Env:GSC_BUILD_COMMIT -ErrorAction SilentlyContinue
    }
    else {
        $env:GSC_BUILD_COMMIT = $previousBuildCommit
    }
}
