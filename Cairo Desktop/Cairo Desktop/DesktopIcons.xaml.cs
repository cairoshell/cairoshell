using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using System.ComponentModel;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopIcons.xaml
    /// </summary>
    public partial class DesktopIcons : UserControl
    {
        public static DependencyProperty locationProperty =
            DependencyProperty.Register("Location",
                typeof(SystemDirectory),
                typeof(DesktopIcons),
                new PropertyMetadata(null));

        public DesktopIcons()
        {
            InitializeComponent();

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            panel.Margin = new Thickness(xOffset, yOffset, 0, 0);

            if (Settings.DesktopLabelPosition == 1 && Settings.DesktopIconSize == 0)
                IconsControl.Style = Application.Current.FindResource("DesktopFolderViewVerticalSmallStyle") as Style;
            else if (Settings.DesktopLabelPosition == 1 && Settings.DesktopIconSize == 2)
                IconsControl.Style = Application.Current.FindResource("DesktopFolderViewVerticalStyle") as Style;
            else if (Settings.DesktopIconSize == 0)
                IconsControl.Style = Application.Current.FindResource("DesktopFolderViewHorizontalSmallStyle") as Style;
        }

        private void SetDesktopDir(string desktopDir)
        {
            SystemDirectory desktop = new SystemDirectory(desktopDir, Dispatcher.CurrentDispatcher);
            Location = desktop;
        }
        
        public SystemDirectory Location
        {
            get
            {
                return GetValue(locationProperty) as SystemDirectory;
            }
            set
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => Location = value);
                    return;
                }

                SetValue(locationProperty, value);
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "DesktopLabelPosition":
                        setPosition();
                        break;
                }
            }
        }

        private void setPosition()
        {
            int xOffset = 7;
            int yOffset = 13;

            if (Settings.Instance.DesktopLabelPosition == 1)
                xOffset = 0;

            panel.Margin = new Thickness(xOffset, yOffset, 0, 0);
        }
    }
}
