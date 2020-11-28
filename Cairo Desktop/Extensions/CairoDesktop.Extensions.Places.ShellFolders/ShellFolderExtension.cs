using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    [Export(typeof(ShellExtension))]
    public sealed class ShellFolderExtension : ShellExtension
    {
        public List<MenuItem> MenuItems { get; private set; }

        public ShellFolderExtension()
        {
            MenuItems = new List<MenuItem>();
        }

        public override void Start()
        {
            MenuItems.AddRange(new[]
            {
                new ShellLocationMenuItem("All Tasks", "shell:::{ED7BA470-8E54-465E-825C-99712043E01C}"),
                new ShellLocationMenuItem("OneDrive", "shell:::{018D5C66-4533-4307-9B53-224DE2ED1FE6}")
            });

            CairoApplication.Current.PlacesMenu.AddRange(MenuItems);
        }

        public override void Stop()
        {
            MenuItems.Select(CairoApplication.Current.PlacesMenu.Remove).ToList();
            MenuItems.Clear();
            MenuItems = null;
        }
    }
}