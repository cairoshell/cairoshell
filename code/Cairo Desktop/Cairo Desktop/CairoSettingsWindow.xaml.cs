namespace CairoDesktop
{
    using System;
    using System.Windows;
    using System.Resources;
    using System.Windows.Input;
    /// <summary>
    /// Interaction logic for CairoSettingsWindow.xaml
    /// </summary>
    public partial class CairoSettingsWindow : Window
    {
        public CairoSettingsWindow()
        {
            InitializeComponent();
        }

        private void EnableDesktop_Click(object sender, RoutedEventArgs e)
        {
            bool IsDesktopEnabled = Properties.Settings.Default.EnableDesktop;
            if (IsDesktopEnabled == true)
            {
                Desktop window = new Desktop();
                window.Show();
                Activate();
            }
            else
            {
                if (Startup.DesktopWindow != null)
                {
                    Startup.DesktopWindow.Close();
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

        private void MenuBarWhite_Click(object sender, RoutedEventArgs e)
        {
            bool IsMenuBarWhite = Properties.Settings.Default.MenuBarWhite;
            this.restartButton.Visibility = Visibility.Visible;
        }

        private void restartCairo(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TimeFormat = timeSetting.Text;
            Properties.Settings.Default.DateFormat = dateSetting.Text;
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
            Properties.Settings.Default.Save();
        }
    }
}
