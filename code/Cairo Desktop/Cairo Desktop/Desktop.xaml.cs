using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.IO;
using System.Windows.Threading;
using System.Collections.Generic;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using System.Windows.Forms;
//using System.Windows.Shapes;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window
    {
        public Stack<string> PathHistory = new Stack<string>();
        public DesktopIcons Icons;
        public Desktop()
        {
            InitializeComponent();
        }

        private void ShowWindowBottomMost_Internal(IntPtr handle)
        {
            NativeMethods.SetWindowPos(
                handle,
                (IntPtr)NativeMethods.HWND_BOTTOMMOST,
                0,
                0,
                0,
                0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        public void ShowWindowBottomMost(IntPtr handle)
        {
            this.ShowWindowBottomMost_Internal((new WindowInteropHelper(this)).Handle);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            WindowInteropHelper f = new WindowInteropHelper(this);
            this.ShowWindowBottomMost(f.Handle);

            if (Properties.Settings.Default.EnableDesktop && Icons == null)
            {
                Icons = new DesktopIcons();
                grid.Children.Add(Icons);

                if (Properties.Settings.Default.EnableDynamicDesktop)
                {
                    DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };

                    nav.Show();
                }
            }
        }
    }
}
