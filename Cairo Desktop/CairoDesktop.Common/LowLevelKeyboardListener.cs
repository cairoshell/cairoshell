using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Need to make this honor modifier keys, ie Win+Tab
    /// </summary>
    /// <remarks>
    /// Created from:
    /// http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/
    /// https://www.codeproject.com/Articles/19004/A-Simple-C-Global-Low-Level-Keyboard-Hook
    /// </remarks>
    public class LowLevelKeyboardListener
    {
        private const int WH_KEYBOARD_LL = 13;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyEventArgs> OnKeyPressed;
        public event EventHandler<KeyEventArgs> OnKeyDown;
        public event EventHandler<KeyEventArgs> OnKeyUp;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private List<int> keysPressed = new List<int>();

        public LowLevelKeyboardListener()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // keep track of pressed keys so we don't intercept hotkeys
                if (wParam == (IntPtr)NativeMethods.WM.KEYDOWN || wParam == (IntPtr)NativeMethods.WM.SYSKEYDOWN)
                {
                    if (!keysPressed.Contains(vkCode))
                        keysPressed.Add(vkCode);

                    var kpa = new KeyEventArgs(KeyInterop.KeyFromVirtualKey(vkCode));
                    OnKeyDown?.Invoke(this, kpa);
                    if (kpa.Handled)
                        return new IntPtr(1);
                }

                // act only when key is raised
                if (wParam == (IntPtr)NativeMethods.WM.KEYUP || wParam == (IntPtr)NativeMethods.WM.SYSKEYUP)
                {
                    // if more than one key was pressed before a key was raised, user attempted hotkey
                    if (keysPressed.Count == 1 && OnKeyPressed != null)
                    {
                        var kpaPressed = new KeyEventArgs(KeyInterop.KeyFromVirtualKey(vkCode));
                        OnKeyPressed?.Invoke(this, kpaPressed);
                        if (kpaPressed.Handled)
                            return new IntPtr(1);
                    }

                    // reset pressed keys
                    keysPressed.Clear();
                    
                    var kpaUp = new KeyEventArgs(KeyInterop.KeyFromVirtualKey(vkCode));
                    OnKeyUp?.Invoke(this, kpaUp);
                    if (kpaUp.Handled)
                        return new IntPtr(1);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

    public class KeyEventArgs : EventArgs
    {
        public Key Key { get; private set; }
        public bool Handled { get; set; }

        public KeyEventArgs(Key key)
        {
            Key = key;
            Handled = false;
        }
    }
}
