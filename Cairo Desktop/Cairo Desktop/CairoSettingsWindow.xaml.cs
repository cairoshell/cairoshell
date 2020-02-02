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
            switch (Settings.DesktopLabelPosition)
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

            switch (Settings.DesktopIconSize)
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

            switch (Settings.ProgramsMenuLayout)
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

            switch (Settings.TaskbarMode)
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

            switch (Settings.TaskbarPosition)
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

            switch (Settings.TaskbarIconSize)
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

            switch (Settings.TaskbarMiddleClick)
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

            switch (Settings.SysTrayAlwaysExpanded)
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
            fbd.SelectedPath = Settings.DesktopDirectory;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    Settings.DesktopDirectory = fbd.SelectedPath;
                    txtDesktopHome.Text = fbd.SelectedPath;
                }
            }
        }

        private void radProgramsMenuLayout0_Click(object sender, RoutedEventArgs e)
        {
            Settings.ProgramsMenuLayout = 0;
            showRestartButton();
        }

        private void radProgramsMenuLayout1_Click(object sender, RoutedEventArgs e)
        {
            Settings.ProgramsMenuLayout = 1;
            showRestartButton();
        }

        private void radTaskbarMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMode = 0;
            showRestartButton();
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMode = 1;
            showRestartButton();
        }

        private void radTaskbarMode2_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMode = 2;
            showRestartButton();
        }

        private void radTaskbarPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarPosition = 0;
            showRestartButton();
        }

        private void radTaskbarPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarPosition = 1;
            showRestartButton();
        }

        private void radTaskbarSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 0;
            showRestartButton();
        }

        private void radTaskbarSize1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 1;
            showRestartButton();
        }

        private void radTaskbarSize10_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 10;
            showRestartButton();
        }

        private void radTaskbarMiddleClick0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMiddleClick = 0;
        }

        private void radTaskbarMiddleClick1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMiddleClick = 1;
        }

        private void radDesktopLabelPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopLabelPosition = 0;
            showRestartButton();
        }

        private void radDesktopLabelPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopLabelPosition = 1;
            showRestartButton();
        }

        private void radDesktopIconSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopIconSize = 0;
            showRestartButton();
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopIconSize = 2;
            showRestartButton();
        }

        private void radTrayMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.SysTrayAlwaysExpanded = false;
            showRestartButton();
        }

        private void radTrayMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.SysTrayAlwaysExpanded = true;
            showRestartButton();
        }

        private void cboCairoMenuHotKey_DropDownClosed(object sender, EventArgs e)
        {
            List<string> hotkey = new List<string> { cboCairoMenuHotKeyMod1.SelectedValue.ToString(), cboCairoMenuHotKeyMod2.SelectedValue.ToString(), cboCairoMenuHotKeyKey.SelectedValue.ToString() };
            Settings.CairoMenuHotKey = hotkey;

            showRestartButton();
        }

        private void cboDesktopOverlayHotKey_DropDownClosed(object sender, EventArgs e)
        {
            List<string> hotkey = new List<string> { cboDesktopOverlayHotKeyMod1.SelectedValue.ToString(), cboDesktopOverlayHotKeyMod2.SelectedValue.ToString(), cboDesktopOverlayHotKeyKey.SelectedValue.ToString() };
            Settings.DesktopOverlayHotKey = hotkey;

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
    }
}