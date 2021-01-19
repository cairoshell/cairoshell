using System.Windows;
using System.Windows.Controls;
using CairoDesktop.Common;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.MenuBarExtensions
{
    /// <summary>
    /// Interaction logic for MenuExtraSearchResult.xaml
    /// </summary>
    public partial class SearchResult : UserControl
    {
        public SearchResult()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.CommandParameter is ManagedShell.Common.SupportingClasses.SearchResult searchObj)
                {
                    if (!ShellHelper.StartProcess(searchObj.Path))
                    {
                        CairoMessage.Show(Localization.DisplayString.sSearch_Error,
                            Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                    }
                }
            }
        }
    }
}
