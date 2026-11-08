[CmdletBinding()]
param(
    [ValidateSet(1, 2, 3)]
    [int[]] $Pass = @(1, 2, 3),
    [ValidatePattern('^[A-Za-z0-9_-]+$')]
    [string] $RunId = ('final-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')),
    [string] $UnityPath,
    [ValidateRange(1, 180)]
    [int] $TimeoutMinutes = 30,
    [switch] $NoGraphics,
    [switch] $CleanBuildCache,
    [switch] $JobLeakDiagnostics
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Report-Time.ps1')
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$sourceDirectory = Join-Path $projectPath 'Logs/Revival'
$runDirectory = Join-Path $sourceDirectory "FinalReview/$RunId"
$runner = Join-Path $PSScriptRoot 'Validate-Luna.ps1'
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null

foreach ($round in ($Pass | Select-Object -Unique)) {
    $attempt = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff')
    $roundDirectory = Join-Path $runDirectory ('pass-{0:D2}/{1}' -f $round, $attempt)
    New-Item -ItemType Directory -Path $roundDirectory -Force | Out-Null
    $summaryPath = Join-Path $roundDirectory 'summary.json'
    $summary = [ordered]@{
        runId = $RunId
        pass = $round
        startedUtc = Format-LunaUtcTimestamp
        finishedUtc = $null
        success = $false
        steps = @()
    }
    $steps = @('Audit', 'Smoke')
    if ($round -eq 3) { $steps += 'BuildWindows' }
    try {
        foreach ($step in $steps) {
            $operation = switch ($step) { 'Audit' { 'audit' }; 'Smoke' { 'smoke' }; 'BuildWindows' { 'build-windows' } }
            $started = [DateTime]::UtcNow
            $entry = [ordered]@{ operation = $operation; startedUtc = (Format-LunaUtcTimestamp $started); finishedUtc = $null; success = $false; error = $null; files = @() }
            $summary.steps += $entry
            $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
            $arguments = @{ Mode = $step; TimeoutMinutes = $TimeoutMinutes }
            if ($UnityPath) { $arguments.UnityPath = $UnityPath }
            if ($NoGraphics) { $arguments.NoGraphics = $true }
            if ($CleanBuildCache -and $step -eq 'BuildWindows') { $arguments.CleanBuildCache = $true }
            if ($JobLeakDiagnostics -and $step -eq 'BuildWindows') { $arguments.JobLeakDiagnostics = $true }
            Write-Host "Final review pass $round / $step"
            try {
                # Validate-Luna starts and waits for one new editor process per step.
                & $runner @arguments
                $entry.success = $true
            }
            catch {
                $entry.error = $_.Exception.Message
                throw
            }
            finally {
                foreach ($extension in @('json', 'log')) {
                    $source = Join-Path $sourceDirectory "$operation.$extension"
                    if (Test-Path -LiteralPath $source -PathType Leaf) {
                        $file = Get-Item -LiteralPath $source
                        # Do not attach a stale report if compilation failed before the execute method.
                        if ($file.LastWriteTimeUtc -ge $started.AddSeconds(-2)) {
                            $destination = Join-Path $roundDirectory $file.Name
                            Copy-Item -LiteralPath $source -Destination $destination
                            $entry.files += $file.Name
                        }
                    }
                }
                $entry.finishedUtc = Format-LunaUtcTimestamp
                $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
            }
        }
        $summary.success = $true
    }
    finally {
        $summary.finishedUtc = Format-LunaUtcTimestamp
        $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
        Write-Host "Pass $round evidence: $roundDirectory"
    }
}
