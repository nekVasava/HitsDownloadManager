using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
namespace HitsDownloadManager.DownloadEngine
{
    public class DownloadManager
    {
        private readonly ConcurrentDictionary<string, DownloadTask> _downloads;
        private readonly ConcurrentDictionary<string, HttpDownloadEngine> _engines;
        private int _maxConcurrentDownloads;
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
        public int MaxConcurrentDownloads
        {
            get => _maxConcurrentDownloads;
            set
            {
                _maxConcurrentDownloads = value;
                _ = Task.Run(() => ProcessQueueAsync()); // Restart queue processing with new limit
            }
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
                Debug.WriteLine($"[DownloadManager] Download added: {task.Filename} (ID: {task.Id})");
                Task.Run(() => ProcessQueueAsync());
                return task.Id;
            }
            return null;
        }
        public IEnumerable<DownloadTask> GetAllDownloads()
        {
            return _downloads.Values.OrderByDescending(d => d.Priority).ThenByDescending(d => d.CreatedAt);
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
                    task.Speed = "0 KB/s";  // Reset speed on pause
                    Debug.WriteLine($"[DownloadManager] Download paused: {task.Filename}");
                }
            }
        }
        public void ResumeDownload(string id)
        {
            if (_downloads.TryGetValue(id, out var task))
            {
                task.DownloadedBytes = task.PersistedDownloadedBytes; // Reset downloaded bytes on resume
                task.Status = DownloadStatus.Pending;
                Debug.WriteLine($"[DownloadManager] Download resumed: {task.Filename}");
                Task.Run(() => ProcessQueueAsync());
            }
        }
        public void CancelDownload(string id)
        {
            Debug.WriteLine($"[DownloadManager] CancelDownload called for ID: {id}");
            // Stop the download engine first
            if (_engines.TryGetValue(id, out var engine))
            {
                engine.Pause();
                _engines.TryRemove(id, out _);
                Debug.WriteLine($"[DownloadManager] Download engine stopped for ID: {id}");
            }
            // Update task status to Cancelled but KEEP it in the dictionary for history
            if (_downloads.TryGetValue(id, out var task))
            {
                task.Status = DownloadStatus.Cancelled;
                task.Speed = "0 KB/s";
                task.TimeRemaining = "-";
                Debug.WriteLine($"[DownloadManager] Download cancelled: {task.Filename} (Status: {task.Status})");
                // DO NOT REMOVE from _downloads - keep it for history!
            }
            else
            {
                Debug.WriteLine($"[DownloadManager] Warning: Task ID {id} not found in downloads dictionary");
            }
        }
        private async Task ProcessQueueAsync()
        {
            Debug.WriteLine($"[DownloadManager] ProcessQueue called. Active downloads: {_activeDownloads}");
            while (_activeDownloads < _maxConcurrentDownloads)
            {
                var nextTask = _downloads.Values
                    .Where(d => d.Status == DownloadStatus.Pending)
                    .OrderByDescending(d => d.Priority)
                    .ThenBy(d => d.CreatedAt)
                    .FirstOrDefault();
                if (nextTask == null)
                {
                    Debug.WriteLine($"[DownloadManager] No pending downloads in queue");
                    break;
                }
                Debug.WriteLine($"[DownloadManager] Starting download: {nextTask.Filename}");
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
                            Debug.WriteLine($"[DownloadManager] Download completed: {nextTask.Filename}");
                            DownloadCompleted?.Invoke(this, nextTask);
                        }
                        else
                        {
                            Debug.WriteLine($"[DownloadManager] Download failed: {nextTask.Filename}");
                            DownloadFailed?.Invoke(this, nextTask);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DownloadManager] Exception in download task: {ex.Message}");
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
        public List<DownloadTask> GetAllTasks()
        {
            var tasks = _downloads.Values.ToList();
            Debug.WriteLine($"[DownloadManager] GetAllTasks returning {tasks.Count} tasks");
            foreach (var task in tasks)
            {
                Debug.WriteLine($"  - {task.Filename}: Status={task.Status}, Created={task.CreatedAt}");
            }
            return tasks;
        }
        public void MoveTaskUp(string taskId)
        {
            Debug.WriteLine($"[DownloadManager] MoveTaskUp: {taskId}");
            if (_downloads.TryGetValue(taskId, out var task))
            {
                var allTasks = GetAllTasks()
                    .Where(t => t.Status != DownloadStatus.Completed)
                    .OrderBy(t => t.QueuePosition)
                    .ToList();
                int currentIndex = allTasks.FindIndex(t => t.Id == taskId);
                if (currentIndex > 0)
                {
                    // Swap queue positions
                    int temp = task.QueuePosition;
                    task.QueuePosition = allTasks[currentIndex - 1].QueuePosition;
                    allTasks[currentIndex - 1].QueuePosition = temp;
                    Debug.WriteLine($"[DownloadManager] Moved task up: {taskId}");
                }
            }
        }
        public void MoveTaskDown(string taskId)
        {
            Debug.WriteLine($"[DownloadManager] MoveTaskDown: {taskId}");
            if (_downloads.TryGetValue(taskId, out var task))
            {
                var allTasks = GetAllTasks()
                    .Where(t => t.Status != DownloadStatus.Completed)
                    .OrderBy(t => t.QueuePosition)
                    .ToList();
                int currentIndex = allTasks.FindIndex(t => t.Id == taskId);
                if (currentIndex >= 0 && currentIndex < allTasks.Count() - 1)
                {
                    // Swap queue positions
                    int temp = task.QueuePosition;
                    task.QueuePosition = allTasks[currentIndex + 1].QueuePosition;
                    allTasks[currentIndex + 1].QueuePosition = temp;
                    Debug.WriteLine($"[DownloadManager] Moved task down: {taskId}");
                }
            }
        }
        public void SetTaskPriority(string taskId, Priority priority)
        {
            Debug.WriteLine($"[DownloadManager] SetTaskPriority: {taskId} to {priority}");
            if (_downloads.TryGetValue(taskId, out var task))
            {
                task.Priority = priority;
                Debug.WriteLine($"[DownloadManager] Task priority updated: {taskId}");
            }
        }

        public void RemoveTask(string taskId)
        {
            if (_downloads.TryRemove(taskId, out var task))
            {
                Debug.WriteLine($"[DownloadManager] Task removed from history: {task.Filename}");
            }
        }
        public void ClearHistory()
        {
            var completedTasks = _downloads.Where(t =>
                t.Value.Status == DownloadStatus.Completed ||
                t.Value.Status == DownloadStatus.Failed ||
                t.Value.Status == DownloadStatus.Cancelled).ToList();
            Debug.WriteLine($"[DownloadManager] Clearing {completedTasks.Count} history items");
            foreach (var task in completedTasks)
            {
                _downloads.TryRemove(task.Key, out _);
            }
        }
    }
}

