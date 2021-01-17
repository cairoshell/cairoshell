using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        private Search _search;

        public override UserControl StartControl(IMenuExtraHost host)
        {
            if (Settings.Instance.EnableMenuExtraSearch)
            {
                _search = new Search(host);
                return _search;
            }

            return null;
        }
    }
}