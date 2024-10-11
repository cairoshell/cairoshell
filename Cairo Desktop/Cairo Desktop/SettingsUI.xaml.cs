using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using ManagedShell.Interop;
using CairoDesktop.SupportingClasses;
using ManagedShell;
using ManagedShell.WindowsTray;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CairoDesktop.Infrastructure.Services;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using ManagedShell.Common.Enums;
using CairoDesktop.Services;
using ManagedShell.AppBar;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for SettingsUI.xaml
    /// </summary>
    public partial class SettingsUI : Window
    {
        private readonly IAppGrabber _appGrabber;
        private readonly IApplicationUpdateService _applicationUpdateService;
        private readonly ICairoApplication _cairoApplication;
        private readonly ICommandService _commandService;
        private readonly ShellManager _shellManager;
        private readonly IThemeService _themeService;
        private readonly SettingsUIService _uiService;
        private readonly Settings _settings;

        internal SettingsUI(ICairoApplication cairoApplication,
            SettingsUIService uiService,
            ShellManagerService shellManagerService,
            IApplicationUpdateService applicationUpdateService,
            IAppGrabber appGrabber,
            IThemeService themeService,
            ICommandService commandService,
            Settings settings)
        {
            InitializeComponent();

            _appGrabber = appGrabber;
            _applicationUpdateService = applicationUpdateService;
            _cairoApplication = cairoApplication;
            _commandService = commandService;
            _shellManager = shellManagerService.ShellManager;
            _themeService = themeService;
            _uiService = uiService;
            _settings = settings;

            DataContext = _settings;

            loadThemes();
            loadLanguages();
            loadRadioGroups();
            loadCategories();
            loadDesktopBackgroundSettings();
            loadHotKeys();
            loadLoggingLevels();
            loadNotficationSettings();
            loadVersionDependentSettings();

            checkUpdateConfig();
            checkTrayStatus();
            checkRunAtLogOn();
            checkIfCanHibernate();

            _settings.PropertyChanged += Settings_PropertyChanged;
            _commandService = commandService;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DesktopLabelPosition":
                case "DesktopIconSize":
                case "MenuBarEdge":
                case "ProgramsMenuLayout":
                case "TaskbarMode":
                case "TaskbarEdge":
                case "TaskbarIconSize":
                case "TaskbarMiddleClick":
                case "TaskbarGroupingStyle":
                case "TaskbarMultiMonMode":
                case "SysTrayAlwaysExpanded":
                    loadRadioGroups();
                    break;
            }
        }

        private void loadRadioGroups()
        {
            switch (_settings.DesktopLabelPosition)
            {
                case 0:
                    radDesktopLabelPos0.IsChecked = true;
                    radDesktopLabelPos1.IsChecked = false;
                    break;
                case 1:
                    radDesktopLabelPos0.IsChecked = false;
                    radDesktopLabelPos1.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.DesktopIconSize)
            {
                case IconSize.Large:
                    radDesktopIconSize0.IsChecked = true;
                    radDesktopIconSize2.IsChecked = false;
                    break;
                case IconSize.ExtraLarge:
                    radDesktopIconSize0.IsChecked = false;
                    radDesktopIconSize2.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.MenuBarEdge)
            {
                case AppBarEdge.Top:
                    radMenuBarEdgeTop.IsChecked = true;
                    radMenuBarEdgeBottom.IsChecked = false;
                    break;
                case AppBarEdge.Bottom:
                    radMenuBarEdgeTop.IsChecked = false;
                    radMenuBarEdgeBottom.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.ProgramsMenuLayout)
            {
                case 0:
                    radProgramsMenuLayout0.IsChecked = true;
                    radProgramsMenuLayout1.IsChecked = false;
                    break;
                case 1:
                    radProgramsMenuLayout0.IsChecked = false;
                    radProgramsMenuLayout1.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarMode)
            {
                case 0:
                    radTaskbarMode0.IsChecked = true;
                    radTaskbarMode1.IsChecked = false;
                    radTaskbarMode2.IsChecked = false;
                    break;
                case 1:
                    radTaskbarMode0.IsChecked = false;
                    radTaskbarMode1.IsChecked = true;
                    radTaskbarMode2.IsChecked = false;
                    break;
                case 2:
                    radTaskbarMode0.IsChecked = false;
                    radTaskbarMode1.IsChecked = false;
                    radTaskbarMode2.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarEdge)
            {
                case AppBarEdge.Bottom:
                    radTaskbarPosBottom.IsChecked = true;
                    radTaskbarPosTop.IsChecked = false;
                    break;
                case AppBarEdge.Top:
                    radTaskbarPosBottom.IsChecked = false;
                    radTaskbarPosTop.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarIconSize)
            {
                case IconSize.Large:
                    radTaskbarSize0.IsChecked = true;
                    radTaskbarSize10.IsChecked = false;
                    radTaskbarSize1.IsChecked = false;
                    break;
                case IconSize.Small:
                    radTaskbarSize0.IsChecked = false;
                    radTaskbarSize10.IsChecked = false;
                    radTaskbarSize1.IsChecked = true;
                    break;
                case IconSize.Medium:
                    radTaskbarSize0.IsChecked = false;
                    radTaskbarSize10.IsChecked = true;
                    radTaskbarSize1.IsChecked = false;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarMiddleClick)
            {
                case 0:
                    radTaskbarMiddleClick0.IsChecked = true;
                    radTaskbarMiddleClick1.IsChecked = false;
                    break;
                case 1:
                    radTaskbarMiddleClick0.IsChecked = false;
                    radTaskbarMiddleClick1.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarGroupingStyle)
            {
                case 0:
                    radTaskbarGrouping0.IsChecked = true;
                    radTaskbarGrouping1.IsChecked = false;
                    radTaskbarGrouping2.IsChecked = false;
                    break;
                case 1:
                    radTaskbarGrouping0.IsChecked = false;
                    radTaskbarGrouping1.IsChecked = true;
                    radTaskbarGrouping2.IsChecked = false;
                    break;
                case 2:
                    radTaskbarGrouping0.IsChecked = false;
                    radTaskbarGrouping1.IsChecked = false;
                    radTaskbarGrouping2.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.TaskbarMultiMonMode)
            {
                case 0:
                    radTaskbarMultiMonMode0.IsChecked = true;
                    radTaskbarMultiMonMode1.IsChecked = false;
                    radTaskbarMultiMonMode2.IsChecked = false;
                    break;
                case 1:
                    radTaskbarMultiMonMode0.IsChecked = false;
                    radTaskbarMultiMonMode1.IsChecked = true;
                    radTaskbarMultiMonMode2.IsChecked = false;
                    break;
                case 2:
                    radTaskbarMultiMonMode0.IsChecked = false;
                    radTaskbarMultiMonMode1.IsChecked = false;
                    radTaskbarMultiMonMode2.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch (_settings.SysTrayAlwaysExpanded)
            {
                case false:
                    radTrayMode0.IsChecked = true;
                    radTrayMode1.IsChecked = false;
                    break;
                case true:
                    radTrayMode0.IsChecked = false;
                    radTrayMode1.IsChecked = true;
                    break;
            }
        }

        private void loadThemes()
        {
            foreach (var theme in _themeService.GetThemes())
            {
                cboThemeSelect.Items.Add(theme);
            }
        }

        private void loadLanguages()
        {
            cboLangSelect.DisplayMemberPath = "Key";
            cboLangSelect.SelectedValuePath = "Value";
            foreach (KeyValuePair<string, string> lang in Common.Localization.DisplayString.Languages)
            {
                cboLangSelect.Items.Add(lang);
            }
        }

        private void loadCategories()
        {
            foreach (Category cat in _appGrabber.CategoryList)
            {
                if (cat.ShowInMenu)
                {
                    string cboCat = cat.DisplayName;
                    cboDefaultProgramsCategory.Items.Add(cboCat);
                }
            }
        }

        private void loadHotKeys()
        {
            string[] modifiers = { "Win", "Shift", "Alt", "Ctrl" };
            string[] keys = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            cboCairoMenuHotKeyMod2.Items.Add("None");
            cboDesktopOverlayHotKeyMod2.Items.Add("None");

            foreach (string modifier in modifiers)
            {
                cboCairoMenuHotKeyMod1.Items.Add(modifier);
                cboCairoMenuHotKeyMod2.Items.Add(modifier);
                cboDesktopOverlayHotKeyMod1.Items.Add(modifier);
                cboDesktopOverlayHotKeyMod2.Items.Add(modifier);
            }

            foreach (string key in keys)
            {
                cboCairoMenuHotKeyKey.Items.Add(key);
                cboDesktopOverlayHotKeyKey.Items.Add(key);
            }
        }

        private void loadLoggingLevels()
        {
            foreach (string sev in Enum.GetNames(typeof(LogSeverity)))
            {
                cboLogSeverity.Items.Add(sev);
            }
        }

        private void loadNotficationSettings()
        {
            if (_settings.EnableSysTray)
            {
                PinnedIcons.ItemsSource = _shellManager.NotificationArea.PinnedIcons;
                UnpinnedIcons.ItemsSource = _shellManager.NotificationArea.UnpinnedIcons;
            }
            else
            {
                pnlTraySettings.Visibility = Visibility.Collapsed;
            }
        }

        private void loadVersionDependentSettings()
        {
            if (!EnvironmentHelper.IsWindows10OrBetter)
            {
                chkEnableMenuBarBlur.Visibility = Visibility.Collapsed;
                chkEnableMenuExtraActionCenter.Visibility = Visibility.Collapsed;
                chkEnableMenuExtraVolume.Visibility = Visibility.Collapsed;
            }
            else if (EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell)
            {
                chkEnableMenuExtraVolume.Visibility = Visibility.Collapsed;
            }
            else if (EnvironmentHelper.IsWindows10OrBetter && EnvironmentHelper.IsAppRunningAsShell)
            {
                chkEnableMenuExtraActionCenter.Visibility = Visibility.Collapsed;
            }
        }


        #region Notification area icon drag and drop

        private Point? startPoint = null;
        private bool inDrag = false;

        // receive drop functions
        private void UnpinnedIcons_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                if (dropData.IsPinned) e.Effects = DragDropEffects.Move;
                else e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void PinnedIcons_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void UnpinnedIcons_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                if (dropData.IsPinned)
                {
                    dropData.Unpin();
                    _settings.PinnedNotifyIcons = _shellManager.NotificationArea.PinnedNotifyIcons;
                }
            }

            e.Handled = true;
        }

        private void PinnedIcons_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                dropData.Pin();
                _settings.PinnedNotifyIcons = _shellManager.NotificationArea.PinnedNotifyIcons;
            }

            e.Handled = true;
        }

        private void icon_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                Border border = sender as Border;
                NotifyIcon existingNotifyIcon = border.DataContext as NotifyIcon;
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                // allow move if we are dragging into pinned items, out of pinned items, or we are rearranging inside pinned items
                if (dropData.IsPinned || existingNotifyIcon.IsPinned) e.Effects = DragDropEffects.Move;
                else e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void icon_Drop(object sender, DragEventArgs e)
        {
            Border border = sender as Border;
            NotifyIcon existingNotifyIcon = border.DataContext as NotifyIcon;

            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                if (existingNotifyIcon.IsPinned)
                {
                    // pinned area; allow sorting
                    dropData.Pin(existingNotifyIcon.PinOrder);
                }
                else
                {
                    // non-pinned icons area
                    dropData.Unpin();
                }

                _settings.PinnedNotifyIcons = _shellManager.NotificationArea.PinnedNotifyIcons;
            }

            e.Handled = true;
        }

        // send drag functions
        private void icon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(this);
        }

        private void icon_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!inDrag && startPoint != null)
            {
                inDrag = true;

                Point mousePos = e.GetPosition(this);
                Vector diff = (Point)startPoint - mousePos;

                if (mousePos.Y <= ActualHeight && ((Point)startPoint).Y <= ActualHeight && e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Border border = sender as Border;
                    NotifyIcon notifyIcon = border.DataContext as NotifyIcon;

                    try
                    {
                        DragDrop.DoDragDrop(border, notifyIcon, DragDropEffects.Move);
                    }
                    catch { }

                    // reset the stored mouse position
                    startPoint = null;
                }
                else if (e.LeftButton != MouseButtonState.Pressed)
                {
                    // reset the stored mouse position
                    startPoint = null;
                }

                inDrag = false;
            }

            e.Handled = true;
        }

        #endregion

        #region Startup checks
        private void checkUpdateConfig()
        {
            if (_applicationUpdateService.IsAvailable)
            {
                chkEnableAutoUpdates.IsChecked = _applicationUpdateService.AutomaticUpdatesEnabled;
            }
            else
            {
                chkEnableAutoUpdates.Visibility = Visibility.Collapsed;
            }
        }

        private void checkTrayStatus()
        {
            if (!_settings.EnableTaskbar && !EnvironmentHelper.IsAppRunningAsShell)
            {
                // if taskbar is disabled and we aren't running as shell, then Explorer tray is visible. Show warning.
                lblTrayTaskbarWarning.Visibility = Visibility.Visible;
            }
        }

        private void checkRunAtLogOn()
        {
            if (EnvironmentHelper.IsAppConfiguredAsShell.Equals(true))
            {
                chkRunAtLogOn.Visibility = Visibility.Collapsed;
            }

            try
            {
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                List<string> rKeyValueNames = rKey?.GetValueNames().ToList();

                if (rKeyValueNames != null)
                {
                    if (rKeyValueNames.Contains("CairoShell"))
                    {
                        chkRunAtLogOn.IsChecked = true;
                    }
                    else
                    {
                        chkRunAtLogOn.IsChecked = false;
                    }
                }
            }
            catch (Exception e)
            {
                ShellLogger.Error($"SettingsWindow: Unable to load autorun setting from registry: {e.Message}");
            }
        }

        private void checkIfCanHibernate()
        {
            if (!PowerHelper.CanHibernate())
            {
                chkShowHibernate.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void EnableAutoUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (chkEnableAutoUpdates.IsChecked != null)
            {
                _applicationUpdateService.AutomaticUpdatesEnabled = (bool)chkEnableAutoUpdates.IsChecked;
            }
            else
            {
                _applicationUpdateService.AutomaticUpdatesEnabled = false;
            }
        }

        private void ShowRestartButton()
        {
            btnRestart.Visibility = Visibility.Visible;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ShowRestartButton();
        }

        private void DropDown_Changed(object sender, EventArgs e)
        {
            ShowRestartButton();
        }

        private void RestartCairo(object sender, RoutedEventArgs e)
        {
            saveChanges();

            _cairoApplication.RestartCairo();
        }

        /// <summary>
        /// Handles the Closing event of the window to save the settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments for the event.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveChanges();
            _uiService.SettingsUi = null;
        }

        private void saveChanges()
        {
            // placeholder in case we need to do extra work in the future
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);

            cboDesktopBackgroundType_SelectionChanged(null, null);

            NativeMethods.SetForegroundWindow(helper.Handle);

        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)NativeMethods.WM.SETTINGCHANGE &&
                wParam.ToInt32() == (int)NativeMethods.SPI.SETDESKWALLPAPER)
            {
                loadDesktopBackgroundSettings();

                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private void btnDesktopHomeSelect_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = Common.Localization.DisplayString.sDesktop_BrowseTitle;
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = _settings.DesktopDirectory;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    _settings.DesktopDirectory = fbd.SelectedPath;
                    txtDesktopHome.Text = fbd.SelectedPath;
                }
            }
        }

        private void radMenuBarEdgeTop_Click(object sender, RoutedEventArgs e)
        {
            _settings.MenuBarEdge = AppBarEdge.Top;
        }

        private void radMenuBarEdgeBottom_Click(object sender, RoutedEventArgs e)
        {
            _settings.MenuBarEdge = AppBarEdge.Bottom;
        }

        private void radProgramsMenuLayout0_Click(object sender, RoutedEventArgs e)
        {
            _settings.ProgramsMenuLayout = 0;
        }

        private void radProgramsMenuLayout1_Click(object sender, RoutedEventArgs e)
        {
            _settings.ProgramsMenuLayout = 1;
        }

        private void radTaskbarMode0_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMode = 0;
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMode = 1;
        }

        private void radTaskbarMode2_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMode = 2;
        }

        private void radTaskbarPosBottom_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarEdge = AppBarEdge.Bottom;
        }

        private void radTaskbarPosTop_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarEdge = AppBarEdge.Top;
        }

        private void radTaskbarSize0_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarIconSize = IconSize.Large;
        }

        private void radTaskbarSize1_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarIconSize = IconSize.Small;
        }

        private void radTaskbarSize10_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarIconSize = IconSize.Medium;
        }

        private void radTaskbarMiddleClick0_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMiddleClick = 0;
        }

        private void radTaskbarMiddleClick1_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMiddleClick = 1;
        }

        private void radTaskbarGrouping0_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarGroupingStyle = 0;
        }

        private void radTaskbarGrouping1_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarGroupingStyle = 1;
        }

        private void radTaskbarGrouping2_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarGroupingStyle = 2;
        }

        private void radTaskbarMultiMonMode0_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMultiMonMode = 0;
        }

        private void radTaskbarMultiMonMode1_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMultiMonMode = 1;
        }

        private void radTaskbarMultiMonMode2_Click(object sender, RoutedEventArgs e)
        {
            _settings.TaskbarMultiMonMode = 2;
        }

        private void radDesktopLabelPos0_Click(object sender, RoutedEventArgs e)
        {
            _settings.DesktopLabelPosition = 0;
        }

        private void radDesktopLabelPos1_Click(object sender, RoutedEventArgs e)
        {
            _settings.DesktopLabelPosition = 1;
        }

        private void radDesktopIconSize0_Click(object sender, RoutedEventArgs e)
        {
            _settings.DesktopIconSize = IconSize.Large;
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            _settings.DesktopIconSize = IconSize.ExtraLarge;
        }

        private void radTrayMode0_Click(object sender, RoutedEventArgs e)
        {
            _settings.SysTrayAlwaysExpanded = false;
        }

        private void radTrayMode1_Click(object sender, RoutedEventArgs e)
        {
            _settings.SysTrayAlwaysExpanded = true;
        }

        private void cboCairoMenuHotKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCairoMenuHotKeyMod1.SelectedValue == null || cboCairoMenuHotKeyMod2.SelectedValue == null || cboCairoMenuHotKeyKey.SelectedValue == null)
            {
                return;
            }
            string[] hotkey = { cboCairoMenuHotKeyMod1.SelectedValue.ToString(), cboCairoMenuHotKeyMod2.SelectedValue.ToString(), cboCairoMenuHotKeyKey.SelectedValue.ToString() };

            if (_settings.CairoMenuHotKey[0] == hotkey[0] && _settings.CairoMenuHotKey[1] == hotkey[1] && _settings.CairoMenuHotKey[2] == hotkey[2])
            {
                return;
            }
            _settings.CairoMenuHotKey = hotkey;
        }

        private void cboDesktopOverlayHotKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDesktopOverlayHotKeyMod1.SelectedValue == null || cboDesktopOverlayHotKeyMod2.SelectedValue == null || cboDesktopOverlayHotKeyKey.SelectedValue == null)
            {
                return;
            }
            string[] hotkey = { cboDesktopOverlayHotKeyMod1.SelectedValue.ToString(), cboDesktopOverlayHotKeyMod2.SelectedValue.ToString(), cboDesktopOverlayHotKeyKey.SelectedValue.ToString() };

            if (_settings.DesktopOverlayHotKey[0] == hotkey[0] && _settings.DesktopOverlayHotKey[1] == hotkey[1] && _settings.DesktopOverlayHotKey[2] == hotkey[2])
            {
                return;
            }
            _settings.DesktopOverlayHotKey = hotkey;
        }

        private void btnChangeShell_Click(object sender, RoutedEventArgs e)
        {
            btnChangeShell.IsEnabled = false;

            EnvironmentHelper.IsAppConfiguredAsShell = !EnvironmentHelper.IsAppConfiguredAsShell;

            CairoMessage.ShowOkCancel(Common.Localization.DisplayString.sSettings_Advanced_ShellChangedText,
                Common.Localization.DisplayString.sSettings_Advanced_ShellChanged, CairoMessageImage.LogOff,
                Common.Localization.DisplayString.sSettings_Advanced_LogOffNow, Common.Localization.DisplayString.sSettings_Advanced_LogOffLater,
                result =>
                {
                    if (result == true)
                        ShellHelper.Logoff();
                });
        }

        private void btnOpenLogsFolder_OnClick(object sender, RoutedEventArgs e)
        {
            _commandService.InvokeCommand("OpenLocationInWindow", ("Path", CairoApplication.LogsFolder));
        }

        private void chkRunAtLogOn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey rKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                var chkBox = (CheckBox)sender;

                if (chkBox.IsChecked.Equals(false))
                {
                    //Delete SubKey
                    rKey.DeleteValue("CairoShell");
                }
                else
                {
                    string exePath = ExePath.GetExecutablePath();
                    //Write SubKey
                    rKey.SetValue("CairoShell", exePath);
                }
            }
            catch (Exception exception)
            {
                ShellLogger.Error($"SettingsWindow: Unable to update registry autorun setting: {exception.Message}");
            }
        }

        #region Desktop Background
        private void loadDesktopBackgroundSettings()
        {
            cboDesktopBackgroundType.Items.Clear();
            cboWindowsBackgroundStyle.Items.Clear();
            cboCairoBackgroundStyle.Items.Clear();
            cboBingBackgroundStyle.Items.Clear();

            #region windowsDefaultBackground

            ComboBoxItem windowsDefaultBackgroundItem = new ComboBoxItem()
            {
                Name = "windowsDefaultBackground",
                Content = Common.Localization.DisplayString.sSettings_Desktop_BackgroundType_windowsDefaultBackground,
                Tag = windowsImageBackgroundStackPanel
            };

            cboDesktopBackgroundType.Items.Add(windowsDefaultBackgroundItem);

            // draw wallpaper
            string regWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", "") as string;
            string regWallpaperStyle = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", "") as string;
            string regTileWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", "") as string;

            CairoWallpaperStyle style = CairoWallpaperStyle.Stretch;
            // https://docs.microsoft.com/en-us/windows/desktop/Controls/themesfileformat-overview
            switch ($"{regWallpaperStyle}{regTileWallpaper}")
            {
                case "01": // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                    style = CairoWallpaperStyle.Tile;
                    break;
                case "00": // Centered { WallpaperStyle = 1; TileWallpaper = 0 }
                    style = CairoWallpaperStyle.Center;
                    break;
                case "60": // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                    style = CairoWallpaperStyle.Fit;
                    break;
                case "100": // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                    style = CairoWallpaperStyle.Fill;
                    break;
                case "220": // Span { WallpaperStyle = 10; TileWallpaper = 0 }
                    style = CairoWallpaperStyle.Span;
                    break;
                case "20": // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
                default:
                    style = CairoWallpaperStyle.Stretch;
                    break;
            }

            txtWindowsBackgroundPath.Text = regWallpaper;

            #endregion

            foreach (var backgroundStyleItem in Enum.GetValues(typeof(CairoWallpaperStyle)).Cast<CairoWallpaperStyle>())
            {
                string display;

                switch (backgroundStyleItem)
                {
                    case CairoWallpaperStyle.Center:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Center;
                        break;
                    case CairoWallpaperStyle.Fill:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Fill;
                        break;
                    case CairoWallpaperStyle.Fit:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Fit;
                        break;
                    case CairoWallpaperStyle.Span:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Span;
                        break;
                    case CairoWallpaperStyle.Stretch:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Stretch;
                        break;
                    case CairoWallpaperStyle.Tile:
                        display = Common.Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Tile;
                        break;
                    default:
                        display = backgroundStyleItem.ToString();
                        break;
                }

                // windows
                ComboBoxItem cboWindowsItem = new ComboBoxItem()
                {
                    Tag = backgroundStyleItem,
                    Content = display
                };
                cboWindowsBackgroundStyle.Items.Add(cboWindowsItem);

                if (backgroundStyleItem == style) cboWindowsItem.IsSelected = true;

                if (EnvironmentHelper.IsAppRunningAsShell)
                {
                    // image
                    ComboBoxItem cboImageItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = display
                    };
                    cboCairoBackgroundStyle.Items.Add(cboImageItem);

                    if (backgroundStyleItem == (CairoWallpaperStyle)_settings.CairoBackgroundImageStyle) cboImageItem.IsSelected = true;

                    // bing
                    ComboBoxItem cboBingItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = display
                    };
                    cboBingBackgroundStyle.Items.Add(cboBingItem);

                    if (backgroundStyleItem == (CairoWallpaperStyle)_settings.BingWallpaperStyle) cboBingItem.IsSelected = true;
                }
            }

            if (EnvironmentHelper.IsAppRunningAsShell)
            {
                #region  cairoImageWallpaper
                ComboBoxItem cairoImageWallpaperItem = new ComboBoxItem()
                {
                    Name = "cairoImageWallpaper",
                    Content = Common.Localization.DisplayString.sSettings_Desktop_BackgroundType_cairoImageWallpaper,
                    Tag = cairoImageBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(cairoImageWallpaperItem);
                txtCairoBackgroundPath.Text = _settings.CairoBackgroundImagePath;
                #endregion

                #region  cairoVideoWallpaper
                ComboBoxItem cairoVideoWallpaperItem = new ComboBoxItem()
                {
                    Name = "cairoVideoWallpaper",
                    Content = Common.Localization.DisplayString.sSettings_Desktop_BackgroundType_cairoVideoWallpaper,
                    Tag = cairoVideoBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(cairoVideoWallpaperItem);
                txtCairoVideoBackgroundPath.Text = _settings.CairoBackgroundVideoPath;
                #endregion

                #region  bingWallpaper
                ComboBoxItem bingWallpaperItem = new ComboBoxItem()
                {
                    Name = "bingWallpaper",
                    Content = Common.Localization.DisplayString.sSettings_Desktop_BackgroundType_bingWallpaper,
                    Tag = bingImageBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(bingWallpaperItem);
                #endregion

                var listBoxItems = cboDesktopBackgroundType.Items.Cast<ComboBoxItem>().ToList();
                var listBoxItem = listBoxItems.FirstOrDefault(l => l.Name == _settings.DesktopBackgroundType);

                cboDesktopBackgroundType.SelectedItem = listBoxItem;
            }
            else
            {
                cboDesktopBackgroundType.SelectedItem = windowsDefaultBackgroundItem;
                desktopBackgroundTypeStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void btnWindowsBackgroundFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = GetImageFilter()
            })
            {
                if (dlg.SafeShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtWindowsBackgroundPath.Text = dlg.FileName;
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", dlg.FileName);

                    NativeMethods.SystemParametersInfo(NativeMethods.SPI.SETDESKWALLPAPER, 0, dlg.FileName, (NativeMethods.SPIF.UPDATEINIFILE | NativeMethods.SPIF.SENDWININICHANGE));
                }
            }
        }

        private void btnCairoBackgroundFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = GetImageFilter()
            })
            {
                if (dlg.SafeShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtCairoBackgroundPath.Text = dlg.FileName;
                    _settings.CairoBackgroundImagePath = dlg.FileName;
                }
            }
        }

        private void btnCairoVideoFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = GetVideoFilter()
            })
            {
                if (dlg.SafeShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (Path.GetExtension(dlg.FileName) == ".zip")
                    {
                        string[] exts = new[] { ".wmv", ".3g2", ".3gp", ".3gp2", ".3gpp", ".amv", ".asf", ".avi", ".bin", ".cue", ".divx", ".dv", ".flv", ".gxf", ".iso", ".m1v", ".m2v", ".m2t", ".m2ts", ".m4v", ".mkv", ".mov", ".mp2", ".mp2v", ".mp4", ".mp4v", ".mpa", ".mpe", ".mpeg", ".mpeg1", ".mpeg2", ".mpeg4", ".mpg", ".mpv2", ".mts", ".nsv", ".nuv", ".ogg", ".ogm", ".ogv", ".ogx", ".ps", ".rec", ".rm", ".rmvb", ".tod", ".ts", ".tts", ".vob", ".vro", ".webm", ".wmv" };

                        FileInfo fileInfo = new FileInfo(dlg.FileName);
                        string extractPath = fileInfo.FullName.Remove((fileInfo.FullName.Length - fileInfo.Extension.Length), fileInfo.Extension.Length);

                        if (Directory.Exists(extractPath) && MessageBox.Show("Path already exists, overwrite?", "DreamScene", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Directory.Delete(extractPath, true);
                        }

                        if (!Directory.Exists(extractPath))
                        {
                            ZipFile.ExtractToDirectory(dlg.FileName, extractPath);
                        }

                        string file = Directory.GetFiles(extractPath).FirstOrDefault(f => exts.Contains(Path.GetExtension(f)));
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            dlg.FileName = file;
                        }
                    }

                    txtCairoVideoBackgroundPath.Text = dlg.FileName;
                    _settings.CairoBackgroundVideoPath = dlg.FileName;
                }
            }
        }

        private string GetImageFilter()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Image Files|");
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                stringBuilder.Append($"{codec.FilenameExtension};");
            }

            return stringBuilder.ToString();
        }

        private string GetVideoFilter()
        {
            return "All Videos Files |*.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp;" +
                " *.amv; *.asf;  *.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf;" +
                " *.iso; *.m1v; *.m2v; *.m2t; *.m2ts; *.m4v; *.mkv; *.mov; *.mp2;" +
                " *.mp2v; *.mp4; *.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4;" +
                " *.mpg; *.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps;" +
                " *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm | DreamScene | *.zip";
        }

        private void cboDesktopBackgroundType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDesktopBackgroundType.SelectedItem != null)
            {
                ComboBoxItem item = cboDesktopBackgroundType.SelectedItem as ComboBoxItem;
                windowsImageBackgroundStackPanel.Visibility = (item.Tag == windowsImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
                cairoImageBackgroundStackPanel.Visibility = (item.Tag == cairoImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
                cairoVideoBackgroundStackPanel.Visibility = (item.Tag == cairoVideoBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
                bingImageBackgroundStackPanel.Visibility = (item.Tag == bingImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;

                _settings.DesktopBackgroundType = item.Name;
            }
        }

        private void cboWindowsBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboWindowsBackgroundStyle.SelectedItem != null)
            {
                // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
                string wallpaperStyle = "2";
                string tileWallpaper = "0";
                string origWallpaperStyle = Registry
                    .GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", wallpaperStyle).ToString();
                string origTileWallpaper = Registry
                    .GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", tileWallpaper).ToString();

                switch ((cboWindowsBackgroundStyle.SelectedItem as ComboBoxItem).Tag)
                {
                    case CairoWallpaperStyle.Tile: // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                        wallpaperStyle = "0";
                        tileWallpaper = "1";
                        break;
                    case CairoWallpaperStyle.Center: // Centered { WallpaperStyle = 0; TileWallpaper = 0 }
                        wallpaperStyle = "0";
                        tileWallpaper = "0";
                        break;
                    case CairoWallpaperStyle.Fit: // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                        wallpaperStyle = "6";
                        tileWallpaper = "0";
                        break;
                    case CairoWallpaperStyle.Fill: // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                        wallpaperStyle = "10";
                        tileWallpaper = "0";
                        break;
                    case CairoWallpaperStyle.Span: // Span { WallpaperStyle = 22; TileWallpaper = 0 }
                        wallpaperStyle = "22";
                        tileWallpaper = "0";
                        break;
                }

                // since we run here when settings opens, don't set background if nothing changed
                if (origWallpaperStyle != wallpaperStyle || origTileWallpaper != tileWallpaper)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", wallpaperStyle);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", tileWallpaper);

                    NativeMethods.SystemParametersInfo(NativeMethods.SPI.SETDESKWALLPAPER, 0,
                        txtWindowsBackgroundPath.Text,
                        (NativeMethods.SPIF.UPDATEINIFILE | NativeMethods.SPIF.SENDWININICHANGE));
                }
            }
        }

        private void cboCairoBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCairoBackgroundStyle.SelectedItem != null)
            {
                int selected = (int)(cboCairoBackgroundStyle.SelectedItem as ComboBoxItem).Tag;
                if (_settings.CairoBackgroundImageStyle != selected)
                    _settings.CairoBackgroundImageStyle = selected;
            }
        }

        private void cboBingBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboBingBackgroundStyle.SelectedItem != null)
            {
                int selected = (int)(cboBingBackgroundStyle.SelectedItem as ComboBoxItem).Tag;
                if (_settings.BingWallpaperStyle != selected) _settings.BingWallpaperStyle = selected;
            }
        }

        #endregion
    }
}