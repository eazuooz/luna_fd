[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $WarmLog,
    [Parameter(Mandatory)][string] $ColdLog,
    [Parameter(Mandatory)][string] $PlayerLog,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot ('.check/gate-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff')))
)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $OutputDirectory) { throw "Test evidence directory must be new: $OutputDirectory" }
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$results = [Collections.Generic.List[object]]::new()
$gate = Join-Path $PSScriptRoot 'Assert-MemoryLog.ps1'

function Invoke-Case([string] $Name, [string] $InputLog, [bool] $ShouldPass) {
    $output = Join-Path $OutputDirectory "$Name.memory.json"
    $caught = $null
    try { $null = & $gate -LogPath $InputLog -OutputPath $output }
    catch { $caught = $_.Exception.Message }
    if (-not (Test-Path -LiteralPath $output)) { throw "$Name lost diagnostic evidence: $caught" }
    $report = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
    if (($null -eq $caught) -ne $ShouldPass) { throw "$Name gate outcome differed from expected pass=$ShouldPass; error=$caught" }
    if (-not $ShouldPass -and $report.explicitNativeMemoryIssueCount -eq 0) { throw "$Name failed for a reason other than the explicit memory gate: $caught" }
    $results.Add([ordered]@{ name=$Name; expectedPass=$ShouldPass; gatePassed=($null -eq $caught); diagnosticCount=$report.explicitNativeMemoryIssueCount; exception=$caught; evidence=$output })
    return $report
}

# These are real completed engine logs, not mocks of the allocation parser.
$warm = Invoke-Case 'actual-warm-shader-cache' $WarmLog $false
if ($warm.uniqueJobAllocationCount -ne 5 -or $warm.uniqueJobAllocationBytes -ne 1366) { throw 'Warm-build native stack/allocation evidence changed; inspect it rather than accepting a generic failure.' }
$cold = Invoke-Case 'actual-cold-shader-cache' $ColdLog $true
$player = Invoke-Case 'actual-player-utp-accounting' $PlayerLog $true
$utpLeaks = @($player.utpRecords | Where-Object { $_.record.type -eq 'MemoryLeaks' -and $_.record.allocatedMemory -gt 0 })
if ($utpLeaks.Count -eq 0) { throw 'Player fixture did not prove positive UTP shutdown accounting remains present on a narrow-gate pass.' }

$synthetic = [ordered]@{
    'undisposed-native-collection' = 'A Native Collection has not been disposed, resulting in a memory leak. Allocated from:'
    'persistent-allocation' = 'Leak Detected : Persistent allocates 1 individual allocations.'
    'persistent-callback' = 'Unity.Collections.LowLevel.Unsafe.UnsafeUtility:LeakDetectedPersistent()'
    'temporary-tls' = 'TLS Allocator ALLOC_TEMP_TLS, underlying allocator ALLOC_TEMP_MAIN has unfreed allocations, size 584'
    'computebuffer-finalizer' = 'GarbageCollector disposing of ComputeBuffer. Please use ComputeBuffer.Release() or .Dispose() to manually release the buffer.'
    'old-job-lifetime' = 'Internal: JobTempAlloc has allocations that are more than 4 frames old - this is not allowed and likely a leak'
}
foreach ($name in $synthetic.Keys) {
    $syntheticLog = Join-Path $OutputDirectory "$name.log"
    [IO.File]::WriteAllText($syntheticLog, $synthetic[$name] + [Environment]::NewLine)
    $null = Invoke-Case $name $syntheticLog $false
}
$neutral = Join-Path $OutputDirectory 'no-explicit-leak.log'
[IO.File]::WriteAllText($neutral, 'Checking for leaked weakptr:' + [Environment]::NewLine + '  Found no leaked weakptrs.' + [Environment]::NewLine)
$null = Invoke-Case 'no-explicit-leak' $neutral $true
$summary = [ordered]@{ success=$true; finishedUtc=[DateTime]::UtcNow.ToString('o'); launchedUnity=$false; cases=@($results); retainedPlayerUtpAllocatedMemory=@($utpLeaks | ForEach-Object { $_.record.allocatedMemory }) }
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'test-results.json') -Encoding utf8
Write-Host "Memory gate regression passed $($results.Count) cases without launching Unity. Evidence: $OutputDirectory"
