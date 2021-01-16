using CairoDesktop.ObjectModel;
using System.Windows.Controls;
using CairoDesktop.Configuration;

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