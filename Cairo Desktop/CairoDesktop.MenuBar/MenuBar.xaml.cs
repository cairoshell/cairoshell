using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using ManagedShell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;
using ManagedShell.Common.Enums;
using NativeMethods = ManagedShell.Interop.NativeMethods;
using System.Windows.Threading;
using ManagedShell.ShellFolders;
using Microsoft.Extensions.Hosting;

namespace CairoDesktop.MenuBar
{
    public partial class MenuBar : CairoAppBarWindow, IMenuBar
    {
        internal readonly IAppGrabber _appGrabber;
        private readonly IApplicationUpdateService _applicationUpdateService;
        private readonly ISettingsUIService _settingsUiService;
        internal readonly ICommandService _commandService;
        internal readonly IHost _host;
        private readonly Settings _settings;

        private bool isCairoMenuInitialized;
        private bool isPlacesMenuInitialized;
        private MenuBarShadow shadow;
        private static HotKey cairoMenuHotKey;
        private static List<HotKey> programsMenuHotKeys = new List<HotKey>();

        //private static LowLevelKeyboardListener keyboardListener; // temporarily removed due to stuck key issue, commented out to prevent warnings
        
        public MenuBar(ICairoApplication cairoApplication, ShellManager shellManager, IWindowManager windowManager, IHost host, IAppGrabber appGrabber, IApplicationUpdateService applicationUpdateService, ISettingsUIService settingsUiService, ICommandService commandService, Settings settings, AppBarScreen screen, AppBarEdge edge, AppBarMode mode) : base(cairoApplication, shellManager, windowManager, screen, edge, mode, 23)
        {
            _appGrabber = appGrabber;
            _applicationUpdateService = applicationUpdateService;
            _settingsUiService = settingsUiService;
            _commandService = commandService;
            _host = host;
            _settings = settings;

            object menuBarWindowAllowsTransparencyResource = TryFindResource("MenuBarWindowAllowsTransparency");
            if (menuBarWindowAllowsTransparencyResource is bool resourceValue)
                AllowsTransparency = resourceValue;

            InitializeComponent();
            
            AutoHideShowDelayMs = _settings.AutoHideShowDelayMs;
            RequiresScreenEdge = true;

            SetPosition();

            setupChildren();

            _settings.PropertyChanged += Settings_PropertyChanged;

            // Set element that should be transformed for auto-hide
            AutoHideElement = MenuBarWindow;
        }

        private void setupChildren()
        {
            stacksContainer.MenuBar = this;
            programsMenuControl.MenuBar = this;

            if (!_settings.EnableProgramsMenu)
                ProgramsMenu.Visibility = Visibility.Collapsed;

            if (!_settings.EnablePlacesMenu)
                PlacesMenu.Visibility = Visibility.Collapsed;
        }

        private void SetupMenuBarExtensions()
        {
            foreach (var userControlMenuBarExtension in _cairoApplication.MenuBarExtensions.OfType<UserControlMenuBarExtension>())
            {
                try
                {
                    var menuExtra = userControlMenuBarExtension.StartControl(this);
                    if (menuExtra != null)
                    {
                        MenuExtrasHost.Children.Add(menuExtra);
                    }
                }
                catch (Exception ex)
                {
                    ShellLogger.Error($"Unable to start UserControl menu bar extension due to exception in {ex.TargetSite?.Module}: {ex.Message}");
                }
            }
        }

        private void CairoMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (isCairoMenuInitialized)
            {
                return;
            }

            CairoMenu.Items.Clear();

            // TODO: Make configurable
            var cairoMenuItems = new string[]
            {
                "AboutCairo",
                "CheckForUpdates",
                "",
                "OpenAppGrabber",
                "OpenCairoSettings",
                "OpenControlPanel",
                "OpenWindowsSettings",
                "",
                "ShowRunDialog",
                "StartTaskManager",
                "",
                "ExitCairo",
                "",
                "Sleep",
                "Hibernate",
                "Restart",
                "ShutDown",
                "",
                "Lock",
                "LogOff"
            };

            foreach (var identifier in cairoMenuItems)
            {
                if (string.IsNullOrEmpty(identifier))
                {
                    CairoMenu.Items.Add(new Separator());
                    continue;
                }

                var command = _commandService.Commands.FirstOrDefault(cmd => cmd.Identifier == identifier);
                if (command == null)
                {
                    continue;
                }
                var menuItem = new MenuItem
                {
                    Header = command.Label,
                    Visibility = command.IsAvailable ? Visibility.Visible : Visibility.Collapsed
                };
                menuItem.Click += (_sender, _e) => _commandService.InvokeCommand(command.Identifier);
                if (command is INotifyPropertyChanged notifyPropertyChangedCommand)
                {
                    notifyPropertyChangedCommand.PropertyChanged += (_sender, _e) =>
                    {
                        if (_e.PropertyName == "Label")
                        {
                            menuItem.Header = command.Label;
                        }
                        if (_e.PropertyName == "IsAvailable")
                        {
                            menuItem.Visibility = command.IsAvailable ? Visibility.Visible : Visibility.Collapsed;
                            DeduplicateSeparators();
                        }
                    };
                }
                CairoMenu.Items.Add(menuItem);
            }

