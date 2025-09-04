// FILE: Stacks/GlobalHotkeyManager.cs

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Stacks
{
    public class GlobalHotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private HwndSource? _source;
        private readonly Window _window;
        private int _hotkeyId = 0;

        public event Action? HotkeyPressed;

        public GlobalHotkeyManager(Window window)
        {
            _window = window;
            var helper = new WindowInteropHelper(window);
            helper.EnsureHandle(); // Pastikan handle jendela sudah dibuat
            _source = HwndSource.FromHwnd(helper.Handle);
            _source?.AddHook(WndProc);
        }

        public void Register(ModifierKeys modifier, Key key)
        {
            var helper = new WindowInteropHelper(_window);
            _hotkeyId = this.GetHashCode(); // ID unik untuk hotkey
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            uint fsModifiers = (uint)modifier;

            if (!RegisterHotKey(helper.Handle, _hotkeyId, fsModifiers, vk))
            {
                // Gagal mendaftarkan hotkey
            }
        }

        public void Unregister()
        {
            var helper = new WindowInteropHelper(_window);
            UnregisterHotKey(helper.Handle, _hotkeyId);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
            {
                HotkeyPressed?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _source?.RemoveHook(WndProc);
            _source = null;
            Unregister();
            GC.SuppressFinalize(this);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}