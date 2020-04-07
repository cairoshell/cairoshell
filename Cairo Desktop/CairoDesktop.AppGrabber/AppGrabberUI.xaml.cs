using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
            : this(AppGrabber.Instance)
        {
        }

        public AppGrabberUI(AppGrabber appGrabber)
        {
            this.appGrabber = appGrabber;
            InitializeComponent();

            Height = (SystemParameters.MaximizedPrimaryScreenHeight / Shell.DpiScaleAdjustment) - 100;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight / Shell.DpiScaleAdjustment;
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
            string filter = "Programs and shortcuts|";
            foreach (string ext in AppGrabber.ExecutableExtensions)
            {
                filter += $"*{ext};";
            }

            filter = filter.Substring(0, filter.Length - 2);

            using (OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = filter
            })
            {
                if (dlg.SafeShowDialog() == System.Windows.Forms.DialogResult.OK && Shell.Exists(dlg.FileName))
                {
                    ApplicationInfo customApp = AppGrabber.PathToApp(dlg.FileName, true, true);
                    if (!ReferenceEquals(customApp, null))
                    {
                        if (!programsMenuAppsCollection.Contains(customApp) && !(InstalledAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Contains(customApp))
                        {
                            programsMenuAppsCollection.Add(customApp);
                        }
                        else
                        {
                            // disallow adding a duplicate
                            CairoLogger.Instance.Debug("Excluded duplicate item: " + customApp.Name + ": " + customApp.Target);
                        }
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            appGrabber.CategoryList.Add(new Category(Localization.DisplayString.sAppGrabber_Untitled));
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
                    if (autoAddApps && ((app.IsStoreApp && autoSelectByName(app.Name)) || (!app.IsStoreApp && autoSelectByName(Path.GetFileNameWithoutExtension(app.Path)))))
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

        // categories
        static string accessories = Localization.DisplayString.sAppGrabber_Category_Accessories;
        static string productivity = Localization.DisplayString.sAppGrabber_Category_Productivity;
        static string development = Localization.DisplayString.sAppGrabber_Category_Development;
        static string graphics = Localization.DisplayString.sAppGrabber_Category_Graphics;
        static string media = Localization.DisplayString.sAppGrabber_Category_Media;
        static string internet = Localization.DisplayString.sAppGrabber_Category_Internet;
        static string games = Localization.DisplayString.sAppGrabber_Category_Games;

        string[] autoAppNames = {
            // accessories
            "File Explorer|full|" + accessories,
            "Windows Explorer|full|" + accessories,
            "Command Prompt|full|" + accessories,
            "Calculator|full|" + accessories,
            "Notepad|full|" + accessories,
            "Snipping Tool|full|" + accessories,
            "Wordpad|full|" + accessories,
            "Sticky Notes|full|" + accessories,
            "Paint|full|" + accessories,
            // productivity
            "LibreOffice|contains|" + productivity,
            "OpenOffice|contains|" + productivity,
            "Access 20|contains|" + productivity,
            "Excel 20|contains|" + productivity,
            "Lync 20|contains|" + productivity,
            "PowerPoint 20|contains|" + productivity,
            "Publisher 20|contains|" + productivity,
            "OneNote 20|contains|" + productivity,
            "Outlook 20|contains|" + productivity,
            "Skype for Business 20|contains|" + productivity,
            "Word 20|contains|" + productivity,
            "Visio 20|contains|" + productivity,
            "Access|full|" + productivity,
            "Excel|full|" + productivity,
            "PowerPoint|full|" + productivity,
            "Publisher|full|" + productivity,
            "OneNote|full|" + productivity,
            "Outlook|full|" + productivity,
            "Word|full|" + productivity,
            "Visio|full|" + productivity,
            "SumatraPDF|full|" + productivity,
            "Microsoft Teams|full|" + productivity,
            // development
            "Android Studio|contains|" + development,
            "Eclipse|contains|" + development,
            "Netbeans IDE|contains|" + development,
            "Notepad++|full|" + development,
            "Sublime Text|contains|" + development,
            "Visual Studio 20|contains|" + development,
            // graphics
            "Adobe After Effects|contains|" + graphics,
            "Adobe Illustrator|contains|" + graphics,
            "Adobe InDesign|contains|" + graphics,
            "Adobe Dreamweaver|contains|" + graphics,
            "Adobe Photoshop|contains|" + graphics,
            "Adobe Premiere|contains|" + graphics,
            "GIMP |contains|" + graphics,
            "Inkscape|contains|" + graphics,
            // media
            "Windows Media Player|full|" + media,
            "Spotify|full|" + media,
            "iTunes|full|" + media,
            "Audacity|full|" + media,
            "VLC media player|full|" + media,
            // internet
            "Firefox|contains|" + internet,
            "Thunderbird|contains|" + internet,
            "Chrome|contains|" + internet,
            "Remote Desktop Connection|full|" + internet,
            "PuTTY|full|" + internet,
            "WinSCP|full|" + internet,
            "FileZilla|full|" + internet,
            "Pidgin|full|" + internet,
            "OneDrive|full|" + internet,
            "Backup and Sync from Google|full|" + internet,
            "Dropbox|full|" + internet,
            "Skype|full|" + internet,
            "Twitter|full|" + internet,
            "Microsoft Edge|full|" + internet,
            "Internet Explorer|full|" + internet,
            "Slack|full|" + internet,
            "PureCloud|full|" + internet,
            "Discord|full|" + internet,
            // games
            "Steam|full|" + games,
            "Epic Games|full|" + games,
            "Uplay|full|" + games,
            "Battle.net|full|" + games,
            "OBS Studio|contains|" + games,
            "Origin|full|" + games
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

            // Get the Uncategorized category
            Category uncat = catList.GetSpecialCategory(AppCategoryType.Uncategorized);

            // Get the Quick Launch category
            Category quicklaunch = catList.GetSpecialCategory(AppCategoryType.QuickLaunch);

            // Add apps to category if they haven't been added to one yet.
            foreach (ApplicationInfo app in programsMenuAppsCollection)
            {
                if (app.Category == null)
                {
                    string cat;
                    if (app.IsStoreApp)
                        cat = autoCategorizeByName(app.Name);
                    else
                        cat = autoCategorizeByName(Path.GetFileNameWithoutExtension(app.Path));

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
                        if (uncat != null)
                            uncat.Add(app);
                    }
                }
            }

            categoryView.ItemsSource = catList;
            categoryView.Visibility = Visibility.Visible;
        }
    }
}
