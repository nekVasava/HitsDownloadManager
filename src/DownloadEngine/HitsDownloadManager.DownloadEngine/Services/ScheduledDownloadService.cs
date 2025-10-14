using System;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using HitsDownloadManager.DownloadEngine.Infrastructure;


namespace HitsDownloadManager.DownloadEngine.Services
{
    public class ScheduledDownloadService
    {
        private Timer? _timer;
        private DownloadRequest? _request;
        private DateTime _scheduledTime;

        public void ScheduleDownload(DownloadRequest request, DateTime scheduledTime)
        {
            _request = request;
            _scheduledTime = scheduledTime;

            var delay = scheduledTime - DateTime.Now;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            _timer = new Timer(async _ => await StartDownloadAsync(), null, delay, Timeout.InfiniteTimeSpan);
            Console.WriteLine($"Download scheduled for {scheduledTime}");
        }

        private async Task StartDownloadAsync()
        {
            if (_request == null) return;

            Console.WriteLine($"Scheduled download started: {_request.Url} at {_scheduledTime}");

            var downloadService = new DownloadService(new Services.Http.HttpDownloader(), new RetryPolicyConfig { Enabled = true, MaxAttempts = 0, DelaySeconds = 5 });
            await downloadService.StartDownloadAsync(_request, new Progress<long>(), CancellationToken.None);

            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void CancelScheduledDownload()
        {
            _timer?.Dispose();
            Console.WriteLine("Scheduled download cancelled.");
        }
    }
}
