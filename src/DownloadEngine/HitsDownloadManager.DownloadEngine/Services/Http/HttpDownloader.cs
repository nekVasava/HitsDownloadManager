using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
namespace HitsDownloadManager.DownloadEngine.Services.Http
{
    public class HttpDownloader : IHttpDownloader
    {
        public async Task<(bool success, string error)> DownloadAsync(
            DownloadRequest request,
            IProgress<long> progress,
            CancellationToken cancellationToken)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(request.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(request.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalBytesRead += bytesRead;
                    progress?.Report(totalBytesRead);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
