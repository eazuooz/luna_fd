function Assert-LunaExpectedWarnings {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [object] $Report,
        [string] $Context = 'Validation'
    )

    # Claims come from completed native regression assertions, not a global
    # message-prefix whitelist. Equal text repeated twice needs two claims.
    $observed = [Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
    $claimed = [Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
    foreach ($pair in @(@{ Values = @($Report.warnings); Counts = $observed }, @{ Values = @($Report.expectedWarnings); Counts = $claimed })) {
        foreach ($message in $pair.Values) {
            if ($null -eq $message) { continue }
            if ($message -isnot [string] -or [string]::IsNullOrWhiteSpace($message)) { throw "$Context contains a malformed warning or warning claim." }
            if ($pair.Counts.ContainsKey($message)) { $pair.Counts[$message]++ }
            else { $pair.Counts.Add($message, 1) }
        }
    }
    $mismatches = [Collections.Generic.List[string]]::new()
    foreach ($message in $observed.Keys) {
        $expected = if ($claimed.ContainsKey($message)) { $claimed[$message] } else { 0 }
        if ($observed[$message] -ne $expected) { $mismatches.Add("observed=$($observed[$message]), claimed=$expected`: $message") }
    }
    foreach ($message in $claimed.Keys) {
        if (-not $observed.ContainsKey($message)) { $mismatches.Add("observed=0, claimed=$($claimed[$message]): $message") }
    }
    if ($mismatches.Count -ne 0) { throw "$Context warning reconciliation failed: $($mismatches -join ' | ')" }
    if (@($Report.errors | Where-Object { $null -ne $_ }).Count -ne 0) { throw "$Context contains errors despite its reported success." }
    [pscustomobject]@{
        warningCount = (@($Report.warnings | Where-Object { $null -ne $_ })).Count
        expectedWarningCount = (@($Report.expectedWarnings | Where-Object { $null -ne $_ })).Count
        unexpectedWarningCount = 0
    }
}
