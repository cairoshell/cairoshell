using System.Windows.Controls;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ActionCenterMenuExtra : MenuExtra
    {
        public override UserControl StartControl(IMenuExtraHost host)
        {
            if (Settings.Instance.EnableMenuExtraActionCenter && EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                return new ActionCenter(host);
            }

            return null;
        }
    }
}