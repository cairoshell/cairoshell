using CairoDesktop.ObjectModel;
using System.Windows.Controls;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ActionCenterMenuExtra : MenuExtra
    {
        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Settings.Instance.EnableMenuExtraActionCenter && EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                return new ActionCenter(menuBar);
            }

            return null;
        }
    }
}