using Serilog;
namespace HitsDownloadManager.DownloadEngine;
public static class LoggerExtensions
{
    public static ILogger Application => LoggingService.ForContext(LogCategory.Application.ToString());
    public static ILogger Downloads => LoggingService.ForContext(LogCategory.Downloads.ToString());
    public static ILogger AI => LoggingService.ForContext(LogCategory.AI.ToString());
    public static ILogger Browser => LoggingService.ForContext(LogCategory.Browser.ToString());
    public static ILogger Errors => LoggingService.ForContext(LogCategory.Errors.ToString());
}
