[CmdletBinding()]
param([string]$OutputRoot = (Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) '.tmp/sdk-baseline-tests'))
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repoRoot 'scripts/build-diagnostics.ps1')
. (Join-Path $repoRoot 'scripts/sdk-baseline.ps1')
$context = New-GscDiagnosticContext $repoRoot $OutputRoot
$context.RunType = 'sdk-baseline-tests'
$context.TestsRequested = $true
$succeeded = $false
try {
    Push-Location $repoRoot
    try {
        $required = Get-GscSdkBaseline $repoRoot
        $installed = Invoke-GscRecordedCommand $context 'sdk-list' dotnet @('--list-sdks')
        if ($installed.ExitCode -ne 0) { throw 'SDK inventory failed' }
        $actual = Invoke-GscRecordedCommand $context 'sdk-baseline' dotnet @('--version')
        $selected = ($actual.Output -join '').Trim()
        Assert-GscSdkSelection $required $selected $actual.ExitCode $installed.Output
        $context.Sdk = $selected
        # Another installed major must never be accepted by the build preflight.
        $mismatchError = $null
        try { Assert-GscSdkSelection $required '10.0.401' 0 @('10.0.401 [fixture]') }
        catch { $mismatchError = $_.Exception.Message }
        if (-not $mismatchError -or -not $mismatchError.Contains($required)) { throw 'Different-major SDK was accepted' }
        $missingRoot = Join-Path $context.RunRoot 'missing-sdk'
        New-Item -ItemType Directory -Path $missingRoot -Force | Out-Null
        # An absent older SDK proves that an installed newer major is not used.
        # An imaginary future SDK would fail even under the old latestMajor policy.
        $missingVersion = @('1.0.100','2.0.100','3.0.100','6.0.100','7.0.100','8.0.100') |
            Where-Object { [version]$_ -lt [version]$required -and -not ($installed.Output -match ('^' + [regex]::Escape($_) + '\s')) } |
            Select-Object -First 1
        if (-not $missingVersion) { throw 'No absent older SDK available for the resolver negative fixture' }
        $missingConfig = @{ sdk = @{ version = $missingVersion; rollForward = 'disable'; allowPrerelease = $false } } | ConvertTo-Json
        [IO.File]::WriteAllText((Join-Path $missingRoot 'global.json'), $missingConfig, [Text.UTF8Encoding]::new($false))
        Push-Location $missingRoot
        try {
            $missingVersion = Get-GscSdkBaseline $missingRoot
            $missing = Invoke-GscRecordedCommand $context 'missing-sdk' dotnet @('--version')
            if ($missing.ExitCode -eq 0) { throw 'Native resolver substituted an installed SDK for a missing exact version' }
            $missingError = $null
            try { Assert-GscSdkSelection $missingVersion '' $missing.ExitCode $installed.Output }
            catch { $missingError = $_.Exception.Message }
            if (-not $missingError -or -not $missingError.Contains($missingVersion) -or -not $missingError.Contains('Install')) { throw 'Missing SDK diagnostic is not actionable' }
            [IO.File]::WriteAllText((Join-Path $context.RunRoot 'preflight-errors.txt'), (ConvertTo-GscDiagnosticText ($mismatchError + "`n" + $missingError) $context))
            # Characterize the old policy in this disposable directory only:
            # the same installed host now substitutes a newer SDK successfully.
            $legacyConfig = @{ sdk = @{ version = $missingVersion; rollForward = 'latestMajor'; allowPrerelease = $false } } | ConvertTo-Json
            [IO.File]::WriteAllText((Join-Path $missingRoot 'global.json'), $legacyConfig, [Text.UTF8Encoding]::new($false))
            $legacy = Invoke-GscRecordedCommand $context 'legacy-rollforward' dotnet @('--version')
            if ($legacy.ExitCode -ne 0 -or [version](($legacy.Output -join '').Trim()) -lt [version]$required) { throw 'Legacy resolver control did not substitute a newer SDK' }
            $invalidPolicyError = $null
            try { Get-GscSdkBaseline $missingRoot | Out-Null }
            catch { $invalidPolicyError = $_.Exception.Message }
            if (-not $invalidPolicyError) { throw 'Roll-forward policy was accepted by the preflight' }
        }
        finally { Pop-Location }
        $succeeded = $true
        Write-Host "SDK baseline tests passed: actual $required; different major and real missing-SDK resolution rejected. Evidence: $($context.RunRoot)"
    }
    finally { Pop-Location }
}
finally { Complete-GscDiagnostics $context $succeeded }
# Expected negative resolver steps must not leak into an outer pwsh CI step.
exit 0
