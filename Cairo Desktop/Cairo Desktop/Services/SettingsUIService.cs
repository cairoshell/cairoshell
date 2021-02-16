using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.Services;

namespace CairoDesktop.Services
{
    public class SettingsUIService : ISettingsUIService
    {
        private readonly AppGrabberService _appGrabber;
        private readonly ICairoApplication _cairoApplication;
        private readonly ShellManagerService _shellManager;
        private readonly ThemeService _themeService;
        private readonly IApplicationUpdateService _updateService;

        internal SettingsUI SettingsUi;
        
        public SettingsUIService(ICairoApplication cairoApplication, AppGrabberService appGrabber, IApplicationUpdateService updateService,
            ShellManagerService shellManager, ThemeService themeService)
        {
            _appGrabber = appGrabber;
            _cairoApplication = cairoApplication;
            _shellManager = shellManager;
            _themeService = themeService;
            _updateService = updateService;
        }
        
        public void Show()
        {
            if (SettingsUi == null)
            {
                SettingsUi = new SettingsUI(_cairoApplication, this, _shellManager, _updateService, _appGrabber, _themeService);
            }
            
            SettingsUi.Show();
            SettingsUi.Activate();
        }

        public void Show(string tabIdentifier)
        {
            // Eventually extensions should be able to add their own tab to the settings window. To show their tab, they pass their extension GUID here.

            Show();
            
            switch (tabIdentifier)
            {
                case "desktop":
                    SettingsUi.TabDesktop.IsSelected = true;
                    break;
            }
        }
    }
}
