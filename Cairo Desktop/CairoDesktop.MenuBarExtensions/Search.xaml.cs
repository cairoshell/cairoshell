using CairoDesktop.Application.Interfaces;
using ManagedShell.Common.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class Search : UserControl
    {
        internal IMenuBar Host;

        public Search(IMenuBar host, ObjectDataProvider provider)
        {
            InitializeComponent();

            Host = host;

            SetSearchProvider(provider);
        }

        internal void SetSearchProvider(ObjectDataProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            CairoSearchMenu.DataContext = provider;

            Binding bSearchText = new Binding("SearchText")
            {
                Mode = BindingMode.Default,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            Binding bSearchResults = new Binding("Results")
            {
                Mode = BindingMode.OneWay,
                IsAsync = true
            };

            searchStr.SetBinding(TextBox.TextProperty, bSearchText);
            lstSearchResults.SetBinding(ListView.ItemsSourceProperty, bSearchResults);

            CairoSearchMenu.Visibility = Visibility.Visible;
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
            if (CairoSearchMenu.Visibility != Visibility.Visible)
            {
                return;
            }

            CairoSearchMenu.IsSubmenuOpen = !CairoSearchMenu.IsSubmenuOpen;
        }
    }
}