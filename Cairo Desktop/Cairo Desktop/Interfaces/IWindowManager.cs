using CairoDesktop.Services;
using CairoDesktop.SupportingClasses;
using ManagedShell.AppBar;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Interfaces
{
    public interface IWindowManager
    {
        event EventHandler<WindowManagerEventArgs> DwmChanged;

        event EventHandler<WindowManagerEventArgs> ScreensChanged;

        bool IsSettingDisplays { get; set; }

        List<AppBarScreen> ScreenState { get; set; }

        void InitialSetup();

        void NotifyDisplayChange(ScreenSetupReason reason);

        void RegisterWindowService(IWindowService service);
    }
}