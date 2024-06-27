using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class ClockMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICommandService _commandService;
        private Clock _clock;

        internal ClockMenuBarExtension(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!Settings.Instance.EnableMenuExtraClock)
            {
                return null;
            }

            _clock = new Clock(host, _commandService);
            return _clock;
        }
    }
}