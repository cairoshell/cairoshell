using CairoDesktop.Extensibility.Attributes;
using CairoDesktop.Extensibility.ObjectModel;
using CairoDesktop.Extensibility.Plugins;

namespace CairoDesktop.Plugins.Places1
{
    [PluginName("GodMode Plugin")]
    [PluginAuthors("Josuave")]
    [PluginDescription("Plugin that adds an 'GodMode' to the Places Menu")]
    [PluginId("e658f49a-244b-4db8-9e9d-b4ec2623024")]
    [PluginManufacturer("CairoShell")]
    [PluginVersion("0.0.0.2")]
    public sealed class Places1Plugin : Plugin
    {
        public GodModeMenuItem MenuItem { get; private set; }

        protected override void Start(PluginContext context, PluginDescriptorEventArgs e)
        {
            MenuItem = new GodModeMenuItem();
            _CairoShell.Instance.PlacesMenu.Add(MenuItem);
        }

        protected override void Stop(PluginContext context, PluginDescriptorEventArgs e)
        {
            _CairoShell.Instance.PlacesMenu.Remove(MenuItem);
            MenuItem = null;
        }
    }
}