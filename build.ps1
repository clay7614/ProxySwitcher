$ErrorActionPreference = "Stop"

Write-Host "Build starting... (.NET Framework 4.8)" -ForegroundColor Cyan

# Cleanup
if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }

# Build
dotnet build -c Release

Write-Host "`nBuild complete!" -ForegroundColor Green
$outputDir = "bin\Release\net48"
Write-Host "Output: $outputDir"

Get-ChildItem -Path $outputDir -Recurse -Include "*.exe" | Select-Object FullName, Length
