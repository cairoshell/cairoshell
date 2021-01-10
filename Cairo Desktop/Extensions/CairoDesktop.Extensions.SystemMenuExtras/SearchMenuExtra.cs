using CairoDesktop.ObjectModel;
using System.Windows.Controls;
using CairoDesktop.Configuration;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        private Search _search;

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableMenuExtraSearch)
            {
                _search = new Search(menuBar);
                return _search;
            }

            return null;
        }
    }
}