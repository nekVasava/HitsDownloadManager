# HitsDownloadManager User Guide
## Installation
### Method 1: Single-file EXE
1. Download HitsDownloadManager.exe from releases
2. Run the executable - no installation required
3. Run RegisterNativeHost.bat to enable browser integration
### Method 2: MSI Installer
1. Download HitsDownloadManager.msi
2. Run the installer and follow the wizard
3. Native host registration is automatic
## Getting Started
### First Launch
1. Configure download directory in Settings
2. Set maximum concurrent downloads (default: 3)
3. Enable browser integration for automatic interception
### Adding Downloads
- Method 1: Click browser download link (automatic interception)
- Method 2: Paste URL in application and click Add
- Method 3: Drag and drop download link
### Managing Downloads
- Pause: Click Pause button or right-click > Pause
- Resume: Click Resume button (works after app restart)
- Cancel: Click Cancel to stop and remove
- Retry: Failed downloads auto-retry infinitely
## Features
### Infinite Auto-Retry
- Downloads never fail permanently
- Exponential backoff with jitter prevents server overload
- Manual retry option available
### Browser Integration
- Supports Chrome, Edge, Brave (Chromium-based)
- Automatic download interception
- Preserves referrer and headers
### Queue Management
- Priority levels: High, Normal, Low
- Bandwidth fair distribution
- Configurable concurrent downloads
### Download Resume
- Resumes after app restart
- Supports interrupted connections
- Chunk-based progress tracking
## Troubleshooting
### Browser Integration Not Working
1. Check native host registration: Run RegisterNativeHost.bat
2. Restart browser after installation
3. Check extension is enabled in browser settings
### Downloads Not Resuming
1. Verify server supports HTTP Range headers
2. Check download state file exists in logs
3. Ensure sufficient disk space
### Slow Download Speeds
1. Increase concurrent downloads in Settings
2. Check bandwidth limits
3. Verify network connection
## Keyboard Shortcuts
- Ctrl+N: Add new download
- Ctrl+P: Pause selected download
- Ctrl+R: Resume selected download
- Delete: Remove selected download
- F5: Refresh download list

