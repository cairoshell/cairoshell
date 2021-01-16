using CairoDesktop.ObjectModel;
using System.Windows.Controls;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;

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