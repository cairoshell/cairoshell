using System;
using System.Collections.Generic;
using System.Windows;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Localization;
using CairoDesktop.SupportingClasses;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        const int maxSize = 780;
        string _initialLanguage = "en_US";

        public Welcome()
        {
            InitializeComponent();

            SetSizeAndLocation();

            LoadLanguages();
        }

        private void SetSizeAndLocation()
        {
            SetSize();
            SetLocation();
        }

        private void SetSize()
        {
            double size = WindowManager.PrimaryMonitorSize.Height - 100;
            if (size >= maxSize)
            {
                Height = maxSize;
            }
            else
            {
                Height = size;
            }

            MaxHeight = WindowManager.PrimaryMonitorSize.Height;
        }

        private void SetLocation()
        {
            Left = (SystemParameters.FullPrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.FullPrimaryScreenHeight - Height) / 2;

            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private void LoadLanguages()
        {
            _initialLanguage = Settings.Instance.Language;

            cboLangSelect.DisplayMemberPath = "Key";
            cboLangSelect.SelectedValuePath = "Value";

            foreach (KeyValuePair<string, string> lang in DisplayString.Languages)
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
            if (Settings.Instance.Language != _initialLanguage)
            {
                CairoMessage.Show(DisplayString.sWelcome_ChangingLanguageText, DisplayString.sWelcome_ChangingLanguage, MessageBoxButton.OK, CairoMessageImage.Information,
                    result =>
                    {
                        if (result == true)
                        {
                            Startup.RestartCairo();
                        }
                    });
            }
        }
    }
}
