using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        private readonly ICairoApplication _cairoApplication;
        private Search _search;

        internal SearchMenuExtra(ICairoApplication cairoApplication)
        {
            _cairoApplication = cairoApplication;
        }

        public override UserControl StartControl(IMenuExtraHost host)
        {
            if (Settings.Instance.EnableMenuExtraSearch)
            {
                _search = new Search(_cairoApplication, host);
                return _search;
            }

            return null;
        }
    }
}