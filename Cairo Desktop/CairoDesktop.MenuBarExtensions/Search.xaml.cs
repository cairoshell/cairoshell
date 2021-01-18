using CairoDesktop.Common;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CairoDesktop.Application.Interfaces;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class Search : UserControl
    {
        private readonly ICairoApplication _cairoApplication;
        public bool _isPrimaryScreen;
        private static bool isSearchHotkeyRegistered;

        public Search(ICairoApplication cairoApplication, IMenuBar host)
        {
            InitializeComponent();

            _cairoApplication = cairoApplication;
            _isPrimaryScreen = host.GetIsPrimaryDisplay();

            SetupSearch();
        }

        private void SetupSearch()
        {
            // Show the search button only if the service is running
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                SetSearchProvider();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
                DispatcherTimer searchcheck = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
                {
                    Interval = new TimeSpan(0, 0, 5)
                };
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
                SetSearchProvider();
                CairoSearchMenu.Visibility = Visibility.Visible;
                (sender as DispatcherTimer).Stop();
            }
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void SetSearchProvider()
        {
            var thread = new Thread(() =>
            {
                // this sometimes takes a while
                Type provider = typeof(SearchHelper);

                _cairoApplication.Dispatch(() =>
                {
                    CairoSearchMenu.DataContext = new ObjectDataProvider
                    {
                        ObjectType = provider
                    };

                    Binding bSearchText = new Binding("SearchText")
                    {
                        Mode = BindingMode.Default,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };

                    Binding bSearchResults = new Binding("Results")
                    {
                        Mode = BindingMode.Default,
                        IsAsync = true
                    };

                    searchStr.SetBinding(TextBox.TextProperty, bSearchText);
                    lstSearchResults.SetBinding(ListView.ItemsSourceProperty, bSearchResults);
                });
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                if (_isPrimaryScreen && !isSearchHotkeyRegistered)
                {
                    new HotKey(Key.S, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowSearchHotkey);
                    isSearchHotkeyRegistered = true;
                }
            }
            catch (Exception ex)
            {
                ShellLogger.Warning($"Search: Unable to bind hotkey: {ex.Message}");
            }
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("search:query=" + searchStr.Text);
        }

        private void searchStr_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ShellHelper.StartProcess("search:query=" + searchStr.Text);
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
            searchStr.Clear();
            FocusSearchBox(sender, e);
        }

        public void FocusSearchBox(object sender, RoutedEventArgs e)
        {
            searchStr.Dispatcher.BeginInvoke(new Action(FocusSearchBox), DispatcherPriority.Render);
        }

        private void FocusSearchBox()
        {
            searchStr.Focusable = true;
            searchStr.Focus();
            Keyboard.Focus(searchStr);
        }

        internal void ToggleSearch()
        {
            CairoSearchMenu.IsSubmenuOpen = !CairoSearchMenu.IsSubmenuOpen;
        }
    }
}