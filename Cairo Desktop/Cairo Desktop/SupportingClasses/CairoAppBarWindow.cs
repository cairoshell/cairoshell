using ManagedShell;
using ManagedShell.AppBar;

namespace CairoDesktop.SupportingClasses
{
    public class CairoAppBarWindow : AppBarWindow
    {
        protected readonly WindowManager _windowManager;
        
        public CairoAppBarWindow() : base(null, null, null)
        { }
        
        public CairoAppBarWindow(ShellManager shellManager, WindowManager windowManager) : base(
            shellManager.AppBarManager, shellManager.ExplorerHelper, shellManager.FullScreenHelper)
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
