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
. (Join-Path $PSScriptRoot 'Warning-Policy.ps1')
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
        source = $null
        steps = @()
    }
    $fingerprintTool = Join-Path $PSScriptRoot 'source_fingerprint.py'
    $before = & python $fingerprintTool (Join-Path $roundDirectory 'source-before.json') | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'Could not fingerprint Unity source before review.' }
    # A RunId denotes three reviews of the same implementation, including when
    # passes are invoked separately. A changed implementation needs a new RunId.
    $priorSummaries = Get-ChildItem -LiteralPath $runDirectory -Filter 'summary.json' -Recurse -File
    foreach ($priorFile in $priorSummaries) {
        $prior = Get-Content -LiteralPath $priorFile.FullName -Raw | ConvertFrom-Json
        if ($prior.success -and ($prior.source.sha256 -ne $before.sha256 -or $prior.source.algorithmVersion -ne $before.algorithmVersion)) {
            throw "Source differs from a completed pass in '$RunId'. Start a new RunId and run all three passes."
        }
    }
    $summary.source = $before
    $steps = @('Audit', 'Smoke', 'ContentMatrix')
    if ($round -eq 3) { $steps += @('BuildWindows', 'Player') }
    try {
        foreach ($step in $steps) {
            $operation = switch ($step) { 'Audit' { 'audit' }; 'Smoke' { 'smoke' }; 'ContentMatrix' { 'content-matrix' }; 'BuildWindows' { 'build-windows' }; 'Player' { 'player' } }
            $started = [DateTime]::UtcNow
            $entry = [ordered]@{ operation = $operation; startedUtc = (Format-LunaUtcTimestamp $started); finishedUtc = $null; success = $false; error = $null; warningAudits = @(); files = @() }
            $summary.steps += $entry
            $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
            $arguments = @{ Mode = $step; TimeoutMinutes = $TimeoutMinutes }
            if ($UnityPath) { $arguments.UnityPath = $UnityPath }
            if ($NoGraphics) { $arguments.NoGraphics = $true }
            if ($CleanBuildCache -and $step -eq 'BuildWindows') { $arguments.CleanBuildCache = $true }
            if ($JobLeakDiagnostics -and $step -eq 'BuildWindows') { $arguments.JobLeakDiagnostics = $true }
            Write-Host "Final review pass $round / $step"
            $playerEvidence = $null
            try {
                if ($step -eq 'Player') {
                    $playerRun = "$RunId-pass03-$attempt"
                    $playerEvidence = Join-Path $sourceDirectory "PlayerValidation/$playerRun"
                    # Exercise the exact executable just built, with hidden graphics/audio
                    # and offscreen capture. No desktop input or foreground window is used.
                    & (Join-Path $PSScriptRoot 'Validate-Player.ps1') -Mode All -Section all -RenderMode offscreen -RunId $playerRun -SoakSeconds 300 -TimeoutMinutes $TimeoutMinutes
                    foreach ($probe in @('seed', 'verify')) {
                        $probeReport = Get-Content -LiteralPath (Join-Path $playerEvidence "player-$probe.json") -Raw | ConvertFrom-Json
                        $entry.warningAudits += Assert-LunaExpectedWarnings -Report $probeReport -Context "Final pass $round / Player $probe"
                    }
                }
                else {
                    # Validate-Luna starts and waits for one new editor process per step.
                    & $runner @arguments
                    $stepReport = Get-Content -LiteralPath (Join-Path $sourceDirectory "$operation.json") -Raw | ConvertFrom-Json
                    $entry.warningAudits += Assert-LunaExpectedWarnings -Report $stepReport -Context "Final pass $round / $step"
                }
                $entry.success = $true
            }
            catch {
                $entry.error = $_.Exception.Message
                throw
            }
            finally {
                if ($playerEvidence -and (Test-Path -LiteralPath $playerEvidence)) {
                    Copy-Item -LiteralPath $playerEvidence -Destination (Join-Path $roundDirectory 'PlayerValidation') -Recurse
                    $entry.files += 'PlayerValidation'
                }
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
                if ($step -eq 'Smoke' -or $step -eq 'ContentMatrix') {
                    $runtime = Join-Path $sourceDirectory "native-runtime-$operation.json"
                    if ((Test-Path -LiteralPath $runtime) -and (Get-Item -LiteralPath $runtime).LastWriteTimeUtc -ge $started.AddSeconds(-2)) {
                        Copy-Item -LiteralPath $runtime -Destination $roundDirectory
                        $entry.files += [IO.Path]::GetFileName($runtime)
                    }
                }
                $entry.finishedUtc = Format-LunaUtcTimestamp
                $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
            }
        }
        $after = & python $fingerprintTool (Join-Path $roundDirectory 'source-after.json') | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0 -or $before.sha256 -ne $after.sha256 -or $before.algorithmVersion -ne $after.algorithmVersion) { throw 'Unity source changed during this review; stabilize the source and repeat this pass.' }
        $summary.success = $true
    }
    finally {
        $summary.finishedUtc = Format-LunaUtcTimestamp
        $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
        Write-Host "Pass $round evidence: $roundDirectory"
    }
}
