// FILE: Stacks/InputSimulator.cs

using System;
using System.Runtime.InteropServices;

namespace Stacks
{
    public static class InputSimulator
    {
        // Konstanta Virtual Key
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_MENU = 0x12; // ALT key
        private const ushort VK_S = 0x53;

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void SimulateCtrlAltS()
        {
            INPUT[] inputs = new INPUT[]
            {
                // Tekan Ctrl
                CreateKeyInput(VK_CONTROL, 0),
                // Tekan Alt
                CreateKeyInput(VK_MENU, 0),
                // Tekan S
                CreateKeyInput(VK_S, 0),
                // Lepas S
                CreateKeyInput(VK_S, KEYEVENTF_KEYUP),
                // Lepas Alt
                CreateKeyInput(VK_MENU, KEYEVENTF_KEYUP),
                // Lepas Ctrl
                CreateKeyInput(VK_CONTROL, KEYEVENTF_KEYUP)
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT CreateKeyInput(ushort vk, uint flags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUT_UNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public INPUT_UNION U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT_UNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}