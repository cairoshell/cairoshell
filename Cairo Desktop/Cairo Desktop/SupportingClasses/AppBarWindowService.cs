using System.Collections.Generic;
using System.Windows.Forms;

namespace CairoDesktop.SupportingClasses
{
    public abstract class AppBarWindowService : IWindowService
    {
        public bool EnableMultiMon { get; protected set; }
        public bool EnableService { get; protected set; }

        public List<AppBarWindow> Windows { get; } = new List<AppBarWindow>();

        protected WindowManager _windowManager;

        protected AppBarWindowService()
        {
        }

        public void Initialize(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public virtual void Dispose()
        {
        }

        public void HandleScreenAdded(Screen screen)
        {
            if (EnableService && (EnableMultiMon || screen.Primary))
            {
                OpenWindow(screen);
            }
        }

        public void HandleScreenRemoved(string screenDeviceName)
        {
            if (EnableService && EnableMultiMon)
            {
                // close menu bars
                AppBarWindow windowToClose = null;
                foreach (var window in Windows)
                {
                    if (window.Screen != null && window.Screen.DeviceName == screenDeviceName)
                    {
                        windowToClose = window;
                        break;
                    }
                }

                if (windowToClose != null)
                {
                    if (!windowToClose.IsClosing)
                    {
                        windowToClose.Close();
                    }

                    Windows.Remove(windowToClose);
                }
            }
        }

        public void RefreshWindows(WindowManagerEventArgs args)
        {
            // update screens of stale windows
            if (!EnableService)
            {
                return;
            }

            if (EnableMultiMon)
            {
                foreach (Screen screen in _windowManager.ScreenState)
                {
                    AppBarWindow window = WindowManager.GetScreenWindow(Windows, screen);

                    if (window != null)
                    {
                        window.Screen = screen;
                        window.SetScreenPosition();
                    }
                }
            }
            else if (Windows.Count > 0)
            {
                Windows[0].Screen = Screen.PrimaryScreen;
                Windows[0].SetScreenPosition();
            }
        }

        protected abstract void OpenWindow(Screen screen);
    }
}
