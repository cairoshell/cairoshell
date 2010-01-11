using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Markup;

namespace CairoDesktop
{
	public partial class QuickLaunchButton
	{
		public QuickLaunchButton()
		{
			this.InitializeComponent();
			// Insert code required on object creation below this point.
            if (Properties.Settings.Default.MenuBarWhite)
            {
                ResourceDictionary CairoDictionary = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "CairoStyles_alt.xaml"));
                this.Resources.MergedDictionaries[0] = CairoDictionary;
            }
		}

private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            try {
                System.Diagnostics.Process.Start(item.CommandParameter.ToString());
            } catch {
                CairoMessage.Show("The file could not be found.  If you just removed this program, try removing it from the App Grabber to make the icon go away.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
	}
}