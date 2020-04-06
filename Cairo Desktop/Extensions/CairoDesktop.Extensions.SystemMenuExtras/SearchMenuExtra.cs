using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class SearchMenuExtra : MenuExtra
    {
        public SearchMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            Search search = new Search(menuBar);

            return search;
        }
    }
}
