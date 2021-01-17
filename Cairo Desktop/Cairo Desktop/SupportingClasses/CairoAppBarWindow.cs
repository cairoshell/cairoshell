using System.Windows.Forms;
using CairoDesktop.Application.Interfaces;
using ManagedShell;
using ManagedShell.AppBar;
using ManagedShell.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class CairoAppBarWindow : AppBarWindow
    {
        protected readonly ICairoApplication _cairoApplication;
        protected readonly WindowManager _windowManager;
        
        public CairoAppBarWindow() : base(null, null, null, Screen.PrimaryScreen, NativeMethods.ABEdge.ABE_BOTTOM, 0)
        { }
        
        public CairoAppBarWindow(ICairoApplication cairoApplication, ShellManager shellManager, WindowManager windowManager, Screen screen, NativeMethods.ABEdge edge, double height) : base(
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
