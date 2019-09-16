using CairoDesktop.Common.Logging;
using CairoDesktop.Extensibility.Attributes;
using CairoDesktop.Extensibility.Plugins;

namespace CairoDesktop.Plugins.CairoShellCoreServices
{
    [PluginName("CairoShell Core Services Plugin")]
    [PluginAuthors("Josuave")]
    [PluginDescription("Plugin that contains the core functionality of CairoShell")]
    [PluginId("406C38DC-8C92-4983-A8DD-756B754C2877")]
    [PluginManufacturer("Cairo Development Team")]
    [PluginVersion("0.0.0.1")]
    public sealed class CairoShellCoreServicesPlugin : Plugin
    {
        protected override void Start(PluginContext context, PluginDescriptorEventArgs e)
        {
            CairoLogger.Instance.Debug("CairoShellCoreServicesPlugin Start");
        }

        protected override void Stop(PluginContext context, PluginDescriptorEventArgs e)
        {
            CairoLogger.Instance.Debug("CairoShellCoreServicesPlugin Stop");
        }
    }
}