using System;
using Microsoft.Win32;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Point = System.Windows.Point;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Stacks
{
    // PERBAIKAN: Tentukan secara eksplisit untuk menghindari ambiguitas
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

            // PERBAIKAN: Menggunakan SystemEvents untuk mendeteksi perubahan tema sistem, ini lebih baik untuk aplikasi tray
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (_fanView is null) return;

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

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is SettingsWindow)
                {
                    window.Activate();
                    return;
                }
            }

            var cursorPosition = GetMousePosition();
            new SettingsWindow().ShowAt(cursorPosition);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) { Shutdown(); }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged; // Hapus langganan saat keluar
            base.OnExit(e);
        }

        public void ApplyTheme(AppTheme theme)
        {
            ApplicationTheme themeToApply = theme switch
            {
                AppTheme.Light => ApplicationTheme.Light,
                AppTheme.Dark => ApplicationTheme.Dark,
                _ => ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light
            };

            ApplicationThemeManager.Apply(themeToApply);
        }

        // PERBAIKAN: Mengembalikan metode ini untuk menangani perubahan tema OS
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
    }
}