using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class VolumeMenuBarExtension : UserControlMenuBarExtension
    {
        private Volume _volume;

        public override UserControl StartControl(IMenuBar host)
        {
            if (Enabled)
                return null;

            _volume = new Volume();
            return _volume;
        }

        private static bool Enabled => !Settings.Instance.EnableMenuExtraVolume || !EnvironmentHelper.IsWindows10OrBetter || !EnvironmentHelper.IsAppRunningAsShell;
    }
}