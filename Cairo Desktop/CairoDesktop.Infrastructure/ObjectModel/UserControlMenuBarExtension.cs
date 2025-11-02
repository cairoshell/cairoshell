using CairoDesktop.Application.Interfaces;
using System.Windows.Controls;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public abstract class UserControlMenuBarExtension : IMenuBarExtension<UserControl>
    {
        public abstract UserControl StartControl(IMenuBar host);

        // Optional as some controls may not need to do anything to stop
        public virtual void StopControl(IMenuBar host) { }
    }
}