using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Stacks
{
    public static class StartupManager
    {
        private const string AppName = "Stacks";
        private static readonly string AppPath =
            Assembly.GetEntryAssembly()?.Location ??
            Process.GetCurrentProcess().MainModule.FileName;

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
