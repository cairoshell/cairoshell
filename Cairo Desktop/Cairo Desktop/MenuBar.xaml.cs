using CairoDesktop.AppGrabber;
using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using CairoDesktop.WindowsTray;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace CairoDesktop
{
    public partial class MenuBar : Window
    {
        public System.Windows.Forms.Screen Screen;
        private double dpiScale = 1.0;

        // AppBar properties
        private WindowInteropHelper helper;
        public IntPtr handle;
        private int appbarMessageId = -1;
        private bool isRaising;

        public bool IsClosing = false;

        private static bool isCairoMenuHotkeyRegistered = false;
        private static bool isProgramsMenuHotkeyRegistered = false;

        // AppGrabber instance
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // delegates for WinSparkle
        private WinSparkle.win_sparkle_can_shutdown_callback_t canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t shutdownDelegate;

        //private static LowLevelKeyboardListener keyboardListener; // temporarily removed due to stuck key issue, commented out to prevent warnings
        public MenuBar() : this(System.Windows.Forms.Screen.PrimaryScreen)
        {

        }

        public MenuBar(System.Windows.Forms.Screen screen)
        {
            InitializeComponent();

            Screen = screen;

            setPosition();

            setupMenu();

            setupCairoMenu();

            setupPlacesMenu();

            initSparkle();
        }

        private void initSparkle()
        {
            if (Screen.Primary)
            {
                WinSparkle.win_sparkle_set_appcast_url("https://cairoshell.github.io/appdescriptor.rss");
                canShutdownDelegate = canShutdown;
                shutdownDelegate = Startup.Shutdown;
                WinSparkle.win_sparkle_set_can_shutdown_callback(canShutdownDelegate);
                WinSparkle.win_sparkle_set_shutdown_request_callback(shutdownDelegate);
                WinSparkle.win_sparkle_init();
            }
        }

        private void setupMenu()
        {
            if (Shell.IsWindows10OrBetter && !Startup.IsCairoRunningAsShell)
            {
                // show Windows 10 features
                miOpenUWPSettings.Visibility = Visibility.Visible;
            }

            // I didnt like the Exit Cairo option available when Cairo was set as Shell
            if (Startup.IsCairoRunningAsShell)
            {
                miExitCairo.Visibility = Visibility.Collapsed;
            }

            // Fix for concurrent seperators
            Type previousType = null;
            foreach (UIElement item in CairoMenu.Items)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    Type currentType = item.GetType();
                    if (previousType == typeof(Separator) && currentType == typeof(Separator))
                    {
                        ((Separator)item).Visibility = Visibility.Collapsed;
                    }

                    previousType = currentType;
                }
            }

            // Show power options depending on system support
            if (Settings.Instance.ShowHibernate && Shell.CanHibernate())
            {
                miHibernate.Visibility = Visibility.Visible;
            }

            if (!Shell.CanSleep())
            {
                miSleep.Visibility = Visibility.Collapsed;
            }
        }

        private void setupMenuExtras()
        {
            // set up menu extras
            if (Settings.Instance.EnableSysTray)
            {
                // add systray
                SystemTray systemTray = new SystemTray();
                systemTray.Margin = new Thickness(0, 0, 8, 0);
                MenuExtrasHost.Children.Add(systemTray);

                // add volume
                MenuExtrasHost.Children.Add(new MenuExtraVolume());
            }

            if (Shell.IsWindows10OrBetter && !Startup.IsCairoRunningAsShell)
            {
                // add action center
                MenuExtrasHost.Children.Add(new MenuExtraActionCenter(helper.Handle));
            }

            // add date/time
            MenuExtrasHost.Children.Add(new MenuExtraClock(Screen.Primary));

            // add search
            MenuExtrasHost.Children.Add(new MenuExtraSearch());
        }

        private void setupCairoMenu()
        {
            // Add _Application CairoMenu MenuItems
            if (Extensibility.ObjectModel._CairoShell.Instance.CairoMenu.Count > 0)
            {
                CairoMenu.Items.Insert(7, new Separator());
                foreach (var cairoMenuItem in Extensibility.ObjectModel._CairoShell.Instance.CairoMenu)
                {
                    MenuItem menuItem = new MenuItem { Header = cairoMenuItem.Header };
                    menuItem.Click += cairoMenuItem.MenuItem_Click;
                    CairoMenu.Items.Insert(8, menuItem);
                }
            }
        }

        private void setupPlacesMenu()
        {
            // Set username
            string username = Environment.UserName.Replace("_", "__");
            miUserName.Header = username;

            // Only show Downloads folder on Vista or greater
            if (!Shell.IsWindowsVistaOrBetter)
            {
                PlacesDownloadsItem.Visibility = Visibility.Collapsed;
                PlacesVideosItem.Visibility = Visibility.Collapsed;
            }

            // Add _Application PlacesMenu MenuItems
            if (Extensibility.ObjectModel._CairoShell.Instance.PlacesMenu.Count > 0)
            {
                PlacesMenu.Items.Add(new Separator());
                foreach (var placesMenuItem in Extensibility.ObjectModel._CairoShell.Instance.PlacesMenu)
                {
                    MenuItem menuItem = new MenuItem { Header = placesMenuItem.Header };
                    menuItem.Click += placesMenuItem.MenuItem_Click;
                    PlacesMenu.Items.Add(menuItem);
                }
            }
        }

        private void setupPostInit()
        {
            setupMenuExtras();

            // set initial DPI. We do it here so that we get the correct value when DPI has changed since initial user logon to the system.
            if (Screen.Primary)
            {
                Shell.DpiScale = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;
            }

            this.dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            setPosition();

            appbarMessageId = AppBarHelper.RegisterBar(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, AppBarHelper.ABEdge.ABE_TOP);

            Shell.HideWindowFromTasks(handle);

            if (Settings.Instance.EnableCairoMenuHotKey && Screen.Primary && !isCairoMenuHotkeyRegistered)
            {
                HotKeyManager.RegisterHotKey(Settings.Instance.CairoMenuHotKey, OnShowCairoMenu);
                isCairoMenuHotkeyRegistered = true;
            }

            // Register L+R Windows key to open Programs menu
            if (Startup.IsCairoRunningAsShell && Screen.Primary && !isProgramsMenuHotkeyRegistered)
            {
                /*if (keyboardListener == null)
                    keyboardListener = new LowLevelKeyboardListener();

                keyboardListener.OnKeyPressed += keyboardListener_OnKeyPressed;
                keyboardListener.HookKeyboard();*/

                new HotKey(Key.LWin, KeyModifier.Win | KeyModifier.NoRepeat, OnShowProgramsMenu);
                new HotKey(Key.RWin, KeyModifier.Win | KeyModifier.NoRepeat, OnShowProgramsMenu);

                isProgramsMenuHotkeyRegistered = true;
            }
            if (Settings.Instance.EnableMenuBarBlur)
            {
                Shell.EnableWindowBlur(handle);
            }

            FullScreenHelper.Instance.FullScreenApps.CollectionChanged += FullScreenApps_CollectionChanged;
        }

        private int canShutdown()
        {
            return 1;
        }

        #region Programs menu
        private void OnShowProgramsMenu(HotKey hotKey)
        {
            ToggleProgramsMenu();
        }

        private void ToggleProgramsMenu()
        {
            if (!ProgramsMenu.IsSubmenuOpen)
            {
                NativeMethods.SetForegroundWindow(helper.Handle);
                ProgramsMenu.IsSubmenuOpen = true;
            }
            else
            {
                ProgramsMenu.IsSubmenuOpen = false;
            }
        }

        private void ProgramsMenu_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else if (!e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void ProgramsMenu_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                appGrabber.AddByPath(fileNames, AppCategoryType.Uncategorized);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;

                appGrabber.AddByPath(dropData.Path, AppCategoryType.Uncategorized);
            }
        }
        #endregion

        #region Events
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appbarMessageId && appbarMessageId != -1)
            {
                switch ((NativeMethods.AppBarNotifications)wParam.ToInt32())
                {
                    case NativeMethods.AppBarNotifications.PosChanged:
                        // Reposition to the top of the screen.
                        AppBarHelper.ABSetPos(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, AppBarHelper.ABEdge.ABE_TOP);
                        break;

                    case NativeMethods.AppBarNotifications.FullScreenApp:
                        // we have our own implementation now
                        /*if ((int)lParam == 1)
                        {
                            CairoLogger.Instance.Debug("Cairo leaving on-top");
                            this.Topmost = false;
                            Shell.ShowWindowBottomMost(this.handle);

                            if (Settings.Instance.EnableTaskbar)
                            {
                                Startup.TaskbarWindow.SetFullScreenMode(true);
                            }
                        }
                        else
                        {
                            CairoLogger.Instance.Debug("Cairo entering on-top");
                            this.Topmost = true;
                            Shell.ShowWindowTopMost(this.handle);

                            if (Settings.Instance.EnableTaskbar)
                            {
                                Startup.TaskbarWindow.SetFullScreenMode(false);
                            }
                        }
                        */
                        break;

                    case NativeMethods.AppBarNotifications.WindowArrange:
                        if ((int)lParam != 0)    // before
                        {
                            this.Visibility = Visibility.Collapsed;
                        }
                        else                         // after
                        {
                            this.Visibility = Visibility.Visible;
                        }

                        break;
                }
                handled = true;
            }
            else if (msg == NativeMethods.WM_ACTIVATE)
            {
                AppBarHelper.AppBarActivate(hwnd);
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGING)
            {
                // Extract the WINDOWPOS structure corresponding to this message
                NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                // If we are intentionally trying to become topmost, make it so
                if (isRaising && (wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                {
                    // Sometimes Windows thinks we shouldn't go topmost, so poke here to make it happen.
                    wndPos.hwndInsertAfter = (IntPtr)NativeMethods.HWND_TOPMOST;
                    wndPos.UpdateMessage(lParam);
                }
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGED)
            {
                AppBarHelper.AppBarWindowPosChanged(hwnd);
            }
            else if (msg == NativeMethods.WM_DPICHANGED)
            {
                if ((Settings.Instance.EnableMenuBarMultiMon || Configuration.Settings.Instance.EnableTaskbarMultiMon) && !Startup.IsSettingScreens)
                {
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
                }
                else if (!(Settings.Instance.EnableMenuBarMultiMon || Configuration.Settings.Instance.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                if (Screen.Primary)
                {
                    Shell.DpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                }

                this.dpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                setPosition();
                AppBarHelper.ABSetPos(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, AppBarHelper.ABEdge.ABE_TOP);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                if ((Settings.Instance.EnableMenuBarMultiMon || Configuration.Settings.Instance.EnableTaskbarMultiMon) && !Startup.IsSettingScreens && Screen.Primary)
                {
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
                }
                else if (!(Settings.Instance.EnableMenuBarMultiMon || Configuration.Settings.Instance.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }
            else if (msg == NativeMethods.WM_DEVICECHANGE && (int)wParam == 0x0007)
            {
                if ((Settings.Instance.EnableMenuBarMultiMon || Configuration.Settings.Instance.EnableTaskbarMultiMon) && !Startup.IsSettingScreens && Screen.Primary)
                {
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
                }
            }

            return IntPtr.Zero;
        }

        private void FullScreenApps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool found = false;

            foreach (FullScreenHelper.FullScreenApp app in FullScreenHelper.Instance.FullScreenApps)
            {
                if (app.screen.DeviceName == Screen.DeviceName)
                {
                    // we need to not be on top now
                    found = true;
                    break;
                }
            }

            if (found && Topmost)
            {
                setFullScreenMode(true);
            }
            else if (!found && !Topmost)
            {
                setFullScreenMode(false);
            }
        }

        private void setFullScreenMode(bool entering)
        {
            if (entering)
            {
                CairoLogger.Instance.Debug(string.Format("Menu Bar on {0} conceeding to full-screen app", Screen.DeviceName));

                Topmost = false;
                Shell.ShowWindowBottomMost(handle);
            }
            else
            {
                CairoLogger.Instance.Debug(string.Format("Menu Bar on {0} returning to normal state", Screen.DeviceName));

                isRaising = true;
                Topmost = true;
                Shell.ShowWindowTopMost(handle);
                isRaising = false;
            }
        }

        private void setPosition()
        {
            Top = Screen.Bounds.Y / dpiScale;
            Left = Screen.Bounds.X / dpiScale;
            Width = Screen.WorkingArea.Width / dpiScale;
            setShadowPosition();
        }

        private void setPosition(uint x, uint y)
        {
            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out int sWidth, out int sHeight);


            double top = Screen.Bounds.Y / dpiScale;
            double left = Screen.Bounds.X / dpiScale;

            this.Top = top;
            this.Left = left;
            this.Width = sWidth;
            setShadowPosition();
        }

        private void setShadowPosition()
        {
            foreach (MenuBarShadow barShadow in Startup.MenuBarShadowWindows)
            {
                if (barShadow != null && barShadow.MenuBar == this)
                {
                    barShadow.SetPosition();
                }
            }
        }

        private void OnWindowInitialized(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;

            setupPostInit();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            double top = Screen.Bounds.Y / dpiScale;

            if (this.Top != top)
            {
                this.Top = top;
                setShadowPosition();
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;

            if (Startup.IsShuttingDown && Screen.Primary)
            {
                NotificationArea.Instance.Dispose();

                AppBarHelper.RegisterBar(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale);

                WinSparkle.win_sparkle_cleanup();

                // Currently Unused
                /*if (keyboardListener != null)
                {
                    keyboardListener.UnHookKeyboard();
                }*/

                if (Startup.IsCairoRunningAsShell)
                {
                    AppBarHelper.ResetWorkArea();
                }

                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;
                FullScreenHelper.Instance.Dispose();
            }
            else if (Startup.IsSettingScreens || Startup.IsShuttingDown)
            {
                AppBarHelper.RegisterBar(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale);

                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;
            }
            else
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }

        private void OnShowCairoMenu(HotKey hotKey)
        {
            if (!CairoMenu.IsSubmenuOpen)
            {
                NativeMethods.SetForegroundWindow(helper.Handle);
                CairoMenu.IsSubmenuOpen = true;
            }
            else
            {
                CairoMenu.IsSubmenuOpen = false;
            }
        }

        private void keyboardListener_OnKeyPressed(object sender, Common.KeyEventArgs e)
        {
            if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                CairoLogger.Instance.Debug(e.Key.ToString() + " Key Pressed");
                ToggleProgramsMenu();
                e.Handled = true;
            }
        }

        #endregion

        #region Cairo menu items
        private void AboutCairo(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            CairoMessage.ShowAlert(
                Localization.DisplayString.sAbout_Version + " " + version + " - " + Localization.DisplayString.sAbout_PreRelease
                + "\n\n" + String.Format(Localization.DisplayString.sAbout_Copyright, DateTime.Now.Year.ToString()), "Cairo Desktop Environment", MessageBoxImage.None);
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            WinSparkle.win_sparkle_check_update_with_ui();
        }

        private void OpenLogoffBox(object sender, RoutedEventArgs e)
        {
            SystemPower.ShowLogOffConfirmation();
        }

        private void OpenRebootBox(object sender, RoutedEventArgs e)
        {
            SystemPower.ShowRebootConfirmation();
        }

        private void OpenShutDownBox(object sender, RoutedEventArgs e)
        {
            SystemPower.ShowShutdownConfirmation();
        }

        private void OpenRunWindow(object sender, RoutedEventArgs e)
        {
            Shell.ShowRunDialog();
        }

        private void OpenCloseCairoBox(object sender, RoutedEventArgs e)
        {
            bool? CloseCairoChoice = CairoMessage.ShowOkCancel(Localization.DisplayString.sExitCairo_Info, Localization.DisplayString.sExitCairo_Title, "Resources/exitIcon.png", Localization.DisplayString.sExitCairo_ExitCairo, Localization.DisplayString.sInterface_Cancel);
            if (CloseCairoChoice.HasValue && CloseCairoChoice.Value)
            {
                Startup.Shutdown();
            }
        }

        private void OpenControlPanel(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("control.exe");
        }

        private void miOpenUWPSettings_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("ms-settings://");
        }

        private void OpenTaskManager(object sender, RoutedEventArgs e)
        {
            Shell.StartTaskManager();
        }

        private void SysHibernate(object sender, RoutedEventArgs e)
        {
            Shell.Hibernate();
        }

        private void SysSleep(object sender, RoutedEventArgs e)
        {
            Shell.Sleep();
        }

        private void InitCairoSettingsWindow(object sender, RoutedEventArgs e)
        {
            CairoSettingsWindow.Instance.Show();
            CairoSettingsWindow.Instance.Activate();
        }

        private void InitAppGrabberWindow(object sender, RoutedEventArgs e)
        {
            appGrabber.ShowDialog();
        }
        #endregion

        #region Places menu items
        private void OpenMyDocs(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(KnownFolders.GetPath(KnownFolder.Documents));
        }

        private void OpenMyPics(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(KnownFolders.GetPath(KnownFolder.Pictures));
        }

        private void OpenMyMusic(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(KnownFolders.GetPath(KnownFolder.Music));
        }

        private void OpenMyVideos(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(KnownFolders.GetPath(KnownFolder.Videos));
        }

        private void OpenDownloads(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(KnownFolders.GetPath(KnownFolder.Downloads));
        }

        private void OpenMyComputer(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        }

        private void OpenUserFolder(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        private void OpenProgramFiles(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation(Environment.GetEnvironmentVariable("ProgramFiles"));
        }

        private void OpenRecycleBin(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenLocation("::{645FF040-5081-101B-9F08-00AA002F954E}");
        }
        #endregion
    }
}
