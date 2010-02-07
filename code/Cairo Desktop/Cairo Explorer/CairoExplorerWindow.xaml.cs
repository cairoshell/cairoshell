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
using System.Windows.Navigation;
//using System.Windows.Shapes;
using GlassLib;

namespace CairoExplorer
{
    /// <summary>
    /// Interaction logic for CairoExplorerWindow.xaml
    /// </summary>
    public partial class CairoExplorerWindow : Window
    {
        public CairoExplorerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dwm.Glass[this].Enabled = true;
            Thickness CairoExplorerBottomBarThickness = new Thickness(0, 0, 0, 25);
            Dwm.Glass[this].Margins = CairoExplorerBottomBarThickness;
        }

        private void OpenBoltMenu(object sender, RoutedEventArgs e)
        {
            this.BoltMenu.IsOpen = true;
            this.BoltMenu.StaysOpen = false;
        }
    }
}
