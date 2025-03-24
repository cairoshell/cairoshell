using CairoDesktop.Application.Interfaces;
using ManagedShell;
using ManagedShell.AppBar;
using ManagedShell.Interop;
using System;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public class CairoAppBarWindow : AppBarWindow
    {
        protected readonly ICairoApplication _cairoApplication;
        protected readonly IWindowManager _windowManager;
        
        public CairoAppBarWindow() : base(null, null, null, AppBarScreen.FromPrimaryScreen(), AppBarEdge.Bottom, AppBarMode.Normal, 0)
        { }

        public CairoAppBarWindow(ICairoApplication cairoApplication, ShellManager shellManager, IWindowManager windowManager, AppBarScreen screen, AppBarEdge edge, int modeSetting, double height) : base(
            shellManager.AppBarManager, shellManager.ExplorerHelper, shellManager.FullScreenHelper, screen, edge, SettingToAppBarMode(modeSetting), height)
        {
            _cairoApplication = cairoApplication;
            _windowManager = windowManager;
        }

        public CairoAppBarWindow(ICairoApplication cairoApplication, ShellManager shellManager, IWindowManager windowManager, AppBarScreen screen, AppBarEdge edge, AppBarMode mode, double height) : base(
            shellManager.AppBarManager, shellManager.ExplorerHelper, shellManager.FullScreenHelper, screen, edge, mode, height)
        {
            _cairoApplication = cairoApplication;
            _windowManager = windowManager;
            
            AllowsTransparency = true;
        }

        protected override void SetScreenProperties(ScreenSetupReason reason)
        {
            WindowManagerEventReason cairoReason;

            // Manually map enum values so that we don't need to maintain order.
            switch (reason)
            {
                case ScreenSetupReason.DeviceChange:
                    cairoReason = WindowManagerEventReason.DeviceChange;
                    break;
                case ScreenSetupReason.DpiChange:
                    cairoReason = WindowManagerEventReason.DpiChange;
                    break;
                default:
                    cairoReason = WindowManagerEventReason.DisplayChange;
                    break;
            }

            _windowManager.NotifyDisplayChange(cairoReason); // update Cairo window list based on new screen setup
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (Screen.Primary && ProcessScreenChanges && !AllowClose)
            {
                if (msg == (int)NativeMethods.WM.SETTINGCHANGE && wParam == (IntPtr)NativeMethods.SPI.SETWORKAREA)
                {
                    _windowManager.NotifyDisplayChange(WindowManagerEventReason.WorkArea);
                }
                else if (msg == (int)NativeMethods.WM.DWMCOMPOSITIONCHANGED)
                {
                    _windowManager.NotifyDisplayChange(WindowManagerEventReason.DwmChange);
                }
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected static AppBarMode SettingToAppBarMode(int setting)
        {
            switch (setting)
            {
                case 0:
                    return AppBarMode.Normal;
                case 1:
                    return AppBarMode.None;
                case 2:
                    return AppBarMode.AutoHide;
                default:
                    return AppBarMode.Normal;
            }
        }
    }
}
