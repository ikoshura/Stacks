using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Stacks
{
    public partial class MainWindow : Window
    {
        // PERBAIKAN: DependencyProperty untuk melacak apakah FanView terbuka.
        // Ini akan menjadi "single source of truth".
        public static readonly DependencyProperty IsFanViewOpenProperty = DependencyProperty.Register(
            "IsFanViewOpen", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool IsFanViewOpen
        {
            get { return (bool)GetValue(IsFanViewOpenProperty); }
            set { SetValue(IsFanViewOpenProperty, value); }
        }

        public event Action? WidgetClicked;

        public MainWindow()
        {
            InitializeComponent();
            WidgetButton.MouseRightButtonUp += WidgetButton_MouseRightButtonUp;
            this.Topmost = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var windowHandle = new WindowInteropHelper(this).Handle;
            if (windowHandle == IntPtr.Zero) { return; }

            int currentExStyle = (int)GetWindowLong(windowHandle, -20);
            int newExStyle = currentExStyle | 0x80;
            SetWindowLong(windowHandle, -20, (IntPtr)newExStyle);

            PositionInTaskbar(windowHandle);
        }

        // ... sisa kode tidak berubah ...

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
            if (taskbarRect.right - taskbarRect.left > taskbarRect.bottom - taskbarRect.top)
            {
                newX = trayRect.left - widgetWidth - 4;
                newY = taskbarRect.top + ((taskbarRect.bottom - taskbarRect.top - widgetHeight) / 2);
            }
            else
            {
                newX = taskbarRect.left + ((taskbarRect.right - taskbarRect.left - widgetWidth) / 2);
                newY = trayRect.top - widgetHeight - 4;
            }
            Interop.SetWindowPos(windowHandle, Interop.HWND_TOP, newX, newY, widgetWidth, widgetHeight, Interop.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

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