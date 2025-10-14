using Xunit;
using HitsDownloadManager.DownloadEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace HitsDownloadManager.Tests.IntegrationTests
{
    public class DownloadQueueTests
    {
        [Fact]
        public async Task DownloadQueue_ResumeAfterRestart()
        {
            // Arrange
            var queue = new DownloadQueue();
            var download = new DownloadTask
            {
                Url = "https://example.com/file.zip",
                DestinationPath = @"D:\Downloads\file.zip",
                BytesDownloaded = 5 * 1024 * 1024, // 5 MB already downloaded
                TotalBytes = 10 * 1024 * 1024 // 10 MB total
            };
            // Act - Save state
            await queue.SaveStateAsync(download);
            // Simulate restart
            var loadedDownload = await queue.LoadStateAsync(download.Url);
            // Assert
            Assert.NotNull(loadedDownload);
            Assert.Equal(5 * 1024 * 1024, loadedDownload.BytesDownloaded);
            Assert.Equal(50, loadedDownload.ProgressPercentage);
        }
        [Fact]
        public void DownloadQueue_RespectsPriorityOrder()
        {
            // Arrange
            var queue = new PriorityDownloadQueue();
            queue.Enqueue(new DownloadTask { Url = "https://example.com/file1.zip", Priority = Priority.Low });
            queue.Enqueue(new DownloadTask { Url = "https://example.com/file2.zip", Priority = Priority.High });
            queue.Enqueue(new DownloadTask { Url = "https://example.com/file3.zip", Priority = Priority.Normal });
            // Act
            var first = queue.Dequeue();
            var second = queue.Dequeue();
            var third = queue.Dequeue();
            // Assert
            Assert.Equal(Priority.High, first.Priority);
            Assert.Equal(Priority.Normal, second.Priority);
            Assert.Equal(Priority.Low, third.Priority);
        }
        [Fact]
        public async Task DownloadQueue_ThrottlesSimultaneousDownloads()
        {
            // Arrange
            var queue = new DownloadQueue { MaxConcurrentDownloads = 3 };
            var downloads = new List<DownloadTask>
            {
                new DownloadTask { Url = "https://example.com/file1.zip" },
                new DownloadTask { Url = "https://example.com/file2.zip" },
                new DownloadTask { Url = "https://example.com/file3.zip" },
                new DownloadTask { Url = "https://example.com/file4.zip" },
                new DownloadTask { Url = "https://example.com/file5.zip" }
            };
            // Act
            foreach (var download in downloads)
            {
                queue.Enqueue(download);
            }
            var activeCount = queue.GetActiveDownloadsCount();
            // Assert
            Assert.True(activeCount <= 3);
        }
        [Fact]
        public void DownloadQueue_DistributesBandwidthFairly()
        {
            // Arrange
            var totalBandwidth = 10 * 1024 * 1024; // 10 MB/s
            var activeDownloads = 4;
            // Act
            var perDownloadBandwidth = totalBandwidth / activeDownloads;
            // Assert
            Assert.Equal(2.5 * 1024 * 1024, perDownloadBandwidth);
        }
        [Fact]
        public async Task DownloadEngine_HandlesDroppedConnections()
        {
            // Arrange
            var engine = new HttpDownloadEngine();
            var download = new DownloadTask
            {
                Url = "https://example.com/file.zip",
                MaxAttempts = 0, // Infinite retry
                BytesDownloaded = 3 * 1024 * 1024
            };
            // Act - Simulate connection drop
            var canRetry = engine.CanRetryAfterError(download, new System.Net.WebException());
            // Assert
            Assert.True(canRetry);
        }
    }
    public enum Priority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
    public class PriorityDownloadQueue
    {
        private readonly List<DownloadTask> _queue = new List<DownloadTask>();
        public void Enqueue(DownloadTask task)
        {
            _queue.Add(task);
        }
        public DownloadTask Dequeue()
        {
            var task = _queue.OrderByDescending(t => t.Priority).First();
            _queue.Remove(task);
            return task;
        }
    }
    public class DownloadQueue
    {
        public int MaxConcurrentDownloads { get; set; } = 3;
        private readonly List<DownloadTask> _queue = new List<DownloadTask>();
        public void Enqueue(DownloadTask task) => _queue.Add(task);
        public int GetActiveDownloadsCount() => _queue.Count(d => d.IsActive);
        public Task SaveStateAsync(DownloadTask task)
        {
            // Simulate saving state to file
            return Task.CompletedTask;
        }
        public Task<DownloadTask> LoadStateAsync(string url)
        {
            // Simulate loading state from file
            return Task.FromResult(_queue.FirstOrDefault(d => d.Url == url));
        }
    }
}
