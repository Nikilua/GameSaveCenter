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
        SourceFiles = $sourceFiles; Assemblies = @(); Outcome = 'running'; CompletedUtc = $null
        RunType = 'unspecified'; TestsRequested = $null; FailureMessage = $null
    }
    Write-GscDiagnosticSummary $context
    return $context
}

function Write-GscDiagnosticSummary {
    param([hashtable]$Context)
    $summary = [ordered]@{
        schemaVersion = 1; sourceCommit = $Context.SourceCommit
        workingTreeClean = $Context.DirtyPaths.Count -eq 0; dirtyPaths = $Context.DirtyPaths
        sdk = $Context.Sdk; startedUtc = $Context.StartedUtc; completedUtc = $Context.CompletedUtc
        outcome = $Context.Outcome; steps = @($Context.Steps.ToArray()); assemblies = $Context.Assemblies
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
            $row = [ordered]@{ file = $dll.Name; path = (ConvertTo-GscDiagnosticText $dll.FullName $Context); sha256 = (Get-FileHash $dll.FullName).Hash
                productVersion = $dll.VersionInfo.ProductVersion; bytes = $dll.Length; mvid = $null }
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
    $Context.Outcome = if ($Succeeded) { 'passed' } else { 'failed' }
    $Context.CompletedUtc = [DateTime]::UtcNow.ToString('o')
    Write-GscDiagnosticSummary $Context
}
