[CmdletBinding()]
param([string[]]$ManifestPaths=@(), [string]$OutputPath='')
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'artifact-preview.ps1')
if($ManifestPaths.Count -eq 0) {
    $directory=Join-Path $repo 'artifacts/package-ownership'
    if(Test-Path -LiteralPath $directory){$ManifestPaths=@(Get-ChildItem -LiteralPath $directory -Filter *.json -File|ForEach-Object {$_.FullName})}
}
$manifests=@($ManifestPaths|ForEach-Object {Get-Content -LiteralPath $_ -Raw|ConvertFrom-Json})
$rows=@(foreach($manifest in $manifests){Get-GscArtifactPreview $repo $manifest})
$report=@{ReadOnly=$true;Manifests=$ManifestPaths;Artifacts=$rows;Duplicates=@(Get-GscDuplicateArtifactPreview $repo $manifests)}
$json=$report|ConvertTo-Json -Depth 12
if($OutputPath) {
    $full=[IO.Path]::GetFullPath($OutputPath)
    $allowed=@('artifacts','.tmp'|ForEach-Object {[IO.Path]::GetFullPath((Join-Path $repo $_))+[IO.Path]::DirectorySeparatorChar})
    if(-not @($allowed|Where-Object {$full.StartsWith($_,[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Preview output must be under repository artifacts or .tmp'}
    if(Test-Path -LiteralPath $full){throw 'Preview refuses to overwrite an existing file'}
    $parent=Split-Path $full -Parent
    while($parent -and $parent.Length -ge $repo.Length){if(Test-Path -LiteralPath $parent){if((Get-Item -LiteralPath $parent).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked preview output'}};if($parent -eq $repo){break};$parent=Split-Path $parent -Parent}
    New-Item -ItemType Directory (Split-Path $full -Parent) -Force|Out-Null
    [IO.File]::WriteAllText($full,$json,[Text.UTF8Encoding]::new($false))
}
else {Write-Output $json}
