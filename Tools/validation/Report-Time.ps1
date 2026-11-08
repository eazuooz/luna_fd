function ConvertTo-LunaUtcTimestamp {
    [CmdletBinding()]
    [OutputType([DateTime])]
    param([Parameter(Mandatory)] [object] $Value)

    # ConvertFrom-Json returns DateTime in newer PowerShell versions, and strings
    # in older ones. Preserve typed values instead of losing their Kind by casting.
    if ($Value -is [DateTimeOffset]) { return $Value.UtcDateTime }
    if ($Value -is [DateTime]) {
        $timestamp = $Value
    }
    elseif ($Value -is [string]) {
        $timestamp = [DateTime]::Parse($Value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)
    }
    else { throw "Unsupported validation timestamp type: $($Value.GetType().FullName)" }

    # These fields explicitly contain UTC. An unqualified parsed timestamp must
    # not be silently interpreted as the machine's local timezone.
    if ($timestamp.Kind -eq [DateTimeKind]::Unspecified) {
        return [DateTime]::SpecifyKind($timestamp, [DateTimeKind]::Utc)
    }
    return $timestamp.ToUniversalTime()
}

function Format-LunaUtcTimestamp {
    [CmdletBinding()]
    [OutputType([string])]
    param([object] $Value = [DateTime]::UtcNow)

    return (ConvertTo-LunaUtcTimestamp $Value).ToString('o', [Globalization.CultureInfo]::InvariantCulture)
}

function Test-LunaFreshReport {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)] [object] $Report,
        [Parameter(Mandatory)] [DateTime] $ProcessStartedUtc,
        [DateTime] $ProcessFinishedUtc = [DateTime]::UtcNow
    )

    if ($Report.success -ne $true -or -not $Report.startedUtc -or -not $Report.finishedUtc) { return $false }
    try {
        $reportStarted = ConvertTo-LunaUtcTimestamp $Report.startedUtc
        $reportFinished = ConvertTo-LunaUtcTimestamp $Report.finishedUtc
        $earliest = (ConvertTo-LunaUtcTimestamp $ProcessStartedUtc).AddSeconds(-2)
        $latest = (ConvertTo-LunaUtcTimestamp $ProcessFinishedUtc).AddSeconds(2)
        return $reportStarted -ge $earliest -and $reportStarted -le $latest -and
            $reportFinished -ge $reportStarted -and $reportFinished -le $latest
    }
    catch { return $false }
}
