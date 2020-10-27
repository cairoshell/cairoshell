using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Converts the specified CairoMessageImage to an ImageSource to be bound to.
    /// </summary>
    [ValueConversion(typeof(CairoMessageImage), typeof(ImageSource))]
    public class CairoMessageImageConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified CairoMessageImage value to an ImageSource.
        /// </summary>
        /// <param name="value">The CairoMessageImage specified.</param>
        /// <param name="targetType">The target type to convert to.</param>
        /// <param name="parameter">The paramater for converter.</param>
        /// <param name="culture">The culture to convert for.</param>
        /// <returns>An image source representing the CairoMessageImage.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CairoMessageImage imageType = (CairoMessageImage)value;
            BitmapImage retImage;

            switch (imageType)
            {
                case CairoMessageImage.Error:
                    retImage = GetImage("Resources/dialogIconError.png");
                    break;
                case CairoMessageImage.Information:
                    retImage = GetImage("Resources/dialogIconInformation.png");
                    break;
                case CairoMessageImage.Warning:
                    retImage = GetImage("Resources/dialogIconWarning.png");
                    break;
                case CairoMessageImage.LogOff:
                    retImage = GetImage("Resources/dialogIconLogOff.png");
                    break;
                case CairoMessageImage.Restart:
                    retImage = GetImage("Resources/dialogIconRestart.png");
                    break;
                case CairoMessageImage.ShutDown:
                    retImage = GetImage("Resources/dialogIconShutDown.png");
                    break;
                default:
                    retImage = GetDefaultImage();
                    break;
            }

            return retImage;
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>None, Not Implemented.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads and returns the default bitmap image.
        /// </summary>
        /// <returns>The default cairo icon as a bitmap image.</returns>
        private BitmapImage GetDefaultImage()
        {
            return GetImage("Resources/dialogIconCairo.png");
        }
        private BitmapImage GetImage(string uri)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bi.EndInit();

            return bi;
        }
    }
}
