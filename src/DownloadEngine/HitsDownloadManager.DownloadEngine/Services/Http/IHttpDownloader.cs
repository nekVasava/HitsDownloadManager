using System;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
namespace HitsDownloadManager.DownloadEngine.Services.Http
{
    public interface IHttpDownloader
    {
        Task<(bool success, string error)> DownloadAsync(
            DownloadRequest request,
            IProgress<long> progress,
            CancellationToken cancellationToken);
    }
}
