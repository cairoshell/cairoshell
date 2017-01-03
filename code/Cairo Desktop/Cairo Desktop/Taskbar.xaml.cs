using System;
using System.Windows;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using CairoDesktop.Interop;
using System.Windows.Threading;

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

        public AppGrabber.AppGrabber appGrabber;

        private String configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

        public Taskbar()
        {
            InitializeComponent();
            
            appGrabber = AppGrabber.AppGrabber.Instance;
            this.quickLaunchList.ItemsSource = appGrabber.QuickLaunch;
            this.TaskbarBorder.MaxWidth = SystemParameters.WorkArea.Width - 36;
            this.grdTaskbar.Width = SystemParameters.WorkArea.Width;
        }

        private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Manually call dispose on window close...
            (TasksList.DataContext as WindowsTasks.WindowsTasksService).Dispose();
            (TasksList2.DataContext as WindowsTasks.WindowsTasksService).Dispose();

            // show the windows taskbar again
            AppBarHelper.SetWinTaskbarState(NativeMethods.SWP_SHOWWINDOW);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.CairoTaskbarTaskList.IsOpen == false)
            {
                this.CairoTaskbarTaskList.IsOpen = true;
                this.CairoTaskbarTaskList.StaysOpen = false;
            }
            else
            {
                this.CairoTaskbarTaskList.IsOpen = false;
                this.CairoTaskbarTaskList.StaysOpen = false;
            }
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
            this.TaskbarBorder.MaxWidth = SystemParameters.WorkArea.Width - 36;
            this.grdTaskbar.Width = SystemParameters.WorkArea.Width;
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }

            if (msg == appbarMessageId)
            {
                switch (wParam.ToInt32())
                {
                    case 1:
                        // Reposition to the bottom of the screen.
                        setPosition();
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

            DispatcherTimer autoResize = new DispatcherTimer(new TimeSpan(0, 0, 5), DispatcherPriority.Normal, delegate
            {
                setPosition();
            }, this.Dispatcher);

            // Windows bugs make this no bueno...
            //appbarMessageId = AppBarHelper.RegisterBar(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_BOTTOM);
        }
    }
}
