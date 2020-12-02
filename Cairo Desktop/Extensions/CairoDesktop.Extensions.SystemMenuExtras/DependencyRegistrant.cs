using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public class DependencyRegistrant : IDependencyRegistrant
    {
        public void Register(IDependencyRegistrar registrar)
        {
            registrar.AddSingleton<IShellExtension, SystemMenuExtrasExtension>();
        }
    }
}