using CairoDesktop.Configuration;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.WindowsTray;

namespace CairoDesktop.MenuBarExtensions
{
    class SystemTrayMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly NotificationArea _notificationArea;
        private SystemTray _systemTray;

        internal SystemTrayMenuBarExtension(NotificationArea notificationArea)
        {
            _notificationArea = notificationArea;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!Settings.Instance.EnableSysTray)
            {
                return null;
            }

            _systemTray = new SystemTray(host, _notificationArea);
            return _systemTray;
        }
    }
}