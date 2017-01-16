using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Drawing;

namespace CairoDesktop
{
    [ValueConversion(typeof(System.Drawing.Icon), typeof(ImageSource))]
    public class IconConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageSource newIcon = null;
            if (value != null)
                newIcon = AppGrabber.IconImageConverter.GetImageFromHIcon((value as Icon).Handle);
            else
                newIcon = AppGrabber.IconImageConverter.GetDefaultIcon();
            newIcon.Freeze();

            return newIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
