using CairoDesktop.Common;
using CairoDesktop.Interop;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : UserControl
    {
        public bool _isPrimaryScreen;
        private static bool isSearchHotkeyRegistered;

        public Search(MenuBar menuBar)
        {
            InitializeComponent();

            _isPrimaryScreen = menuBar.Screen.Primary;

            setupSearch();
        }

        private void setupSearch()
        {
            CommandBindings.Add(new CommandBinding(CustomCommands.OpenSearchResult, ExecuteOpenSearchResult));

            // Show the search button only if the service is running
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                setSearchProvider();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
                DispatcherTimer searchcheck = new DispatcherTimer(DispatcherPriority.Background, Dispatcher);
                searchcheck.Interval = new TimeSpan(0, 0, 5);
                searchcheck.Tick += searchcheck_Tick;
                searchcheck.Start();
            }
        }

        private void OnShowSearchHotkey(HotKey hotKey)
        {
            ToggleSearch();
        }

        private void searchcheck_Tick(object sender, EventArgs e)
        {
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                setSearchProvider();
                CairoSearchMenu.Visibility = Visibility.Visible;
                (sender as DispatcherTimer).Stop();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void setSearchProvider()
        {
            var thread = new Thread(() =>
            {
                // this sometimes takes a while
                Type provider = typeof(SearchHelper);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ObjectDataProvider vistaSearchProvider = new ObjectDataProvider();
                    vistaSearchProvider.ObjectType = provider;
                    CairoSearchMenu.DataContext = vistaSearchProvider;

                    Binding bSearchText = new Binding("SearchText");
                    bSearchText.Mode = BindingMode.Default;
                    bSearchText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    Binding bSearchResults = new Binding("Results");
                    bSearchResults.Mode = BindingMode.Default;
                    bSearchResults.IsAsync = true;

                    searchStr.SetBinding(TextBox.TextProperty, bSearchText);
                    lstSearchResults.SetBinding(ListView.ItemsSourceProperty, bSearchResults);
                }));
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                if (_isPrimaryScreen && !isSearchHotkeyRegistered)
                {
                    new HotKey(Key.S, KeyModifier.Win | KeyModifier.NoRepeat, OnShowSearchHotkey);
                    isSearchHotkeyRegistered = true;
                }
            }
            catch { }
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("search:query=" + searchStr.Text);
        }

        private void searchStr_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Shell.StartProcess("search:query=" + searchStr.Text);
            }
        }

        private void searchStr_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CairoSearchMenu.IsKeyboardFocusWithin)
            {
                e.Handled = true;
            }
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            searchStr.Text = "";
            FocusSearchBox(sender, e);
        }

        public void FocusSearchBox(object sender, RoutedEventArgs e)
        {
            searchStr.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                searchStr.Focusable = true;
                searchStr.Focus();
                Keyboard.Focus(searchStr);
            }),
            DispatcherPriority.Render);
        }

        public void ExecuteOpenSearchResult(object sender, ExecutedRoutedEventArgs e)
        {
            var searchObj = (SearchResult)e.Parameter;

            if (!Shell.StartProcess(searchObj.Path))
            {
                CairoMessage.Show(Localization.DisplayString.sSearch_Error, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void ToggleSearch()
        {
            CairoSearchMenu.IsSubmenuOpen = !CairoSearchMenu.IsSubmenuOpen;
        }
    }
}