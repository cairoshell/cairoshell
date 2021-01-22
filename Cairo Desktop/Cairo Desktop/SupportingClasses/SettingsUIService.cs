using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.Services;

namespace CairoDesktop.SupportingClasses
{
    class SettingsUIService : ISettingsUIService
    {
        private readonly AppGrabberService _appGrabber;
        private readonly IApplicationUpdateService _updateService;
        private readonly ShellManagerService _shellManager;

        internal SettingsUI SettingsUi;
        
        public SettingsUIService(AppGrabberService appGrabber, IApplicationUpdateService updateService,
            ShellManagerService shellManager)
        {
            _appGrabber = appGrabber;
            _shellManager = shellManager;
            _updateService = updateService;
        }
        
        public void Show()
        {
            if (SettingsUi == null)
            {
                SettingsUi = new SettingsUI(this, _shellManager, _updateService, _appGrabber);
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
