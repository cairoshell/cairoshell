using System.ComponentModel.Composition;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    [Export(typeof(IDependencyRegistrant))]
    public class DependencyRegistrant : IDependencyRegistrant
    {
        public void Register(IDependencyRegistrar registrar)
        {
           registrar.AddSingleton<ShellExtension, ShellFolderExtension>();
        }
    }
}