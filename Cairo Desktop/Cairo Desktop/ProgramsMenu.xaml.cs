using CairoDesktop.AppGrabber;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ManagedShell.Common.Helpers;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for ProgramsMenu.xaml
    /// </summary>
    public partial class ProgramsMenu : UserControl
    {
        public MenuBar MenuBar;
        
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
                categorizedProgramsList.ItemsSource = MenuBar._appGrabber.CategoryList;

                // set tab based on user preference
                int i = categorizedProgramsList.Items.IndexOf(MenuBar._appGrabber.CategoryList.GetCategory(Settings.Instance.DefaultProgramsCategory));
                categorizedProgramsList.SelectedIndex = i;

                hasLoaded = true;
            }
        }

        #region Sidebar items
        private void btnAppGrabber_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBar == null)
            {
                return;
            }
            
            MenuBar.ProgramsMenu.IsSubmenuOpen = false;

            // Buttons capture the mouse; need to release so that mouse events go to the intended recipient after closing
            Mouse.Capture(null);

            MenuBar._appGrabber.ShowDialog();
        }

        private void btnUninstallApps_Click(object sender, RoutedEventArgs e)
        {
            // Buttons capture the mouse; need to release so that mouse events go to the intended recipient after closing
            Mouse.Capture(null);

            if (!ShellHelper.StartProcess("appwiz.cpl"))
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

            MenuBar._appGrabber.LaunchProgram(app);
        }

        private void programsMenu_OpenAsAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_OpenRunAs(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.LaunchProgramVerb(app, "runasuser");
        }

        private void programsMenu_AddToQuickLaunch(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.AddToQuickLaunch(app);
        }

        private void programsMenu_Rename(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.RenameAppDialog(app, (bool? result) =>
            {
                MenuBar.ProgramsMenu.IsSubmenuOpen = true;
            });
        }

        private void programsMenu_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.RemoveAppConfirm(app, (bool? result) => 
            {
                MenuBar.ProgramsMenu.IsSubmenuOpen = true;
            });
        }

        private void programsMenu_Properties(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            MenuBar._appGrabber.ShowAppProperties(app);
        }

        private void miProgramsChangeCategory_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ApplicationInfo ai = mi.DataContext as ApplicationInfo;
            mi.Items.Clear();

            // Dynamically add existing categories
            foreach (Category cat in MenuBar._appGrabber.CategoryList)
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

            // Add separated option to add new category
            if (mi.Items.Count > 0)
            {
                mi.Items.Add(new Separator());
            }

            MenuItem addCategoryItem = new MenuItem();
            addCategoryItem.Header = DisplayString.sProgramsMenu_AddToNewCategory;
            addCategoryItem.Click += miProgramsAddCategory_Click;
            addCategoryItem.DataContext = ai;

            mi.Items.Add(addCategoryItem);
        }

        private void miProgramsChangeCategory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            object[] appNewCat = mi.DataContext as object[];
            ApplicationInfo ai = appNewCat[0] as ApplicationInfo;
            Category newCat = appNewCat[1] as Category;

            ai.Category.Remove(ai);
            newCat.Add(ai);

            MenuBar._appGrabber.Save();
        }

        private void miProgramsAddCategory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ApplicationInfo ai = mi.DataContext as ApplicationInfo;

            Common.MessageControls.Input inputControl = new Common.MessageControls.Input();
            inputControl.Initialize(DisplayString.sAppGrabber_Untitled);

            CairoMessage.ShowControl(DisplayString.sProgramsMenu_AddCategoryInfo,
                DisplayString.sProgramsMenu_AddCategoryTitle,
                CairoMessageImage.Default,
                inputControl,
                DisplayString.sInterface_OK,
                DisplayString.sInterface_Cancel,
                (bool? result) => {
                    if (result == true)
                    {
                        Category newCat = new Category(inputControl.Text);
                        MenuBar._appGrabber.CategoryList.Add(newCat);

                        ai.Category.Remove(ai);
                        newCat.Add(ai);

                        MenuBar._appGrabber.Save();
                    }

                    MenuBar.ProgramsMenu.IsSubmenuOpen = true;
                });
        }
        #endregion

        #region Category context menu
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            Category category = menu.DataContext as Category;

            bool enableItems = category.Type <= 0;

            foreach (Control item in menu.Items)
            {
                item.IsEnabled = enableItems;

                if (item is MenuItem mi)
                {
                    if ((string)mi.CommandParameter == "MoveUp" && category.ParentCategoryList.IndexOf(category) <= CategoryList.MIN_CATEGORIES)
                    {
                        item.IsEnabled = false;
                    }
                    else if ((string)mi.CommandParameter == "MoveDown" && category.ParentCategoryList.IndexOf(category) >= category.ParentCategoryList.Count - 1)
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }
        
        private void categoryMenu_Rename(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Category category = menuItem.DataContext as Category;

            Common.MessageControls.Input inputControl = new Common.MessageControls.Input();
            inputControl.Initialize(category.Name);

            CairoMessage.ShowControl(string.Format(DisplayString.sProgramsMenu_RenameCategoryInfo, category.DisplayName),
                string.Format(DisplayString.sProgramsMenu_RenameTitle, category.DisplayName),
                CairoMessageImage.Default,
                inputControl,
                DisplayString.sInterface_Rename,
                DisplayString.sInterface_Cancel,
                (bool? result) => {
                    if (result == true)
                    {
                        category.Name = inputControl.Text;
                        MenuBar._appGrabber.Save();
                    }

                    MenuBar.ProgramsMenu.IsSubmenuOpen = true;
                });
        }

        private void categoryMenu_Delete(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Category category = menuItem.DataContext as Category;
            CategoryList catList = category.ParentCategoryList;

            CairoMessage.ShowOkCancel(string.Format(DisplayString.sProgramsMenu_DeleteCategoryInfo, category.DisplayName, MenuBar._appGrabber.CategoryList.GetSpecialCategory(AppCategoryType.All)),
                string.Format(DisplayString.sProgramsMenu_DeleteCategoryTitle, category.DisplayName),
                CairoMessageImage.Warning,
                DisplayString.sInterface_Yes,
                DisplayString.sInterface_No,
                (bool? result) => {
                    if (result == true)
                    {
                        catList.Remove(category);
                        MenuBar._appGrabber.Save();
                    }

                    MenuBar.ProgramsMenu.IsSubmenuOpen = true;
                });
        }

        private void categoryMenu_MoveUp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Category category = menuItem.DataContext as Category;
            CategoryList catList = category.ParentCategoryList;

            catList.MoveCategory(category, -1);
            MenuBar._appGrabber.Save();
        }

        private void categoryMenu_MoveDown(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Category category = menuItem.DataContext as Category;
            CategoryList catList = category.ParentCategoryList;

            catList.MoveCategory(category, 1);
            MenuBar._appGrabber.Save();
        }
        #endregion
    }
}
