using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ExtensionMethods;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Collections.Generic;
using ManagedShell;
using CairoDesktop.SupportingClasses;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public sealed class SystemMenuExtrasExtension : IShellExtension
    {
        private ShellManager _shellManager;
        
        public List<MenuExtra> MenuExtras { get; private set; }

        public SystemMenuExtrasExtension(ShellManagerService shellManagerService)
        {
            _shellManager = shellManagerService.ShellManager;
            MenuExtras = new List<MenuExtra>();
        }

        public void Start()
        {
            MenuExtras.AddRange(new SystemTrayMenuExtra(_shellManager.NotificationArea),
                                new VolumeMenuExtra(),
                                new ActionCenterMenuExtra(),
                                new ClockMenuExtra(),
                                new SearchMenuExtra());

            MenuExtras.AddTo(CairoApplication.Current.MenuExtras);
        }

        public void Stop()
        {
            MenuExtras.RemoveFrom(CairoApplication.Current.MenuExtras);
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}