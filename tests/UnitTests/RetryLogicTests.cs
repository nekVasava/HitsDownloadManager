using Xunit;
using HitsDownloadManager.DownloadEngine;
using System;
using System.Threading.Tasks;
namespace HitsDownloadManager.Tests.UnitTests
{
    public class RetryLogicTests
    {
        [Fact]
        public void RetryPolicy_InfiniteRetryWhenMaxAttemptsIsZero()
        {
            // Arrange
            var policy = new RetryPolicy { MaxAttempts = 0 };
            // Act & Assert
            Assert.True(policy.ShouldRetry(1));
            Assert.True(policy.ShouldRetry(100));
            Assert.True(policy.ShouldRetry(1000));
        }
        [Fact]
        public void RetryPolicy_StopsAfterMaxAttempts()
        {
            // Arrange
            var policy = new RetryPolicy { MaxAttempts = 3 };
            // Act & Assert
            Assert.True(policy.ShouldRetry(1));
            Assert.True(policy.ShouldRetry(2));
            Assert.True(policy.ShouldRetry(3));
            Assert.False(policy.ShouldRetry(4));
        }
        [Fact]
        public void BackoffCalculation_ExponentialWithJitter()
        {
            // Arrange
            var baseDelay = 5; // 5 seconds
            var attempt = 3;
            // Act
            var delay = CalculateBackoff(baseDelay, attempt);
            // Assert - exponential: 5 * 2^(3-1) = 20 seconds (plus jitter)
            Assert.InRange(delay, 20, 30); // Allow jitter range
        }
        [Fact]
        public void RetryState_PersistsAcrossRestarts()
        {
            // Arrange
            var state = new DownloadState
            {
                Url = "https://example.com/file.zip",
                BytesDownloaded = 1024 * 1024, // 1 MB
                TotalBytes = 10 * 1024 * 1024, // 10 MB
                Attempts = 5,
                LastAttempt = DateTime.UtcNow
            };
            // Act - simulate saving and loading
            var json = System.Text.Json.JsonSerializer.Serialize(state);
            var loadedState = System.Text.Json.JsonSerializer.Deserialize<DownloadState>(json);
            // Assert
            Assert.Equal(state.Url, loadedState.Url);
            Assert.Equal(state.BytesDownloaded, loadedState.BytesDownloaded);
            Assert.Equal(state.Attempts, loadedState.Attempts);
        }
        private int CalculateBackoff(int baseDelay, int attempt)
        {
            var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);
            var jitter = new Random().Next(0, (int)(exponentialDelay * 0.5));
            return (int)exponentialDelay + jitter;
        }
    }
    public class RetryPolicy
    {
        public int MaxAttempts { get; set; }
        public bool ShouldRetry(int attempt)
        {
            if (MaxAttempts == 0) return true; // Infinite retry
            return attempt <= MaxAttempts;
        }
    }
    public class DownloadState
    {
        public string Url { get; set; }
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public int Attempts { get; set; }
        public DateTime LastAttempt { get; set; }
    }
}
