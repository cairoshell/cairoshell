using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class ClockMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private Clock _clock;

        internal ClockMenuBarExtension(ICommandService commandService, Settings settings)
        {
            _commandService = commandService;
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraClock)
            {
                return null;
            }

            _clock = new Clock(host, _commandService, _settings);
            return _clock;
        }
    }
}