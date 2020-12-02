using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.ExtensionMethods;
using CairoDesktop.ObjectModel;
using System.Collections.Generic;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public sealed class SystemMenuExtrasExtension : IShellExtension
    {
        public List<MenuExtra> MenuExtras { get; private set; }

        public SystemMenuExtrasExtension()
        {
            MenuExtras = new List<MenuExtra>();
        }

        public void Start()
        {
            MenuExtras.AddRange(new SystemTrayMenuExtra(),
                                new VolumeMenuExtra(),
                                new ActionCenterMenuExtra(),
                                new ClockMenuExtra(),
                                new SearchMenuExtra());

            MenuExtras.AddTo(CairoApplication.Current.MenuExtras);
        }

        public void Stop()
        {
            MenuExtras.RemoveFrom(CairoApplication.Current.MenuExtras);
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}