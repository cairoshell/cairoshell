using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace CairoDesktop.AppGrabber
{
    /// <summary>
    /// Interaction logic for AppGrabber.xaml
    /// </summary>
    public partial class AppGrabberUI : Window
    {
        private AppGrabber appGrabber;
        ObservableCollection<ApplicationInfo> programsMenuAppsCollection;

        public Visibility AppVisibility
        {
            get { return (Visibility)GetValue(AppVisibilityProperty); }
            set { SetValue(AppVisibilityProperty, value); }
        }

        private bool bAppsHidden
        {
            get { return (AppVisibility != Visibility.Visible); }
            set
            {
                if (value)
                {
                    SetValue(AppVisibilityProperty, Visibility.Collapsed);
                    SetValue(bAppsHiddenProperty, true);
                }
                else
                {
                    SetValue(AppVisibilityProperty, Visibility.Visible);
                    SetValue(bAppsHiddenProperty, false);
                }
            }
        }

        public static readonly DependencyProperty AppVisibilityProperty = DependencyProperty.Register("AppVisibility", typeof(Visibility), typeof(AppGrabberUI), new PropertyMetadata(null));
        private static readonly DependencyProperty bAppsHiddenProperty = DependencyProperty.Register("bAppsHidden", typeof(bool), typeof(AppGrabberUI), new PropertyMetadata(null));

        public AppGrabberUI()
            : this(new AppGrabber())
        {
        }

        public AppGrabberUI(AppGrabber appGrabber)
        {
            this.appGrabber = appGrabber;
            InitializeComponent();

            Height = SystemParameters.MaximizedPrimaryScreenHeight - 100;
        }

        #region Button Clicks

        private void SkipWizard(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            goPage2();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            appGrabber.Save();
            this.Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Programs and shortcuts|";

            foreach (string ext in appGrabber.ExecutableExtensions)
            {
                dlg.Filter += "*" + ext + ";";
            }

            dlg.Filter = dlg.Filter.Substring(0, dlg.Filter.Length - 2);

            System.Windows.Forms.DialogResult result;

            try
            {
                result = dlg.ShowDialog();
            }
            catch
            {
                // show retro dialog if the better one fails to load
                dlg.AutoUpgradeEnabled = false;
                result = dlg.ShowDialog();
            }

            if (result == System.Windows.Forms.DialogResult.OK && Interop.Shell.Exists(dlg.FileName))
            {
                ApplicationInfo customApp = appGrabber.PathToApp(dlg.FileName, true);
                if (!object.ReferenceEquals(customApp, null))
                    programsMenuAppsCollection.Add(customApp);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            appGrabber.CategoryList.Add(new Category("New"));
            scrollViewer.ScrollToEnd();
        }

        private void tglHide_Click(object sender, RoutedEventArgs e)
        {
            if (!bAppsHidden)
                bAppsHidden = true;
            else
                bAppsHidden = false;
        }

        #endregion

        #region Events

        void programsMenuAppsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (ApplicationInfo app in e.OldItems)
                {
                    if (app.Category != null)
                    {
                        app.Category.Remove(app);
                    }
                }
            }
        }

        private void ProgramsMenuAppsView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext != null)
            {
                ApplicationInfo app = ((FrameworkElement)e.OriginalSource).DataContext as ApplicationInfo;

                (ProgramsMenuAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Remove(app);

                (InstalledAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Add(app);
            }
        }

        private void InstalledAppsView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext != null)
            {
                ApplicationInfo app = ((FrameworkElement)e.OriginalSource).DataContext as ApplicationInfo;

                (InstalledAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Remove(app);

                (ProgramsMenuAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Add(app);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Now to add them to the list on the AppGrabber
            // Create ObservableCollections to bind to the ListViews

            ObservableCollection<ApplicationInfo> installedAppsCollection = new ObservableCollection<ApplicationInfo>();
            programsMenuAppsCollection = appGrabber.CategoryList.FlatList;
            InstalledAppsView.ItemsSource = installedAppsCollection;
            ProgramsMenuAppsView.ItemsSource = programsMenuAppsCollection;
            // Need to use an event handler to remove Apps from categories when moved to the "Installed Applications" listing
            programsMenuAppsCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(programsMenuAppsCollection_CollectionChanged);

            // Grab the Programs
            List<ApplicationInfo> apps = appGrabber.ProgramList;

            // automatically select apps if none have been yet
            bool autoAddApps = programsMenuAppsCollection.Count < 1;

            // Iterate thru the apps, creating ApplicationInfoPanels and
            // add them to the installedAppsCollection
            foreach (ApplicationInfo app in apps)
            {
                if (!programsMenuAppsCollection.Contains(app))
                {
                    if (autoAddApps && autoSelectByName(app.Name))
                        programsMenuAppsCollection.Add(app);
                    else
                        installedAppsCollection.Add(app);
                }
            }
            AppViewSorter.Sort(installedAppsCollection, "Name");
            AppViewSorter.Sort(programsMenuAppsCollection, "Name");

            // show content
            bdrLoad.Visibility = Visibility.Collapsed;
            bdrMain.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppGrabber.uiInstance = null;
        }

        private void ScrollViewer_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            int scrollTollerance = 12;
            ScrollViewer scroller = sender as ScrollViewer;
            System.Diagnostics.Debug.WriteLine("Mouse rel scroller:" + e.GetPosition(scroller).Y);
            System.Diagnostics.Debug.WriteLine("Scroller Height:" + scroller.ActualHeight);
            if (e.GetPosition(scroller).Y > scroller.ActualHeight - scrollTollerance)
            {
                scroller.LineDown();
            }
            else if (e.GetPosition(scroller).Y < scrollTollerance)
            {
                scroller.LineUp();
            }
        }

        #endregion

        #region Auto selection

        string[] autoAppNames = {
            // system
            "File Explorer|full|System",
            "Internet Explorer|full|System",
            "Windows Explorer|full|System",
            "Calculator|full|System",
            "Notepad|contains|System",
            "Snipping Tool|full|System",
            // productivity
            "LibreOffice|contains|Productivity",
            "Access 20|contains|Productivity",
            "Excel 20|contains|Productivity",
            "Lync 20|contains|Productivity",
            "PowerPoint 20|contains|Productivity",
            "Publisher 20|contains|Productivity",
            "OneNote 20|contains|Productivity",
            "Outlook 20|contains|Productivity",
            "Skype for Business 20|contains|Productivity",
            "Word 20|contains|Productivity",
            "Visio 20|contains|Productivity",
            "Visual Studio 20|contains|Productivity",
            // graphics
            "Adobe After Effects|contains|Graphics",
            "Adobe Illustrator|contains|Graphics",
            "Adobe InDesign|contains|Graphics",
            "Adobe Dreamweaver|contains|Graphics",
            "Adobe Photoshop|contains|Graphics",
            "Adobe Premiere|contains|Graphics",
            // media
            "Windows Media Player|full|Media",
            "Spotify|full|Media",
            "iTunes|full|Media",
            "Audacity|full|Media",
            "VLC media player|full|Media",
            // internet
            "Firefox|contains|Internet",
            "Chrome|contains|Internet",
            "Remote Desktop Connection|full|Internet",
            "PuTTY|full|Internet",
            "FileZilla|full|Internet",
            "Pidgin|full|Internet",
            "OneDrive|full|Internet",
            "Dropbox|full|Internet",
            "Skype|full|Internet",
            "Twitter|full|Internet",
            "Microsoft Edge|full|Internet",
            "Slack|full|Internet",
            "PureCloud|full|Internet",
            "Discord|full|Internet",
            // games
            "Steam|full|Games",
            "Epic Games|full|Games",
            "Uplay|full|Games",
            "Battle.net|full|Games",
            "Open Broadcaster Software|full|Games",
            "Origin|full|Games"
        };

        private bool autoSelectByName(string name)
        {

            foreach (string autoApp in autoAppNames)
            {
                string[] autoAppName = autoApp.Split('|');

                if (autoAppName[1] == "full" && autoAppName[0] == name)
                    return true;
                if (autoAppName[1] == "contains" && name.Contains(autoAppName[0]))
                    return true;
            }

            return false;
        }

        private string autoCategorizeByName(string name)
        {
            foreach (string autoApp in autoAppNames)
            {
                string[] autoAppName = autoApp.Split('|');

                if (autoAppName[1] == "full" && autoAppName[0] == name)
                    return autoAppName[2];
                if (autoAppName[1] == "contains" && name.Contains(autoAppName[0]))
                    return autoAppName[2];
            }

            return "";
        }

        #endregion

        private void goPage2()
        {
            bdrMain.Visibility = Visibility.Collapsed;
            bdrPage2.Visibility = Visibility.Visible;

            // Grab the Programs
            CategoryList catList = appGrabber.CategoryList;

            // Get the Uncategorized category - create it if it doesn't exist.
            Category uncat = catList.GetCategory("Uncategorized");
            if (uncat == null)
            {
                uncat = new Category("Uncategorized", false);
                catList.Add(uncat);
            }

            // Get the Quick Launch category - create it if it doesn't exist.
            Category quicklaunch = catList.GetCategory("Quick Launch");
            if (quicklaunch == null)
            {
                quicklaunch = new Category("Quick Launch", false);
                catList.Add(quicklaunch);
            }

            // Add apps to category if they haven't been added to one yet.
            foreach (ApplicationInfo app in programsMenuAppsCollection)
            {
                if (app.Category == null)
                {
                    string cat = autoCategorizeByName(app.Name);

                    if (cat != "")
                    {
                        // Get the category - create it if it doesn't exist.
                        Category categoryToAdd = catList.GetCategory(cat);
                        if (categoryToAdd == null)
                        {
                            categoryToAdd = new Category(cat, true);
                            catList.Add(categoryToAdd);
                        }

                        categoryToAdd.Add(app);
                    }
                    else
                    {
                        // not a known app, add to Uncategorized
                        uncat.Add(app);
                    }
                }
            }

            categoryView.ItemsSource = catList;
            categoryView.Visibility = Visibility.Visible;
        }
    }
}
