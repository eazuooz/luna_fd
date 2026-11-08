[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $LogPath,
    [Parameter(Mandatory)][string] $OutputPath
)

$ErrorActionPreference = 'Stop'
# The caller must wait for process exit first. Always persist all parsed evidence
# before raising an error, including UTP accounting and complete native stacks.
& (Join-Path $PSScriptRoot 'Read-MemoryLog.ps1') -LogPath $LogPath -OutputPath $OutputPath
$report = Get-Content -LiteralPath $OutputPath -Raw | ConvertFrom-Json
if ($null -eq $report.explicitNativeMemoryIssueCount) {
    throw "Native memory parser did not produce the required gate result: $OutputPath"
}
if ($report.explicitNativeMemoryIssueCount -gt 0) {
    $kinds = @($report.explicitNativeMemoryIssues | ForEach-Object kind | Sort-Object -Unique)
    throw "Explicit native memory diagnostics failed ($($report.explicitNativeMemoryIssueCount) lines: $($kinds -join ', ')). Raw log: $LogPath; evidence: $OutputPath"
}
return $report
