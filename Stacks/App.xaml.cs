using System;
using Microsoft.Win32;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Point = System.Windows.Point;

namespace Stacks
{
    public partial class App : System.Windows.Application
    {
        private FanView? _fanView;
        private TaskbarIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            SettingsManager.Load();
            ApplyTheme(SettingsManager.Current.Theme);
            _fanView = new FanView();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (_fanView != null)
            {
                if (_fanView.IsVisible)
                {
                    _fanView.Hide();
                }
                else
                {
                    var cursorPosition = GetMousePosition();
                    _fanView.ShowAt(cursorPosition);
                }
            }
        }

        #region Kode Lengkap Lainnya (Tidak Berubah)
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) { new SettingsWindow().ShowDialog(); }
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) { Shutdown(); }
        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            base.OnExit(e);
        }
        public void ApplyTheme(AppTheme theme)
        {
            ResourceDictionary? themeDictionary = null;
            var currentTheme = GetCurrentSystemTheme();
            if (theme == AppTheme.System) theme = currentTheme;
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
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/ModernStyles.xaml", UriKind.Relative) });
                Resources.MergedDictionaries.Add(themeDictionary);
            }
        }
        private AppTheme GetCurrentSystemTheme()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                using var key = Registry.CurrentUser.OpenSubKey(keyPath);
                var value = key?.GetValue("AppsUseLightTheme");
                return (value is int v && v == 0) ? AppTheme.Dark : AppTheme.Light;
            }
            catch { return AppTheme.Light; }
        }
        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General && SettingsManager.Current.Theme == AppTheme.System)
            {
                Dispatcher.Invoke(() => ApplyTheme(AppTheme.System));
            }
        }
        private Point GetMousePosition()
        {
            GetCursorPos(out Win32Point w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point pt);
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct Win32Point { public Int32 X; public Int32 Y; };
        #endregion
    }
}