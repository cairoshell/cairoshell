using CairoDesktop.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    public sealed class ShellFolderExtension : IShellExtension
    {
        private readonly ICairoApplication<RoutedEventArgs> _app;
        public List<IMenuItem<RoutedEventArgs>> MenuItems { get; private set; }

        public ShellFolderExtension(ICairoApplication<RoutedEventArgs> app)
        {
            _app = app;
            MenuItems = new List<IMenuItem<RoutedEventArgs>>();
        }

        public void Start()
        {
            MenuItems.AddRange(new[]
            {
                new ShellLocationMenuItem("All Tasks", "shell:::{ED7BA470-8E54-465E-825C-99712043E01C}"),
                new ShellLocationMenuItem("OneDrive", "shell:::{018D5C66-4533-4307-9B53-224DE2ED1FE6}")
            });

            _app.Places.AddRange(MenuItems);
        }

        public void Stop()
        {
            MenuItems.Select(_app.Places.Remove).ToList();
            MenuItems.Clear();
            MenuItems = null;
        }
    }
}