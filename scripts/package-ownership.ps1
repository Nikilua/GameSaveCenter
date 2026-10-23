function Write-GscPackageOwnership {
    param([string]$RepositoryRoot, [string]$Stage, [string[]]$Archives, [string]$SourceCommit, [string]$FailureMessage = '')
    $root=[IO.Path]::GetFullPath((Join-Path $RepositoryRoot 'artifacts'))
    $runId=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')+'-'+[guid]::NewGuid().ToString('N').Substring(0,8)
    $entries=@()
    foreach($path in @($Stage)+@($Archives)) {
        if(-not $path){continue}
        $full=[IO.Path]::GetFullPath($path)
        if(-not $full.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Package ownership path outside artifacts'}
        $exists=Test-Path -LiteralPath $full
        if(-not $exists -and -not $FailureMessage){throw "Package artifact missing: $full"}
        $item=if($exists){Get-Item -LiteralPath $full -ErrorAction Stop}else{$null}
        if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked package artifact'}
        $hash=if(-not $exists -or $item.PSIsContainer){$null}else{(Get-FileHash -LiteralPath $full).Hash}
        $entries+=@{Path=$full;Stage='package';CreatedByThisRun=(-not [bool]$FailureMessage);Exists=$exists;Active=$false;Retention=$(if($FailureMessage){'Failure evidence or prior stage; retain for diagnosis and ownership review'}else{'Current delivery output; keep until superseded and references reviewed'});SHA256=$hash}
    }
    $directory=Join-Path $root 'package-ownership'
    New-Item -ItemType Directory $directory -Force|Out-Null
    $record=@{schemaVersion=1;outcome=$(if($FailureMessage){'failed'}else{'passed'});failureMessage=$FailureMessage;sourceCommit=$SourceCommit;owner=@{ProcessId=$PID;ProcessStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o');RunId=$runId};artifacts=$entries}
    $record|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $directory ($runId+'.json')) -Encoding UTF8
}
