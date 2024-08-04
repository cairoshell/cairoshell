using CairoDesktop.Common;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.WindowsTray;

namespace CairoDesktop.MenuBarExtensions
{
    class SystemTrayMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly NotificationArea _notificationArea;
        private readonly Settings _settings;
        private SystemTray _systemTray;

        internal SystemTrayMenuBarExtension(NotificationArea notificationArea, Settings settings)
        {
            _notificationArea = notificationArea;
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableSysTray)
            {
                return null;
            }

            _systemTray = new SystemTray(host, _notificationArea, _settings);
            return _systemTray;
        }
    }
}