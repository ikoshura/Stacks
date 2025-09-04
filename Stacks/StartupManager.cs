// FILE: Stacks/StartupManager.cs

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.IO; // <-- TAMBAHKAN BARIS INI

namespace Stacks
{
    public static class StartupManager
    {
        private const string AppName = "Stacks";
        private static readonly string AppPath = GetExecutablePath();

        private static string GetExecutablePath()
        {
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(path))
            {
                path = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(path) && Path.GetExtension(path).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return Path.ChangeExtension(path, ".exe");
            }

            return path ?? string.Empty;
        }

        public static void SetStartup(bool enable)
        {
            if (string.IsNullOrEmpty(AppPath)) return;

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (enable)
                {
                    key.SetValue(AppName, $"\"{AppPath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to update startup settings: {ex.Message}");
            }
        }
    }
}