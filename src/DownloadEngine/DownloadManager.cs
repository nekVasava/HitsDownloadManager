using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace HitsDownloadManager.DownloadEngine
{
    public class DownloadManager
    {
        private readonly ConcurrentDictionary<string, DownloadTask> _downloads;
        private readonly ConcurrentDictionary<string, HttpDownloadEngine> _engines;
        private readonly int _maxConcurrentDownloads;
        private int _activeDownloads;
        public event EventHandler<DownloadTask> DownloadProgressChanged;
        public event EventHandler<DownloadTask> DownloadCompleted;
        public event EventHandler<DownloadTask> DownloadFailed;
        public DownloadManager(int maxConcurrentDownloads = 5)
        {
            _downloads = new ConcurrentDictionary<string, DownloadTask>();
            _engines = new ConcurrentDictionary<string, HttpDownloadEngine>();
            _maxConcurrentDownloads = maxConcurrentDownloads;
            _activeDownloads = 0;
        }
        public string AddDownload(string url, string destinationPath, Priority priority = Priority.Normal)
        {
            var task = new DownloadTask
            {
                Url = url,
                DestinationPath = destinationPath,
                Filename = System.IO.Path.GetFileName(destinationPath),
                Priority = priority,
                Status = DownloadStatus.Pending
            };
            if (_downloads.TryAdd(task.Id, task))
            {
                Console.WriteLine($"[DEBUG] Download added: {task.Filename}");
                Task.Run(() => ProcessQueueAsync());
                return task.Id;
            }
            return null;
        }
        public IEnumerable<DownloadTask> GetAllDownloads()
        {
            return _downloads.Values.OrderByDescending(d => d.Priority).ThenBy(d => d.CreatedAt);
        }
        public DownloadTask GetDownload(string id)
        {
            _downloads.TryGetValue(id, out var task);
            return task;
        }
        public void PauseDownload(string id)
        {
            if (_engines.TryGetValue(id, out var engine))
            {
                engine.Pause();
                if (_downloads.TryGetValue(id, out var task))
                {
                    task.Status = DownloadStatus.Paused;
                }
            }
        }
        public void ResumeDownload(string id)
        {
            if (_downloads.TryGetValue(id, out var task))
            {
                task.Status = DownloadStatus.Pending;
                Task.Run(() => ProcessQueueAsync());
            }
        }
        public void CancelDownload(string id)
        {
            if (_engines.TryGetValue(id, out var engine))
            {
                engine.Pause();
            }
            if (_downloads.TryRemove(id, out var task))
            {
                task.Status = DownloadStatus.Cancelled;
            }
            _engines.TryRemove(id, out _);
        }
        private async Task ProcessQueueAsync()
        {
            Console.WriteLine($"[DEBUG] ProcessQueue called. Active downloads: {_activeDownloads}");
            while (_activeDownloads < _maxConcurrentDownloads)
            {
                var nextTask = _downloads.Values
                    .Where(d => d.Status == DownloadStatus.Pending)
                    .OrderByDescending(d => d.Priority)
                    .ThenBy(d => d.CreatedAt)
                    .FirstOrDefault();
                if (nextTask == null)
                {
                    Console.WriteLine($"[DEBUG] No pending downloads in queue");
                    break;
                }
                Console.WriteLine($"[DEBUG] Starting download: {nextTask.Filename}");
                nextTask.Status = DownloadStatus.Downloading;
                System.Threading.Interlocked.Increment(ref _activeDownloads);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var engine = new HttpDownloadEngine();
                        _engines[nextTask.Id] = engine;
                        var progress = new Progress<DownloadTask>(task =>
                        {
                            DownloadProgressChanged?.Invoke(this, task);
                        });
                        var success = await engine.DownloadFileAsync(nextTask, progress);
                        if (success)
                        {
                            DownloadCompleted?.Invoke(this, nextTask);
                        }
                        else
                        {
                            DownloadFailed?.Invoke(this, nextTask);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Exception in download task: {ex.Message}");
                        nextTask.Status = DownloadStatus.Failed;
                        nextTask.ErrorMessage = ex.Message;
                        DownloadFailed?.Invoke(this, nextTask);
                    }
                    finally
                    {
                        System.Threading.Interlocked.Decrement(ref _activeDownloads);
                        _engines.TryRemove(nextTask.Id, out _);
                        await ProcessQueueAsync();
                    }
                });
            }
        }
    }
}
