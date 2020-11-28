using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        private Search _search;

        public override UserControl StartControl(MenuBar menuBar)
        {
            _search = new Search(menuBar);
            return _search;
        }
    }
}