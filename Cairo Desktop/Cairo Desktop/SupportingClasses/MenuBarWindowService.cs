using System.Windows.Forms;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using ManagedShell.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class MenuBarWindowService : AppBarWindowService
    {
        private readonly IApplicationUpdateService _updateService;

        public MenuBarWindowService(ShellManagerService shellManagerService, WindowManager windowManager, IApplicationUpdateService updateService) : base(shellManagerService, windowManager)
        {
            _updateService = updateService;

            EnableMultiMon = Settings.Instance.EnableMenuBarMultiMon;
            EnableService = Settings.Instance.EnableMenuBar;
        }

        protected override void OpenWindow(Screen screen)
        {
            MenuBar newMenuBar = new MenuBar(_shellManager, _windowManager, _updateService, screen, NativeMethods.ABEdge.ABE_TOP);
            Windows.Add(newMenuBar);
            newMenuBar.Show();
        }
    }
}
