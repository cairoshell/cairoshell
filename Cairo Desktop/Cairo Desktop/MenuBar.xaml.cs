using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CairoDesktop.Interop;
using System.Windows.Input;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using CairoDesktop.AppGrabber;
using CairoDesktop.WindowsTray;
using System.Threading;
using System.Collections.Generic;
using CairoDesktop.Extensibility.ObjectModel;
using System.Linq;

namespace CairoDesktop
{
    public partial class MenuBar
    {
        public System.Windows.Forms.Screen Screen;
        private double dpiScale = 1.0;

        // AppBar properties
        private WindowInteropHelper helper;
        public IntPtr handle;
        private int appbarMessageId = -1;

        public bool IsClosing = false;

        private static bool isHotkeyRegistered = false;

        // AppGrabber instance
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // delegates for WinSparkle
        private WinSparkle.win_sparkle_can_shutdown_callback_t canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t shutdownDelegate;

        private LowLevelKeyboardListener _listener;

        private Action WinKeyOverride;

        public MenuBar() : this(System.Windows.Forms.Screen.PrimaryScreen)
        {

        }

        public MenuBar(System.Windows.Forms.Screen screen)
        {
            InitializeComponent();

            Screen = screen;

            setPosition();

            setupMenu();

            setupPlaces();

            setupSearch();

            initializeClock();

            setupPrograms();

            initSparkle();

            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();

            if (true)
            {
                WinKeyOverride = ToggleProgramsMenu;
            }
        }

        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            Debug.WriteLine(e.KeyPressed.ToString() + " Key Pressed");
            if (e.KeyPressed == Key.LWin && WinKeyOverride != null)
            {
                e.Handled = true;
                WinKeyOverride.Invoke();
            }
        }

        private void initSparkle()
        {
            if (Screen.Primary)
            {
                WinSparkle.win_sparkle_set_appcast_url("https://cairoshell.github.io/appdescriptor.rss");
                canShutdownDelegate = canShutdown;
                shutdownDelegate = shutdown;
                WinSparkle.win_sparkle_set_can_shutdown_callback(canShutdownDelegate);
                WinSparkle.win_sparkle_set_shutdown_request_callback(shutdownDelegate);
                WinSparkle.win_sparkle_init();
            }
        }

        private void setupMenu()
        {
            if (Shell.IsWindows10OrBetter && !Startup.IsCairoUserShell)
            {
                // show Windows 10 features
                miOpenUWPSettings.Visibility = Visibility.Visible;
                MenuExtras.Visibility = Visibility.Visible;
            }
        }

        private void setupPrograms()
        {
            // Set Programs Menu to use appGrabber's ProgramList as its source
            categorizedProgramsList.ItemsSource = appGrabber.CategoryList;

            // set tab based on user preference
            int i = categorizedProgramsList.Items.IndexOf(appGrabber.CategoryList.GetCategory(Settings.DefaultProgramsCategory));
            categorizedProgramsList.SelectedIndex = i;
        }

