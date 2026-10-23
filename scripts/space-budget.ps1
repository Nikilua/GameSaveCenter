function Get-GscSpaceBudget {
    param([Parameter(Mandatory=$true)][object[]]$Requirements, [scriptblock]$ReadVolume)
    if (-not $ReadVolume) {
        $ReadVolume = {
            param($Path)
            $full = [IO.Path]::GetFullPath($Path)
            $root = [IO.Path]::GetPathRoot($full)
            $drive = [IO.DriveInfo]::new($root)
            if (-not $drive.IsReady) { throw "Volume not ready: $root" }
            @{Root=$root;AvailableBytes=$drive.AvailableFreeSpace}
        }
    }
    $volumes = @{}
    foreach ($requirement in $Requirements) {
        if ([string]::IsNullOrWhiteSpace($requirement.Path) -or [long]$requirement.BudgetBytes -le 0) { throw 'Every space requirement needs an explicit path and positive byte budget' }
        $full = [IO.Path]::GetFullPath($requirement.Path)
        $volume = & $ReadVolume $full
        if (-not $volume.Root -or $null -eq $volume.AvailableBytes -or [long]$volume.AvailableBytes -lt 0) { throw "Cannot determine free space: $full" }
        $key = ([string]$volume.Root).ToUpperInvariant()
        if (-not $volumes.ContainsKey($key)) { $volumes[$key]=@{Root=$volume.Root;AvailableBytes=[long]$volume.AvailableBytes;BudgetBytes=[long]0;Requirements=[Collections.Generic.List[object]]::new()} }
        $row = $volumes[$key]
        $row.AvailableBytes = [Math]::Min($row.AvailableBytes, [long]$volume.AvailableBytes)
        if ([long]$requirement.BudgetBytes -gt [long]::MaxValue - $row.BudgetBytes) { throw 'Space budget overflow' }
        $row.BudgetBytes += [long]$requirement.BudgetBytes
        $row.Requirements.Add(@{Stage=$requirement.Stage;Path=$full;BudgetBytes=[long]$requirement.BudgetBytes;Basis=$requirement.Basis})
    }
    foreach ($row in $volumes.Values) {
        [pscustomobject]@{Root=$row.Root;AvailableBytes=$row.AvailableBytes;BudgetBytes=$row.BudgetBytes;Sufficient=($row.AvailableBytes -ge $row.BudgetBytes);Requirements=$row.Requirements.ToArray();Unknown='SDK downloads, NuGet cache growth and host data are not included'}
    }
}
function Assert-GscSpaceBudget {
    param([object[]]$Report)
    $short = @($Report | Where-Object {-not $_.Sufficient})
    if ($short.Count) { throw ('Insufficient free space; review owned artifact cleanup before retry: ' + (($short | ForEach-Object { '{0} needs {1} bytes; available {2}' -f $_.Root,$_.BudgetBytes,$_.AvailableBytes }) -join '; ')) }
}
function Invoke-GscSpacePreflight {
    param([string]$RepositoryRoot, [string]$OutputPath, [string]$TempPath, [switch]$IncludePackage)
    $config=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'scripts/space-budget.json') -Raw|ConvertFrom-Json
    if ($config.schemaVersion -ne 1) {throw 'Unsupported space budget configuration'}
    if (-not $OutputPath) {$OutputPath=Join-Path $RepositoryRoot 'artifacts'}
    if (-not $TempPath) {$TempPath=Join-Path $OutputPath 'test-temp'}
    $requirements=@(
        @{Stage='build-output';Path=$OutputPath;BudgetBytes=$config.outputBytes;Basis=$config.basis},
        @{Stage='test-temp';Path=$TempPath;BudgetBytes=$config.tempBytes;Basis=$config.basis},
        @{Stage='system-temp';Path=[IO.Path]::GetTempPath();BudgetBytes=$config.tempBytes;Basis=$config.basis}
    )
    if($IncludePackage){$requirements+=@{Stage='package';Path=(Join-Path $RepositoryRoot 'artifacts');BudgetBytes=$config.packageBytes;Basis=$config.basis}}
    $report=@(Get-GscSpaceBudget $requirements)
    foreach($volume in $report){Write-Host ('Space preflight {0}: available {1} bytes, configured headroom {2} bytes' -f $volume.Root,$volume.AvailableBytes,$volume.BudgetBytes)}
    Write-Host ('Budget basis: '+$config.basis)
    Assert-GscSpaceBudget $report
    return $report
}
