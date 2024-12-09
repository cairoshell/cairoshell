using ManagedShell.AppBar;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public interface IWindowManager
    {
        event EventHandler<WindowManagerEventArgs> DwmChanged;

        event EventHandler<WindowManagerEventArgs> ScreensChanged;

        event EventHandler<EventArgs> TaskbarCreated;

        bool IsSettingDisplays { get; set; }

        List<AppBarScreen> ScreenState { get; set; }

        void InitialSetup();

        void NotifyDisplayChange(ScreenSetupReason reason);

        void RegisterWindowService(IWindowService service);

        T GetScreenWindow<T>(List<T> windowList, AppBarScreen screen) where T : AppBarWindow;
    }
}