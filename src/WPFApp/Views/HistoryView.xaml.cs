using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HitsDownloadManager.DownloadEngine;
using HitsDownloadManager.WPFApp;
namespace HitsDownloadManager.Views
{
    public partial class HistoryView : UserControl
    {
        private List<DownloadHistoryItem> _allHistoryItems = new();
        private List<DownloadHistoryItem> _filteredHistoryItems = new();
        private DownloadManager _downloadManager;
        private bool _isInitializing = true; // Flag to prevent filter triggers during load
        public HistoryView()
        {
            InitializeComponent();
            Loaded += HistoryView_Loaded;
        }
        private void HistoryView_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[HistoryView] View loaded, getting DownloadManager...");
            // Get DownloadManager from MainWindow
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                _downloadManager = mainWindow.GetDownloadManager();
                LoadHistory();
                _isInitializing = false; // Allow filters to work after initial load
                Debug.WriteLine("[HistoryView] Initialization complete");
            }
        }
        private void LoadHistory()
        {
            if (_downloadManager == null) return;
            Debug.WriteLine("[HistoryView] Loading history...");
            _allHistoryItems.Clear();
            // Get all tasks from DownloadManager
            var allTasks = _downloadManager.GetAllTasks();
            Debug.WriteLine($"[HistoryView] Found {allTasks.Count} total tasks");
            foreach (var task in allTasks)
            {
                Debug.WriteLine($"[HistoryView] Task: {task.Filename}, Status: {task.Status}, Created: {task.CreatedAt}");
                _allHistoryItems.Add(new DownloadHistoryItem
                {
                    Id = task.Id,
                    FileName = task.Filename,
                    Url = task.Url,
                    TotalBytes = task.TotalBytes,
                    Status = task.Status.ToString(), // Convert enum to string
                    DateAdded = task.CreatedAt,
                    DateCompleted = task.CompletedAt
                });
            }
            Debug.WriteLine($"[HistoryView] Loaded {_allHistoryItems.Count} history items");
            ApplyFilters();
            UpdateStatistics();
        }
        private void ApplyFilters()
        {
            Debug.WriteLine("[HistoryView] ApplyFilters called");
            _filteredHistoryItems = _allHistoryItems.ToList();
            Debug.WriteLine($"[HistoryView] Starting with {_filteredHistoryItems.Count} items");
            // Apply search filter
            var searchText = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                _filteredHistoryItems = _filteredHistoryItems
                    .Where(h => h.FileName.ToLower().Contains(searchText) ||
                                h.Url.ToLower().Contains(searchText))
                    .ToList();
                Debug.WriteLine($"[HistoryView] After search filter: {_filteredHistoryItems.Count} items");
            }
            // Apply date filter
            var dateFilter = (DateFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            Debug.WriteLine($"[HistoryView] Date filter: {dateFilter}");
            var now = DateTime.Now;
            _filteredHistoryItems = dateFilter switch
            {
                "Today" => _filteredHistoryItems.Where(h => h.DateAdded.Date == now.Date).ToList(),
                "Last 7 Days" => _filteredHistoryItems.Where(h => h.DateAdded >= now.AddDays(-7)).ToList(),
                "Last 30 Days" => _filteredHistoryItems.Where(h => h.DateAdded >= now.AddDays(-30)).ToList(),
                _ => _filteredHistoryItems
            };
            Debug.WriteLine($"[HistoryView] After date filter: {_filteredHistoryItems.Count} items");
            // Apply status filter
            var statusFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            Debug.WriteLine($"[HistoryView] Status filter: {statusFilter}");
            if (statusFilter != null && statusFilter != "All Status")
            {
                _filteredHistoryItems = _filteredHistoryItems
                    .Where(h => h.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Debug.WriteLine($"[HistoryView] After status filter: {_filteredHistoryItems.Count} items");
            }
            Debug.WriteLine($"[HistoryView] Final filtered count: {_filteredHistoryItems.Count}");
            HistoryDataGrid.ItemsSource = _filteredHistoryItems;
        }
        private void UpdateStatistics()
        {
            var completed = _allHistoryItems.Count(h => h.Status == "Completed");
            var totalSize = _allHistoryItems.Sum(h => h.TotalBytes);
            var successRate = _allHistoryItems.Count > 0
                ? (double)completed / _allHistoryItems.Count * 100
                : 0;
            TotalDownloadsText.Text = _allHistoryItems.Count.ToString();
            TotalSizeText.Text = FormatBytes(totalSize);
            SuccessRateText.Text = $"{successRate:F1}%";
            Debug.WriteLine($"[HistoryView] Statistics - Total: {_allHistoryItems.Count}, Completed: {completed}, Success Rate: {successRate:F1}%");
        }
        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing) return;
            Debug.WriteLine("[HistoryView] Search text changed");
            ApplyFilters();
        }
        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            Debug.WriteLine("[HistoryView] Date filter selection changed");
            ApplyFilters();
        }
        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            Debug.WriteLine("[HistoryView] Status filter selection changed");
            ApplyFilters();
        }
        private void ReDownload_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as DownloadHistoryItem;
            if (item != null)
            {
                Debug.WriteLine($"[HistoryView] Re-download clicked for: {item.FileName}");
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.AddDownloadFromHistory(item.Url);
            }
        }
        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as DownloadHistoryItem;
            if (item != null)
            {
                var result = MessageBox.Show(
                    $"Delete '{item.FileName}' from history?\n\nThis will not delete the downloaded file.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Debug.WriteLine($"[HistoryView] Deleting history item: {item.FileName}");
                    _downloadManager?.RemoveTask(item.Id);
                    LoadHistory();
                }
            }
        }
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear ALL download history?\n\nThis will not delete downloaded files.",
                "Confirm Clear History",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Debug.WriteLine("[HistoryView] Clearing all history");
                _downloadManager?.ClearHistory();
                LoadHistory();
            }
        }
        public void RefreshHistory()
        {
            Debug.WriteLine("[HistoryView] RefreshHistory called");
            LoadHistory();
        }
    }
    public class DownloadHistoryItem
    {
        public string Id { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Url { get; set; } = "";
        public long TotalBytes { get; set; }
        public string Status { get; set; } = "";
        public DateTime DateAdded { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string FileSizeFormatted => FormatBytes(TotalBytes);
        public string DateAddedFormatted => DateAdded.ToString("MMM dd, yyyy HH:mm");
        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
