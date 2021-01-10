using System.Collections.Generic;
using System.Windows.Forms;
using ManagedShell;

namespace CairoDesktop.SupportingClasses
{
    public abstract class AppBarWindowService : IWindowService
    {
        public bool EnableMultiMon { get; protected set; }
        public bool EnableService { get; protected set; }

        public List<CairoAppBarWindow> Windows { get; } = new List<CairoAppBarWindow>();

        protected ShellManager _shellManager;
        protected WindowManager _windowManager;

        protected AppBarWindowService(ShellManagerService shellManagerService, WindowManager windowManager)
        {
            _shellManager = shellManagerService.ShellManager;
            _windowManager = windowManager;
        }

        public void Register()
        {
            _windowManager?.RegisterWindowService(this);
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
                CairoAppBarWindow windowToClose = null;
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
                    CairoAppBarWindow window = WindowManager.GetScreenWindow(Windows, screen);

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
