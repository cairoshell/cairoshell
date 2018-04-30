using CairoDesktop.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Interop;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.Common
{
    public class HotKeyManager
    {
        public static void RegisterHotKey(List<string> keys, Action<HotKey> action)
        {
            KeyModifier mod1 = KeyModifier.None;
            KeyModifier mod2 = KeyModifier.None;
            Key key = Key.None;

            Enum.TryParse(keys[0], out mod1);

            if (keys.Count > 2)
            {
                Enum.TryParse(keys[1], out mod2);
                Enum.TryParse(keys[2], out key);
            }
            else if (keys.Count == 2)
                Enum.TryParse(keys[1], out key);

            HotKey hotkey;
            if (mod1 != KeyModifier.None && mod2 != KeyModifier.None && key != Key.None)
                hotkey = new HotKey(key, mod2 | mod1, action);
            else if (mod1 != KeyModifier.None && key != Key.None)
                hotkey = new HotKey(key, mod1, action);
        }
    }

    // thanks to https://stackoverflow.com/a/9330358
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> hotkeys;
        private bool _disposed = false;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }

        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;

            if (register)
                Register();
        }
        
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (hotkeys == null)
            {
                hotkeys = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            hotkeys.Add(Id, this);
            
            return result;
        }
        
        public void Unregister()
        {
            HotKey hotKey;
            if (hotkeys.TryGetValue(Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }
        
        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WM_HOTKEY)
                {
                    HotKey hotKey;

                    if (hotkeys.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            CairoLogger.Instance.Debug(string.Format("Hotkey {0} pressed", hotKey.Key.ToString()));
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }

        // ******************************************************************
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // ******************************************************************
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Unregister();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
    
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
