using CairoDesktop.Configuration;
using System;
using System.Collections.Generic;
using System.Windows;

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

            double size = SupportingClasses.WindowManager.PrimaryMonitorSize.Height - 100;
            if (size >= maxSize)
            {
                Height = maxSize;
            }
            else
            {
                Height = size;
            }

            MaxHeight = SupportingClasses.WindowManager.PrimaryMonitorSize.Height;
            loadLanguages();
        }

        private void loadLanguages()
        {
            language = Settings.Instance.Language;
            cboLangSelect.DisplayMemberPath = "Key";
            cboLangSelect.SelectedValuePath = "Value";

            foreach (KeyValuePair<string, string> lang in Localization.DisplayString.Languages)
            {
                cboLangSelect.Items.Add(lang);
            }

            cboLangSelect.SelectedValue = Settings.Instance.Language;
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
            Settings.Instance.IsFirstRun = false;
            AppGrabber.AppGrabber.Instance.ShowDialog();
            Close();
        }

        private void cboLangSelect_DropDownClosed(object sender, EventArgs e)
        {
            if (Settings.Instance.Language != language)
            {
                Common.CairoMessage.Show(Localization.DisplayString.sWelcome_ChangingLanguageText, Localization.DisplayString.sWelcome_ChangingLanguage, MessageBoxButton.OK, MessageBoxImage.Information);
                Startup.Restart();
            }
        }
    }
}
