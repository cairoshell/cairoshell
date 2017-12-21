using System.Windows;
using System.Windows.Controls;
using CairoDesktop.AppGrabber;

namespace CairoDesktop
{
	public partial class QuickLaunchButton
	{
        static AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

		public QuickLaunchButton()
		{
			this.InitializeComponent();

            switch (Configuration.Settings.TaskbarIconSize)
            {
                case 0:
                    imgIcon.Width = 32;
                    imgIcon.Height = 32;
                    break;
                case 10:
                    imgIcon.Width = 24;
                    imgIcon.Height = 24;
                    break;
                default:
                    imgIcon.Width = 16;
                    imgIcon.Height = 16;
                    break;
            }
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramMenu(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.RemoveAppConfirm(app);
        }

        private void programsMenu_Properties(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            AppGrabber.AppGrabber.ShowAppProperties(app);
        }
    }
}