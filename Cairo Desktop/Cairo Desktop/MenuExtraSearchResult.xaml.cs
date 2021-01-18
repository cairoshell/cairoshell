using System.Windows;
using System.Windows.Controls;
using CairoDesktop.Common;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.SupportingClasses;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuExtraSearchResult.xaml
    /// </summary>
    public partial class MenuExtraSearchResult : UserControl
    {
        public MenuExtraSearchResult()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.CommandParameter is SearchResult searchObj)
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
