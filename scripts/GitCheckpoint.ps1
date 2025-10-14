# Git Auto-Commit Script for HitsDownloadManager
param(
    [Parameter(Mandatory=$true)]
    [string]$CheckpointMessage
)
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$commitMessage = "CHECKPOINT: $CheckpointMessage - $timestamp"
Write-Host "Creating checkpoint commit..." -ForegroundColor Cyan
git add -A
git commit -m "$commitMessage"
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Checkpoint committed successfully!" -ForegroundColor Green
    Write-Host "Message: $commitMessage" -ForegroundColor Gray
} else {
    Write-Host "✗ Git commit failed!" -ForegroundColor Red
}
