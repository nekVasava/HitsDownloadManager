# Complete Packaging Script for HitsDownloadManager
param(
    [string]$Version = "1.0.0",
    [switch]$SkipTests = $false,
    [switch]$CreateMSI = $false,
    [switch]$CreateMSIX = $false
)
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  HitsDownloadManager Packaging Script" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
$ErrorActionPreference = "Stop"
$outputPath = "build\Release\$Version"
# Step 1: Run tests (unless skipped)
if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test tests --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed! Aborting packaging." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ All tests passed!" -ForegroundColor Green
    Write-Host ""
}
# Step 2: Build single-file EXE
Write-Host "📦 Building single-file EXE..." -ForegroundColor Yellow
& "build\BuildSingleFile.ps1" -Version $Version -Configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host ""
# Step 3: Copy browser extension files
Write-Host "🌐 Copying browser extension files..." -ForegroundColor Yellow
$extPath = "$outputPath\BrowserExtension"
New-Item -Path $extPath -ItemType Directory -Force | Out-Null
Copy-Item -Path "src\BrowserExtension\*" -Destination $extPath -Recurse -Force
Write-Host "✅ Browser extension copied" -ForegroundColor Green
Write-Host ""
# Step 4: Create native host manifest
Write-Host "📝 Creating native host manifest..." -ForegroundColor Yellow
$nativeHostManifest = @{
    name = "com.hitsdownloadmanager.native"
    description = "HitsDownloadManager Native Messaging Host"
    path = "$outputPath\HitsDownloadManager.exe"
    type = "stdio"
    allowed_origins = @(
        "chrome-extension://EXTENSION_ID_HERE/"
    )
} | ConvertTo-Json
Set-Content -Path "$outputPath\native-host-manifest.json" -Value $nativeHostManifest -Encoding UTF8
Write-Host "✅ Native host manifest created" -ForegroundColor Green
Write-Host ""
# Step 5: Copy documentation
Write-Host "📄 Copying documentation..." -ForegroundColor Yellow
Copy-Item -Path "README.md" -Destination $outputPath -Force
Copy-Item -Path "docs\*" -Destination "$outputPath\docs" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Documentation copied" -ForegroundColor Green
Write-Host ""
# Step 6: Create ZIP archive
Write-Host "📦 Creating ZIP archive..." -ForegroundColor Yellow
$zipPath = "build\Release\HitsDownloadManager-$Version-win-x64.zip"
Compress-Archive -Path "$outputPath\*" -DestinationPath $zipPath -Force
Write-Host "✅ ZIP created: $zipPath" -ForegroundColor Green
Write-Host ""
# Step 7: Create MSI (optional)
if ($CreateMSI) {
    Write-Host "🔨 Building MSI installer..." -ForegroundColor Yellow
    # Requires WiX Toolset installed
    if (Get-Command "candle.exe" -ErrorAction SilentlyContinue) {
        Push-Location Installer
        candle.exe HitsDownloadManager.wxs -dSourceDir="..\$outputPath"
        light.exe HitsDownloadManager.wixobj -out "..\build\Release\HitsDownloadManager-$Version.msi"
        Pop-Location
        Write-Host "✅ MSI created" -ForegroundColor Green
    } else {
        Write-Host "⚠️  WiX Toolset not found. Skipping MSI creation." -ForegroundColor Yellow
    }
    Write-Host ""
}
# Step 8: Create MSIX (optional)
if ($CreateMSIX) {
    Write-Host "🔨 Building MSIX package..." -ForegroundColor Yellow
    # Requires Windows SDK installed
    if (Get-Command "makeappx.exe" -ErrorAction SilentlyContinue) {
        makeappx.exe pack /d $outputPath /p "build\Release\HitsDownloadManager-$Version.msix"
        Write-Host "✅ MSIX created" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Windows SDK not found. Skipping MSIX creation." -ForegroundColor Yellow
    }
    Write-Host ""
}
# Step 9: Generate checksums
Write-Host "🔐 Generating checksums..." -ForegroundColor Yellow
$checksums = @()
Get-ChildItem "build\Release" -Filter "*.$Version*" | ForEach-Object {
    $hash = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
    $checksums += "$hash  $($_.Name)"
}
$checksums | Out-File "build\Release\HitsDownloadManager-$Version-checksums.txt" -Encoding UTF8
Write-Host "✅ Checksums generated" -ForegroundColor Green
Write-Host ""
# Final summary
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ Packaging Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Artifacts created in: build\Release\" -ForegroundColor Cyan
Write-Host "   - Single-file EXE" -ForegroundColor White
Write-Host "   - ZIP archive" -ForegroundColor White
if ($CreateMSI) { Write-Host "   - MSI installer" -ForegroundColor White }
if ($CreateMSIX) { Write-Host "   - MSIX package" -ForegroundColor White }
Write-Host "   - Checksums file" -ForegroundColor White
Write-Host ""
