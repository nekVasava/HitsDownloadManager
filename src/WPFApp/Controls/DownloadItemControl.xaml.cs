using System.Windows;
using System.Windows.Controls;
using HitsDownloadManager.DownloadEngine;
namespace HitsDownloadManager.WPFApp.Controls
{
    public partial class DownloadItemControl : UserControl
    {
        private DownloadTask _task;
        private DownloadManager _downloadManager;
        public DownloadItemControl(DownloadTask task, DownloadManager downloadManager)
        {
            InitializeComponent();
            _task = task;
            _downloadManager = downloadManager;
            UpdateUI();
            _task.PropertyChanged += Task_PropertyChanged;
        }
        private void Task_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUI());
        }
        private void UpdateUI()
        {
            txtFilename.Text = _task.Filename;
            txtUrl.Text = _task.Url;
            progressBar.Value = _task.Progress;
            txtProgress.Text = string.Format("{0:F1}%", _task.Progress);
            txtSize.Text = string.Format("{0} / {1}", _task.DownloadedSizeFormatted, _task.TotalSizeFormatted);
            txtSpeed.Text = _task.Speed;
            txtTimeRemaining.Text = _task.TimeRemaining;
            switch (_task.Status)
            {
                case DownloadStatus.Downloading:
                    btnPause.Visibility = Visibility.Visible;
                    btnResume.Visibility = Visibility.Collapsed;
                    break;
                case DownloadStatus.Paused:
                    btnPause.Visibility = Visibility.Collapsed;
                    btnResume.Visibility = Visibility.Visible;
                    break;
                case DownloadStatus.Completed:
                case DownloadStatus.Failed:
                case DownloadStatus.Cancelled:
                    btnPause.Visibility = Visibility.Collapsed;
                    btnResume.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.PauseDownload(_task.Id);
        }
        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.ResumeDownload(_task.Id);
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.CancelDownload(_task.Id);
            if (this.Parent is Panel parent)
            {
                parent.Children.Remove(this);
            }
        }
    }
}
