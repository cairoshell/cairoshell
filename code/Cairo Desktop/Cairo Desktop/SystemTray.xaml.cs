using System;
using System.Windows;
using System.Collections.ObjectModel;
using CairoDesktop.WindowsTray;
using System.Linq;
using System.Diagnostics;
using static CairoDesktop.Interop.NativeMethods;
using CairoDesktop.Common;
using System.Windows.Threading;

namespace CairoDesktop
{
    public partial class SystemTray
    {
        private DateTime _lastLClick = DateTime.Now;
        private DateTime _lastRClick = DateTime.Now;
        private object _lockObject = new object();
        IWindowsHooksWrapper hooksWrapper = new WindowsHooksWrapper();

        private DependencyProperty iconListProperty = DependencyProperty.Register("TrayIcons", typeof(ObservableCollection<TrayIcon>), typeof(SystemTray), new PropertyMetadata(new ObservableCollection<TrayIcon>()));
        private SystrayDelegate trayDelegate;

        public SystemTray()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the list of visible tray icons.
        /// </summary>
        public ObservableCollection<TrayIcon> TrayIcons
        {
            get
            {
                return GetValue(iconListProperty) as ObservableCollection<TrayIcon>;
            }
            set
            {
                // Synchronised!
                if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate { this.TrayIcons = value; });
                    return;
                }
                SetValue(iconListProperty, value);
            }
        }

        public void InitializeSystemTray()
        {
            try
            {
                trayDelegate = new SystrayDelegate(SysTrayCallback);
                hooksWrapper.SetSystrayCallback(trayDelegate);
                hooksWrapper.InitializeSystray();
                hooksWrapper.Run();

                if (Configuration.Settings.EnableSysTrayRehook)
                {
                    DispatcherTimer trayRehook = new DispatcherTimer(DispatcherPriority.Background, this.Dispatcher);
                    trayRehook.Interval = new TimeSpan(0, 0, 10);
                    trayRehook.Tick += trayRehook_Tick;
                    trayRehook.Start();
                }
            }
            catch (Exception ex)
            {
                CairoMessage.ShowAlert("Error initializing the system tray component.\n\n" + ex.ToString() + "\n\nIf this error continues please report it (including a screen shot of this message) to the Cairo Development Team. \nThank you.", "Error", MessageBoxImage.Asterisk);
            }
        }

        private void trayRehook_Tick(object sender, EventArgs e)
        {
            // check if setting has changed
            if (Configuration.Settings.EnableSysTrayRehook)
            {
                hooksWrapper.InitializeSystray();
                hooksWrapper.Run();
            }
            else
            {
                (sender as DispatcherTimer).Stop();
            }
        }

        public void DestroySystemTray()
        {
            if(trayDelegate != null)
                hooksWrapper.ShutdownSystray();
        }

        private bool SysTrayCallback(uint message, NOTIFYICONDATA nicData)
        {
            TrayIcon trayIcon = new TrayIcon((IntPtr)nicData.hWnd);
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
                            if (TrayIcons.Contains(trayIcon))
                            {
                                exists = true;
                                trayIcon = TrayIcons.Single(i => i.HWnd == (IntPtr)nicData.hWnd && i.UID == nicData.uID);
                            }

                            trayIcon.Title = nicData.szTip;
                            try
                            {
                                trayIcon.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon((IntPtr)nicData.hIcon, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                            }
                            catch
                            {
                                trayIcon.Icon = AppGrabber.IconImageConverter.GetDefaultIcon();
                            }
                            trayIcon.HWnd = (IntPtr)nicData.hWnd;
                            trayIcon.UID = nicData.uID;
                            trayIcon.CallbackMessage = nicData.uCallbackMessage;

                            if (!exists)
                            {
                                if(!Configuration.Settings.SysTrayAlwaysExpanded)
                                    btnToggle.Visibility = Visibility.Visible;
                                else
                                    LayoutRoot.Visibility = Visibility.Visible;

                                TrayIcons.Add(trayIcon);
                                Trace.WriteLine("Added tray icon: " + trayIcon.Title);
                            }
                            else
                                Trace.WriteLine("Modified tray icon: " + trayIcon.Title);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Unable to modify the icon in the collection. Error: " + ex.ToString());
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

                        Trace.WriteLine("Removed tray icon: " + nicData.szTip);

                        if (TrayIcons.Count < 1 && !Configuration.Settings.SysTrayAlwaysExpanded)
                            btnToggle.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Unable to remove the icon from the collection. Error: " + ex.ToString());
                    }
                }
            }
            return true;
        }

        private void Image_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            uint WM_LBUTTONDOWN = 0x201;
            uint WM_LBUTTONUP = 0x202;
            uint WM_LBUTTONDBLCLK = 0x203;
            uint WM_RBUTTONDOWN = 0x204;
            uint WM_RBUTTONUP = 0x205;
            uint WM_RBUTTONDBLCLK = 0x206;

            var trayIcon = (sender as System.Windows.Controls.Image).DataContext as TrayIcon;
                        
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (DateTime.Now.Subtract(_lastLClick).TotalSeconds < 1)
                {
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_LBUTTONDBLCLK);
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_LBUTTONUP);
                }
                else
                {
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_LBUTTONDOWN);
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_LBUTTONUP);
                }
                _lastLClick = DateTime.Now;
            }
            else if(e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                if (DateTime.Now.Subtract(_lastRClick).TotalSeconds < 1)
                {
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_RBUTTONDBLCLK);
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_RBUTTONUP);
                }
                else
                {
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_RBUTTONDOWN);
                    PostMessage(trayIcon.HWnd, (uint)trayIcon.CallbackMessage, (uint)trayIcon.UID, WM_RBUTTONUP);
                }
                _lastRClick = DateTime.Now;
            }

            Trace.WriteLine("Mouse up ("+ e.ChangedButton.ToString() +") on trayicon " + trayIcon.Title);
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

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var trayIcon = (sender as System.Windows.Controls.Image).DataContext as TrayIcon;

            if (!IsWindow(trayIcon.HWnd))
            {
                TrayIcons.Remove(trayIcon);
                return;
            }
        }
    }
}