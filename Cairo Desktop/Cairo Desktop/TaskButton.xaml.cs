using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CairoDesktop.Common;
using CairoDesktop.WindowsTasks;

namespace CairoDesktop
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

        public ApplicationWindow Window;
        private DispatcherTimer dragTimer;
        private DispatcherTimer thumbTimer;
        public TaskThumbWindow ThumbWindow;

        public TaskButton()
        {
            InitializeComponent();

            // register for settings changes
            Settings.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        public void SelectWindow()
        {
            if (Window != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) ||
                    Keyboard.IsKeyDown(Key.RightShift))
                {
                    Shell.StartProcess(Window.WinFileName);
                    return;
                }

                if (Window.State == ApplicationWindow.WindowState.Active)
                {
                    Window.Minimize();
                }
                else
                {
                    Window.BringToFront();
                }
            }

            closeThumb(true);
        }

        public void ConfigureContextMenu()
        {
            if (Window != null)
            {
                Visibility vis = Visibility.Collapsed;
                NativeMethods.WindowShowStyle wss = Window.ShowStyle;
                int ws = Window.WindowStyles;

                // show pin option if this app is not yet in quick launch
                if (AppGrabber.AppGrabber.Instance.QuickLaunchManager.GetQuickLaunchApplicationInfo(Window) == null)
                    vis = Visibility.Visible;

                miPin.Visibility = vis;
                miPinSeparator.Visibility = vis;

                // disable window operations depending on current window state. originally tried implementing via bindings but found there is no notification we get regarding maximized state
                miMaximize.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowMaximized && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0);
                miMinimize.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowMinimized && (ws & (int)NativeMethods.WindowStyles.WS_MINIMIZEBOX) != 0);
                miRestore.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowNormal);
                miMove.IsEnabled = wss == NativeMethods.WindowShowStyle.ShowNormal;
                miSize.IsEnabled = (wss == NativeMethods.WindowShowStyle.ShowNormal && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0);
            }
        }

        public void SetParentAutoHide(bool enabled)
        {
            if (!ListMode && ParentTaskbar != null) ParentTaskbar.CanAutoHide = enabled;
        }

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window = DataContext as ApplicationWindow;

            Window.PropertyChanged += Window_PropertyChanged;

            if (!ListMode)
            {
                setIconSize();
                setToolTip();
            }
            else
            {
                // Task list display changes
                btn.Style = FindResource("CairoTaskListButtonStyle") as Style;
                pbProgress.Style = FindResource("TaskListProgressBar") as Style;
                ToolTipService.SetPlacement(btn, System.Windows.Controls.Primitives.PlacementMode.Right);
                WinTitle.TextAlignment = TextAlignment.Left;
                imgIcon.Margin = new Thickness(3, 0, 6, 0);
            }

            // drag support - delayed activation using system setting
            dragTimer = new DispatcherTimer { Interval = SystemParameters.MouseHoverTime };
            dragTimer.Tick += dragTimer_Tick;

            // thumbnails - delayed activation using system setting
            thumbTimer = new DispatcherTimer { Interval = SystemParameters.MouseHoverTime };
            thumbTimer.Tick += thumbTimer_Tick;
        }

        private void TaskButton_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Window.PropertyChanged -= Window_PropertyChanged;
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
                        setIconSize();
                        break;
                }
            }
        }

        private void Window_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // handle progress changes
                case "ProgressState":
                    pbProgress.IsIndeterminate = Window.ProgressState == NativeMethods.TBPFLAG.TBPF_INDETERMINATE;
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

        private void setIconSize()
        {
            if (!ListMode)
            {
                switch ((IconSize)Settings.Instance.TaskbarIconSize)
                {
                    case IconSize.Large:
                        imgIcon.Width = 32;
                        imgIcon.Height = 32;
                        break;
                    case IconSize.Medium:
                        imgIcon.Width = 24;
                        imgIcon.Height = 24;
                        break;
                    default:
                        imgIcon.Width = 16;
                        imgIcon.Height = 16;
                        break;
                }
            }
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            SelectWindow();
        }

        private void miRestore_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Window.Restore();
            }
        }

        private void miMove_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Window.Move();
            }
        }

        private void miSize_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Window.Size();
            }
        }

        private void miMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Window.Minimize();
            }
        }

        private void miMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Window.Maximize();
            }
        }

        private void miNewWindow_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Shell.StartProcess(Window.WinFileName);
            }
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                Tasks.Instance.CloseWindow(Window);
            }
        }

        /// <summary>
        /// Handler that adjusts the visibility and usability of menu items depending on window state and app's inclusion in Quick Launch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            ConfigureContextMenu();
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (!IsMouseOver) closeThumb(true);
            SetParentAutoHide(true);
        }

        private void miPin_Click(object sender, RoutedEventArgs e)
        {
            if (Window != null)
            {
                AppGrabber.AppGrabber.Instance.QuickLaunchManager.PinToQuickLaunch(Window.IsUWP, Window.IsUWP ? Window.AppUserModelID : Window.WinFileName);
            }
        }

        private void miTaskMan_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartTaskManager();
        }

        private void btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (Window != null)
                {
                    switch (Settings.Instance.TaskbarMiddleClick)
                    {
                        case 0 when Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift):
                            Tasks.Instance.CloseWindow(Window);
                            break;
                        case 1 when !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift):
                            Tasks.Instance.CloseWindow(Window);
                            break;
                        default:
                            Shell.StartProcess(Window.WinFileName);
                            break;
                    }
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

        #region Drag support
        private bool inDrag = false;

        private void dragTimer_Tick(object sender, EventArgs e)
        {
            if (inDrag && Window != null)
            {
                Window.BringToFront();
            }

            dragTimer.Stop();
        }

        private void btn_DragEnter(object sender, DragEventArgs e)
        {
            if (!inDrag)
            {
                inDrag = true;
                SetParentAutoHide(false);
                dragTimer.Start();
            }
        }

        private void btn_DragLeave(object sender, DragEventArgs e)
        {
            if (inDrag)
            {
                dragTimer.Stop();
                SetParentAutoHide(true);
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
            if (!ListMode && ThumbWindow == null && Settings.Instance.EnableTaskbarThumbnails)
            {
                ThumbWindow = new TaskThumbWindow(this);
                ThumbWindow.Owner = ParentTaskbar;
                ThumbWindow.Show();
            }
        }

        private void closeThumb(bool force = false)
        {
            thumbTimer.Stop();
            if (!ListMode && ThumbWindow != null && !btn.ContextMenu.IsOpen && (!ThumbWindow.IsMouseOver || force))
            {
                ThumbWindow.Close();
                ThumbWindow = null;
            }
        }

        public Point GetThumbnailAnchor()
        {
            Window ancestor = System.Windows.Window.GetWindow(this);
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