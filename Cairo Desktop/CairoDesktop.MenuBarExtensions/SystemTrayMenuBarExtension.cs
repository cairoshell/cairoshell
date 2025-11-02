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
        private List<SystemTray> _systemTrays = new List<SystemTray>();

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

        public override void StopControl(IMenuBar host)
        {
            List<SystemTray> toRemove = new List<SystemTray>();

            foreach (SystemTray tray in _systemTrays)
            {
                if (tray.Host != host)
                {
                    continue;
                }

                tray.Host = null;
                toRemove.Add(tray);
            }

            foreach (SystemTray tray in toRemove)
            {
                _systemTrays.Remove(tray);
            }

            toRemove.Clear();
        }
    }
}