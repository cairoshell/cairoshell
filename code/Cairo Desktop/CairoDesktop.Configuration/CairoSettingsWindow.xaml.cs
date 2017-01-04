namespace CairoDesktop.Configuration
{
    using System;
    using System.Windows;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using Interop;
    using System.Windows.Interop;
    using System.Windows.Forms;

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();

            loadThemes();
            loadTaskbarMode();
        }

        private void loadTaskbarMode()
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
        }

        private void loadThemes()
        {
            cboThemeSelect.Items.Add("Default");
            cboThemeSelect.SelectedIndex = 0;
            foreach (string subStr in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(s => Path.GetExtension(s).Contains("xaml")))
            {
                string theme = Path.GetFileName(subStr);
                cboThemeSelect.Items.Add(theme);

                if (theme == Settings.CairoTheme)
                    cboThemeSelect.SelectedIndex = cboThemeSelect.Items.Count - 1;
            }
        }

        private void EnableDesktop_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableDesktop = (bool)chkEnableDesktop.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableDynamicDesktop_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableDynamicDesktop = (bool)chkEnableDynamicDesktop.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableSubDirs_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableSubDirs = (bool)chkEnableSubDirs.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void ShowFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            Settings.ShowFileExtensions = (bool)chkShowFileExtensions.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableTaskbar_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableTaskbar = (bool)chkEnableTaskbar.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableMenuBarShadow_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableMenuBarShadow = (bool)chkEnableMenuBarShadow.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void EnableSysTray_Click(object sender, RoutedEventArgs e)
        {
            Settings.EnableSysTray = (bool)chkEnableSysTray.IsChecked;
            this.btnRestart.Visibility = Visibility.Visible;
        }

        private void themeSetting_Changed(object sender, EventArgs e)
        {
            string s1 = cboThemeSelect.SelectedValue.ToString();
            s1.Replace("'", "");
            Settings.CairoTheme = s1;
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
            Settings.TimeFormat = timeSetting.Text;
            Settings.DateFormat = dateSetting.Text;
            Settings.DesktopDirectory = txtDesktopHome.Text;
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
    }
}