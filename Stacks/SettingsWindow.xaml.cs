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
            SystemThemeRadio.IsChecked = settings.Theme == AppTheme.System;
            LightThemeRadio.IsChecked = settings.Theme == AppTheme.Light;
            DarkThemeRadio.IsChecked = settings.Theme == AppTheme.Dark;
            StartupCheckBox.IsChecked = settings.RunAtStartup;
            VerticalOffsetTextBox.Text = settings.VerticalOffset.ToString();
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded) return;

            AppTheme selectedTheme = AppTheme.System;
            if (LightThemeRadio.IsChecked == true) selectedTheme = AppTheme.Light;
            else if (DarkThemeRadio.IsChecked == true) selectedTheme = AppTheme.Dark;

            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(selectedTheme);
            }
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
            if (SystemThemeRadio.IsChecked == true) settings.Theme = AppTheme.System;
            else if (LightThemeRadio.IsChecked == true) settings.Theme = AppTheme.Light;
            else settings.Theme = AppTheme.Dark;
            settings.RunAtStartup = StartupCheckBox.IsChecked == true;

            if (int.TryParse(VerticalOffsetTextBox.Text, out int newOffset))
            {
                settings.VerticalOffset = newOffset;
            }

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