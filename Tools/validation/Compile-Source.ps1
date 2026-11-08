param(
    [string]$UnityEditorRoot = 'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor',
    [string]$ReportDirectory = 'Logs/NativeRewrite'
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
Push-Location -LiteralPath $projectRoot
try {
    $sourceResponse = Get-ChildItem -LiteralPath 'Library/Bee/artifacts' -Filter 'Assembly-CSharp.rsp' -Recurse |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -eq $sourceResponse) { throw 'A prior Unity compilation response file is required for assembly references.' }
    $sdkRoot = Join-Path $UnityEditorRoot 'Data/DotNetSdk'
    $dotnet = Join-Path $sdkRoot 'dotnet.exe'
    $compiler = Get-ChildItem -LiteralPath (Join-Path $sdkRoot 'sdk') -Filter 'csc.dll' -Recurse |
        Where-Object { $_.FullName.Replace('\', '/') -Like '*/Roslyn/bincore/*' } | Select-Object -First 1
    if ($null -eq $compiler) { throw 'Unity Roslyn compiler could not be located.' }
    [void](New-Item -ItemType Directory -Path $ReportDirectory -Force)
    $outputRoot = (Resolve-Path -LiteralPath $ReportDirectory).Path
    $responsePath = Join-Path $outputRoot 'current-source-check.rsp'
    $assemblyPath = Join-Path $outputRoot 'current-source-check.dll'
    $logPath = Join-Path $outputRoot 'current-source-check.log'
    $arguments = [System.Collections.Generic.List[string]]::new()
    $sources = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($line in Get-Content -LiteralPath $sourceResponse.FullName) {
        if ($line.StartsWith('-out:')) { $arguments.Add('-out:"' + $assemblyPath + '"'); continue }
        if ($line.StartsWith('-refout:') -or $line.StartsWith('/additionalfile:')) { continue }
        # A removed compatibility assembly must not conceal unresolved legacy references.
        if ($line -match '^(-r:|-reference:).*Luna\.Framework') { continue }
        if ($line -match '^"(Assets/.+\.cs)"$') {
            $sourcePath = $Matches[1]
            if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) { continue }
            [void]$sources.Add($sourcePath)
        }
        $arguments.Add($line)
    }
    foreach ($file in Get-ChildItem -LiteralPath 'Assets/Script' -Filter '*.cs' -Recurse) {
        $relative = $file.FullName.Substring($projectRoot.Length + 1).Replace('\', '/')
        if ($relative -match '/Editor/' -or -not $sources.Add($relative)) { continue }
        $arguments.Add('"' + $relative + '"')
    }
    [System.IO.File]::WriteAllLines($responsePath, $arguments, [System.Text.UTF8Encoding]::new($false))
    $compilerOutput = & $dotnet $compiler.FullName ('@' + $responsePath) 2>&1
    $compilerExitCode = $LASTEXITCODE
    $compilerOutput | Set-Content -LiteralPath $logPath -Encoding utf8
    $compilerOutput
    Write-Output ('Source compiler exit code: ' + $compilerExitCode + '; log: ' + $logPath)
    exit $compilerExitCode
}
finally { Pop-Location }

