using CairoDesktop.Common;
using ManagedShell.Interop;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.WindowsTasks;
using System.Collections.ObjectModel;
using CairoDesktop.Taskbar.SupportingClasses;

namespace CairoDesktop.Taskbar
{
    public partial class TaskButton
    {
        public static readonly DependencyProperty ListModeProperty = DependencyProperty.Register("ListMode", typeof(bool), typeof(TaskButton), new PropertyMetadata(new bool()));
        public static DependencyProperty ParentTaskbarProperty = DependencyProperty.Register("ParentTaskbar", typeof(Taskbar), typeof(TaskButton));

        public bool ListMode
        {
            get { return (bool)GetValue(ListModeProperty); }
            set { SetValue(ListModeProperty, value); }
        }

        public Taskbar ParentTaskbar
        {
            get { return (Taskbar)GetValue(ParentTaskbarProperty); }
            set { SetValue(ParentTaskbarProperty, value); }
        }

        public ReadOnlyObservableCollection<object> WindowGroup { get; set; }

        private DispatcherTimer dragTimer;
        private DispatcherTimer thumbTimer;
        public TaskThumbWindow ThumbWindow;

        private bool _isGroup => WindowGroup != null && WindowGroup.Count > 1;

        public TaskButton()
        {
            InitializeComponent();

            // register for settings changes
            Settings.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private ApplicationWindow getWindow()
        {
            if (WindowGroup == null || WindowGroup.Count < 1)
            {
                return null;
            }

            return WindowGroup[0] as ApplicationWindow;
        }

        public void SelectWindow()
        {
            if (_isGroup)
            {
                openThumb();
                return;
            }

            if (getWindow() is ApplicationWindow window)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) ||
                    Keyboard.IsKeyDown(Key.RightShift))
                {
                    ShellHelper.StartProcess(window.IsUWP ? "appx:" + window.AppUserModelID : window.WinFileName);
                    return;
                }

                if (window.State == ApplicationWindow.WindowState.Active)
                {
                    window.Minimize();
                }
                else
                {
                    window.BringToFront();
                }
            }

