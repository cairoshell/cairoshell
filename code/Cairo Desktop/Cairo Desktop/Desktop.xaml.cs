using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window
    {
        public Desktop()
        {
            InitializeComponent();
            if (Properties.Settings.Default.EnableDynamicDesktop)
            {
                this.DesktopNavToolbar.IsOpen = true;
                this.DesktopNavToolbar.StaysOpen = true;
            }
            else
            {
                this.DesktopNavToolbar.IsOpen = false;
                this.DesktopNavToolbar.StaysOpen = false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            CairoMessage.Show("This will go back.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            CairoMessage.Show("This will go to %USERPROFILE%\\Desktop.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            CairoMessage.Show("This will go forward.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
