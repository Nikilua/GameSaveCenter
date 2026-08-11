param([string]$Destination = (Join-Path $HOME ".agents\skills"))
$ErrorActionPreference = "Stop"
$SkillRoot = Split-Path -Parent $PSScriptRoot
$SkillName = Split-Path -Leaf $SkillRoot
$Target = Join-Path $Destination $SkillName
New-Item -ItemType Directory -Force -Path $Destination | Out-Null
if (Test-Path $Target) {
    $Backup = "$Target.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Move-Item $Target $Backup
    Write-Host "Existing skill backed up to: $Backup"
}
Copy-Item -Recurse -Force $SkillRoot $Target
Write-Host "Installed skill to: $Target"
Write-Host 'Restart Codex, then invoke with: $wpf-apple-desktop-ui'