            closeThumb(true);
        }

        public void SetParentPopupOpen(bool isPopupOpen)
        {
            if (!ListMode && ParentTaskbar != null) ParentTaskbar.IsPopupOpen = isPopupOpen;
        }

        private void closeWindows()
        {
            if (WindowGroup != null)
            {
                foreach (ApplicationWindow window in WindowGroup)
                {
                    window.Close();
                }
            }
        }

        private TimeSpan getDelayInterval()
        {
            int autoHideDelay = Settings.Instance.TaskbarMode == 2 ? Settings.Instance.AutoHideShowDelayMs : 0;
            return SystemParameters.MouseHoverTime.Add(TimeSpan.FromMilliseconds(autoHideDelay));
        }

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TaskGroup group)
            {
                WindowGroup = group.Windows;
                group.PropertyChanged += Data_PropertyChanged;
            }
            else if (DataContext is ApplicationWindow window)
            {
                WindowGroup = new ReadOnlyObservableCollection<object>(new ObservableCollection<object>() { window });
                window.PropertyChanged += Data_PropertyChanged;
            }

            if (!ListMode)
            {
                setLabelVisibility();
                setIconSize();
                setToolTip();
            }
            else
            {
                // Task list display changes
                btn.Style = FindResource("CairoTaskListButtonStyle") as Style;
                pbProgress.Style = FindResource("TaskListProgressBar") as Style;
                imgIcon.Style = FindResource("TaskListIcon") as Style;
                WinTitle.Style = FindResource("TaskListTitle") as Style;
                ToolTipService.SetPlacement(btn, System.Windows.Controls.Primitives.PlacementMode.Right);
            }

            TimeSpan interval = getDelayInterval();
            // drag support - delayed activation using system setting
            dragTimer = new DispatcherTimer { Interval = interval };
            dragTimer.Tick += dragTimer_Tick;

            // thumbnails - delayed activation using system setting
            thumbTimer = new DispatcherTimer { Interval = interval };
            thumbTimer.Tick += thumbTimer_Tick;
        }

        private void TaskButton_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TaskGroup group)
            {
                group.PropertyChanged -= Data_PropertyChanged;
            }
            else if (DataContext is ApplicationWindow window)
            {
                window.PropertyChanged -= Data_PropertyChanged;
            }

            Settings.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "EnableTaskbarThumbnails":
                        setToolTip();
                        break;
                    case "TaskbarIconSize":
                    case "ShowTaskbarLabels":
                        setLabelVisibility();
                        setIconSize();
                        break;
                    case "ShowTaskbarBadges":
                        setIconBadges();
                        break;
                    case "AutoHideShowDelayMs":
                        TimeSpan interval = getDelayInterval();
                        dragTimer.Interval = interval;
                        thumbTimer.Interval = interval;
                        break;
                }
            }
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // handle progress changes
                case "ProgressState":
                    if (sender is TaskGroup group)
                    {
                        pbProgress.IsIndeterminate = group.ProgressState == NativeMethods.TBPFLAG.TBPF_INDETERMINATE;
                    }
                    else if (sender is ApplicationWindow window)
                    {
                        pbProgress.IsIndeterminate = window.ProgressState == NativeMethods.TBPFLAG.TBPF_INDETERMINATE;
                    }
                    break;
            }
        }

        private void setToolTip()
        {
            // list mode always shows tooltips; button mode only shows when thumbnails are disabled
            if (!ListMode)
            {
                ToolTipService.SetIsEnabled(btn, !Settings.Instance.EnableTaskbarThumbnails);
            }
        }

        private void setLabelVisibility()
        {
            if (!ListMode)
            {
                WinTitle.Visibility = Settings.Instance.ShowTaskbarLabels? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void setIconSize()
        {
            if (!ListMode)
            {
                int size = IconHelper.GetSize(Settings.Instance.TaskbarIconSize);

                imgIcon.Width = size;
                imgIcon.Height = size;

                setIconBadges();
            }
        }

        private void setIconBadges()
        {
            if (ListMode || (IconSize)Settings.Instance.TaskbarIconSize == IconSize.Small || !Settings.Instance.ShowTaskbarBadges)
            {
                overlayIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                overlayIcon.Visibility = Visibility.Visible;
            }
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            SelectWindow();
        }

        private void btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                switch (Settings.Instance.TaskbarMiddleClick)
                {
                    case 0 when Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift):
                        closeWindows();
                        break;
                    case 1 when !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift):
                        closeWindows();
                        break;
                    default:
                        if (getWindow() is ApplicationWindow window)
                        {
                            ShellHelper.StartProcess(window.IsUWP ? "appx:" + window.AppUserModelID : window.WinFileName);
                        }
                        break;
                }
            }
        }

        private void btn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!ListMode)
                thumbTimer.Start();
        }

        private void btn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!ListMode)
            {
                thumbTimer.Stop();
                closeThumb();
            }
        }
        #endregion

        #region Context menu

        private void ContextMenu_Opening(object sender, RoutedEventArgs e)
        {
            ApplicationWindow window = getWindow();

            if (window == null)
            {
                return;
            }

            Visibility pinVisibility = Visibility.Collapsed;
            Visibility singleWindowVisibility = _isGroup ? Visibility.Collapsed : Visibility.Visible;
            NativeMethods.WindowShowStyle wss = window.ShowStyle;
            int ws = window.WindowStyles;

            // show pin option if this app is not yet in quick launch
            if (ParentTaskbar._appGrabber.QuickLaunchManager.GetQuickLaunchApplicationInfo(window) == null)
            {
                pinVisibility = Visibility.Visible;
            }

            miPin.Visibility = pinVisibility;
            miPinSeparator.Visibility = pinVisibility;

            // show/hide single window controls
            miMaximize.Visibility = singleWindowVisibility;
            miMinimize.Visibility = singleWindowVisibility;
            miRestore.Visibility = singleWindowVisibility;
            miMove.Visibility = singleWindowVisibility;
            miSize.Visibility = singleWindowVisibility;
            miSingleWindowSeparator.Visibility = singleWindowVisibility;

            if (!_isGroup)
            {
                // disable window operations depending on current window state. originally tried implementing via bindings but found there is no notification we get regarding maximized state
                miMaximize.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowMaximized && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0;
                miMinimize.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowMinimized && (ws & (int)NativeMethods.WindowStyles.WS_MINIMIZEBOX) != 0;
                miRestore.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowNormal;
                miMove.IsEnabled = wss == NativeMethods.WindowShowStyle.ShowNormal;
                miSize.IsEnabled = wss == NativeMethods.WindowShowStyle.ShowNormal && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0;
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (!IsMouseOver) closeThumb(true);
        }

        private void miRestore_Click(object sender, RoutedEventArgs e)
        {
            getWindow()?.Restore();
        }

        private void miMove_Click(object sender, RoutedEventArgs e)
        {
            getWindow()?.Move();
        }

        private void miSize_Click(object sender, RoutedEventArgs e)
        {
            getWindow()?.Size();
        }

        private void miMinimize_Click(object sender, RoutedEventArgs e)
        {
            getWindow()?.Minimize();
        }

        private void miMaximize_Click(object sender, RoutedEventArgs e)
        {
            getWindow()?.Maximize();
        }

        private void miNewWindow_Click(object sender, RoutedEventArgs e)
        {
            ApplicationWindow toOpen = getWindow();

            if (toOpen != null)
            {
                ShellHelper.StartProcess(toOpen.IsUWP ? "appx:" + toOpen.AppUserModelID : toOpen.WinFileName);
            }
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            closeWindows();
        }

        private void miPin_Click(object sender, RoutedEventArgs e)
        {
            ApplicationWindow toPin = getWindow();

            if (toPin != null)
            {
                ParentTaskbar._appGrabber.QuickLaunchManager.AddToQuickLaunch(toPin.IsUWP, toPin.IsUWP ? toPin.AppUserModelID : toPin.WinFileName);
            }
        }

        private void miTaskMan_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartTaskManager();
        }

        #endregion

        #region Drag support
        private bool inDrag = false;

        private void dragTimer_Tick(object sender, EventArgs e)
        {
            if (inDrag && getWindow() is ApplicationWindow window)
            {
                window.BringToFront();
            }

            dragTimer.Stop();
        }

        private void btn_DragEnter(object sender, DragEventArgs e)
        {
            if (!inDrag)
            {
                inDrag = true;
                dragTimer.Start();
            }
        }

        private void btn_DragLeave(object sender, DragEventArgs e)
        {
            if (inDrag)
            {
                dragTimer.Stop();
                inDrag = false;
            }
        }
        #endregion

        #region Thumbnails
        private void thumbTimer_Tick(object sender, EventArgs e)
        {
            thumbTimer.Stop();
            if (IsMouseOver) openThumb();
        }

        private void openThumb()
        {
            if (!ListMode && ThumbWindow == null && (Settings.Instance.EnableTaskbarThumbnails || _isGroup))
            {
                ThumbWindow = new TaskThumbWindow(this);
                ThumbWindow.Owner = ParentTaskbar;
                ThumbWindow.Show();
            }
        }

        private void closeThumb(bool force = false)
        {
            thumbTimer.Stop();
            if (!ListMode && ThumbWindow != null && (!ThumbWindow.IsMouseOver || force))
            {
                ThumbWindow.Close();
                ThumbWindow = null;
            }
        }

        public Point GetThumbnailAnchor()
        {
            Window ancestor = Window.GetWindow(this);
            if (ancestor != null)
            {
                var generalTransform = TransformToAncestor(ancestor);
                var anchorPoint = generalTransform.Transform(new Point(0, 0));

                anchorPoint.Y += ancestor.Top;
                anchorPoint.X += ancestor.Left;

                return anchorPoint;
            }

            return new Point(0, 0);
        }
        #endregion
    }
}