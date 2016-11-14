namespace CairoDesktop
{
    using System;
    using System.Windows;
    using System.Resources;
    using System.Windows.Input;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Interop;
    using SupportingClasses;

    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();
            string themelist = Properties.Settings.Default.ThemeList;
            StringBuilder sBuilder = new StringBuilder();
            int id = 0;
            foreach (string subStr in Regex.Split(themelist, " |, |,"))
            {
                if (subStr == Properties.Settings.Default.CairoTheme)
                {
                    int index = id;
                    selectTheme.SelectedIndex = index;
                }
                selectTheme.Items.Add(subStr);
                id++;
            }

        }

        private void EnableDesktop_Click(object sender, RoutedEventArgs e)
        {
            bool IsDesktopEnabled = Properties.Settings.Default.EnableDesktop;
            if (IsDesktopEnabled == true)
            {
                Startup.DesktopWindow = new Desktop() { Owner = Startup.DeskParent };
                Startup.DesktopWindow.Show();
                WindowInteropHelper f = new WindowInteropHelper(Startup.DesktopWindow);
                int result = NativeMethods.SetShellWindow(f.Handle);
                Startup.DesktopWindow.ShowWindowBottomMost(f.Handle);
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

        private void UseDarkIcons_Click(object sender, RoutedEventArgs e)
        {
            bool IsDarkIconsEnabled = Properties.Settings.Default.UseDarkIcons;
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
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
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
            string s1 = selectTheme.SelectedValue.ToString();
            s1.Replace("'", "");
            Properties.Settings.Default.CairoTheme = s1;
            Properties.Settings.Default.Save();
        }
    }
}