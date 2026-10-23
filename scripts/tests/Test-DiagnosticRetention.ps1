[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
. (Join-Path $repo 'scripts/diagnostic-retention.ps1')
$fixture=Join-Path $repo ('.tmp/retention-'+[guid]::NewGuid().ToString('N'))
$source=Join-Path $fixture 'source'
$archive=Join-Path $fixture 'archive'
try {
    New-Item -ItemType Directory -Path $source,$archive -Force|Out-Null
    foreach($name in @('summary.json','failure-console.txt','failure.trx','failure.dmp','first.png','second.png')) {
        $content=if($name -like '*.png'){'same visual evidence'}else{'unique evidence '+$name}
        [IO.File]::WriteAllText((Join-Path $source $name),$content)
    }
    $inventory=@(Get-GscEvidenceInventory $source)
    if(@($inventory|Where-Object Tier -eq 'large-diagnostic').Count -ne 1){throw 'Dump tier missing'}
    if(@($inventory|Where-Object DuplicateOf).Count -ne 1){throw 'Visual duplicate identification failed'}
    Copy-Item -Path (Join-Path $source '*') -Destination $archive
    if(-not (Test-GscEvidenceArchive $inventory $archive).Verified){throw 'Complete archive rejected'}
    [IO.File]::WriteAllText((Join-Path $archive 'failure.trx'),'corrupted')
    if((Test-GscEvidenceArchive $inventory $archive).Verified){throw 'Changed archive accepted'}
    Remove-Item -LiteralPath (Join-Path $archive 'failure.dmp') -Force
    if((Test-GscEvidenceArchive $inventory $archive).Verified){throw 'Missing dump accepted'}
    $rejected=$false
    try { Test-GscEvidenceArchive @([pscustomobject]@{Path='../outside';SHA256='x';Bytes=1}) $archive|Out-Null } catch {$rejected=$true}
    if(-not $rejected){throw 'Traversal accepted'}
    $emptyRejected=$false
    try { $emptyResult=Test-GscEvidenceArchive -Inventory @() -ArchiveRoot $archive; $emptyRejected=-not $emptyResult.Verified } catch {$emptyRejected=$true}
    if(-not $emptyRejected){throw 'Empty inventory accepted'}
    $link=Join-Path $fixture 'linked-source'
    New-Item -ItemType Junction -Path $link -Target $source|Out-Null
    try {
        $linkRejected=$false
        try { Get-GscEvidenceInventory $link|Out-Null } catch {$linkRejected=$true}
        if(-not $linkRejected){throw 'Linked evidence root accepted'}
    }
    finally { [IO.Directory]::Delete($link, $false) }
    $after=@(Get-GscEvidenceInventory $source)
    if(($inventory|ConvertTo-Json -Compress) -ne ($after|ConvertTo-Json -Compress)){throw 'Original evidence changed'}
    $out=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-06'
    @{Engine=$PSVersionTable.PSVersion.ToString();Edition=$PSVersionTable.PSEdition;CompleteArchive=$true;ChangedRefused=$true;MissingRefused=$true;TraversalRefused=$true;DumpTier=$true;VisualDuplicate=$true;OriginalsUnchanged=$true;CanDelete=$false}|ConvertTo-Json|ForEach-Object {[IO.File]::WriteAllText((Join-Path $out ('verification-'+$PSVersionTable.PSEdition+'.json')), $_, [Text.UTF8Encoding]::new($false))}
}
finally {
    $full=[IO.Path]::GetFullPath($fixture)
    $allowed=[IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar
    if(-not $full.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe fixture cleanup'}
    if(Test-Path -LiteralPath $full){Remove-Item -LiteralPath $full -Recurse -Force}
}
Write-Host 'Diagnostic retention checks passed'
