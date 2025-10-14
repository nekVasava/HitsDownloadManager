using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using HitsDownloadManager.DownloadEngine.Infrastructure;
using HitsDownloadManager.DownloadEngine.Services;
using HitsDownloadManager.DownloadEngine.Services.Http;

namespace HitsDownloadManager.Tests.DownloadEngine
{
    public class MockHttpDownloader : IHttpDownloader
    {
        private int _callCount = 0;
        public async Task<(bool success, string error)> DownloadAsync(DownloadRequest request, IProgress<long> progress, CancellationToken cancellationToken)
        {
            _callCount++;
            Console.WriteLine($"Attempt {_callCount}: Simulating network failure for {request.Url}");
            await Task.Delay(1000);
            return (false, "Simulated network error");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var request = new DownloadRequest
            {
                Url = "http://example.com/fake-file.zip",
                DestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fake-file.zip")
            };

            var retryConfig = new RetryPolicyConfig
            {
                Enabled = true,
                MaxAttempts = 0,
                DelaySeconds = 5
            };

            var downloader = new MockHttpDownloader();
            var downloadService = new DownloadService(downloader, retryConfig);

            Console.WriteLine("Starting download with infinite retry...");
            try
            {
                await downloadService.StartDownloadAsync(request, new Progress<long>(), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed: {ex.Message}");
            }
            Console.WriteLine("Test completed.");
        }
    }
}
