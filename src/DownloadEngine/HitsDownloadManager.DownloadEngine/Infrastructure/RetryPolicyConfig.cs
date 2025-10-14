namespace HitsDownloadManager.DownloadEngine.Infrastructure
{
    public class RetryPolicyConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxAttempts { get; set; } = 3;
        public int DelaySeconds { get; set; } = 5;
    }
}
