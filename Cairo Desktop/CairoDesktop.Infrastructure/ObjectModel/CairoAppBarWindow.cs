using CairoDesktop.Application.Interfaces;
using ManagedShell;
using ManagedShell.AppBar;

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
            _windowManager.NotifyDisplayChange(reason); // update Cairo window list based on new screen setup
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
