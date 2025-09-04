// FILE: Stacks/FanView.xaml.cs

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
using Point = System.Windows.Point;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Stacks
{
    public partial class FanView : FluentWindow
    {
        public ObservableCollection<FileItem> Files { get; set; }

        public FanView()
        {
            InitializeComponent();
            Files = new ObservableCollection<FileItem>();
            FileItemsControl.ItemsSource = Files;
            // Langsung sembunyikan jendela saat tidak aktif (kehilangan fokus)
            this.Deactivated += (sender, e) => this.Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // Method publik untuk mengontrol visibilitas secara instan
        public void ToggleAt(Point cursorPosition)
        {
            if (this.IsVisible)
            {
                this.Hide();
            }
            else
            {
                ShowAt(cursorPosition);
            }
        }

        private void ShowAt(Point cursorPosition)
        {
            // Muat file terbaru
            LoadFiles();
            this.DataContext = SettingsManager.Instance;

            // Pastikan efek visual (akrilik) diterapkan sesuai pengaturan
            if (SettingsManager.Current.UseAcrylic)
            {
                WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
            }
            else
            {
                WindowBackdrop.RemoveBackdrop(this);
            }

            // Hitung posisi final dan langsung tempatkan jendela di sana
            Point finalPos = CalculateWindowPosition(cursorPosition);
            this.Left = finalPos.X;
            this.Top = finalPos.Y;

            // Tampilkan jendela dan berikan fokus
            this.Show();
            this.Activate();
        }

        private Point CalculateWindowPosition(Point cursorPosition)
        {
            var workArea = SystemParameters.WorkArea;
            const double screenMargin = 12.0;
            // Paksa pembaruan layout untuk mendapatkan ActualWidth dan ActualHeight yang benar
            this.UpdateLayout();
            double left = cursorPosition.X - 10;
            double top = cursorPosition.Y - this.ActualHeight - 10;
            if (left + this.ActualWidth > workArea.Right) left = workArea.Right - this.ActualWidth - screenMargin;
            if (left < workArea.Left) left = workArea.Left + screenMargin;
            if (top < workArea.Top) top = cursorPosition.Y + 10;
            if (top + this.ActualHeight > workArea.Bottom) top = workArea.Bottom - this.ActualHeight - screenMargin;
            return new Point(left, top);
        }

        #region Kode Lengkap Lainnya (Tidak Berubah)
        private async void LoadFiles()
        {
            try
            {
                string sourcePath = SettingsManager.Current.SourceFolderPath;
                if (!Directory.Exists(sourcePath)) { Files.Clear(); return; }
                var filePaths = await Task.Run(() => Directory.GetFiles(sourcePath).OrderByDescending(f => new FileInfo(f).LastWriteTime).Take(10).ToList());
                Files.Clear();
                foreach (var path in filePaths)
                {
                    Files.Add(new FileItem { FilePath = path, FileName = Path.GetFileName(path), Thumbnail = CreateThumbnail(path) });
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
                    using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                    {
                        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch { return null; }
            }
        }

        private void File_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element)
            {
                if (element.DataContext is FileItem fileItem && fileItem.FilePath != null)
                {
                    var data = new System.Windows.DataObject(System.Windows.DataFormats.FileDrop, new string[] { fileItem.FilePath });
                    System.Windows.DragDrop.DoDragDrop(element, data, System.Windows.DragDropEffects.Copy);
                }
            }
        }

        private void File_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { IsMouseOver: true, DataContext: FileItem fileItem } && !string.IsNullOrEmpty(fileItem.FilePath))
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

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", SettingsManager.Current.SourceFolderPath);
                this.Hide();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open File Explorer: " + ex.Message);
            }
        }
        #endregion
    }
}