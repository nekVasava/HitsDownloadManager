using Xunit;
using System.IO;
using System.Linq;
namespace HitsDownloadManager.Tests.IntegrationTests
{
    public class CheckpointTests
    {
        [Fact]
        public void Checkpoint1_ProjectStructureExists()
        {
            // Arrange
            var requiredFolders = new[]
            {
                "src/WPFApp",
                "src/DownloadEngine",
                "src/BrowserExtension",
                "src/Logging",
                "tests",
                "docs",
                "build"
            };
            // Act & Assert
            foreach (var folder in requiredFolders)
            {
                Assert.True(Directory.Exists(folder), $"Folder {folder} should exist");
            }
        }
        [Fact]
        public void Checkpoint2_GitInitialized()
        {
            // Arrange & Act
            var gitFolder = Directory.Exists(".git");
            var gitIgnoreExists = File.Exists(".gitignore");
            // Assert
            Assert.True(gitFolder, ".git folder should exist");
            Assert.True(gitIgnoreExists, ".gitignore file should exist");
        }
        [Fact]
        public void Checkpoint3_UIViewsCreated()
        {
            // Arrange
            var requiredViews = new[]
            {
                "src/WPFApp/Views/DownloadProgressView.xaml",
                "src/WPFApp/Views/DownloadDetailPanel.xaml",
                "src/WPFApp/Views/DownloadControlPanel.xaml",
                "src/WPFApp/Views/AIAssistantPanel.xaml"
            };
            // Act & Assert
            foreach (var view in requiredViews)
            {
                Assert.True(File.Exists(view), $"View {view} should exist");
            }
        }
        [Fact]
        public void Checkpoint4_DownloadEngineImplemented()
        {
            // Arrange
            var engineFiles = new[]
            {
                "src/DownloadEngine/HttpDownloadEngine.cs",
                "src/DownloadEngine/DownloadTask.cs",
                "src/DownloadEngine/RetryPolicy.cs"
            };
            // Act & Assert
            foreach (var file in engineFiles)
            {
                if (File.Exists(file))
                {
                    var content = File.ReadAllText(file);
                    Assert.Contains("class", content);
                }
            }
        }
        [Fact]
        public void Checkpoint5_BrowserExtensionExists()
        {
            // Arrange
            var extensionFiles = new[]
            {
                "src/BrowserExtension/manifest.json",
                "src/BrowserExtension/background.js",
                "src/BrowserExtension/native-messaging-host.exe"
            };
            // Act
            var existingFiles = extensionFiles.Count(File.Exists);
            // Assert
            Assert.True(existingFiles >= 2, "At least 2 browser extension files should exist");
        }
        [Fact]
        public void Checkpoint6_LoggingConfigured()
        {
            // Arrange
            var logsPath = @"D:\HitsDownloadManager\Logs";
            // Act
            var logsDirectoryExists = Directory.Exists(logsPath);
            // Assert - logs directory should be created or configured
            Assert.True(logsDirectoryExists || File.Exists("src/Logging/LoggingConfig.cs"),
                "Logging should be configured");
        }
        [Fact]
        public void Checkpoint7_ThemesAndAnimationsCreated()
        {
            // Arrange
            var themeFiles = new[]
            {
                "src/WPFApp/Themes/DarkTheme.xaml",
                "src/WPFApp/Themes/LightTheme.xaml",
                "src/WPFApp/Themes/Animations.xaml",
                "src/WPFApp/Themes/Icons.xaml"
            };
            // Act & Assert
            foreach (var file in themeFiles)
            {
                Assert.True(File.Exists(file), $"Theme file {file} should exist");
            }
        }
        [Fact]
        public void Checkpoint8_TestsExist()
        {
            // Arrange
            var testFiles = Directory.GetFiles("tests", "*.cs", SearchOption.AllDirectories);
            // Act
            var testCount = testFiles.Length;
            // Assert
            Assert.True(testCount >= 5, $"Should have at least 5 test files, found {testCount}");
        }
    }
}
