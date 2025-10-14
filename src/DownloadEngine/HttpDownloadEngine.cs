using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace HitsDownloadManager.DownloadEngine
{
    public class HttpDownloadEngine
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        public HttpDownloadEngine()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 50
            };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(30) };
        }
        public async Task<bool> DownloadFileAsync(DownloadTask task, IProgress<DownloadTask> progress)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            int retryCount = 0;
            int maxRetries = int.MaxValue;
            int delaySeconds = 2;
            while (retryCount < maxRetries)
            {
                try
                {
                    Console.WriteLine($"[DEBUG] Starting download: {task.Url}");
                    task.Status = DownloadStatus.Downloading;
                    progress?.Report(task);
                    using var request = new HttpRequestMessage(HttpMethod.Get, task.Url);
                    if (task.DownloadedBytes > 0 && File.Exists(task.DestinationPath))
                    {
                        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(task.DownloadedBytes, null);
                        Console.WriteLine($"[DEBUG] Resuming from byte: {task.DownloadedBytes}");
                    }
                    Console.WriteLine($"[DEBUG] Sending HTTP request...");
                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
                    Console.WriteLine($"[DEBUG] Response status: {response.StatusCode}");
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP {(int)response.StatusCode}: {response.StatusCode}");
                    }
                    task.TotalBytes = response.Content.Headers.ContentLength ?? 0;
                    Console.WriteLine($"[DEBUG] Total bytes: {task.TotalBytes}");
                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(task.DestinationPath, 
                        task.DownloadedBytes > 0 ? FileMode.Append : FileMode.Create, 
                        FileAccess.Write, FileShare.None, 8192, true);
                    var buffer = new byte[8192];
                    int bytesRead;
                    var startTime = DateTime.Now;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
                        task.DownloadedBytes += bytesRead;
                        var elapsed = (DateTime.Now - startTime).TotalSeconds;
                        if (elapsed > 0)
                        {
                            var speed = task.DownloadedBytes / elapsed;
                            task.Speed = FormatSpeed(speed);
                            if (task.TotalBytes > 0)
                            {
                                var remaining = task.TotalBytes - task.DownloadedBytes;
                                var timeRemaining = remaining / speed;
                                task.TimeRemaining = FormatTime(timeRemaining);
                            }
                        }
                        progress?.Report(task);
                    }
                    task.Status = DownloadStatus.Completed;
                    task.CompletedAt = DateTime.Now;
                    progress?.Report(task);
                    Console.WriteLine($"[DEBUG] Download completed!");
                    return true;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    retryCount++;
                    task.RetryCount = retryCount;
                    task.ErrorMessage = ex.Message;
                    task.Status = DownloadStatus.Failed;
                    progress?.Report(task);
                    Console.WriteLine($"[ERROR] Download failed: {ex.Message}");
                    Console.WriteLine($"[DEBUG] Retry {retryCount} in {delaySeconds} seconds...");
                    var delay = delaySeconds * Math.Pow(2, Math.Min(retryCount - 1, 5));
                    var jitter = new Random().Next(0, 1000);
                    await Task.Delay(TimeSpan.FromSeconds(delay) + TimeSpan.FromMilliseconds(jitter));
                    delaySeconds = Math.Min(delaySeconds * 2, 300);
                }
            }
            return false;
        }
        public void Pause()
        {
            _cancellationTokenSource?.Cancel();
        }
        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024) return $"{bytesPerSecond:F0} B/s";
            if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024:F2} KB/s";
            return $"{bytesPerSecond / (1024 * 1024):F2} MB/s";
        }
        private string FormatTime(double seconds)
        {
            if (seconds < 60) return $"{seconds:F0}s";
            if (seconds < 3600) return $"{seconds / 60:F0}m {seconds % 60:F0}s";
            return $"{seconds / 3600:F0}h {(seconds % 3600) / 60:F0}m";
        }
    }
}
