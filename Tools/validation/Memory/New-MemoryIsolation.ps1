[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string] $RunId,
    [string] $UnityVersion = '6000.6.0f1',
    [ValidateSet('builtin','urp')][string] $Pipeline = 'builtin',
    [ValidatePattern('^\d+\.\d+\.\d+$')][string] $UrpVersion = '17.6.0',
    [switch] $SpineShaders
)
$ErrorActionPreference = 'Stop'
if (-not $RunId) { throw 'Supply a unique RunId; an existing project is never overwritten.' }
if ($SpineShaders -and $Pipeline -ne 'urp') { throw 'Spine URP shaders require the explicit URP experiment pipeline.' }
$repository = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..')).Path
$runDirectory = Join-Path $repository "Logs/MemoryIsolation/$RunId"
$projectDirectory = Join-Path $runDirectory 'Project'
if (Test-Path -LiteralPath $runDirectory) { throw "Run already exists: $runDirectory" }
foreach ($relative in @('Assets/Editor', 'Packages', 'ProjectSettings')) {
    [void](New-Item -ItemType Directory -Path (Join-Path $projectDirectory $relative) -Force)
}
$utf8 = [Text.UTF8Encoding]::new($false)
$dependencies = [ordered]@{ 'com.unity.modules.audio'='1.0.0'; 'com.unity.modules.jsonserialize'='1.0.0' }
if ($Pipeline -eq 'urp') { $dependencies['com.unity.render-pipelines.universal']=$UrpVersion }
[IO.File]::WriteAllText((Join-Path $projectDirectory 'Packages/manifest.json'), (@{ dependencies=$dependencies } | ConvertTo-Json -Depth 3), $utf8)
[IO.File]::WriteAllText((Join-Path $projectDirectory 'ProjectSettings/ProjectVersion.txt'), "m_EditorVersion: $UnityVersion`n", $utf8)
[IO.File]::WriteAllText((Join-Path $projectDirectory '.memory-isolation-project'), 'Standalone engine experiment. No Luna sources or production saves.', $utf8)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Templates/MemoryIsolationBuilder.cs') -Destination (Join-Path $projectDirectory 'Assets/Editor/MemoryIsolationBuilder.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Templates/MemoryIsolationPlayer.cs') -Destination (Join-Path $projectDirectory 'Assets/MemoryIsolationPlayer.cs')
if ($Pipeline -eq 'urp') {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Templates/MemoryIsolationUrpBuilder.cs') -Destination (Join-Path $projectDirectory 'Assets/Editor/MemoryIsolationUrpBuilder.cs')
}
$spinePackage = $null
if ($SpineShaders) {
    $sourcePackage = Join-Path $repository 'Packages/com.esotericsoftware.spine.urp-shaders-4.0-2019.1'
    $packageInfo = Get-Content -Raw -LiteralPath (Join-Path $sourcePackage 'package.json') | ConvertFrom-Json
    if ($packageInfo.name -ne 'com.esotericsoftware.spine.urp-shaders') { throw 'Unexpected Spine package identity.' }
    if (@(Get-ChildItem -LiteralPath $sourcePackage -Recurse -File -Filter '*.cs').Count -ne 0) { throw 'The shader-only package unexpectedly contains C#; isolation would be invalid.' }
    $destinationPackage = Join-Path $projectDirectory 'Packages/com.esotericsoftware.spine.urp-shaders'
    Copy-Item -LiteralPath $sourcePackage -Destination $destinationPackage -Recurse
    $packageFiles = @(Get-ChildItem -LiteralPath $sourcePackage -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($sourcePackage.Length+1)
        $sourceHash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $copiedHash = (Get-FileHash -LiteralPath (Join-Path $destinationPackage $relative) -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($sourceHash -ne $copiedHash) { throw "Spine package copy mismatch: $relative" }
        [ordered]@{ path=$relative.Replace('\','/'); sha256=$sourceHash }
    })
    $spinePackage = [ordered]@{ name=$packageInfo.name; version=$packageInfo.version; originalPath=$sourcePackage; originalUrpDependency=$packageInfo.dependencies.'com.unity.render-pipelines.universal'; explicitUrpVersion=$UrpVersion; copiedFiles=$packageFiles }
}
$sources = @(Get-ChildItem -LiteralPath $projectDirectory -Recurse -File | ForEach-Object {
    [ordered]@{ path=$_.FullName.Substring($projectDirectory.Length + 1).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
})
[ordered]@{ runId=$RunId; unityVersion=$UnityVersion; pipeline=$Pipeline; urpVersion=$(if ($Pipeline -eq 'urp') { $UrpVersion } else { $null }); spinePackage=$spinePackage; project=$projectDirectory; createdUtc=[DateTime]::UtcNow.ToString('o'); sources=$sources } |
    ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $runDirectory 'created.json') -Encoding utf8
Write-Output $projectDirectory
