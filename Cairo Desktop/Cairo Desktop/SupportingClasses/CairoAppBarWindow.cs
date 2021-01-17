using System.Windows.Forms;
using ManagedShell;
using ManagedShell.AppBar;
using ManagedShell.Interop;
using ManagedShell.WindowsTray;

namespace CairoDesktop.SupportingClasses
{
    public class CairoAppBarWindow : AppBarWindow
    {
        protected readonly WindowManager _windowManager;
        
        public CairoAppBarWindow() : base(null, null, null, Screen.PrimaryScreen, NativeMethods.ABEdge.ABE_BOTTOM, 0)
        { }
        
        public CairoAppBarWindow(ShellManager shellManager, WindowManager windowManager, Screen screen, NativeMethods.ABEdge edge, double height) : base(
            shellManager.AppBarManager, shellManager.ExplorerHelper, shellManager.FullScreenHelper, screen, edge, height)
        {
            _windowManager = windowManager;
            
            AllowsTransparency = true;
        }

        protected override void SetScreenProperties(ScreenSetupReason reason)
        {
            _windowManager.NotifyDisplayChange(reason); // update Cairo window list based on new screen setup
        }
    }
}
