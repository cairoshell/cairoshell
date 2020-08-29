using System.Windows.Controls;
using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SystemTrayMenuExtra : MenuExtra
    {
        private SystemTray _systemTray;

        public SystemTrayMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableSysTray)
            {
                _systemTray = new SystemTray(menuBar);
                return _systemTray;
            }
            else
            {
                return null;
            }
        }
    }
}