namespace CairoDesktop
{
    using AppGrabber;
    using CairoDesktop.Common;
    using CairoDesktop.WindowsTray;
    using Configuration;
    using Interop;
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
    using System.Windows.Forms;
    using System.Windows.Interop;

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        private static CairoSettingsWindow _instance = null;

        static CairoSettingsWindow() { }

        private CairoSettingsWindow()
        {
            InitializeComponent();

            loadThemes();
            loadLanguages();
            loadRadioGroups();
            loadCategories();
            loadDesktopBackgroundSettings();
            loadHotKeys();
            loadLoggingLevels();

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

            switch (Settings.Instance.DesktopIconSize)
            {
                case 0:
                    radDesktopIconSize0.IsChecked = true;
                    radDesktopIconSize2.IsChecked = false;
                    break;
                case 2:
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

            switch (Settings.Instance.TaskbarIconSize)
            {
                case 0:
                    radTaskbarSize0.IsChecked = true;
                    radTaskbarSize10.IsChecked = false;
                    radTaskbarSize1.IsChecked = false;
                    break;
                case 1:
                    radTaskbarSize0.IsChecked = false;
                    radTaskbarSize10.IsChecked = false;
                    radTaskbarSize1.IsChecked = true;
                    break;
                case 10:
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

        #region Startup checks
        private void checkUpdateConfig()
        {
            chkEnableAutoUpdates.IsChecked = Convert.ToBoolean(WinSparkle.win_sparkle_get_automatic_check_for_updates());
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
            else if (!Settings.Instance.EnableTaskbar && !Startup.IsCairoRunningAsShell)
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

            RegistryKey rKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            List<string> rKeyValueNames = rKey.GetValueNames().ToList();

            if (rKeyValueNames.Contains("CairoShell"))
            {
                chkRunAtLogOn.IsChecked = true;
            }
            else
            {
                chkRunAtLogOn.IsChecked = false;
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
            WinSparkle.win_sparkle_set_automatic_check_for_updates(Convert.ToInt32(chkEnableAutoUpdates.IsChecked));
        }

        private void showRestartButton()
        {
            btnRestart.Visibility = Visibility.Visible;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            showRestartButton();
        }

        private void DropDown_Changed(object sender, EventArgs e)
        {
            showRestartButton();
        }

        private void restartCairo(object sender, RoutedEventArgs e)
        {
            saveChanges();

            Startup.Restart();
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
            cboDesktopBackgroundType_SelectionChanged(null, null);

            NativeMethods.SetForegroundWindow(new WindowInteropHelper(this).Handle);

        }

        private void btnDesktopHomeSelect_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            showRestartButton();
        }

        private void radProgramsMenuLayout1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.ProgramsMenuLayout = 1;
            showRestartButton();
        }

        private void radTaskbarMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 0;
            showRestartButton();
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 1;
            showRestartButton();
        }

        private void radTaskbarMode2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarMode = 2;
            showRestartButton();
        }

