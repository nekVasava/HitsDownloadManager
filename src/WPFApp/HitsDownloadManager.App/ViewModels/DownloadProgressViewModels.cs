namespace HitsDownloadManager.App.ViewModels;
public class DownloadProgressItem
{
    public string FileName { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public double DownloadSpeedBytesPerSec { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public int SegmentsCompleted { get; set; }
    public int TotalSegments { get; set; }
}
public class DownloadProgressListViewModel
{
    public ObservableCollection<DownloadProgressItem> Downloads { get; set; } = new();
    public void UpdateProgress(string fileName, double progress, double speed, TimeSpan eta, int completedSegments, int totalSegments)
    {
        var item = Downloads.FirstOrDefault(d => d.FileName == fileName);
        if (item == null)
        {
            item = new DownloadProgressItem { FileName = fileName };
            Downloads.Add(item);
        }
        item.ProgressPercentage = progress;
        item.DownloadSpeedBytesPerSec = speed;
        item.EstimatedTimeRemaining = eta;
        item.SegmentsCompleted = completedSegments;
        item.TotalSegments = totalSegments;
    }
}
