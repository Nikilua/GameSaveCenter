[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tokens=$null;$parseErrors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $repo 'scripts/run-playnite-tests-isolated.ps1'),[ref]$tokens,[ref]$parseErrors)
$function=$ast.Find({param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Invoke-PlayniteTestProcess'},$true)
if(-not $function -or $parseErrors.Count){throw 'Production test runner function unavailable'}
. ([scriptblock]::Create($function.Extent.Text))
$fixture=Join-Path $repo ('.tmp/temp-cleanup-'+[guid]::NewGuid().ToString('N'))
$OutputRoot=$fixture;$ProjectRoot=$repo;$Configuration='Release';$DiagnosticsContext=@{RunRoot=$fixture}
$previousTemp=$env:TEMP
function Complete-GscTestDiagnostics {param($Context,$ResultsDirectory)}
function Invoke-GscRecordedCommand {
    param($Context,$StepId,$FilePath,$Arguments,$TimeoutSeconds)
    New-Item -ItemType Directory -Path (Join-Path $env:TEMP $StepId)|Out-Null
    [IO.File]::WriteAllText((Join-Path (Join-Path $env:TEMP $StepId) 'shadow.bin'),'owned temporary data')
    if($script:lockFixture){$script:heldFile=[IO.File]::Open((Join-Path (Join-Path $env:TEMP $StepId) 'shadow.bin'),[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::None)}
    [pscustomobject]@{ExitCode=$script:fixtureExit;Output=@('controlled command result')}
}
try {
    New-Item -ItemType Directory -Path $fixture|Out-Null
    $env:TEMP=$fixture
    $unknown=Join-Path $fixture 'unknown-user-file';[IO.File]::WriteAllText($unknown,'preserve unknown bytes')
    $hash=(Get-FileHash -LiteralPath $unknown).Hash
    $rows=@()
    foreach($exit in @(7,124,0,0,0)){
        $script:fixtureExit=$exit;$id='run-'+$rows.Count
        $threw=$false
        try{Invoke-PlayniteTestProcess -Filter 'fixture' -Label $id -StepId $id}catch{$threw=$true}
        if($threw -ne ($exit -ne 0)){throw 'Cleanup runner lost command failure'}
        if((Test-Path -LiteralPath (Join-Path $fixture $id)) -ne ($exit -ne 0)){throw 'Failure evidence or successful cleanup mismatch'}
        $files=@(Get-ChildItem -LiteralPath $fixture -Recurse -File)
        $rows+=@{ExitCode=$exit;Files=$files.Count;Bytes=($files|Measure-Object Length -Sum).Sum}
    }
    if($rows[2].Files -ne $rows[4].Files -or $rows[2].Bytes -ne $rows[4].Bytes){throw 'Success retries accumulated shadow copies'}
    if((Get-FileHash -LiteralPath $unknown).Hash -ne $hash){throw 'Unknown user file changed'}
    $script:fixtureExit=0;$script:lockFixture=$true;$script:heldFile=$null
    $cleanupFailed=$false
    try {Invoke-PlayniteTestProcess -Filter 'fixture' -Label 'locked-cleanup' -StepId 'locked-cleanup'} catch {$cleanupFailed=$true}
    finally {if($script:heldFile){$script:heldFile.Dispose()};$script:lockFixture=$false}
    if(-not $cleanupFailed){throw 'Occupied file cleanup failure was hidden'}
    $lockedPath=Join-Path $fixture 'locked-cleanup'
    if(-not (Test-Path -LiteralPath (Join-Path $lockedPath 'shadow.bin'))){throw 'Occupied evidence was lost'}
    Remove-Item -LiteralPath $lockedPath -Recurse -Force
    if(Test-Path -LiteralPath $lockedPath){throw 'Cleanup retry failed after lock release'}
    $out=Join-Path $repo 'docs/design/reviews/build-ui-reliability-20261022/evidence/T01-08'
    New-Item -ItemType Directory -Path $out -Force|Out-Null
    @{Engine=$PSVersionTable.PSVersion.ToString();Scope='Production function with controlled command result; exit124 models timeout, not physical cancellation';Rows=$rows;UnknownUnchanged=$true;OccupiedCleanupFailureVisible=$cleanupFailed;ReleasedLockCleanupRetry=$true}|ConvertTo-Json -Depth 6|ForEach-Object {[IO.File]::WriteAllText((Join-Path $out ('cleanup-'+$PSVersionTable.PSEdition+'.json')), $_,[Text.UTF8Encoding]::new($false))}
}
finally{
    $env:TEMP=$previousTemp
    $full=[IO.Path]::GetFullPath($fixture);$allowed=[IO.Path]::GetFullPath((Join-Path $repo '.tmp'))+[IO.Path]::DirectorySeparatorChar
    if(-not $full.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe cleanup'}
    if(Test-Path -LiteralPath $full){Remove-Item -LiteralPath $full -Recurse -Force}
}
Write-Host 'Isolated temp cleanup sequence passed'
