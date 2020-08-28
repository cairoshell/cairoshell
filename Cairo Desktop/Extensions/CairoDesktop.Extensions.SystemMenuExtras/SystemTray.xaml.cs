using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.WindowsTray;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public partial class SystemTray
    {
        public MenuBar MenuBar;

        public SystemTray(MenuBar menuBar)
        {
            InitializeComponent();

            DataContext = NotificationArea.Instance;
            MenuBar = menuBar;

            ((INotifyCollectionChanged)PinnedItems.Items).CollectionChanged += PinnedItems_CollectionChanged;
            ((INotifyCollectionChanged)UnpinnedItems.Items).CollectionChanged += UnpinnedItems_CollectionChanged;
        }

        private void PinnedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (PinnedItems.Items.Count > 0)
            {
                PinnedItems.Margin = new Thickness(16, 0, 0, 0);
            }
            else
            {
                PinnedItems.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void UnpinnedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Configuration.Settings.Instance.SysTrayAlwaysExpanded)
            {
                UnpinnedItems.Visibility = Visibility.Visible;
                btnToggle.Visibility = Visibility.Collapsed;
            }
            else if (UnpinnedItems.Items.Count > 0)
            {
                btnToggle.Visibility = Visibility.Visible;
            }
            else
            {
                btnToggle.Visibility = Visibility.Collapsed;
            }
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (UnpinnedItems.Visibility == Visibility.Visible)
            {
                UnpinnedItems.Visibility = Visibility.Collapsed;
            }
            else
            {
                UnpinnedItems.Visibility = Visibility.Visible;
            }
        }

        private uint GetMousePos()
        {
            return (((uint)System.Windows.Forms.Cursor.Position.Y << 16) | (uint)System.Windows.Forms.Cursor.Position.X);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Decorator sendingDecorator
                && sendingDecorator.DataContext is NotifyIcon trayIcon)
            {
                if (MenuBar != null)
                {
                    // set current menu bar to return placement for ABM_GETTASKBARPOS message
                    NotificationArea.Instance.SetMenuBarSizeData(MenuBar.GetMenuBarSizeData());
                }

                trayIcon.IconMouseClick(e.ChangedButton, GetMousePos(), System.Windows.Forms.SystemInformation.DoubleClickTime);
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Decorator sendingDecorator
                && sendingDecorator.DataContext is NotifyIcon trayIcon)
            {
                // update icon position for Shell_NotifyIconGetRect
                Point location = sendingDecorator.PointToScreen(new Point(0, 0));
                double dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

                trayIcon.Placement = new Interop.NativeMethods.Rect
                {
                    Top = (int)location.Y,
                    Left = (int)location.X,
                    Bottom = (int)(sendingDecorator.ActualHeight * dpiScale),
                    Right = (int)(sendingDecorator.ActualWidth * dpiScale)
                };

                trayIcon.IconMouseEnter(GetMousePos());
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Decorator sendingDecorator
                && sendingDecorator.DataContext is NotifyIcon trayIcon)
            {
                trayIcon.IconMouseLeave(GetMousePos());
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Decorator sendingDecorator
                && sendingDecorator.DataContext is NotifyIcon trayIcon)
            {
                trayIcon.IconMouseMove(GetMousePos());
            }
        }
    }
}