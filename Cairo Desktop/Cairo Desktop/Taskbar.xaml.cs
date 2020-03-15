using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : AppBarWindow
    {
        #region Properties
        // Item sources
        private AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // display properties
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
            InitializeComponent();

            Screen = screen;

            setupTaskbar();
            setupTaskbarAppearance();
        }

        #region Startup and shutdown
        private void setupTaskbar()
        {
            // setup app bar settings
            if (Settings.Instance.TaskbarMode != 0)
            {
                enableAppBar = false;
            }

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
            double screenWidth = Screen.Bounds.Width / dpiScale;
            Left = Screen.Bounds.Left / dpiScale;
            bdrTaskbar.MaxWidth = screenWidth - 36;
            Width = screenWidth;

            switch (Settings.Instance.TaskbarIconSize)
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
            desiredHeight = Height;

            // set taskbar edge based on preference
            if (Settings.Instance.TaskbarPosition == 1)
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
                appBarEdge = AppBarHelper.ABEdge.ABE_BOTTOM;
                bdrTaskListPopup.Margin = new Thickness(5, 0, 5, Height - 1);
                setTopPosition(Screen.Bounds.Bottom / dpiScale);
            }

            // show task view on windows >= 10, adjust margin if not shown
            if (Shell.IsWindows10OrBetter && !Startup.IsCairoRunningAsShell)
                bdrTaskView.Visibility = Visibility.Visible;
            else
                TasksList2.Margin = new Thickness(0, -3, 0, -3);

            if (Settings.Instance.FullWidthTaskBar)
            {
                bdrTaskbarLeft.CornerRadius = new CornerRadius(0);
                bdrTaskbarEnd.CornerRadius = new CornerRadius(0);
            }
        }

        protected override void customClosing()
        {
            if (Startup.IsShuttingDown && Screen.Primary)
            {
                // Manually call dispose on window close...
                WindowsTasks.WindowsTasksService.Instance.GroupedWindows.CollectionChanged -= GroupedWindows_Changed;
                WindowsTasks.WindowsTasksService.Instance.Dispose();

                // show the windows taskbar again
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.OnTop);
                AppBarHelper.SetWinTaskbarPos((int)NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW);
            }
            else if (Startup.IsSettingScreens || Startup.IsShuttingDown)
            {
                WindowsTasks.WindowsTasksService.Instance.GroupedWindows.CollectionChanged -= GroupedWindows_Changed;
            }
        }

        protected override void postInit()
        {
            setTaskButtonSize();
        }

        private void TaskbarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
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

        internal override void setPosition()
        {
            double screenWidth = Screen.Bounds.Width / dpiScale;

            Height = desiredHeight;

            setTopPosition(Screen.Bounds.Bottom / dpiScale);

            Left = Screen.Bounds.Left / dpiScale;

            if (Settings.Instance.FullWidthTaskBar)
                bdrTaskbar.Width = screenWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1; // account for border

            // set maxwidth always
            bdrTaskbar.MaxWidth = screenWidth - btnDesktopOverlay.Width - btnTaskList.Width + 1;

            Width = screenWidth;
        }

        private void setTopPosition(double top)
        {
            if (Settings.Instance.TaskbarPosition == 1)
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

        private void takeFocus()
        {
            // because we are setting WS_EX_NOACTIVATE, popups won't go away when clicked outside, since they are not losing focus (they never got it). calling this fixes that.
            NativeMethods.SetForegroundWindow(Handle);
        }
        #endregion

        #region Window procedure
        protected override IntPtr customWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
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
