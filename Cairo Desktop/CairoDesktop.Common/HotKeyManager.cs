using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using CairoDesktop.Common.Logging;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.Common
{
    public static class HotKeyManager
    {
        internal readonly static Dictionary<int, HotKey> HotKeys = new Dictionary<int, HotKey>();

        static HotKeyManager()
        {
            ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == (int)WM.HOTKEY)
                {
                    if (HotKeys.TryGetValue((int)msg.wParam, out HotKey hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            CairoLogger.Instance.Debug(string.Format("HotKey {0} pressed", hotKey.Key.ToString()));
                            hotKey.Action.Invoke(hotKey);
                        }

                        handled = true;
                    }
                }
            }
        }

        public static HotKey RegisterHotKey(IList<string> keys, Action<HotKey> action)
        {
            Key key = Key.None;

            Enum.TryParse(keys[0], out HotKeyModifier mod1);
            HotKeyModifier mod2 = HotKeyModifier.None;

            if (keys.Count > 2)
            {
                Enum.TryParse(keys[1], out mod2);
                Enum.TryParse(keys[2], out key);
            }
            else if (keys.Count == 2)
            {
                Enum.TryParse(keys[1], out key);
            }

            HotKey hotkey = null;
            if (mod1 != HotKeyModifier.None && mod2 != HotKeyModifier.None && key != Key.None)
            {
                hotkey = new HotKey(key, mod2 | mod1, action);
            }
            else if (mod1 != HotKeyModifier.None && key != Key.None)
            {
                hotkey = new HotKey(key, mod1, action);
            }

            return hotkey;
        }

        public static void UnregisterHotKey(HotKey hotKey)
        {
            hotKey.Unregister();
        }
    }
}