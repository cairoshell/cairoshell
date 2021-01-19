using ManagedShell.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.Application.Interfaces;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.MenuBarExtensions
{
    /// <summary>
    /// Interaction logic for ActionCenter.xaml
    /// </summary>
    public partial class ActionCenter : UserControl
    {
        private IntPtr _parentHwnd;

        public ActionCenter(IMenuBar host)
        {
            InitializeComponent();

            _parentHwnd = host.GetHandle();
        }

        private void miOpenActionCenter_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.ShowActionCenter();
        }

        private void miOpenActionCenter_MouseEnter(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_NOACTIVATE);
        }

        private void miOpenActionCenter_MouseLeave(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(_parentHwnd, NativeMethods.GWL_EXSTYLE) & ~(int)NativeMethods.ExtendedWindowStyles.WS_EX_NOACTIVATE);
        }
    }
}