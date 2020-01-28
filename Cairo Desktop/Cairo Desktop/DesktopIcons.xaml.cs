using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using CairoDesktop.Configuration;
using CairoDesktop.Common;

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

        int xOffset = 7;
        int yOffset = 13;

        public DesktopIcons()
        {
            string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string userDesktopPath = Settings.DesktopDirectory;

            // first run won't have desktop directory set
            if (string.IsNullOrWhiteSpace(userDesktopPath))
            {
                Settings.DesktopDirectory = defaultDesktopPath;
                userDesktopPath = defaultDesktopPath;
            }

            if (Directory.Exists(userDesktopPath))
                SetDesktopDir(userDesktopPath);
            else if (Directory.Exists(defaultDesktopPath))
                SetDesktopDir(defaultDesktopPath);

            InitializeComponent();

            if (Settings.DesktopLabelPosition == 1)
                xOffset = 0;

            panel.Margin = new Thickness(xOffset, yOffset, 0, 0);
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
    }
}
