# ProxySwitcher ビルドスクリプト (for .NET Framework 4.8)
# 使用方法: PowerShellで .\build.ps1 を実行してください。

Write-Host "ビルドを開始します (.NET Framework 4.8)..." -ForegroundColor Cyan

# クリーンアップ
if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }

# ビルド実行
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "ビルドに失敗しました。"
    exit 1
}

Write-Host "`nビルドが完了しました！" -ForegroundColor Green
$outputDir = "bin\Release\net48"
Write-Host "出力先: $outputDir"

Get-ChildItem -Path $outputDir -Recurse -Include "*.exe" | Select-Object FullName, Length