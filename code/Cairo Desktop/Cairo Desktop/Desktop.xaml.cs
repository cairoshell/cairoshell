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
using System.Windows.Interop;
using System.Windows.Forms;
using CairoDesktop.Interop;
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

            WindowInteropHelper f = new WindowInteropHelper(this);
            int result = NativeMethods.SetShellWindow(f.Handle);
            Shell.ShowWindowBottomMost(f.Handle);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            WindowInteropHelper f = new WindowInteropHelper(this);
            int result = NativeMethods.SetShellWindow(f.Handle);
            Shell.ShowWindowBottomMost(f.Handle);

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
