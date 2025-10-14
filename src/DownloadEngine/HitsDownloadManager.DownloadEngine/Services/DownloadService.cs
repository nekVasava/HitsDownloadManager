using System;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using HitsDownloadManager.DownloadEngine.Infrastructure;
using HitsDownloadManager.DownloadEngine.Services.Http;

namespace HitsDownloadManager.DownloadEngine.Services
{
    public class DownloadService
    {
        private readonly IHttpDownloader _httpDownloader;
        private readonly RetryPolicyConfig _retryConfig;

        public DownloadService(IHttpDownloader httpDownloader, RetryPolicyConfig retryConfig)
        {
            _httpDownloader = httpDownloader;
            _retryConfig = retryConfig;
        }

        public async Task<(bool success, string error)> StartDownloadAsync(
            DownloadRequest request,
            IProgress<long> progress,
            CancellationToken cancellationToken)
        {
            int attempt = 0;
            bool isInfinite = _retryConfig.MaxAttempts == 0;

            while (isInfinite || attempt < _retryConfig.MaxAttempts)
            {
                attempt++;

                var (success, error) = await _httpDownloader.DownloadAsync(request, progress, cancellationToken);

                if (success)
                {
                    Console.WriteLine("Download completed successfully.");
                    return (true, string.Empty);
                }

                if (!isInfinite && attempt >= _retryConfig.MaxAttempts)
                {
                    Console.WriteLine("Max retry attempts reached. Download failed.");
                    return (false, error);
                }

                var delay = TimeSpan.FromSeconds(_retryConfig.DelaySeconds + new Random().Next(0, 3));
                Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                await Task.Delay(delay, cancellationToken);
            }

            return (false, "Download failed after retries.");
        }
    }
}
