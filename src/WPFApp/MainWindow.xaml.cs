using System.Windows;
using System.Windows.Media;
using System;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using HitsDownloadManager.DownloadEngine;
using HitsDownloadManager.WPFApp.Controls;
namespace HitsDownloadManager.WPFApp
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode = true;
        private const string PlaceholderText = "Enter download URL here...";
        private DownloadManager _downloadManager;
        private string _downloadFolder;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.SettingsViewModel();
            // Initialize download manager with default concurrent downloads
            _downloadManager = new DownloadManager(3);
            _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;
            _downloadManager.DownloadCompleted += DownloadManager_Completed;
            _downloadManager.DownloadFailed += DownloadManager_Failed;
            _downloadFolder = txtDownloadFolder.Text;
            LoadSettings();
        }
        #region Theme Management
        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTheme(false);
        }
        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            SwitchTheme(true);
        }
        private void SwitchTheme(bool darkMode)
        {
            isDarkMode = darkMode;
            var resources = Application.Current.Resources;
            if (darkMode)
            {
                resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
                resources["HoverAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                resources["PressedAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                resources["DisabledAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#546E7A"));
                resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E2E"));
                resources["PanelBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A3B"));
                resources["InputBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F2F3F"));
                resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#404050"));
                resources["TextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B0"));
                resources["HeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["TabBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A3B"));
                resources["TabHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A4B"));
                resources["TabSelectedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
            }
            else
            {
                resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
                resources["HoverAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                resources["PressedAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                resources["DisabledAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90CAF9"));
                resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                resources["PanelBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["InputBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                resources["TextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121"));
                resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
                resources["HeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121"));
                resources["TabBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["TabHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                resources["TabSelectedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
            }
        }
        #endregion
        #region Downloads Tab
        private void TxtDownloadUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDownloadUrl.Text == PlaceholderText)
            {
                txtDownloadUrl.Text = "";
                txtDownloadUrl.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrush"];
            }
        }
        private void TxtDownloadUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDownloadUrl.Text))
            {
                txtDownloadUrl.Text = PlaceholderText;
                txtDownloadUrl.Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"];
            }
        }
        private void BtnAddDownload_Click(object sender, RoutedEventArgs e)
        {
            string url = txtDownloadUrl.Text;
            if (string.IsNullOrWhiteSpace(url) || url == PlaceholderText)
            {
                MessageBox.Show("Please enter a valid download URL.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Generate filename from URL
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                MessageBox.Show("Invalid URL format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string filename = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = string.Format("download_{0}", DateTime.Now.Ticks);
            }
            string destinationPath = Path.Combine(_downloadFolder, filename);

            // Ensure download folder exists
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            // Add download to manager
            string downloadId = _downloadManager.AddDownload(url, destinationPath, Priority.Normal);
            if (!string.IsNullOrEmpty(downloadId))
            {
                var task = _downloadManager.GetDownload(downloadId);
                if (task != null)
                {
                    // Add UI control for this download
                    var downloadControl = new DownloadItemControl(task, _downloadManager);
                    downloadsContainer.Children.Add(downloadControl);
                }
                // Clear the input
                txtDownloadUrl.Text = PlaceholderText;
                txtDownloadUrl.Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"];
                System.Diagnostics.Debug.WriteLine(string.Format("Download started: {0}", filename));
            }
        }
        private void DownloadManager_ProgressChanged(object sender, DownloadTask task)
        {
            // UI updates happen automatically through INotifyPropertyChanged in DownloadTask
            System.Diagnostics.Debug.WriteLine(string.Format("Progress: {0} - {1:F1}%", task.Filename, task.Progress));
        }
        private void DownloadManager_Completed(object sender, DownloadTask task)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Download completed: {0}", task.Filename));
            // TODO: Show notification
        }
        private void DownloadManager_Failed(object sender, DownloadTask task)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Download failed: {0} - {1}", task.Filename, task.ErrorMessage));
            // TODO: Show error notification
        }
        #endregion
        #region Settings Tab
        private void BtnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Download Folder",
                ShowNewFolderButton = true,
                SelectedPath = txtDownloadFolder.Text
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDownloadFolder.Text = dialog.SelectedPath;
                _downloadFolder = dialog.SelectedPath;
                System.Diagnostics.Debug.WriteLine(string.Format("Download folder changed to: {0}", dialog.SelectedPath));
            }
        }
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Validate and save settings to configuration
            if (!Directory.Exists(txtDownloadFolder.Text))
            {
                var result = MessageBox.Show(
                    string.Format("The folder '{0}' does not exist.\n\nWould you like to create it?", txtDownloadFolder.Text),
                    "Folder Not Found",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(txtDownloadFolder.Text);
                        _downloadFolder = txtDownloadFolder.Text;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Failed to create folder:\n{0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            // Validate concurrent downloads
            if (!int.TryParse(txtConcurrentLimit.Text, out int concurrentLimit) || concurrentLimit < 1 || concurrentLimit > 10)
            {
                MessageBox.Show("Concurrent downloads must be between 1 and 10.", "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Validate speed limit
            if (!int.TryParse(txtSpeedLimit.Text, out int speedLimit) || speedLimit < 0)
            {
                MessageBox.Show("Speed limit must be 0 or greater (0 = unlimited).", "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Update download folder
            _downloadFolder = txtDownloadFolder.Text;
            // Recreate download manager with new settings
            _downloadManager = new DownloadManager(concurrentLimit);
            _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;
            _downloadManager.DownloadCompleted += DownloadManager_Completed;
            _downloadManager.DownloadFailed += DownloadManager_Failed;
            System.Diagnostics.Debug.WriteLine("Settings saved:");
            System.Diagnostics.Debug.WriteLine(string.Format("  Download Folder: {0}", txtDownloadFolder.Text));
            System.Diagnostics.Debug.WriteLine(string.Format("  Concurrent Limit: {0}", concurrentLimit));
            System.Diagnostics.Debug.WriteLine(string.Format("  Speed Limit: {0} KB/s", speedLimit));
            System.Diagnostics.Debug.WriteLine(string.Format("  Auto-start: {0}", chkAutoStart.IsChecked));
            System.Diagnostics.Debug.WriteLine(string.Format("  Notifications: {0}", chkNotifications.IsChecked));
            // TODO: Show status bar notification instead of popup
        }
        private void ChkAutoStart_Changed(object sender, RoutedEventArgs e)
        {
            if (chkAutoStart == null) return;
            System.Diagnostics.Debug.WriteLine(string.Format("Auto-start changed: {0}", chkAutoStart.IsChecked));
        }
        private void ChkNotifications_Changed(object sender, RoutedEventArgs e)
        {
            if (chkNotifications == null) return;
            System.Diagnostics.Debug.WriteLine(string.Format("Notifications changed: {0}", chkNotifications.IsChecked));
        }
        private void LoadSettings()
        {
            // TODO: Load settings from configuration file/database
            System.Diagnostics.Debug.WriteLine("Settings loaded (using defaults)");
        }
        #endregion
        #region Tab Navigation
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl == null || MainTabControl.SelectedItem == null) return;
            var selectedTab = (TabItem)MainTabControl.SelectedItem;
            var tabHeader = selectedTab.Header.ToString();
            System.Diagnostics.Debug.WriteLine(string.Format("Tab changed to: {0}", tabHeader));
            switch (tabHeader)
            {
                case "⬇️ Downloads":
                    RefreshDownloadsTab();
                    break;
                case "⚙️ Settings":
                    RefreshSettingsTab();
                    break;
                case "📜 History":
                    RefreshHistoryTab();
                    break;
                case "⏳ Queue":
                    RefreshQueueTab();
                    break;
                case "🔧 Advanced":
                    RefreshAdvancedTab();
                    break;
                case "📊 Stats":
                    RefreshStatsTab();
                    break;
                case "ℹ️ About":
                    RefreshAboutTab();
                    break;
            }
        }
        private void RefreshDownloadsTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing Downloads tab...");
        }
        private void RefreshSettingsTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing Settings tab...");
        }
        private void RefreshHistoryTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing History tab...");
        }
        private void RefreshQueueTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing Queue tab...");
        }
        private void RefreshAdvancedTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing Advanced tab...");
        }
        private void RefreshStatsTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing Stats tab...");
        }
        private void RefreshAboutTab()
        {
            System.Diagnostics.Debug.WriteLine("Refreshing About tab...");
        }
        #endregion
    }
}

