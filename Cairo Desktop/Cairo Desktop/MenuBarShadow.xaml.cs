using CairoDesktop.Interop;
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
        public System.Windows.Forms.Screen Screen;
        
        public bool IsClosing = false;

        public MenuBar MenuBar;

        public MenuBarShadow(MenuBar bar) : this(bar, System.Windows.Forms.Screen.PrimaryScreen)
        {
            
        }

        public MenuBarShadow(MenuBar bar, System.Windows.Forms.Screen screen)
        {
            Screen = screen;
            MenuBar = bar;

            InitializeComponent();

            SetPosition();
        }

        public void SetPosition()
        {
            if (MenuBar != null)
            {
                this.Width = MenuBar.ActualWidth;
                this.Top = MenuBar.Top + MenuBar.ActualHeight;
                this.Left = MenuBar.Left;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            int HTTRANSPARENT = (-1);
            if (msg == NativeMethods.WM_NCHITTEST)
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
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SetPosition();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            if (!Startup.IsShuttingDown && !Startup.IsSettingScreens)
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }
    }
}
