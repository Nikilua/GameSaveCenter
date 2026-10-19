function Get-GscPackageRelevantDirtyFiles {
    [CmdletBinding()]
    param([AllowEmptyCollection()][string[]]$StatusLines)

    $relevantFiles = [System.Collections.Generic.List[string]]::new()
    foreach ($statusLine in $StatusLines) {
        if ([string]::IsNullOrWhiteSpace($statusLine)) { continue }
        if ($statusLine.Length -lt 4) {
            $relevantFiles.Add($statusLine)
            continue
        }

        $statusCode = $statusLine.Substring(0, 2)
        $path = $statusLine.Substring(3).Trim('"').Replace('\', '/')
        $isGeneratedEvidence = $statusCode -eq '??' -and (
            $path -match '^docs/.+/evidence/' -or
            $path -match '^tests/(?:[^/]+/)*TestResults(?:/|$)'
        )
        if (-not $isGeneratedEvidence) {
            $relevantFiles.Add($statusLine)
        }
    }

    return $relevantFiles.ToArray()
}
