using CairoDesktop.Configuration;
using ManagedShell.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Services;
using ManagedShell;
using ManagedShell.AppBar;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : CairoAppBarWindow
    {
        #region Properties
        // Item sources
        internal readonly AppGrabberService _appGrabber;
        private readonly ShellManager _shellManager;

        // display properties
        private int addToSize;
        private int baseButtonWidth;
        private bool isCondensed;

        private bool useFullWidthAppearance => Settings.Instance.FullWidthTaskBar || isCondensed;

        public static DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(Taskbar), new PropertyMetadata(new double()));
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        public static DependencyProperty CanAutoHideProperty = DependencyProperty.Register("CanAutoHide", typeof(bool), typeof(Taskbar), new PropertyMetadata(new bool()));
        private DesktopManager _desktopManager;

        public bool CanAutoHide
        {
            get { return (bool)GetValue(CanAutoHideProperty); }
            set { SetValue(CanAutoHideProperty, value); }
        }
        #endregion
        
        public Taskbar(ICairoApplication cairoApplication, ShellManager shellManager, WindowManager windowManager, DesktopManager desktopManager, AppGrabberService appGrabber, AppBarScreen screen, AppBarEdge edge) : base(cairoApplication, shellManager, windowManager, screen, edge, 0)
        {
            InitializeComponent();
            _appGrabber = appGrabber;
            _desktopManager = desktopManager;
            _shellManager = shellManager;

            if (!Screen.Primary && !Settings.Instance.EnableMenuBarMultiMon)
            {
                ProcessScreenChanges = true;
            }
            else
            {
                ProcessScreenChanges = false;
            }

            setupTaskbar();
            setupTaskbarAppearance();
        }

        #region Startup and shutdown
        private void setupTaskbar()
        {
            // setup app bar settings
            if (Settings.Instance.TaskbarMode != 0)
            {
                EnableAppBar = false;
            }
            CanAutoHide = true;

            // setup taskbar item source
            _shellManager.Tasks.Initialize(new TaskCategoryProvider(_appGrabber));

            TasksList.ItemsSource = _shellManager.Tasks.GroupedWindows;
            TasksList2.ItemsSource = _shellManager.Tasks.GroupedWindows;
            if (_shellManager.Tasks.GroupedWindows != null) _shellManager.Tasks.GroupedWindows.CollectionChanged += GroupedWindows_Changed;

            // setup data contexts
            bdrMain.DataContext = Settings.Instance;
            quickLaunchList.ItemsSource = _appGrabber.QuickLaunch;

            setTaskbarDesktopOverlayButton();

            // register for settings changes
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void setupTaskbarAppearance()
        {
            double screenWidth = Screen.Bounds.Width / DpiScale;
            Left = Screen.Bounds.Left / DpiScale;
            bdrTaskbar.MaxWidth = screenWidth;
            Width = screenWidth;

            // set taskbar edge based on preference
            if (Settings.Instance.TaskbarPosition == 1)
            {
                AppBarEdge = AppBarEdge.Top;
                TaskbarGroupStyle.ContainerStyle = FindResource("CairoTaskbarTopGroupStyle") as Style;
                TasksList.Margin = new Thickness(0);
            }
            else
            {
                AppBarEdge = AppBarEdge.Bottom;
                TaskbarGroupStyle.ContainerStyle = FindResource("CairoTaskbarGroupStyle") as Style;
                TasksList.Margin = new Thickness(-3, -1, 0, 0);
            }

            // show task view on windows >= 10, adjust margin if not shown
            if (EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
                bdrTaskView.Visibility = Visibility.Visible;
            else
                TasksList2.Margin = new Thickness(0, -3, 0, -3);

            setTaskbarSize();
            setTaskbarWidthMode();
        }

        private void setTaskbarDesktopOverlayButton()
        {
            if (DesktopManager.IsEnabled)
            {
                btnDesktopOverlay.Visibility = Visibility.Visible;
                btnDesktopOverlay.DataContext = _desktopManager;
                bdrBackground.DataContext = _desktopManager;
                bdrTaskbar.Padding = new Thickness(0);
            }
            else
            {
                btnDesktopOverlay.Visibility = Visibility.Collapsed;
                btnDesktopOverlay.DataContext = null;
                bdrBackground.DataContext = null;
                bdrTaskbar.Padding = new Thickness(5, 0, 0, 0);
            }
        }

        private void setTaskbarSize()
        {
            switch ((IconSize)Settings.Instance.TaskbarIconSize)
            {
                case IconSize.Large:
                    addToSize = 16;
                    break;
                case IconSize.Medium:
                    addToSize = 8;
                    break;
                default:
                    addToSize = 0;
                    break;
            }

            baseButtonWidth = (Settings.Instance.ShowTaskbarLabels ? Settings.Instance.TaskbarButtonWidth : Settings.Instance.TaskbarButtonHeight) + addToSize;
            Height = Settings.Instance.TaskbarButtonHeight + addToSize;
            DesiredHeight = Height;
            Top = getDesiredTopPosition();

            if (Settings.Instance.TaskbarPosition == 1)
                bdrTaskListPopup.Margin = new Thickness(5, Top + Height - 1, 5, 11);
            else
                bdrTaskListPopup.Margin = new Thickness(5, 0, 5, (Screen.Bounds.Bottom / DpiScale) - Top - 1);
        }

        private void setTaskbarWidthMode()
        {
            if (useFullWidthAppearance)
            {
                bdrTaskbar.Width = getDesiredWidth();

                if (Settings.Instance.TaskbarPosition == 1)
                {
                    bdrTaskbar.Style = FindResource("CairoTaskbarTopFullBorderStyle") as Style;
                    btnDesktopOverlay.Style = FindResource("CairoTaskbarTopFullButtonDesktopOverlay") as Style;
                    btnTaskList.Style = FindResource("CairoTaskbarTopFullButtonList") as Style;
                }
                else
                {
                    bdrTaskbar.Style = FindResource("CairoTaskbarFullBorderStyle") as Style;
                    btnDesktopOverlay.Style = FindResource("CairoTaskbarFullButtonDesktopOverlay") as Style;
                    btnTaskList.Style = FindResource("CairoTaskbarFullButtonList") as Style;
                }
            }
            else
            {
                bdrTaskbar.Width = double.NaN;

                if (Settings.Instance.TaskbarPosition == 1)
                {
                    bdrTaskbar.Style = FindResource("CairoTaskbarTopBorderStyle") as Style;
                    btnDesktopOverlay.Style = FindResource("CairoTaskbarTopButtonDesktopOverlay") as Style;
                    btnTaskList.Style = FindResource("CairoTaskbarTopButtonList") as Style;
                }
                else
                {
                    bdrTaskbar.Style = FindResource("CairoTaskbarBorderStyle") as Style;
                    btnDesktopOverlay.Style = FindResource("CairoTaskbarButtonDesktopOverlay") as Style;
                    btnTaskList.Style = FindResource("CairoTaskbarButtonList") as Style;
                }
            }

            setTaskbarBlur();
        }

        private void SetDesktopPosition()
        {
            // if we are showing but not reserving space, tell the desktop to adjust here
            // since we aren't changing the work area, it doesn't do this on its own
            if (Settings.Instance.TaskbarMode == 1 && Screen.Primary)
                _desktopManager.ResetPosition(false);
        }

        protected override void CustomClosing()
        {
            if (_windowManager.IsSettingDisplays || AllowClose)
            {
                _shellManager.Tasks.GroupedWindows.CollectionChanged -= GroupedWindows_Changed;
            }
        }

        protected override void OnSourceInitialized(object sender, EventArgs e)
        {
            base.OnSourceInitialized(sender, e);
            
            setTaskButtonSize();

            SetDesktopPosition();
        }

        private void TaskbarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_NOACTIVATE);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) 
                return;

            switch (e.PropertyName)
            {
                case "ShowTaskbarLabels":
                case "TaskbarIconSize":
                    setTaskbarSize();
                    SetScreenPosition();
                    if (EnvironmentHelper.IsAppRunningAsShell) _appBarManager.SetWorkArea(Screen);
                    break;
                case "TaskbarMode":
                    if (Settings.Instance.TaskbarMode == 0)
                    {
                        EnableAppBar = true;
                        RegisterAppBar();
                    }
                    else
                    {
                        EnableAppBar = false;
                        UnregisterAppBar();
                    }
                    if (EnvironmentHelper.IsAppRunningAsShell) _appBarManager.SetWorkArea(Screen);
                    SetDesktopPosition();
                    setTaskbarBlur();
                    break;
                case "TaskbarPosition":
                    setupTaskbarAppearance();
                    SetScreenPosition();
                    if (Settings.Instance.TaskbarMode == 1) SetDesktopPosition();
                    if (EnvironmentHelper.IsAppRunningAsShell) _appBarManager.SetWorkArea(Screen);
                    break;
                case "FullWidthTaskBar":
                    setTaskbarWidthMode();
                    break;
                case "EnableDesktop":
                    setTaskbarDesktopOverlayButton();
                    break;
                case "EnableMenuBarBlur":
                    setTaskbarBlur();
                    break;
            }
        }
        #endregion

        #region Position and appearance
        private void setTaskbarBlur()
        {
            if (Settings.Instance.EnableMenuBarBlur && useFullWidthAppearance && Settings.Instance.TaskbarMode != 2)
            {
                SetBlur(true);
            }
            else
            {
                SetBlur(false);
            }
        }

        private void setTaskButtonSize()
        {
            if (TasksList.Items.Groups != null)
            {
                // calculate the maximum per-button size
                double adjustedSize = Math.Floor((ActualWidth - quickLaunchList.ActualWidth - (btnDesktopOverlay.ActualWidth - 5) - btnTaskList.ActualWidth - (TasksList.Items.Groups.Count * 4 - 3) - 11) / TasksList.Items.Count);

                if (adjustedSize > baseButtonWidth)
                {
                    ButtonWidth = baseButtonWidth;

                    if (isCondensed)
                    {
                        // set back to non-condensed mode if appropriate
                        isCondensed = false;
                        setTaskbarWidthMode();
                    }
                }
                else
                {
                    ButtonWidth = adjustedSize;

                    if (!isCondensed)
                    {
                        // use condensed appearance if not already
                        isCondensed = true;
                        setTaskbarWidthMode();
                    }
                }
            }
        }

        public override void SetPosition()
        {
            base.SetPosition();

            setTaskButtonSize();
            setTaskbarWidthMode();

            // set maxwidth always
            bdrTaskbar.MaxWidth = getDesiredWidth();
        }

        private double getDesiredTopPosition()
        {
            if (Settings.Instance.TaskbarPosition == 1)
            {
                // set to bottom of this display's menu bar
                return (Screen.Bounds.Y / DpiScale) + _shellManager.AppBarManager.GetAppBarEdgeWindowsHeight(AppBarEdge, Screen);
            }
            else
            {
                // set to bottom of workspace
                return (Screen.Bounds.Bottom / DpiScale) - Height - _shellManager.AppBarManager.GetAppBarEdgeWindowsHeight(AppBarEdge, Screen);
            }
        }

        private double getDesiredWidth()
        {
            return Screen.Bounds.Width / DpiScale;
        }

        private void takeFocus()
        {
            // because we are setting WS_EX_NOACTIVATE, popups won't go away when clicked outside, since they are not losing focus (they never got it). calling this fixes that.
            NativeMethods.SetForegroundWindow(Handle);
        }

        private void TaskbarWindow_LocationChanged(object sender, EventArgs e)
        {
            // primarily for win7/8, they will set up the appbar correctly but then put it in the wrong place
            double desiredTop = getDesiredTopPosition();

            if (Top != desiredTop)
            {
                Top = desiredTop;
            }
        }

        public override void AfterAppBarPos(bool isSameCoords, NativeMethods.Rect rect)
        {
            base.AfterAppBarPos(isSameCoords, rect);

            if (useFullWidthAppearance)
                bdrTaskbar.Width = getDesiredWidth();

            // set maxwidth always
            bdrTaskbar.MaxWidth = getDesiredWidth();

            // set button size since available space may have changed
            setTaskButtonSize();
        }

        private void CairoTaskbarTaskList_Closed(object sender, EventArgs e)
        {
            CanAutoHide = true;
        }

        private void SetTaskListOffset()
        {
            if (ShellHelper.GetMenuDropAlignment() == 1)
            {
                CairoTaskbarTaskList.HorizontalOffset = 26;
            }
            else
            {
                CairoTaskbarTaskList.HorizontalOffset = -426;
            }
        }
        #endregion

        #region Window procedure
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            base.WndProc(hwnd, msg, wParam, lParam, ref handled);
            
            if (msg == (int)NativeMethods.WM.MOUSEACTIVATE)
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
            ShellHelper.ShowWindowSwitcher();
        }

        private void btnDesktopOverlay_Click(object sender, RoutedEventArgs e)
        {
            _desktopManager.IsOverlayOpen = (bool)(sender as ToggleButton).IsChecked;
        }

        private void btnTaskList_Click(object sender, RoutedEventArgs e)
        {
            SetTaskListOffset();
            takeFocus();

            CanAutoHide = false;
        }

        private void TaskButton_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            takeFocus();

            CanAutoHide = false;
        }
        #endregion

        #region Quick Launch
        private void quickLaunchList_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                _appGrabber.AddByPath(fileNames, AppGrabber.AppCategoryType.QuickLaunch);
            }

            CanAutoHide = true;

            e.Handled = true;
        }

        private void quickLaunchList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            CanAutoHide = false;

            e.Handled = true;
        }
        #endregion

        #region Taskbar context menu items
        private void grdTaskbar_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            takeFocus();
            CanAutoHide = false;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            CanAutoHide = true;
        }

        private void OpenRunWindow(object sender, RoutedEventArgs e)
        {
            ShellHelper.ShowRunDialog(Localization.DisplayString.sRun_Title, Localization.DisplayString.sRun_Info);
        }

        private void OpenTaskManager(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartTaskManager();
        }
        #endregion
    }
}
