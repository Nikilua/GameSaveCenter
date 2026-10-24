[CmdletBinding()]
param([string]$EvidenceRoot = '')
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/sdk-baseline.ps1')
$fixture = Join-Path $repo ('.tmp/sdk-recovery-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'global.json') -Destination $fixture
$install = Join-Path $fixture 'sdk'
$marker = Join-Path $fixture 'ready'
$required = Get-GscSdkBaseline $fixture
$initialPath = $env:PATH
$initialRoot = $env:DOTNET_ROOT
$initialRootX64 = $env:DOTNET_ROOT_X64
$initialTemp = $env:TEMP
$initialTmp = $env:TMP
$mode = 'interrupted'
$attempts = 0
$completed = $false
# Replace only the transport and SDK observation in this test process. The real
# bootstrap function, child PowerShell invocation and failure cleanup still run.
function Get-GscDotNetSdkSelection {
    if (Test-Path -LiteralPath $marker) { return [pscustomobject]@{ExitCode=0;Version=$required;Output=@($required)} }
    return [pscustomobject]@{ExitCode=1;Version='';Output=@('Only SDK 9.0.302 is installed')}
}
function Get-GscInstalledSdks { return @('9.0.302 [isolated observation]') }
function Invoke-WebRequest {
    param($Uri, $OutFile, [switch]$UseBasicParsing, $MaximumRedirection)
    if ($Uri -ne 'https://dot.net/v1/dotnet-install.ps1') { throw 'Unexpected download source' }
    $script:attempts++
    if ($mode -eq 'interrupted') {
        Set-Content -LiteralPath $OutFile -Value 'partial download'
        throw 'Injected connection interruption'
    }
    if ($mode -eq 'corrupt') { Set-Content -LiteralPath $OutFile -Value 'exit 37'; return }
    Set-Content -LiteralPath $OutFile -Value ("Set-Content -LiteralPath '" + $marker.Replace("'", "''") + "' -Value ready`nexit 0")
}
$results = @()
try {
    $env:TEMP = $fixture
    $env:TMP = $fixture
    foreach ($mode in @('interrupted', 'corrupt')) {
        $failure = ''
        try { Initialize-GscSdkBaseline -ProjectRoot $fixture -InstallDirectory $install | Out-Null }
        catch { $failure = $_.Exception.Message }
        $expected = if ($mode -eq 'interrupted') { 'Injected connection interruption' } else { '37' }
        if (-not $failure.Contains($expected)) { throw "Missing actionable failure: $mode : $failure" }
        if ($env:PATH -ne $initialPath -or $env:DOTNET_ROOT -ne $initialRoot -or $env:DOTNET_ROOT_X64 -ne $initialRootX64) { throw 'Failed bootstrap leaked SDK environment' }
        if (@(Get-ChildItem -LiteralPath $fixture -Directory -Filter 'gsc-sdk-bootstrap-*').Count) { throw 'Partial download directory leaked' }
        $results += [pscustomobject]@{Case=$mode;Passed=$true;Diagnostic=$failure}
    }
    $mode = 'retry'
    $result = Initialize-GscSdkBaseline -ProjectRoot $fixture -InstallDirectory $install
    if (-not $result.Bootstrapped -or $result.SelectedVersion -ne $required -or $attempts -ne 3) { throw 'Retry did not validate the exact SDK' }
    $results += [pscustomobject]@{Case='retry-after-two-failures';Passed=$true;Diagnostic='Exact SDK observation required after installer exit 0; installer payload is simulated.'}
    $results | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $fixture 'results.json')
    $results | Format-Table | Out-Host
    if (-not [string]::IsNullOrWhiteSpace($EvidenceRoot)) {
        $resolvedEvidenceRoot = [IO.Path]::GetFullPath($EvidenceRoot)
        $repoPrefix = [IO.Path]::GetFullPath($repo).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if (-not $resolvedEvidenceRoot.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw "EvidenceRoot must be inside the repository: $resolvedEvidenceRoot"
        }
        if (Test-Path -LiteralPath (Join-Path $resolvedEvidenceRoot 'sdk-bootstrap-recovery.json')) {
            throw "Refusing to overwrite existing SDK recovery evidence: $resolvedEvidenceRoot"
        }
        New-Item -ItemType Directory -Path $resolvedEvidenceRoot -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $fixture 'results.json') -Destination (Join-Path $resolvedEvidenceRoot 'sdk-bootstrap-recovery.json')
        $completed = $true
        Write-Host "Evidence: $resolvedEvidenceRoot"
    }
    else {
        Write-Host "Evidence: $fixture"
    }
}
finally {
    $env:PATH = $initialPath
    $env:DOTNET_ROOT = $initialRoot
    $env:DOTNET_ROOT_X64 = $initialRootX64
    $env:TEMP = $initialTemp
    $env:TMP = $initialTmp
    if ($completed) {
        $resolvedFixture = [IO.Path]::GetFullPath($fixture)
        $allowedPrefix = [IO.Path]::GetFullPath((Join-Path $repo '.tmp')).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if (-not $resolvedFixture.StartsWith($allowedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Unsafe SDK recovery fixture cleanup: $resolvedFixture"
        }
        $fixtureItem = Get-Item -LiteralPath $resolvedFixture -Force
        if ($fixtureItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Refusing to clean SDK recovery fixture link: $resolvedFixture"
        }
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}
