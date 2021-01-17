using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public abstract class MenuExtra
    {
        public abstract UserControl StartControl(IMenuExtraHost host);
    }
}