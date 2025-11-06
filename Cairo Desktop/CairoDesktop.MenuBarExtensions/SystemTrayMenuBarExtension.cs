using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.WindowsTray;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class SystemTrayMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly NotificationArea _notificationArea;
        private readonly Settings _settings;
        private readonly List<SystemTray> _systemTrays = new List<SystemTray>();

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

            SystemTray tray = new SystemTray(host, _notificationArea, _settings);
            _systemTrays.Add(tray);
            return tray;
        }

        public override void StopControl(IMenuBar host, UserControl control)
        {
            if (control is SystemTray tray && _systemTrays.Contains(tray))
            {
                tray.Host = null;
                _systemTrays.Remove(tray);
            }
        }
    }
}