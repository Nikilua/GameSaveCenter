function Write-GscPackageOwnership {
    param([string]$RepositoryRoot, [string]$Stage, [string[]]$Archives, [string]$SourceCommit)
    $root=[IO.Path]::GetFullPath((Join-Path $RepositoryRoot 'artifacts'))
    $runId=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')+'-'+[guid]::NewGuid().ToString('N').Substring(0,8)
    $entries=@()
    foreach($path in @($Stage)+@($Archives)) {
        if(-not $path){continue}
        $full=[IO.Path]::GetFullPath($path)
        if(-not $full.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Package ownership path outside artifacts'}
        $item=Get-Item -LiteralPath $full -ErrorAction Stop
        if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked package artifact'}
        $hash=if($item.PSIsContainer){$null}else{(Get-FileHash -LiteralPath $full).Hash}
        $entries+=@{Path=$full;Stage='package';CreatedByThisRun=$true;Active=$false;Retention='Current delivery output; keep until superseded and references reviewed';SHA256=$hash}
    }
    $directory=Join-Path $root 'package-ownership'
    New-Item -ItemType Directory $directory -Force|Out-Null
    $record=@{schemaVersion=1;sourceCommit=$SourceCommit;owner=@{ProcessId=$PID;ProcessStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o');RunId=$runId};artifacts=$entries}
    $record|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $directory ($runId+'.json')) -Encoding UTF8
}
