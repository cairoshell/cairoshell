using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.MenuBarExtensions
{
    class SearchMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly Settings _settings;
        private Search _search;

        internal SearchMenuBarExtension(ICairoApplication cairoApplication, Settings settings)
        {
            _cairoApplication = cairoApplication;
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraSearch) 
                return null;

            _search = new Search(_cairoApplication, host);
            return _search;
        }
    }
}