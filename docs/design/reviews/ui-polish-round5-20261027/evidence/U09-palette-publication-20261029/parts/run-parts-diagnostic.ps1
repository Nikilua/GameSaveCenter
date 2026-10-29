$ErrorActionPreference='Stop'
$phase=$PSScriptRoot
$records=@()
$order=@('before','after')
foreach($i in 0..1){
 $side=$order[$i]
 $output=Join-Path $phase ('parts-diagnostic-'+($i+1)+'-'+$side)
 if(Test-Path -LiteralPath $output){throw 'measurement output already exists'}
 New-Item -ItemType Directory $output|Out-Null
 $exe=Join-Path $phase "$side/tests/GameSaveCenter.RenderHarness/bin/Release/net472/GameSaveCenter.RenderHarness.exe"
 $dll=Join-Path $phase "$side/src/GameSaveCenter.Playnite/bin/Release/net462/GameSaveCenter.Playnite.dll"
 $record=@{order=$i+1;side=$side;utcStart=[DateTime]::UtcNow.ToString('O');host='ControlledWpfWindow';size='1040x700 DIP';exeSha256=(Get-FileHash -LiteralPath $exe).Hash;pluginSha256=(Get-FileHash -LiteralPath $dll).Hash;pluginVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion;processesBefore=@(Get-Process dotnet,MSBuild,Playnite.DesktopApp -ErrorAction SilentlyContinue|Select-Object Id,ProcessName,CPU,StartTime);freeBefore=@(Get-PSDrive C,D|Select-Object Name,Free)}
 & $exe enduranceprobe $output 3600 > (Join-Path $output 'console.txt') 2>&1
 $record.exitCode=$LASTEXITCODE
 $record.utcEnd=[DateTime]::UtcNow.ToString('O')
 $record.processesAfter=@(Get-Process dotnet,MSBuild,Playnite.DesktopApp -ErrorAction SilentlyContinue|Select-Object Id,ProcessName,CPU,StartTime)
 $record|ConvertTo-Json -Depth 7|Set-Content (Join-Path $output 'identity.json') -Encoding utf8
 $records+=$record
 $records|ConvertTo-Json -Depth 7|Set-Content (Join-Path $phase 'parts-diagnostic-index.json') -Encoding utf8
 Write-Output "MEASUREMENT_$($i+1)_$side EXIT=$($record.exitCode)"
 if($record.exitCode){Get-Content (Join-Path $output 'console.txt') -Tail 12;throw 'measurement failed; preserve diagnostic'}
}
