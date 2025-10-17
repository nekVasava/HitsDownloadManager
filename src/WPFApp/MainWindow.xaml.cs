using System.Windows;
using System.Windows.Media;
using System;
using System.Collections.Generic;
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
                private readonly HashSet<string> _filesInUse = new HashSet<string>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.SettingsViewModel();
            // Initialize download manager with default concurrent downloads
            _downloadManager = new DownloadManager(3);
            _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;
            _downloadManager.DownloadCompleted += DownloadManager_Completed;
            _downloadManager.DownloadFailed += DownloadManager_Failed;
            _downloadFolder = txtDownloadFolderPath.Text;
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
            // Handle duplicate filenames
            string originalFilename = filename;
            string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            int counter = 1;
            // Check both filesystem AND in-memory tracking
            while (File.Exists(Path.Combine(_downloadFolder, filename)) || _filesInUse.Contains(filename))
            {
                filename = $"{nameWithoutExt}({counter}){extension}";
                counter++;
            }
            // Mark this filename as in-use
            _filesInUse.Add(filename);
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
                    activeDownloadsContainer.Children.Insert(0, downloadControl);
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
            // Move download from Active to Completed section
            Dispatcher.Invoke(() =>
            {
                // Find and remove from active section
                DownloadItemControl controlToMove = null;
                foreach (var child in activeDownloadsContainer.Children)
                {
                    if (child is DownloadItemControl control && control.Task == task)
                    {
                        controlToMove = control;
                        break;
                    }
                }
                if (controlToMove != null)
                {
                    activeDownloadsContainer.Children.Remove(controlToMove);
                    completedDownloadsContainer.Children.Insert(0, controlToMove);
                    // Keep only last 5 completed
                    while (completedDownloadsContainer.Children.Count > 5)
                    {
                        completedDownloadsContainer.Children.RemoveAt(completedDownloadsContainer.Children.Count - 1);
                    }
                }
            });
        }
        private void DownloadManager_Failed(object sender, DownloadTask task)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Download failed: {0} - {1}", task.Filename, task.ErrorMessage));
            // TODO: Show error notification
        }
        #endregion
        private void LoadSettings()
        {
            // Initialize settings from saved preferences
            if (txtDownloadFolderPath != null)
            {
                txtDownloadFolderPath.Text = _downloadFolder;
            }
        }
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle tab changes if needed
        }
        #region Settings Tab Event Handlers
        private void BtnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Download Folder",
                SelectedPath = _downloadFolder
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _downloadFolder = dialog.SelectedPath;
                txtDownloadFolderPath.Text = _downloadFolder;
            }
        }
        private void SliderConcurrent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtConcurrentValue != null)
            {
                int newValue = (int)sliderConcurrent.Value;
                txtConcurrentValue.Text = newValue.ToString();
                // Update DownloadManager concurrent limit
                if (_downloadManager != null)
                {
                    _downloadManager.MaxConcurrentDownloads = newValue;
                }
            }
        }
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Save settings logic here
            MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
        
    }
}











