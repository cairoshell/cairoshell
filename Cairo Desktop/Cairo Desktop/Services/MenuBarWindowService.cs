using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.Services;
using CairoDesktop.SupportingClasses;
using ManagedShell.AppBar;

namespace CairoDesktop.Services
{
    public class MenuBarWindowService : AppBarWindowService
    {
        private readonly AppGrabberService _appGrabber;
        private readonly ISettingsUIService _settingsUiService;
        private readonly IApplicationUpdateService _updateService;

        public MenuBarWindowService(ICairoApplication cairoApplication, ShellManagerService shellManagerService, WindowManager windowManager, AppGrabberService appGrabber, IApplicationUpdateService updateService, ISettingsUIService settingsUiService) : base(cairoApplication, shellManagerService, windowManager)
        {
            _appGrabber = appGrabber;
            _settingsUiService = settingsUiService;
            _updateService = updateService;

            EnableMultiMon = Settings.Instance.EnableMenuBarMultiMon;
            EnableService = Settings.Instance.EnableMenuBar;
        }

        protected override void HandleSettingChange(string setting)
        {
            switch (setting)
            {
                case "EnableMenuBar":
                    HandleEnableServiceChanged(Settings.Instance.EnableMenuBar);
                    break;
                case "EnableMenuBarMultiMon":
                    HandleEnableMultiMonChanged(Settings.Instance.EnableMenuBarMultiMon);
                    break;
            }
        }

        protected override void OpenWindow(AppBarScreen screen)
        {
            MenuBar newMenuBar = new MenuBar(_cairoApplication, _shellManager, _windowManager, _appGrabber, _updateService, _settingsUiService, screen, AppBarEdge.Top);
            Windows.Add(newMenuBar);
            newMenuBar.Show();
        }
    }
}
