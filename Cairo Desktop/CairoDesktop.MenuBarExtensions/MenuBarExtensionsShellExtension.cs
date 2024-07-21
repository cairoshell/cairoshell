using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ExtensionMethods;
using ManagedShell;
using System.Collections.Generic;
using CairoDesktop.Infrastructure.Services;

namespace CairoDesktop.MenuBarExtensions
{
    public sealed class MenuBarExtensionsShellExtension : IShellExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly ICommandService _commandService;
        private readonly ShellManager _shellManager;

        public List<IMenuBarExtension> MenuExtras { get; private set; }

        public MenuBarExtensionsShellExtension(ICairoApplication cairoApplication, ShellManagerService shellManagerService, ICommandService commandService)
        {
            _cairoApplication = cairoApplication;
            _commandService = commandService;
            _shellManager = shellManagerService.ShellManager;
            MenuExtras = new List<IMenuBarExtension>();
        }

        public void Start()
        {
            MenuExtras.AddRange(new ClockMenuBarExtension(_commandService),
                new SystemTrayMenuBarExtension(_shellManager.NotificationArea),
                new VolumeMenuBarExtension(),
                new ActionCenterMenuBarExtension(),
                new SearchMenuBarExtension(_cairoApplication));

            MenuExtras.AddTo(_cairoApplication.MenuBarExtensions);
        }

        public void Stop()
        {
            MenuExtras.RemoveFrom(_cairoApplication.MenuBarExtensions);
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}