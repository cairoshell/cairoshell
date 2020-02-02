using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using CairoDesktop.WindowsTray;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : Window
    {
        #region Properties
        public System.Windows.Forms.Screen Screen;
        private double dpiScale = 1.0;

        public bool IsClosing = false;

        // Item sources
        private AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // AppBar properties
        private WindowInteropHelper helper;
        public IntPtr handle;
        private int appbarMessageId = -1;
        private bool displayChanged = false;
        private AppBarHelper.ABEdge appBarEdge = AppBarHelper.ABEdge.ABE_BOTTOM;
        private int addToSize = 0;

        public static DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(Taskbar), new PropertyMetadata(new double()));
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        public static DependencyProperty ButtonTextWidthProperty = DependencyProperty.Register("ButtonTextWidth", typeof(double), typeof(Taskbar), new PropertyMetadata(new double()));
        public double ButtonTextWidth
        {
            get { return (double)GetValue(ButtonTextWidthProperty); }
            set { SetValue(ButtonTextWidthProperty, value); }
        }
        #endregion

        public Taskbar() : this(System.Windows.Forms.Screen.PrimaryScreen)
        {

        }

        public Taskbar(System.Windows.Forms.Screen screen)
        {
            Screen = screen;

            InitializeComponent();

            setupTaskbar();
            setupTaskbarAppearance();
        }

        #region Startup and shutdown
        private void setupTaskbar()
        {
            // setup taskbar item source
            TasksList.ItemsSource = WindowsTasks.WindowsTasksService.Instance.GroupedWindows;
            TasksList2.ItemsSource = WindowsTasks.WindowsTasksService.Instance.GroupedWindows;
            WindowsTasks.WindowsTasksService.Instance.GroupedWindows.CollectionChanged += GroupedWindows_Changed;

            // setup data contexts
            bdrMain.DataContext = Settings.Instance;
            quickLaunchList.ItemsSource = appGrabber.QuickLaunch;

            if (Startup.DesktopWindow != null)
            {
                btnDesktopOverlay.DataContext = Startup.DesktopWindow;
                bdrBackground.DataContext = Startup.DesktopWindow;
            }
            else
            {
                btnDesktopOverlay.Visibility = Visibility.Collapsed;
                btnDesktopOverlay.DataContext = null;
                bdrBackground.DataContext = null;
            }
        }

        private void setupTaskbarAppearance()
        {
            double screenWidth = screenWidth = Screen.Bounds.Width / dpiScale;
            Left = Screen.Bounds.Left / dpiScale;
            bdrTaskbar.MaxWidth = screenWidth - 36;
            Width = screenWidth;

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

            Height = 29 + addToSize;

            // set taskbar edge based on preference
            if (Settings.TaskbarPosition == 1)
            {
                Top = Startup.MenuBarWindow.Height;
                appBarEdge = AppBarHelper.ABEdge.ABE_TOP;
                bdrTaskbar.Style = Application.Current.FindResource("CairoTaskbarTopBorderStyle") as Style;
                bdrTaskbarEnd.Style = Application.Current.FindResource("CairoTaskbarEndTopBorderStyle") as Style;
                bdrTaskbarLeft.Style = Application.Current.FindResource("CairoTaskbarLeftTopBorderStyle") as Style;
                btnTaskList.Style = Application.Current.FindResource("CairoTaskbarTopButtonList") as Style;
                btnDesktopOverlay.Style = Application.Current.FindResource("CairoTaskbarTopButtonDesktopOverlay") as Style;
                TaskbarGroupStyle.ContainerStyle = Application.Current.FindResource("CairoTaskbarTopGroupStyle") as Style;
                TasksList.Margin = new Thickness(0);
                bdrTaskListPopup.Margin = new Thickness(5, Top + Height - 1, 5, 11);
            }
            else
            {
                bdrTaskListPopup.Margin = new Thickness(5, 0, 5, Height - 1);
                setTopPosition(Screen.Bounds.Bottom / dpiScale);
            }

            // show task view on windows >= 10, adjust margin if not shown
            if (Shell.IsWindows10OrBetter && !Startup.IsCairoUserShell)
                bdrTaskView.Visibility = Visibility.Visible;
            else
                TasksList2.Margin = new Thickness(0, -3, 0, -3);

            if (Settings.FullWidthTaskBar)
            {
                bdrTaskbarLeft.CornerRadius = new CornerRadius(0);
                bdrTaskbarEnd.CornerRadius = new CornerRadius(0);
            }
        }

        private void Taskbar_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            if (Startup.IsShuttingDown && Screen.Primary)
            {
                // Manually call dispose on window close...
                WindowsTasks.WindowsTasksService.Instance.GroupedWindows.CollectionChanged -= GroupedWindows_Changed;
                WindowsTasks.WindowsTasksService.Instance.Dispose();

                // dispose system tray if it's still running to prevent conflicts when doing AppBar stuff
                NotificationArea.Instance.Dispose();

                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;

                if (AppBarHelper.appBars.Contains(handle))
                    AppBarHelper.RegisterBar(this, Screen, ActualWidth * dpiScale, this.ActualHeight * dpiScale);

                // show the windows taskbar again
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.OnTop);
                AppBarHelper.SetWinTaskbarPos((int)NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW);
            }
            else if (Startup.IsSettingScreens || Startup.IsShuttingDown)
            {
                WindowsTasks.WindowsTasksService.Instance.GroupedWindows.CollectionChanged -= GroupedWindows_Changed;
                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;

                if (AppBarHelper.appBars.Contains(handle))
                    AppBarHelper.RegisterBar(this, Screen, ActualWidth * dpiScale, this.ActualHeight * dpiScale);
            }
            else
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }

        private void TaskbarWindow_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;

            this.dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            setPosition();
            setTaskButtonSize();

            if (Settings.TaskbarMode == 0)
                appbarMessageId = AppBarHelper.RegisterBar(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, appBarEdge);

            Shell.HideWindowFromTasks(handle);

            FullScreenHelper.Instance.FullScreenApps.CollectionChanged += FullScreenApps_CollectionChanged;
        }
        #endregion

        #region Position and appearance
        private void setTaskButtonSize()
        {
            double size = Math.Floor((ActualWidth - quickLaunchList.ActualWidth - bdrTaskbarEnd.ActualWidth - (TasksList.Items.Groups.Count * (5 * dpiScale)) - 14) / TasksList.Items.Count);
            if (size > (140 + addToSize))
                ButtonWidth = 140 + addToSize;
            else
                ButtonWidth = size;
            ButtonTextWidth = ButtonWidth - 33 - addToSize;
        }

        public void setPosition()
        {
            double screenWidth = Screen.Bounds.Width / dpiScale;
            double screenHeight = Screen.Bounds.Height / dpiScale;

            setTopPosition(Screen.Bounds.Bottom / dpiScale);

            Left = Screen.Bounds.Left / dpiScale;

            if (Settings.FullWidthTaskBar)
                bdrTaskbar.Width = screenWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1; // account for border

            // set maxwidth always
            bdrTaskbar.MaxWidth = screenWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1;

            Width = screenWidth;
        }

        private void setPosition(uint x, uint y)
        {
            displayChanged = true;

            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out int sWidth, out int sHeight);

            setTopPosition(Screen.Bounds.Bottom / dpiScale);

            Left = Screen.Bounds.Left / dpiScale;

            double screenWidth = Screen.Bounds.Width / dpiScale;

            if (Settings.FullWidthTaskBar)
                bdrTaskbar.Width = screenWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1; // push the border off the edge

            // set maxwidth always
            bdrTaskbar.MaxWidth = sWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1;

            Width = sWidth;
        }

        private void setTopPosition(double top)
        {
            if (Settings.TaskbarPosition == 1)
            {
                // set to bottom of menu bar
                Top = (Screen.Bounds.Y / dpiScale) + Startup.MenuBarWindow.Height;
            }
            else
            {
                // set to bottom of workspace
                Top = top - Height;
            }
        }

        private void FullScreenApps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool found = false;

            foreach (FullScreenHelper.FullScreenApp app in FullScreenHelper.Instance.FullScreenApps)
            {
                if (app.screen.DeviceName == Screen.DeviceName)
                {
                    // we need to not be on top now
                    found = true;
                    break;
                }
            }

            if (found && Topmost)
            {
                setFullScreenMode(true);
            }
            else if (!found && !Topmost)
            {
                setFullScreenMode(false);
            }
        }

        private void setFullScreenMode(bool entering)
        {
            if (entering)
            {
                CairoLogger.Instance.Debug(string.Format("Taskbar on {0} conceeding to full-screen app", Screen.DeviceName));

                Topmost = false;
                Shell.ShowWindowBottomMost(handle);
            }
            else
            {
                CairoLogger.Instance.Debug(string.Format("Taskbar on {0} returning to normal state", Screen.DeviceName));

                Topmost = true;
                Shell.ShowWindowTopMost(handle);
            }
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

        private void takeFocus()
        {
            // because we are setting WS_EX_NOACTIVATE, popups won't go away when clicked outside, since they are not losing focus (they never got it). calling this fixes that.
            NativeMethods.SetForegroundWindow(helper.Handle);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }
        #endregion

        #region Window procedure
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
                        AppBarHelper.ABSetPos(this, Screen, ActualWidth * dpiScale, ActualHeight * dpiScale, appBarEdge);
                        break;

                    case NativeMethods.AppBarNotifications.FullScreenApp:
                        //SetFullScreenMode((int)lParam == 1);

                        break;

                    case NativeMethods.AppBarNotifications.WindowArrange:
                        if ((int)lParam != 0)    // before
                            Visibility = Visibility.Collapsed;
                        else                         // after
                            Visibility = Visibility.Visible;

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
                if (!(Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                if (Screen.Primary)
                    Shell.DpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;

                dpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                AppBarHelper.ABSetPos(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, appBarEdge);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                if (!(Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            return IntPtr.Zero;
        }
        #endregion

        #region Data
        private void GroupedWindows_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            setTaskButtonSize();
        }
        #endregion

        #region Button clicks
        private void TaskView_Click(object sender, RoutedEventArgs e)
        {
            Shell.ShowWindowSwitcher();
        }

        private void btnDesktopOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (Startup.DesktopWindow != null)
                Startup.DesktopWindow.IsOverlayOpen = (bool)(sender as ToggleButton).IsChecked;
        }

        private void btnTaskList_Click(object sender, RoutedEventArgs e)
        {
            takeFocus();
        }

        private void TaskButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            takeFocus();
        }
        #endregion

        #region Quick Launch
        private void quickLaunchList_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                appGrabber.AddByPath(fileNames, AppGrabber.AppCategoryType.QuickLaunch);
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
        #endregion
    }
}
