[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$BaselineRoot,
    [Parameter(Mandatory = $true)][string]$CandidateRoot,
    [Parameter(Mandatory = $true)][string]$EvidenceRoot,
    [Parameter(Mandatory = $true)][string]$BaselineLabel,
    [Parameter(Mandatory = $true)][string]$CandidateLabel
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$repositoryPrefix = $repositoryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar

function Resolve-RepositoryPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $combined = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $repositoryRoot $Path }
    $fullPath = [System.IO.Path]::GetFullPath($combined)
    if (-not $fullPath.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path must remain inside the repository: $fullPath"
    }
    return $fullPath
}

function Assert-NoRepositoryReparsePoint {
    param([Parameter(Mandatory = $true)][string]$Path)

    $current = [System.IO.Path]::GetFullPath($Path)
    while ($current.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force -ErrorAction Stop
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Repository evidence paths cannot traverse a reparse point: $current"
            }
        }

        $parent = [System.IO.Path]::GetDirectoryName($current)
        if ([string]::IsNullOrWhiteSpace($parent) -or
            [string]::Equals($parent.TrimEnd([System.IO.Path]::DirectorySeparatorChar), $repositoryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar), [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        $current = $parent
    }
}

$baselinePath = Resolve-RepositoryPath $BaselineRoot
$candidatePath = Resolve-RepositoryPath $CandidateRoot
$evidencePath = Resolve-RepositoryPath $EvidenceRoot
foreach ($path in @($baselinePath, $candidatePath, $evidencePath)) {
    Assert-NoRepositoryReparsePoint $path
}
foreach ($inputPath in @($baselinePath, $candidatePath)) {
    if (-not (Test-Path -LiteralPath $inputPath -PathType Container)) {
        throw "Render input directory does not exist: $inputPath"
    }
}

$baselineSnapshot = Join-Path $baselinePath 'task-sample-snapshot.tsv'
$candidateSnapshot = Join-Path $candidatePath 'task-sample-snapshot.tsv'
$requiredInputPaths = @(
    $baselineSnapshot,
    $candidateSnapshot,
    (Join-Path $baselinePath 'task-evidence-report.txt'),
    (Join-Path $candidatePath 'task-evidence-report.txt'),
    (Join-Path $baselinePath 'runner-metadata.json'),
    (Join-Path $candidatePath 'runner-metadata.json')
)
foreach ($name in @('1040x700', '1600x900')) {
    $fileName = "task-dark-$name.png"
    $requiredInputPaths += Join-Path $baselinePath $fileName
    $requiredInputPaths += Join-Path $candidatePath $fileName
}
foreach ($inputFile in $requiredInputPaths) {
    Assert-NoRepositoryReparsePoint $inputFile
    if (-not (Test-Path -LiteralPath $inputFile -PathType Leaf)) {
        throw "Required render evidence input is missing: $inputFile"
    }
}

if (Test-Path -LiteralPath $evidencePath) {
    if (-not (Test-Path -LiteralPath $evidencePath -PathType Container)) {
        throw "Evidence output path is not a directory: $evidencePath"
    }
    $existingItems = @(Get-ChildItem -LiteralPath $evidencePath -Force -ErrorAction Stop)
    if ($existingItems.Count -gt 0) {
        throw "Evidence directory must be empty before composition: $evidencePath"
    }
}
else {
    New-Item -ItemType Directory -Path $evidencePath | Out-Null
}

$baselineSnapshotHash = (Get-FileHash -LiteralPath $baselineSnapshot -Algorithm SHA256).Hash
$candidateSnapshotHash = (Get-FileHash -LiteralPath $candidateSnapshot -Algorithm SHA256).Hash
if ($baselineSnapshotHash -ne $candidateSnapshotHash) {
    throw "Task fixture snapshots differ: baseline=$baselineSnapshotHash candidate=$candidateSnapshotHash"
}

Add-Type -AssemblyName System.Drawing
$names = @('1040x700', '1600x900')
$margin = 24
$gutter = 24
$headerHeight = 46
$manifest = [ordered]@{
    schemaVersion = 1
    theme = 'dark'
    dpiScale = '1.00 logical DIP; offscreen WPF'
    baselineLabel = $BaselineLabel
    candidateLabel = $CandidateLabel
    windows = @()
}

