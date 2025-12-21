Write-Host "Building Standard..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/Publish/Standard

if (Test-Path obj) { Remove-Item -Path obj -Recurse -Force }

Write-Host "Building Lightweight..."
dotnet publish -c Release -r win-x64 --self-contained false -p:SelfContained=false -p:PublishSingleFile=true -o bin/Publish/Lightweight

# Lightweight版にアイコンファイルをコピー
Write-Host "Copying icons to Lightweight..."
Copy-Item "Utilities/icon_on.ico" "bin/Publish/Lightweight/icon_on.ico" -Force
Copy-Item "Utilities/icon_off.ico" "bin/Publish/Lightweight/icon_off.ico" -Force

Get-ChildItem -Path bin/Publish -Recurse -Include *.exe, *.ico | Select-Object FullName, Length