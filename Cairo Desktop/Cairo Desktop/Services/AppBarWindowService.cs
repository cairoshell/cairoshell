using System.Collections.Generic;
using System.ComponentModel;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.Services;
using CairoDesktop.SupportingClasses;
using ManagedShell;
using ManagedShell.AppBar;

namespace CairoDesktop.Services
{
    public abstract class AppBarWindowService : IWindowService
    {
        public bool EnableMultiMon { get; protected set; }
        public bool EnableService { get; protected set; }

        public List<CairoAppBarWindow> Windows { get; } = new List<CairoAppBarWindow>();

        protected readonly ICairoApplication _cairoApplication;
        protected readonly ShellManager _shellManager;
        protected readonly WindowManager _windowManager;

        protected AppBarWindowService(ICairoApplication cairoApplication, ShellManagerService shellManagerService, WindowManager windowManager)
        {
            _cairoApplication = cairoApplication;
            _shellManager = shellManagerService.ShellManager;
            _windowManager = windowManager;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        public void Register()
        {
            _windowManager?.RegisterWindowService(this);
        }

        public virtual void Dispose()
        {
            Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) return;

            HandleSettingChange(e.PropertyName);
        }

        public void HandleScreenAdded(AppBarScreen screen)
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
                CloseScreenWindow(screenDeviceName);
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
                foreach (AppBarScreen screen in _windowManager.ScreenState)
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
                Windows[0].Screen = AppBarScreen.FromPrimaryScreen();
                Windows[0].SetScreenPosition();
            }
        }

        protected void HandleEnableMultiMonChanged(bool newValue)
        {
            EnableMultiMon = newValue;
            
            if (!EnableService)
            {
                return;
            }

            if (EnableMultiMon)
            {
                foreach (var screen in _windowManager.ScreenState)
                {
                    bool exists = false;

                    foreach (var window in Windows)
                    {
                        if (window.Screen != null && window.Screen.DeviceName == screen.DeviceName)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                    {
                        continue;
                    }

                    HandleScreenAdded(screen);
                }
            }
            else
            {
                foreach (var screen in _windowManager.ScreenState)
                {
                    if (screen.Primary)
                    {
                        continue;
                    }

                    CloseScreenWindow(screen.DeviceName);
                }
            }
        }

        protected void HandleEnableServiceChanged(bool newValue)
        {
            EnableService = newValue;
            
            if (EnableService)
            {
                foreach (var screen in _windowManager.ScreenState)
                {
                    HandleScreenAdded(screen);
                }
            }
            else
            {
                foreach (var window in Windows)
                {
                    CloseWindow(window);
                }

                Windows.Clear();
            }
        }

        protected void CloseScreenWindow(string screenDeviceName)
        {
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
                CloseWindow(windowToClose);

                Windows.Remove(windowToClose);
            }
        }

        protected void CloseWindow(CairoAppBarWindow window)
        {
            if (!window.IsClosing)
            {
                window.AllowClose = true;
                window.Close();
            }
        }

        protected abstract void OpenWindow(AppBarScreen screen);

        protected abstract void HandleSettingChange(string setting);
    }
}
