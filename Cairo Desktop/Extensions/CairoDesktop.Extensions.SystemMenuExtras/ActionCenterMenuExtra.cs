using CairoDesktop.ObjectModel;
using System.Windows.Controls;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ActionCenterMenuExtra : MenuExtra
    {
        public override UserControl StartControl(MenuBar menuBar)
        {
            if (EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                return new ActionCenter(menuBar);
            }

            return null;
        }
    }
}