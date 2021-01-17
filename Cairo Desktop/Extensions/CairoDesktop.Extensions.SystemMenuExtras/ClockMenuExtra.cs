using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ClockMenuExtra : MenuExtra
    {
        private Clock _clock;

        public override UserControl StartControl(IMenuExtraHost host)
        {
            if (!Settings.Instance.EnableMenuExtraClock)
            {
                return null;
            }

            _clock = new Clock(host);
            return _clock;
        }
    }
}