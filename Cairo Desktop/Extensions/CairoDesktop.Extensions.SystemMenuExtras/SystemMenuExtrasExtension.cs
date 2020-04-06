using CairoDesktop.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

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
            MenuExtras.AddRange(new MenuExtra[]
            {
                new SystemTrayMenuExtra(),
                new VolumeMenuExtra(),
                new ActionCenterMenuExtra(),
                new ClockMenuExtra(),
                new SearchMenuExtra()
            });

            _CairoShell.Instance.MenuExtras.AddRange(MenuExtras);
        }

        public override void Stop()
        {
            MenuExtras.Select(_CairoShell.Instance.MenuExtras.Remove).ToList();
            MenuExtras.Clear();
            MenuExtras = null;
        }
    }
}
