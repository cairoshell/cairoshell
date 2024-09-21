using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace CairoDesktop.MenuBarExtensions
{
    internal sealed class LangMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private LanguagePane _lang;

        internal LangMenuBarExtension(ICommandService commandService, Settings settings)
        {
            _commandService = commandService;
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            _lang = new LanguagePane(host, _commandService, _settings);
            return _lang;
        }
    }
}