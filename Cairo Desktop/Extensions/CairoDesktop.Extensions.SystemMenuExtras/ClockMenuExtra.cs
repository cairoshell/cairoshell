using System.Windows.Controls;
using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ClockMenuExtra : MenuExtra
    {
        private Clock _clock;

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (!Settings.Instance.EnableMenuExtraClock)
            {
                return null;
            }

            _clock = new Clock(menuBar);
            return _clock;
        }
    }
}