        private void radTaskbarPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarPosition = 0;
            showRestartButton();
        }

        private void radTaskbarPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarPosition = 1;
            showRestartButton();
        }

        private void radTaskbarSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = 0;
            showRestartButton();
        }

        private void radTaskbarSize1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = 1;
            showRestartButton();
        }

        private void radTaskbarSize10_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.TaskbarIconSize = 10;
            showRestartButton();
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
            Settings.Instance.DesktopIconSize = 0;
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopIconSize = 2;
        }

        private void radTrayMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SysTrayAlwaysExpanded = false;
            showRestartButton();
        }

        private void radTrayMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SysTrayAlwaysExpanded = true;
            showRestartButton();
        }

        private void cboCairoMenuHotKey_DropDownClosed(object sender, EventArgs e)
        {
            List<string> hotkey = new List<string> { cboCairoMenuHotKeyMod1.SelectedValue.ToString(), cboCairoMenuHotKeyMod2.SelectedValue.ToString(), cboCairoMenuHotKeyKey.SelectedValue.ToString() };
            Settings.Instance.CairoMenuHotKey = hotkey.ToArray();

            showRestartButton();
        }

        private void cboDesktopOverlayHotKey_DropDownClosed(object sender, EventArgs e)
        {
            string[] hotkey = new string[] { cboDesktopOverlayHotKeyMod1.SelectedValue.ToString(), cboDesktopOverlayHotKeyMod2.SelectedValue.ToString(), cboDesktopOverlayHotKeyKey.SelectedValue.ToString() };
            Settings.Instance.DesktopOverlayHotKey = hotkey;

            showRestartButton();
        }

        private void btnChangeShell_Click(object sender, RoutedEventArgs e)
        {
            btnChangeShell.IsEnabled = false;

            Shell.IsCairoConfiguredAsShell = !Shell.IsCairoConfiguredAsShell;

            bool? LogoffChoice = CairoMessage.ShowOkCancel(Localization.DisplayString.sSettings_Advanced_ShellChangedText, Localization.DisplayString.sSettings_Advanced_ShellChanged, "Resources/logoffIcon.png", Localization.DisplayString.sSettings_Advanced_LogOffNow, Localization.DisplayString.sSettings_Advanced_LogOffLater);

            if (LogoffChoice.HasValue && LogoffChoice.Value)
                Shell.Logoff();
        }

        private void chkRunAtLogOn_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey rKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var chkBox = (System.Windows.Controls.CheckBox)sender;

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

        public static CairoSettingsWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CairoSettingsWindow();
                }

                return _instance;
            }
        }

        #region Desktop Background
        private void loadDesktopBackgroundSettings()
        {
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

            foreach (var backgroundStyleItem in Enum.GetValues(typeof(Desktop.CairoWallpaperStyle)).Cast<Desktop.CairoWallpaperStyle>())
            {
                ComboBoxItem cboItem = new ComboBoxItem()
                {
                    Tag = backgroundStyleItem,
                    Content = backgroundStyleItem.ToString()
                };
                cboWindowsBackgroundStyle.Items.Add(cboItem);

                if (backgroundStyleItem == style) cboItem.IsSelected = true;
            }

            #endregion

            if (Startup.IsCairoRunningAsShell)
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

                foreach (var backgroundStyleItem in Enum.GetValues(typeof(Desktop.CairoWallpaperStyle)).Cast<Desktop.CairoWallpaperStyle>())
                {
                    ComboBoxItem cboItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = backgroundStyleItem.ToString()
                    };
                    cboCairoBackgroundStyle.Items.Add(cboItem);

                    if (backgroundStyleItem == (Desktop.CairoWallpaperStyle)Settings.Instance.CairoBackgroundImageStyle) cboItem.IsSelected = true;
                }
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
                
                foreach (var backgroundStyleItem in Enum.GetValues(typeof(Desktop.CairoWallpaperStyle)).Cast<Desktop.CairoWallpaperStyle>())
                {
                    ComboBoxItem cboItem = new ComboBoxItem()
                    {
                        Tag = backgroundStyleItem,
                        Content = backgroundStyleItem.ToString()
                    };
                    cboBingBackgroundStyle.Items.Add(cboItem);

                    if (backgroundStyleItem == (Desktop.CairoWallpaperStyle)Settings.Instance.BingWallpaperStyle) cboItem.IsSelected = true;
                }
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
            if (ShowOpenFileDialog(GetImageFilter(), out string wallpaperPath) == System.Windows.Forms.DialogResult.OK)
            {
                txtWindowsBackgroundPath.Text = wallpaperPath;
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", txtWindowsBackgroundPath.Text);

                NativeMethods.SystemParametersInfo(NativeMethods.SPI.SETDESKWALLPAPER, 0, wallpaperPath, (NativeMethods.SPIF.UPDATEINIFILE | NativeMethods.SPIF.SENDWININICHANGE));
            }
        }

        private void btnCairoBackgroundFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (ShowOpenFileDialog(GetImageFilter(), out string wallpaperPath) == System.Windows.Forms.DialogResult.OK)
            {
                txtCairoBackgroundPath.Text = wallpaperPath;
                Settings.Instance.CairoBackgroundImagePath = wallpaperPath;
            }
        }

        private void btnCairoVideoFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string wallpaperPath;
            if (ShowOpenFileDialog(GetVideoFilter(), out wallpaperPath) == System.Windows.Forms.DialogResult.OK)
            {
                if (Path.GetExtension(wallpaperPath) == ".zip")
                {
                    string[] exts = new[] { ".wmv", ".3g2", ".3gp", ".3gp2", ".3gpp", ".amv", ".asf", ".avi", ".bin", ".cue", ".divx", ".dv", ".flv", ".gxf", ".iso", ".m1v", ".m2v", ".m2t", ".m2ts", ".m4v", ".mkv", ".mov", ".mp2", ".mp2v", ".mp4", ".mp4v", ".mpa", ".mpe", ".mpeg", ".mpeg1", ".mpeg2", ".mpeg4", ".mpg", ".mpv2", ".mts", ".nsv", ".nuv", ".ogg", ".ogm", ".ogv", ".ogx", ".ps", ".rec", ".rm", ".rmvb", ".tod", ".ts", ".tts", ".vob", ".vro", ".webm", ".wmv" };

                    FileInfo fileInfo = new FileInfo(wallpaperPath);
                    string extractPath = fileInfo.FullName.Remove((fileInfo.FullName.Length - fileInfo.Extension.Length), fileInfo.Extension.Length);

                    if (Directory.Exists(extractPath) && System.Windows.MessageBox.Show("Path alreadt exists, overwrite?", "DreamScene", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        Directory.Delete(extractPath, true);
                    }

                    if (!Directory.Exists(extractPath))
                    {
                        ZipFile.ExtractToDirectory(wallpaperPath, extractPath);
                    }

                    string file = Directory.GetFiles(extractPath).FirstOrDefault(f => exts.Contains(Path.GetExtension(f)));
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        wallpaperPath = file;
                    }
                }

                txtCairoVideoBackgroundPath.Text = wallpaperPath;
                Settings.Instance.CairoBackgroundVideoPath = wallpaperPath;
            }
        }

        private System.Windows.Forms.DialogResult ShowOpenFileDialog(string filter, out string filename)
        {
            return SupportingClasses.Dialogs.OpenFileDialog(filter, out filename);
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

        #endregion

        private void cboWindowsBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
            string wallpaperStyle = "2";
            string tileWallpaper = "0";
            string origWallpaperStyle = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", wallpaperStyle).ToString();
            string origTileWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", tileWallpaper).ToString();

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

                NativeMethods.SystemParametersInfo(NativeMethods.SPI.SETDESKWALLPAPER, 0, txtWindowsBackgroundPath.Text, (NativeMethods.SPIF.UPDATEINIFILE | NativeMethods.SPIF.SENDWININICHANGE));
            }
        }

        private void cboCairoBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = (int)(cboCairoBackgroundStyle.SelectedItem as ComboBoxItem).Tag;
            if (Settings.Instance.CairoBackgroundImageStyle != selected) Settings.Instance.CairoBackgroundImageStyle = selected;
        }

        private void cboBingBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = (int)(cboBingBackgroundStyle.SelectedItem as ComboBoxItem).Tag;
            if (Settings.Instance.BingWallpaperStyle != selected) Settings.Instance.BingWallpaperStyle = selected;
        }
    }
}