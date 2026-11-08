[CmdletBinding()]
param(
    [ValidateSet('All', 'Seed', 'Verify', 'Diagnostics')][string] $Mode = 'All',
    [ValidateSet('all', 'persistence', 'audio', 'input', 'render', 'soak')][string] $Section = 'all',
    [ValidateSet('backbuffer', 'offscreen')][string] $RenderMode = 'backbuffer',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string] $RunId = ('player-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')),
    [string] $PlayerPath,
    [ValidateRange(10, 7200)][int] $SoakSeconds = 300,
    [ValidateRange(1, 180)][int] $TimeoutMinutes = 20
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Report-Time.ps1')
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $PlayerPath) { $PlayerPath = Join-Path $projectPath 'Builds/Revival/Windows/LunaDefense.exe' }
if (-not (Test-Path -LiteralPath $PlayerPath -PathType Leaf)) { throw "Build the development player first: $PlayerPath" }
$runDirectory = Join-Path $projectPath "Logs/Revival/PlayerValidation/$RunId"
$saveDirectory = Join-Path $runDirectory 'Save'
$marker = Join-Path $saveDirectory '.luna-validation-isolated'
if ($Mode -eq 'Verify' -and -not (Test-Path -LiteralPath $marker)) { throw 'Verify requires a prior seed run with the same RunId.' }
if ((Test-Path -LiteralPath $saveDirectory) -and -not (Test-Path -LiteralPath $marker)) { throw 'Existing directory is not marked as an isolated validation save.' }
New-Item -ItemType Directory -Path $saveDirectory -Force | Out-Null
Set-Content -LiteralPath $marker -Value 'Dedicated development diagnostics save; never user progress.' -Encoding utf8
$modes = if ($Mode -eq 'All') { @('seed', 'verify') } else { @($Mode.ToLowerInvariant()) }
foreach ($probe in $modes) {
    $attemptDirectory = Join-Path $runDirectory ('attempts/' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '-' + $probe)
    New-Item -ItemType Directory -Path $attemptDirectory -Force | Out-Null
    $logPath = Join-Path $runDirectory "player-$probe.log"
    $reportPath = Join-Path $runDirectory "player-$probe.json"
    $arguments = @('-batchmode', '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', ('"' + $logPath + '"'))
    $environment = @{
        LUNA_VALIDATION_MODE = $probe
        LUNA_VALIDATION_DIRECTORY = $runDirectory
        LUNA_SAVE_DIRECTORY = $saveDirectory
        LUNA_PLAYER_SECTION = $Section
        LUNA_RENDER_MODE = $RenderMode
        LUNA_VALIDATION_SOAK_SECONDS = $SoakSeconds.ToString([Globalization.CultureInfo]::InvariantCulture)
    }
    $previous = @{}
    $started = [DateTime]::UtcNow
    $summary = [ordered]@{mode=$probe;section=$Section;renderMode=$RenderMode;startedUtc=(Format-LunaUtcTimestamp $started);finishedUtc=$null;success=$false;error=$null;files=@()}
    try {
        foreach ($name in $environment.Keys) { $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process'); [Environment]::SetEnvironmentVariable($name, $environment[$name], 'Process') }
        try {
            # Graphics/audio enabled, but no desktop interaction or foreground activation.
            $process = Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $projectPath -WindowStyle Hidden -PassThru
        }
        finally { foreach ($name in $previous.Keys) { [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process') } }
        Write-Host "Hidden development player $probe PID $($process.Id); evidence $attemptDirectory"
        if (-not $process.WaitForExit($TimeoutMinutes * 60000)) { Stop-Process -Id $process.Id -Force; throw "$probe timed out after $TimeoutMinutes minutes." }
        $process.Refresh()
        if (-not (Test-Path -LiteralPath $reportPath)) { throw "Player produced no $probe report; verify this is a development build with the diagnostic driver." }
        $report = Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json
        if ($process.ExitCode -ne 0 -or -not (Test-LunaFreshReport $report $started)) { throw "$probe failed or was unavailable; independent case results remain in $reportPath and $logPath" }
        $summary.success = $true
    }
    catch { $summary.error = $_.Exception.Message; throw }
    finally {
        foreach ($path in @($reportPath, $logPath)) {
            if (Test-Path -LiteralPath $path -PathType Leaf) {
                $file = Get-Item -LiteralPath $path
                if ($file.LastWriteTimeUtc -ge $started.AddSeconds(-2)) { Copy-Item -LiteralPath $path -Destination $attemptDirectory; $summary.files += $file.Name }
            }
        }
        foreach ($file in Get-ChildItem -LiteralPath $runDirectory -Filter 'render-*.png') {
            if ($file.LastWriteTimeUtc -ge $started.AddSeconds(-2)) { Copy-Item -LiteralPath $file.FullName -Destination $attemptDirectory; $summary.files += $file.Name }
        }
        $summary.finishedUtc = Format-LunaUtcTimestamp
        $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $attemptDirectory 'summary.json') -Encoding utf8
    }
}
