using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.MenuBarExtensions
{
    class SearchMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private Search _search;

        internal SearchMenuBarExtension(ICairoApplication cairoApplication)
        {
            _cairoApplication = cairoApplication;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!Settings.Instance.EnableMenuExtraSearch) 
                return null;

            _search = new Search(_cairoApplication, host);
            return _search;
        }
    }
}