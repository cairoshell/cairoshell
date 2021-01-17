using CairoDesktop.Configuration;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.WindowsTray;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SystemTrayMenuExtra : MenuExtra
    {
        private NotificationArea _notificationArea;
        private SystemTray _systemTray;

        internal SystemTrayMenuExtra(NotificationArea notificationArea)
        {
            _notificationArea = notificationArea;
        }

        public override UserControl StartControl(IMenuExtraHost host)
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