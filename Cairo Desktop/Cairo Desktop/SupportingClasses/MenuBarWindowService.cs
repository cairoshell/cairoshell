using System.Windows.Forms;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;

namespace CairoDesktop.SupportingClasses
{
    public class MenuBarWindowService : AppBarWindowService
    {
        private readonly IApplicationUpdateService _updateService;

        public MenuBarWindowService(IApplicationUpdateService updateService)
        {
            _updateService = updateService;

            EnableMultiMon = Settings.Instance.EnableMenuBarMultiMon;
            EnableService = true;
        }

        protected override void OpenWindow(Screen screen)
        {
            MenuBar newMenuBar = new MenuBar(this, _windowManager, _updateService, screen);
            newMenuBar.Show();
            Windows.Add(newMenuBar);
        }
    }
}
