using CairoDesktop.Extensibility.Attributes;
using CairoDesktop.Extensibility.Plugins;
using CairoDesktop.Plugins.CairoShellCoreServices;


namespace CairoDesktop.Plugins.ShellExtensionSupport
{
    [PluginName("ShellExtension Support Plugin")]
    [PluginAuthors("Josuave")]
    [PluginDescription("Plugin that extends the plugin system using Microsoft's Managed Extensability Framework")]
    [PluginId("63C24EEB-8998-4F0A-B6D4-97EEFB23D759")]
    [PluginManufacturer("Cairo Development Team")]
    [PluginVersion("0.0.0.1")]
    [PluginDependency(typeof(CairoShellCoreServicesPlugin))]
    public sealed class ShellExtensionSupportPlugin : Plugin
    {
        private ShellExtensionService shellExtensionService = new ShellExtensionService();

        protected override void Start(PluginContext context, PluginDescriptorEventArgs e)
        {
            shellExtensionService.Start();
        }

        protected override void Stop(PluginContext context, PluginDescriptorEventArgs e)
        {
            shellExtensionService.Stop();
        }
    }
}