# The SDK declaration belongs to global.json. Build tooling must fail closed
# instead of choosing a different compiler from the machine's installed SDKs.
function Get-GscSdkBaseline {
    param([Parameter(Mandatory = $true)][string]$ProjectRoot)
    $sdk = (Get-Content -LiteralPath (Join-Path $ProjectRoot 'global.json') -Raw | ConvertFrom-Json).sdk
    if ($sdk.version -notmatch '^\d+\.\d+\.\d+$' -or $sdk.rollForward -ne 'disable' -or $sdk.allowPrerelease -ne $false) {
        throw 'global.json must declare an exact stable SDK version, rollForward=disable and allowPrerelease=false.'
    }
    return [string]$sdk.version
}

function Get-GscSdkInstallDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Version,
        [string]$InstallDirectory = ''
    )

    if (-not [string]::IsNullOrWhiteSpace($InstallDirectory)) {
        return [System.IO.Path]::GetFullPath($InstallDirectory)
    }

    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    if ([string]::IsNullOrWhiteSpace($localAppData)) { $localAppData = $env:LOCALAPPDATA }
    if ([string]::IsNullOrWhiteSpace($localAppData)) {
        throw '无法确定当前用户的 LocalAppData 目录，不能安装所需的 .NET SDK。'
    }

    return Join-Path $localAppData (Join-Path 'GameSaveCenter' ("dotnet-sdk-$Version"))
}

