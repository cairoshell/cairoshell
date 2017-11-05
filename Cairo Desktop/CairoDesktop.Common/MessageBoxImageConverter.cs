
namespace CairoDesktop.Common
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Converts the specified MessageBoxImage to an ImageSource to be bound to.
    /// </summary>
    [ValueConversion(typeof(MessageBoxImage), typeof(ImageSource))]
    public class MessageBoxImageConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified MessageBoxImage value to an ImageSource.
        /// </summary>
        /// <param name="value">The MessageBoxImage specified.</param>
        /// <param name="targetType">The target type to convert to.</param>
        /// <param name="parameter">The paramater for converter.</param>
        /// <param name="culture">The culture to convert for.</param>
        /// <returns>An image source representing the MessageBoxImage.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MessageBoxImage imageType = (MessageBoxImage)value;
            BitmapImage retImage = this.GetDefaultImage();

            switch (imageType)
            {
                case MessageBoxImage.Information:
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
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Resources/cairoIcon.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();

            return bi;
        }
    }
}
