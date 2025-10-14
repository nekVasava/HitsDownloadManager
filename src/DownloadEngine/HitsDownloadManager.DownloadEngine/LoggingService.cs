using Serilog;
using Serilog.Events;
using Serilog.Sinks.File.Archive;
using System.IO.Compression;
namespace HitsDownloadManager.DownloadEngine;
public static class LoggingService
{
    public static void Initialize()
    {
        var logPath = @"D:\HitsDownloadManager\Logs\";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: logPath + "application-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                hooks: new ArchiveHooks(CompressionLevel.Optimal))
            .WriteTo.File(
                path: logPath + "downloads-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                hooks: new ArchiveHooks(CompressionLevel.Optimal))
            .WriteTo.File(
                path: logPath + "errors-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error,
                retainedFileCountLimit: 90,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                hooks: new ArchiveHooks(CompressionLevel.Optimal))
            .CreateLogger();
        Log.Information("HitsDownloadManager logging initialized");
    }
    public static ILogger ForContext(string category)
    {
        return Log.ForContext("Category", category);
    }
    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }
}
