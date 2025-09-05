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
        private bool _isFirstLoad = true;
        public event Action? ViewDeactivated;

        public FanView()
        {
            InitializeComponent();
            Files = new ObservableCollection<FileItem>();
            FileItemsControl.ItemsSource = Files;
            this.Deactivated += OnDeactivated;
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            this.Hide();
            ViewDeactivated?.Invoke();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ViewDeactivated?.Invoke();
        }

        public async void ToggleAt(Point cursorPosition)
        {
            if (this.IsVisible)
            {
                this.Hide();
                ViewDeactivated?.Invoke();
            }
            else
            {
                await ShowAt(cursorPosition);
            }
        }

        public async Task ShowAt(Point cursorPosition)
        {
            // 1. Muat file terlebih dahulu agar konten siap diukur
            await LoadFiles();

            this.DataContext = SettingsManager.Instance;

            WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);

            // PERBAIKAN: Logika untuk mengunci ukuran setelah pengukuran pertama
            if (_isFirstLoad)
            {
                // Tampilkan di luar layar untuk diukur sesuai konten yang sudah dimuat
                this.Opacity = 0;
                this.Left = -9999;
                this.Show();
                this.UpdateLayout();

                // Kunci ukuran jendela berdasarkan hasil pengukuran
                this.Width = this.ActualWidth;
                this.Height = this.ActualHeight;

                // SANGAT PENTING: Matikan SizeToContent agar jendela tidak lagi meresize
                this.SizeToContent = SizeToContent.Manual;

                // Sembunyikan dan siapkan untuk ditampilkan di posisi yang benar
                this.Hide();
                this.Opacity = 1;
                _isFirstLoad = false;
            }

            Point finalPos = CalculateWindowPosition(cursorPosition);
            this.Left = finalPos.X;
            this.Top = finalPos.Y;

            this.Show();
            this.Activate();
        }

        private Point CalculateWindowPosition(Point cursorPosition)
        {
            var workArea = SystemParameters.WorkArea;
            const double screenMargin = 12.0;
            // Ukuran sudah tetap, jadi tidak perlu UpdateLayout() lagi di sini.
            double left = cursorPosition.X - 10;
            double top = cursorPosition.Y - this.Height - 10; // Gunakan Height yang sudah terkunci

            if (left + this.Width > workArea.Right) left = workArea.Right - this.Width - screenMargin;
            if (left < workArea.Left) left = workArea.Left + screenMargin;
            if (top < workArea.Top) top = cursorPosition.Y + 10;
            if (top + this.Height > workArea.Bottom) top = workArea.Bottom - this.Height - screenMargin;

            return new Point(left, top);
        }

        // ... Sisa kode tidak berubah ...
        private async Task LoadFiles()
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
                    ViewDeactivated?.Invoke();
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
                ViewDeactivated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open File Explorer: " + ex.Message);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is SettingsWindow)
                {
                    window.Activate();
                    return;
                }
            }

            GetCursorPos(out Win32Point w32Mouse);
            var cursorPosition = new Point(w32Mouse.X, w32Mouse.Y);
            new SettingsWindow().ShowAt(cursorPosition);
            this.Hide();
            ViewDeactivated?.Invoke();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point pt);
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct Win32Point { public Int32 X; public Int32 Y; };
    }
}