        private void setupPlaces()
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
            if (_CairoShell.Instance.PlacesMenu.Count > 0)
            {
                var separatorStyle = PlacesMenu.Items.OfType<Separator>().First().Style;
                var menuItemStyle = PlacesMenu.Items.OfType<MenuItem>().First().Style;

                PlacesMenu.Items.Add(new Separator() { Style = separatorStyle });
                foreach (var menuItem in _CairoShell.Instance.PlacesMenu)
                {
                    menuItem.Style = menuItemStyle;
                    PlacesMenu.Items.Add(menuItem);
                }
            }
        }

        private void setupPostInit()
        {
            // set initial DPI. We do it here so that we get the correct value when DPI has changed since initial user logon to the system.
            if (Screen.Primary)
                Shell.DpiScale = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;

            this.dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            setPosition();

            appbarMessageId = AppBarHelper.RegisterBar(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, AppBarHelper.ABEdge.ABE_TOP);

            Shell.HideWindowFromTasks(handle);

            if (Settings.EnableCairoMenuHotKey && Screen.Primary && !isHotkeyRegistered)
            {
                HotKeyManager.RegisterHotKey(Settings.CairoMenuHotKey, OnShowCairoMenu);
                isHotkeyRegistered = true;
            }

            // Register Windows key to open Programs menu
            if (Startup.IsCairoUserShell)
            {
                HotKeyManager.RegisterHotKey(new List<string> { "Win", "LWin" }, OnShowProgramsMenu);
                HotKeyManager.RegisterHotKey(new List<string> { "Win", "RWin" }, OnShowProgramsMenu);
            }

            if (Settings.EnableMenuBarBlur)
                Shell.EnableWindowBlur(handle);
        }

        private void setupSearch()
        {
            this.CommandBindings.Add(new CommandBinding(CustomCommands.OpenSearchResult, ExecuteOpenSearchResult));

            // Show the search button only if the service is running
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                setSearchProvider();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
                DispatcherTimer searchcheck = new DispatcherTimer(DispatcherPriority.Background, this.Dispatcher);
                searchcheck.Interval = new TimeSpan(0, 0, 5);
                searchcheck.Tick += searchcheck_Tick;
                searchcheck.Start();
            }
        }

        private void searchcheck_Tick(object sender, EventArgs e)
        {
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                setSearchProvider();
                CairoSearchMenu.Visibility = Visibility.Visible;
                (sender as DispatcherTimer).Stop();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void setSearchProvider()
        {
            var thread = new Thread(() =>
            {
                // this sometimes takes a while
                Type provider = typeof(VistaSearchProvider.VistaSearchProviderHelper);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ObjectDataProvider vistaSearchProvider = new ObjectDataProvider();
                    vistaSearchProvider.ObjectType = provider;
                    CairoSearchMenu.DataContext = vistaSearchProvider;

                    Binding bSearchText = new Binding("SearchText");
                    bSearchText.Mode = BindingMode.Default;
                    bSearchText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    Binding bSearchResults = new Binding("Results");
                    bSearchResults.Mode = BindingMode.Default;
                    bSearchResults.IsAsync = true;

                    searchStr.SetBinding(TextBox.TextProperty, bSearchText);
                    lstSearchResults.SetBinding(ListView.ItemsSourceProperty, bSearchResults);
                }));
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void shutdown()
        {
            Startup.Shutdown();
        }

        private int canShutdown()
        {
            return 1;
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_AddToQuickLaunch(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.AddToQuickLaunch(app);
        }

        private void programsMenu_Rename(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            DockPanel parent = ((MenuItem)((ContextMenu)item.Parent).PlacementTarget).Header as DockPanel;
            TextBox rename = parent.FindName("txtProgramRename") as TextBox;
            TextBlock label = parent.FindName("lblProgramName") as TextBlock;

            rename.Visibility = Visibility.Visible;
            label.Visibility = Visibility.Collapsed;
            rename.Focus();
            rename.SelectAll();
        }

        private void programsMenu_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.RemoveAppConfirm(app);
        }

        private void programsMenu_Properties(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            AppGrabber.AppGrabber.ShowAppProperties(app);
        }

        #region Date/time
        /// <summary>
        /// Initializes the dispatcher timers to updates the time and date bindings
        /// </summary>
        private void initializeClock()
        {
            // initial display
            clock_Tick();

            // Create our timer for clock
            DispatcherTimer clock = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, delegate
            {
                clock_Tick();
            }, this.Dispatcher);
        }

        private void clock_Tick()
        {
            dateText.Text = DateTime.Now.ToString(Settings.TimeFormat);
            dateText.ToolTip = DateTime.Now.ToString(Settings.DateFormat);
        }

        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("timedate.cpl");
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
                        if ((int)lParam == 1)
                        {
                            Trace.WriteLine("Cairo leaving on-top");
                            this.Topmost = false;
                            Shell.ShowWindowBottomMost(this.handle);

                            if (Settings.EnableTaskbar)
                            {
                                Startup.TaskbarWindow.Topmost = false;
                                Shell.ShowWindowBottomMost(Startup.TaskbarWindow.handle);
                            }
                        }
                        else
                        {
                            Trace.WriteLine("Cairo entering on-top");
                            this.Topmost = true;
                            Shell.ShowWindowTopMost(this.handle);

                            if (Settings.EnableTaskbar)
                            {
                                Startup.TaskbarWindow.Topmost = true;
                                Shell.ShowWindowTopMost(Startup.TaskbarWindow.handle);
                            }
                        }

                        break;

                    case NativeMethods.AppBarNotifications.WindowArrange:
                        if ((int)lParam != 0)    // before
                            this.Visibility = Visibility.Collapsed;
                        else                         // after
                            this.Visibility = Visibility.Visible;

                        break;
                }
                handled = true;
            }
            else if (msg == NativeMethods.WM_ACTIVATE)
            {
                AppBarHelper.AppBarActivate(hwnd);
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGED)
            {
                AppBarHelper.AppBarWindowPosChanged(hwnd);
            }
            else if (msg == NativeMethods.WM_DPICHANGED)
            {
                if ((Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon) && !Startup.IsSettingScreens)
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
                else if (!(Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                if (Screen.Primary)
                    Shell.DpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                this.dpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                setPosition();
                AppBarHelper.ABSetPos(this, Screen, this.ActualWidth * dpiScale, this.ActualHeight * dpiScale, AppBarHelper.ABEdge.ABE_TOP);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                if ((Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon) && !Startup.IsSettingScreens)
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
                else if (!(Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon))
                {
                    Startup.ResetScreenCache();
                    Screen = System.Windows.Forms.Screen.PrimaryScreen;
                }

                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }
            else if (msg == NativeMethods.WM_DEVICECHANGE && (int)wParam == 0x0007)
            {
                if ((Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon) && !Startup.IsSettingScreens)
                    Startup.ScreenSetup(); // update Cairo window list based on new screen setup
            }

            return IntPtr.Zero;
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
            int sWidth;
            int sHeight;
            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out sWidth, out sHeight);


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
                    barShadow.SetPosition();
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

                if (Startup.IsCairoUserShell)
                    AppBarHelper.ResetWorkArea();
            }
            else if (Startup.IsSettingScreens || Startup.IsShuttingDown)
            {
                AppBarHelper.RegisterBar(this, Screen, this.ActualWidth, this.ActualHeight);
            }
            else
            {
                IsClosing = false;
                e.Cancel = true;
            }

            _listener.UnHookKeyboard();
        }

        private void Programs_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                appGrabber.AddByPath(fileNames, 2);
            }
        }

        private void txtProgramRename_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = e.OriginalSource as TextBox;
            ApplicationInfo app = ((box.Parent as DockPanel).Parent as MenuItem).DataContext as ApplicationInfo;

            if (!object.ReferenceEquals(app, null))
            {
                appGrabber.Rename(app, box.Text);
            }

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
            {
                if (peer is TextBlock)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }
            box.Visibility = Visibility.Collapsed;
        }

        private void txtProgramRename_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
        }

        private void txtProgramRename_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ProgramsMenu.IsKeyboardFocusWithin && !(e.NewFocus is TextBox))
                e.Handled = true;
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

        private void OnShowProgramsMenu(HotKey hotKey)
        {
            ToggleProgramsMenu();

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
            bool? LogoffChoice = CairoMessage.ShowOkCancel(Localization.DisplayString.sLogoff_Info, Localization.DisplayString.sLogoff_Title, "Resources/logoffIcon.png", Localization.DisplayString.sLogoff_Logoff, Localization.DisplayString.sInterface_Cancel);
            if (LogoffChoice.HasValue && LogoffChoice.Value)
            {
                NativeMethods.Logoff();
            }
        }

        private void OpenRebootBox(object sender, RoutedEventArgs e)
        {
            bool? RebootChoice = CairoMessage.ShowOkCancel(Localization.DisplayString.sRestart_Info, Localization.DisplayString.sRestart_Title, "Resources/restartIcon.png", Localization.DisplayString.sRestart_Restart, Localization.DisplayString.sInterface_Cancel);
            if (RebootChoice.HasValue && RebootChoice.Value)
            {
                NativeMethods.Reboot();
            }
        }

        private void OpenShutDownBox(object sender, RoutedEventArgs e)
        {
            bool? ShutdownChoice = CairoMessage.ShowOkCancel(Localization.DisplayString.sShutDown_Info, Localization.DisplayString.sShutDown_Title, "Resources/shutdownIcon.png", Localization.DisplayString.sShutDown_ShutDown, Localization.DisplayString.sInterface_Cancel);
            if (ShutdownChoice.HasValue && ShutdownChoice.Value)
            {
                NativeMethods.Shutdown();
            }
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
                shutdown();
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

        private void SysSleep(object sender, RoutedEventArgs e)
        {
            NativeMethods.Sleep();
        }

        private void InitCairoSettingsWindow(object sender, RoutedEventArgs e)
        {
            CairoSettingsWindow window = new CairoSettingsWindow();
            window.Show();
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

        #region Menu extras

        private void miOpenVolume_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("sndvol.exe", "-f " + (int)(((ushort)1880) | (uint)(23 << 16)));
        }

        private void miOpenActionCenter_Click(object sender, RoutedEventArgs e)
        {
            Shell.ShowActionCenter();
        }

        private void miOpenActionCenter_MouseEnter(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE,
                        NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }

        private void miOpenActionCenter_MouseLeave(object sender, MouseEventArgs e)
        {
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE,
                        NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) & ~NativeMethods.WS_EX_NOACTIVATE);
        }

        #endregion

        #region Search menu
        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("search:query=" + searchStr.Text);
        }

        private void searchStr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Shell.StartProcess("search:query=" + searchStr.Text);
            }
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            searchStr.Text = "";
            FocusSearchBox(sender, e);
        }

        public void FocusSearchBox(object sender, RoutedEventArgs e)
        {
            searchStr.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                searchStr.Focusable = true;
                searchStr.Focus();
                Keyboard.Focus(searchStr);
            }),
            DispatcherPriority.Render);
        }

        public void ExecuteOpenSearchResult(object sender, ExecutedRoutedEventArgs e)
        {
            var searchObj = (VistaSearchProvider.SearchResult)e.Parameter;

            if (!Shell.StartProcess(searchObj.Path))
            {
                CairoMessage.Show(Localization.DisplayString.sSearch_Error, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

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
    }
}
