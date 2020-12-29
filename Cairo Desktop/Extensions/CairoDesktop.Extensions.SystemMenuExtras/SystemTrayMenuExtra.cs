using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;
using System.Windows.Controls;
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

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (!Settings.Instance.EnableSysTray)
            {
                return null;
            }

            _systemTray = new SystemTray(menuBar, _notificationArea);
            return _systemTray;
        }
    }
}