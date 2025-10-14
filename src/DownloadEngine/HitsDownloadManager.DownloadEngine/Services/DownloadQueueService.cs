using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HitsDownloadManager.DownloadEngine.Models;
using HitsDownloadManager.DownloadEngine.Services.Http;

namespace HitsDownloadManager.DownloadEngine.Services
{
    public class DownloadQueueService
    {
        private readonly Queue<QueuedDownload> _queue = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxConcurrentDownloads;

        public DownloadQueueService(int maxConcurrentDownloads = 3)
        {
            _maxConcurrentDownloads = maxConcurrentDownloads;
            _semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        }

        public void Enqueue(DownloadRequest request, DownloadPriority priority, IProgress<long> progress)
        {
            var queuedDownload = new QueuedDownload
            {
                Request = request,
                Priority = priority,
                Progress = progress
            };

            lock (_queue)
            {
                _queue.Enqueue(queuedDownload);
            }

            Task.Run(() => ProcessQueue());
        }

        private async Task ProcessQueue()
        {
            QueuedDownload download;

            lock (_queue)
            {
                if (_queue.Count == 0) return;

                var sortedQueue = _queue.OrderByDescending(d => d.Priority).ToList();
                _queue.Clear();
                foreach (var item in sortedQueue)
                {
                    _queue.Enqueue(item);
                }

                download = _queue.Dequeue();
            }

            await _semaphore.WaitAsync();

            try
            {
                Console.WriteLine($"Starting download: {download.Request.Url} (Priority: {download.Priority})");

                var downloader = new HttpDownloader();
                await downloader.DownloadAsync(download.Request, download.Progress, CancellationToken.None);

                Console.WriteLine($"Completed download: {download.Request.Url}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public class QueuedDownload
    {
        public DownloadRequest Request { get; set; }
        public DownloadPriority Priority { get; set; }
        public IProgress<long> Progress { get; set; }
    }
}
