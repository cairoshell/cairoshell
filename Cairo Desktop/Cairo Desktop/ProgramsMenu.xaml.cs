using CairoDesktop.AppGrabber;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for ProgramsMenu.xaml
    /// </summary>
    public partial class ProgramsMenu : UserControl
    {
        public MenuBar MenuBar;

        // AppGrabber instance
        private AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;
        bool hasLoaded;

        public ProgramsMenu()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!hasLoaded)
            {
                // Set Programs Menu to use appGrabber's ProgramList as its source
                categorizedProgramsList.ItemsSource = appGrabber.CategoryList;

                // set tab based on user preference
                int i = categorizedProgramsList.Items.IndexOf(appGrabber.CategoryList.GetCategory(Settings.Instance.DefaultProgramsCategory));
                categorizedProgramsList.SelectedIndex = i;

                hasLoaded = true;
            }
        }

        #region Sidebar items
        private void btnAppGrabber_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBar != null)
            {
                MenuBar.ProgramsMenu.IsSubmenuOpen = false;
            }

            appGrabber.ShowDialog();
        }

        private void btnUninstallApps_Click(object sender, RoutedEventArgs e)
        {
            if (!Interop.Shell.StartProcess("appwiz.cpl"))
                CairoMessage.Show(DisplayString.sError_CantOpenAppWiz, DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
        }
        #endregion

        #region Context menu
        private void ctxProgramsItem_Opened(object sender, RoutedEventArgs e)
        {
            if (KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
            {
                ContextMenu menu = (sender as ContextMenu);
                foreach (Control item in menu.Items)
                {
                    if (item.Name == "miProgramsItemRunAs")
                    {
                        item.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
            else
            {
                ContextMenu menu = (sender as ContextMenu);
                foreach (Control item in menu.Items)
                {
                    if (item.Name == "miProgramsItemRunAs")
                    {
                        item.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
            }
        }

        private void programsMenu_Open(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void programsMenu_OpenAsAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_OpenRunAs(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgramVerb(app, "runasuser");
        }

        private void programsMenu_AddToQuickLaunch(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.AddToQuickLaunch(app);
        }

        private void programsMenu_Rename(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            DockPanel parent = ((MenuItem)((ContextMenu)item.Parent).PlacementTarget).Header as DockPanel;
            TextBox rename = parent.FindName("txtProgramRename") as TextBox;
            TextBlock label = parent.FindName("lblProgramName") as TextBlock;

            rename.Visibility = Visibility.Visible;
            label.Visibility = Visibility.Collapsed;
            rename.Focus();
            rename.SelectAll();
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

        private void miProgramsChangeCategory_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ApplicationInfo ai = mi.DataContext as ApplicationInfo;
            mi.Items.Clear();

            foreach (Category cat in appGrabber.CategoryList)
            {
                if (cat.Type == 0 && cat != ai.Category)
                {
                    MenuItem newItem = new MenuItem();
                    newItem.Header = cat.DisplayName;

                    object[] appNewCat = new object[] { ai, cat };
                    newItem.DataContext = appNewCat;

                    newItem.Click += new RoutedEventHandler(miProgramsChangeCategory_Click);
                    mi.Items.Add(newItem);
                }
            }
        }

        private void miProgramsChangeCategory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            object[] appNewCat = mi.DataContext as object[];
            ApplicationInfo ai = appNewCat[0] as ApplicationInfo;
            Category newCat = appNewCat[1] as Category;

            ai.Category.Remove(ai);
            newCat.Add(ai);

            appGrabber.Save();
        }
        #endregion

        #region Rename
        private void txtProgramRename_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = e.OriginalSource as TextBox;
            ApplicationInfo app = ((box.Parent as DockPanel).Parent as MenuItem).DataContext as ApplicationInfo;

            if (!ReferenceEquals(app, null))
            {
                appGrabber.Rename(app, box.Text);
            }

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
            {
                if (peer is TextBlock)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }
            box.Visibility = Visibility.Collapsed;
        }

        private void txtProgramRename_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
        }

        private void txtProgramRename_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (IsKeyboardFocusWithin && !(e.NewFocus is TextBox))
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}
