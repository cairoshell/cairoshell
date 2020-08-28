using System;
using System.Windows.Input;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.Common
{
    // thanks to https://stackoverflow.com/a/9330358
    public class HotKey : DisposableObject
    {
        public Key Key { get; private set; }

        public HotKeyModifier KeyModifiers { get; private set; }

        public Action<HotKey> Action { get; set; }

        public int Id { get; set; }

        private bool _registered;

        public HotKey(Key k, HotKeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;

            if (register)
            {
                Register();
            }
        }

        public bool Register()
        {
            // TODO: Should we check the HotKey _registered value here?

            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);

            _registered = RegisterHotKey(IntPtr.Zero, Id, (uint)KeyModifiers, (uint)virtualKeyCode);
            if (_registered)
            {
                HotKeyManager.HotKeys.Add(Id, this);
            }

            return _registered;
        }

        public void Unregister()
        {
            if (_registered)
            {
                if (HotKeyManager.HotKeys.TryGetValue(Id, out HotKey hotKey))
                {
                    UnregisterHotKey(IntPtr.Zero, Id);
                    HotKeyManager.HotKeys.Remove(hotKey.Id);
                    _registered = false;
                }
            }
        }

        protected override void DisposeOfUnManagedResources()
        {
            Unregister();
        }
    }
}