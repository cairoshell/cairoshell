using System.Windows;
using System.Windows.Controls;
using CairoDesktop.Common;

namespace CairoDesktop
{
	public partial class QuickLaunchButton
	{
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
            if (!Interop.Shell.StartProcess(item.CommandParameter.ToString()))
            {
                CairoMessage.Show("The file could not be found.  If you just removed this program, try removing it from the App Grabber to make the icon go away.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
	}
}