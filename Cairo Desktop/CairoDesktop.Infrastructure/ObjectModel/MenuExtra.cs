using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public abstract class MenuExtra: IMenuExtra<UserControl>
    {
        public abstract UserControl StartControl(IMenuExtraHost host);
    }
}