# HitsDownloadManager Architecture
## Overview
HitsDownloadManager is a Windows-based download manager with infinite auto-retry capabilities, browser integration, and intelligent queue management.
## Component Architecture
### 1. WPF Application Layer
- Views: XAML-based UI components
- ViewModels: MVVM pattern implementation
- Models: Download state, queue items, configuration
- Services: Download coordination, logging
### 2. Download Engine
- HTTP/HTTPS Handler: Chunked downloads with resume support
- Retry Logic: Infinite retry with exponential backoff
- Queue Manager: Priority-based queue with throttling
- State Persistence: JSON-based download state
### 3. Browser Integration
- Chrome MV3 Extension: Intercepts downloads
- Native Messaging Host: Stdio-based communication
- Protocol: JSON message schema
### 4. Logging System
- Location: D:\HitsDownloadManager\Logs\
- Rotation: Daily with compression
- Categories: Application, Downloads, AI, Browser, Errors
## Technologies Used
- .NET 8: WPF Application framework
- C#: Primary language
- JavaScript: Browser extension
- Git: Version control

