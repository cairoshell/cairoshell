using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;

namespace CairoDesktop.AppGrabber
{
    /// <summary>
    /// Interaction logic for AppGrabber.xaml
    /// </summary>
    public partial class AppGrabberUI : Window
    {
        private AppGrabber appGrabber;
        ObservableCollection<ApplicationInfo> programsMenuAppsCollection;

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

        void programsMenuAppsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove) {
                foreach (ApplicationInfo app in e.OldItems) {
                    if (app.Category != null) {
                        app.Category.Remove(app);
                    }
                }
            }
        }

        private void SkipWizard(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Visibility = Visibility.Hidden;
            new AppGrabberUI_Page2(this.appGrabber, programsMenuAppsCollection).Show();
            this.Close();
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

            // Iterate thru the apps, creating ApplicationInfoPanels and
            // add them to the installedAppsCollection
            foreach (ApplicationInfo app in apps)
            {
                if (!programsMenuAppsCollection.Contains(app))
                {
                    installedAppsCollection.Add(app);
                }
            }
            AppViewSorter.Sort(installedAppsCollection, "Name");
            AppViewSorter.Sort(programsMenuAppsCollection, "Name");

            // show content
            bdrLoad.Visibility = Visibility.Collapsed;
            bdrMain.Visibility = Visibility.Visible;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Programs and shortcuts|";

                foreach (string ext in appGrabber.ExecutableExtensions)
                {
                    dlg.Filter += "*" + ext + ";";
                }

                dlg.Filter = dlg.Filter.Substring(0, dlg.Filter.Length - 2);

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    ApplicationInfo customApp = appGrabber.PathToApp(dlg.FileName, true);
                    if (!object.ReferenceEquals(customApp, null))
                        programsMenuAppsCollection.Add(customApp);
                }
            }
            catch { }
        }

        private void ProgramsMenuAppsView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ApplicationInfo app = ((FrameworkElement)e.OriginalSource).DataContext as ApplicationInfo;

            (ProgramsMenuAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Remove(app);

            (InstalledAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Add(app);
        }

        private void InstalledAppsView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ApplicationInfo app = ((FrameworkElement)e.OriginalSource).DataContext as ApplicationInfo;

            (InstalledAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Remove(app);

            (ProgramsMenuAppsView.ItemsSource as ObservableCollection<ApplicationInfo>).Add(app);
        }
    }
}
