using CairoDesktop.Interop;
using System.Windows;
using System.Windows.Controls;

namespace CairoDesktop
{
    public partial class TaskButton
    {
        public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register("TextWidth", typeof(double), typeof(TaskButton), new PropertyMetadata(new double()));
        public double TextWidth
        {
            get { return (double)GetValue(TextWidthProperty); }
            set { SetValue(TextWidthProperty, value); }
        }

        public TaskButton()
        {
            this.InitializeComponent();

            switch (Configuration.Settings.TaskbarIconSize)
            {
                case 0:
                    imgIcon.Width = 32;
                    imgIcon.Height = 32;
                    break;
                case 10:
                    imgIcon.Width = 24;
                    imgIcon.Height = 24;
                    break;
                default:
                    imgIcon.Width = 16;
                    imgIcon.Height = 16;
                    break;
            }
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                if (Window.State == WindowsTasks.ApplicationWindow.WindowState.Active)
                {
                    Window.Minimize();
                }
                else
                {
                    Window.BringToFront();
                }
            }
        }

        private void miRestore_Click(object sender, RoutedEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Restore();
            }
        }

        private void miMinimize_Click(object sender, RoutedEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Minimize();
            }
        }

        private void miMaximize_Click(object sender, RoutedEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Maximize();
            }
        }

        private void miNewWindow_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess((DataContext as WindowsTasks.ApplicationWindow).WinFileName);
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Close();
            }
        }

        /// <summary>
        /// Handler that adjusts the visibility and usability of menu items depending on window state and app's inclusion in Quick Launch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Visibility vis = Visibility.Collapsed;
                NativeMethods.WindowShowStyle wss = Window.ShowStyle;

                // show pin option if this app is not yet in quick launch
                if (Window.QuickLaunchAppInfo == null)
                    vis = Visibility.Visible;

                miPin.Visibility = vis;
                miPinSeparator.Visibility = vis;

                // disable window operations depending on current window state. originally tried implementing via bindings but found there is no notification we get regarding maximized state
                miMaximize.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowMaximized);
                miMinimize.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowMinimized);
                miRestore.IsEnabled = (wss != NativeMethods.WindowShowStyle.ShowNormal);
            }
        }

        private void miPin_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.PinToQuickLaunch();
            }
        }

        private void miTaskMan_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartTaskManager();
        }

        private void btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle)
            {
                Shell.StartProcess((DataContext as WindowsTasks.ApplicationWindow).WinFileName);
            }
        }
    }
}