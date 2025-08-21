using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

// Perbaikan: Mendefinisikan alias untuk menghindari konflik
using Point = System.Windows.Point;
using Icon = System.Drawing.Icon;

namespace Stacks
{
    public partial class FanView : MicaWPF.Controls.MicaWindow
    {
        public ObservableCollection<FileItem> Files { get; set; }

        private bool _isFirstTimeShowing = true;
        private Point _currentTargetPosition;
        private double _currentTargetWidth;

        public FanView()
        {
            InitializeComponent();
            Files = new ObservableCollection<FileItem>();
            FileItemsControl.ItemsSource = Files;
        }

        public void ShowAt(Point widgetPosition, double widgetWidth)
        {
            _currentTargetPosition = widgetPosition;
            _currentTargetWidth = widgetWidth;

            LoadFiles();
            this.DataContext = SettingsManager.Instance;

            if (_isFirstTimeShowing)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.ShowActivated = false;
                this.Show();
                this.Hide();

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    PositionWindow();
                    this.Show();
                    _isFirstTimeShowing = false;
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            else
            {
                PositionWindow();
                this.Show();
            }

            this.Activate();
        }

        private void PositionWindow()
        {
            var workArea = SystemParameters.WorkArea;
            this.Left = _currentTargetPosition.X + (_currentTargetWidth / 2) - (this.ActualWidth / 2) - 500;
            this.Top = _currentTargetPosition.Y - this.ActualHeight - SettingsManager.Current.VerticalOffset;

            if (this.Top < workArea.Top)
                this.Top = _currentTargetPosition.Y + 40;

            if (this.Left < workArea.Left) this.Left = workArea.Left;
            if (this.Left + this.ActualWidth > workArea.Right)
                this.Left = workArea.Right - this.ActualWidth;
        }

        private async void LoadFiles()
        {
            try
            {
                string sourcePath = SettingsManager.Current.SourceFolderPath;
                if (!Directory.Exists(sourcePath)) { Files.Clear(); return; }

                var filePaths = await Task.Run(() =>
                    Directory.GetFiles(sourcePath)
                             .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                             .Take(10).ToList());

                Files.Clear();
                foreach (var path in filePaths)
                {
                    Files.Add(new FileItem
                    {
                        FilePath = path,
                        FileName = Path.GetFileName(path),
                        Thumbnail = CreateThumbnail(path)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load files: {ex.Message}");
            }
        }

        private ImageSource? CreateThumbnail(string filePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.DecodePixelWidth = 80;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                try
                {
                    // PERBAIKAN: Memanggil method dari kelas Icon yang benar
                    using (Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch { return null; }
            }
        }

        // PERBAIKAN: Spesifikasikan System.Windows.Input.MouseEventArgs
        private void File_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element)
            {
                var fileItem = element.DataContext as FileItem;
                if (fileItem != null && fileItem.FilePath != null)
                {
                    var data = new System.Windows.DataObject(System.Windows.DataFormats.FileDrop, new string[] { fileItem.FilePath });
                    System.Windows.DragDrop.DoDragDrop(element, data, System.Windows.DragDropEffects.Copy);
                }
            }
        }

        // PERBAIKAN: Spesifikasikan System.Windows.Input.MouseButtonEventArgs
        private void File_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsMouseOver)
            {
                var fileItem = element.DataContext as FileItem;
                if (fileItem != null && !string.IsNullOrEmpty(fileItem.FilePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(fileItem.FilePath) { UseShellExecute = true });
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Cannot open file: " + ex.Message);
                    }
                }
            }
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = SettingsManager.Current.SourceFolderPath;
            try
            {
                Process.Start("explorer.exe", sourcePath);
                this.Hide();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open File Explorer: " + ex.Message);
            }
        }
    }
}