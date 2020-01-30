namespace CairoDesktop
{
    using System;
    using System.Windows;
    using System.IO;
    using System.Linq;
    using Interop;
    using System.Windows.Interop;
    using System.Windows.Forms;
    using Configuration;
    using AppGrabber;
    using System.Collections.Generic;
    using CairoDesktop.WindowsTray;
    using CairoDesktop.Common;
    using Microsoft.Win32;
    using System.Windows.Controls;
    using System.Text;
    using System.Drawing.Imaging;

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
        }

        private void checkRunAtLogOn()
        {
            if (Shell.IsCairoUserShell.Equals(true))
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
            saveDesktopBackgroundSettings();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
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
            showRestartButton();
        }

        private void radDesktopLabelPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopLabelPosition = 1;
            showRestartButton();
        }

        private void radDesktopIconSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopIconSize = 0;
            showRestartButton();
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopIconSize = 2;
            showRestartButton();
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

            Shell.IsCairoUserShell = !Shell.IsCairoUserShell;

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
            cboWindowsBackgroundStyle.Text = style.ToString();

            #endregion
            #region  cairoImageWallpaper
            txtCairoBackgroundPath.Text = Settings.Instance.CairoBackgroundImagePath;
            cboCairoBackgroundStyle.Text = Settings.Instance.CairoBackgroundImageStyle;
            #endregion
            #region  cairoVideoWallpaper
            txtCairoVideoBackgroundPath.Text = Settings.Instance.CairoBackgroundVideoPath;
            #endregion
            #region  bingWallpaper
            cboBingBackgroundStyle.Text = Settings.Instance.BingWallpaperStyle;
            #endregion

            var listBoxItems = cboDesktopBackgroundType.Items.Cast<ListBoxItem>().ToList();
            var item = listBoxItems.FirstOrDefault(l => l.Name == Settings.Instance.DesktopBackgroundType);

            cboDesktopBackgroundType.SelectedItem = item;
        }

        private void btnWindowsBackgroundFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string wallpaperPath;
            if (ShowOpenFileDialog(GetImageFilter(), out wallpaperPath))
            {
                txtWindowsBackgroundPath.Text = wallpaperPath;
            }
        }

        private void btnCairoBackgroundFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string wllpaperPath;
            if (ShowOpenFileDialog(GetImageFilter(), out wllpaperPath))
            {
                txtCairoBackgroundPath.Text = wllpaperPath;
            }
        }

        private void btnCairoVideoFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string wllpaperPath;
            if (ShowOpenFileDialog(GetVideoFilter(), out wllpaperPath))
            {
                txtCairoVideoBackgroundPath.Text = wllpaperPath;
            }
        }

        private bool ShowOpenFileDialog(string filter, out string filename)
        {
            bool result;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = filter;
            if (openFileDialog.ShowDialog() == true)
            {
                result = true;
                filename = openFileDialog.FileName;
            }
            else
            {
                result = false;
                filename = string.Empty;
            }

            return result;
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
                " *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm";
        }

        private void cboDesktopBackgroundType_SelectionChanged(object sender, EventArgs e)
        {
            ListBoxItem listBoxItem = cboDesktopBackgroundType.SelectedItem as ListBoxItem;
            windowsImageBackgroundStackPanel.Visibility = (listBoxItem.Tag == windowsImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
            cairoImageBackgroundStackPanel.Visibility = (listBoxItem.Tag == cairoImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
            cairoVideoBackgroundStackPanel.Visibility = (listBoxItem.Tag == cairoVideoBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
            bingImageBackgroundStackPanel.Visibility = (listBoxItem.Tag == bingImageBackgroundStackPanel) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void saveDesktopBackgroundSettings()
        {
            Settings.Instance.DesktopBackgroundType = ((ListBoxItem)cboDesktopBackgroundType.SelectedItem).Name;

            #region Windows Image
            if (!string.IsNullOrWhiteSpace(txtWindowsBackgroundPath.Text) && File.Exists(txtWindowsBackgroundPath.Text))
            {
                // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
                string wallpaperStyle = "2";
                string tileWallpaper = "0";
                switch (cboBingBackgroundStyle.Text)
                {
                    case "Tile": // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                        wallpaperStyle = "0";
                        tileWallpaper = "1";
                        break;
                    case "Center": // Centered { WallpaperStyle = 1; TileWallpaper = 0 }
                        wallpaperStyle = "1";
                        tileWallpaper = "0";
                        break;
                    case "fit": // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                        wallpaperStyle = "6";
                        tileWallpaper = "0";
                        break;
                    case "Fill": // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                        wallpaperStyle = "10";
                        tileWallpaper = "0";
                        break;
                    case "Span": // Span { WallpaperStyle = 10; TileWallpaper = 0 }
                        wallpaperStyle = "20";
                        tileWallpaper = "0";
                        break;
                }

                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", txtWindowsBackgroundPath.Text);
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", wallpaperStyle);
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", tileWallpaper);
            }
            #endregion

            #region Cairo Image
            if (!string.IsNullOrWhiteSpace(txtCairoBackgroundPath.Text) && File.Exists(txtCairoBackgroundPath.Text))
            {
                Settings.Instance.CairoBackgroundImagePath = txtCairoBackgroundPath.Text;
                Settings.Instance.CairoBackgroundImageStyle = cboCairoBackgroundStyle.Text;
            }
            #endregion

            #region Cairo Video

            if (!string.IsNullOrWhiteSpace(txtCairoVideoBackgroundPath.Text) && File.Exists(txtCairoVideoBackgroundPath.Text))
            {
                Settings.Instance.CairoBackgroundVideoPath = txtCairoVideoBackgroundPath.Text;
            }
            #endregion

            #region Bing
            Settings.Instance.BingWallpaperStyle = cboBingBackgroundStyle.Text;
            #endregion

            // Tell Desktop to reload the Background
            Startup.DesktopWindow.ReloadBackground();
        }
        #endregion
    }
}