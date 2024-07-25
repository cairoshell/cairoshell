using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class VolumeMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly Settings _settings;
        private Volume _volume;

        internal VolumeMenuBarExtension(Settings settings)
        {
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraVolume || !EnvironmentHelper.IsWindows10OrBetter || !EnvironmentHelper.IsAppRunningAsShell)
                return null;

            _volume = new Volume();
            return _volume;
        }
    }
}