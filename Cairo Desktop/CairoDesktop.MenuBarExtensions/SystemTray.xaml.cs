using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using System.ComponentModel;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.WindowsTray;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class SystemTray
    {
        private readonly IMenuBar Host;
        private readonly NotificationArea _notificationArea;

        public SystemTray(IMenuBar host, NotificationArea notificationArea)
        {
            InitializeComponent();
            
            _notificationArea = notificationArea;
            DataContext = _notificationArea;
            Host = host;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            ((INotifyCollectionChanged)PinnedItems.Items).CollectionChanged += PinnedItems_CollectionChanged;
            ((INotifyCollectionChanged)UnpinnedItems.Items).CollectionChanged += UnpinnedItems_CollectionChanged;

            if (Settings.Instance.SysTrayAlwaysExpanded)
            {
                UnpinnedItems.Visibility = Visibility.Visible;
            }

            // Don't allow showing both the Windows TaskBar and the Cairo tray
            if (Settings.Instance.EnableSysTray && (Settings.Instance.EnableTaskbar || EnvironmentHelper.IsAppRunningAsShell) && _notificationArea.Handle == IntPtr.Zero)
            {
                _notificationArea.Initialize();
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

        private TrayHostSizeData GetTrayHostSizeData()
        {
            MenuBarDimensions dimensions = Host.GetDimensions();

            return new TrayHostSizeData
            {
                edge = (NativeMethods.ABEdge)dimensions.ScreenEdge,
                rc = new NativeMethods.Rect
                {
                    Top = (int)(dimensions.Top * dimensions.DpiScale),
                    Left = (int)(dimensions.Left * dimensions.DpiScale),
                    Bottom = (int)((dimensions.Top + dimensions.Height) * dimensions.DpiScale),
                    Right = (int)((dimensions.Left + dimensions.Width) * dimensions.DpiScale)
                }
            };
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            if (Host != null)
            {
                // set current menu bar to return placement for ABM_GETTASKBARPOS message
                _notificationArea.SetTrayHostSizeData(GetTrayHostSizeData());
            }

            trayIcon?.IconMouseDown(e.ChangedButton, MouseHelper.GetCursorPositionParam(), System.Windows.Forms.SystemInformation.DoubleClickTime);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            trayIcon?.IconMouseUp(e.ChangedButton, MouseHelper.GetCursorPositionParam(), System.Windows.Forms.SystemInformation.DoubleClickTime);
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

                trayIcon.Placement = new NativeMethods.Rect { Top = (int)location.Y, Left = (int)location.X, Bottom = (int)(sendingDecorator.ActualHeight * dpiScale), Right = (int)(sendingDecorator.ActualWidth * dpiScale) };
                trayIcon.IconMouseEnter(MouseHelper.GetCursorPositionParam());
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            trayIcon?.IconMouseLeave(MouseHelper.GetCursorPositionParam());
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var trayIcon = (sender as Decorator).DataContext as NotifyIcon;

            trayIcon?.IconMouseMove(MouseHelper.GetCursorPositionParam());
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