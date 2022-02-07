using System;
using System.Windows;
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
            // TODO: Promote icons with balloons to the pinned area when tray is collapsed
            // Don't allow showing both the Windows TaskBar and the Cairo tray
            if (Settings.Instance.EnableSysTray && (Settings.Instance.EnableTaskbar || EnvironmentHelper.IsAppRunningAsShell) && _notificationArea.Handle == IntPtr.Zero)
            {
                _notificationArea.Initialize();
            }
        }

        public void SetTrayHostSizeData()
        {
            if (Host != null)
            {
                // set current menu bar to return placement for ABM_GETTASKBARPOS message
                _notificationArea.SetTrayHostSizeData(GetTrayHostSizeData());
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