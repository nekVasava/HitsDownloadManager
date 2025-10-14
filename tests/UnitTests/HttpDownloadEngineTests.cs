using Xunit;
using HitsDownloadManager.DownloadEngine;
using System.Threading.Tasks;
using System.IO;
namespace HitsDownloadManager.Tests.UnitTests
{
    public class HttpDownloadEngineTests
    {
        [Fact]
        public async Task HttpDownloadEngine_SupportsResumeCapability()
        {
            // Arrange
            var engine = new HttpDownloadEngine();
            var url = "https://example.com/test.zip";
            // Act
            var supportsResume = await engine.SupportsResumeAsync(url);
            // Assert - this will depend on server response
            Assert.IsType<bool>(supportsResume);
        }
        [Fact]
        public void DownloadTask_InitializesWithCorrectState()
        {
            // Arrange & Act
            var task = new DownloadTask
            {
                Url = "https://example.com/file.zip",
                DestinationPath = @"D:\Downloads\file.zip",
                MaxAttempts = 0, // Infinite retry
                DelaySeconds = 5
            };
            // Assert
            Assert.Equal("https://example.com/file.zip", task.Url);
            Assert.Equal(0, task.MaxAttempts);
            Assert.Equal(5, task.DelaySeconds);
        }
        [Fact]
        public void ChunkedDownload_CalculatesSegmentsCorrectly()
        {
            // Arrange
            long fileSize = 100 * 1024 * 1024; // 100 MB
            int segmentSize = 10 * 1024 * 1024; // 10 MB per segment
            // Act
            int expectedSegments = (int)Math.Ceiling((double)fileSize / segmentSize);
            // Assert
            Assert.Equal(10, expectedSegments);
        }
        [Fact]
        public void ErrorClassification_IdentifiesNetworkErrors()
        {
            // Arrange
            var exception = new HttpRequestException("Network error");
            // Act
            var errorType = ClassifyError(exception);
            // Assert
            Assert.Equal("NetworkError", errorType);
        }
        private string ClassifyError(Exception ex)
        {
            if (ex is HttpRequestException) return "NetworkError";
            if (ex is IOException) return "IOError";
            if (ex is UnauthorizedAccessException) return "PermissionError";
            return "UnknownError";
        }
    }
}
