[CmdletBinding()]
param([Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string] $RunId)

$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..')).Path.TrimEnd('\','/')
$source = [IO.Path]::GetFullPath((Join-Path $workspace 'Library/ShaderCache'))
$runDirectory = [IO.Path]::GetFullPath((Join-Path $workspace "Logs/MemoryIsolation/$RunId"))
$destination = [IO.Path]::GetFullPath((Join-Path $runDirectory 'ShaderCache-before'))
$comparison = [StringComparison]::OrdinalIgnoreCase

function Assert-WorkspacePath([string] $Path) {
    $absolute = [IO.Path]::GetFullPath($Path).TrimEnd('\','/')
    if (-not $absolute.StartsWith($workspace + [IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw "Path is outside the intended workspace: $absolute"
    }
    return $absolute
}

function Assert-NoReparseAncestors([string] $Path) {
    $cursor = [IO.Path]::GetFullPath($Path).TrimEnd('\','/')
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Reparse points are not allowed in the cache backup path: $cursor"
            }
        }
        if ($cursor.Equals($workspace, $comparison)) { break }
        $parent = [IO.Path]::GetDirectoryName($cursor)
        if ($parent -eq $cursor) { break }
        $cursor = $parent
    }
}

function Assert-LunaEditorClosed {
    $busy = [Collections.Generic.List[object]]::new()
    foreach ($process in Get-CimInstance Win32_Process -Filter "name = 'Unity.exe'") {
        if ([string]::IsNullOrWhiteSpace($process.CommandLine)) {
            throw "Cannot inspect Unity process $($process.ProcessId); refusing to move a cache while its project ownership is unknown."
        }
        $match = [regex]::Match($process.CommandLine, '(?i)(?:^|\s)-projectPath\s+(?:"([^"]+)"|(\S+))')
        if (-not $match.Success) { continue }
        $projectArgument = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
        # An absolute path can be compared without guessing another process's cwd.
        if (-not [IO.Path]::IsPathRooted($projectArgument)) {
            throw "Unity process $($process.ProcessId) uses a relative project path; ownership cannot be safely resolved."
        }
        $project = [IO.Path]::GetFullPath($projectArgument).TrimEnd('\','/')
        if ($project.Equals($workspace, $comparison)) { $busy.Add($process) }
    }
    if ($busy.Count -gt 0) { throw "Close every Unity editor for $workspace before moving its ShaderCache. PIDs: $(($busy | ForEach-Object ProcessId) -join ', ')" }
    # Unity can also reopen a project without exposing -projectPath in its command
    # line. An exclusive probe of this project's existing lock catches that case.
    $lockPath = Join-Path $workspace 'Temp/UnityLockfile'
    if (Test-Path -LiteralPath $lockPath -PathType Leaf) {
        $probe = $null
        try { $probe = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None) }
        catch { throw "Project lock cannot be acquired exclusively; refusing cache move: $lockPath" }
        finally { if ($null -ne $probe) { $probe.Dispose() } }
    }
}

function Get-CacheManifest([string] $Directory) {
    Assert-NoReparseAncestors $Directory
    $entries = @(Get-ChildItem -LiteralPath $Directory -Recurse -Force)
    foreach ($item in $entries) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Cache contains a reparse point: $($item.FullName)"
        }
    }
    $byPath = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($file in $entries | Where-Object { -not $_.PSIsContainer }) {
        $relative = $file.FullName.Substring($Directory.Length + 1).Replace('\','/')
        $byPath.Add($relative, [ordered]@{ path=$relative; bytes=$file.Length; sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() })
    }
    [string[]]$paths = @($byPath.Keys)
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $files = [Collections.Generic.List[object]]::new()
    $canonical = [Text.StringBuilder]::new()
    $totalBytes = [long]0
    foreach ($path in $paths) {
        $file = $byPath[$path]
        $files.Add($file); $totalBytes += [long]$file.bytes
        [void]$canonical.Append($file.path).Append([char]0).Append($file.bytes).Append([char]0).Append($file.sha256).Append("`n")
    }
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $treeHash = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical.ToString()))).Replace('-','').ToLowerInvariant() }
    finally { $sha.Dispose() }
    return [ordered]@{ directory=$Directory; capturedUtc=[DateTime]::UtcNow.ToString('o'); fileCount=$files.Count; bytes=$totalBytes; treeSha256=$treeHash; files=@($files) }
}

[void](Assert-WorkspacePath $source)
[void](Assert-WorkspacePath $runDirectory)
[void](Assert-WorkspacePath $destination)
$expectedSource = (Join-Path $workspace 'Library/ShaderCache').TrimEnd('\','/')
if (-not $source.Equals($expectedSource, $comparison)) { throw "Source must be the exact project-local Library/ShaderCache: $source" }
Assert-NoReparseAncestors $source
Assert-NoReparseAncestors $destination
if (-not (Test-Path -LiteralPath $source -PathType Container)) { throw "Exact ShaderCache directory is missing: $source" }
if (Test-Path -LiteralPath $runDirectory) { throw "RunId must be unique; refusing an existing destination run: $runDirectory" }
if (Test-Path -LiteralPath $destination) { throw "Backup destination already exists: $destination" }
Assert-LunaEditorClosed

$before = Get-CacheManifest $source
[void](New-Item -ItemType Directory -Path $runDirectory)
$before | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runDirectory 'shader-cache-before.json') -Encoding utf8
$summary = [ordered]@{ operation='move-project-shader-cache-for-diagnosis'; source=$source; destination=$destination; startedUtc=[DateTime]::UtcNow.ToString('o'); finishedUtc=$null; moved=$false; verified=$false; error=$null }
try {
    # Recheck final absolute targets and process ownership immediately before the
    # one reversible mutation. Never delete, overwrite or touch the rest of Library.
    Assert-LunaEditorClosed
    Assert-NoReparseAncestors $source
    Assert-NoReparseAncestors $destination
    if (Test-Path -LiteralPath $destination) { throw 'Destination appeared after the initial check; refusing overwrite.' }
    Move-Item -LiteralPath $source -Destination $destination
    $summary.moved = $true
    if (Test-Path -LiteralPath $source) { throw 'Source still exists after move; inspect both paths without overwriting either.' }
    $after = Get-CacheManifest $destination
    $after | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runDirectory 'shader-cache-after.json') -Encoding utf8
    if ($before.fileCount -ne $after.fileCount -or $before.bytes -ne $after.bytes -or $before.treeSha256 -ne $after.treeSha256) {
        throw 'Moved cache failed the complete per-file SHA/count/length verification. Both manifests and backup are retained.'
    }
    $summary.verified = $true
    $summary.fileCount=$after.fileCount; $summary.bytes=$after.bytes; $summary.treeSha256=$after.treeSha256
}
catch { $summary.error=$_.Exception.Message; throw }
finally {
    $summary.finishedUtc=[DateTime]::UtcNow.ToString('o')
    $summary | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runDirectory 'shader-cache-move.json') -Encoding utf8
}
Write-Host "Verified reversible ShaderCache backup: $destination ($($before.fileCount) files, $($before.bytes) bytes)."
Write-Host 'No cache warning is filtered or declared fixed. Compare cold and warm builds from an unchanged source fingerprint.'
