// FILE: Stacks/App.xaml.cs

using System;
using Microsoft.Win32;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Point = System.Windows.Point;
using Wpf.Ui.Appearance;

namespace Stacks
{
    public partial class App : System.Windows.Application
    {
        private FanView? _fanView;
        private TaskbarIcon? _trayIcon;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

            SettingsManager.Load();
            ApplyTheme(SettingsManager.Current.Theme);

            _mainWindow = new MainWindow();
            _mainWindow.Show();

            _fanView = new FanView();
            _fanView.ViewDeactivated += OnFanViewDeactivated;

            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void OnFanViewDeactivated()
        {
            if (_mainWindow != null)
            {
                _mainWindow.IsFanViewOpen = false;
            }
        }

        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (_fanView is null || _mainWindow is null) return;

            var cursorPosition = GetMousePosition();
            _fanView.ToggleAt(cursorPosition);

            _mainWindow.IsFanViewOpen = _fanView.IsVisible;
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
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            base.OnExit(e);
        }

        public void ApplyTheme(AppTheme theme)
        {
            var themeToApply = theme switch
            {
                AppTheme.Light => ApplicationTheme.Light,
                AppTheme.Dark => ApplicationTheme.Dark,
                _ =>
                    ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark
                        ? ApplicationTheme.Dark
                        : ApplicationTheme.Light
            };

            // This is the correct and simplified way to apply the theme and accent.
            // The 'updateAccent' parameter ensures that the accent color is recalculated
            // based on the new 'themeToApply'.
            ApplicationThemeManager.Apply(themeToApply, updateAccent: true);
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (SettingsManager.Current.Theme == AppTheme.System)
            {
                if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
                {
                    Dispatcher.Invoke(() => ApplyTheme(AppTheme.System));
                }
            }
        }

        private Point GetMousePosition()
        {
            GetCursorPos(out Win32Point w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(
            System.Runtime.InteropServices.UnmanagedType.Bool
        )]
        internal static extern bool GetCursorPos(out Win32Point pt);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        }
    }
}