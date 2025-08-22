using System.Windows;
using System.Windows.Controls; // <-- Pastikan using ini ada
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Stacks
{
    public partial class SettingsWindow : MicaWPF.Controls.MicaWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettingsToUI();
        }

        private void LoadSettingsToUI()
        {
            var settings = SettingsManager.Current;

            SourceFolderTextBox.Text = settings.SourceFolderPath;
            StartupCheckBox.IsChecked = settings.RunAtStartup;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = SourceFolderTextBox.Text
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SourceFolderTextBox.Text = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = SettingsManager.Current;

            settings.SourceFolderPath = SourceFolderTextBox.Text;
            settings.RunAtStartup = StartupCheckBox.IsChecked == true;

            StartupManager.SetStartup(settings.RunAtStartup);
            SettingsManager.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Kembalikan tema ke pengaturan yang tersimpan sebelum menutup
            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(SettingsManager.Current.Theme);
            }
            this.DialogResult = false;
            this.Close();
        }
    }
}