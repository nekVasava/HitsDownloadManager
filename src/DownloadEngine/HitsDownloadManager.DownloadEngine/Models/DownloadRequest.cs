namespace HitsDownloadManager.DownloadEngine.Models
{
    public class DownloadRequest
    {
        public string Url { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public int MaxConnections { get; set; } = 8;
        public bool ResumeSupported { get; set; } = true;
    }
}
