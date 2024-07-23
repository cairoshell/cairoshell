using System;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.WindowsTray;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class SystemTray
    {
        private bool _isLoaded;
        private readonly NotificationArea _notificationArea;
        private readonly Settings _settings;

        internal readonly IMenuBar Host;

        public ObservableCollection<NotifyIcon> PromotedIcons { get; private set; }

        public SystemTray(IMenuBar host, NotificationArea notificationArea, Settings settings)
        {
            PromotedIcons = new ObservableCollection<NotifyIcon>();

            InitializeComponent();
            
            _notificationArea = notificationArea;
            _settings = settings;
            DataContext = _notificationArea;
            Host = host;

            ((INotifyCollectionChanged)PinnedItems.Items).CollectionChanged += PinnedItems_CollectionChanged;
            ((INotifyCollectionChanged)UnpinnedItems.Items).CollectionChanged += UnpinnedItems_CollectionChanged;

            if (_settings.SysTrayAlwaysExpanded)
            {
                PromotedItems.Visibility = Visibility.Collapsed;
                UnpinnedItems.Visibility = Visibility.Visible;
            }

            // Don't allow showing both the Windows TaskBar and the Cairo tray
            if (_settings.EnableSysTray && (_settings.EnableTaskbar || EnvironmentHelper.IsAppRunningAsShell) && _notificationArea.Handle == IntPtr.Zero)
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

        private void NotificationArea_NotificationBalloonShown(object sender, NotificationBalloonEventArgs e)
        {
            // This is used to promote unpinned icons to show when the tray is collapsed.

            if (_notificationArea == null)
            {
                return;
            }

            NotifyIcon notifyIcon = e.Balloon.NotifyIcon;
            int duration = SystemTrayIcon.GetAdjustedBalloonTimeout(e.Balloon);

            if (Host?.GetIsPrimaryDisplay() == true)
            {
                Host.PeekDuringAutoHide(duration);
            }

            if (_notificationArea.PinnedIcons.Contains(notifyIcon))
            {
                // Do not promote pinned icons (they're already there!)
                return;
            }

            if (PromotedIcons.Contains(notifyIcon))
            {
                // Do not duplicate promoted icons
                return;
            }

            PromotedIcons.Add(notifyIcon);

            DispatcherTimer unpromoteTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(duration + 400) // Keep it around a few ms for the animation to complete
            };
            unpromoteTimer.Tick += (object timerSender, EventArgs timerE) =>
            {
                if (PromotedIcons.Contains(notifyIcon))
                {
                    PromotedIcons.Remove(notifyIcon);
                }
                unpromoteTimer.Stop();
            };
            unpromoteTimer.Start();
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (UnpinnedItems.Visibility == Visibility.Visible)
            {
                PromotedItems.Visibility = Visibility.Visible;
                UnpinnedItems.Visibility = Visibility.Collapsed;
            }
            else
            {
                PromotedItems.Visibility = Visibility.Collapsed;
                UnpinnedItems.Visibility = Visibility.Visible;
            }
        }

        private void SetToggleVisibility()
        {
            if (_settings.SysTrayAlwaysExpanded)
            {
                return;
            }

            if (UnpinnedItems.Items.Count > 0)
            {
                btnToggle.Visibility = Visibility.Visible;
            }
            else
            {
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

            if (_settings.SysTrayAlwaysExpanded)
            {
                btnToggle.Visibility = Visibility.Collapsed;
                PromotedItems.Visibility = Visibility.Collapsed;
                UnpinnedItems.Visibility = Visibility.Visible;
            }
            else
            {
                btnToggle.IsChecked = false;
                PromotedItems.Visibility = Visibility.Visible;
                UnpinnedItems.Visibility = Visibility.Collapsed;
                SetToggleVisibility();
            }
        }

        private void CairoSystemTray_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            _settings.PropertyChanged += Settings_PropertyChanged;
            _notificationArea.NotificationBalloonShown += NotificationArea_NotificationBalloonShown;

            _isLoaded = true;
        }

        private void CairoSystemTray_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            _settings.PropertyChanged -= Settings_PropertyChanged;
            _notificationArea.NotificationBalloonShown -= NotificationArea_NotificationBalloonShown;

            _isLoaded = false;
        }
    }
}