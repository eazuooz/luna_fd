[CmdletBinding()]
param(
    [ValidateSet('Audit', 'Smoke', 'ContentMatrix', 'BuildWindows', 'All')]
    [string] $Mode = 'All',
    [string] $UnityPath,
    [int] $TimeoutMinutes = 30,
    [switch] $NoGraphics,
    [switch] $CleanBuildCache,
    [switch] $JobLeakDiagnostics,
    [ValidateSet('all', 'stages', 'skins', 'actors', 'projectiles', 'skills', 'rewards', 'combos', 'ranks', 'lifecycle')][string] $MatrixSection = 'all',
    [string] $MatrixFilter = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Report-Time.ps1')
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
$modes = if ($Mode -eq 'All') { @('Audit', 'Smoke', 'BuildWindows') } else { @($Mode) }
foreach ($step in $modes) {
    $operation = switch ($step) { 'Audit' { 'audit' }; 'Smoke' { 'smoke' }; 'ContentMatrix' { 'content-matrix' }; 'BuildWindows' { 'build-windows' } }
    $logPath = Join-Path $logDirectory "$operation.log"
    $reportPath = Join-Path $logDirectory "$operation.json"
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
    if (-not $process.WaitForExit($TimeoutMinutes * 60000)) {
        Stop-Process -Id $process.Id -Force
        throw "$step timed out after $TimeoutMinutes minutes. See $logPath"
    }
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "$step failed with exit code $($process.ExitCode). See $logPath and $reportPath" }
    if (-not (Test-Path -LiteralPath $reportPath)) { throw "$step returned no validation report. See $logPath" }
    $report = Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json
    if (-not (Test-LunaFreshReport -Report $report -ProcessStartedUtc $started)) {
        throw "$step did not produce a fresh passing report. See $reportPath"
    }
    Write-Host "$step passed: $($report.checks.Count) checks, $($report.warnings.Count) warnings. Report: $reportPath"
}
