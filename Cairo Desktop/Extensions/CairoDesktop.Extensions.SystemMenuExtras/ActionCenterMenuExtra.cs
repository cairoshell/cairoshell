using System.Windows.Controls;
using CairoDesktop.Interop;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    class ActionCenterMenuExtra : MenuExtra
    {
        public ActionCenterMenuExtra() { }

        public override UserControl StartControl(MenuBar menuBar)
        {
            if (Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                return new ActionCenter(menuBar);
            }
            else
            {
                return null;
            }
        }
    }
}