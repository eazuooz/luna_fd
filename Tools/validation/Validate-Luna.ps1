[CmdletBinding()]
param(
    [ValidateSet('Audit', 'Smoke', 'ContentMatrix', 'BuildWindows', 'All')]
    [string] $Mode = 'All',
    [string] $UnityPath,
    [int] $TimeoutMinutes = 30,
    [switch] $NoGraphics,
    [switch] $CleanBuildCache,
    [switch] $JobLeakDiagnostics,
    [switch] $RebuildShaderCache,
    [switch] $CheckNativeMemory,
    [ValidateSet('all', 'stages', 'dungeons', 'skins', 'actors', 'projectiles', 'skills', 'rewards', 'combos', 'ranks', 'lifecycle', 'heroes', 'motion', 'buffs', 'ownership', 'ui', 'skill-levels')][string] $MatrixSection = 'all',
    [string] $MatrixFilter = ''
)

$ErrorActionPreference = 'Stop'
if ($RebuildShaderCache -and $Mode -notin @('BuildWindows', 'All')) {
    throw '-RebuildShaderCache applies only to BuildWindows (or All including BuildWindows).'
}
. (Join-Path $PSScriptRoot 'Report-Time.ps1')
. (Join-Path $PSScriptRoot 'Warning-Policy.ps1')
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$versionLine = Get-Content -LiteralPath (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt') | Where-Object { $_ -like 'm_EditorVersion: *' } | Select-Object -First 1
$version = $versionLine.Split(':')[1].Trim()
if (-not $UnityPath) {
    $UnityPath = Join-Path ${env:ProgramFiles} "Unity/Hub/Editor/$version/Editor/Unity.exe"
}
if (-not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw "Unity $version not found at '$UnityPath'. Supply -UnityPath."
}
$existing = Get-CimInstance Win32_Process -Filter "name = 'Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.IndexOf($projectPath, [StringComparison]::OrdinalIgnoreCase) -ge 0
}
if ($existing) { throw "Close the Unity editor for $projectPath before running validation." }
$logDirectory = Join-Path $projectPath 'Logs/Revival'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$modes = if ($Mode -eq 'All') { @('Audit', 'Smoke', 'ContentMatrix', 'BuildWindows') } else { @($Mode) }
foreach ($step in $modes) {
    $operation = switch ($step) { 'Audit' { 'audit' }; 'Smoke' { 'smoke' }; 'ContentMatrix' { 'content-matrix' }; 'BuildWindows' { 'build-windows' } }
    $logPath = Join-Path $logDirectory "$operation.log"
    $reportPath = Join-Path $logDirectory "$operation.json"
    $cacheAction = $null
    if ($RebuildShaderCache -and $step -eq 'BuildWindows') {
        $cachePath = Join-Path $projectPath 'Library/ShaderCache'
        $cacheRunId = 'shader-cache-build-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
        if (Test-Path -LiteralPath $cachePath -PathType Container) {
            & (Join-Path $PSScriptRoot 'Memory/Backup-ShaderCache.ps1') -RunId $cacheRunId
            $cacheEvidence = Join-Path $projectPath "Logs/MemoryIsolation/$cacheRunId/shader-cache-move.json"
            $cacheAction = [ordered]@{ status='verified-reversible-backup'; source=$cachePath; evidence=$cacheEvidence }
            Write-Host "Opt-in cold-cache build workaround; reversible backup evidence: $cacheEvidence"
        }
        elseif (Test-Path -LiteralPath $cachePath) { throw "ShaderCache exists but is not a directory: $cachePath" }
        else {
            $cacheAction = [ordered]@{ status='already-absent'; source=$cachePath; evidence=$null }
            Write-Host "Opt-in cold-cache build: ShaderCache is already absent at $cachePath; nothing was moved."
        }
    }
    $method = if ($step -eq 'BuildWindows' -and $CleanBuildCache) { 'BuildWindowsClean' } else { $step }
    $arguments = @('-batchmode', '-projectPath', ('"' + $projectPath + '"'), '-executeMethod', "LunaValidation.$method", '-logFile', ('"' + $logPath + '"'))
    # Smoke must remain alive while play mode runs. Each method exits explicitly.
    if ($NoGraphics) { $arguments += '-nographics' }
    if ($JobLeakDiagnostics -and $step -eq 'BuildWindows') { $arguments += '-diag-job-temp-memory-leak-validation' }
    Write-Host "Running $step using Unity $version. Log: $logPath"
    $started = [DateTime]::UtcNow
    $oldSection = [Environment]::GetEnvironmentVariable('LUNA_MATRIX_SECTION', 'Process')
    $oldFilter = [Environment]::GetEnvironmentVariable('LUNA_MATRIX_FILTER', 'Process')
    try {
        if ($step -eq 'ContentMatrix') {
            [Environment]::SetEnvironmentVariable('LUNA_MATRIX_SECTION', $MatrixSection, 'Process')
            [Environment]::SetEnvironmentVariable('LUNA_MATRIX_FILTER', $MatrixFilter, 'Process')
        }
        $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WorkingDirectory $projectPath -WindowStyle Hidden -PassThru
    }
    finally {
        [Environment]::SetEnvironmentVariable('LUNA_MATRIX_SECTION', $oldSection, 'Process')
        [Environment]::SetEnvironmentVariable('LUNA_MATRIX_FILTER', $oldFilter, 'Process')
    }
    $timedOut = -not $process.WaitForExit($TimeoutMinutes * 60000)
    if ($timedOut) {
        Stop-Process -Id $process.Id -Force
        $process.WaitForExit()
    }
    $process.Refresh()
    if ($CheckNativeMemory) {
        # Preserve a unique closed-process snapshot before checking exit/report
        # success. A failed retry must never overwrite the previous evidence.
        $memoryRunId = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '-' + $operation + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
        $memoryDirectory = Join-Path $logDirectory "NativeMemory/$memoryRunId"
        New-Item -ItemType Directory -Path $memoryDirectory | Out-Null
        $closedLog = Join-Path $memoryDirectory "$operation.log"
        $memoryReport = Join-Path $memoryDirectory "$operation.memory.json"
        Copy-Item -LiteralPath $logPath -Destination $closedLog
        if (Test-Path -LiteralPath $reportPath) {
            Copy-Item -LiteralPath $reportPath -Destination (Join-Path $memoryDirectory "$operation.json")
        }
        [ordered]@{
            operation=$operation; processId=$process.Id; processStartedUtc=$started.ToString('o')
            processClosedUtc=[DateTime]::UtcNow.ToString('o'); exitCode=$process.ExitCode; timedOut=$timedOut
            unityPath=$UnityPath; arguments=$arguments; originalLogPath=$logPath; closedLogPath=$closedLog
            shaderCacheRebuildRequested=[bool]($RebuildShaderCache -and $step -eq 'BuildWindows')
            shaderCacheAction=$cacheAction
            reportNote='The managed report is a retained snapshot; freshness and passing status are checked separately after the native memory gate.'
        } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $memoryDirectory 'closed-process.json') -Encoding utf8
        try {
            $memoryAudit = & (Join-Path $PSScriptRoot 'Memory/Assert-MemoryLog.ps1') -LogPath $closedLog -OutputPath $memoryReport
        }
        finally {
            if (Test-Path -LiteralPath $memoryReport) {
                Copy-Item -LiteralPath $memoryReport -Destination (Join-Path $logDirectory "$operation.memory.json") -Force
            }
        }
        Write-Host "Closed-log explicit native memory gate passed; all $($memoryAudit.utpRecords.Count) UTP records remain in $memoryReport."
    }
    if ($timedOut) { throw "$step timed out after $TimeoutMinutes minutes. See $logPath" }
    if ($process.ExitCode -ne 0) { throw "$step failed with exit code $($process.ExitCode). See $logPath and $reportPath" }
    if (-not (Test-Path -LiteralPath $reportPath)) { throw "$step returned no validation report. See $logPath" }
    $report = Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json
    if (-not (Test-LunaFreshReport -Report $report -ProcessStartedUtc $started)) {
        throw "$step did not produce a fresh passing report. See $reportPath"
    }
    $warningAudit = Assert-LunaExpectedWarnings -Report $report -Context "$step report $reportPath"
    Write-Host "$step passed: $($report.checks.Count) checks, $($report.warnings.Count) warnings. Report: $reportPath"
}
