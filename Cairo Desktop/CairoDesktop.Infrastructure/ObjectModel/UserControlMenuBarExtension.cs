using CairoDesktop.Application.Interfaces;
using System.Windows.Controls;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public abstract class UserControlMenuBarExtension : IMenuBarExtension<UserControl>
    {
        public abstract UserControl StartControl(IMenuBar host);
    }
}