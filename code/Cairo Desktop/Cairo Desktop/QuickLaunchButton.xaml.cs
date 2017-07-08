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