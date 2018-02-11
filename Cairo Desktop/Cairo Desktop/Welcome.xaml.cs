using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        const int maxSize = 780;
        string language = "en_US";

        public Welcome()
        {
            InitializeComponent();

            double size = SupportingClasses.AppBarHelper.PrimaryMonitorSize.Height - 100;

            if (size >= maxSize)
                Height = maxSize;
            else
                Height = size;

            MaxHeight = SupportingClasses.AppBarHelper.PrimaryMonitorSize.Height;

            loadLanguages();
        }

        private void loadLanguages()
        {
            language = Settings.Language;
            cboLangSelect.DisplayMemberPath = "Key";
            cboLangSelect.SelectedValuePath = "Value";
            foreach (KeyValuePair<string, string> lang in Localization.DisplayString.Languages)
            {
                cboLangSelect.Items.Add(lang);
            }
        }

        private void btnGoPage2_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcome.Visibility = Visibility.Collapsed;
            bdrMenuBar.Visibility = Visibility.Visible;
        }

        private void btnGoPage3_Click(object sender, RoutedEventArgs e)
        {
            bdrMenuBar.Visibility = Visibility.Collapsed;
            bdrDesktop.Visibility = Visibility.Visible;
        }

        private void btnGoPage4_Click(object sender, RoutedEventArgs e)
        {
            bdrDesktop.Visibility = Visibility.Collapsed;
            bdrTaskbar.Visibility = Visibility.Visible;
        }

        private void btnAppGrabber_Click(object sender, RoutedEventArgs e)
        {
            Settings.IsFirstRun = false;
            AppGrabber.AppGrabber.Instance.ShowDialog();
            Close();
        }

        private void cboLangSelect_DropDownClosed(object sender, EventArgs e)
        {
            if (Settings.Language != language)
            {
                Common.CairoMessage.Show(Localization.DisplayString.sWelcome_ChangingLanguageText, Localization.DisplayString.sWelcome_ChangingLanguage, MessageBoxButton.OK, MessageBoxImage.Information);
                Startup.Restart();
            }
        }
    }
}
