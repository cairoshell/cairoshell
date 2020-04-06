using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ClockMenuExtra : MenuExtra
    {
        public ClockMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            Clock clock = new Clock(menuBar);

            return clock;
        }
    }
}
