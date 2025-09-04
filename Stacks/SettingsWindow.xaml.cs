using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Drawing; // For Rectangle, Point
using System.Windows.Forms; // For NotifyIcon, MouseButtons
using System.Windows.Media; // For VisualTreeHelper (WPF)
using System.Reflection;

namespace Stacks
{
    public partial class SettingsWindow : MicaWPF.Controls.MicaWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettingsToUI();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
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
            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(SettingsManager.Current.Theme);
            }
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Show the settings window near the tray icon, instead of (0,0).
        /// </summary>
        public void ShowNearTray(NotifyIcon trayIcon)
        {
            if (trayIcon == null)
            {
                this.Show();
                return;
            }

            // Use reflection to get the NotifyIcon bounds (tray position)
            Rectangle trayBounds = Rectangle.Empty;
            try
            {
                var fi = trayIcon.GetType().GetProperty("Bounds", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    trayBounds = (Rectangle)fi.GetValue(trayIcon);
                }
            }
            catch
            {
                // fallback to mouse position
                trayBounds = new Rectangle(System.Windows.Forms.Control.MousePosition, new System.Drawing.Size(1, 1));
            }

            if (trayBounds == Rectangle.Empty)
            {
                trayBounds = new Rectangle(System.Windows.Forms.Control.MousePosition, new System.Drawing.Size(1, 1));
            }

            // Get DPI scaling for correct positioning
            var dpi = VisualTreeHelper.GetDpi(this);

            double screenX = trayBounds.X / dpi.DpiScaleX;
            double screenY = trayBounds.Y / dpi.DpiScaleY;

            // Position window just above tray icon
            this.Left = screenX - (this.Width / 2);
            this.Top = screenY - this.Height - 10;

            this.Show();
            this.Activate();
        }
    }
}
