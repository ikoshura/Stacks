using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Stacks
{
    public partial class FanView : Window
    {
        public ObservableCollection<FileItem> Files { get; set; }

        /// <summary>
        /// Offset vertikal popup relatif terhadap widget.
        /// Nilai positif = naik, negatif = turun.
        /// </summary>
        public int PopupOffsetY { get; set; } = 320;

        public FanView()
        {
            InitializeComponent();

            // Sembunyikan saat pertama kali dibuat
            this.Visibility = Visibility.Hidden;

            Files = new ObservableCollection<FileItem>();
            FileItemsControl.ItemsSource = Files;
            LoadFiles();
        }

        // --- METODE: Perintah untuk muncul di posisi tertentu ---
        public void ShowAt(Point widgetPosition)
        {
            // Untuk menghindari flicker, kita posisikan DULU baru tampilkan
            this.Visibility = Visibility.Hidden;
            this.UpdateLayout(); // Paksa ukur
            PositionWindow(widgetPosition);

            this.Show();
            this.Activate();
        }

        // Atur posisi popup window
        private void PositionWindow(Point widgetPosition)
        {
            var workArea = SystemParameters.WorkArea;

            this.Left = widgetPosition.X - (this.ActualWidth / 2) + (this.Width / 2);
            this.Top = widgetPosition.Y - this.ActualHeight - PopupOffsetY;

            // Kalau mentok atas layar, taruh di bawah widget biar tetap kelihatan
            if (this.Top < workArea.Top)
            {
                this.Top = widgetPosition.Y + 40;
            }

            // Batas kiri/kanan layar
            if (this.Left < workArea.Left) this.Left = workArea.Left;
            if (this.Left + this.ActualWidth > workArea.Right)
                this.Left = workArea.Right - this.ActualWidth;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide(); // Gunakan Hide() agar bisa ditampilkan lagi
        }

        private async void LoadFiles()
        {
            try
            {
                string downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );
                if (!Directory.Exists(downloadsPath)) { return; }

                var filePaths = await Task.Run(() =>
                    Directory.GetFiles(downloadsPath)
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
                MessageBox.Show($"Failed to load files: {ex.Message}");
            }
        }

        private BitmapImage? CreateThumbnail(string filePath)
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
            catch { return null; }
        }

        private void File_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element)
            {
                var fileItem = element.DataContext as FileItem;
                if (fileItem != null && fileItem.FilePath != null)
                {
                    var data = new DataObject(DataFormats.FileDrop, new string[] { fileItem.FilePath });
                    DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                }
            }
        }

        private void File_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                        MessageBox.Show("Cannot open file: " + ex.Message);
                    }
                }
            }
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            try
            {
                Process.Start("explorer.exe", downloadsPath);
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open File Explorer: " + ex.Message);
            }
        }
    }
}