            // Add CairoMenu MenuItems
            // TODO: Extensions should instead define Commands, and the user can add them to the Cairo menu
            if (_cairoApplication.CairoMenu.Count > 0)
            {
                var insertIndex = Array.IndexOf(cairoMenuItems, "OpenWindowsSettings") + 1;
                CairoMenu.Items.Insert(insertIndex, new Separator());
                foreach (var cairoMenuItem in _cairoApplication.CairoMenu)
                {
                    insertIndex += 1;
                    var menuItem = new MenuItem { Header = cairoMenuItem.Header };
                    menuItem.Click += cairoMenuItem.MenuItem_Click;
                    CairoMenu.Items.Insert(insertIndex, menuItem);
                }
            }

            DeduplicateSeparators();

            isCairoMenuInitialized = true;
        }

        private void DeduplicateSeparators()
        {
            Type previousType = null;
            foreach (UIElement item in CairoMenu.Items)
            {
                Type currentType = item.GetType();
                if (previousType == typeof(Separator) && currentType == typeof(Separator))
                {
                    ((Separator)item).Visibility = Visibility.Collapsed;
                }
                else if (currentType == typeof(Separator))
                {
                    ((Separator)item).Visibility = Visibility.Visible;
                }

                if (item.Visibility == Visibility.Visible)
                {
                    previousType = currentType;
                }
            }
        }

        private void PlacesMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (isPlacesMenuInitialized)
            {
                return;
            }

            PlacesMenu.Items.Clear();

