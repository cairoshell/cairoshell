using System.Windows.Controls;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ClockMenuExtra : MenuExtra
    {
        private Clock _clock;

        public ClockMenuExtra()
        {
        }

        public override UserControl StartControl(MenuBar menuBar)
        {
            _clock = new Clock(menuBar);
            return _clock;
        }
    }
}