using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace HitsDownloadManager.WPFApp.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _downloadFolder = @"C:\Downloads";
        private int _maxConcurrentDownloads = 3;
        private int _speedLimitKBps = 1024;
        private bool _enableSpeedLimit = false;
        private bool _autoStartDownloads = true;
        private bool _showNotifications = true;
        public string DownloadFolder
        {
            get => _downloadFolder;
            set { _downloadFolder = value; OnPropertyChanged(); }
        }
        public int MaxConcurrentDownloads
        {
            get => _maxConcurrentDownloads;
            set { _maxConcurrentDownloads = value; OnPropertyChanged(); }
        }
        public int SpeedLimitKBps
        {
            get => _speedLimitKBps;
            set { _speedLimitKBps = value; OnPropertyChanged(); }
        }
        public bool EnableSpeedLimit
        {
            get => _enableSpeedLimit;
            set { _enableSpeedLimit = value; OnPropertyChanged(); }
        }
        public bool AutoStartDownloads
        {
            get => _autoStartDownloads;
            set { _autoStartDownloads = value; OnPropertyChanged(); }
        }
        public bool ShowNotifications
        {
            get => _showNotifications;
            set { _showNotifications = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}