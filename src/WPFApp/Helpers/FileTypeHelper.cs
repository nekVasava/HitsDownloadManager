using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Windows.Media;
namespace HitsDownloadManager.Helpers
{
    public static class FileTypeHelper
    {
        public static (PackIconKind Icon, Brush Color) GetFileTypeIconAndColor(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension switch
            {
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => 
                    (PackIconKind.FolderZip, new SolidColorBrush(Color.FromRgb(255, 152, 0))),
                ".pdf" or ".doc" or ".docx" or ".txt" or ".rtf" => 
                    (PackIconKind.FileDocument, new SolidColorBrush(Color.FromRgb(33, 150, 243))),
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".m4a" => 
                    (PackIconKind.FileMusic, new SolidColorBrush(Color.FromRgb(156, 39, 176))),
                ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" => 
                    (PackIconKind.FileVideo, new SolidColorBrush(Color.FromRgb(244, 67, 54))),
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => 
                    (PackIconKind.FileImage, new SolidColorBrush(Color.FromRgb(76, 175, 80))),
                ".exe" or ".msi" or ".apk" => 
                    (PackIconKind.ApplicationCog, new SolidColorBrush(Color.FromRgb(255, 193, 7))),
                _ => (PackIconKind.FileDownload, new SolidColorBrush(Color.FromRgb(158, 158, 158)))
            };
        }
    }
}
