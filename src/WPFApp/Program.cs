using System;
using System.Windows;
using System.Net;
namespace HitsDownloadManager.WPFApp
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            // Enable TLS 1.2 and 1.3 for HTTPS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
