[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string] $RunId,
    [Parameter(Mandatory)][ValidateSet('Build','Player')][string] $Mode,
    [ValidateSet('empty','builtin-shader')][string] $Scene = 'empty',
    [string] $ShaderName = '',
    [switch] $NoGraphicsEditor,
    [switch] $NoGraphicsPlayer,
    [switch] $CaptureSnapshot,
    [ValidateRange(1,300)][int] $Seconds = 3,
    [ValidateRange(1,60)][int] $TimeoutMinutes = 15,
    [string] $UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe'
)
$ErrorActionPreference = 'Stop'
$repository = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..')).Path
$runDirectory = Join-Path $repository "Logs/MemoryIsolation/$RunId"
$projectDirectory = Join-Path $runDirectory 'Project'
if (-not (Test-Path -LiteralPath (Join-Path $projectDirectory '.memory-isolation-project'))) { throw 'Create the standalone project first with New-MemoryIsolation.ps1.' }
$created = Get-Content -Raw -LiteralPath (Join-Path $runDirectory 'created.json') | ConvertFrom-Json
$pipeline = if ($created.pipeline) { $created.pipeline } else { 'builtin' }
$attempt = Join-Path $runDirectory ('attempts/' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '-' + $Mode.ToLowerInvariant())
[void](New-Item -ItemType Directory -Path $attempt -Force)
$log = Join-Path $attempt 'process.log'
$report = Join-Path $attempt 'operation.json'
$player = Join-Path $runDirectory 'Build/MemoryIsolation.exe'
$arguments = @('-batchmode', '-logFile', ('"'+$log+'"'), '-stackTraceLogType', 'Full', '-native-leak-detection', 'EnabledWithStackTrace', '-diag-job-temp-memory-leak-validation', '-diag-temp-memory-leak-validation')
$environment = @{}
if ($Mode -eq 'Build') {
    if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity is missing: $UnityPath" }
    $live = Get-CimInstance Win32_Process -Filter "name = 'Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($projectDirectory,[StringComparison]::OrdinalIgnoreCase) -ge 0 }
    if ($live) { throw 'A Unity process already owns the isolated project.' }
    $exe = $UnityPath
    $buildMethod = if ($pipeline -eq 'urp') { 'MemoryIsolationUrpBuilder.Build' } else { 'MemoryIsolationBuilder.Build' }
    $arguments += @('-projectPath', ('"'+$projectDirectory+'"'), '-executeMethod', $buildMethod, '-log-memory-performance-stats')
    if ($NoGraphicsEditor) { $arguments += '-nographics' }
    $environment = @{ MEMORY_ISOLATION_DIRECTORY=$attempt; MEMORY_ISOLATION_PLAYER=$player; MEMORY_ISOLATION_SCENE=$Scene; MEMORY_ISOLATION_PIPELINE=$pipeline; MEMORY_ISOLATION_SHADER=$ShaderName; UNITY_JOBS_NATIVE_LEAK_DETECTION_MODE='2' }
}
else {
    if (-not (Test-Path -LiteralPath $player)) { throw 'Build this isolated project first.' }
    $exe = $player
    $arguments += @('-screen-fullscreen','0','-screen-width','320','-screen-height','180','-memory-isolation-directory',('"'+$attempt+'"'),'-memory-isolation-seconds',$Seconds.ToString([Globalization.CultureInfo]::InvariantCulture))
    if ($NoGraphicsPlayer) { $arguments += '-nographics' }
    if ($CaptureSnapshot) { $arguments += '-memory-isolation-snapshot' }
}
$sourceRoots = @('Assets','Packages','ProjectSettings') | ForEach-Object { Join-Path $projectDirectory $_ }
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoots -Recurse -File | ForEach-Object {
    [ordered]@{ path=$_.FullName.Substring($projectDirectory.Length+1).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
})
$summary = [ordered]@{ mode=$Mode; scene=$Scene; pipeline=$pipeline; requestedShader=$ShaderName; urpVersion=$created.urpVersion; unityVersion=$created.unityVersion; executable=$exe; executableSha256=(Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant(); arguments=$arguments; sourceFiles=$sourceFiles; startedUtc=[DateTime]::UtcNow.ToString('o'); finishedUtc=$null; processId=$null; exitCode=$null; operationCompleted=$false; error=$null }
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $attempt 'launch.json') -Encoding utf8
Write-Host ('Launching hidden: "'+$exe+'" '+($arguments -join ' '))
$previous=@{}
try {
    foreach($key in $environment.Keys) { $previous[$key]=[Environment]::GetEnvironmentVariable($key,'Process'); [Environment]::SetEnvironmentVariable($key,$environment[$key],'Process') }
    try { $process=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $projectDirectory -WindowStyle Hidden -PassThru }
    finally { foreach($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key],'Process') } }
    $summary.processId=$process.Id
    if (-not $process.WaitForExit($TimeoutMinutes*60000)) { Stop-Process -Id $process.Id -Force; throw 'The isolated process exceeded its timeout.' }
    $process.Refresh(); $summary.exitCode=$process.ExitCode
    if (-not (Test-Path -LiteralPath $report)) { throw 'The isolated process did not produce operation.json. Inspect the unfiltered process log.' }
    $operation=Get-Content -Raw -LiteralPath $report | ConvertFrom-Json
    $summary.operationCompleted=($process.ExitCode -eq 0 -and $operation.completed -eq $true)
    if (-not $summary.operationCompleted) { throw 'The isolated operation failed; raw diagnostics are retained.' }
}
catch { $summary.error=$_.Exception.Message; throw }
finally {
    if(Test-Path -LiteralPath $log) { & (Join-Path $PSScriptRoot 'Read-MemoryLog.ps1') -LogPath $log -OutputPath (Join-Path $attempt 'closed-log-evidence.json') }
    $summary.finishedUtc=[DateTime]::UtcNow.ToString('o')
    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $attempt 'summary.json') -Encoding utf8
    Write-Host "Evidence: $attempt. Operation completion does not assert leak-free shutdown."
}
