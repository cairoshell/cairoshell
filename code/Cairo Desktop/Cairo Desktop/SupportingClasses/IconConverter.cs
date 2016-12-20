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
            if (value == null) return AppGrabber.WpfWin32ImageConverter.GetImageFromHIcon(IntPtr.Zero);

            ImageSource newIcon = null;
            newIcon = AppGrabber.WpfWin32ImageConverter.GetImageFromHIcon((value as Icon).Handle);

            return newIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
