[CmdletBinding()]
param([string]$EvidenceRoot = '')
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tokens = $null; $errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $repo 'scripts/dev-install-run.ps1'), [ref]$tokens, [ref]$errors)
if ($errors.Count) { throw $errors }
foreach ($name in @('Read-ManifestVersion','Get-GscInstalledAssemblyIdentities','Install-ExtensionAtomically')) {
    $node = $ast.Find({param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $name}, $true)
    Invoke-Expression $node.Extent.Text
}
$extensionId = 'fixture'
$output = Join-Path $repo ('.tmp/install-transaction-' + [Guid]::NewGuid().ToString('N'))
$diagnostics = Join-Path $output 'attempts'
$source = Join-Path $output 'source'; $extensions = Join-Path $output 'extensions'; $target = Join-Path $extensions $extensionId
New-Item -ItemType Directory -Path $source,$target -Force | Out-Null
Set-Content (Join-Path $source 'extension.yaml') 'Version: 1.2.3'
Set-Content (Join-Path $source 'GameSaveCenter.Playnite.dll') 'new fixture'
Set-Content (Join-Path $target 'GameSaveCenter.Playnite.dll') 'old fixture'
Set-Content (Join-Path $target 'sentinel') 'preserve'
$lock = [IO.File]::Open((Join-Path $target 'GameSaveCenter.Playnite.dll'),'Open','Read','None')
try {
    $failed = $false
    try { Install-ExtensionAtomically $source $extensions '1.2.3' -DiagnosticRoot $diagnostics } catch { $failed = $true }
    if (-not $failed -or -not (Test-Path (Join-Path $target 'sentinel'))) { throw 'Occupied installation was modified' }
} finally { $lock.Dispose() }
if ((Get-Content (Join-Path $target 'GameSaveCenter.Playnite.dll') -Raw).Trim() -ne 'old fixture') { throw 'Old DLL changed' }
# Fail only the staging rename, then allow the actual rollback rename.
function Move-Item {
    param($LiteralPath,$Destination,$ErrorAction)
    if ($LiteralPath -like '*.__new-*') { throw 'Injected staging rename failure' }
    Microsoft.PowerShell.Management\Move-Item -LiteralPath $LiteralPath -Destination $Destination -ErrorAction Stop
}
$failed = $false
try { Install-ExtensionAtomically $source $extensions '1.2.3' -DiagnosticRoot $diagnostics } catch { $failed = $true }
Remove-Item Function:Move-Item
if (-not $failed -or -not (Test-Path (Join-Path $target 'sentinel'))) { throw 'Rollback failed' }
$result = Install-ExtensionAtomically $source $extensions '1.2.3' -DiagnosticRoot $diagnostics
if ((Get-Content (Join-Path $target 'GameSaveCenter.Playnite.dll') -Raw).Trim() -ne 'new fixture') { throw 'Retry failed' }
$identities = @($result.AssemblyIdentities)
$pluginIdentity = $identities | Where-Object File -EQ 'GameSaveCenter.Playnite.dll'
if ($identities.Count -ne 6 -or -not $pluginIdentity.Exists -or $pluginIdentity.BuildIdentity -ne 'unknown') { throw 'Missing/unknown assembly metadata was misrepresented' }
if ($pluginIdentity.Path -ne [IO.Path]::GetFullPath((Join-Path $target 'GameSaveCenter.Playnite.dll')) -or
    $pluginIdentity.SHA256 -ne (Get-FileHash -LiteralPath (Join-Path $source 'GameSaveCenter.Playnite.dll')).Hash) { throw 'Installed path/hash does not identify the actual file' }
if (@($identities | Where-Object { -not $_.Exists -and $_.BuildIdentity -eq 'missing' -and $_.SHA256 -eq 'unavailable' }).Count -ne 5) { throw 'Missing assembly identity was hidden' }
$attempts=@(Get-ChildItem -LiteralPath $diagnostics -Filter *.json|ForEach-Object {Get-Content $_.FullName -Raw|ConvertFrom-Json})
if($attempts.Count -ne 3 -or @($attempts|Where-Object {$_.Outcome -eq 'failed'}).Count -ne 2 -or @($attempts|Where-Object {$_.Outcome -eq 'passed'}).Count -ne 1){throw 'Install attempt outcomes missing'}
if(@($attempts|Where-Object {$_.Outcome -eq 'failed' -and -not $_.FailureMessage}).Count){throw 'Failure reason not retained'}
if(@($attempts|Where-Object {$_.Outcome -eq 'passed' -and $_.TemporaryExists}).Count){throw 'Successful staging still present'}
$evidence = if ($EvidenceRoot) { [IO.Path]::GetFullPath($EvidenceRoot) } else { Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T00-05' }
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
@{ ActualInstalledPathAndHashVerified = $true; MissingAndUnknownIdentitiesExplicit = $true; LockedFilePreserved = $true; RenameFailureRolledBack = $true; AttemptRecordsVerified = $true; RetrySucceeded = $true; Fixture = 'Synthetic files, production functions parsed from AST'; Timestamp = [DateTime]::UtcNow.ToString('o') } | ConvertTo-Json | Set-Content (Join-Path $evidence 'results.json')
$resolved = [IO.Path]::GetFullPath($output)
if (-not $resolved.StartsWith([IO.Path]::GetFullPath((Join-Path $repo '.tmp')) + [IO.Path]::DirectorySeparatorChar)) { throw 'Unsafe cleanup' }
Remove-Item -LiteralPath $resolved -Recurse -Force
Write-Host 'Installation transaction: 3/3 passed'
