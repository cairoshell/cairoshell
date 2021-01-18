using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class ClockMenuBarExtension : UserControlMenuBarExtension
    {
        private Clock _clock;

        public override UserControl StartControl(IMenuBar host)
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