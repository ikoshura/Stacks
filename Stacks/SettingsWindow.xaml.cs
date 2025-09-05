// FILE: Stacks/SettingsWindow.xaml.cs

using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using Point = System.Windows.Point;

namespace Stacks
{
    public partial class SettingsWindow : FluentWindow
    {
        private Point? _initialPosition;
        private bool _isClosing = false;

        public SettingsWindow()
        {
            InitializeComponent();

            this.Deactivated += (s, e) =>
            {
                if (!_isClosing)
                {
                    this.Close();
                }
            };

            this.Loaded += SettingsWindow_Loaded;
        }

        public void ShowAt(Point cursorPosition)
        {
            _initialPosition = cursorPosition;
            this.Show();
            this.Activate();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }


        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_initialPosition.HasValue)
            {
                PositionWindow(_initialPosition.Value);
            }
            LoadSettingsToUI();
        }

        private void PositionWindow(Point cursorPosition)
        {
            var workArea = SystemParameters.WorkArea;
            const double screenMargin = 12.0;

            this.UpdateLayout();

            double left = cursorPosition.X - (this.ActualWidth / 2);
            double top = cursorPosition.Y - this.ActualHeight - 20;

            if (left + this.ActualWidth > workArea.Right) left = workArea.Right - this.ActualWidth - screenMargin;
            if (left < workArea.Left) left = workArea.Left + screenMargin;
            if (top < workArea.Top) top = workArea.Top + screenMargin;
            if (top + this.ActualHeight > workArea.Bottom) top = workArea.Bottom - this.ActualHeight - screenMargin;

            this.Left = left;
            this.Top = top;
        }

        private void LoadSettingsToUI()
        {
            var settings = SettingsManager.Current;
            SourceFolderTextBlock.Text = settings.SourceFolderPath;
            StartupToggleButton.IsChecked = settings.RunAtStartup;
            ThemeComboBox.SelectedIndex = (int)settings.Theme;
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
            _isClosing = true;

            var settings = SettingsManager.Current;
            settings.SourceFolderPath = SourceFolderTextBlock.Text;
            settings.RunAtStartup = StartupToggleButton.IsChecked == true;
            settings.Theme = (AppTheme)ThemeComboBox.SelectedIndex;

            StartupManager.SetStartup(settings.RunAtStartup);
            SettingsManager.Save();

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isClosing = true;
            // Revert to saved theme if user cancels
            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(SettingsManager.Current.Theme);
            }
            this.Close();
        }

        private void ThemeComboBox_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // PERBAIKAN: Use the fully qualified name for ComboBox to resolve ambiguity
            if (sender is not System.Windows.Controls.ComboBox { IsLoaded: true } comboBox) return;

            var selectedTheme = (AppTheme)comboBox.SelectedIndex;
            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(selectedTheme);
            }
        }
    }
}