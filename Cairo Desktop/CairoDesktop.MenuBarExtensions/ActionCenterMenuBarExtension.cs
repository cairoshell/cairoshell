using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class ActionCenterMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly Settings _settings;

        internal ActionCenterMenuBarExtension(Settings settings)
        {
            _settings = settings;
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (_settings.EnableMenuExtraActionCenter && EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                return new ActionCenter(host);
            }

            return null;
        }
    }
}