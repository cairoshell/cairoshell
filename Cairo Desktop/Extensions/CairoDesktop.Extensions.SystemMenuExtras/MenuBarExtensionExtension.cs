using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ExtensionMethods;
using CairoDesktop.SupportingClasses;
using ManagedShell;
using System.Collections.Generic;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public sealed class MenuBarExtensionsShellExtension : IShellExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly ShellManager _shellManager;

        public List<IMenuBarExtension> MenuExtras { get; private set; }

        public MenuBarExtensionsShellExtension(ICairoApplication cairoApplication, ShellManagerService shellManagerService)
        {
            _cairoApplication = cairoApplication;
            _shellManager = shellManagerService.ShellManager;
            MenuExtras = new List<IMenuBarExtension>();
        }

        public void Start()
        {
            MenuExtras.AddRange(new SystemTrayMenuBarExtension(_shellManager.NotificationArea),
                                new VolumeMenuBarExtension(),
                                new ActionCenterMenuBarExtension(),
                                new ClockMenuBarExtension(),
                                new SearchMenuBarExtension(_cairoApplication));

            MenuExtras.AddTo(CairoApplication.Current.MenuBarExtensions);
        }

        public void Stop()
        {
            MenuExtras.RemoveFrom(CairoApplication.Current.MenuBarExtensions);
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}