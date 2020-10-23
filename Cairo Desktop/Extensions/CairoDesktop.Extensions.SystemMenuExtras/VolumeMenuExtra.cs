using System.Windows.Controls;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class VolumeMenuExtra : MenuExtra
    {
        private Volume _volume;

        public VolumeMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableSysTray && Shell.IsWindows10OrBetter && Shell.IsCairoRunningAsShell)
            {
                _volume = new Volume();
                return _volume;
            }
            else
            {
                return null;
            }
        }
    }
}