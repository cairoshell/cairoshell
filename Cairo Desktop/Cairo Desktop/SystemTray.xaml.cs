using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.WindowsTray;
using System.Collections.Specialized;
using System.ComponentModel;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;

namespace CairoDesktop
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
            if (UnpinnedItems.Visibility == Visibility.Visible)
            {
                UnpinnedItems.Visibility = Visibility.Collapsed;
            }
            else
            {
                UnpinnedItems.Visibility = Visibility.Visible;
            }
        }

        private void SetToggleVisibility()
        {
            if (!Settings.Instance.SysTrayAlwaysExpanded)
            {
                if (UnpinnedItems.Items.Count > 0)
                    btnToggle.Visibility = Visibility.Visible;
                else
                    btnToggle.Visibility = Visibility.Collapsed;
            }
        }

        private uint getMousePos()
        {
            return (((uint)System.Windows.Forms.Cursor.Position.Y << 16) | (uint)System.Windows.Forms.Cursor.Position.X);
        }

        public TrayHostSizeData GetMenuBarSizeData()
        {
            return new TrayHostSizeData { edge = (int)MenuBar.appBarEdge, rc = new NativeMethods.Rect { Top = (int)(MenuBar.Top * MenuBar.dpiScale), Left = (int)(MenuBar.Left * MenuBar.dpiScale), Bottom = (int)((MenuBar.Top + MenuBar.Height) * MenuBar.dpiScale), Right = (int)((MenuBar.Left + MenuBar.Width) * MenuBar.dpiScale) } };
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;
            
            if (trayIcon != null)
            {
                if (MenuBar != null)
                {
                    // set current menu bar to return placement for ABM_GETTASKBARPOS message
                    NotificationArea.Instance.SetTrayHostSizeData(GetMenuBarSizeData());
                }
                trayIcon.IconMouseClick(e.ChangedButton, getMousePos(), System.Windows.Forms.SystemInformation.DoubleClickTime);
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

                trayIcon.Placement = new Interop.NativeMethods.Rect { Top = (int)location.Y, Left = (int)location.X, Bottom = (int)(sendingDecorator.ActualHeight * dpiScale), Right = (int)(sendingDecorator.ActualWidth * dpiScale) };
                trayIcon.IconMouseEnter(getMousePos());
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            if (trayIcon != null)
            {
                trayIcon.IconMouseLeave(getMousePos());
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            if (trayIcon != null)
            {
                trayIcon.IconMouseMove(getMousePos());
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "SysTrayAlwaysExpanded":
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
                        break;
                }
            }
        }
    }
}