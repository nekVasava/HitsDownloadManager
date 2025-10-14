using System.Windows;
using HitsDownloadManager.WPFApp.ViewModels;
namespace HitsDownloadManager.WPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
