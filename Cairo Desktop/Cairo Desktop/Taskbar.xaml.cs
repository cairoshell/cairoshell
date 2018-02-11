using System;
using System.Windows;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using CairoDesktop.Interop;
using CairoDesktop.Configuration;
using System.Windows.Threading;
using System.Collections.Specialized;
using CairoDesktop.WindowsTray;
using System.Windows.Controls.Primitives;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : Window
    {
        // AppBar properties
        private WindowInteropHelper helper;
        public IntPtr handle;
        private int appbarMessageId = -1;
        private bool displayChanged = false;
        private AppBarHelper.ABEdge appBarEdge = AppBarHelper.ABEdge.ABE_BOTTOM;
        private int addToSize = 0;

        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        public DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(Taskbar), new PropertyMetadata(new double()));
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        public DependencyProperty ButtonTextWidthProperty = DependencyProperty.Register("ButtonTextWidth", typeof(double), typeof(Taskbar), new PropertyMetadata(new double()));
        public double ButtonTextWidth
        {
            get { return (double)GetValue(ButtonTextWidthProperty); }
            set { SetValue(ButtonTextWidthProperty, value); }
        }

        public Taskbar()
        {
            InitializeComponent();

            setupTaskbar();
        }

        private void setupTaskbar()
        {
            this.DataContext = WindowsTasks.WindowsTasksService.Instance;
            AppGrabber.Category quickLaunch = appGrabber.QuickLaunch;
            
            this.quickLaunchList.ItemsSource = quickLaunch;
            this.bdrTaskbar.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
            this.Width = AppBarHelper.PrimaryMonitorSize.Width;

            switch (Settings.TaskbarIconSize)
            {
                case 0:
                    addToSize = 16;
                    break;
                case 10:
                    addToSize = 8;
                    break;
                default:
                    addToSize = 0;
                    break;
            }

            this.Height = 29 + addToSize;

            ((INotifyCollectionChanged)TasksList.Items).CollectionChanged += TasksList_Changed;
            btnDesktopOverlay.DataContext = Startup.DesktopWindow;

            // set taskbar edge based on preference
            if (Settings.TaskbarPosition == 1)
            {
                this.Top = Startup.MenuBarWindow.Height;
                appBarEdge = AppBarHelper.ABEdge.ABE_TOP;
                bdrTaskbar.Style = Application.Current.FindResource("CairoTaskbarTopBorderStyle") as Style;
                bdrTaskbarEnd.Style = Application.Current.FindResource("CairoTaskbarEndTopBorderStyle") as Style;
                bdrTaskbarLeft.Style = Application.Current.FindResource("CairoTaskbarLeftTopBorderStyle") as Style;
                btnTaskList.Style = Application.Current.FindResource("CairoTaskbarTopButtonList") as Style;
                btnDesktopOverlay.Style = Application.Current.FindResource("CairoTaskbarTopButtonDesktopOverlay") as Style;
                TaskbarGroupStyle.ContainerStyle = Application.Current.FindResource("CairoTaskbarTopGroupStyle") as Style;
                TasksList.Margin = new Thickness(0);
                bdrTaskListPopup.Margin = new Thickness(5, this.Top + this.Height - 1, 5, 11);
            }
            else
            {
                int screen = AppBarHelper.PrimaryMonitorSize.Height;
                bdrTaskListPopup.Margin = new Thickness(5, 0, 5, this.Height - 1);
                setTopPosition(screen, true);
            }

            // hide desktop overlay button if desktop is not enabled
            if (!Settings.EnableDesktop)
            {
                btnDesktopOverlay.Visibility = Visibility.Collapsed;
            }

            // show task view on windows >= 10, adjust margin if not shown
            if (Shell.IsWindows10OrBetter && !Startup.IsCairoUserShell)
                bdrTaskView.Visibility = Visibility.Visible;
            else
                TasksList2.Margin = new Thickness(0, -3, 0, -3);
        }

        private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Startup.IsShuttingDown)
            {
                // Manually call dispose on window close...
                (this.DataContext as WindowsTasks.WindowsTasksService).Dispose();

                // dispose system tray if it's still running to prevent conflicts when doing AppBar stuff
                NotificationArea.Instance.Dispose();

                if (AppBarHelper.appBars.Contains(this.handle))
                    AppBarHelper.RegisterBar(this, this.ActualWidth, this.ActualHeight);

                // show the windows taskbar again
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.OnTop);
                AppBarHelper.SetWinTaskbarPos((int)NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW);
            }
            else
                e.Cancel = true;
        }

        private void setTaskButtonSize()
        {
            double size = Math.Floor((ActualWidth - quickLaunchList.ActualWidth - bdrTaskbarEnd.ActualWidth - (TasksList.Items.Groups.Count * (5 * Shell.DpiScale)) - 14) / TasksList.Items.Count);
            if (size > (140 + addToSize))
                ButtonWidth = 140 + addToSize;
            else
                ButtonWidth = size;
            ButtonTextWidth = ButtonWidth - 33 - addToSize;
        }

        private void setPosition()
        {
            int screen = AppBarHelper.PrimaryMonitorSize.Height;

            setTopPosition(screen);

            this.Left = 0;
            this.bdrTaskbar.MaxWidth = AppBarHelper.PrimaryMonitorSize.Width - 36;
            this.Width = AppBarHelper.PrimaryMonitorSize.Width;
        }

        private void setPosition(uint x, uint y)
        {
            displayChanged = true;
            int sWidth;
            int sHeight;
            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out sWidth, out sHeight);
            
            setTopPosition(sHeight);

            this.Left = 0;
            this.bdrTaskbar.MaxWidth = sWidth - 36;
            this.Width = sWidth;
        }

        private void setTopPosition(int top, bool force = false)
        {
            if (Startup.IsCairoUserShell || Settings.TaskbarMode > 0 || this.Top < Startup.MenuBarWindow.Height || force)
            {
                if (Settings.TaskbarPosition == 1)
                {
                    double workArea = SystemParameters.WorkArea.Top / Shell.DpiScaleAdjustment;

                    // set to top of workspace
                    if (workArea >= this.Height + Startup.MenuBarWindow.Height)
                        this.Top = workArea - this.Height;
                    else if (workArea == 0)
                        this.Top = Startup.MenuBarWindow.Height;
                    else
                        this.Top = workArea;
                }
                else
                {
                    // set to bottom of workspace
                    this.Top = top - this.Height;
                }
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            
            if (msg == appbarMessageId && appbarMessageId != -1 && Settings.TaskbarMode == 0)
            {
                switch ((NativeMethods.AppBarNotifications)wParam.ToInt32())
                {
                    case NativeMethods.AppBarNotifications.PosChanged:
                        // Reposition to the top of the screen.
                        AppBarHelper.ABSetPos(this, this.ActualWidth, this.ActualHeight, appBarEdge);
                        break;

                    case NativeMethods.AppBarNotifications.FullScreenApp:
                        if ((int)lParam == 1)
                        {
                            this.Topmost = false;
                            Shell.ShowWindowBottomMost(this.handle);
                        }
                        else
                        {
                            this.Topmost = true;
                            Shell.ShowWindowTopMost(this.handle);
                        }

                        break;

                    case NativeMethods.AppBarNotifications.WindowArrange:
                        if ((int)lParam != 0)    // before
                            this.Visibility = Visibility.Collapsed;
                        else                         // after
                            this.Visibility = Visibility.Visible;

                        break;
                }
                handled = true;
            }
            else if (msg == NativeMethods.WM_ACTIVATE && Settings.TaskbarMode == 0)
            {
                AppBarHelper.AppBarActivate(hwnd);
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGED && Settings.TaskbarMode == 0)
            {
                AppBarHelper.AppBarWindowPosChanged(hwnd);
            }
            else if (msg == NativeMethods.WM_DPICHANGED)
            {
                Shell.DpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                AppBarHelper.ABSetPos(this, this.ActualWidth, this.ActualHeight, AppBarHelper.ABEdge.ABE_TOP);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
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
            setTaskButtonSize();

            if (Settings.TaskbarMode == 0)
                appbarMessageId = AppBarHelper.RegisterBar(this, this.ActualWidth, this.ActualHeight, appBarEdge);

            Shell.HideWindowFromTasks(handle);
        }

        private void TasksList_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            setTaskButtonSize();
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
            {
                displayChanged = false;

                if (Settings.TaskbarMode > 0)
                {
                    // set position after 2 seconds anyway in case we missed something
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Start();
                    timer.Tick += (sender1, args) =>
                    {
                        setPosition();
                        timer.Stop();
                    };
                }
            }
        }

        private void TaskView_Click(object sender, RoutedEventArgs e)
        {
            Shell.ShowWindowSwitcher();
        }

        private void takeFocus()
        {
            // because we are setting WS_EX_NOACTIVATE, popups won't go away when clicked outside, since they are not losing focus (they never got it). calling this fixes that.
            NativeMethods.SetForegroundWindow(helper.Handle);
        }

        private void btnTaskList_Click(object sender, RoutedEventArgs e)
        {
            takeFocus();
        }

        private void TaskButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            takeFocus();
        }

        private void quickLaunchList_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                appGrabber.AddByPath(fileNames, 3);
            }

            e.Handled = true;
        }

        private void quickLaunchList_DragEnter(object sender, DragEventArgs e)
        {
            String[] formats = e.Data.GetFormats(true);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        private void btnDesktopOverlay_Click(object sender, RoutedEventArgs e)
        {
            Startup.DesktopWindow.IsOverlayOpen = (bool)(sender as ToggleButton).IsChecked;
        }
    }
}
