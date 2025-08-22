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
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var windowHandle = new WindowInteropHelper(this).Handle;
            if (windowHandle == IntPtr.Zero) { return; }

            var hwndSource = HwndSource.FromHwnd(windowHandle);
            hwndSource?.AddHook(WndProc);

            InjectToTaskbar(windowHandle);
            PositionInTaskbar(windowHandle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTCLIENT = 1;
            const int WM_LBUTTONUP = 0x0202;
            const int WM_WINDOWPOSCHANGED = 0x0047;

            switch (msg)
            {
                case WM_NCHITTEST:
                    handled = true;
                    return (IntPtr)HTCLIENT;

                case WM_LBUTTONUP:
                    WidgetClicked?.Invoke();
                    handled = true;
                    break;

                case WM_WINDOWPOSCHANGED:
                    var taskbarHandleCheck = Interop.FindWindow(Interop.TASKBAR_CLASS, null);
                    if (lParam != IntPtr.Zero)
                    {
                        var pos = Marshal.PtrToStructure<WINDOWPOS>(lParam);
                        if (pos.hwnd == taskbarHandleCheck)
                        {
                            PositionInTaskbar(hwnd);
                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void InjectToTaskbar(IntPtr windowHandle)
        {
            try
            {
                int currentStyle = (int)GetWindowLong(windowHandle, -16);
                int newStyle = currentStyle | 0x40000000;
                SetWindowLong(windowHandle, -16, (IntPtr)newStyle);
                int currentExStyle = (int)GetWindowLong(windowHandle, -20);
                int newExStyle = currentExStyle | 0x80;
                SetWindowLong(windowHandle, -20, (IntPtr)newExStyle);
                IntPtr taskbarHandle = Interop.FindWindow(Interop.TASKBAR_CLASS, null);
                if (taskbarHandle == IntPtr.Zero) { return; }
                Interop.SetParent(windowHandle, taskbarHandle);
            }
            catch { }
        }

        private void PositionInTaskbar(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;
            var taskbarHandle = Interop.FindWindow(Interop.TASKBAR_CLASS, null);
            if (taskbarHandle == IntPtr.Zero) { return; }
            IntPtr trayHandle = Interop.FindWindowEx(taskbarHandle, IntPtr.Zero, Interop.TRAY_CLASS, null);
            if (trayHandle == IntPtr.Zero) { return; }
            Interop.GetWindowRect(trayHandle, out Interop.RECT trayRect);
            Interop.GetWindowRect(taskbarHandle, out Interop.RECT taskbarRect);
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null) return;
            double dpiX = source.CompositionTarget.TransformToDevice.M11;
            int widgetWidth = (int)(this.Width * dpiX);
            int widgetHeight = (int)(this.Height * dpiX);
            int newX, newY;
            if (taskbarRect.right - taskbarRect.left > taskbarRect.bottom - taskbarRect.top)
            {
                newX = trayRect.left - widgetWidth - 4;
                newY = ((taskbarRect.bottom - taskbarRect.top - widgetHeight) / 2);
            }
            else
            {
                newX = (taskbarRect.right - taskbarRect.left - widgetWidth) / 2;
                newY = trayRect.top - widgetHeight - 4;
            }
            Interop.SetWindowPos(windowHandle, Interop.HWND_TOP, newX, newY, widgetWidth, widgetHeight, Interop.SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS { public IntPtr hwnd; public IntPtr hwndInsertAfter; public int x; public int y; public int cx; public int cy; public uint flags; }

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