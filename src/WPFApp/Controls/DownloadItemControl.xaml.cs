using System;
using System.Windows;
using System.Windows.Controls;
using HitsDownloadManager.DownloadEngine;
using HitsDownloadManager.Helpers;
using System.Diagnostics;
using System.IO;
namespace HitsDownloadManager.WPFApp.Controls
{
    public partial class DownloadItemControl : UserControl
    {
        public DownloadTask Task { get; }
        private DownloadManager _downloadManager;
        public DownloadItemControl(DownloadTask task, DownloadManager downloadManager)
        {
            InitializeComponent();
            Task = task;
            _downloadManager = downloadManager;
            // Subscribe to property changes
            Task.PropertyChanged += Task_PropertyChanged;
            UpdateUI();
            var (icon, color) = FileTypeHelper.GetFileTypeIconAndColor(Task.Filename);
            FileTypeIcon.Kind = icon;
            FileTypeIcon.Foreground = color;
        }
        private void Task_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Update UI on the UI thread when any property changes
            Dispatcher.Invoke(() => UpdateUI());
        }
        private void UpdateUI()
        {
            txtFilename.Text = Task.Filename;
            txtCompleted.Visibility = Task.Status == DownloadStatus.Completed ? Visibility.Visible : Visibility.Collapsed;
            txtFailed.Visibility = Task.Status == DownloadStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
            progressBar.Value = Task.Progress;
            txtProgress.Text = $"{Task.Progress:F1}%";
            var downloaded = Task.DownloadedBytes / (1024.0 * 1024.0);
            var total = Task.TotalBytes > 0 ? Task.TotalBytes / (1024.0 * 1024.0) : 0;
            if (total > 0)
            {
                txtSize.Text = $"{downloaded:F2} MB / {total:F2} MB";
            }
            else
            {
                txtSize.Text = $"{downloaded:F2} MB";
            }
            // Hide speed and time remaining for completed downloads
            if (Task.Status == DownloadStatus.Completed)
            {
                txtSpeed.Visibility = Visibility.Collapsed;
                txtTimeRemaining.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtSpeed.Visibility = Visibility.Visible;
                txtSpeed.Text = Task.Speed;
                txtTimeRemaining.Visibility = Visibility.Visible;
                txtTimeRemaining.Text = Task.TimeRemaining;
            }
            btnPause.Visibility = Task.Status == DownloadStatus.Downloading ? Visibility.Visible : Visibility.Collapsed;
            btnResume.Visibility = Task.Status == DownloadStatus.Paused ? Visibility.Visible : Visibility.Collapsed;
            btnCancel.Visibility = Task.Status == DownloadStatus.Completed ? Visibility.Collapsed : Visibility.Visible;
            btnShowInFolder.Visibility = Task.Status == DownloadStatus.Completed ? Visibility.Visible : Visibility.Collapsed;
        }
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.PauseDownload(Task.Id);
        }
        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.ResumeDownload(Task.Id);
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.CancelDownload(Task.Id);
            // Remove this control from parent container
            var parent = this.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(this);
            }
        }
        private void BtnShowInFolder_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Task.DestinationPath))
            {
                Process.Start("explorer.exe", $"/select,\"{Task.DestinationPath}\"");
            }
        }
        public void Refresh()
        {
            UpdateUI();
        }
    }
}
