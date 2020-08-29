using System.Windows.Controls;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        private Search _search;

        public SearchMenuExtra()
        {
        }

        public override UserControl StartControl(MenuBar menuBar)
        {
            _search = new Search(menuBar);
            return _search;
        }
    }
}