            // TODO: Make configurable
            var placesMenuItems = new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify),
                KnownFolders.GetPath(KnownFolder.Downloads),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic, Environment.SpecialFolderOption.DoNotVerify),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.DoNotVerify),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos, Environment.SpecialFolderOption.DoNotVerify),
                "",
                ShellFolderPath.ComputerFolder.Value,
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify),
                "",
                ShellFolderPath.RecycleBinFolder.Value
            };

            var boldItems = new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify),
                ShellFolderPath.ComputerFolder.Value
            };

            var openInWindowItems = new string[]
            {
                ShellFolderPath.RecycleBinFolder.Value
            };

            foreach (var path in placesMenuItems)
            {
                if (string.IsNullOrEmpty(path))
                {
                    PlacesMenu.Items.Add(new Separator());
                    continue;
                }

                var folder = new ShellFolder(path, IntPtr.Zero, true, false);
                if (!folder.Loaded)
                {
                    continue;
                }

                var menuItem = new MenuItem
                {
                    Header = folder.DisplayName.Replace("_", "__"),
                    FontWeight = boldItems.Contains(path) ? FontWeights.Bold : FontWeights.Normal
                };
                menuItem.Click += (_sender, _e) => _commandService.InvokeCommand(!_settings.FoldersOpenDesktopOverlay || openInWindowItems.Contains(path) ? "OpenLocationInWindow" : "OpenLocation", ("Path", path));
                PlacesMenu.Items.Add(menuItem);
            }

            // Add PlacesMenu MenuItems
            if (_cairoApplication.Places.Count > 0)
            {
                PlacesMenu.Items.Add(new Separator());
                foreach (var placesMenuItem in _cairoApplication.Places)
                {
                    var menuItem = new MenuItem { Header = placesMenuItem.Header };
                    menuItem.Click += placesMenuItem.MenuItem_Click;
                    PlacesMenu.Items.Add(menuItem);
                }
            }

            isPlacesMenuInitialized = true;
        }

        private void setupShadow()
        {
            if (_settings.EnableMenuBarShadow && shadow == null && AppBarMode != AppBarMode.AutoHide)
            {
                shadow = new MenuBarShadow(_cairoApplication, _windowManager, this);
                shadow.Show();
            }
        }

        protected override void OnSourceInitialized(object sender, EventArgs e)
        {
            base.OnSourceInitialized(sender, e);
            
            SetupMenuBarExtensions();

            registerCairoMenuHotKey();

            // Register L+R Windows key to open Programs menu
            if (EnvironmentHelper.IsAppRunningAsShell && Screen.Primary && programsMenuHotKeys.Count < 1)
            {
                /*if (keyboardListener == null)
                    keyboardListener = new LowLevelKeyboardListener();

                keyboardListener.OnKeyPressed += keyboardListener_OnKeyPressed;
                keyboardListener.HookKeyboard();*/

                programsMenuHotKeys.Add(new HotKey(Key.LWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.RWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.Escape, HotKeyModifier.Ctrl | HotKeyModifier.NoRepeat, OnShowProgramsMenu));
            }
            else if (EnvironmentHelper.IsAppRunningAsShell && Screen.Primary)
            {
                foreach (var hotkey in programsMenuHotKeys)
                {
                    hotkey.Action = OnShowProgramsMenu;
                }
            }

            SetBlur(_settings.EnableMenuBarBlur);

            setupShadow();
        }

        private void registerCairoMenuHotKey()
        {
            if (_settings.EnableCairoMenuHotKey && Screen.Primary && cairoMenuHotKey == null)
            {
                cairoMenuHotKey = HotKeyManager.RegisterHotKey(_settings.CairoMenuHotKey, OnShowCairoMenu);
            }
            else if (_settings.EnableCairoMenuHotKey && Screen.Primary)
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

        #region Programs menu
        internal void OpenProgramsMenu()
        {
            if (ProgramsMenu.IsSubmenuOpen || !_settings.EnableProgramsMenu) { return; }

            NativeMethods.SetForegroundWindow(Handle);
            UnAutoHideBeforeAction(() => ProgramsMenu.IsSubmenuOpen = true);
        }

        private void OnShowProgramsMenu(HotKey hotKey)
        {
            ToggleProgramsMenu();
        }

        private void ToggleProgramsMenu()
        {
            if (!ProgramsMenu.IsSubmenuOpen)
            {
                OpenProgramsMenu();
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
                _appGrabber.AddByPath(fileNames, AppCategoryType.Uncategorized);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;

                _appGrabber.AddByPath(dropData.Path, AppCategoryType.Uncategorized);
            }
        }
        #endregion

        #region Events

        public override void AfterAppBarPos(bool isSameCoords, NativeMethods.Rect rect)
        {
            base.AfterAppBarPos(isSameCoords, rect);

            if (!isSameCoords)
            {
                setShadowPosition();
            }
        }

        public override void SetPosition()
        {
            base.SetPosition();
            
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
            setShadowPosition();
        }

        protected override void CustomClosing()
        {
            if (!_windowManager.IsSettingDisplays && !AllowClose)
            {
                return;
            }

            closeShadow();
            _settings.PropertyChanged -= Settings_PropertyChanged;
        }

        private void UnAutoHideBeforeAction(Action action)
        {
            if (AppBarMode == AppBarMode.AutoHide && AllowAutoHide)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(AutoHideShowAnimationMs);
                timer.Tick += (object sender, EventArgs e) =>
                {
                    action();
                    timer.Stop();
                };

                PeekDuringAutoHide();
                timer.Start();
            }
            else
            {
                action();
            }
        }

        private void OnShowCairoMenu(HotKey hotKey)
        {
            if (!CairoMenu.IsSubmenuOpen)
            {
                NativeMethods.SetForegroundWindow(Handle);
                UnAutoHideBeforeAction(() => CairoMenu.IsSubmenuOpen = true);
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
                ShellLogger.Debug(e.Key.ToString() + " Key Pressed");
                ToggleProgramsMenu();
                e.Handled = true;
            }
        }

        private void MenuBar_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _appBarManager.NotifyAppBarEvent(this, AppBarEventReason.MouseEnter);
        }

        private void MenuBar_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _appBarManager.NotifyAppBarEvent(this, AppBarEventReason.MouseLeave);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "EnableCairoMenuHotKey" when Screen.Primary:
                        if (_settings.EnableCairoMenuHotKey)
                        {
                            registerCairoMenuHotKey();
                        }
                        else
                        {
                            unregisterCairoMenuHotKey();
                        }

                        break;
                    case "CairoMenuHotKey" when Screen.Primary:
                        if (_settings.EnableCairoMenuHotKey)
                        {
                            unregisterCairoMenuHotKey();
                            registerCairoMenuHotKey();
                        }

                        break;
                    case "EnableMenuBarBlur":
                        SetBlur(_settings.EnableMenuBarBlur);
                        break;
                    case "EnableMenuBarShadow":
                        if (_settings.EnableMenuBarShadow)
                        {
                            setupShadow();
                        }
                        else
                        {
                            closeShadow();
                        }
                        break;
                    case "EnableMenuBarAutoHide":
                        AppBarMode = _settings.EnableMenuBarAutoHide ? AppBarMode.AutoHide : AppBarMode.Normal;
                        if (AppBarMode == AppBarMode.AutoHide)
                        {
                            closeShadow();
                        }
                        else
                        {
                            setupShadow();
                        }
                        break;
                    case "AutoHideShowDelayMs":
                        AutoHideShowDelayMs = _settings.AutoHideShowDelayMs;
                        break;
                    case "Theme":
                        PeekDuringAutoHide();
                        break;
                    case "MenuBarEdge":
                        PeekDuringAutoHide();
                        AppBarEdge = _settings.MenuBarEdge;
                        SetScreenPosition();
                        if (EnvironmentHelper.IsAppRunningAsShell) _appBarManager.SetWorkArea(Screen);
                        break;
                }
            }
        }

        #endregion
        
        #region IMenuExtraHost
        public IntPtr GetHandle()
        {
            return Handle;
        }

        public bool GetIsPrimaryDisplay()
        {
            return Screen.Primary;
        }

        public MenuBarDimensions GetDimensions()
        {
            return new MenuBarDimensions
            {
                ScreenEdge = (int)AppBarEdge,
                DpiScale = DpiScale,
                Height = Height,
                Width = Width,
                Left = Left,
                Top = Top
            };
        }
        #endregion

        #region IMenuBar
        void IMenuBar.PeekDuringAutoHide(int msToPeek)
        {
            PeekDuringAutoHide(msToPeek);
        }
        #endregion
    }
}