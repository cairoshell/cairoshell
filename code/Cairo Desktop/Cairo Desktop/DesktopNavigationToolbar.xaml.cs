using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window
    {
        public DesktopNavigationToolbar()
        {
            InitializeComponent();

            this.Top = SystemParameters.WorkArea.Bottom - this.Height - 150;
            this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go back.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
            DirectoryInfo parent = (this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.Parent;
            if (parent != null)
            {
                (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.FullName);
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(parent.FullName, Dispatcher.CurrentDispatcher);
            }

        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go to %USERPROFILE%\\Desktop.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);

            (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.FullName);
            (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Dispatcher.CurrentDispatcher);

        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go forward.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
            if ((this.Owner as Desktop).PathHistory.Count > 0)
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory((this.Owner as Desktop).PathHistory.Pop(), Dispatcher.CurrentDispatcher);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.SelectedPath = (this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.FullName;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.FullName);
                    (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(dir.FullName, Dispatcher.CurrentDispatcher);
                }
            }
        }
    }
}
