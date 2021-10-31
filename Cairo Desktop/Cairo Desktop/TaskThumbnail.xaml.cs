using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.WindowsTasks;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for TaskThumbnail.xaml
    /// </summary>
    public partial class TaskThumbnail : UserControl
    {
        private ApplicationWindow _window;
        private bool _isLoaded;
        private IntPtr _taskbarHwnd;

        public static DependencyProperty ThumbWindowProperty = DependencyProperty.Register("ThumbWindow", typeof(TaskThumbWindow), typeof(TaskThumbnail));

        public TaskThumbWindow ThumbWindow
        {
            get { return (TaskThumbWindow)GetValue(ThumbWindowProperty); }
            set { SetValue(ThumbWindowProperty, value); }
        }

        public TaskThumbnail()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            _window = DataContext as ApplicationWindow;

            // if DWM or thumbnails disabled, hide the thumbnail placeholder
            if (!ThumbWindow.IsDwmEnabled || !Settings.Instance.EnableTaskbarThumbnails)
            {
                dwmThumbnail.Visibility = Visibility.Collapsed;
                pnlTitle.Margin = new Thickness(0);
            }
            else
            {
                _taskbarHwnd = ThumbWindow.TaskButton.ParentTaskbar.Handle;

                // set up thumbnail
                dwmThumbnail.DpiScale = ThumbWindow.TaskButton.ParentTaskbar.DpiScale;
                dwmThumbnail.ThumbnailOpacity = 0;
                dwmThumbnail.SourceWindowHandle = _window.Handle;

                // set up animation
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }

            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            if (ThumbWindow.IsDwmEnabled)
            {
                dwmThumbnail.SourceWindowHandle = IntPtr.Zero;
            }
            _isLoaded = false;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // runs once per frame for the duration of the animation
            if (ThumbWindow.IsAnimating)
            {
                dwmThumbnail.ThumbnailOpacity = Convert.ToByte(ThumbWindow.bdrThumb.Opacity * 255);
                dwmThumbnail.Refresh();
            }
            else
            {
                // refresh one last time to get the final frame's updates
                dwmThumbnail.ThumbnailOpacity = 255;
                dwmThumbnail.Refresh();
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        private void bdrThumbInner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_window == null)
                {
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftShift) ||
                    Keyboard.IsKeyDown(Key.RightShift))
                {
                    ShellHelper.StartProcess(_window.IsUWP ? "appx:" + _window.AppUserModelID : _window.WinFileName);
                    return;
                }

                if (_window.State == ApplicationWindow.WindowState.Active)
                {
                    _window.Minimize();
                }
                else
                {
                    _window.BringToFront();
                }

                ThumbWindow.Close();
            }
        }

        #region Context menu
        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ThumbWindow.IsContextMenuOpen = true;

            if (_window == null)
            {
                return;
            }

            NativeMethods.WindowShowStyle wss = _window.ShowStyle;
            int ws = _window.WindowStyles;

            miMaximize.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowMaximized && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0;
            miMinimize.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowMinimized && (ws & (int)NativeMethods.WindowStyles.WS_MINIMIZEBOX) != 0;
            miRestore.IsEnabled = wss != NativeMethods.WindowShowStyle.ShowNormal;
            miMove.IsEnabled = wss == NativeMethods.WindowShowStyle.ShowNormal;
            miSize.IsEnabled = wss == NativeMethods.WindowShowStyle.ShowNormal && (ws & (int)NativeMethods.WindowStyles.WS_MAXIMIZEBOX) != 0;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ThumbWindow.IsContextMenuOpen = false;
            ThumbWindow.Close();
        }

        private void miRestore_Click(object sender, RoutedEventArgs e)
        {
            _window?.Restore();
        }

        private void miMove_Click(object sender, RoutedEventArgs e)
        {
            _window?.Move();
        }

        private void miSize_Click(object sender, RoutedEventArgs e)
        {
            _window?.Size();
        }

        private void miMinimize_Click(object sender, RoutedEventArgs e)
        {
            _window?.Minimize();
        }

        private void miMaximize_Click(object sender, RoutedEventArgs e)
        {
            _window?.Maximize();
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            _window?.Close();
        }
        #endregion

        private void bdrThumbInner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (closeButton.Visibility != Visibility.Visible)
            {
                closeButton.Visibility = Visibility.Visible;
            }

            if (ThumbWindow.IsDwmEnabled)
            {
                WindowHelper.PeekWindow(true, _window.Handle, _taskbarHwnd);
            }
        }

        private void bdrThumbInner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (closeButton.Visibility == Visibility.Visible)
            {
                closeButton.Visibility = Visibility.Hidden;
            }

            if (ThumbWindow.IsDwmEnabled)
            {
                WindowHelper.PeekWindow(false, _window.Handle, _taskbarHwnd);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool closeThumbWindow = true;

            if (ThumbWindow.TaskButton.WindowGroup.Count > 1)
            {
                closeThumbWindow = false;
            }

            _window.Close();

            if (closeThumbWindow)
            {
                ThumbWindow.Close();
            }
        }
    }
}
