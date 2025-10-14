using HitsDownloadManager.DownloadEngine;
// Initialize logging system
LoggingService.Initialize();
// Test all 5 categories
LoggerExtensions.Application.Information("CHECKPOINT: Application started");
LoggerExtensions.Downloads.Information("CHECKPOINT: Download test - File: test.zip");
LoggerExtensions.AI.Information("CHECKPOINT: AI assistant query processed");
LoggerExtensions.Browser.Information("CHECKPOINT: Browser extension connected");
LoggerExtensions.Errors.Error("CHECKPOINT: Test error logged");
Console.WriteLine("Logging test complete! Check D:\\HitsDownloadManager\\Logs\\");
// Shutdown logging
LoggingService.Shutdown();
