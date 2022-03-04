using ManagedShell.Common.Helpers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class LayoutSwitcher : UserControl
    {
        private int lastZeroLayout;
        private int lastForegroundLayout;

        public LayoutSwitcher()
        {
            InitializeComponent();

            InitializeLayoutIndicator();
        }

        private void InitializeLayoutIndicator()
        {
            LayoutIndicator_Tick();

            DispatcherTimer layoutIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, delegate
            {
                LayoutIndicator_Tick();
            }, Dispatcher);
        }

        private void LayoutIndicator_Tick()
        {
            //HACK: I don't why, but GetKeyboardLayout with foreground window PID doesn't update when explorer.exe is focused.
            //(ex. try to click desktop and try to change layout)
            //but: with zero PID it updates when explorer.exe or Cairo is focused only.
            var newZeroLayout = (short) LayoutHelper.GetCurrentLayoutId(true);
            var newForegroundLayout = (short) LayoutHelper.GetCurrentLayoutId(false);

            if (newForegroundLayout != lastForegroundLayout)
            {
                currentLayout.Text = CultureInfo.GetCultureInfo(newForegroundLayout).ThreeLetterISOLanguageName.ToUpper();
            } else if (newZeroLayout != lastZeroLayout)
            {
                currentLayout.Text = CultureInfo.GetCultureInfo(newZeroLayout).ThreeLetterISOLanguageName.ToUpper();
            }

            lastForegroundLayout = newForegroundLayout;
            lastZeroLayout = newZeroLayout;
        }

        private void LayoutSwitcherItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            
        }

        private void OpenLayoutSettings(object sender, RoutedEventArgs e)
        {
            //Keyboard layout settings was removed from intl.cpl in Windows 10 (?) 
            if (EnvironmentHelper.IsWindows10OrBetter)
            {
                ShellHelper.StartProcess("ms-settings:keyboard");
                return;
            }
            ShellHelper.StartProcess("intl.cpl");
        }
    }
}
