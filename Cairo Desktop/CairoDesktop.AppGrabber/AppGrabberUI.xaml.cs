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

        private bool autoSelectByName(string name)
        {
            string[] autoAppNames = {
                // system
                "File Explorer|full",
                "Internet Explorer|full",
                "Windows Explorer|full",
                "Calculator|full",
                "Notepad|contains",
                "Snipping Tool|full",
                // productivity
                "LibreOffice|contains",
                "Access 20|contains",
                "Excel 20|contains",
                "Lync 20|contains",
                "PowerPoint 20|contains",
                "Publisher 20|contains",
                "OneNote 20|contains",
                "Outlook 20|contains",
                "Skype for Business 20|contains",
                "Word 20|contains",
                "Visio 20|contains",
                "Visual Studio 20|contains",
                // graphics
                "Adobe After Effects|contains",
                "Adobe Illustrator|contains",
                "Adobe InDesign|contains",
                "Adobe Dreamweaver|contains",
                "Adobe Photoshop|contains",
                "Adobe Premiere|contains",
                // media
                "Media Player|contains",
                "Spotify|full",
                "iTunes|full",
                "Audacity|full",
                // internet
                "Mozilla Firefox|full",
                "Chrome|contains",
                "Remote Desktop Connection|full",
                "PuTTY|full",
                "FileZilla|full",
                "Pidgin|full",
                "OneDrive|full",
                "Dropbox|full",
                "Skype|full",
                "Twitter|full",
                "Microsoft Edge|full",
                "Slack|full",
                "PureCloud|full",
                "Discord|full",
                // games
                "Steam|full",
                "Epic Games|full",
                "Uplay|full",
                "Battle.net|full",
                "Open Broadcaster Software|full",
                "Origin|full"
            };

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

            // Add Apps to uncat if they haven't been added to a category yet.
            foreach (ApplicationInfo app in programsMenuAppsCollection)
            {
                if (app.Category == null)
                {
                    uncat.Add(app);
                }
            }

            categoryView.ItemsSource = catList;
            categoryView.Visibility = Visibility.Visible;
        }
    }
}
