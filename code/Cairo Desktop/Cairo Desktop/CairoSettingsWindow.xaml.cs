namespace CairoDesktop
{
    using System;
    using System.Windows;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using Interop;
    using System.Windows.Interop;
    using System.Windows.Forms;
    using Configuration;
    using AppGrabber;

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();

            loadThemes();
            loadRadioGroups();
            loadCategories();

            checkTrayStatus();
        }

        private void loadRadioGroups()
        {
            switch (Settings.WindowsTaskbarMode)
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

        private void loadCategories()
        {
            foreach (Category cat in AppGrabber.AppGrabber.Instance.CategoryList)
            {
                if (cat.ShowInMenu)
                {
                    string theme = cat.Name;
                    cboDefaultProgramsCategory.Items.Add(theme);
                }
            }
        }

        private void checkTrayStatus()
        {
            if (Startup.MenuBarWindow.SystemTrayFailure)
            {
                // adjust settings window to alert user they need to install vc_redist

                chkEnableSysTray.IsEnabled = false;
                pnlTraySettings.Visibility = Visibility.Collapsed;
                chkEnableSysTrayRehook.Visibility = Visibility.Collapsed;

                lblTrayWarning.Visibility = Visibility.Visible;
            }
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

        private void ShowFileExtensions_Click(object sender, RoutedEventArgs e)
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

            try
            {
                //run the program again and close this one
                Process current = new Process();
                current.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "CairoDesktop.exe";
                current.StartInfo.Arguments = "/restart";
                current.Start();

                //close this one
                System.Windows.Application.Current.Shutdown();
            }
            catch
            { }
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
            fbd.Description = "Select a folder to display as your desktop:";
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
            Settings.WindowsTaskbarMode = 0;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarMode1_Click(object sender, RoutedEventArgs e)
        {
            Settings.WindowsTaskbarMode = 1;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void radTaskbarMode2_Click(object sender, RoutedEventArgs e)
        {
            Settings.WindowsTaskbarMode = 2;
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
    }
}