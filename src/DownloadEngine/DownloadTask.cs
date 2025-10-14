using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace HitsDownloadManager.DownloadEngine
{
    public enum DownloadStatus
    {
        Pending,
        Downloading,
        Paused,
        Completed,
        Failed,
        Cancelled
    }
    public enum Priority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
    public class DownloadTask : INotifyPropertyChanged
    {
        private string _speed = "0 KB/s";
        private string _timeRemaining = "Calculating...";
        private long _downloadedBytes;
        private long _totalBytes;
        private DownloadStatus _status;
        private int _retryCount;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Url { get; set; }
        public string DestinationPath { get; set; }
        public string Filename { get; set; }
        public string FileExtension => System.IO.Path.GetExtension(Filename)?.ToUpper().TrimStart('.') ?? "Unknown";
        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                _totalBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalSizeFormatted));
                OnPropertyChanged(nameof(Progress));
            }
        }
        public long DownloadedBytes
        {
            get => _downloadedBytes;
            set
            {
                _downloadedBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(DownloadedSizeFormatted));
            }
        }
        public DownloadStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                if (value == DownloadStatus.Failed || value == DownloadStatus.Completed)
                {
                    TimeRemaining = value == DownloadStatus.Completed ? "0s" : "-";
                }
            }
        }
        public Priority Priority { get; set; } = Priority.Normal;
        public int RetryCount
        {
            get => _retryCount;
            set
            {
                _retryCount = value;
                OnPropertyChanged();
            }
        }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
        public double Progress => TotalBytes > 0 ? (DownloadedBytes * 100.0) / TotalBytes : 0;
        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }
        public string TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }
        public string TotalSizeFormatted => FormatBytes(TotalBytes);
        public string DownloadedSizeFormatted => FormatBytes(DownloadedBytes);
        private string FormatBytes(long bytes)
        {
            if (bytes == 0) return "Unknown";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

