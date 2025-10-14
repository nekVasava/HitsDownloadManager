# Build script for HitsDownloadManager single-file EXE
param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release"
)
Write-Host "🔨 Building HitsDownloadManager v$Version..." -ForegroundColor Cyan
# Set paths
$projectPath = "src\WPFApp\HitsDownloadManager.csproj"
$outputPath = "build\Release\$Version"
# Clean previous build
if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Recurse -Force
    Write-Host "✅ Cleaned previous build" -ForegroundColor Green
}
# Publish single-file EXE
Write-Host "📦 Publishing single-file EXE..." -ForegroundColor Yellow
dotnet publish $projectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true `
    -p:PublishTrimmed=false `
    -o $outputPath
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Single-file EXE created successfully!" -ForegroundColor Green
    Write-Host "📍 Location: $outputPath\HitsDownloadManager.exe" -ForegroundColor Cyan
    # Get file size
    $exePath = "$outputPath\HitsDownloadManager.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length / 1MB
        Write-Host "📊 File size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
# Copy additional files
Write-Host "📄 Copying additional files..." -ForegroundColor Yellow
Copy-Item -Path "README.md" -Destination $outputPath -Force
Copy-Item -Path "src\BrowserExtension\manifest.json" -Destination "$outputPath\BrowserExtension" -Force -ErrorAction SilentlyContinue
Write-Host ""
Write-Host "🎉 Build complete! Package is ready at: $outputPath" -ForegroundColor Green
