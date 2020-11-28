using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SystemTrayMenuExtra : MenuExtra
    {
        private SystemTray _systemTray;

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (!Settings.Instance.EnableSysTray)
            {
                return null;
            }

            _systemTray = new SystemTray(menuBar);
            return _systemTray;
        }
    }
}