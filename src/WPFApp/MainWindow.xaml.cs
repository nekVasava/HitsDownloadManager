using System;
using System.Windows;
namespace HitsDownloadManager.WPFApp
{
    public partial class MainWindow : Window
    {
        private bool _isDarkMode = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();
            // Load base colors
            mergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/Colors.xaml", UriKind.Relative) });
            // Load theme
            if (_isDarkMode)
            {
                mergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/Colors.Dark.xaml", UriKind.Relative) });
            }
            // Load styles
            mergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Themes/Styles.xaml", UriKind.Relative) });
        }
    }
}
