using System.Windows.Controls;

namespace CairoDesktop.ObjectModel
{
    public abstract class MenuExtra
    {
        public abstract UserControl StartControl(IMenuExtraHost host);
    }
}