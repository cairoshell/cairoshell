using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using CairoDesktop.Configuration;
using CairoDesktop.Common;

namespace CairoDesktop {
    /// <summary>
    /// Interaction logic for DesktopIcons.xaml
    /// </summary>
    public partial class DesktopIcons : UserControl 
    {
        public static DependencyProperty locationsProperty = DependencyProperty.Register("Locations", typeof(InvokingObservableCollection<SystemDirectory>), typeof(DesktopIcons), new PropertyMetadata(new InvokingObservableCollection<SystemDirectory>(Dispatcher.CurrentDispatcher)));

        public DesktopIcons() 
        {
            InitializeComponent();

            if (Settings.DesktopLabelPosition == 1)
            {
                IconsControl.Style = Application.Current.FindResource("DesktopFolderViewStyle") as Style;
            }

            string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string userDesktopPath = Settings.DesktopDirectory;

            if (userDesktopPath == "")
            {
                // first run won't have desktop directory set
                Settings.DesktopDirectory = defaultDesktopPath;
                userDesktopPath = defaultDesktopPath;
            }

            if (Directory.Exists(userDesktopPath))
            {
                setDesktopDir(userDesktopPath);
            }
            else if (Directory.Exists(defaultDesktopPath))
            {
                setDesktopDir(defaultDesktopPath);
            }
        }

        private void setDesktopDir(string desktopDir)
        {
            SystemDirectory desktop = new SystemDirectory(desktopDir, Dispatcher.CurrentDispatcher);
            Locations.Add(desktop);
        }

        public InvokingObservableCollection<SystemDirectory> Locations
        {
            get
            {
                return GetValue(locationsProperty) as InvokingObservableCollection<SystemDirectory>;
            }
            set
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke((Action)(() => this.Locations = value), null);
                    return;
                }

                SetValue(locationsProperty, value);
            }
        }
    }
}
