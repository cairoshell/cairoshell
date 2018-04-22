using CairoDesktop.Common.Logging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    public class NotificationArea : DependencyObject, IDisposable
    {
        const string VOLUME_GUID = "7820ae73-23e3-4229-82c1-e41cb67d5b9c";
        IWindowsHooksWrapper hooksWrapper = new WindowsHooksWrapper();
        private SystrayDelegate trayDelegate;
        private IconDataDelegate iconDataDelegate;
        private object _lockObject = new object();
        public IntPtr Handle;
        public bool IsFailed = false;

        private DateTime _lastLClick = DateTime.Now;
        private DateTime _lastRClick = DateTime.Now;
        private IntPtr _lastClickHwnd;

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

        private NotificationArea() { }

        public void Initialize()
        {
            try
            {
                setWindowsTaskbarBottommost();
                trayDelegate = new SystrayDelegate(SysTrayCallback);
                iconDataDelegate = new IconDataDelegate(IconDataCallback);
                hooksWrapper.SetSystrayCallback(trayDelegate);
                hooksWrapper.SetIconDataCallback(iconDataDelegate);
                Handle = hooksWrapper.InitializeSystray();
                hooksWrapper.Run();
            }
            catch
            {
                IsFailed = true;
            }
        }

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

        private IntPtr IconDataCallback(CAIROWINNOTIFYICONIDENTIFIER iconData)
        {
            NotifyIcon icon = null;
            foreach (NotifyIcon ti in TrayIcons)
            {
                if ((iconData.guidItem != Guid.Empty && iconData.guidItem == ti.GUID) || (ti.HWnd == (IntPtr)iconData.hWnd && ti.UID == iconData.uID))
                {
                    icon = ti;
                    break;
                }
            }

            if (icon != null || iconData.guidItem == new Guid(VOLUME_GUID))
            {
                if (iconData.dwMessage == 1)
                    return Interop.Shell.MakeLParam(GetSystemMetrics(0) - 23, 0);
                else if (iconData.dwMessage == 2)
                    return Interop.Shell.MakeLParam(23, 23);
            }

            return IntPtr.Zero;
        }

        private bool SysTrayCallback(uint message, NOTIFYICONDATA nicData)
        {
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

                            if (((uint)NIF.NIF_TIP & nicData.uFlags) != 0)
                                trayIcon.Title = nicData.szTip;

                            if (((uint)NIF.NIF_ICON & nicData.uFlags) != 0)
                            {
                                if ((IntPtr)nicData.hIcon != IntPtr.Zero)
                                {
                                    try
                                    {
                                        trayIcon.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon((IntPtr)nicData.hIcon, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                    }
                                    catch
                                    {
                                        if (trayIcon.Icon == null)
                                            trayIcon.Icon = Common.IconImageConverter.GetDefaultIcon();
                                    }
                                }
                                else
                                {
                                    trayIcon.Icon = Common.IconImageConverter.GetDefaultIcon();
                                }
                            }
                            trayIcon.HWnd = (IntPtr)nicData.hWnd;
                            trayIcon.UID = nicData.uID;
                            trayIcon.GUID = nicData.guidItem;

                            if (nicData.uVersion > 0 && nicData.uVersion <= 4)
                                trayIcon.Version = nicData.uVersion;

                            if (((uint)NIF.NIF_MESSAGE & nicData.uFlags) != 0)
                                trayIcon.CallbackMessage = nicData.uCallbackMessage;

                            if (!exists)
                            {
                                TrayIcons.Add(trayIcon);
                                CairoLogger.Instance.Info("Added tray icon: " + trayIcon.Title);
                            }
                            else
                                CairoLogger.Instance.Info("Modified tray icon: " + trayIcon.Title);
                        }
                    }
                    catch (Exception ex)
                    {
                        CairoLogger.Instance.Error("Unable to modify the icon in the collection. Error: " + ex.ToString());
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

                        // Woo! Using Linq to avoid iterating!
                        TrayIcons.Remove(trayIcon);

                        CairoLogger.Instance.Info("Removed tray icon: " + nicData.szTip);
                    }
                    catch (Exception ex)
                    {
                        CairoLogger.Instance.Error("Unable to remove the icon from the collection. Error: " + ex.ToString());
                    }
                }
                else if ((NIM)message == NIM.NIM_SETVERSION)
                {
                    foreach (NotifyIcon ti in TrayIcons)
                    {
                        if ((nicData.guidItem != Guid.Empty && nicData.guidItem == ti.GUID) || (ti.HWnd == (IntPtr)nicData.hWnd && ti.UID == nicData.uID))
                        {
                            ti.Version = nicData.uVersion;
                            break;
                        }
                    }
                }
            }
            return true;
        }

        public void Dispose()
        {
            if (!IsFailed && trayDelegate != null)
                hooksWrapper.ShutdownSystray();
        }

        public void IconMouseEnter(NotifyIcon icon, uint mouse)
        {
            if (!IsWindow(icon.HWnd))
            {
                TrayIcons.Remove(icon);
                return;
            }
            else
            {
                uint wparam = icon.UID;

                if (icon.Version > 3)
                    wparam = mouse;

                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_MOUSEHOVER);

                if (icon.Version > 3)
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, NIN_POPUPOPEN);
            }
        }

        public void IconMouseLeave(NotifyIcon icon, uint mouse)
        {
            if (!IsWindow(icon.HWnd))
            {
                TrayIcons.Remove(icon);
                return;
            }
            else
            {
                uint wparam = icon.UID;

                if (icon.Version > 3)
                    wparam = mouse;

                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_MOUSELEAVE);

                if (icon.Version > 3)
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, NIN_POPUPCLOSE);
            }
        }

        public void IconMouseClick(NotifyIcon icon, MouseButton button, uint mouse, int doubleClickTime)
        {
            CairoLogger.Instance.Info(String.Format("{0} mouse button clicked icon: {1}", button.ToString(), icon.Title));

            uint wparam = icon.UID;

            if (icon.Version > 3)
                wparam = mouse;

            if (button == MouseButton.Left)
            {
                if (DateTime.Now.Subtract(_lastLClick).TotalMilliseconds <= doubleClickTime && _lastClickHwnd == icon.HWnd)
                {
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_LBUTTONDBLCLK);
                }
                else
                {
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_LBUTTONDOWN);
                }

                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_LBUTTONUP);
                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, mouse, (NIN_SELECT | (icon.UID << 16)));

                _lastLClick = DateTime.Now;
            }
            else if (button == MouseButton.Right)
            {
                if (DateTime.Now.Subtract(_lastRClick).TotalMilliseconds <= doubleClickTime && _lastClickHwnd == icon.HWnd)
                {
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_RBUTTONDBLCLK);
                }
                else
                {
                    PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_RBUTTONDOWN);
                }

                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, wparam, WM_RBUTTONUP);
                PostMessage(icon.HWnd, (uint)icon.CallbackMessage, mouse, (WM_CONTEXTMENU | (icon.UID << 16)));

                _lastRClick = DateTime.Now;
            }

            _lastClickHwnd = icon.HWnd;

            SetForegroundWindow(icon.HWnd);
        }
    }
}
