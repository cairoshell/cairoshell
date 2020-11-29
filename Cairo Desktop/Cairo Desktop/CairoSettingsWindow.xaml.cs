using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using CairoDesktop.WindowsTray;
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
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        private static CairoSettingsWindow _instance = null;

        private readonly IApplicationUpdateService _applicationUpdateService;


        static CairoSettingsWindow() { }

        private CairoSettingsWindow(IApplicationUpdateService applicationUpdateService)
        {
            InitializeComponent();

            _applicationUpdateService = applicationUpdateService;

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
        }

        private void loadRadioGroups()
        {
            switch (Settings.Instance.DesktopLabelPosition)
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

            switch ((IconSize)Settings.Instance.DesktopIconSize)
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

            switch (Settings.Instance.ProgramsMenuLayout)
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

            switch (Settings.Instance.TaskbarMode)
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

            switch (Settings.Instance.TaskbarPosition)
            {
                case 0:
                    radTaskbarPos0.IsChecked = true;
                    radTaskbarPos1.IsChecked = false;
                    break;
                case 1:
                    radTaskbarPos0.IsChecked = false;
                    radTaskbarPos1.IsChecked = true;
                    break;
                default:
                    break;
            }

            switch ((IconSize)Settings.Instance.TaskbarIconSize)
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

            switch (Settings.Instance.TaskbarMiddleClick)
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

            switch (Settings.Instance.SysTrayAlwaysExpanded)
            {
                case false:
                    radTrayMode0.IsChecked = true;
                    radTrayMode1.IsChecked = false;
                    break;
                case true:
                    radTrayMode0.IsChecked = false;
                    radTrayMode1.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void loadThemes()
        {
            cboThemeSelect.Items.Add("Default");

            foreach (string subStr in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(s => Path.GetExtension(s).Contains("xaml")))
            {
                string theme = Path.GetFileName(subStr);
                cboThemeSelect.Items.Add(theme);
            }
        }

        private void loadLanguages()
        {
            cboLangSelect.DisplayMemberPath = "Key";
            cboLangSelect.SelectedValuePath = "Value";
            foreach (KeyValuePair<string, string> lang in Localization.DisplayString.Languages)
            {
                cboLangSelect.Items.Add(lang);
            }
        }

        private void loadCategories()
        {
            foreach (Category cat in AppGrabber.AppGrabber.Instance.CategoryList)
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
            foreach (string sev in Enum.GetNames(typeof(Common.Logging.LogSeverity)))
            {
                cboLogSeverity.Items.Add(sev);
            }
        }

        private void loadNotficationSettings()
        {
            if (Settings.Instance.EnableSysTray)
            {
                PinnedIcons.ItemsSource = NotificationArea.Instance.PinnedIcons;
                UnpinnedIcons.ItemsSource = NotificationArea.Instance.UnpinnedIcons;
            }
            else
            {
                pnlTraySettings.Visibility = Visibility.Collapsed;
            }
        }

        private void loadVersionDependentSettings()
        {
            if (!Shell.IsWindows10OrBetter)
            {
                chkEnableMenuBarBlur.Visibility = Visibility.Collapsed;
                chkEnableMenuExtraActionCenter.Visibility = Visibility.Collapsed;
                chkEnableMenuExtraVolume.Visibility = Visibility.Collapsed;
            }
            else if (Shell.IsWindows10OrBetter && !Shell.IsCairoRunningAsShell)
            {
                chkEnableMenuExtraVolume.Visibility = Visibility.Collapsed;
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

                if (dropData.IsPinned) dropData.Unpin();
            }

            e.Handled = true;
        }

        private void PinnedIcons_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NotifyIcon)))
            {
                NotifyIcon dropData = e.Data.GetData(typeof(NotifyIcon)) as NotifyIcon;

                dropData.Pin(Settings.Instance.PinnedNotifyIcons.Length);
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
            chkEnableAutoUpdates.IsChecked = _applicationUpdateService.AutomaticUpdatesEnabled;
        }

        private void checkTrayStatus()
        {
            if (NotificationArea.Instance.IsFailed)
            {
                // adjust settings window to alert user they need to install vc_redist

                chkEnableSysTray.IsEnabled = false;
                pnlTraySettings.Visibility = Visibility.Collapsed;

                lblTrayWarning.Visibility = Visibility.Visible;
            }
            else if (!Settings.Instance.EnableTaskbar && !Shell.IsCairoRunningAsShell)
            {
                // if taskbar is disabled and we aren't running as shell, then Explorer tray is visible. Show warning.
                lblTrayTaskbarWarning.Visibility = Visibility.Visible;
            }
        }

        private void checkRunAtLogOn()
        {
            if (Shell.IsCairoConfiguredAsShell.Equals(true))
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
                CairoLogger.Instance.Error($"SettingsWindow: Unable to load autorun setting from registry: {e.Message}");
            }
        }

        private void checkIfCanHibernate()
        {
            if (!Shell.CanHibernate())
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

            CairoApplication.Current.RestartCairo();
        }

        /// <summary>
        /// Handles the Closing event of the window to save the settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments for the event.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveChanges();
            _instance = null;
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
            fbd.Description = Localization.DisplayString.sDesktop_BrowseTitle;
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = Settings.Instance.DesktopDirectory;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    Settings.Instance.DesktopDirectory = fbd.SelectedPath;
                    txtDesktopHome.Text = fbd.SelectedPath;
                }
            }
        }

        private void radProgramsMenuLayout0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.ProgramsMenuLayout = 0;
        }

        private void radProgramsMenuLayout1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.ProgramsMenuLayout = 1;
        }

        private void radTaskbarMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 0;
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 1;
        }

        private void radTaskbarMode2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 2;
        }

        private void radTaskbarPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarPosition = 0;
        }

        private void radTaskbarPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarPosition = 1;
        }

        private void radTaskbarSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = (int)IconSize.Large;
        }

        private void radTaskbarSize1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = (int)IconSize.Small;
        }

        private void radTaskbarSize10_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = (int)IconSize.Medium;
        }

        private void radTaskbarMiddleClick0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMiddleClick = 0;
        }

        private void radTaskbarMiddleClick1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMiddleClick = 1;
        }

        private void radDesktopLabelPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopLabelPosition = 0;
        }

        private void radDesktopLabelPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopLabelPosition = 1;
        }

        private void radDesktopIconSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopIconSize = (int)IconSize.Large;
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopIconSize = (int)IconSize.ExtraLarge;
        }

        private void radTrayMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SysTrayAlwaysExpanded = false;
        }

        private void radTrayMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SysTrayAlwaysExpanded = true;
        }

        private void cboCairoMenuHotKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] hotkey = { cboCairoMenuHotKeyMod1.SelectedValue.ToString(), cboCairoMenuHotKeyMod2.SelectedValue.ToString(), cboCairoMenuHotKeyKey.SelectedValue.ToString() };
            Settings.Instance.CairoMenuHotKey = hotkey;
        }

        private void cboDesktopOverlayHotKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] hotkey = { cboDesktopOverlayHotKeyMod1.SelectedValue.ToString(), cboDesktopOverlayHotKeyMod2.SelectedValue.ToString(), cboDesktopOverlayHotKeyKey.SelectedValue.ToString() };
            Settings.Instance.DesktopOverlayHotKey = hotkey;
        }

        private void btnChangeShell_Click(object sender, RoutedEventArgs e)
        {
            btnChangeShell.IsEnabled = false;

            Shell.IsCairoConfiguredAsShell = !Shell.IsCairoConfiguredAsShell;

            CairoMessage.ShowOkCancel(Localization.DisplayString.sSettings_Advanced_ShellChangedText,
                Localization.DisplayString.sSettings_Advanced_ShellChanged, CairoMessageImage.LogOff,
                Localization.DisplayString.sSettings_Advanced_LogOffNow, Localization.DisplayString.sSettings_Advanced_LogOffLater,
                result =>
                {
                    if (result == true)
                        Shell.Logoff();
                });
        }

        private void btnOpenLogsFolder_OnClick(object sender, RoutedEventArgs e)
        {
            FolderHelper.OpenWithShell("%localappdata%\\Cairo_Development_Team\\Logs");
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
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    //Write SubKey
                    rKey.SetValue("CairoShell", exePath);
                }
            }
            catch (Exception exception)
            {
                CairoLogger.Instance.Error($"SettingsWindow: Unable to update registry autorun setting: {exception.Message}");
            }
        }

        public static CairoSettingsWindow Instance
        {
            get
            {
                return _instance ?? (_instance = new CairoSettingsWindow(
                    (IApplicationUpdateService) CairoApplication.Current.Host.Services.GetService(
                        typeof(IApplicationUpdateService))));
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
                Content = Localization.DisplayString.sSettings_Desktop_BackgroundType_windowsDefaultBackground,
                Tag = windowsImageBackgroundStackPanel
            };

            cboDesktopBackgroundType.Items.Add(windowsDefaultBackgroundItem);

            // draw wallpaper
            string regWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", "") as string;
            string regWallpaperStyle = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", "") as string;
            string regTileWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", "") as string;

            Desktop.CairoWallpaperStyle style = Desktop.CairoWallpaperStyle.Stretch;
            // https://docs.microsoft.com/en-us/windows/desktop/Controls/themesfileformat-overview
            switch ($"{regWallpaperStyle}{regTileWallpaper}")
            {
                case "01": // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                    style = Desktop.CairoWallpaperStyle.Tile;
                    break;
                case "00": // Centered { WallpaperStyle = 1; TileWallpaper = 0 }
                    style = Desktop.CairoWallpaperStyle.Center;
                    break;
                case "60": // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                    style = Desktop.CairoWallpaperStyle.Fit;
                    break;
                case "100": // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                    style = Desktop.CairoWallpaperStyle.Fill;
                    break;
                case "220": // Span { WallpaperStyle = 10; TileWallpaper = 0 }
                    style = Desktop.CairoWallpaperStyle.Span;
                    break;
                case "20": // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
                default:
                    style = Desktop.CairoWallpaperStyle.Stretch;
                    break;
            }

            txtWindowsBackgroundPath.Text = regWallpaper;

            #endregion

            foreach (var backgroundStyleItem in Enum.GetValues(typeof(Desktop.CairoWallpaperStyle)).Cast<Desktop.CairoWallpaperStyle>())
            {
                string display;

                switch (backgroundStyleItem)
                {
                    case Desktop.CairoWallpaperStyle.Center:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Center;
                        break;
                    case Desktop.CairoWallpaperStyle.Fill:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Fill;
                        break;
                    case Desktop.CairoWallpaperStyle.Fit:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Fit;
                        break;
                    case Desktop.CairoWallpaperStyle.Span:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Span;
                        break;
                    case Desktop.CairoWallpaperStyle.Stretch:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Stretch;
                        break;
                    case Desktop.CairoWallpaperStyle.Tile:
                        display = Localization.DisplayString.sSettings_Desktop_BackgroundStyle_Tile;
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

                if (Shell.IsCairoRunningAsShell)
                {
                    // image
                    ComboBoxItem cboImageItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = display
                    };
                    cboCairoBackgroundStyle.Items.Add(cboImageItem);

                    if (backgroundStyleItem == (Desktop.CairoWallpaperStyle)Settings.Instance.CairoBackgroundImageStyle) cboImageItem.IsSelected = true;

                    // bing
                    ComboBoxItem cboBingItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = display
                    };
                    cboBingBackgroundStyle.Items.Add(cboBingItem);

                    if (backgroundStyleItem == (Desktop.CairoWallpaperStyle)Settings.Instance.BingWallpaperStyle) cboBingItem.IsSelected = true;
                }
            }

            if (Shell.IsCairoRunningAsShell)
            {
                #region  cairoImageWallpaper
                ComboBoxItem cairoImageWallpaperItem = new ComboBoxItem()
                {
                    Name = "cairoImageWallpaper",
                    Content = Localization.DisplayString.sSettings_Desktop_BackgroundType_cairoImageWallpaper,
                    Tag = cairoImageBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(cairoImageWallpaperItem);
                txtCairoBackgroundPath.Text = Settings.Instance.CairoBackgroundImagePath;
                #endregion

                #region  cairoVideoWallpaper
                ComboBoxItem cairoVideoWallpaperItem = new ComboBoxItem()
                {
                    Name = "cairoVideoWallpaper",
                    Content = Localization.DisplayString.sSettings_Desktop_BackgroundType_cairoVideoWallpaper,
                    Tag = cairoVideoBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(cairoVideoWallpaperItem);
                txtCairoVideoBackgroundPath.Text = Settings.Instance.CairoBackgroundVideoPath;
                #endregion

                #region  bingWallpaper
                ComboBoxItem bingWallpaperItem = new ComboBoxItem()
                {
                    Name = "bingWallpaper",
                    Content = Localization.DisplayString.sSettings_Desktop_BackgroundType_bingWallpaper,
                    Tag = bingImageBackgroundStackPanel
                };
                cboDesktopBackgroundType.Items.Add(bingWallpaperItem);
                #endregion

                var listBoxItems = cboDesktopBackgroundType.Items.Cast<ComboBoxItem>().ToList();
                var listBoxItem = listBoxItems.FirstOrDefault(l => l.Name == Settings.Instance.DesktopBackgroundType);

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
                    Settings.Instance.CairoBackgroundImagePath = dlg.FileName;
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
                    Settings.Instance.CairoBackgroundVideoPath = dlg.FileName;
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

                Settings.Instance.DesktopBackgroundType = item.Name;
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
                    case Desktop.CairoWallpaperStyle.Tile: // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                        wallpaperStyle = "0";
                        tileWallpaper = "1";
                        break;
                    case Desktop.CairoWallpaperStyle.Center: // Centered { WallpaperStyle = 0; TileWallpaper = 0 }
                        wallpaperStyle = "0";
                        tileWallpaper = "0";
                        break;
                    case Desktop.CairoWallpaperStyle.Fit: // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                        wallpaperStyle = "6";
                        tileWallpaper = "0";
                        break;
                    case Desktop.CairoWallpaperStyle.Fill: // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                        wallpaperStyle = "10";
                        tileWallpaper = "0";
                        break;
                    case Desktop.CairoWallpaperStyle.Span: // Span { WallpaperStyle = 22; TileWallpaper = 0 }
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
                if (Settings.Instance.CairoBackgroundImageStyle != selected)
                    Settings.Instance.CairoBackgroundImageStyle = selected;
            }
        }

        private void cboBingBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboBingBackgroundStyle.SelectedItem != null)
            {
                int selected = (int)(cboBingBackgroundStyle.SelectedItem as ComboBoxItem).Tag;
                if (Settings.Instance.BingWallpaperStyle != selected) Settings.Instance.BingWallpaperStyle = selected;
            }
        }

        #endregion
    }
}