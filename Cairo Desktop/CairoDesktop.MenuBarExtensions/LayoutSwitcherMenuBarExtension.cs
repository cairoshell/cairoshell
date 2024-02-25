using CairoDesktop.Configuration;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;

namespace CairoDesktop.MenuBarExtensions
{
    class LayoutSwitcherMenuBarExtension : UserControlMenuBarExtension
    {
        private LayoutSwitcher _layoutSwitcher;

        public override UserControl StartControl(IMenuBar host)
        {
            if (!Settings.Instance.EnableMenuExtraLayoutSwitcher)
            {
                return null;
            }

            _layoutSwitcher = new LayoutSwitcher();
            return _layoutSwitcher;
        }
    }
}
