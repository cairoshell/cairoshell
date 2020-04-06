using CairoDesktop.Interop;
using CairoDesktop.ObjectModel;
using System.Windows.Controls;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ActionCenterMenuExtra : MenuExtra
    {
        public ActionCenterMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                ActionCenter actionCenter = new ActionCenter(menuBar);

                return actionCenter;
            }
            else
            {
                return null;
            }
        }
    }
}
