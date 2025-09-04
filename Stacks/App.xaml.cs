// FILE: Stacks/App.xaml.cs
using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Wpf.Ui.Appearance;

namespace Stacks
{
    public partial class App : Application
    {
        private readonly IHost _host;
        private Tray? _tray;

        public App(IHost host)
        {
            _host = host;
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsManager.Load();
            var theme = SettingsManager.Current.Theme;
            if (Application.Current is App app)
            {
                app.ApplyTheme(theme);
            }

            _host.Start();
            _tray = _host.Services.GetService(typeof(Tray)) as Tray;
            if (_tray == null) Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.StopAsync().Wait();
            _host.Dispose();
            base.OnExit(e);
        }

        public void ApplyTheme(AppTheme theme)
        {
            var themeToApply = theme switch
            {
                AppTheme.Light => ApplicationTheme.Light,
                AppTheme.Dark => ApplicationTheme.Dark,
                _ => ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light
            };
            ApplicationThemeManager.Apply(themeToApply);
        }

        public static Point GetMousePosition()
        {
            GetCursorPos(out Win32Point w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point { public int X; public int Y; }
    }
}