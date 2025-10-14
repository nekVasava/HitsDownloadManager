using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HitsDownloadManager.DownloadEngine;
namespace HitsDownloadManager.WPFApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DownloadManager _downloadManager;
        private DownloadTask _selectedDownload;
        private string _newDownloadUrl;
        private string _downloadDirectory;
        public ObservableCollection<DownloadTask> Downloads { get; }
        public DownloadTask SelectedDownload
        {
            get => _selectedDownload;
            set
            {
                SetProperty(ref _selectedDownload, value);
                CommandManager.InvalidateRequerySuggested(); // Force button refresh
            }
        }
        public string NewDownloadUrl
        {
            get => _newDownloadUrl;
            set => SetProperty(ref _newDownloadUrl, value);
        }
        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set => SetProperty(ref _downloadDirectory, value);
        }
        public ICommand AddDownloadCommand { get; }
        public ICommand PauseDownloadCommand { get; }
        public ICommand ResumeDownloadCommand { get; }
        public ICommand CancelDownloadCommand { get; }
        public ICommand BrowseDirectoryCommand { get; }
        public MainViewModel()
        {
            Downloads = new ObservableCollection<DownloadTask>();
            _downloadManager = new DownloadManager(maxConcurrentDownloads: 5);
            _downloadDirectory = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            _downloadManager.DownloadProgressChanged += OnDownloadProgressChanged;
            _downloadManager.DownloadCompleted += OnDownloadCompleted;
            _downloadManager.DownloadFailed += OnDownloadFailed;
            AddDownloadCommand = new RelayCommand(AddDownload, CanAddDownload);
            PauseDownloadCommand = new RelayCommand(PauseDownload, CanPauseDownload);
            ResumeDownloadCommand = new RelayCommand(ResumeDownload, CanResumeDownload);
            CancelDownloadCommand = new RelayCommand(CancelDownload, CanCancelDownload);
            BrowseDirectoryCommand = new RelayCommand(BrowseDirectory);
        }
        private void AddDownload(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewDownloadUrl))
                return;
            try
            {
                var uri = new System.Uri(NewDownloadUrl);
                var filename = System.IO.Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrEmpty(filename))
                    filename = "download_" + System.DateTime.Now.Ticks;
                var destinationPath = System.IO.Path.Combine(DownloadDirectory, filename);
                var downloadId = _downloadManager.AddDownload(NewDownloadUrl, destinationPath);
                if (downloadId != null)
                {
                    var task = _downloadManager.GetDownload(downloadId);
                    Application.Current.Dispatcher.Invoke(() => Downloads.Add(task));
                    NewDownloadUrl = string.Empty;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error adding download: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanAddDownload(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewDownloadUrl) && 
                   System.Uri.TryCreate(NewDownloadUrl, System.UriKind.Absolute, out _);
        }
        private void PauseDownload(object parameter)
        {
            if (SelectedDownload != null)
            {
                _downloadManager.PauseDownload(SelectedDownload.Id);
            }
        }
        private bool CanPauseDownload(object parameter)
        {
            return SelectedDownload != null && SelectedDownload.Status == DownloadStatus.Downloading;
        }
        private void ResumeDownload(object parameter)
        {
            if (SelectedDownload != null)
            {
                _downloadManager.ResumeDownload(SelectedDownload.Id);
            }
        }
        private bool CanResumeDownload(object parameter)
        {
            return SelectedDownload != null && 
                   (SelectedDownload.Status == DownloadStatus.Paused || 
                    SelectedDownload.Status == DownloadStatus.Failed);
        }
        private void CancelDownload(object parameter)
        {
            if (SelectedDownload != null)
            {
                _downloadManager.CancelDownload(SelectedDownload.Id);
                Application.Current.Dispatcher.Invoke(() => Downloads.Remove(SelectedDownload));
            }
        }
        private bool CanCancelDownload(object parameter)
        {
            return SelectedDownload != null && 
                   SelectedDownload.Status != DownloadStatus.Completed;
        }
        private void BrowseDirectory(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = DownloadDirectory
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DownloadDirectory = dialog.SelectedPath;
            }
        }
        private void OnDownloadProgressChanged(object sender, DownloadTask task)
        {
            // Task already updates via INotifyPropertyChanged
        }
        private void OnDownloadCompleted(object sender, DownloadTask task)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Download completed: {task.Filename}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        private void OnDownloadFailed(object sender, DownloadTask task)
        {
            // Don't show popup for failed - it will auto-retry
        }
    }
}

