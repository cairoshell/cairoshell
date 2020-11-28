using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.WindowsTray;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            ((INotifyCollectionChanged)PinnedItems.Items).CollectionChanged += PinnedItems_CollectionChanged;
            ((INotifyCollectionChanged)UnpinnedItems.Items).CollectionChanged += UnpinnedItems_CollectionChanged;

            if (Settings.Instance.SysTrayAlwaysExpanded)
            {
                UnpinnedItems.Visibility = Visibility.Visible;
            }
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
            SetToggleVisibility();
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            UnpinnedItems.Visibility = UnpinnedItems.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void SetToggleVisibility()
        {
            if (Settings.Instance.SysTrayAlwaysExpanded)
            {
                return;
            }

            btnToggle.Visibility = UnpinnedItems.Items.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private uint GetMousePos()
        {
            return (((uint)System.Windows.Forms.Cursor.Position.Y << 16) | (uint)System.Windows.Forms.Cursor.Position.X);
        }

        public TrayHostSizeData GetMenuBarSizeData()
        {
            return new TrayHostSizeData { edge = (int)MenuBar.appBarEdge, rc = new NativeMethods.Rect { Top = (int)(MenuBar.Top * MenuBar.dpiScale), Left = (int)(MenuBar.Left * MenuBar.dpiScale), Bottom = (int)((MenuBar.Top + MenuBar.Height) * MenuBar.dpiScale), Right = (int)((MenuBar.Left + MenuBar.Width) * MenuBar.dpiScale) } };
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Decorator sendingDecorator) ||
                !(sendingDecorator.DataContext is NotifyIcon trayIcon))
            {
                return;
            }

            if (MenuBar != null)
            {
                // set current menu bar to return placement for ABM_GETTASKBARPOS message
                NotificationArea.Instance.SetTrayHostSizeData(GetMenuBarSizeData());
            }

            trayIcon.IconMouseClick(e.ChangedButton, GetMousePos(), System.Windows.Forms.SystemInformation.DoubleClickTime);
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is Decorator sendingDecorator) ||
                !(sendingDecorator.DataContext is NotifyIcon trayIcon))
            {
                return;
            }

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

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName))
            {
                return;
            }

            if (e.PropertyName != "SysTrayAlwaysExpanded")
            {
                return;
            }

            if (Settings.Instance.SysTrayAlwaysExpanded)
            {
                btnToggle.Visibility = Visibility.Collapsed;
                UnpinnedItems.Visibility = Visibility.Visible;
            }
            else
            {
                btnToggle.IsChecked = false;
                UnpinnedItems.Visibility = Visibility.Collapsed;
                SetToggleVisibility();
            }
        }
    }
}