function Set-GscSdkInstallEnvironment {
    param([Parameter(Mandatory = $true)][string]$InstallDirectory)

    $fullInstallDirectory = [System.IO.Path]::GetFullPath($InstallDirectory)
    $pathEntries = @($env:PATH -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if (-not ($pathEntries | Where-Object { [string]::Equals($_.TrimEnd('\'), $fullInstallDirectory.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase) })) {
        $env:PATH = $fullInstallDirectory + ';' + $env:PATH
    }
    $env:DOTNET_ROOT = $fullInstallDirectory
    $env:DOTNET_ROOT_X64 = $fullInstallDirectory
}

function Get-GscDotNetSdkSelection {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        return [pscustomobject]@{ ExitCode = 9009; Version = ''; Output = @('dotnet was not found on PATH.') }
    }

    # Windows PowerShell 5.1 can promote a native stderr line to a terminating
    # NativeCommandError when ErrorActionPreference is Stop. An SDK mismatch or
    # an incomplete dotnet host is an expected probe result here: let the
    # bootstrap path handle it instead of aborting before it can run.
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = @(& dotnet --version 2>&1 | ForEach-Object { $_.ToString() })
        $exitCode = $LASTEXITCODE
    }
    catch {
        $output = @($_.Exception.Message)
        $exitCode = if ($LASTEXITCODE) { $LASTEXITCODE } else { 9009 }
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $version = if ($exitCode -eq 0) { ($output -join '').Trim() } else { '' }
    return [pscustomobject]@{ ExitCode = $exitCode; Version = $version; Output = $output }
}

function Get-GscInstalledSdks {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) { return @() }

    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = @(& dotnet --list-sdks 2>&1 | ForEach-Object { $_.ToString() })
        $exitCode = $LASTEXITCODE
    }
    catch {
        return @()
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($exitCode -ne 0) { return @() }
    return $output
}

function Initialize-GscSdkBaseline {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$ProjectRoot,
        [string]$InstallDirectory = ''
    )

    $projectRootFull = [System.IO.Path]::GetFullPath($ProjectRoot)
    $requiredVersion = Get-GscSdkBaseline $projectRootFull
    $oldPath = $env:PATH
    $oldDotNetRoot = $env:DOTNET_ROOT
    $oldDotNetRootX64 = $env:DOTNET_ROOT_X64
    $succeeded = $false
    $bootstrapTemp = ''
    $selection = $null

    Push-Location $projectRootFull
    try {
        $selection = Get-GscDotNetSdkSelection
        if ($selection.ExitCode -eq 0 -and $selection.Version -eq $requiredVersion) {
            $selectedCommand = Get-Command dotnet
            Write-Host ".NET SDK $requiredVersion 已就绪：$($selectedCommand.Source)" -ForegroundColor DarkCyan
            $succeeded = $true
            return [pscustomobject]@{
                RequiredVersion = $requiredVersion
                SelectedVersion = $selection.Version
                InstallDirectory = (Split-Path -Parent $selectedCommand.Source)
                Bootstrapped = $false
            }
        }

        $installRoot = Get-GscSdkInstallDirectory -Version $requiredVersion -InstallDirectory $InstallDirectory
        $cachedDotNet = Join-Path $installRoot 'dotnet.exe'
        if (Test-Path -LiteralPath $cachedDotNet -PathType Leaf) {
            Set-GscSdkInstallEnvironment $installRoot
            $cachedSelection = Get-GscDotNetSdkSelection
            if ($cachedSelection.ExitCode -eq 0 -and $cachedSelection.Version -eq $requiredVersion) {
                Write-Host ".NET SDK $requiredVersion 已从当前用户缓存选择：$installRoot" -ForegroundColor DarkCyan
                $succeeded = $true
                return [pscustomobject]@{
                    RequiredVersion = $requiredVersion
                    SelectedVersion = $cachedSelection.Version
                    InstallDirectory = $installRoot
                    Bootstrapped = $false
                }
            }
        }

        Write-Host "缺少 global.json 锁定的 .NET SDK $requiredVersion，正在安装到当前用户目录：$installRoot" -ForegroundColor Cyan
        New-Item -ItemType Directory -Path $installRoot -Force | Out-Null
        $bootstrapTemp = Join-Path ([System.IO.Path]::GetTempPath()) ("gsc-sdk-bootstrap-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $bootstrapTemp -Force | Out-Null
        $installerScript = Join-Path $bootstrapTemp 'dotnet-install.ps1'

        $oldSecurityProtocol = [Net.ServicePointManager]::SecurityProtocol
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installerScript -UseBasicParsing -MaximumRedirection 10
        }
        catch {
            throw "下载 Microsoft 的 .NET SDK 安装脚本失败。请检查网络/代理，或手动安装 SDK $requiredVersion 后重试。下载源：https://dot.net/v1/dotnet-install.ps1。原因：$($_.Exception.Message)"
        }
        finally {
            [Net.ServicePointManager]::SecurityProtocol = $oldSecurityProtocol
        }

        $hostProcess = Get-Process -Id $PID -ErrorAction SilentlyContinue
        $powershellExecutable = if ($hostProcess -and $hostProcess.Path) { $hostProcess.Path } else { '' }
        if ([string]::IsNullOrWhiteSpace($powershellExecutable) -or -not (Test-Path -LiteralPath $powershellExecutable -PathType Leaf)) {
            $powershellCommand = Get-Command powershell.exe -ErrorAction SilentlyContinue
            if (-not $powershellCommand) { $powershellCommand = Get-Command pwsh.exe -ErrorAction SilentlyContinue }
            if ($powershellCommand) { $powershellExecutable = $powershellCommand.Source }
        }
        if ([string]::IsNullOrWhiteSpace($powershellExecutable)) {
            throw '找不到可用于运行 Microsoft SDK 安装脚本的 PowerShell 可执行文件。'
        }

        # Run the official script in a child process so its exit statement cannot
        # terminate this build/installer process.
        & $powershellExecutable -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $installerScript `
            -Version $requiredVersion `
            -InstallDir $installRoot `
            -Architecture x64 `
            -NoPath
        $installerExitCode = $LASTEXITCODE
        if ($installerExitCode -ne 0) {
            throw "Microsoft SDK installer exited with code $installerExitCode."
        }

        Set-GscSdkInstallEnvironment $installRoot
        $finalSelection = Get-GscDotNetSdkSelection
        if ($finalSelection.ExitCode -ne 0 -or $finalSelection.Version -ne $requiredVersion) {
            $installedSdks = Get-GscInstalledSdks
            $installedText = if ($installedSdks.Count) { $installedSdks -join [Environment]::NewLine } else { '(none)' }
            $actual = if ($finalSelection.Version) { $finalSelection.Version } else { 'unavailable' }
            throw "SDK 安装完成后仍未选择到准确版本 $requiredVersion；dotnet --version exit=$($finalSelection.ExitCode)，selected=$actual；已安装 SDK：`n$installedText"
        }

        Write-Host ".NET SDK $requiredVersion 安装并验证成功：$installRoot" -ForegroundColor Green
        $succeeded = $true
        return [pscustomobject]@{
            RequiredVersion = $requiredVersion
            SelectedVersion = $finalSelection.Version
            InstallDirectory = $installRoot
            Bootstrapped = $true
        }
    }
    catch {
        if ($selection -and ($selection.ExitCode -ne 0 -or $selection.Version -ne $requiredVersion)) {
            $installedSdks = Get-GscInstalledSdks
            $installedText = if ($installedSdks.Count) { $installedSdks -join [Environment]::NewLine } else { '(none)' }
            throw "无法准备 global.json 要求的 .NET SDK $requiredVersion。不会用其他 SDK 替代。当前可见 SDK：`n$installedText`n详细原因：$($_.Exception.Message)"
        }
        throw
    }
    finally {
        Pop-Location
        if (-not $succeeded) {
            $env:PATH = $oldPath
            if ($null -eq $oldDotNetRoot) { Remove-Item Env:DOTNET_ROOT -ErrorAction SilentlyContinue } else { $env:DOTNET_ROOT = $oldDotNetRoot }
            if ($null -eq $oldDotNetRootX64) { Remove-Item Env:DOTNET_ROOT_X64 -ErrorAction SilentlyContinue } else { $env:DOTNET_ROOT_X64 = $oldDotNetRootX64 }
        }
        if ($bootstrapTemp -and (Test-Path -LiteralPath $bootstrapTemp -PathType Container)) {
            Remove-Item -LiteralPath $bootstrapTemp -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

function Assert-GscSdkSelection {
    param([string]$RequiredVersion, [string]$SelectedVersion, [int]$SelectionExitCode, [string[]]$InstalledSdks)
    if ($SelectionExitCode -ne 0 -or $SelectedVersion -ne $RequiredVersion) {
        $actual = if ([string]::IsNullOrWhiteSpace($SelectedVersion)) { 'unavailable' } else { $SelectedVersion }
        $installed = if ($InstalledSdks.Count) { $InstalledSdks -join [Environment]::NewLine } else { '(none)' }
        throw "Required .NET SDK $RequiredVersion (global.json, exact match). Install that version; another SDK cannot substitute. dotnet --version exit=$SelectionExitCode; selected=$actual; installed:`n$installed"
    }
}
