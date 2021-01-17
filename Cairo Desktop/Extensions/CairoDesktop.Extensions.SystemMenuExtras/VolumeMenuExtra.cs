using CairoDesktop.Configuration;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class VolumeMenuExtra : MenuExtra
    {
        private Volume _volume;

        public override UserControl StartControl(IMenuExtraHost host)
        {
            if (Settings.Instance.EnableMenuExtraVolume && EnvironmentHelper.IsWindows10OrBetter && EnvironmentHelper.IsAppRunningAsShell)
            {
                _volume = new Volume();
                return _volume;
            }

            return null;
        }
    }
}