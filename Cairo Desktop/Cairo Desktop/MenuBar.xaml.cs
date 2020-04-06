using CairoDesktop.AppGrabber;
using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop
{
    public partial class MenuBar : AppBarWindow
    {
        private MenuBarShadow shadow = null;
        private static bool isCairoMenuHotkeyRegistered = false;
        private static bool isProgramsMenuHotkeyRegistered = false;
        private static bool isClockHotkeyRegistered = false;
        private static bool isSearchHotkeyRegistered = false;

        private bool secretBottomMenuBar = false;

        // AppGrabber instance
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // delegates for WinSparkle
        private WinSparkle.win_sparkle_can_shutdown_callback_t canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t shutdownDelegate;
        private SystemTray systemTray;
        private MenuExtraVolume menuExtraVolume;
        private MenuExtraActionCenter menuExtraActionCenter;
        private MenuExtraClock menuExtraClock;
        private MenuExtraSearch menuExtraSearch;

        //private static LowLevelKeyboardListener keyboardListener; // temporarily removed due to stuck key issue, commented out to prevent warnings
        public MenuBar() : this(System.Windows.Forms.Screen.PrimaryScreen)
        {

        }

        public MenuBar(System.Windows.Forms.Screen screen)
        {
            InitializeComponent();

            Screen = screen;
            desiredHeight = 23;
            processScreenChanges = true;
            requiresScreenEdge = true;
            if (secretBottomMenuBar) appBarEdge = NativeMethods.ABEdge.ABE_BOTTOM;

            setPosition();

            setupChildren();

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

        private void setupChildren()
        {
            stacksContainer.MenuBar = this;
            programsMenuControl.MenuBar = this;
        }

        private void setupMenu()
        {
            if (Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                // show Windows 10 features
                miOpenUWPSettings.Visibility = Visibility.Visible;
            }

            // I didnt like the Exit Cairo option available when Cairo was set as Shell
            if (Shell.IsCairoRunningAsShell)
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
                systemTray = new SystemTray();
                systemTray.Margin = new Thickness(0, 0, 8, 0);
                systemTray.MenuBar = this;
                MenuExtrasHost.Children.Add(systemTray);

                // add volume
                menuExtraVolume = new MenuExtraVolume();
                MenuExtrasHost.Children.Add(menuExtraVolume);
            }

            if (Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                // add action center
                menuExtraActionCenter = new MenuExtraActionCenter(Handle);
                MenuExtrasHost.Children.Add(menuExtraActionCenter);
            }

            // add date/time
            menuExtraClock = new MenuExtraClock(Screen.Primary);
            MenuExtrasHost.Children.Add(menuExtraClock);

            // add search
            menuExtraSearch = new MenuExtraSearch();
            MenuExtrasHost.Children.Add(menuExtraSearch);
        }

        private void setupCairoMenu()
        {
            // Add _Application CairoMenu MenuItems
            if (ObjectModel._CairoShell.Instance.CairoMenu.Count > 0)
            {
                CairoMenu.Items.Insert(7, new Separator());
                foreach (var cairoMenuItem in ObjectModel._CairoShell.Instance.CairoMenu)
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
            if (ObjectModel._CairoShell.Instance.PlacesMenu.Count > 0)
            {
                PlacesMenu.Items.Add(new Separator());
                foreach (var placesMenuItem in ObjectModel._CairoShell.Instance.PlacesMenu)
                {
                    MenuItem menuItem = new MenuItem { Header = placesMenuItem.Header };
                    menuItem.Click += placesMenuItem.MenuItem_Click;
                    PlacesMenu.Items.Add(menuItem);
                }
            }
        }

        private void setupShadow()
        {
            if (Settings.Instance.EnableMenuBarShadow && shadow == null && !secretBottomMenuBar)
            {
                shadow = new MenuBarShadow(this);
                shadow.Show();
                setShadowPosition();
            }
        }

        protected override void postInit()
        {
            setupMenuExtras();

            if (Settings.Instance.EnableCairoMenuHotKey && Screen.Primary && !isCairoMenuHotkeyRegistered)
            {
                HotKeyManager.RegisterHotKey(Settings.Instance.CairoMenuHotKey, OnShowCairoMenu);
                isCairoMenuHotkeyRegistered = true;
            }

            if (Screen.Primary && !isClockHotkeyRegistered)
            {
                new HotKey(Key.D, KeyModifier.Win | KeyModifier.Alt | KeyModifier.NoRepeat, OnShowClock);
                isClockHotkeyRegistered = true;
            }

            if (Screen.Primary && !isSearchHotkeyRegistered)
            {
                new HotKey(Key.S, KeyModifier.Win | KeyModifier.NoRepeat, OnShowSearchHotkey);
                isSearchHotkeyRegistered = true;
            }

            // Register L+R Windows key to open Programs menu
            if (Shell.IsCairoRunningAsShell && Screen.Primary && !isProgramsMenuHotkeyRegistered)
            {
                /*if (keyboardListener == null)
                    keyboardListener = new LowLevelKeyboardListener();

                keyboardListener.OnKeyPressed += keyboardListener_OnKeyPressed;
                keyboardListener.HookKeyboard();*/

                new HotKey(Key.LWin, KeyModifier.Win | KeyModifier.NoRepeat, OnShowProgramsMenu);
                new HotKey(Key.RWin, KeyModifier.Win | KeyModifier.NoRepeat, OnShowProgramsMenu);
                new HotKey(Key.Escape, KeyModifier.Ctrl | KeyModifier.NoRepeat, OnShowProgramsMenu);

                isProgramsMenuHotkeyRegistered = true;
            }

            if (Settings.Instance.EnableMenuBarBlur)
            {
                Shell.EnableWindowBlur(Handle);
            }

            setupShadow();
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
                NativeMethods.SetForegroundWindow(Handle);
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

        internal override void afterAppBarPos(bool isSameCoords, NativeMethods.Rect rect)
        {
            base.afterAppBarPos(isSameCoords, rect);

            if (!isSameCoords)
            {
                setShadowPosition();
            }
        }

        internal override void setPosition()
        {
            Top = getDesiredTopPosition();
            Left = Screen.Bounds.X / dpiScale;
            Width = Screen.Bounds.Width / dpiScale;
            Height = desiredHeight;
            setShadowPosition();
        }

        private void setShadowPosition()
        {
            if (shadow != null)
            {
                shadow.SetPosition();
                // running this on a short delay fixes changing dpi on multiple displays
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
                timer.Start();
                timer.Tick += (sender1, args) =>
                {
                    if (shadow != null) shadow.SetPosition();
                    timer.Stop();
                };
            }
        }

        private void closeShadow()
        {
            if (shadow != null && !shadow.IsClosing)
            {
                shadow.Close();
                shadow = null;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            double top = getDesiredTopPosition();

            if (Top != top)
            {
                Top = top;
                setShadowPosition();
            }
        }

        private double getDesiredTopPosition()
        {
            double top;

            if (secretBottomMenuBar) top = (Screen.Bounds.Bottom / dpiScale) - desiredHeight;
            else top = Screen.Bounds.Y / dpiScale;

            return top;
        }

        public NativeMethods.MenuBarSizeData GetMenuBarSizeData()
        {
            return new NativeMethods.MenuBarSizeData { edge = (int)appBarEdge, rc = new NativeMethods.Rect { Top = (int)(Top * dpiScale), Left = (int)(Left * dpiScale), Bottom = (int)((Top + Height) * dpiScale), Right = (int)((Left + Width) * dpiScale) } };
        }

        protected override void customClosing()
        {
            if (Startup.IsShuttingDown && Screen.Primary)
            {
                WinSparkle.win_sparkle_cleanup();
            }

            if (WindowManager.Instance.IsSettingDisplays || Startup.IsShuttingDown)
            {
                closeShadow();
            }
        }

        private void OnShowCairoMenu(HotKey hotKey)
        {
            if (!CairoMenu.IsSubmenuOpen)
            {
                NativeMethods.SetForegroundWindow(Handle);
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

        private void OnShowClock(HotKey hotKey)
        {
            if (menuExtraClock != null)
            {
                menuExtraClock.ToggleClockDisplay();
            }
        }

        private void OnShowSearchHotkey(HotKey hotKey)
        {
            if (menuExtraSearch != null)
            {
                menuExtraSearch.ToggleSearch();
            }
        }
    }
}