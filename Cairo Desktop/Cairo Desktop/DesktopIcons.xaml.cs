using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using CairoDesktop.Interop;
using System.Windows.Media;

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

            setPosition();
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

        public RenderTargetBitmap GenerateBitmap(Grid sourceGrid)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(sourceGrid.ActualWidth * Shell.DpiScale), (int)(sourceGrid.ActualHeight * Shell.DpiScale), Shell.DpiScale * 96, Shell.DpiScale * 96, PixelFormats.Default);
            bmp.Render(this);
            bmp.Freeze();

            return bmp;
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

        private void IconsControl_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = VisualTreeHelper.GetChild(sender as ItemsControl, 0) as ScrollViewer;

            if (scrollViewer == null)
                return;

            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            e.Handled = true;
        }
    }
}
