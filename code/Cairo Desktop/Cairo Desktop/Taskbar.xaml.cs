using System;
using System.Windows;
using System.Windows.Markup;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;

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
            // Set custom theme if selected
            string theme = Properties.Settings.Default.CairoTheme;
            if (theme != "Default")
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + theme)) this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme)));

            appGrabber = AppGrabber.AppGrabber.Instance;
            this.quickLaunchList.ItemsSource = appGrabber.QuickLaunch;
            this.TaskbarBorder.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
            this.grdTaskbar.Width = AppBarHelper.PrimaryMonitorSize.Width;

            moveToBottom();
        }

        private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Manually call dispose on window close...
            (TasksList.DataContext as WindowsTasks.WindowsTasksService).Dispose();
            (TasksList2.DataContext as WindowsTasks.WindowsTasksService).Dispose();

            // show the windows taskbar again
            AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.OnTop);
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

        private void Window_Activated(object sender, EventArgs e)
        {
            moveToBottom();
        }

        private void moveToBottom()
        {
            // set to bottom of workspace
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }

            if (msg == appbarMessageId)
            {
                switch (wParam.ToInt32())
                {
                    case 1:
                        // Reposition to the bottom of the screen.
                        this.TaskbarBorder.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
                        this.grdTaskbar.Width = AppBarHelper.PrimaryMonitorSize.Width;
                        //AppBarHelper.ABSetPos(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_BOTTOM);
                        break;
                }
                handled = true;
            }

            return IntPtr.Zero;
        }
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;

        private void TaskbarWindow_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;

            // Windows bugs make this no bueno...
            //appbarMessageId = AppBarHelper.RegisterBar(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_BOTTOM);
        }
    }
}
