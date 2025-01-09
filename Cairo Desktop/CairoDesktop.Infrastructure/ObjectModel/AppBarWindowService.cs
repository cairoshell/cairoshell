using System;
using System.Collections.Generic;
using System.ComponentModel;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.Services;
using ManagedShell;
using ManagedShell.AppBar;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public abstract class AppBarWindowService : IWindowService
    {
        public bool EnableMultiMon { get; protected set; }
        public bool EnableService { get; protected set; }

        public List<CairoAppBarWindow> Windows { get; } = new List<CairoAppBarWindow>();

        protected readonly ICairoApplication _cairoApplication;
        protected readonly ShellManager _shellManager;
        protected readonly IWindowManager _windowManager;
        protected readonly Settings _settings;

        protected AppBarWindowService(ICairoApplication cairoApplication, ShellManagerService shellManagerService, IWindowManager windowManager, Settings settings)
        {
            _cairoApplication = cairoApplication;
            _shellManager = shellManagerService.ShellManager;
            _windowManager = windowManager;
            _settings = settings;

            _settings.PropertyChanged += Settings_PropertyChanged;
            _windowManager.TaskbarCreated += WindowManager_TaskbarCreated;
        }

        public void Register()
        {
            _windowManager?.RegisterWindowService(this);
        }

        public virtual void Dispose()
        {
            _settings.PropertyChanged -= Settings_PropertyChanged;
            if (_windowManager != null)
            {
                _windowManager.TaskbarCreated -= WindowManager_TaskbarCreated;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) return;

            HandleSettingChange(e.PropertyName);
        }

        private void WindowManager_TaskbarCreated(object sender, EventArgs e)
        {
            if (!EnableService) return;
            HandleEnableServiceChanged(false);
            HandleEnableServiceChanged(true);
        }

        public void HandleScreenAdded(AppBarScreen screen)
        {
            if (EnableService && (EnableMultiMon || (screen.Primary && Windows.Count < 1)))
            {
                OpenWindow(screen);
            }
        }

        public void HandleScreenRemoved(string screenDeviceName)
        {
            if (EnableService)
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

            if (EnableMultiMon && !args.IsFastSetup)
            {
                foreach (AppBarScreen screen in _windowManager.ScreenState)
                {
                    CairoAppBarWindow window = _windowManager.GetScreenWindow(Windows, screen);

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
