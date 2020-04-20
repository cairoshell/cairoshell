using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Windows;
using System.Windows.Interop;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuBarShadow.xaml
    /// </summary>
    public partial class MenuBarShadow : Window
    {
        public bool IsClosing = false;

        public MenuBar MenuBar;

        public MenuBarShadow(MenuBar bar)
        {
            MenuBar = bar;

            InitializeComponent();

            SetPosition();
        }

        public void SetPosition()
        {
            if (MenuBar != null)
            {
                // reset properties so we actually reposition
                Top = 0;
                
                Width = MenuBar.ActualWidth;
                Top = MenuBar.Top + MenuBar.ActualHeight;
                Left = MenuBar.Left;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            int HTTRANSPARENT = (-1);
            if (msg == (int)NativeMethods.WM.NCHITTEST)
            {
                handled = true;
                return (IntPtr)HTTRANSPARENT;
            }
            else
            {
                handled = false;
            }
            return IntPtr.Zero;
        }

        private void MenuBarShadow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);

            source.AddHook(new HwndSourceHook(WndProc));

            // Makes click-through by adding transparent style
            // basically same as Shell.HideWindowFromTasks(helper.Handle);
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TRANSPARENT);

            Shell.ExcludeWindowFromPeek(helper.Handle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            if (!Startup.IsShuttingDown && !WindowManager.Instance.IsSettingDisplays)
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }
    }
}
