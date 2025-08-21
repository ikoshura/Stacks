using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Stacks
{
    public class SettingsManager
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Stacks",
            "settings.json"
        );

        private static SettingsManager? _instance;
        public static SettingsManager Instance => _instance ??= new SettingsManager();

        public static AppSettings Current => Instance.Settings;

        public AppSettings Settings { get; private set; }

        private SettingsManager()
        {
            Settings = new AppSettings();
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    Instance.Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                Instance.Settings = new AppSettings();
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Instance.Settings, options);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                // PERBAIKAN: Nama lengkap untuk MessageBox
                System.Windows.MessageBox.Show($"Failed to save settings: {ex.Message}");
            }
        }
    }
}