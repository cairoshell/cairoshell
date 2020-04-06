using CairoDesktop.Configuration;
using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class VolumeMenuExtra : MenuExtra
    {
        public VolumeMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableSysTray)
            {
                Volume volume = new Volume();

                return volume;
            }
            else
            {
                return null;
            }
        }
    }
}
