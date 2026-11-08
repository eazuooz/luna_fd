[CmdletBinding()]
param([Parameter(Mandatory)][string] $LogPath, [string] $OutputPath)
$ErrorActionPreference = 'Stop'
$resolved = (Resolve-Path -LiteralPath $LogPath).Path
$lines = [IO.File]::ReadAllLines($resolved)
$utp = [Collections.Generic.List[object]]::new()
$diagnostics = [Collections.Generic.List[object]]::new()
$allocations = [Collections.Generic.List[object]]::new()
$jobMessages = [Collections.Generic.List[int]]::new()
$explicitIssues = [Collections.Generic.List[object]]::new()
# These are engine diagnostics which explicitly report an ownership/lifetime
# failure. UTP's MemoryLeaks accounting record is deliberately a separate field:
# its presence alone does not identify an allocation owner or a leak rate.
$failurePatterns = [ordered]@{
    JobTempAlloc = '(?i)There are remaining Allocations on the JobTempAlloc|Internal:\s*JobTempAlloc has allocations'
    PersistentAllocation = '(?i)\bLeakDetectedPersistent\b|\bLeak Detected\s*:\s*Persistent\b'
    TemporaryAllocator = '(?i)\bTLS Allocator\b.*\bunfreed allocations\b'
    NativeCollection = '(?i)\bA Native Collection has not been disposed\b'
    ComputeBufferFinalizer = '(?i)\bGarbage\s*Collector\b.*\bdisposing of\s+ComputeBuffer\b|\bComputeBuffer\b.*\b(?:has not been disposed|was not disposed)\b'
}
for ($index = 0; $index -lt $lines.Length; $index++) {
    $line = $lines[$index]
    $marker = $line.IndexOf('##utp:', [StringComparison]::Ordinal)
    if ($marker -ge 0) {
        try { $record = $line.Substring($marker + 6) | ConvertFrom-Json }
        catch { throw "Malformed UTP JSON at line $($index + 1): $($_.Exception.Message)" }
        $utp.Add([ordered]@{ line=$index+1; record=$record })
    }
    # Preserve all candidates, including infrastructure text. This is an index,
    # not a suppression list or an inference that every matching word is an error.
    if ($line -match '(?i)warning|error|exception|leak|assert|fail|crash') {
        $diagnostics.Add([ordered]@{ line=$index+1; text=$line })
    }
    foreach ($kind in $failurePatterns.Keys) {
        if ($line -match $failurePatterns[$kind]) {
            $explicitIssues.Add([ordered]@{ kind=$kind; line=$index+1; text=$line })
        }
    }
    if ($line.Contains('There are remaining Allocations on the JobTempAlloc')) { $jobMessages.Add($index+1) }
    if ($line -match '^Allocation of (\d+) bytes at ([0-9a-fA-Fx]+) in block (\d+) \(frameIndex=(\d+), lifetime=(\d+), age=(\d+), state=([^\)]+)\)') {
        $allocation = [ordered]@{ line=$index+1; bytes=[long]$Matches[1]; address=$Matches[2]; block=[int]$Matches[3]; frameIndex=[int]$Matches[4]; lifetime=[int]$Matches[5]; age=[int]$Matches[6]; state=$Matches[7]; stack=@() }
        for ($next=$index+1; $next -lt $lines.Length -and $lines[$next] -match '^0x'; $next++) { $allocation.stack += $lines[$next] }
        $allocations.Add($allocation)
    }
}
$unique = @($allocations | Group-Object { "$($_.address):$($_.bytes):$($_.block)" } | ForEach-Object { $_.Group[0] })
$bytes = [long]0
foreach ($allocation in $unique) { $bytes += [long]$allocation['bytes'] }
$report = [ordered]@{
    logPath=$resolved; logSha256=(Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash.ToLowerInvariant(); lines=$lines.Length
    jobTempWarningLines=@($jobMessages); allocationRecords=@($allocations); uniqueJobAllocationCount=$unique.Count; uniqueJobAllocationBytes=$bytes
    explicitNativeMemoryIssues=@($explicitIssues); explicitNativeMemoryIssueCount=$explicitIssues.Count
    passedExplicitNativeMemoryGate=($explicitIssues.Count -eq 0)
    utpRecords=@($utp); diagnosticCandidates=@($diagnostics)
    interpretation='Raw post-exit evidence. UTP allocatedMemory and memoryLabels are retained independently; their ownership and accounting are not inferred. Zero managed warnings is not proof of zero native allocations.'
}
if ($OutputPath) { $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding utf8 }
else { $report | ConvertTo-Json -Depth 12 }
