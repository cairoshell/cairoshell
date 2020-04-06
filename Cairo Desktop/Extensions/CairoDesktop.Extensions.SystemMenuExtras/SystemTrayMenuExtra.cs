using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SystemTrayMenuExtra : MenuExtra
    {
        public SystemTrayMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableSysTray)
            {
                SystemTray systemTray = new SystemTray(menuBar);

                return systemTray;
            }
            else
            {
                return null;
            }
        }
    }
}
