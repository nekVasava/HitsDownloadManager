using Xunit;
using HitsDownloadManager.ViewModels;
using System.Collections.ObjectModel;
namespace HitsDownloadManager.Tests.UnitTests
{
    public class ViewModelTests
    {
        [Fact]
        public void DownloadProgressViewModel_InitializesWithEmptyCollection()
        {
            // Arrange & Act
            var viewModel = new DownloadProgressViewModel();
            // Assert
            Assert.NotNull(viewModel.Downloads);
            Assert.Empty(viewModel.Downloads);
        }
        [Fact]
        public void DownloadProgressViewModel_CanAddDownloadItem()
        {
            // Arrange
            var viewModel = new DownloadProgressViewModel();
            var downloadItem = new DownloadItemViewModel
            {
                Filename = "test.zip",
                ProgressPercentage = 50,
                Speed = "1.5 MB/s",
                ETA = "2 minutes",
                Segments = "4",
                Status = "Downloading"
            };
            // Act
            viewModel.Downloads.Add(downloadItem);
            // Assert
            Assert.Single(viewModel.Downloads);
            Assert.Equal("test.zip", viewModel.Downloads[0].Filename);
        }
        [Fact]
        public void DownloadItemViewModel_UpdatesProgressCorrectly()
        {
            // Arrange
            var item = new DownloadItemViewModel { ProgressPercentage = 0 };
            // Act
            item.ProgressPercentage = 75;
            // Assert
            Assert.Equal(75, item.ProgressPercentage);
        }
    }
}
