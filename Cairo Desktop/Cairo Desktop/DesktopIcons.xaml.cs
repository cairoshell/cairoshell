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
            InitializeComponent();

            if (Settings.Instance.DesktopLabelPosition == 1)
                xOffset = 0;

            panel.Margin = new Thickness(xOffset, yOffset, 0, 0);
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
