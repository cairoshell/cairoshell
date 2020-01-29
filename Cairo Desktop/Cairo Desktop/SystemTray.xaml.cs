using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.WindowsTray;
using System.Collections.Specialized;

namespace CairoDesktop
{
    public partial class SystemTray
    {
        public SystemTray()
        {
            this.InitializeComponent();

            this.DataContext = NotificationArea.Instance;

            ((INotifyCollectionChanged)TrayItems.Items).CollectionChanged += TrayItems_CollectionChanged;
        }

        private void TrayItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Configuration.Settings.SysTrayAlwaysExpanded)
            {
                LayoutRoot.Visibility = Visibility.Visible;
                btnToggle.Visibility = Visibility.Collapsed;
            }
            else if (TrayItems.Items.Count > 0)
                btnToggle.Visibility = Visibility.Visible;
            else
                btnToggle.Visibility = Visibility.Collapsed;
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LayoutRoot.Visibility == Visibility.Visible)
            {
                LayoutRoot.Visibility = Visibility.Collapsed;
            }
            else
            {
                LayoutRoot.Visibility = Visibility.Visible;
            }
        }

        private uint getMousePos()
        {
            return (((uint)System.Windows.Forms.Cursor.Position.Y << 16) | (uint)System.Windows.Forms.Cursor.Position.X);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;
            
            if (trayIcon != null)
            {
                NotificationArea.Instance.IconMouseClick(trayIcon, e.ChangedButton, getMousePos(), System.Windows.Forms.SystemInformation.DoubleClickTime);
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            Decorator sendingDecorator = sender as Decorator;
            var trayIcon = sendingDecorator.DataContext as NotifyIcon;

            if (trayIcon != null)
            {
                // update icon position for Shell_NotifyIconGetRect
                Point location = sendingDecorator.PointToScreen(new Point(0, 0));
                double dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

                trayIcon.Placement = new Interop.NativeMethods.RECT { top = (int)location.Y, left = (int)location.X, bottom = (int)(sendingDecorator.ActualHeight * dpiScale), right = (int)(sendingDecorator.ActualWidth * dpiScale) };
                NotificationArea.Instance.IconMouseEnter(trayIcon, getMousePos());
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            if (trayIcon != null)
            {
                NotificationArea.Instance.IconMouseLeave(trayIcon, getMousePos());
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            if (trayIcon != null)
            {
                NotificationArea.Instance.IconMouseMove(trayIcon, getMousePos());
            }
        }
    }
}