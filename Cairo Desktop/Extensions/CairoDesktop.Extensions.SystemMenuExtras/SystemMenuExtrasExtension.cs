using System.Collections.Generic;
using System.ComponentModel.Composition;
using CairoDesktop.Common.ExtensionMethods;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    [Export(typeof(ShellExtension))]
    public sealed class SystemMenuExtrasExtension : ShellExtension
    {
        public List<MenuExtra> MenuExtras { get; private set; }

        public SystemMenuExtrasExtension()
        {
            MenuExtras = new List<MenuExtra>();
        }

        public override void Start()
        {
            MenuExtras.AddRange(new SystemTrayMenuExtra(),
                                new VolumeMenuExtra(),
                                new ActionCenterMenuExtra(),
                                new ClockMenuExtra(),
                                new SearchMenuExtra());

            MenuExtras.AddTo(CairoApplication.Current.MenuExtras);
        }

        public override void Stop()
        {
            MenuExtras.RemoveFrom(CairoApplication.Current.MenuExtras);
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}