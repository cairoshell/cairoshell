namespace CairoDesktop
{
    using System;
    using System.Windows;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using Interop;
    using System.Windows.Interop;

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();

            selectTheme.Items.Add("Default");
            selectTheme.SelectedIndex = 0;
            foreach (string subStr in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(s => Path.GetExtension(s).Contains("xaml")))
            {
                string theme = Path.GetFileName(subStr);
                selectTheme.Items.Add(theme);

                if (theme == Properties.Settings.Default.CairoTheme)
                    selectTheme.SelectedIndex = selectTheme.Items.Count - 1;
            }

        }

        private void EnableDesktop_Click(object sender, RoutedEventArgs e)
        {
            bool IsDesktopEnabled = Properties.Settings.Default.EnableDesktop;
            if (IsDesktopEnabled == true)
            {
                Startup.DesktopWindow = new Desktop() { Owner = Startup.DeskParent };
                Startup.DesktopWindow.Show();
            }
            else
            {
                if (Startup.DesktopWindow != null)
                {
                    Startup.DesktopWindow.Close();
                    Startup.DesktopWindow = null;
                }
            }
        }

        private void EnableDynamicDesktop_Click(object sender, RoutedEventArgs e)
        {
            bool IsDynamicDesktopEnabled = Properties.Settings.Default.EnableDynamicDesktop;
            if (IsDynamicDesktopEnabled == true)
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
        }

        private void EnableSubDirs_Click(object sender, RoutedEventArgs e)
        {
            bool IsSubDirsEnabled = Properties.Settings.Default.EnableSubDirs;
            if (IsSubDirsEnabled == true)
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
        }

        private void ShowFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            bool IsShowFileExtensions = Properties.Settings.Default.ShowFileExtensions;
            if (IsShowFileExtensions == true)
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.restartButton.Visibility = Visibility.Visible;
            }
        }

        private void EnableTaskbar_Click(object sender, RoutedEventArgs e)
        {
            bool IsTaskbarEnabled = Properties.Settings.Default.EnableTaskbar;
            if (IsTaskbarEnabled == true)
            {
                Startup.TaskbarWindow = new Taskbar();
                Startup.TaskbarWindow.Show();
                this.Activate();
            }
            else
            {
                if (Startup.TaskbarWindow != null)
                {
                    Startup.TaskbarWindow.Close();
                }
            }
        }

        private void EnableMenuBarShadow_Click(object sender, RoutedEventArgs e)
        {
            bool IsMenuBarShadowEnabled = Properties.Settings.Default.EnableMenuBarShadow;
            if (IsMenuBarShadowEnabled == true)
            {
                Startup.MenuBarShadowWindow = new MenuBarShadow();
                Startup.MenuBarShadowWindow.Show();
                this.Activate();
            }
            else
            {
                if (Startup.MenuBarShadowWindow != null)
                {
                    Startup.MenuBarShadowWindow.Close();
                }
            }
        }

        private void EnableSysTray_Click(object sender, RoutedEventArgs e)
        {
            bool IsSysTrayEnabled = Properties.Settings.Default.EnableSysTray;
            this.restartButton.Visibility = Visibility.Visible;
        }

        private void themeSetting_Changed(object sender, EventArgs e)
        {
            this.restartButton.Visibility = Visibility.Visible;
        }

        private void restartCairo(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TimeFormat = timeSetting.Text;
            Properties.Settings.Default.DateFormat = dateSetting.Text;
            string s1 = selectTheme.SelectedValue.ToString();
            s1.Replace("'", "");
            Properties.Settings.Default.CairoTheme = s1;
            Properties.Settings.Default.Save();
            try
            {
                //run the program again and close this one
                Process current = new Process();
                current.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "CairoDesktop.exe";
                current.StartInfo.Arguments = "/restart";
                current.Start();

                //close this one
                Process.GetCurrentProcess().Kill();
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
            Properties.Settings.Default.TimeFormat = timeSetting.Text;
            Properties.Settings.Default.DateFormat = dateSetting.Text;
            Properties.Settings.Default.DesktopDirectory = desktopHome.Text;
            string s1 = selectTheme.SelectedValue.ToString();
            s1.Replace("'", "");
            Properties.Settings.Default.CairoTheme = s1;
            Properties.Settings.Default.Save();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.SetForegroundWindow(new WindowInteropHelper(this).Handle);
        }
    }
}