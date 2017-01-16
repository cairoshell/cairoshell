using System;
using System.Windows;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using CairoDesktop.Interop;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : Window
    {
        // AppBar properties
        private WindowInteropHelper helper;
        private IntPtr handle;
        private int appbarMessageId = -1;
        private bool displayChanged = false;

        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        public Taskbar()
        {
            InitializeComponent();

            setupTaskbar();
        }

        private void setupTaskbar()
        {
            AppGrabber.Category quickLaunch = appGrabber.QuickLaunch;
            
            this.quickLaunchList.ItemsSource = quickLaunch;
            this.TaskbarBorder.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
            this.Width = AppBarHelper.PrimaryMonitorSize.Width;
        }

        private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Manually call dispose on window close...
            (TasksList.DataContext as WindowsTasks.WindowsTasksService).Dispose();

            // show the windows taskbar again
            AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.OnTop);
            AppBarHelper.SetWinTaskbarPos(NativeMethods.SWP_SHOWWINDOW);
        }

        private void setPosition()
        {
            double screen = AppBarHelper.PrimaryMonitorSize.Height;
            double workArea = SystemParameters.WorkArea.Bottom;
            
            if (screen - workArea == this.Height)
            {
                this.Top = screen - this.Height;
            }
            else
            {
                // set to bottom of workspace
                this.Top = screen - this.Height;
            }
            this.Left = 0;
            this.TaskbarBorder.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
            this.Width = AppBarHelper.PrimaryMonitorSize.Width;
        }

        private void setPosition(uint x, uint y)
        {
            displayChanged = true;
            int sWidth;
            int sHeight;
            // adjust size for dpi
            AppBarHelper.TransformFromPixels(x, y, out sWidth, out sHeight);
            
            this.Top = sHeight - this.Height;
            this.Left = 0;
            this.TaskbarBorder.MaxWidth =sWidth - 36;
            this.Width = sWidth;
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }

            if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            if (msg == appbarMessageId)
            {
                switch (wParam.ToInt32())
                {
                    case 1:
                        // Reposition to the bottom of the screen.
                        //AppBarHelper.ABSetPos(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_BOTTOM);
                        break;
                }
                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }

        private void TaskbarWindow_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;

            setPosition();

            // Windows bugs make this no bueno...
            //appbarMessageId = AppBarHelper.RegisterBar(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_BOTTOM);
        }

        private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        {
            WindowsTasks.ApplicationWindow window = e.Item as WindowsTasks.ApplicationWindow;

            if (window.ShowInTaskbar)
                e.Accepted = true;
            else
                e.Accepted = false;
        }

        private void TaskbarWindow_LocationChanged(object sender, EventArgs e)
        {
            // this variable is set when the display size is changed, since that event handles this function. if we run here too, wrong position is set
            if (!displayChanged)
                setPosition();
            else
                displayChanged = false;
        }
    }
}
