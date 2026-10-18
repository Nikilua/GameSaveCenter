# Shared by build.ps1 and the standalone Playnite test runner. No target assemblies
# are loaded to inspect identity; diagnostics never replace a native exit code.
function ConvertTo-GscDiagnosticText {
    param([string]$Text, [hashtable]$Context)
    $value = $Text -replace '\x1b\[[0-9;]*m', ''
    foreach ($path in @($Context.RepoRoot, $env:USERPROFILE)) {
        if (-not [string]::IsNullOrWhiteSpace($path)) {
            $label = if ($path -eq $Context.RepoRoot) { '[repo]' } else { '[user-profile]' }
            $value = [regex]::Replace($value, [regex]::Escape($path), $label, 'IgnoreCase')
            $value = [regex]::Replace($value, [regex]::Escape($path.Replace('\', '/')), $label, 'IgnoreCase')
            # JSON escapes backslashes before this final redaction pass.
            $value = [regex]::Replace($value, [regex]::Escape($path.Replace('\', '\\')), $label, 'IgnoreCase')
        }
    }
    $value = $value -replace '(?i)(https?://)[^/\s:@]+:[^/\s@]+@', '$1[credentials]@'
    $value = $value -replace '(?i)([?&](?:access_token|token|sig|api_key|key|password)=)[^&\s"<>]+', '$1[redacted]'
    $value = $value -replace 'computerName="[^"]*"', 'computerName="redacted-host"'
    $value = $value -replace 'runUser="[^"]*"', 'runUser="redacted-user"'
    $value = $value -replace 'runDeploymentRoot="[^"]*"', 'runDeploymentRoot="redacted-deployment"'
    $value = $value -replace '(<TestRun\b[^>]*\bname=)"[^"]*"', '$1"redacted-test-run"'
    return $value
}

function New-GscDiagnosticContext {
    param([string]$RepoRoot, [string]$DiagnosticsRoot)
    if ([string]::IsNullOrWhiteSpace($DiagnosticsRoot)) {
        $DiagnosticsRoot = Join-Path $RepoRoot 'artifacts/build-diagnostics'
    }
    $runRoot = Join-Path ([IO.Path]::GetFullPath($DiagnosticsRoot)) (
        (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssfffZ') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
    New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
    $commit = @(& git -C $RepoRoot rev-parse HEAD 2>$null)
    $dirtyPaths = @(& git -C $RepoRoot status --porcelain 2>$null)
    $sourceFiles = @(& git -C $RepoRoot ls-files --cached --others --exclude-standard -- src tests scripts global.json Directory.Build.props Directory.Build.targets .github/workflows/windows-build.yml |
        Sort-Object -Unique | Where-Object { $_ -match '\.(cs|xaml|csproj|props|targets|json|ps1|py|yml)$' } |
        ForEach-Object {
            $sourcePath = Join-Path $RepoRoot $_
            if (Test-Path -LiteralPath $sourcePath -PathType Leaf) {
                [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash }
            }
        })
    $context = @{
        RepoRoot = $RepoRoot; RunRoot = $runRoot; SourceCommit = ($commit -join '').Trim()
        DirtyPaths = $dirtyPaths; Sdk = 'unknown'; StartedUtc = [DateTime]::UtcNow.ToString('o')
        Steps = [Collections.Generic.List[object]]::new()
        SourceFiles = $sourceFiles; Assemblies = @(); TestSummary = $null; Outcome = 'running'; CompletedUtc = $null
        RunType = 'unspecified'; TestsRequested = $null; FailureMessage = $null
    }
    Write-GscDiagnosticSummary $context
    return $context
}

function New-GscTestOutcomeCounts {
    return [ordered]@{
        testCaseCount = 0; passed = 0; failed = 0; error = 0; timeout = 0; aborted = 0
        skipped = 0; notRunnable = 0; inconclusive = 0; disconnected = 0; warning = 0; otherOutcome = 0
        reportedCounterTotal = 0; reportedCounterFileCount = 0; counterMismatchCount = 0
        parseErrorCount = 0; trxFileCount = 0; distinctTestClassCount = 0; wpfIsolatedClassRunCount = 0
    }
}

function Add-GscTrxResultToCounts {
    param([Collections.IDictionary]$Counts, [string]$Outcome)

    $Counts['testCaseCount']++
    switch -CaseSensitive ($Outcome) {
        'Passed' { $Counts['passed']++ }
        'Failed' { $Counts['failed']++ }
        'Error' { $Counts['error']++ }
        'Timeout' { $Counts['timeout']++ }
        'Aborted' { $Counts['aborted']++ }
        'NotExecuted' { $Counts['skipped']++ }
        'NotRunnable' { $Counts['notRunnable']++ }
        'Inconclusive' { $Counts['inconclusive']++ }
        'Disconnected' { $Counts['disconnected']++ }
        'Warning' { $Counts['warning']++ }
        default { $Counts['otherOutcome']++ }
    }
}

function Get-GscTestSummary {
    param([hashtable]$Context)

    $testRoot = Join-Path $Context.RunRoot 'tests'
    $testRootFull = [IO.Path]::GetFullPath($testRoot)
    $trxFiles = if (Test-Path -LiteralPath $testRootFull -PathType Container) {
        @(Get-ChildItem -LiteralPath $testRootFull -Filter '*.trx' -Recurse -File -ErrorAction SilentlyContinue | Sort-Object FullName)
    }
    else { @() }

    $groups = [ordered]@{}
    $fileSummaries = [Collections.Generic.List[object]]::new()
    foreach ($trx in $trxFiles) {
        $relativePath = $trx.FullName.Substring($testRootFull.Length).TrimStart([char[]]@('\', '/')).Replace('\', '/')
        $groupId = if ($relativePath -match '^core/') { 'core' }
            elseif ($relativePath -match '^worker/') { 'worker' }
            elseif ($relativePath -match '^playnite-source/') { 'playnite-source' }
            elseif ($relativePath -match '^wpf\.[^/]+/') { 'playnite-wpf-isolated' }
            else { 'unclassified' }

        if (-not $groups.Contains($groupId)) {
            $groups[$groupId] = [ordered]@{
                counts = (New-GscTestOutcomeCounts)
                classes = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            }
        }
        $group = $groups[$groupId]
        $fileCounts = New-GscTestOutcomeCounts
        $fileCounts['trxFileCount'] = 1
        if ($groupId -eq 'playnite-wpf-isolated') { $fileCounts['wpfIsolatedClassRunCount'] = 1 }
        $fileClasses = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $declaredTotal = $null
        $parseError = $null
        try {
            [xml]$document = [IO.File]::ReadAllText($trx.FullName)
            $unitTestClasses = @{}
            foreach ($unitTest in @($document.SelectNodes("//*[local-name()='UnitTest']"))) {
                $method = $unitTest.SelectSingleNode("./*[local-name()='TestMethod']")
                $className = if ($method) { $method.GetAttribute('className') } else { '' }
                if (-not [string]::IsNullOrWhiteSpace($className)) {
                    $unitTestClasses[$unitTest.GetAttribute('id')] = $className
                }
            }

            foreach ($result in @($document.SelectNodes("//*[local-name()='UnitTestResult']"))) {
                $outcome = $result.GetAttribute('outcome')
                Add-GscTrxResultToCounts $fileCounts $outcome
                $className = $unitTestClasses[$result.GetAttribute('testId')]
                if (-not [string]::IsNullOrWhiteSpace($className)) {
                    [void]$group.classes.Add($className)
                    [void]$fileClasses.Add($className)
                }
            }

            $counters = $document.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
            if ($counters -and $counters.HasAttribute('total')) {
                $parsedTotal = 0
                if ([int]::TryParse($counters.GetAttribute('total'), [ref]$parsedTotal)) {
                    $declaredTotal = $parsedTotal
                    $fileCounts['reportedCounterTotal'] = $parsedTotal
                    $fileCounts['reportedCounterFileCount'] = 1
                    if ($parsedTotal -ne $fileCounts['testCaseCount']) { $fileCounts['counterMismatchCount'] = 1 }
                }
            }
        }
        catch {
            $parseError = $_.Exception.GetType().FullName
            $fileCounts['parseErrorCount'] = 1
        }

        foreach ($key in $fileCounts.Keys) { $group.counts[$key] += $fileCounts[$key] }
        $fileSummaries.Add([ordered]@{
            path = $relativePath; group = $groupId; testCaseCount = $fileCounts['testCaseCount']
            passed = $fileCounts['passed']; failed = $fileCounts['failed']; error = $fileCounts['error']
            timeout = $fileCounts['timeout']; aborted = $fileCounts['aborted']; skipped = $fileCounts['skipped']
            notRunnable = $fileCounts['notRunnable']; inconclusive = $fileCounts['inconclusive']
            disconnected = $fileCounts['disconnected']; warning = $fileCounts['warning']
            otherOutcome = $fileCounts['otherOutcome']; reportedCounterTotal = $declaredTotal
            counterMatchesResults = if ($null -ne $declaredTotal) { $declaredTotal -eq $fileCounts['testCaseCount'] } else { $null }
            distinctTestClassCount = $fileClasses.Count; wpfIsolatedClassRun = ($groupId -eq 'playnite-wpf-isolated')
            parseError = $parseError
        })
    }

    $groupSummaries = [Collections.Generic.List[object]]::new()
    $totalCounts = New-GscTestOutcomeCounts
    foreach ($groupId in $groups.Keys) {
        $group = $groups[$groupId]
        $group.counts['distinctTestClassCount'] = $group.classes.Count
        $groupSummary = [ordered]@{ id = $groupId }
        foreach ($key in $group.counts.Keys) { $groupSummary[$key] = $group.counts[$key] }
        $groupSummaries.Add($groupSummary)
        foreach ($key in @($totalCounts.Keys)) { $totalCounts[$key] += $group.counts[$key] }
    }

    return [ordered]@{
        schemaVersion = 1
        testCaseCountBasis = 'count of TRX UnitTestResult entries'
        reportedTotalBasis = 'sum of TRX ResultSummary/Counters total; compared with each file entry count'
        trxFileCount = $trxFiles.Count
        totals = $totalCounts
        groups = $groupSummaries.ToArray()
        files = $fileSummaries.ToArray()
    }
}

function Write-GscDiagnosticSummary {
    param([hashtable]$Context)
    $summary = [ordered]@{
        schemaVersion = 1; sourceCommit = $Context.SourceCommit
        workingTreeClean = $Context.DirtyPaths.Count -eq 0; dirtyPaths = $Context.DirtyPaths
        sdk = $Context.Sdk; startedUtc = $Context.StartedUtc; completedUtc = $Context.CompletedUtc
        outcome = $Context.Outcome; steps = @($Context.Steps.ToArray()); assemblies = $Context.Assemblies
        testSummary = $Context.TestSummary
        runType = $Context.RunType; testsRequested = $Context.TestsRequested
        failureMessage = $Context.FailureMessage; sourceFiles = $Context.SourceFiles
    }
    $json = ConvertTo-GscDiagnosticText ($summary | ConvertTo-Json -Depth 10) $Context
    [IO.File]::WriteAllText((Join-Path $Context.RunRoot 'summary.json'), $json, [Text.UTF8Encoding]::new($false))
}

function Set-GscDiagnosticAssemblies {
    param([hashtable]$Context, [string]$BuildRoot, [string]$Configuration)
    $rows = @()
    foreach ($project in @('GameSaveCenter.Playnite', 'GameSaveCenter.Playnite.Tests',
                          'GameSaveCenter.Worker', 'GameSaveCenter.Worker.Tests', 'GameSaveCenter.Core.Tests')) {
        $projectRoot = if ($BuildRoot) { Join-Path $BuildRoot "bin/$project/$Configuration" }
        elseif ($project.EndsWith('.Tests')) { Join-Path $Context.RepoRoot "tests/$project/bin/$Configuration" }
        else { Join-Path $Context.RepoRoot "src/$project/bin/$Configuration" }
        foreach ($dll in @(Get-ChildItem -LiteralPath $projectRoot -Filter "$project.dll" -Recurse -ErrorAction SilentlyContinue)) {
            $pdbPath = [IO.Path]::ChangeExtension($dll.FullName, '.pdb')
            $pdbIdentity = $null
            if (Test-Path -LiteralPath $pdbPath -PathType Leaf) {
                $pdbIdentity = [ordered]@{
                    file = [IO.Path]::GetFileName($pdbPath)
                    path = (ConvertTo-GscDiagnosticText $pdbPath $Context)
                    sha256 = $null
                    bytes = $null
                }
                try {
                    $pdbFile = Get-Item -LiteralPath $pdbPath
                    $pdbIdentity.bytes = $pdbFile.Length
                    $pdbIdentity.sha256 = (Get-FileHash -LiteralPath $pdbPath -Algorithm SHA256).Hash
                }
                catch {
                    # A locked or unreadable optional PDB must never mask the
                    # build/test exit that this diagnostic collector records.
                    $pdbIdentity['unavailableReason'] = $_.Exception.GetType().FullName
                }
            }
            $row = [ordered]@{ file = $dll.Name; path = (ConvertTo-GscDiagnosticText $dll.FullName $Context); sha256 = (Get-FileHash $dll.FullName).Hash
                productVersion = $dll.VersionInfo.ProductVersion; bytes = $dll.Length; mvid = $null; pdb = $pdbIdentity }
            $stream = $null; $pe = $null
            try {
                $stream = [IO.File]::OpenRead($dll.FullName)
                $pe = [System.Reflection.PortableExecutable.PEReader]::new($stream)
                $reader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                $row.mvid = $reader.GetGuid($reader.GetModuleDefinition().Mvid).ToString()
            }
            catch { $row['mvidUnavailableReason'] = $_.Exception.GetType().FullName }
            finally { if ($pe) { $pe.Dispose() }; if ($stream) { $stream.Dispose() } }
            $rows += $row
        }
    }
    $Context.Assemblies = $rows
    Write-GscDiagnosticSummary $Context
}

function Invoke-GscRecordedCommand {
    param([hashtable]$Context, [string]$StepId, [string]$FilePath, [string[]]$Arguments, [switch]$EchoOutput)
    if ($StepId -notmatch '^[A-Za-z0-9._-]+$') { throw "Invalid diagnostic step ID: $StepId" }
    if (@($Context.Steps | Where-Object { $_.id -eq $StepId }).Count -ne 0) { throw "Duplicate diagnostic step ID: $StepId" }
    $logPath = Join-Path $Context.RunRoot "$StepId-console.txt"
    $outputLines = [Collections.Generic.List[string]]::new()
    $writer = [IO.StreamWriter]::new($logPath, $false, [Text.UTF8Encoding]::new($false))
    $writer.AutoFlush = $true
    $exitCode = -1; $launchError = $null; $started = [DateTime]::UtcNow
    $previousErrorAction = $ErrorActionPreference
    # PowerShell 7 can optionally turn native nonzero exits into terminating errors.
    # Read the real code ourselves so the failure report is written before throwing.
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        # PATH may expose the same executable name more than once. Match native
        # shell precedence rather than passing an array of paths as one command.
        $nativeCommand = Get-Command $FilePath -CommandType Application -ErrorAction Stop | Select-Object -First 1
        # Windows PowerShell 5.1 surfaces native stderr as ErrorRecord even when
        # the process launches correctly. It must not abort collection before
        # LASTEXITCODE is available. Resolve the executable first so a missing
        # command cannot accidentally inherit an earlier successful exit code.
        $global:LASTEXITCODE = $null
        $ErrorActionPreference = 'Continue'
        & $nativeCommand.Source @Arguments 2>&1 | ForEach-Object {
            $line = ConvertTo-GscDiagnosticText ([string]$_) $Context
            $outputLines.Add($line)
            $writer.WriteLine($line)
            if ($EchoOutput) { Write-Host $line }
        }
        if ($null -ne $global:LASTEXITCODE) { $exitCode = $global:LASTEXITCODE }
    }
    catch {
        $launchError = ConvertTo-GscDiagnosticText $_.Exception.ToString() $Context
        $writer.WriteLine($launchError)
    }
    finally { $ErrorActionPreference = $previousErrorAction; $writer.Dispose() }
    $record = [ordered]@{
        id = $StepId; command = $FilePath; arguments = $Arguments
        startedUtc = $started.ToString('o'); completedUtc = [DateTime]::UtcNow.ToString('o')
        exitCode = $exitCode; console = [IO.Path]::GetFileName($logPath); launchError = $launchError
    }
    $Context.Steps.Add($record)
    Write-GscDiagnosticSummary $Context
    return [pscustomobject]@{ ExitCode = $exitCode; Output = $outputLines.ToArray() }
}

function Complete-GscTestDiagnostics {
    param([hashtable]$Context, [string]$ResultsDirectory)
    foreach ($trx in @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue)) {
        $text = ConvertTo-GscDiagnosticText ([IO.File]::ReadAllText($trx.FullName)) $Context
        [IO.File]::WriteAllText($trx.FullName, $text, [Text.UTF8Encoding]::new($false))
    }
}

function Complete-GscDiagnostics {
    param([hashtable]$Context, [bool]$Succeeded)
    if ($Context.TestsRequested) {
        $Context.TestSummary = Get-GscTestSummary $Context
        Write-Host ("TRX test cases: {0} total; {1} passed; {2} failed outcomes; {3} skipped; {4} errors/timeouts/aborts; {5} WPF isolated class runs." -f `
            $Context.TestSummary.totals.testCaseCount, $Context.TestSummary.totals.passed,
            $Context.TestSummary.totals.failed, $Context.TestSummary.totals.skipped,
            ($Context.TestSummary.totals.error + $Context.TestSummary.totals.timeout + $Context.TestSummary.totals.aborted),
            $Context.TestSummary.totals.wpfIsolatedClassRunCount) -ForegroundColor DarkCyan
    }
    $Context.Outcome = if ($Succeeded) { 'passed' } else { 'failed' }
    $Context.CompletedUtc = [DateTime]::UtcNow.ToString('o')
    Write-GscDiagnosticSummary $Context
}
