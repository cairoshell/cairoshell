using CairoDesktop.Extensibility.Attributes;
using CairoDesktop.Extensibility.ObjectModel;
using CairoDesktop.Extensibility.Plugins;
using System.Diagnostics;
using System.Windows.Controls;

namespace CairoDesktop.Plugins.Places1
{
   [PluginName("Plugin that adds an Item to the Places Menu")]
    [PluginAuthors("Joe LaFountain")]
    [PluginDescription("Provides private labeling of WelcomeScreen")]
    [PluginId("e658f49a-244b-4db8-9e9d-b4ec2623024")]
    [PluginManufacturer("OptionSoft Technologies, Inc.")]
    [PluginVersion("0.0.0.1")]
    public sealed class Places1Plugin : Plugin
    {
        public MenuItem MenuItem { get; private set; }

        protected override void Start(PluginContext context, PluginDescriptorEventArgs e)
        {
            MenuItem = new MenuItem();
            MenuItem.Header = "GodMode";
            MenuItem.Click += MenuItem_Click;

            _Application.Instance.PlacesMenu.Add(MenuItem);
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // https://winaero.com/blog/clsid-guid-shell-list-windows-10/
            Process.Start("shell:::{ED7BA470-8E54-465E-825C-99712043E01C}");
        }

        protected override void Stop(PluginContext context, PluginDescriptorEventArgs e)
        {
        }
    }
}
