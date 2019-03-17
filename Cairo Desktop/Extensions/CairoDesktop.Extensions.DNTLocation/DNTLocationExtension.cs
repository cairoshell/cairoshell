using CairoDesktop.Extensibility.ObjectModel;
using System.ComponentModel.Composition;

namespace CairoDesktop.Extensions.DNTLocation
{
    /// <summary>
    /// This is a very dirty example. We may want to tear the whole DesktopLocationToolbar right out of cairo and make it a plugin.
    /// There should NEVER be a scenario where a plugin creates settings and the core of cairo uses them but this is what I came up with.
    /// </summary>
    [Export(typeof(ShellExtension))]
    public sealed class DNTLocationExtension : ShellExtension
    {
        public override void Start()
        {
            CairoDesktop.Configuration.Settings.Initializing += Settings_Initializing;
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        private void Settings_Initializing(object sender, System.EventArgs args)
        {
            CairoDesktop.Configuration.Settings.AddPropertySetting("DesktopNavigationToolbarTop", typeof(double?), null);
            CairoDesktop.Configuration.Settings.AddPropertySetting("DesktopNavigationToolbarLeft", typeof(double?), null);
        }


    }
}
