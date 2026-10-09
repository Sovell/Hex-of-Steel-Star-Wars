# Builds recovered libraries into verification/ only. Does not start or patch the game.
$ErrorActionPreference = 'Stop'
$taskRoot = $PSScriptRoot
$taskDotnet = (Get-Command dotnet -ErrorAction Stop).Source
$taskSdkRoot = Join-Path (Split-Path $taskDotnet) 'sdk'
$taskCompiler = Get-ChildItem -LiteralPath $taskSdkRoot -Directory |
    Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'Roslyn\bincore\csc.dll') } |
    Sort-Object { [version](($_.Name -split '-')[0]) } -Descending |
    Select-Object -First 1
if (!$taskCompiler) { throw 'A .NET SDK with the C# compiler is required.' }
$taskCompilerPath = Join-Path $taskCompiler.FullName 'Roslyn\bincore\csc.dll'
New-Item -ItemType Directory -Path (Join-Path $taskRoot 'verification'), (Join-Path $taskRoot 'logs') -Force | Out-Null
foreach ($taskAssembly in @('Assembly-CSharp-firstpass', 'Assembly-CSharp')) {
    $taskOutput = Join-Path $taskRoot "verification\$taskAssembly.dll"
    $taskArguments = @('/nologo', '/target:library', '/nostdlib+', '/unsafe+', '/langversion:7.3', '/deterministic+')
    $taskArguments += '/out:"' + $taskOutput + '"'
    foreach ($taskReference in Get-ChildItem -LiteralPath (Join-Path $taskRoot 'references\Managed') -Filter '*.dll') {
        if ($taskReference.Name -in @('Assembly-CSharp.dll', 'Assembly-CSharp-firstpass.dll')) { continue }
        $taskArguments += '/reference:"' + $taskReference.FullName + '"'
    }
    if ($taskAssembly -eq 'Assembly-CSharp') {
        $taskArguments += '/reference:"' + (Join-Path $taskRoot 'verification\Assembly-CSharp-firstpass.dll') + '"'
    }
    $taskArguments += Get-ChildItem -LiteralPath (Join-Path $taskRoot "source\$taskAssembly") -Filter '*.cs' -Recurse |
        ForEach-Object { '"' + $_.FullName + '"' }
    $taskResponse = Join-Path $taskRoot "verification\$taskAssembly.rsp"
    $taskLog = Join-Path $taskRoot "logs\build-$taskAssembly.log"
    $taskArguments | Set-Content -LiteralPath $taskResponse -Encoding utf8
    & $taskDotnet $taskCompilerPath "@$taskResponse" *> $taskLog
    $taskExitCode = $LASTEXITCODE
    $taskErrors = @(Select-String -LiteralPath $taskLog -Pattern 'error CS')
    $taskWarnings = @(Select-String -LiteralPath $taskLog -Pattern 'warning CS')
    Write-Output "$taskAssembly : exit=$taskExitCode; errors=$($taskErrors.Count); warnings=$($taskWarnings.Count)"
    if ($taskExitCode -ne 0) { throw "Compilation failed. See $taskLog" }
}
