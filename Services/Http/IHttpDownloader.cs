using System;
using System.Net.Http;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using System.Threading;
namespace HitsDownloadManager.DownloadEngine.Services.Http
{
    public interface IHttpDownloader
    {
        Task<(bool success, string error)> DownloadAsync(DownloadRequest request, IProgress<long> progress, CancellationToken cancellationToken);
    }
}
