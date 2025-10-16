using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
namespace HitsDownloadManager.WPFApp
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode = true; // Default is Dark Mode
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.SettingsViewModel();
        }
        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Switch to Light Mode
            SwitchTheme(false);
        }
        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // Switch to Dark Mode
            SwitchTheme(true);
        }
        private void SwitchTheme(bool darkMode)
        {
            isDarkMode = darkMode;
            var resources = Application.Current.Resources;
            if (darkMode)
            {
                // Dark Theme Colors
                resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
                resources["HoverAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                resources["PressedAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                resources["DisabledAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#546E7A"));
                resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E2E"));
                resources["PanelBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A3B"));
                resources["InputBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F2F3F"));
                resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#404050"));
                resources["TextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B0"));
                resources["HeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["TabBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A3B"));
                resources["TabHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A4B"));
                resources["TabSelectedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
            }
            else
            {
                // Light Theme Colors
                resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
                resources["HoverAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                resources["PressedAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                resources["DisabledAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90CAF9"));
                resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                resources["PanelBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["InputBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                resources["TextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121"));
                resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
                resources["HeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121"));
                resources["TabBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                resources["TabHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                resources["TabSelectedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));
            }
        }
    }
}
