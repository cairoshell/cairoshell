using System;
using System.Windows;
using System.Windows.Controls;
using GlassLib;
using System.IO;

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

            //Update UI
            UpdateSideBar();
        }

        private void OpenBoltMenu(object sender, RoutedEventArgs e)
        {
            this.BoltMenu.IsOpen = true;
            this.BoltMenu.StaysOpen = false;
        }

        private void UpdateSideBar()
        {
            RefreshFavorites();
            RefreshDiskDrives();
        }

        private void RefreshFavorites()
        {
            //TODO:Read the favorites from database.
        }

        private void RefreshDiskDrives()
        {
            //Add disk drives
            int baseDrivesIndex = Sidebar.Children.IndexOf(DrivesListSample);

            //Get drives
            DriveInfo[] drives = DriveInfo.GetDrives();

            //Get style references
            Style stackStyle = (Style)this.FindResource("CairoExplorerSidebarSectionHeaderOpen");
            Style stackTextStyle = (Style)this.FindResource("CairoExplorerSidebarSectionHeaderOpenText");

            //Loop drives
            for (int i = drives.Length - 1; i >= 0; i--)
            {
                string volLabel = drives[i].IsReady ? drives[i].VolumeLabel : drives[i].DriveType.ToString();

                //Only active drives
                StackPanel panel = new StackPanel();
                panel.Style = stackStyle;

                TextBlock block = new TextBlock();
                block.Text = String.Format("{0} ({1})", volLabel, drives[i].Name);
                block.Style = stackTextStyle;
                panel.Children.Add(block);
                Sidebar.Children.Insert(baseDrivesIndex + 1, panel);
            }//for

            //Remove sample items
            Sidebar.Children.Remove(DrivesListSample);
        }
    }
}
