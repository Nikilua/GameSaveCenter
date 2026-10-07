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

function Assert-GscSdkSelection {
    param([string]$RequiredVersion, [string]$SelectedVersion, [int]$SelectionExitCode, [string[]]$InstalledSdks)
    if ($SelectionExitCode -ne 0 -or $SelectedVersion -ne $RequiredVersion) {
        $actual = if ([string]::IsNullOrWhiteSpace($SelectedVersion)) { 'unavailable' } else { $SelectedVersion }
        $installed = if ($InstalledSdks.Count) { $InstalledSdks -join [Environment]::NewLine } else { '(none)' }
        throw "Required .NET SDK $RequiredVersion (global.json, exact match). Install that version; another SDK cannot substitute. dotnet --version exit=$SelectionExitCode; selected=$actual; installed:`n$installed"
    }
}
