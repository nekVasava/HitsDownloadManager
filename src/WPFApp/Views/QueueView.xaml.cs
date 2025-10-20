using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
namespace HitsDownloadManager.WPFApp.Views
{
    public partial class QueueView : Window
    {
        private DownloadManager _downloadManager;
        public QueueView()
        {
            InitializeComponent();
            _downloadManager = ((MainWindow)Application.Current.MainWindow).GetDownloadManager();
            StartAllButton.Click += StartAllButton_Click;
            PauseAllButton.Click += PauseAllButton_Click;
            CancelAllButton.Click += CancelAllButton_Click;
            ClearCompletedButton.Click += ClearCompletedButton_Click;
            Debug.WriteLine("[QueueView] Initialized");
        }
        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[QueueView] Start All clicked");
            _downloadManager.StartAll();
        }
        private void PauseAllButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[QueueView] Pause All clicked");
            _downloadManager.PauseAll();
        }
        private void CancelAllButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[QueueView] Cancel All clicked");
            // TODO: Implement CancelAll functionality calling DownloadManager method(s)
        }
        private void ClearCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[QueueView] Clear Completed clicked");
            // TODO: Implement ClearCompleted functionality calling DownloadManager method(s)
        }
    }
}
private void CancelAllButton_Click(object sender, RoutedEventArgs e)
{
    Debug.WriteLine("[QueueView] Cancel All clicked");
    _downloadManager.CancelAll();
}
private void ClearCompletedButton_Click(object sender, RoutedEventArgs e)
{
    Debug.WriteLine("[QueueView] Clear Completed clicked");
    _downloadManager.ClearCompleted();
}
