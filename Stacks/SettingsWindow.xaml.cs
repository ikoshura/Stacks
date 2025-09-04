// FILE: Stacks/SettingsWindow.xaml.cs

using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Stacks
{
    public partial class SettingsWindow : FluentWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.Loaded += SettingsWindow_Loaded;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettingsToUI();
        }

        private void LoadSettingsToUI()
        {
            var settings = SettingsManager.Current;
            SourceFolderTextBlock.Text = settings.SourceFolderPath;
            StartupToggleButton.IsChecked = settings.RunAtStartup;
            ThemeComboBox.SelectedIndex = (int)settings.Theme;
            BackdropToggleButton.IsChecked = settings.UseAcrylic;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = SourceFolderTextBlock.Text
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SourceFolderTextBlock.Text = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = SettingsManager.Current;
            settings.SourceFolderPath = SourceFolderTextBlock.Text;
            settings.RunAtStartup = StartupToggleButton.IsChecked == true;
            settings.Theme = (AppTheme)ThemeComboBox.SelectedIndex;
            settings.UseAcrylic = BackdropToggleButton.IsChecked == true;

            StartupManager.SetStartup(settings.RunAtStartup);
            SettingsManager.Save();

            if (Application.Current is App app)
            {
                app.ApplyTheme(settings.Theme);
            }

            this.Hide(); // Sembunyikan jendela setelah menyimpan
        }

        private void ThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox { IsLoaded: true } comboBox) return;
            var selectedTheme = (AppTheme)comboBox.SelectedIndex;
            if (Application.Current is App app)
            {
                app.ApplyTheme(selectedTheme);
            }
        }

        private void BackdropToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
        }

        private void BackdropToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) WindowBackdrop.RemoveBackdrop(this);
        }
    }
}