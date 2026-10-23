[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$SourceRoot,
    [Parameter(Mandatory=$true)][string]$DestinationRoot
)
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'diagnostic-retention.ps1')
$source=[IO.Path]::GetFullPath($SourceRoot).TrimEnd('\','/')
$destination=[IO.Path]::GetFullPath($DestinationRoot).TrimEnd('\','/')
$allowed=@('artifacts','.tmp'|ForEach-Object {[IO.Path]::GetFullPath((Join-Path $repo $_))+[IO.Path]::DirectorySeparatorChar})
if(-not @($allowed|Where-Object {$destination.StartsWith($_,[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Archive destination must be inside repository artifacts or .tmp'}
if($destination.Equals($source,[StringComparison]::OrdinalIgnoreCase) -or $destination.StartsWith($source+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Archive cannot be inside its source'}
if(Test-Path -LiteralPath $destination){throw 'Archive refuses to overwrite an existing destination'}
$ancestor=Split-Path -Parent $destination
while($ancestor){
    if(Test-Path -LiteralPath $ancestor){
        if((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked archive destination'}
    }
    $ancestor=Split-Path -Parent $ancestor
}
$inventory=@(Get-GscEvidenceInventory $source)
if(-not $inventory.Count){throw 'Empty evidence cannot be archived'}
if(@($inventory|Where-Object Path -eq 'archive-verification.json').Count){throw 'Source contains the reserved archive report filename'}
New-Item -ItemType Directory -Path $destination -ErrorAction Stop|Out-Null
foreach($entry in $inventory){
    $target=Join-Path $destination $entry.Path
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force|Out-Null
    Copy-Item -LiteralPath (Join-Path $source $entry.Path) -Destination $target -ErrorAction Stop
}
$check=Test-GscEvidenceArchive $inventory $destination
if(-not $check.Verified){throw ('Archive verification failed: '+($check.Issues -join '; '))}
$after=@(Get-GscEvidenceInventory $source)
if(($inventory|ConvertTo-Json -Depth 5 -Compress) -ne ($after|ConvertTo-Json -Depth 5 -Compress)){throw 'Source changed during archive; original evidence remains required'}
$report=[ordered]@{SourceFiles=$inventory.Count;Verified=$true;OriginalsRetained=$true;CanDelete=$false;Inventory=$inventory}
[IO.File]::WriteAllText((Join-Path $destination 'archive-verification.json'),($report|ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
Write-Output ([pscustomobject]@{Verified=$true;Files=$inventory.Count;OriginalsRetained=$true;CanDelete=$false})
