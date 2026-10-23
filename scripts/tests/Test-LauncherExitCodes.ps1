[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$output = Join-Path $repo ('.tmp/launcher-exit-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $output 'scripts') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'GameSaveCenter-Run.cmd'), (Join-Path $repo 'GameSaveCenter-一键构建安装运行.cmd') -Destination $output
Copy-Item -LiteralPath (Join-Path $repo 'scripts/build.cmd') -Destination (Join-Path $output 'scripts')
Set-Content -LiteralPath (Join-Path $output 'scripts/dev-install-run.ps1') -Value '# DEV-INSTALL-009 isolated fixture'
Set-Content -LiteralPath (Join-Path $output 'scripts/build.ps1') -Value '# isolated fixture'
$results = @()
foreach ($entry in @('GameSaveCenter-Run.cmd', 'GameSaveCenter-一键构建安装运行.cmd', 'scripts/build.cmd')) {
    foreach ($code in @(0, 7, 23)) {
        $scriptName = if ($entry -eq 'scripts/build.cmd') { 'build.ps1' } else { 'dev-install-run.ps1' }
        Set-Content -LiteralPath (Join-Path $output ('scripts/' + $scriptName)) -Value "# DEV-INSTALL-009 isolated fixture`nexit $code"
        $log = Join-Path $output (($entry -replace '[/\\]', '-') + "-$code.log")
        & cmd.exe /d /c ('"' + (Join-Path $output $entry) + '" < nul > "' + $log + '" 2>&1')
        $actual = $LASTEXITCODE
        $text = Get-Content -LiteralPath $log -Raw
        $falseSuccess = $code -ne 0 -and $text -match '\[OK\]'
        $results += [pscustomobject]@{ Entry = $entry; Injected = $code; Actual = $actual; Passed = ($actual -eq $code -and -not $falseSuccess) }
    }
}
$results | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'results.json')
$results | Format-Table | Out-Host
Write-Host "Evidence: $output"
if (@($results | Where-Object { -not $_.Passed }).Count) { throw 'Launcher exit-code propagation failed; evidence retained.' }
