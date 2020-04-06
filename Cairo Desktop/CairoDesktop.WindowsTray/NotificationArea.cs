using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    public class NotificationArea : DependencyObject, IDisposable
    {
        const string VOLUME_GUID = "7820ae73-23e3-4229-82c1-e41cb67d5b9c";
        NativeMethods.Rect defaultPlacement = new NativeMethods.Rect { Top = 0, Left = GetSystemMetrics(0) - 200, Bottom = 23, Right = 23 };
        IWindowsHooksWrapper hooksWrapper = new WindowsHooksWrapper();
        private SystrayDelegate trayDelegate;
        private IconDataDelegate iconDataDelegate;
        private MenuBarSizeDelegate menuBarSizeDelegate;
        private object _lockObject = new object();
        public IntPtr Handle;
        public bool IsFailed = false;
        private IOleCommandTarget sysTrayObject = null;
        private MenuBarSizeData menuBarSizeData = new MenuBarSizeData { edge = (int)ABEdge.ABE_TOP, rc = new NativeMethods.Rect { Top = 0, Left = 0, Bottom = 23, Right = GetSystemMetrics(0) } };

        private static NotificationArea _instance = new NotificationArea();
        public static NotificationArea Instance
        {
            get { return _instance; }
        }

        public ObservableCollection<NotifyIcon> TrayIcons
        {
            get
            {
                return GetValue(iconListProperty) as ObservableCollection<NotifyIcon>;
            }
            set
            {
                SetValue(iconListProperty, value);
            }
        }

        private static DependencyProperty iconListProperty = DependencyProperty.Register("TrayIcons", typeof(ObservableCollection<NotifyIcon>), typeof(NotificationArea), new PropertyMetadata(new ObservableCollection<NotifyIcon>()));

        public ICollectionView PinnedIcons
        {
            get
            {
                return GetValue(pinnedIconsProperty) as ICollectionView;
            }
            set
            {
                SetValue(pinnedIconsProperty, value);
            }
        }

        private static DependencyProperty pinnedIconsProperty = DependencyProperty.Register("PinnedIcons", typeof(ICollectionView), typeof(NotificationArea));

        public ICollectionView UnpinnedIcons
        {
            get
            {
                return GetValue(unpinnedIconsProperty) as ICollectionView;
            }
            set
            {
                SetValue(unpinnedIconsProperty, value);
            }
        }

        private static DependencyProperty unpinnedIconsProperty = DependencyProperty.Register("UnpinnedIcons", typeof(ICollectionView), typeof(NotificationArea));


        private NotificationArea() { }

        public void Initialize()
        {
            try
            {
                setWindowsTaskbarBottommost();
                prepareCollections();
                trayDelegate = new SystrayDelegate(SysTrayCallback);
                iconDataDelegate = new IconDataDelegate(IconDataCallback);
                menuBarSizeDelegate = new MenuBarSizeDelegate(MenuBarSizeCallback);
                hooksWrapper.SetSystrayCallback(trayDelegate);
                hooksWrapper.SetIconDataCallback(iconDataDelegate);
                hooksWrapper.SetMenuBarSizeCallback(menuBarSizeDelegate);
                Handle = hooksWrapper.InitializeSystray();
                hooksWrapper.Run();

                // load the shell system tray objects (network, power, etc)
                startShellServiceObject();
            }
            catch
            {
                IsFailed = true;
            }
        }

        #region Shell Service Object
        private void startShellServiceObject()
        {
            if (Shell.IsCairoRunningAsShell)
            {
                try
                {
                    sysTrayObject = (IOleCommandTarget)new SysTrayObject();
                    Guid sso = new Guid(CGID_SHELLSERVICEOBJECT);
                    sysTrayObject.Exec(ref sso, OLECMDID_NEW, OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                    CairoLogger.Instance.Debug("Unable to start shell service object.");
                }
            }
        }

        private void stopShellServiceObject()
        {
            if (sysTrayObject != null)
            {
                try
                {
                    Guid sso = new Guid(CGID_SHELLSERVICEOBJECT);
                    sysTrayObject.Exec(ref sso, OLECMDID_SAVE, OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                    CairoLogger.Instance.Debug("Unable to stop shell service object.");
                }
            }
        }
        #endregion

        #region Pause for AppBar interop
        public void Suspend()
        {
            if (Handle != null && Handle != IntPtr.Zero)
                SetWindowPos(Handle, (IntPtr)1, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOSIZE);
        }

        public void MakeActive()
        {
            if (Handle != null && Handle != IntPtr.Zero)
            {
                SetWindowPos(Handle, (IntPtr)(-1), 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOSIZE);
                setWindowsTaskbarBottommost();
            }
        }
        #endregion

        #region Collections
        private void prepareCollections()
        {
            // prepare grouped collections like the taskbar
            // then display these in system tray

            // prepare collections
            PinnedIcons = new ListCollectionView(TrayIcons);
            PinnedIcons.CollectionChanged += PinnedIcons_Changed;
            PinnedIcons.Filter = PinnedIcons_Filter;
            PinnedIcons.SortDescriptions.Add(new SortDescription("PinOrder", ListSortDirection.Ascending));
            var pinnedIconsView = PinnedIcons as ICollectionViewLiveShaping;
            pinnedIconsView.IsLiveFiltering = true;
            pinnedIconsView.LiveFilteringProperties.Add("IsPinned");
            pinnedIconsView.IsLiveSorting = true;
            pinnedIconsView.LiveSortingProperties.Add("PinOrder");

            UnpinnedIcons = new ListCollectionView(TrayIcons);
            UnpinnedIcons.CollectionChanged += PinnedIcons_Changed;
            UnpinnedIcons.Filter = UnpinnedIcons_Filter;
            var unpinnedIconsView = UnpinnedIcons as ICollectionViewLiveShaping;
            unpinnedIconsView.IsLiveFiltering = true;
            unpinnedIconsView.LiveFilteringProperties.Add("IsPinned");
        }

        private void PinnedIcons_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            // yup, do nothing. helps prevent a NRE
        }

        private bool PinnedIcons_Filter(object item)
        {
            return (item as NotifyIcon).IsPinned;
        }

        private bool UnpinnedIcons_Filter(object item)
        {
            return !(item as NotifyIcon).IsPinned;
        }
        #endregion

        #region Callbacks
        private MenuBarSizeData MenuBarSizeCallback()
        {
            return menuBarSizeData;
        }

        private IntPtr IconDataCallback(int dwMessage, uint hWnd, uint uID, Guid guidItem)
        {
            NotifyIcon icon = null;
            foreach (NotifyIcon ti in TrayIcons)
            {
                if ((guidItem != Guid.Empty && guidItem == ti.GUID) || (ti.HWnd == (IntPtr)hWnd && ti.UID == uID))
                {
                    icon = ti;
                    break;
                }
            }

            if (icon != null)
            {
                if (dwMessage == 1)
                    return Shell.MakeLParam(icon.Placement.Left, icon.Placement.Top);
                else if (dwMessage == 2)
                    return Shell.MakeLParam(icon.Placement.Right, icon.Placement.Bottom);
            }
            else if (guidItem == new Guid(VOLUME_GUID))
            {
                if (dwMessage == 1)
                    return Shell.MakeLParam(defaultPlacement.Left, defaultPlacement.Top);
                else if (dwMessage == 2)
                    return Shell.MakeLParam(defaultPlacement.Right, defaultPlacement.Bottom);
            }

            return IntPtr.Zero;
        }

        private bool SysTrayCallback(uint message, NOTIFYICONDATA nicData)
        {
            if (nicData.hWnd == 0)
                return false;

            NotifyIcon trayIcon = new NotifyIcon((IntPtr)nicData.hWnd);
            trayIcon.UID = nicData.uID;

            lock (_lockObject)
            {
                if ((NIM)message == NIM.NIM_ADD || (NIM)message == NIM.NIM_MODIFY)
                {
                    try
                    {
                        bool exists = false;

                        if (nicData.dwState != 1)
                        {
                            if (nicData.guidItem == new Guid(VOLUME_GUID))
                                return false;

                            foreach (NotifyIcon ti in TrayIcons)
                            {
                                if ((nicData.guidItem != Guid.Empty && nicData.guidItem == ti.GUID) || (ti.HWnd == (IntPtr)nicData.hWnd && ti.UID == nicData.uID))
                                {
                                    exists = true;
                                    trayIcon = ti;
                                    break;
                                }
                            }

                            if ((NIF.TIP & nicData.uFlags) != 0 && !string.IsNullOrEmpty(nicData.szTip))
                                trayIcon.Title = nicData.szTip;

                            if ((NIF.ICON & nicData.uFlags) != 0)
                            {
                                if ((IntPtr)nicData.hIcon != IntPtr.Zero)
                                {
                                    try
                                    {
                                        System.Windows.Media.Imaging.BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon((IntPtr)nicData.hIcon, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                                        if (bs != null)
                                            trayIcon.Icon = bs;
                                    }
                                    catch
                                    {
                                        if (trayIcon.Icon == null)
                                            trayIcon.Icon = Common.IconImageConverter.GetDefaultIcon();
                                    }
                                }
                                else
                                {
                                    trayIcon.Icon = null;
                                }
                            }

                            trayIcon.HWnd = (IntPtr)nicData.hWnd;
                            trayIcon.UID = nicData.uID;
                            trayIcon.GUID = nicData.guidItem;

                            // guess version in case we are receiving icons that aren't sending NIM_SETVERSION to new explorers
                            if ((NIF.VISTA_MASK & nicData.uFlags) != 0)
                                trayIcon.Version = 4;
                            else if ((NIF.XP_MASK & nicData.uFlags) != 0)
                                trayIcon.Version = 3;

                            if (nicData.uVersion > 0 && nicData.uVersion <= 4)
                                trayIcon.Version = nicData.uVersion;

                            if ((NIF.MESSAGE & nicData.uFlags) != 0)
                                trayIcon.CallbackMessage = nicData.uCallbackMessage;

                            if (!exists)
                            {
                                // default placement to a menu bar like rect
                                trayIcon.Placement = defaultPlacement;

                                // set pinned item properties
                                trayIcon.Path = Shell.GetPathForHandle(trayIcon.HWnd);
                                trayIcon.SetPinValues();

                                if (trayIcon.Icon == null)
                                    trayIcon.Icon = Common.IconImageConverter.GetDefaultIcon();

                                TrayIcons.Add(trayIcon);
                                CairoLogger.Instance.Debug("Added tray icon: " + trayIcon.Title + " path: " + Shell.GetPathForHandle(trayIcon.HWnd) + " GUID: " + trayIcon.GUID + " UID: " + trayIcon.UID);

                                if ((NIM)message == NIM.NIM_MODIFY)
                                {
                                    // return an error to the notifyicon as we received a modify for an icon we did not yet have
                                    return false;
                                }
                            }
                            else
                                CairoLogger.Instance.Debug("Modified tray icon: " + trayIcon.Title);
                        }
                    }
                    catch (Exception ex)
                    {
                        CairoLogger.Instance.Error("Unable to modify the icon in the collection.", ex);
                    }
                }
                else if ((NIM)message == NIM.NIM_DELETE)
                {
                    try
                    {
                        if (!TrayIcons.Contains(trayIcon))
                        {
                            // Nothing to remove.
                            return false;
                        }

                        TrayIcons.Remove(trayIcon);

                        CairoLogger.Instance.Debug("Removed tray icon: " + nicData.szTip);
                    }
                    catch (Exception ex)
                    {
                        CairoLogger.Instance.Error("Unable to remove the icon from the collection.", ex);
                    }
                }
                else if ((NIM)message == NIM.NIM_SETVERSION)
                {
                    foreach (NotifyIcon ti in TrayIcons)
                    {
                        if ((nicData.guidItem != Guid.Empty && nicData.guidItem == ti.GUID) || (ti.HWnd == (IntPtr)nicData.hWnd && ti.UID == nicData.uID))
                        {
                            ti.Version = nicData.uVersion;
                            CairoLogger.Instance.Debug("Modified version to " + ti.Version + " on tray icon: " + ti.Title);
                            break;
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        // The notification area control calls this when an icon is clicked to set the placement of its menu bar for ABM_GETTASKBARPOS usage
        public void SetMenuBarSizeData(MenuBarSizeData data)
        {
            menuBarSizeData = data;
        }

        private void setWindowsTaskbarBottommost()
        {
            IntPtr taskbarHwnd = FindWindow("Shell_TrayWnd", "");

            if (Handle != null && Handle != IntPtr.Zero)
            {
                while (taskbarHwnd == Handle)
                {
                    taskbarHwnd = FindWindowEx(IntPtr.Zero, taskbarHwnd, "Shell_TrayWnd", "");
                }
            }

            SetWindowPos(taskbarHwnd, (IntPtr)1, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE | (int)SetWindowPosFlags.SWP_NOACTIVATE);
        }

        public void Dispose()
        {
            if (!IsFailed && trayDelegate != null)
            {
                stopShellServiceObject();
                hooksWrapper.ShutdownSystray();
            }
        }
    }
}
