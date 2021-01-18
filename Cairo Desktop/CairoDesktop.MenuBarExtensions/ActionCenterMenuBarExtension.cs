using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using System.Windows.Controls;

namespace CairoDesktop.MenuBarExtensions
{
    class ActionCenterMenuBarExtension : UserControlMenuBarExtension
    {
        public override UserControl StartControl(IMenuBar host)
        {
            if (Settings.Instance.EnableMenuExtraActionCenter && EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                return new ActionCenter(host);
            }

            return null;
        }
    }
}