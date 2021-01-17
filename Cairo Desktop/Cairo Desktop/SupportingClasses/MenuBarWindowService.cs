using System.Windows.Forms;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using ManagedShell.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class MenuBarWindowService : AppBarWindowService
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly IApplicationUpdateService _updateService;

        public MenuBarWindowService(ICairoApplication cairoApplication, ShellManagerService shellManagerService, WindowManager windowManager, IApplicationUpdateService updateService) : base(shellManagerService, windowManager)
        {
            _cairoApplication = cairoApplication;
            _updateService = updateService;

            EnableMultiMon = Settings.Instance.EnableMenuBarMultiMon;
            EnableService = Settings.Instance.EnableMenuBar;
        }

        protected override void OpenWindow(Screen screen)
        {
            MenuBar newMenuBar = new MenuBar(_cairoApplication, _shellManager, _windowManager, _updateService, screen, NativeMethods.ABEdge.ABE_TOP);
            Windows.Add(newMenuBar);
            newMenuBar.Show();
        }
    }
}
