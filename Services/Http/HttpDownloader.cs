using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using System.Threading;
namespace HitsDownloadManager.DownloadEngine.Services.Http
{
    public class HttpDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly string _userAgent;
        public HttpDownloader()
        {
            _httpClient = new HttpClient();
            _userAgent = "HitsDownloadManager/1.0";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
        }
        public async Task<(bool success, string error)> DownloadAsync(DownloadRequest request, IProgress<long> progress, CancellationToken cancellationToken)
        {
            try
            {
                var destinationPath = request.DestinationPath;
                var tempPath = destinationPath + ".tmp";
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                // Get current file size for resume
                long startPos = File.Exists(tempPath) ? new FileInfo(tempPath).Length : 0;
                // Set up request with Range header if resuming
                var msg = new HttpRequestMessage(HttpMethod.Get, request.Url);
                if (startPos > 0)
                {
                    msg.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startPos, null);
                }
                var response = await _httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.PartialContent)
                {
                    return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                }
                var contentLength = response.Content.Headers.ContentLength;
                var totalBytes = (contentLength + startPos) ?? 0;
                // Start or resume writing
                using (var fileStream = new FileStream(tempPath, startPos == 0 ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = startPos;
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            totalBytesRead += bytesRead;
                            progress?.Report(totalBytesRead);
                        }
                    }
                }
                // Rename temp to final destination
                if (File.Exists(destinationPath)) File.Delete(destinationPath);
                File.Move(tempPath, destinationPath);
                return (true, string.Empty);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return (false, "Download canceled");
            }
            catch (Exception ex)
            {
                return (false, $"Network or IO error: {ex.Message}");
            }
        }
    }
}
