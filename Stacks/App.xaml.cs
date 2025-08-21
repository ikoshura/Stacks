using System;
using System.Windows;
using Microsoft.Win32;

// Perbaikan: Mendefinisikan alias untuk System.Windows.Point
using Point = System.Windows.Point;

namespace Stacks
{
    // Perbaikan: Spesifikasikan bahwa ini adalah aplikasi WPF
    public partial class App : System.Windows.Application
    {
        private MainWindow? _mainWindow;
        private FanView? _fanView;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SettingsManager.Load();
            ApplyTheme(SettingsManager.Current.Theme);

            _mainWindow = new MainWindow();
            _fanView = new FanView();

            _fanView.Deactivated += FanView_Deactivated;

            _mainWindow.Show();
            _mainWindow.WidgetClicked += OnWidgetClicked;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void FanView_Deactivated(object sender, EventArgs e)
        {
            _fanView?.Hide();
        }

        private void OnWidgetClicked()
        {
            if (_fanView == null || _mainWindow == null) return;

            if (_fanView.IsVisible)
            {
                _fanView.Hide();
            }
            else
            {
                // Menggunakan alias 'Point' yang sudah didefinisikan
                Point widgetPosition = _mainWindow.PointToScreen(new Point(0, 0));
                _fanView.ShowAt(widgetPosition, _mainWindow.ActualWidth);
            }
        }

        public void ApplyTheme(AppTheme theme)
        {
            ResourceDictionary? themeDictionary = null;
            var currentTheme = GetCurrentSystemTheme();

            if (theme == AppTheme.System)
            {
                theme = currentTheme;
            }

            switch (theme)
            {
                case AppTheme.Light:
                    themeDictionary = new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) };
                    break;
                case AppTheme.Dark:
                    themeDictionary = new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) };
                    break;
            }

            if (themeDictionary != null)
            {
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
                Resources.MergedDictionaries.Add(themeDictionary);
            }
        }

        private AppTheme GetCurrentSystemTheme()
        {
            try
            {
                const string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                const string registryValueName = "AppsUseLightTheme";
                using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath);
                var registryValue = key?.GetValue(registryValueName);

                return (registryValue is int value && value == 0) ? AppTheme.Dark : AppTheme.Light;
            }
            catch
            {
                return AppTheme.Light;
            }
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General && SettingsManager.Current.Theme == AppTheme.System)
            {
                ApplyTheme(AppTheme.System);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            base.OnExit(e);
        }
    }
}