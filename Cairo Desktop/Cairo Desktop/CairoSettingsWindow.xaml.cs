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

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();

            loadThemes();
            loadLanguages();
            loadRadioGroups();
            loadCategories();

            checkUpdateConfig();
            checkTrayStatus();
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

            switch (Settings.TaskbarMode)
            {
                case 0:
                    radTaskbarMode0.IsChecked = true;
                    radTaskbarMode1.IsChecked = false;
                    break;
                case 1:
                    radTaskbarMode0.IsChecked = false;
                    radTaskbarMode1.IsChecked = true;
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
            cboLangSelect.Items.Add(new KeyValuePair<string, string>("English", "en_US"));
            cboLangSelect.Items.Add(new KeyValuePair<string, string>("Français", "fr_FR"));
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

        private void checkUpdateConfig()
        {
            chkEnableAutoUpdates.IsChecked = Convert.ToBoolean(SupportingClasses.WinSparkle.win_sparkle_get_automatic_check_for_updates());
        }

        private void checkTrayStatus()
        {
            if (NotificationArea.Instance.IsFailed)
            {
                // adjust settings window to alert user they need to install vc_redist

                chkEnableSysTray.IsEnabled = false;
                pnlTraySettings.Visibility = Visibility.Collapsed;
                chkEnableSysTrayRehook.Visibility = Visibility.Collapsed;

                lblTrayWarning.Visibility = Visibility.Visible;
            }
        }

        private void EnableAutoUpdates_Click(object sender, RoutedEventArgs e)
        {
            SupportingClasses.WinSparkle.win_sparkle_set_automatic_check_for_updates(Convert.ToInt32(chkEnableAutoUpdates.IsChecked));
        }

        private void EnableDesktop_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableDynamicDesktop_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableSubDirs_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableTaskbar_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableMenuBarShadow_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableSysTray_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void themeSetting_Changed(object sender, EventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
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

        private void radTaskbarMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMode = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarMode = 1;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarPosition = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarPosition = 1;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarSize1_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 1;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarSize10_Click(object sender, RoutedEventArgs e)
        {
            Settings.TaskbarIconSize = 10;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radDesktopLabelPos0_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopLabelPosition = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radDesktopLabelPos1_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopLabelPosition = 1;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radDesktopIconSize0_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopIconSize = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radDesktopIconSize2_Click(object sender, RoutedEventArgs e)
        {
            Settings.DesktopIconSize = 2;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTrayMode0_Click(object sender, RoutedEventArgs e)
        {
            Settings.SysTrayAlwaysExpanded = false;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTrayMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.SysTrayAlwaysExpanded = true;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void chkEnableSysTrayRehook_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void chkEnableTaskbarPolling_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void chkForceSoftwareRendering_Click(object sender, RoutedEventArgs e)
        {
            this.btnRestart.Visibility = Visibility.Visible;
        }
    }
}