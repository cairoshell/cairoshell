using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuExtraActionCenter.xaml
    /// </summary>
    public partial class MenuExtraActionCenter : UserControl
    {
        private IntPtr _parentHwnd;

        public MenuExtraActionCenter(MenuBar menuBar)
        {
            InitializeComponent();

            _parentHwnd = menuBar.Handle;
        }

        private void miOpenActionCenter_Click(object sender, RoutedEventArgs e)
        {
            Shell.ShowActionCenter();
        }

        private void miOpenActionCenter_MouseEnter(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE,
                        NativeMethods.GetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_NOACTIVATE);
        }

        private void miOpenActionCenter_MouseLeave(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE,
                        NativeMethods.GetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE) & ~(int)NativeMethods.ExtendedWindowStyles.WS_EX_NOACTIVATE);
        }
    }
}
