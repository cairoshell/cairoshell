using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace CairoDesktop.AppGrabber
{
    /// <summary>
    /// Interaction logic for AppGrabberUI_Page2.xaml
    /// </summary>
    public partial class AppGrabberUI_Page2 : Window
    {
        private AppGrabber appGrabber;

        /// <summary>
        /// Default constructor never gets called.
        /// </summary>
        private AppGrabberUI_Page2()
            : this(new AppGrabber(), new ObservableCollection<ApplicationInfo>())
        {
        }
        public bool ShowInMenu{ get; set; }
        public AppGrabberUI_Page2(AppGrabber appGrabber, ObservableCollection<ApplicationInfo> selectedApps)
        {
            this.appGrabber = appGrabber;
            InitializeComponent();
            
            // Grab the Programs
            CategoryList catList = appGrabber.CategoryList;

            // Get the Uncategorized category - create it if it doesn't exist.
            Category uncat = catList.GetCategory("Uncategorized");
            if (uncat == null) {
                uncat = new Category("Uncategorized");
                uncat.ShowInMenu = false;
                catList.Add(uncat);
            }

            // Get the Quick Launch category - create it if it doesn't exist.
            Category quicklaunch = catList.GetCategory("Quick Launch");
            if (quicklaunch == null) {
                quicklaunch = new Category("Quick Launch");
                quicklaunch.ShowInMenu = false;
                catList.Add(quicklaunch);
            }
            // Add Apps to uncat if they haven't been added to a category yet.
            foreach (ApplicationInfo app in selectedApps) {
                if (app.Category == null) {
                    uncat.Add(app);
                }
            }

            categoryView.ItemsSource = catList;
        }

        private void SkipWizard(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            appGrabber.CategoryList.Add(new Category("New"));
            scrollViewer.ScrollToEnd();
        }

        private void ScrollViewer_DragOver(object sender, DragEventArgs e) {
            int scrollTollerance = 12;
            ScrollViewer scroller = sender as ScrollViewer;
            System.Diagnostics.Debug.WriteLine("Mouse rel scroller:" + e.GetPosition(scroller).Y);
            System.Diagnostics.Debug.WriteLine("Scroller Height:" + scroller.ActualHeight);
            if (e.GetPosition(scroller).Y > scroller.ActualHeight - scrollTollerance) {
                scroller.LineDown();
            } else if (e.GetPosition(scroller).Y < scrollTollerance) {
                scroller.LineUp();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            appGrabber.Save();
            this.Close();
        }
    }
}
