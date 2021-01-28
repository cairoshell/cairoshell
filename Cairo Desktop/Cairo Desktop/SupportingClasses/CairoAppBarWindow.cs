using CairoDesktop.Application.Interfaces;
using CairoDesktop.Services;
using ManagedShell;
using ManagedShell.AppBar;

namespace CairoDesktop.SupportingClasses
{
    public class CairoAppBarWindow : AppBarWindow
    {
        protected readonly ICairoApplication _cairoApplication;
        protected readonly WindowManager _windowManager;
        
        public CairoAppBarWindow() : base(null, null, null, AppBarScreen.FromPrimaryScreen(), AppBarEdge.Bottom, 0)
        { }
        
        public CairoAppBarWindow(ICairoApplication cairoApplication, ShellManager shellManager, WindowManager windowManager, AppBarScreen screen, AppBarEdge edge, double height) : base(
            shellManager.AppBarManager, shellManager.ExplorerHelper, shellManager.FullScreenHelper, screen, edge, height)
        {
            _cairoApplication = cairoApplication;
            _windowManager = windowManager;
            
            AllowsTransparency = true;
        }

        protected override void SetScreenProperties(ScreenSetupReason reason)
        {
            _windowManager.NotifyDisplayChange(reason); // update Cairo window list based on new screen setup
        }
    }
}
