using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
namespace HitsDownloadManager.NativeMessaging
{
    class Program
    {
        static Stream stdin;
        static Stream stdout;
        static void Main(string[] args)
        {
            stdin = Console.OpenStandardInput();
            stdout = Console.OpenStandardOutput();
            try
            {
                Directory.CreateDirectory(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HitsDownloadManager"
                ));
                while (true)
                {
                    var message = ReadMessage();
                    if (message == null) break;
                    var response = ProcessMessage(message);
                    SendMessage(response);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }
        static string ReadMessage()
        {
            var lengthBytes = new byte[4];
            var read = stdin.Read(lengthBytes, 0, 4);
            if (read == 0) return null;
            var length = BitConverter.ToInt32(lengthBytes, 0);
            if (length == 0) return null;
            var buffer = new byte[length];
            stdin.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }
        static void SendMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var length = BitConverter.GetBytes(bytes.Length);
            stdout.Write(length, 0, 4);
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }
        static string ProcessMessage(string messageJson)
        {
            using var doc = JsonDocument.Parse(messageJson);
            var root = doc.RootElement;
            var action = root.GetProperty("action").GetString();
            switch (action)
            {
                case "ping":
                    return JsonSerializer.Serialize(new { status = "connected" });
                case "download":
                    var url = root.GetProperty("url").GetString();
                    var filename = root.GetProperty("filename").GetString();
                    var referrer = root.TryGetProperty("referrer", out var refProp) ? refProp.GetString() : "";
                    var headers = new Dictionary<string, string>();
                    if (root.TryGetProperty("headers", out var headersProp))
                    {
                        foreach (var h in headersProp.EnumerateObject())
                        {
                            headers[h.Name] = h.Value.GetString();
                        }
                    }
                    var prefs = new Dictionary<string, object>();
                    if (root.TryGetProperty("preferences", out var prefsProp))
                    {
                        foreach (var p in prefsProp.EnumerateObject())
                        {
                            prefs[p.Name] = p.Value.ToString();
                        }
                    }
                    LogDownload($"URL: {url}, File: {filename}, Referrer: {referrer}");
                    return JsonSerializer.Serialize(new
                    {
                        status = "accepted",
                        url,
                        filename,
                        downloadId = Guid.NewGuid().ToString()
                    });
                case "openSettings":
                    return JsonSerializer.Serialize(new { status = "opened" });
                default:
                    return JsonSerializer.Serialize(new { error = "Unknown action" });
            }
        }
        static void LogDownload(string message)
        {
            File.AppendAllText(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HitsDownloadManager", "downloads.log"),
                $"{DateTime.Now}: {message}\n"
            );
        }
        static void LogError(string message)
        {
            File.AppendAllText(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HitsDownloadManager", "native_host.log"),
                $"{DateTime.Now}: {message}\n"
            );
        }
    }
}
