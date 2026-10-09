$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Build-Analysis.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$taskGame = Join-Path (Split-Path $PSScriptRoot) 'Game\Hex of Steel_Data\Managed'
foreach ($taskAssembly in @('Assembly-CSharp.dll', 'Assembly-CSharp-firstpass.dll')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot "verification\$taskAssembly") -Destination $taskGame -Force
}
Write-Output 'Prototype libraries updated. Original Steam installation was not modified.'
