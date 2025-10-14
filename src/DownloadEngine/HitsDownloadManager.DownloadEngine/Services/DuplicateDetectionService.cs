using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using HitsDownloadManager.DownloadEngine.Models;


namespace HitsDownloadManager.DownloadEngine.Services
{
    public class DuplicateDetectionService
    {
        private readonly string _downloadHistoryPath;
        private readonly HashSet<string> _downloadedUrls;
        private readonly HashSet<string> _downloadedFileHashes;

        public DuplicateDetectionService()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _downloadHistoryPath = Path.Combine(appDataFolder, "HitsDownloadManager", "DownloadHistory.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_downloadHistoryPath)!);

            _downloadedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _downloadedFileHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            LoadHistory();
        }

        private void LoadHistory()
        {
            if (!File.Exists(_downloadHistoryPath))
                return;

            try
            {
                var content = File.ReadAllText(_downloadHistoryPath);
                var records = JsonSerializer.Deserialize<List<DownloadRecord>>(content);
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        _downloadedUrls.Add(record.Url);
                        _downloadedFileHashes.Add(record.FileHash);
                    }
                }
            }
            catch
            {
                // Log or ignore errors
            }
        }

        public bool IsDuplicate(DownloadRequest request)
        {
            if (_downloadedUrls.Contains(request.Url))
            {
                Console.WriteLine($"Duplicate detected by URL: {request.Url}");
                return true;
            }

            if (File.Exists(request.DestinationPath))
            {
                var hash = ComputePartialFileHash(request.DestinationPath);
                if (_downloadedFileHashes.Contains(hash))
                {
                    Console.WriteLine($"Duplicate detected by file hash: {hash}");
                    return true;
                }
            }

            return false;
        }

        public void RecordDownload(DownloadRequest request)
        {
            var hash = ComputePartialFileHash(request.DestinationPath);
            _downloadedUrls.Add(request.Url);
            _downloadedFileHashes.Add(hash);

            SaveHistory();
        }

        private void SaveHistory()
        {
            try
            {
                var records = new List<DownloadRecord>();
                foreach (var url in _downloadedUrls)
                {
                    records.Add(new DownloadRecord
                    {
                        Url = url,
                        FileHash = string.Empty // File hash tracking simplified
                    });
                }
                var content = JsonSerializer.Serialize(records);
                File.WriteAllText(_downloadHistoryPath, content);
            }
            catch
            {
                // Log or ignore errors
            }
        }

        private static string ComputePartialFileHash(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            int hashLength = 1024;
            byte[] buffer = new byte[hashLength];
            int bytesRead = stream.Read(buffer, 0, hashLength);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(buffer, 0, bytesRead)).Replace("-", "");
        }

        private class DownloadRecord
        {
            public string Url { get; set; } = string.Empty;
            public string FileHash { get; set; } = string.Empty;
        }
    }
}
