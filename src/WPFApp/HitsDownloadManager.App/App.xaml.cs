using System.Windows;
using HitsDownloadManager.DownloadEngine;
namespace HitsDownloadManager.App;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Initialize logging system
        LoggingService.Initialize();
        LoggerExtensions.Application.Information("CHECKPOINT: HitsDownloadManager application started");
    }
    protected override void OnExit(ExitEventArgs e)
    {
        LoggerExtensions.Application.Information("CHECKPOINT: HitsDownloadManager application exiting");
        LoggingService.Shutdown();
        base.OnExit(e);
    }
}
