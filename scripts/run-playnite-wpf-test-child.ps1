[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SpecificationPath
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'build-diagnostics.ps1')
. (Join-Path $PSScriptRoot 'windows-desktop-process.ps1')

$specification = Get-Content -LiteralPath $SpecificationPath -Raw | ConvertFrom-Json
$result = $null
$started = [DateTime]::UtcNow
$inputBefore = ''
$windowStation = ''
$desktop = ''
$launchError = $null

try {
    $windowStation = [GscHiddenDesktopProcess]::CurrentWindowStation()
    $desktop = [GscHiddenDesktopProcess]::CurrentDesktop()
    $inputBefore = [GscHiddenDesktopProcess]::InputDesktop()
    if (-not [string]::Equals($windowStation, 'WinSta0', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Expected WPF test window station WinSta0; found '$windowStation'."
    }
    if ([string]::Equals($desktop, $inputBefore, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals($desktop, [string]$specification.ExpectedDesktop, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing WPF test execution on an unexpected or input desktop (current='$desktop', input='$inputBefore')."
    }

    $bounded = Invoke-GscBoundedProcess ([string]$specification.FilePath) @($specification.Arguments) ([int]$specification.TimeoutSeconds)
    $context = @{ RepoRoot = [string]$specification.RepositoryRoot }
    $cleanOutput = ConvertTo-GscDiagnosticText ([string]$bounded.Output) $context
    [IO.File]::WriteAllText([string]$specification.ConsolePath, $cleanOutput, [Text.UTF8Encoding]::new($false))

    $result = [ordered]@{
        exitCode = [int]$bounded.ExitCode
        timedOut = [bool]$bounded.TimedOut
        outputDrained = [bool]$bounded.OutputDrained
        testProcessId = [int]$bounded.ProcessId
        testProcessStartedUtc = [string]$bounded.ProcessStartedUtc
        testProcessExited = [bool]$bounded.ProcessExited
        startedUtc = $started.ToString('o')
        completedUtc = [DateTime]::UtcNow.ToString('o')
        windowStation = $windowStation
        desktop = $desktop
        expectedDesktop = [string]$specification.ExpectedDesktop
        inputDesktopBefore = $inputBefore
        inputDesktopAfter = [GscHiddenDesktopProcess]::InputDesktop()
        launchError = $null
    }
}
catch {
    $launchError = ConvertTo-GscDiagnosticText $_.Exception.ToString() @{ RepoRoot = [string]$specification.RepositoryRoot }
    [IO.File]::WriteAllText([string]$specification.ConsolePath, $launchError, [Text.UTF8Encoding]::new($false))
    $result = [ordered]@{
        exitCode = 1
        timedOut = $false
        outputDrained = $true
        testProcessId = $null
        testProcessStartedUtc = $null
        testProcessExited = $true
        startedUtc = $started.ToString('o')
        completedUtc = [DateTime]::UtcNow.ToString('o')
        windowStation = $windowStation
        desktop = $desktop
        expectedDesktop = [string]$specification.ExpectedDesktop
        inputDesktopBefore = $inputBefore
        inputDesktopAfter = $(try { [GscHiddenDesktopProcess]::InputDesktop() } catch { '' })
        launchError = $launchError
    }
}

[IO.File]::WriteAllText(
    [string]$specification.ResultPath,
    (ConvertTo-Json -InputObject $result -Depth 5),
    [Text.UTF8Encoding]::new($false))
exit ([int]$result.exitCode)
