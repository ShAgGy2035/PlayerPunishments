$ErrorActionPreference = 'Stop'

$projectRoot = $PSScriptRoot
$outputDirectory = Join-Path $projectRoot 'release\PlayerPunishments'
$zipPath = Join-Path $projectRoot 'release\PlayerPunishments-v1.0.0.zip'
$dllPath = Join-Path $projectRoot 'bin\Release\net10.0\PlayerPunishments.dll'

dotnet build (Join-Path $projectRoot 'PlayerPunishments.csproj') -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Remove-Item $outputDirectory -Recurse -Force -ErrorAction SilentlyContinue
New-Item $outputDirectory -ItemType Directory -Force | Out-Null
Copy-Item $dllPath $outputDirectory

Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open(
	$zipPath,
	[System.IO.Compression.ZipArchiveMode]::Create)
try {
	[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
		$archive,
		$dllPath,
		'PlayerPunishments/PlayerPunishments.dll',
		[System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
}
finally {
	$archive.Dispose()
}
Write-Host "Created $zipPath"