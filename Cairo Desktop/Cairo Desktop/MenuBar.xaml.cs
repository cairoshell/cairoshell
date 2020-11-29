using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.ObjectModel;
using CairoDesktop.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop
{
    public partial class MenuBar : AppBarWindow
    {
        private readonly IApplicationUpdateService _applicationUpdateService;

        private MenuBarShadow shadow;
        private static HotKey cairoMenuHotKey;
        private static List<HotKey> programsMenuHotKeys = new List<HotKey>();

        private bool secretBottomMenuBar = false;

        // AppGrabber instance
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        // menu extras
        private SystemTray systemTray;
        private MenuExtraVolume menuExtraVolume;
        private MenuExtraActionCenter menuExtraActionCenter;
        private MenuExtraClock menuExtraClock;
        private MenuExtraSearch menuExtraSearch;

        //private static LowLevelKeyboardListener keyboardListener; // temporarily removed due to stuck key issue, commented out to prevent warnings
        public MenuBar(IApplicationUpdateService applicationUpdateService) : this(applicationUpdateService, System.Windows.Forms.Screen.PrimaryScreen)
        {
        }

        public MenuBar(IApplicationUpdateService applicationUpdateService, System.Windows.Forms.Screen screen)
        {
            _applicationUpdateService = applicationUpdateService;

            InitializeComponent();

            Screen = screen;
            desiredHeight = 23;
            processScreenChanges = true;
            requiresScreenEdge = true;
            if (secretBottomMenuBar) appBarEdge = NativeMethods.ABEdge.ABE_BOTTOM;

            SetPosition();

            setupChildren();

            setupMenu();

            setupCairoMenu();

            setupPlacesMenu();

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
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

#if !DEBUG
            // I didnt like the Exit Cairo option available when Cairo was set as Shell
            if (Shell.IsCairoRunningAsShell)
            {
                miExitCairo.Visibility = Visibility.Collapsed;
            }
#endif

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
            SetHibernateVisibility();

            if (!Shell.CanSleep())
            {
                miSleep.Visibility = Visibility.Collapsed;
            }
        }

        private void setupMenuExtras()
        {
            // set up menu extras
            foreach (var extra in CairoApplication.Current.MenuExtras)
            {
                UserControl menuExtra = extra.StartControl(this);
                if (menuExtra != null) MenuExtrasHost.Children.Add(menuExtra);
            }

            if (Settings.Instance.EnableSysTray)
            {
                // add systray
                systemTray = new SystemTray(this);
                MenuExtrasHost.Children.Add(systemTray);
            }

            if (Settings.Instance.EnableMenuExtraVolume && Shell.IsWindows10OrBetter && Shell.IsCairoRunningAsShell)
            {
                // add volume
                menuExtraVolume = new MenuExtraVolume();
                MenuExtrasHost.Children.Add(menuExtraVolume);
            }

            if (Settings.Instance.EnableMenuExtraActionCenter && Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                // add action center
                menuExtraActionCenter = new MenuExtraActionCenter(this);
                MenuExtrasHost.Children.Add(menuExtraActionCenter);
            }

            if (Settings.Instance.EnableMenuExtraClock)
            {
                // add date/time
                menuExtraClock = new MenuExtraClock(this);
                MenuExtrasHost.Children.Add(menuExtraClock);
            }

            if (Settings.Instance.EnableMenuExtraSearch)
            {
                // add search
                menuExtraSearch = new MenuExtraSearch(this);
                MenuExtrasHost.Children.Add(menuExtraSearch);
            }
        }

        private void setupCairoMenu()
        {
            // Add CairoMenu MenuItems
            if (CairoApplication.Current.CairoMenu.Count > 0)
            {
                CairoMenu.Items.Insert(7, new Separator());
                foreach (var cairoMenuItem in CairoApplication.Current.CairoMenu)
                {
                    var menuItem = new System.Windows.Controls.MenuItem { Header = cairoMenuItem.Header };
                    menuItem.Click += cairoMenuItem.MenuItem_Click;
                    CairoMenu.Items.Insert(8, menuItem);
                }
            }
        }

        private void setupPlacesMenu()
        {
            string username = Environment.UserName.Replace("_", "__");
            miUserName.Header = username;

            // Only show Downloads folder on Vista or greater
            if (!Shell.IsWindowsVistaOrBetter)
            {
                PlacesDownloadsItem.Visibility = Visibility.Collapsed;
                PlacesVideosItem.Visibility = Visibility.Collapsed;
            }

            // Add PlacesMenu MenuItems
            if (CairoApplication.Current.PlacesMenu.Count > 0)
            {
                PlacesMenu.Items.Add(new Separator());
                foreach (var placesMenuItem in CairoApplication.Current.PlacesMenu)
                {
                    var menuItem = new System.Windows.Controls.MenuItem { Header = placesMenuItem.Header };
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

        protected override void PostInit()
        {
            setupMenuExtras();

            registerCairoMenuHotKey();

            // Register L+R Windows key to open Programs menu
            if (Shell.IsCairoRunningAsShell && Screen.Primary && programsMenuHotKeys.Count < 1)
            {
                /*if (keyboardListener == null)
                    keyboardListener = new LowLevelKeyboardListener();

                keyboardListener.OnKeyPressed += keyboardListener_OnKeyPressed;
                keyboardListener.HookKeyboard();*/

                programsMenuHotKeys.Add(new HotKey(Key.LWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.RWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.Escape, HotKeyModifier.Ctrl | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
            }
            else if (Shell.IsCairoRunningAsShell && Screen.Primary)
            {
                foreach (var hotkey in programsMenuHotKeys)
                {
                    hotkey.Action = OnShowProgramsMenu;
                }
            }

            SetBlur(Settings.Instance.EnableMenuBarBlur);

            setupShadow();
        }

        private void registerCairoMenuHotKey()
        {
            if (Settings.Instance.EnableCairoMenuHotKey && Screen.Primary && cairoMenuHotKey == null)
            {
                cairoMenuHotKey = HotKeyManager.RegisterHotKey(Settings.Instance.CairoMenuHotKey, OnShowCairoMenu);
            }
            else if (Settings.Instance.EnableCairoMenuHotKey && Screen.Primary)
            {
                cairoMenuHotKey.Action = OnShowCairoMenu;
            }
        }

        private void unregisterCairoMenuHotKey()
        {
            if (Screen.Primary)
            {
                cairoMenuHotKey?.Dispose();
                cairoMenuHotKey = null;
            }
        }

        private void SetHibernateVisibility()
        {
            if (Settings.Instance.ShowHibernate && Shell.CanHibernate())
            {
                miHibernate.Visibility = Visibility.Visible;
            }
            else
            {
                miHibernate.Visibility = Visibility.Collapsed;
            }
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

        internal override void AfterAppBarPos(bool isSameCoords, NativeMethods.Rect rect)
        {
            base.AfterAppBarPos(isSameCoords, rect);

            if (!isSameCoords)
            {
                setShadowPosition();
            }
        }

        internal override void SetPosition()
        {
            Top = getDesiredTopPosition();
            Left = Screen.Bounds.X / dpiScale;
            Width = Screen.Bounds.Width / dpiScale;
            Height = desiredHeight;
            setShadowPosition();
        }

        private void setShadowPosition()
        {
            shadow?.SetPosition();
        }

        private void closeShadow()
        {
            if (shadow != null && !shadow.IsClosing)
            {
                shadow.AllowClose = true;
                shadow.Close();
                shadow = null;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            double top = getDesiredTopPosition();

            if (Top == top)
            {
                return;
            }

            Top = top;

            setShadowPosition();
        }

        private double getDesiredTopPosition()
        {
            double top;

            if (secretBottomMenuBar) 
                top = (Screen.Bounds.Bottom / dpiScale) - desiredHeight;
            else
                top = Screen.Bounds.Y / dpiScale;

            return top;
        }

        protected override void CustomClosing()
        {
            if (!WindowManager.Instance.IsSettingDisplays && !CairoApplication.IsShuttingDown)
            {
                return;
            }

            closeShadow();
            Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
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

        private void MenuBar_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (Settings.Instance.TaskbarMode == 2)
            {
                Taskbar taskbar = WindowManager.GetScreenWindow(WindowManager.Instance.TaskbarWindows, Screen);

                if (taskbar != null && taskbar.appBarEdge == appBarEdge)
                {
                    taskbar.CanAutoHide = false;
                }
            }
        }

        private void MenuBar_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (Settings.Instance.TaskbarMode == 2)
            {
                Taskbar taskbar = WindowManager.GetScreenWindow(WindowManager.Instance.TaskbarWindows, Screen);

                if (taskbar != null && taskbar.appBarEdge == appBarEdge)
                {
                    taskbar.CanAutoHide = true;
                }
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "EnableCairoMenuHotKey" when Screen.Primary:
                        if (Settings.Instance.EnableCairoMenuHotKey)
                        {
                            registerCairoMenuHotKey();
                        }
                        else
                        {
                            unregisterCairoMenuHotKey();
                        }

                        break;
                    case "CairoMenuHotKey" when Screen.Primary:
                        if (Settings.Instance.EnableCairoMenuHotKey)
                        {
                            unregisterCairoMenuHotKey();
                            registerCairoMenuHotKey();
                        }

                        break;
                    case "EnableMenuBarBlur":
                        SetBlur(Settings.Instance.EnableMenuBarBlur);
                        break;
                    case "EnableMenuBarShadow":
                        if (Settings.Instance.EnableMenuBarShadow)
                        {
                            setupShadow();
                        }
                        else
                        {
                            closeShadow();
                        }
                        break;
                    case "ShowHibernate":
                        SetHibernateVisibility();
                        break;
                }
            }
        }

        #endregion

        #region Cairo menu items
        private void AboutCairo(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            CairoMessage.Show(
                Localization.DisplayString.sAbout_Version + " " + version + " - " + Localization.DisplayString.sAbout_PreRelease
                + "\n\n" + String.Format(Localization.DisplayString.sAbout_Copyright, DateTime.Now.Year.ToString()), "Cairo Desktop Environment", CairoMessageImage.Default);
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            _applicationUpdateService?.CheckForUpdates();
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
            CairoMessage.ShowOkCancel(Localization.DisplayString.sExitCairo_Info, Localization.DisplayString.sExitCairo_Title,
                CairoMessageImage.Default, Localization.DisplayString.sExitCairo_ExitCairo, Localization.DisplayString.sInterface_Cancel,
                result =>
                {
                    if (result == true)
                    {
                        CairoApplication.Current.ExitCairo();
                    }
                });
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

        private void SysLock(object sender, RoutedEventArgs e)
        {
            Shell.Lock();
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