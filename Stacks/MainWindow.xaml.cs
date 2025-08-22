using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Stacks
{
    public partial class MainWindow : Window
    {
        public event Action? WidgetClicked;

        public MainWindow()
        {
            InitializeComponent();
            WidgetButton.MouseRightButtonUp += WidgetButton_MouseRightButtonUp;

            // Kita ingin jendela ini selalu di atas, termasuk di atas taskbar
            this.Topmost = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var windowHandle = new WindowInteropHelper(this).Handle;
            if (windowHandle == IntPtr.Zero) { return; }

            // Hapus window style yang membuatnya bisa di-klik kanan (menu sistem)
            int currentExStyle = (int)GetWindowLong(windowHandle, -20);
            int newExStyle = currentExStyle | 0x80; // WS_EX_TOOLWINDOW
            SetWindowLong(windowHandle, -20, (IntPtr)newExStyle);

            PositionInTaskbar(windowHandle);
        }

        // WndProc disederhanakan, kita tidak perlu lagi memantau perubahan posisi taskbar
        // karena jendela kita bukan lagi anak dari taskbar.
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_LBUTTONUP = 0x0202;

            if (msg == WM_LBUTTONUP)
            {
                WidgetClicked?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        // *** PERUBAHAN UTAMA ADA DI SINI ***
        // Kita tidak lagi "menyuntikkan" widget ke taskbar.
        // Kita hanya memposisikannya di lokasi yang benar.
        private void PositionInTaskbar(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            var taskbarHandle = Interop.FindWindow(Interop.TASKBAR_CLASS, null);
            if (taskbarHandle == IntPtr.Zero) return;

            IntPtr trayHandle = Interop.FindWindowEx(taskbarHandle, IntPtr.Zero, Interop.TRAY_CLASS, null);
            if (trayHandle == IntPtr.Zero) return;

            Interop.GetWindowRect(trayHandle, out Interop.RECT trayRect);
            Interop.GetWindowRect(taskbarHandle, out Interop.RECT taskbarRect);

            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null) return;

            double dpiX = source.CompositionTarget.TransformToDevice.M11;
            double dpiY = source.CompositionTarget.TransformToDevice.M22;

            int widgetWidth = (int)(this.Width * dpiX);
            int widgetHeight = (int)(this.Height * dpiY);

            int newX, newY;

            // Logika untuk menentukan apakah taskbar horizontal atau vertikal
            if (taskbarRect.right - taskbarRect.left > taskbarRect.bottom - taskbarRect.top)
            {
                // Taskbar horizontal (di atas atau bawah)
                newX = trayRect.left - widgetWidth - 4; // 4 pixel spasi
                newY = taskbarRect.top + ((taskbarRect.bottom - taskbarRect.top - widgetHeight) / 2);
            }
            else
            {
                // Taskbar vertikal (di kiri atau kanan)
                newX = taskbarRect.left + ((taskbarRect.right - taskbarRect.left - widgetWidth) / 2);
                newY = trayRect.top - widgetHeight - 4; // 4 pixel spasi
            }

            // Gunakan SetWindowPos untuk memindahkan jendela kita
            Interop.SetWindowPos(windowHandle, Interop.HWND_TOP, newX, newY, widgetWidth, widgetHeight, Interop.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Struct ini tidak lagi dibutuhkan di sini karena tidak memantau WM_WINDOWPOSCHANGED
        // [StructLayout(LayoutKind.Sequential)]
        // private struct WINDOWPOS { ... }

        private void WidgetButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WidgetButton.ContextMenu != null)
            {
                WidgetButton.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow { Owner = this };
            settingsWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}