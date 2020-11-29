using CairoDesktop.Application.Interfaces;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public class DependencyRegistrant : IDependencyRegistrant
    {
        public void Register(IDependencyRegistrar registrar)
        {
            registrar.AddSingleton<ShellExtension, SystemMenuExtrasExtension>();
        }
    }
}