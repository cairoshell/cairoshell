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
        private ObservableCollection<ApplicationInfo> selectedApps;

        public Visibility AppVisibility
        {
            get { return (Visibility)GetValue(AppVisibilityProperty); }
            set { SetValue(AppVisibilityProperty, value); }
        }

        private bool bAppsHidden
        {
            get { return (AppVisibility != Visibility.Visible); }
            set {
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
        
        public static readonly DependencyProperty AppVisibilityProperty = DependencyProperty.Register("AppVisibility", typeof(Visibility), typeof(AppGrabberUI_Page2), new PropertyMetadata(null));
        private static readonly DependencyProperty bAppsHiddenProperty = DependencyProperty.Register("bAppsHidden", typeof(bool), typeof(AppGrabberUI_Page2), new PropertyMetadata(null));

        /// <summary>
        /// Default constructor never gets called.
        /// </summary>
        private AppGrabberUI_Page2()
            : this(new AppGrabber(), new ObservableCollection<ApplicationInfo>())
        {
        }
        
        public AppGrabberUI_Page2(AppGrabber appGrabber, ObservableCollection<ApplicationInfo> selectedApps)
        {
            this.appGrabber = appGrabber;
            this.selectedApps = selectedApps;
            bAppsHidden = false;
            InitializeComponent();

            Height = SystemParameters.MaximizedPrimaryScreenHeight - 100;
        }

        private void SkipWizard(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
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

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            appGrabber.Save();
            this.Close();
        }

        private void tglHide_Click(object sender, RoutedEventArgs e)
        {
            if (!bAppsHidden)
                bAppsHidden = true;
            else
                bAppsHidden = false;
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            // Grab the Programs
            CategoryList catList = appGrabber.CategoryList;

            // Get the Uncategorized category - create it if it doesn't exist.
            Category uncat = catList.GetCategory("Uncategorized");
            if (uncat == null)
            {
                uncat = new Category("Uncategorized");
                uncat.ShowInMenu = false;
                catList.Add(uncat);
            }

            // Get the Quick Launch category - create it if it doesn't exist.
            Category quicklaunch = catList.GetCategory("Quick Launch");
            if (quicklaunch == null)
            {
                quicklaunch = new Category("Quick Launch");
                quicklaunch.ShowInMenu = false;
                catList.Add(quicklaunch);
            }

            // Add Apps to uncat if they haven't been added to a category yet.
            foreach (ApplicationInfo app in selectedApps)
            {
                if (app.Category == null)
                {
                    uncat.Add(app);
                }
            }

            categoryView.ItemsSource = catList;
            categoryView.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppGrabber.ui2Instance = null;
        }
    }
}