foreach ($name in $names) {
    $fileName = "task-dark-$name.png"
    $baselineInput = Join-Path $baselinePath $fileName
    $candidateInput = Join-Path $candidatePath $fileName

    $baselineImage = [System.Drawing.Image]::FromFile($baselineInput)
    $candidateImage = [System.Drawing.Image]::FromFile($candidateInput)
    $canvas = $null
    $graphics = $null
    $labelFont = $null
    try {
        if ($baselineImage.Width -ne $candidateImage.Width -or $baselineImage.Height -ne $candidateImage.Height) {
            throw "Before/after dimensions differ for $name: $($baselineImage.Width)x$($baselineImage.Height) vs $($candidateImage.Width)x$($candidateImage.Height)."
        }

        $canvasWidth = ($margin * 2) + $baselineImage.Width + $gutter + $candidateImage.Width
        $canvasHeight = ($margin * 2) + $headerHeight + $baselineImage.Height
        $canvas = [System.Drawing.Bitmap]::new($canvasWidth, $canvasHeight)
        $graphics = [System.Drawing.Graphics]::FromImage($canvas)
        $graphics.Clear([System.Drawing.Color]::FromArgb(244, 246, 249))
        $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
        $labelFont = [System.Drawing.Font]::new('Segoe UI', 12, [System.Drawing.FontStyle]::Bold)
        $textBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(36, 42, 52))
        try {
            $graphics.DrawString($BaselineLabel, $labelFont, $textBrush, [single]$margin, [single]10)
            $graphics.DrawString($CandidateLabel, $labelFont, $textBrush, [single]($margin + $baselineImage.Width + $gutter), [single]10)
            $graphics.DrawImage($baselineImage, $margin, $margin + $headerHeight, $baselineImage.Width, $baselineImage.Height)
            $graphics.DrawImage($candidateImage, $margin + $baselineImage.Width + $gutter, $margin + $headerHeight, $candidateImage.Width, $candidateImage.Height)
        }
        finally {
            $textBrush.Dispose()
        }

        $baselineCopy = "baseline-$fileName"
        $candidateCopy = "candidate-$fileName"
        Copy-Item -LiteralPath $baselineInput -Destination (Join-Path $evidencePath $baselineCopy)
        Copy-Item -LiteralPath $candidateInput -Destination (Join-Path $evidencePath $candidateCopy)
        $comparisonName = "task-dark-$name-before-after.png"
        $comparisonPath = Join-Path $evidencePath $comparisonName
        $canvas.Save($comparisonPath, [System.Drawing.Imaging.ImageFormat]::Png)

        $manifest.windows += [ordered]@{
            windowDip = $name
            baseline = [ordered]@{ file = $baselineCopy; sha256 = (Get-FileHash -LiteralPath (Join-Path $evidencePath $baselineCopy) -Algorithm SHA256).Hash }
            candidate = [ordered]@{ file = $candidateCopy; sha256 = (Get-FileHash -LiteralPath (Join-Path $evidencePath $candidateCopy) -Algorithm SHA256).Hash }
            comparison = [ordered]@{ file = $comparisonName; width = $canvasWidth; height = $canvasHeight; sha256 = (Get-FileHash -LiteralPath $comparisonPath -Algorithm SHA256).Hash }
        }
    }
    finally {
        if ($labelFont) { $labelFont.Dispose() }
        if ($graphics) { $graphics.Dispose() }
        if ($canvas) { $canvas.Dispose() }
        $baselineImage.Dispose()
        $candidateImage.Dispose()
    }
}

Copy-Item -LiteralPath $baselineSnapshot -Destination (Join-Path $evidencePath 'task-sample-snapshot.tsv')
Copy-Item -LiteralPath (Join-Path $baselinePath 'task-evidence-report.txt') -Destination (Join-Path $evidencePath 'baseline-task-evidence-report.txt')
Copy-Item -LiteralPath (Join-Path $candidatePath 'task-evidence-report.txt') -Destination (Join-Path $evidencePath 'candidate-task-evidence-report.txt')
Copy-Item -LiteralPath (Join-Path $baselinePath 'runner-metadata.json') -Destination (Join-Path $evidencePath 'baseline-runner-metadata.json')
Copy-Item -LiteralPath (Join-Path $candidatePath 'runner-metadata.json') -Destination (Join-Path $evidencePath 'candidate-runner-metadata.json')
$manifest.baselineSnapshotSha256 = $baselineSnapshotHash
$manifest.candidateSnapshotSha256 = $candidateSnapshotHash
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $evidencePath 'manifest.json') -Encoding utf8
Write-Host "Task comparison evidence created: $evidencePath" -ForegroundColor Green
