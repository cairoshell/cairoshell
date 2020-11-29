using CairoDesktop.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    public sealed class ShellFolderExtension : ShellExtension
    {
        private readonly CairoApplication _app;
        public List<MenuItem> MenuItems { get; private set; }

        public ShellFolderExtension(CairoApplication app)
        {
            _app = app;
            MenuItems = new List<MenuItem>();
        }

        public override void Start()
        {
            MenuItems.AddRange(new[]
            {
                new ShellLocationMenuItem("All Tasks", "shell:::{ED7BA470-8E54-465E-825C-99712043E01C}"),
                new ShellLocationMenuItem("OneDrive", "shell:::{018D5C66-4533-4307-9B53-224DE2ED1FE6}")
            });

            _app.PlacesMenu.AddRange(MenuItems);
        }

        public override void Stop()
        {
            MenuItems.Select(_app.PlacesMenu.Remove).ToList();
            MenuItems.Clear();
            MenuItems = null;
        }
    }
}